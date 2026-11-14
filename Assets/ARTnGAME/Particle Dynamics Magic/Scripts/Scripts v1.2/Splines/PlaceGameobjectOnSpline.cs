using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Serialization;
using Artngame.PDM;
namespace Artngame.PDM
{
	[ExecuteInEditMode()]
	public class PlaceGameobjectOnSpline : MonoBehaviour
	{

		public GameObject SplinerP_OBJ;
		SplinerP Spline_to_Conform;

		void Start()
		{

			if (Preview_mode & Application.isPlaying)
			{


				if (Parent_OBJ != null)
				{
					int childs = Parent_OBJ.transform.childCount;

					for (int i = childs - 1; i > 0; i--)

					{

						GameObject.Destroy(Parent_OBJ.transform.GetChild(i).gameObject);

					}
				}





			}

			if (Gameobj_instances == null)
			{
				Gameobj_instances = new List<GameObject>();
			}

			if (SplinerP_OBJ != null)
			{
				Spline_to_Conform = SplinerP_OBJ.GetComponent("SplinerP") as SplinerP;
				keep_curve_quality = Spline_to_Conform.CurveQuality;
			}

			if (p2 == null)
			{
				p2 = this.gameObject.GetComponent<ParticleSystem>();
			}

			if (p2 != null & SplinerP_OBJ != null)
			{
				aaa = new ParticleSystem.Particle[Spline_to_Conform.Curve.Count];
			}

			Particle_Num = particle_count;

			Registered_paint_positions = new List<Vector3>();
			Registered_paint_rotations = new List<Vector3>();

			noise = new PerlinPDM();

			if (Application.isPlaying)
			{

				Preview_mode = false;

				if (Gameobj_instances != null)
				{
					for (int i = Gameobj_instances.Count - 1; i >= 0; i--)
					{

						DestroyImmediate(Gameobj_instances[i]);
					}
				}

			}

			keep_particle_count = particle_count;

			if (Parent_OBJ == null | Gameobj == null | p2 == null | SplinerP_OBJ == null)
			{

				Debug.Log("Please add an emitter, a gameobject to be used as particle, a container object for pooling and a SplineP object");

			}

		}

		[Range(1, 10)]
		public int Place_every_N_step = 2;

		private int keep_curve_quality;
		private int keep_particle_count;

		public bool Preview_mode = false;

		public int particle_count = 100;

		bool got_positions = false;


		Vector3[] positions;

		int[] tile;

		private ParticleSystem.Particle[] aaa;
		public ParticleSystem p2;



		private List<Vector3> Registered_paint_positions;
		private List<Vector3> Registered_paint_rotations;

		private List<GameObject> Gameobj_instances;

		public GameObject Gameobj;

		public float Y_offset = 0f;

		[HideInInspector]
		public bool fix_initial = false;
		private bool let_loose = false;
		public bool letloose = false;
		private int place_start_pos;

		public bool Gravity_Mode = false;

		public GameObject Parent_OBJ;

		public bool Angled = false;

		private int Particle_Num;

		public bool Asign_rot = false;
		public Vector3 Local_rot = Vector3.zero;
		private PerlinPDM noise;
		public float Wind_speed = 1f;

		public float Return_speed = 0.005f;

		public bool follow_particles = false;
		public bool Remove_colliders = false;

		void Update()
		{

			if (Parent_OBJ == null | Gameobj == null | p2 == null | SplinerP_OBJ == null)
			{
				//Debug.Log ("Please add an emitter, a gameobject to be used as particle, a container object for pooling and a SplineP object");
				return;
			}



			if (keep_particle_count != particle_count)
			{

				keep_particle_count = particle_count;
				got_positions = false;
			}


			if (Gameobj_instances == null)
			{
				Gameobj_instances = new List<GameObject>();
			}

			if (p2 == null)
			{
				p2 = this.gameObject.GetComponent<ParticleSystem>();
			}

			if (Spline_to_Conform == null)
			{
				if (SplinerP_OBJ != null)
				{
					Spline_to_Conform = SplinerP_OBJ.GetComponent("SplinerP") as SplinerP;
				}
			}

			if (keep_curve_quality != Spline_to_Conform.CurveQuality)
			{

				keep_curve_quality = Spline_to_Conform.CurveQuality;
				got_positions = false;

			}

			if (aaa == null)
			{

				aaa = new ParticleSystem.Particle[Spline_to_Conform.Curve.Count];

			}

			if (particle_count > Spline_to_Conform.Curve.Count * Place_every_N_step)
			{
				particle_count = Spline_to_Conform.Curve.Count * Place_every_N_step;

			}


			if (!Preview_mode & !Application.isPlaying)
			{

				for (int i = Gameobj_instances.Count - 1; i >= 0; i--)
				{

					DestroyImmediate(Gameobj_instances[i]);
				}

			}

			for (int i = Gameobj_instances.Count - 1; i >= 0; i--)
			{


				if (Gameobj_instances[i] == null)
				{

					Gameobj_instances.RemoveAt(i);
				}

			}

			//reset, if particle number changes
			if (Particle_Num != particle_count)
			{
				got_positions = false;
				Particle_Num = particle_count;
			}

			let_loose = letloose;
			if (!Application.isPlaying)
			{

				aaa = new ParticleSystem.Particle[Spline_to_Conform.Curve.Count];

				let_loose = false;

				if (noise == null)
				{
					noise = new PerlinPDM();
				}

			}

			if (p2.particleCount < particle_count)
			{

			}

			if (!let_loose)
			{
				p2.Clear();
			}

			if (p2.main.maxParticles != Spline_to_Conform.Curve.Count)
			{
				ParticleSystem.MainModule main = p2.main;//v2.3
				main.maxParticles = Spline_to_Conform.Curve.Count;
			}


			p2.Emit(Spline_to_Conform.Curve.Count);
			if (!let_loose)
			{
				aaa = new ParticleSystem.Particle[Spline_to_Conform.Curve.Count];
				got_positions = false;
			}

			int tileCount = 15;

			p2.GetParticles(aaa);

			if (!got_positions)
			{

				positions = new Vector3[Spline_to_Conform.Curve.Count];
				tile = new int[Spline_to_Conform.Curve.Count];

				got_positions = true;

				for (int i = 0; i < aaa.Length; i++)
				{
					positions[i] = aaa[i].position;
					tile[i] = Random.Range(0, 15);
				}
			}

			// PROJECTION

			if (!fix_initial)
			{
				Registered_paint_positions.Clear();
				Registered_paint_rotations.Clear();
			}

			if (Registered_paint_positions != null)
			{

				int counter = 0;
				for (int i = 0; i < Spline_to_Conform.Curve.Count; i++)
				{
					if (Registered_paint_positions.Count < Spline_to_Conform.Curve.Count)
					{
						Registered_paint_positions.Add(Spline_to_Conform.Curve[counter].position);
						Registered_paint_rotations.Add(Vector3.zero);
					}

					counter = counter + 1;
					if (counter > Spline_to_Conform.Curve.Count - 1)
					{

						counter = 0;

					}
				}

			}

			if (Preview_mode | Application.isPlaying)
			{

				if (Registered_paint_positions != null)
				{

					if (!follow_particles)
					{

						for (int i = 0; i < Registered_paint_positions.Count; i++)
						{

							if (Gameobj_instances.Count < (particle_count / Place_every_N_step))
							{
								GameObject TEMP = Instantiate(Gameobj, Registered_paint_positions[i], Quaternion.identity) as GameObject;

								Gameobj_instances.Add(TEMP);
								TEMP.transform.position = Registered_paint_positions[i];

								if (Angled)
								{

									TEMP.transform.localEulerAngles = Registered_paint_rotations[i];
								}

								TEMP.transform.parent = Parent_OBJ.transform;
							}
						}
					}

					if (follow_particles)
					{

						for (int i = 0; i < aaa.Length - 1; i = i + Place_every_N_step)
						{


							if (Gameobj_instances.Count < (particle_count / Place_every_N_step) & Registered_paint_positions.Count > (i))
							{
								GameObject TEMP = Instantiate(Gameobj, Registered_paint_positions[i], Quaternion.identity) as GameObject;

								Gameobj_instances.Add(TEMP);
								TEMP.transform.position = Registered_paint_positions[i];

								if (Angled)
								{

									TEMP.transform.localEulerAngles = Registered_paint_rotations[i];

								}

								TEMP.transform.parent = Parent_OBJ.transform;
							}
						}

					}

				}

				for (int i = 0; i < Gameobj_instances.Count; i++)
				{
					if (i < Registered_paint_positions.Count)
					{
						Gameobj_instances[i].transform.position = Registered_paint_positions[i];

						if (Angled)
						{

							Gameobj_instances[i].transform.rotation = Quaternion.identity;
							Quaternion rot = Quaternion.FromToRotation(Gameobj_instances[i].transform.up, Registered_paint_rotations[i]);

							Quaternion rot1 = Quaternion.FromToRotation(Gameobj_instances[i].transform.up, Gameobj_instances[i].transform.right);

							if (Asign_rot)
							{

								if (Wind_speed > 0 & Application.isPlaying)
								{

									float timex = Time.time * Wind_speed + 0.1365143f * 10 * i;

									Local_rot.y = noise.Noise(timex + 10, timex + 20, timex);

								}

								Gameobj_instances[i].transform.localRotation *= rot1 * new Quaternion(Local_rot.x, Local_rot.y, Local_rot.z, 1);

							}

							Gameobj_instances[i].transform.rotation *= rot;

						}
						else
						{

							Gameobj_instances[i].transform.rotation = Quaternion.identity;
						}

					}
				}
			}

			int count_positions = 0;

			if (1 == 1)
			{
				for (int i = 0; i < aaa.Length; i++)
				{

					aaa[i].remainingLifetime = tileCount + 1 - tile[i];
					aaa[i].startLifetime = tileCount;

					if (!let_loose)
					{
						aaa[i].angularVelocity = 0;
						aaa[i].rotation = 0;
						aaa[i].velocity = Vector3.zero;
					}

					aaa[i].startColor = Color.white;

					if (!let_loose | (place_start_pos < 1))
					{

						if (Registered_paint_positions.Count > count_positions)
						{
							aaa[i].position = Registered_paint_positions[count_positions] + new Vector3(i * 0.005f * Y_offset, Y_offset, i * 0.007f * Y_offset);
						}
					}

					//Gravity
					if (let_loose & Gravity_Mode)
					{

						if (Registered_paint_positions.Count > count_positions)
						{
							aaa[i].position = Vector3.Slerp(aaa[i].position, Registered_paint_positions[count_positions] + new Vector3(i * 0.005f * Y_offset, Y_offset, i * 0.007f * Y_offset), Return_speed);
						}
						aaa[i].velocity = Vector3.Slerp(aaa[i].velocity, Vector3.zero, 0.05f);
					}

					if (follow_particles)
					{
						if (Gameobj_instances.Count - 1 > i)
						{

							if (!float.IsNaN(aaa[i].position.x))
							{
								Gameobj_instances[i].transform.position = aaa[i].position;
							}

							//remove colliders
							if (Gameobj_instances[i].GetComponent<Collider>() != null)
							{
								if (Remove_colliders)
								{

									Gameobj_instances[i].GetComponent<Collider>().enabled = false;

								}
								else if (!Remove_colliders)
								{

									Gameobj_instances[i].GetComponent<Collider>().enabled = true;

								}
							}


						}
					}

					if (count_positions > Registered_paint_positions.Count - 2)
					{
						count_positions = 0;
					}
					else { count_positions = count_positions + 1; }

				}

				if (place_start_pos < 1)
				{
					place_start_pos = place_start_pos + 1;
				}

				p2.SetParticles(aaa, aaa.Length);
			}
		}
	}
}