using System;
using System.Collections.Generic;
using UnityEngine;

namespace Artngame.RiverStudio.PBDFluid
{

    public class FluidSolver : IDisposable
    {
        //v0.2
        public Vector4 forcePowerX = new Vector4(1, 1, 1, 1);
        public Vector4 forcePowerY = new Vector4(1, 1, 1, 1);
        public Vector4 forcePowerZ = new Vector4(1, 1, 1, 1);
        public Vector4 InteractorBoundsXZ = new Vector4(0,0,0,0);

        //v0.1
        public float forcePower = 1;

        private const int THREADS = 128;
        private const int READ = 0;
        private const int WRITE = 1;

        public int Groups { get; private set; }

        public FluidBoundary Boundary { get; private set; }

        public FluidBody Body { get; private set; }

        public GridHash Hash { get; private set; }

        public int SolverIterations { get; set; }

        public int ConstraintIterations { get; set; }

        public SmoothingKernel Kernel { get; private set; }

        private ComputeShader m_shader;

        public FluidSolver(FluidBody body, FluidBoundary boundary)
        {
            SolverIterations = 2;
            ConstraintIterations = 2;

            Body = body;
            Boundary = boundary;

            float cellSize = Body.ParticleRadius * 4.0f;
            int total = Body.NumParticles + Boundary.NumParticles;
            Hash = new GridHash(Boundary.Bounds, total, cellSize);
            Kernel = new SmoothingKernel(cellSize);

            int numParticles = Body.NumParticles;
            Groups = numParticles / THREADS;
            if (numParticles % THREADS != 0) Groups++;

            m_shader = Resources.Load("FluidSolver") as ComputeShader;

        }

        public void Dispose()
        {
            Hash.Dispose();
        }

        //v0.2
        public float gravity = -9.81f;

        public void StepPhysics(float dt)
        {

            if (dt <= 0.0) return;
            if (SolverIterations <= 0 || ConstraintIterations <= 0) return;

            dt /= SolverIterations;

            m_shader.SetInt("NumParticles", Body.NumParticles);
            m_shader.SetVector("Gravity", new Vector3(0.0f, gravity, 0.0f));
            m_shader.SetFloat("Dampning", Body.Dampning);
            m_shader.SetFloat("DeltaTime", dt);
            m_shader.SetFloat("Density", Body.Density);
            m_shader.SetFloat("Viscosity", Body.Viscosity);
            m_shader.SetFloat("ParticleMass", Body.ParticleMass);

            m_shader.SetFloat("KernelRadius", Kernel.Radius);
            m_shader.SetFloat("KernelRadius2", Kernel.Radius2);
            m_shader.SetFloat("Poly6Zero", Kernel.Poly6(Vector3.zero));
            m_shader.SetFloat("Poly6", Kernel.POLY6);
            m_shader.SetFloat("SpikyGrad", Kernel.SPIKY_GRAD);
            m_shader.SetFloat("ViscLap", Kernel.VISC_LAP);

            m_shader.SetFloat("HashScale", Hash.InvCellSize);
            m_shader.SetVector("HashSize", Hash.Bounds.size);
            m_shader.SetVector("HashTranslate", Hash.Bounds.min);

            //Predicted and velocities use a double buffer as solver step
            //needs to read from many locations of buffer and write the result
            //in same pass. Could be removed if needed as long as buffer writes 
            //are atomic. Not sure if they are.

            for (int i = 0; i < SolverIterations; i++)
            {
                PredictPositions(dt);

                Hash.Process(Body.Predicted[READ], Boundary.Positions);

                ConstrainPositions();

                UpdateVelocities(dt);

                SolveViscosity();

                UpdatePositions();
            }
  
        }

        private void PredictPositions(float dt)
        {
            int kernel = m_shader.FindKernel("PredictPositions");

            m_shader.SetBuffer(kernel, "Positions", Body.Positions);
            m_shader.SetBuffer(kernel, "PredictedWRITE", Body.Predicted[WRITE]);
            m_shader.SetBuffer(kernel, "VelocitiesREAD", Body.Velocities[READ]);
            m_shader.SetBuffer(kernel, "VelocitiesWRITE", Body.Velocities[WRITE]);

            m_shader.Dispatch(kernel, Groups, 1, 1);

            Swap(Body.Predicted);
            Swap(Body.Velocities);
        }

        public void ConstrainPositions()
        {

            int computeKernel = m_shader.FindKernel("ComputeDensity");
            int solveKernel = m_shader.FindKernel("SolveConstraint");

            m_shader.SetBuffer(computeKernel, "Densities", Body.Densities);
            m_shader.SetBuffer(computeKernel, "Pressures", Body.Pressures);
            m_shader.SetBuffer(computeKernel, "Boundary", Boundary.Positions);
            m_shader.SetBuffer(computeKernel, "IndexMap", Hash.IndexMap);
            m_shader.SetBuffer(computeKernel, "Table", Hash.Table);

            m_shader.SetBuffer(solveKernel, "Pressures", Body.Pressures);
            m_shader.SetBuffer(solveKernel, "Boundary", Boundary.Positions);
            m_shader.SetBuffer(solveKernel, "IndexMap", Hash.IndexMap);
            m_shader.SetBuffer(solveKernel, "Table", Hash.Table);

            for (int i = 0; i < ConstraintIterations; i++)
            {
                m_shader.SetBuffer(computeKernel, "PredictedREAD", Body.Predicted[READ]);
                m_shader.Dispatch(computeKernel, Groups, 1, 1);

                m_shader.SetBuffer(solveKernel, "PredictedREAD", Body.Predicted[READ]);
                m_shader.SetBuffer(solveKernel, "PredictedWRITE", Body.Predicted[WRITE]);
                m_shader.Dispatch(solveKernel, Groups, 1, 1);

                Swap(Body.Predicted);
            }
        }

        private void UpdateVelocities(float dt)
        {
            int kernel = m_shader.FindKernel("UpdateVelocities");

            m_shader.SetBuffer(kernel, "Positions", Body.Positions);
            m_shader.SetBuffer(kernel, "PredictedREAD", Body.Predicted[READ]);
            m_shader.SetBuffer(kernel, "VelocitiesWRITE", Body.Velocities[WRITE]);

            m_shader.Dispatch(kernel, Groups, 1, 1);

            Swap(Body.Velocities);
        }

        private void SolveViscosity()
        {
            int kernel = m_shader.FindKernel("SolveViscosity");

            m_shader.SetBuffer(kernel, "Densities", Body.Densities);
            m_shader.SetBuffer(kernel, "Boundary", Boundary.Positions);
            m_shader.SetBuffer(kernel, "IndexMap", Hash.IndexMap);
            m_shader.SetBuffer(kernel, "Table", Hash.Table);

            //v0.1
            Vector4[] velocities = new Vector4[Body.Velocities[READ].count];
            Vector4[] positions = new Vector4[Body.Predicted[READ].count];
            Body.Velocities[READ].GetData(velocities);
            Body.Predicted[READ].GetData(positions);
            for (int i=0;i< Body.Velocities[READ].count; i++)
            {
                //Debug.Log(positions[i].x);
                if (positions[i].x > -1 && positions[i].x < 1 && positions[i].y < 1)//if (positions[i].x > -1 && positions[i].y < 1)
                {
                    //velocities[i] = velocities[i] * 1.01f;
                    //velocities[i].y = velocities[i].y + 4.5f * forcePower;// -velocities[i].y;

                    //v0.2
                    velocities[i].y += 4.5f * forcePower * forcePowerY.x + Mathf.Cos(1.5f * Time.fixedTime) * 0.1f * forcePower * forcePowerY.y;
                }
                //velocities[i].x = velocities[i].x + Mathf.Cos(1.5f * Time.fixedTime) * 0.1f * forcePower;
                //velocities[i].z = velocities[i].z + Mathf.Cos(9.5f * Time.fixedTime + 1) * 0.05f * forcePower;
                velocities[i].x = velocities[i].x + Mathf.Cos(1.5f * Time.fixedTime) * 0.1f * forcePower * forcePowerX.y + 0.05f * forcePower * forcePowerX.x;
                velocities[i].z = velocities[i].z + Mathf.Cos(9.5f * Time.fixedTime + 1) * 0.05f * forcePower * forcePowerZ.y + 0.05f * forcePower * forcePowerZ.x;


                //v0.2 - Interactor 
                if (forcePowerX.z != 0 || forcePowerZ.z != 0)
                {
                    if(positions[i].x > InteractorBoundsXZ.x && positions[i].x < InteractorBoundsXZ.y
                        && positions[i].z > InteractorBoundsXZ.z && positions[i].z < InteractorBoundsXZ.w)
                    {
                        velocities[i].x = velocities[i].x + Mathf.Cos(1.5f * Time.fixedTime) * 0.1f * forcePower * forcePowerX.z + 0.05f * forcePower * forcePowerX.z;
                        velocities[i].z = velocities[i].z + Mathf.Cos(9.5f * Time.fixedTime + 1) * 0.05f * forcePower * forcePowerZ.z + 0.05f * forcePower * forcePowerZ.z;
                    }
                }
          

            }
            Body.Velocities[READ].SetData(velocities);

            m_shader.SetBuffer(kernel, "PredictedREAD", Body.Predicted[READ]);
            m_shader.SetBuffer(kernel, "VelocitiesREAD", Body.Velocities[READ]);
            m_shader.SetBuffer(kernel, "VelocitiesWRITE", Body.Velocities[WRITE]);

            m_shader.Dispatch(kernel, Groups, 1, 1);

            Swap(Body.Velocities);
        }

        private void UpdatePositions()
        {
            int kernel = m_shader.FindKernel("UpdatePositions");

            m_shader.SetBuffer(kernel, "Positions", Body.Positions);
            m_shader.SetBuffer(kernel, "PredictedREAD", Body.Predicted[READ]);

            m_shader.Dispatch(kernel, Groups, 1, 1);
        }

        private void Swap(ComputeBuffer[] buffers)
        {
            ComputeBuffer tmp = buffers[0];
            buffers[0] = buffers[1];
            buffers[1] = tmp;
        }
    }

}