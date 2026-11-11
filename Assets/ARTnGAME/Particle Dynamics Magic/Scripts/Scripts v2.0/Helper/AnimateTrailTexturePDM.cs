using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Artngame.PDM {

//[RequireComponent(typeof(LineRenderer))]
public class AnimateTrailTexturePDM : MonoBehaviour
{
	//PDM v2.0
	//bool use_line = false;// use line renderer instead of trail one

	//texture atlas anim
	public bool Animated_tex = false;
	public int TexColumns = 5;
	public int TexRows = 5;
	public float FPS = 10f;

	//public Color Start_color = Color.white;
	//public Color End_color = Color.white;	
	//LineRenderer line;
	//public Material lineMaterial;

	//public Vector2 Start_end_width = new Vector2(1,1);

	void Start()
	{
		//line = GetComponent<LineRenderer>();
		//line.SetVertexCount(2);
		//line.renderer.material = lineMaterial;
		//line.SetWidth(Start_end_width.x, Start_end_width.y);
		//
		if(Animated_tex){
			GetComponent<Renderer>().material.SetTextureScale("_MainTex", new Vector2(1f / TexColumns, 1f / TexRows));
			Timing = Time.fixedTime;
		}		
	}		

	int frame_counter=0;
	float Timing;

	void LateUpdate () {

		if(Animated_tex){				
			if(Time.fixedTime - Timing > (1/FPS)){

				float Y_coord = (int)(frame_counter/TexRows);
				float X_coord = frame_counter;
								
				GetComponent<Renderer>().material.SetTextureOffset("_MainTex", new Vector2( ((float)X_coord/TexColumns), ((float)Y_coord/TexRows)  ));
				if(frame_counter > (TexRows*TexColumns)){
					frame_counter=0;
				}else{
					frame_counter++;
				}
				Timing = Time.fixedTime;
			}
		}
	}
}
}