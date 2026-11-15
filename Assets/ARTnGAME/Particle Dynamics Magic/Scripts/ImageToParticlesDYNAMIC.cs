using UnityEngine;
using System.Collections;
using Artngame.PDM;

namespace Artngame.PDM {

[ExecuteInEditMode()]
public class ImageToParticlesDYNAMIC : MonoBehaviour
{
	public float SCALE_FACTOR = 0.1f;
	public float PARTICLE_SIZE = 1f;
	public Texture2D image_texture;

	public Texture2D depth_image_texture;

	private ParticleSystem.Particle[] IMAGE_POINT_CLOUD;
	private ParticleSystem.Particle[] IMAGE_POINT_CLOUD_UPDATE;
	public bool GIVE_DEPTH = true;

	//	public ParticleSystem nbb;

	public float DEPTH_FACTOR = 1f;
		public bool extend_life=false;
	
	void Start ()
	{
		DEFINE_POINTS();
	}

	public bool Changing_texture = false;

	public bool use_Depth_map=false;
	public bool expose_highlights=false;
	public float expose_factor=0.1f;
	Color32[] Pixels2;

		//v1.8
		public bool gravity=false;
		public float gravity_factor = 0.1f;


	void LateUpdate()
	{

			if(GetComponent<ParticleSystem>() == null | image_texture == null){return;}


		if(!Application.isPlaying){
			Start ();

		}
			//v1.8
			if(gravity){

				GetComponent<ParticleSystem>().GetParticles(IMAGE_POINT_CLOUD_UPDATE);

				//float size_x = SCALE_FACTOR / (image_texture.width - 1);
				//float size_z = SCALE_FACTOR / (image_texture.height - 1);
				
				int counter = 0;
				for (int x = 0; x < image_texture.width; x++)
				{
					for (int z = 0; z < image_texture.height; z++)
					{
						if(gravity_factor>0){
							IMAGE_POINT_CLOUD_UPDATE[counter].velocity = Vector3.zero;
							IMAGE_POINT_CLOUD_UPDATE[counter].angularVelocity =0;
							IMAGE_POINT_CLOUD_UPDATE[counter].rotation=0;
						}

						Vector3 pos = IMAGE_POINT_CLOUD[counter].position;//new Vector3(x * size_x, 0 , z * size_z);
//						if(particleSystem.simulationSpace == ParticleSystemSimulationSpace.Local){
//							pos = new Vector3(x * size_x, DEPTH*DEPTH_FACTOR , z * size_z);
//						}
//						else{
//							pos = particleSystem.transform.position + new Vector3(x * size_x, DEPTH*DEPTH_FACTOR  , z * size_z);
//							
//						}
						if(gravity_factor==0){
							//IMAGE_POINT_CLOUD_UPDATE[counter].position = pos;
						}else{
							//IMAGE_POINT_CLOUD_UPDATE[counter].position = Vector3.Slerp(IMAGE_POINT_CLOUD_UPDATE[counter].position, pos,gravity_factor*Time.deltaTime);
							IMAGE_POINT_CLOUD_UPDATE[counter].position = Vector3.Lerp(IMAGE_POINT_CLOUD_UPDATE[counter].position, pos,gravity_factor*Time.deltaTime);
						}
						IMAGE_POINT_CLOUD_UPDATE[counter].startSize = PARTICLE_SIZE;	
						if(gravity_factor==0){
						
						}else{
							IMAGE_POINT_CLOUD_UPDATE[counter].startColor = IMAGE_POINT_CLOUD[counter].startColor;
						}
						if(extend_life){
							
							IMAGE_POINT_CLOUD_UPDATE[counter].remainingLifetime = 16-5;
							IMAGE_POINT_CLOUD_UPDATE[counter].startLifetime = 15;
							
						}
						counter++;
					}
				}


				GetComponent<ParticleSystem>().SetParticles(IMAGE_POINT_CLOUD_UPDATE,IMAGE_POINT_CLOUD_UPDATE.Length);

			}

		if(Changing_texture){

			Color32[] Pixels = image_texture.GetPixels32();

			if(use_Depth_map){
				if(depth_image_texture !=null){
					Pixels2 = depth_image_texture.GetPixels32();
				}
			}
			
			float size_x = SCALE_FACTOR / (image_texture.width - 1);
			float size_z = SCALE_FACTOR / (image_texture.height - 1);
			
			int counter = 0;
			for (int x = 0; x < image_texture.width; x++)
			{
				for (int z = 0; z < image_texture.height; z++)
				{
					Color32 pixel_color = Pixels[x + z * image_texture.width];


					
					Vector3 pos = new Vector3(x * size_x, 0 , z * size_z);
					
					if (GIVE_DEPTH)
					{
						float DEPTH =  ((Color)pixel_color).grayscale;

						if(use_Depth_map){
							if(depth_image_texture !=null){
							Color32 pixel_color2 = Pixels2[x + z * image_texture.width];

							if(expose_highlights){
								DEPTH = expose_factor*DEPTH + (pixel_color2.r/3+ pixel_color2.g/3+ pixel_color2.b/3)/255f;
							}else{DEPTH = (pixel_color2.r/3+ pixel_color2.g/3+ pixel_color2.b/3)/255f;}

							}
						}
						
						if(GetComponent<ParticleSystem>().main.simulationSpace == ParticleSystemSimulationSpace.Local){
							pos = new Vector3(x * size_x, DEPTH*DEPTH_FACTOR , z * size_z);
						}
						else{
							pos = GetComponent<ParticleSystem>().transform.position + new Vector3(x * size_x, DEPTH*DEPTH_FACTOR  , z * size_z);
							
						}
						
					}
					
					IMAGE_POINT_CLOUD_UPDATE[counter].startSize = PARTICLE_SIZE;
					IMAGE_POINT_CLOUD_UPDATE[counter].position = pos;
					IMAGE_POINT_CLOUD_UPDATE[counter].startColor = pixel_color;

						if(extend_life){

							IMAGE_POINT_CLOUD_UPDATE[counter].remainingLifetime = 16-5;
							IMAGE_POINT_CLOUD_UPDATE[counter].startLifetime = 15;

						}
					
					counter++;
				}
			}

			

			GetComponent<ParticleSystem>().SetParticles(IMAGE_POINT_CLOUD_UPDATE, IMAGE_POINT_CLOUD_UPDATE.Length);//change for v1.2

		}

	}

	private void DEFINE_POINTS()
	{
			if(GetComponent<ParticleSystem>() == null | image_texture == null){
				Debug.Log ("Please attach to a particle system and add an image (advanced mode, read/write enabled)");
				return;
			}

		int particle_count = image_texture.width * image_texture.height;
		IMAGE_POINT_CLOUD = new ParticleSystem.Particle[particle_count];

		GetComponent<ParticleSystem>().Emit(particle_count);
		GetComponent<ParticleSystem>().GetParticles(IMAGE_POINT_CLOUD);

		Color32[] Pixels = image_texture.GetPixels32();

		float size_x = SCALE_FACTOR / (image_texture.width - 1);
		float size_z = SCALE_FACTOR / (image_texture.height - 1);

		int counter = 0;
		for (int x = 0; x < image_texture.width; x++)
		{
			for (int z = 0; z < image_texture.height; z++)
			{
				Color32 pixel_color = Pixels[x + z * image_texture.width];

				Vector3 pos = new Vector3(x * size_x, 0 , z * size_z);

				if (GIVE_DEPTH)
				{
					float DEPTH =  ((Color)pixel_color).grayscale;

					if(GetComponent<ParticleSystem>().main.simulationSpace == ParticleSystemSimulationSpace.Local){
						pos = new Vector3(x * size_x, DEPTH*DEPTH_FACTOR , z * size_z);
					}
					else{
						pos = GetComponent<ParticleSystem>().transform.position + new Vector3(x * size_x, DEPTH*DEPTH_FACTOR , z * size_z);

					}

				}

				IMAGE_POINT_CLOUD[counter].startSize = PARTICLE_SIZE;
				IMAGE_POINT_CLOUD[counter].position = pos;
				IMAGE_POINT_CLOUD[counter].startColor = pixel_color;

				counter++;
			}
		}

		IMAGE_POINT_CLOUD_UPDATE = (ParticleSystem.Particle[])IMAGE_POINT_CLOUD.Clone();

		GetComponent<ParticleSystem>().SetParticles(IMAGE_POINT_CLOUD_UPDATE, IMAGE_POINT_CLOUD_UPDATE.Length);
	}

}

}