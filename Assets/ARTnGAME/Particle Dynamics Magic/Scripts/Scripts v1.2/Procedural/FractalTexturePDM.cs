using UnityEngine;
using System.Collections;
using Artngame.PDM;

namespace Artngame.PDM {

public class FractalTexturePDM : MonoBehaviour {

public bool gray = true;
	public int width = 128;
	public 	int height = 128;

	public 	float lacunarity = 6.18f;
	public 	float h = 0.69f;
	public 	float octaves = 8.379f;
	public 	float offset = 0.75f;
	public 	float scale = 0.09f;

	public float offsetPos = 0.0f;

	private Texture2D texture ;
	private PerlinPDM perlin ;
	private FractalNoise fractal ;

	public Vector3 COLOR_SHIFT;

void Start ()
{
	texture = new Texture2D(width, height, TextureFormat.RGB24, false);
	GetComponent<Renderer>().material.mainTexture = texture;
}

	void Update()
{
	Calculate();
}

	void  Calculate()
{
	if (perlin == null)
		perlin = new PerlinPDM();
	fractal = new FractalNoise(h, lacunarity, octaves, perlin);
	
	for (int y = 0;y<height;y++)
	{
			for (int x = 0;x<width;x++)
		{
			if (gray)
			{
				float value = fractal.HybridMultifractal(x*scale + Time.time, y * scale + Time.time, offset);
				texture.SetPixel(x, y, new Color (value, value, value, value));
			}
			else
			{
				offsetPos = Time.time;
					float valuex = fractal.HybridMultifractal(x*scale + offsetPos * 0.6f, y*scale + offsetPos * 0.6f, offset);
					float valuey = fractal.HybridMultifractal(x*scale + 161.7f + offsetPos * 0.2f, y*scale + 161.7f + offsetPos * 0.3f, offset);
					float valuez = fractal.HybridMultifractal(x*scale + 591.1f + offsetPos, y*scale + 591.1f + offsetPos * 0.1f, offset);
					texture.SetPixel(x, y, new Color (valuex+COLOR_SHIFT.x, valuey+COLOR_SHIFT.y, valuez+COLOR_SHIFT.z, 1));
			}
		}	
	}
	
	texture.Apply();
}

}
}