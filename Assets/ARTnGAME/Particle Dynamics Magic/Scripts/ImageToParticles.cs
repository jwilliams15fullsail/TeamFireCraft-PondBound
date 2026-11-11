using UnityEngine;
using System.Collections;
using Artngame.PDM;

namespace Artngame.PDM {

[ExecuteInEditMode()]
public class ImageToParticles : MonoBehaviour
{

	public float SCALE_FACTOR = 0.1f;
	public float PARTICLE_SIZE = 1f;
	public Texture2D image_texture;
	private ParticleSystem.Particle[] IMAGE_POINT_CLOUD;
	private ParticleSystem.Particle[] IMAGE_POINT_CLOUD_UPDATE;
	public bool GIVE_DEPTH = true;

	public float DEPTH_FACTOR = 1f;
	
	void Start ()
	{
		DEFINE_POINTS();
	}

	void Update ()
	{
			if(GetComponent<ParticleSystem>() == null | image_texture == null){return;}

		if(!Application.isPlaying){
			Start ();
		}


		//GetComponent<ParticleSystem>().main.gravityModifier = 1;//v2.3
			ParticleSystem.MainModule main = GetComponent<ParticleSystem>().main;
			main.gravityModifier = 1;//v2.3

		float size_x = SCALE_FACTOR / (image_texture.width - 1);
		float size_z = SCALE_FACTOR / (image_texture.height - 1);

		int counter = 0;
		for (int x = 0; x < image_texture.width; x++)
		{
			for (int z = 0; z < image_texture.height; z++)
			{
			
				Vector3 pos = new Vector3(x * size_x, 0, z * size_z);

				if (GIVE_DEPTH)
				{
					pos = new Vector3(x * size_x, IMAGE_POINT_CLOUD[counter].position.y*DEPTH_FACTOR, z * size_z);
				}

				IMAGE_POINT_CLOUD_UPDATE[counter].startSize = PARTICLE_SIZE;
				IMAGE_POINT_CLOUD_UPDATE[counter].position = pos;

				counter++;
			}
		}
		
		GetComponent<ParticleSystem>().SetParticles(IMAGE_POINT_CLOUD_UPDATE, IMAGE_POINT_CLOUD_UPDATE.Length);

	}

	private void DEFINE_POINTS()
	{
		if(GetComponent<ParticleSystem>() == null | image_texture == null){
				Debug.Log ("Please attach to a particle system and add an image (advanced mode, read/write enabled)");
				return;
			}

		int particle_count = image_texture.width * image_texture.height;
		IMAGE_POINT_CLOUD = new ParticleSystem.Particle[particle_count];
		
		float size_x = SCALE_FACTOR / (image_texture.width - 1);
		float size_z = SCALE_FACTOR / (image_texture.height - 1);
		
		Color32[] Pixels = image_texture.GetPixels32();

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
					pos = new Vector3(x * size_x, DEPTH*DEPTH_FACTOR , z * size_z);}

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