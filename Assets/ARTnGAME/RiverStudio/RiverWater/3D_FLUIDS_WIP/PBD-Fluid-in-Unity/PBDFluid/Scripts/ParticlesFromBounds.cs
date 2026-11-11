using System;
using System.Collections.Generic;
using UnityEngine;

namespace Artngame.RiverStudio.PBDFluid
{

    public class ParticlesFromBounds : ParticleSource
    {

        public Bounds Bounds { get; private set; }

        public List<Bounds> Exclusion { get; private set; }

        public ParticlesFromBounds(float spacing, Bounds bounds) : base(spacing)
        {
            Bounds = bounds;
            Exclusion = new List<Bounds>();
            CreateParticles();
        }

        public ParticlesFromBounds(float spacing, Bounds bounds, Bounds exclusion) : base(spacing)
        {
            Bounds = bounds;
            Exclusion = new List<Bounds>();
            Exclusion.Add(exclusion);
            CreateParticles();
        }

        //v0.1
        public bool IsInsideCollider(Collider c, Vector3 point)
        {
            Vector3 closest = c.ClosestPoint(point);
            // Because closest=point if point is inside - not clear from docs I feel
            return closest == point;
        }

        private void CreateParticles()
        {

            int numX = (int)((Bounds.size.x + HalfSpacing) / Spacing);
            int numY = (int)((Bounds.size.y + HalfSpacing) / Spacing);
            int numZ = (int)((Bounds.size.z + HalfSpacing) / Spacing);

            Positions = new List<Vector3>();

            //v0.1
            Collider[] colliders = GameObject.FindObjectsOfType<Collider>() as Collider[];

            for (int z = 0; z < numZ; z++)
            {
                for (int y = 0; y < numY; y++)
                {
                    for (int x = 0; x < numX; x++)
                    {
                        Vector3 pos = new Vector3();
                        pos.x = Spacing * x + Bounds.min.x + HalfSpacing;
                        pos.y = Spacing * y + Bounds.min.y + HalfSpacing;
                        pos.z = Spacing * z + Bounds.min.z + HalfSpacing;

                        //v0.1
                        bool isPosInsideCollider = false;                        
                        for (int i = 0; i < colliders.Length; i++)
                        {
                            if (colliders[i].gameObject.name == "FluidCollider")
                            {
                               isPosInsideCollider = IsInsideCollider(colliders[i], pos);
                                if (isPosInsideCollider) { break; }
                            }
                           
                        }

                        bool exclude = false;
                        for (int i = 0; i < Exclusion.Count; i++)
                        {
                            if (Exclusion[i].Contains(pos) && !isPosInsideCollider) //&& (pos.x < 2 || pos.y > 2 || pos.z < 0.5)) //v0.1
                            {
                                exclude = true;
                                break;
                            }
                        }

                        if (!exclude) { 
                            Positions.Add(pos);
                        }


                        ////ADD OBSTCALES
                        //pos.x = Spacing * x + Bounds.min.x/2 + 1 + HalfSpacing / 2;
                        //pos.y = Spacing * y + Bounds.min.y/2 + HalfSpacing / 2;
                        //pos.z = Spacing * z + Bounds.min.z/2  + HalfSpacing / 2;    

                        //exclude = false;
                        //for (int i = 0; i < Exclusion.Count; i++)
                        //{
                        //    if (Exclusion[i].Contains(pos))
                        //    {
                        //        exclude = true;
                        //        break;
                        //    }
                        //}

                        //if (!exclude)
                        //{
                        //    Positions.Add(pos);
                        //}
                        ////END


                    }
                }
            }

        }

    }

}