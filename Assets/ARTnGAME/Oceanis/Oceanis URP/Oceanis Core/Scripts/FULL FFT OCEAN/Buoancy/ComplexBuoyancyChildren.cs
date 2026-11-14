//*
// Buoyancy.cs
// by Alex Zhdankin
// Version 2.1
//
// http://forum.unity3d.com/threads/72974-Buoyancy-script
//
// Terms of use: do whatever you like

//https://github.com/William93/CCB/blob/master/Scripts/buoyancy.cs
// Buoyancy.js
// resconstructed for multi colliders dependent from rigidbody
// by Marvin Papin
// Version 1.0
//
// http://forum.unity3d.com/threads/72974-Buoyancy-script
//
// Terms of use: do whatever you like

// Effects : Find each colliders attached to rigidbody and get points according to the dimension of the rigidbody
//	slices : each colliders according to bounds, calculate slices for each axis according to local bounds (ex on X : size rigidB = 10, collider analysed = 2, number of slice on X = 20, calculated slice for that object === 20/(10/2) = 5)
//	    |-> allow found points to "fall" better into objects for thin walls rigidBody by example.


// IMPORTANT : avoid to much important slices, maybe i did wrong but that could be unrealistic or event make unity bug (50x50x50 points in = bug)
// Do not work with concave colliders (is this really usefull ?) because they are hollow.
// Use any other colliders MeshCollider(Covex), BoxCollider, SphereCollider...

using System.Collections.Generic;
using UnityEngine;
using Artngame.Oceanis;

namespace Artngame.PANDORA {
public class ComplexBuoyancyChildren : MonoBehaviour
{

public float waterLevel  = -4.33f;
public float density  = 500.0f;
public int slicesX  = 5; // transformed according to dimension ratio with whole rigidbody bounds
public int slicesY  = 5;
public int slicesZ  = 5;
public int voxelsLimit  = 16;

private int layerEmpty= 7; // a layer where the object will be placed to cast rays at pos0, the last built-in array is probably the less used

private float WATER_DENSITY = 1000.0f;
private float DAMPFER = 0.03f; // resistance to movement / visquosity

private float voxelHalfHeight;
private Vector3 localArchimedesForce ;
private Vector3[] voxels ;
private List<Vector3[]> forces = new List<Vector3[]>(); // For drawing force gizmos

private Bounds combinedBounds  ;
private Bounds[] boundsList;

public List<Collider> collidersArray = new List<Collider>();

		//WAVES
		public bool WaveEffect = false;//use wave motion for forces
		//public Water2DPANDORA Water2D;//water script
		public float WaveForceDepth = 2;//how far from wave top object will be affected
		
		//Particles
		public ParticleSystem FoamParticle; // move to script on impact and add side force motion
		public ParticleSystem BubbleParticle; // move to script position and emit depending on velocity
		public ParticleSystem SplatParticle; // move to script position and emit something when hit
		
		public float BubbleCutoffH=0.3f; //how far above water height bubble particle will be de-parented

		Transform This_transf;
		Rigidbody RGB;

void Awake()
{	
			This_transf = transform;
			RGB = GetComponent<Rigidbody>();

	// Store original rotation and position
	Transform originalParent  = transform.parent; //keep order
	transform.parent = null;
	Quaternion originalRotation  = transform.rotation;
	Vector3 originalPosition  = transform.position;
	Vector3 originalScale = transform.localScale;
	transform.rotation = Quaternion.identity;
	transform.position = Vector3.zero;
	transform.localScale = Vector3.one;
	
	// Get whole rigidbody bounds
	//var collidersArray : Array = new Array(0);

	
	Collider[] colliders ;

	collidersArray = GetCollidersRecursively(gameObject);
	colliders = collidersArray.ToArray();//.ToBuiltin(Collider);
	int nbColliders  = colliders.Length;
	if (nbColliders == 0) // The object must have a collider
	{
		gameObject.AddComponent<MeshCollider>();
		colliders = new Collider[1];
		boundsList = new Bounds[1];
		colliders[0] = GetComponent<Collider>();
		boundsList[0] = GetComponent<Collider>().bounds;
		Debug.LogWarning("You are trying to calculate buoyancy on -" + transform.name + "- which has no collider even in children");
	}
	else // boundList
	{
		boundsList = new Bounds[nbColliders];
			for (int c1 = 0; c1 < nbColliders; c1++) {
				boundsList[c1] = colliders[c1].bounds;
			}
	}
	// combined Bounds
		for (int b1 = 0; b1 < nbColliders; b1++){
			combinedBounds.Encapsulate(boundsList[b1]);
		}
	
	// Get a reference height
	Bounds bounds  = combinedBounds;
	voxelHalfHeight = Mathf.Min(bounds.size.x, bounds.size.y, bounds.size.z);
	voxelHalfHeight /= 2 * (slicesX + slicesY + slicesZ)*1.0f/3;
	
	// The object must have a RidigBody
	if (GetComponent<Rigidbody>() == null)
	{
		gameObject.AddComponent<Rigidbody>();
		Debug.LogWarning("You are trying to calculate buoyancy on -" + transform.name + "- which has no Rigidbody attached");
	}
	//rigidbody.centerOfMass = Vector3(0, -bounds.extents.y * 0f, 0) + transform.InverseTransformPoint(bounds.center);
	
	voxels = SliceIntoVoxels(boundsList, colliders);
	
	// Restore original rotation and position
	transform.rotation = originalRotation;
	transform.position = originalPosition;
	transform.localScale = originalScale;
	transform.parent = originalParent;
	
		float volume  = GetComponent<Rigidbody>().mass / density;
	
	WeldPoints(voxels, voxelsLimit);
	
	float archimedesForceMagnitude = WATER_DENSITY * Mathf.Abs(Physics.gravity.y) * volume;
	localArchimedesForce = new Vector3(0, archimedesForceMagnitude, 0) / voxels.Length;
	
	//Debug.Log(string.Format("[Buoyancy.cs] Name=\"{0}\" volume={1:0.0}, mass={2:0.0}, density={3:0.0}", name, volume, GetComponent<Rigidbody>().mass, density));
}

List<Collider> GetCollidersRecursively (GameObject obj) // limited to Rigidbody area
{
	//var listColliders : Array = new Array(0);
	List<Collider> listColliders = new List<Collider>();

	if (!obj) return null;
	Collider col  = obj.GetComponent<Collider>();
	if (col)
	{
		if (!col.isTrigger)// listColliders.Push(col);
			{
				listColliders.Add(col);
			}
	}
	
	foreach (Transform child in obj.transform)
	{
		if (!child) continue;
			if (child.GetComponent<Rigidbody>()) continue; // get rid of children physically independent

		//var tempArray : Array = new Array(0);
			List<Collider>  tempArray = GetCollidersRecursively(child.gameObject);

		//tempArray = GetCollidersRecursively(child.gameObject);
		//listColliders = listColliders.Concat(tempArray);

			listColliders.AddRange(tempArray);

	}

//		var listColliders : Array = new Array(0);
//		
//		if (!obj) return;
//		var col : Collider = obj.GetComponent(Collider);
//		if (col)
//		{
//			if (!col.isTrigger) listColliders.Push(col);
//		}
//		
//		for (var child : Transform in obj.transform)
//		{
//			if (!child) continue;
//			if (child.GetComponent(Rigidbody)) continue; // get rid of children physically independent
//			var tempArray : Array = new Array(0);
//			tempArray = GetCollidersRecursively(child.gameObject);
//			listColliders = listColliders.Concat(tempArray);
//		}

	//return listColliders.ToArray();
	return listColliders;
}

Vector3[] SliceIntoVoxels(Bounds[] bounds, Collider[] colliders )
{
	//var points : Array = new Array(0);
	List<Vector3> points = new List<Vector3>();

	int nbColliders  = colliders.Length;
	int[]  layer = new int[nbColliders];
	for (int c1 = 0; c1 < nbColliders; c1++)
	{
		layer[c1] = colliders[c1].gameObject.layer;				// Place rigidbody into an empty layer to cast points
		colliders[c1].gameObject.layer = layerEmpty;			// store original layers
		//points = points.Concat(SlicePerCollider(bounds[c1], layerEmpty));	// Get points in the colliders for each bounds of them
			points.AddRange(SlicePerCollider(bounds[c1], layerEmpty));
	}
	
		if (points.Count == 0) {
			points.Add (transform.position);
		}
	
		for (var c3 = 0; c3 < nbColliders; c3++) {
			colliders [c3].gameObject.layer = layer [c3];
		} // Replace rigidbody members into right layer
	
		return points.ToArray();//.ToBuiltin(Vector3);
}

Vector3[] SlicePerCollider(Bounds bounds, int layerEmpty)
{
		float slicesLocalX  = Mathf.CeilToInt(slicesX * bounds.size.x / combinedBounds.size.x);
		float slicesLocalY  = Mathf.CeilToInt(slicesY * bounds.size.y / combinedBounds.size.y);
		float slicesLocalZ  = Mathf.CeilToInt(slicesZ * bounds.size.z / combinedBounds.size.z);
	//slicesLocalX = Mathf.Max(slicesLocalX, 2.0);
	//slicesLocalY = Mathf.Max(slicesLocalY, 2.0);
	//slicesLocalZ = Mathf.Max(slicesLocalZ, 2.0);
	// Whole GO slicing

	//var points : Array = new Array(0);
	List<Vector3> points = new List<Vector3>();

		Vector3 p ;
		Vector3 wp ;
		float x  ;
		float y ;
		float z ;
	for (int ix = 0; ix < slicesLocalX; ix++)
	{
			for (int iy = 0; iy < slicesLocalY; iy++)
		{
				for ( int iz = 0; iz < slicesLocalZ; iz++)
			{
				x = bounds.min.x + bounds.size.x / slicesLocalX * (0.5f + ix);
				y = bounds.min.y + bounds.size.y / slicesLocalY * (0.5f + iy);
				z = bounds.min.z + bounds.size.z / slicesLocalZ * (0.5f + iz);
				
				wp = new Vector3(x, y, z);
				
				float l = 0.1f;//rayLength
				///RaycastHit hit =new RaycastHit() ;
                        if (Physics.CheckSphere(wp, l, 1 << layerEmpty))
                        {
                            p = transform.InverseTransformPoint(wp);
                            points.Add(p);
                            // Debug.Log("p"+p);
                            //Debug.DrawLine(wp, wp+Vector3.one*l, Color.yellow, 20.0f, false);
                        }
                        else {
                           // Debug.DrawLine(wp, wp + Vector3.one * l / 10, Color.black, 20.0f, true);
                        }
			}
		}
	}
	return points.ToArray();
}

	private int firstIndex ;
	private int secondIndex ;

	Vector3[] WeldPoints( Vector3[] listBuiltin, int targetCount)
{
	//var list = new Array();
		List<Vector3> list = new List<Vector3>();

		for (int i = 0; i < listBuiltin.Length; i++) {
			//list.Push (listBuiltin [i]);
			list.Add(listBuiltin [i]);
		}
	//if (list.length <= 2 || targetCount < 2)
	if (list.Count <= 2 || targetCount < 2)
	{
		return null;
	}
	
	Vector3 mixed;
	while (list.Count > targetCount)
	{
		FindClosestPoints(list); // pass by private var : firstIndex, secondIndex
		
		mixed = (list[firstIndex] + list[secondIndex]) * 0.5f;
		list.RemoveAt(secondIndex); // the second index is always greater that the first => removing the second item first
		list.RemoveAt(firstIndex);
		list.Add(mixed);
	}
		return list.ToArray();//list.ToBuiltin(Vector3);
}

void FindClosestPoints(List<Vector3> list)
{
		float minDistance  = float.MaxValue;
		float maxDistance  = float.MinValue;
	firstIndex = 0;
	secondIndex = 1;
	
		float distance ;
		for ( int i  = 0; i < list.Count - 1; i++)
	{
			for ( int j = i + 1; j < list.Count; j++)
		{
			distance = Vector3.Distance(list[i], list[j]);
			if (distance < minDistance)
			{
				minDistance = distance;
				firstIndex = i;
				secondIndex = j;
			}
			if (distance > maxDistance)
			{
				maxDistance = distance;
			}
		}
	}
}


void Update(){

	if(BubbleParticle != null){
		if(BubbleParticle.transform.position.y > waterLevel + BubbleCutoffH){


                    // BubbleParticle.enableEmission = false;
                    ParticleSystem.EmissionModule em = BubbleParticle.emission;
                    em.enabled = false;


                    BubbleParticle.transform.parent = null;
			BubbleParticle.transform.position = new Vector3(0,-1000,0);
			BubbleParticle = null;
		}
	}


	if(WaveEffect){
                //if(Water2D != null){
                //	for (int i = 1; i < Water2D.WaterSprings.Count-1 ; i++)
                //	{ 		
                //		if( (This_transf.position.y < Water2D.WaterSprings[i+1].position.y)
                //		   & (Water2D.WaterSprings[i+1].position.y - This_transf.position.y) < WaveForceDepth){
                //			Vector3 Slope = Water2D.WaterSprings[i+1].position - Water2D.WaterSprings[i].position;
                //			Vector3 Force = new Vector3(Slope.x, Slope.y,0)*Water2D.WaterSprings[i].velocity.y;
                //			//RGB.AddForce(15*new Vector3(Force.x,Force.y,0));
                //			RGB.AddForceAtPosition(5*new Vector3(Force.x,Force.y,0),Water2D.WaterSprings[i+1].position);
                //		}
                //	}
                //}

                //for (int i = 1; i < Water2D.WaterSprings.Count - 1; i++)
                //{
                //    if ((This_transf.position.y < Water2D.WaterSprings[i + 1].position.y)
                //       & (Water2D.WaterSprings[i + 1].position.y - This_transf.position.y) < WaveForceDepth)
                //    {
                //        Vector3 Slope = Water2D.WaterSprings[i + 1].position - Water2D.WaterSprings[i].position;
                //        Vector3 Force = new Vector3(Slope.x, Slope.y, 0) * Water2D.WaterSprings[i].velocity.y;
                //        //RGB.AddForce(15*new Vector3(Force.x,Force.y,0));
                //        RGB.AddForceAtPosition(5 * new Vector3(Force.x, Force.y, 0), Water2D.WaterSprings[i + 1].position);
                //    }
                //}

            }
        }
        public float upForceFactor = 1;
void FixedUpdate()
{
	//forces = new Array(); // For drawing force gizmos
		forces = new List<Vector3[]>();

           // Debug.DrawLine(This_transf.position, This_transf.position + Vector3.up * 25, Color.grey, 16);

            foreach (Vector3 point in voxels)
	{
               // Debug.DrawLine(This_transf.position + point, This_transf.position + point + Vector3.up * 15, Color.cyan, 16);

                Vector3 wp  = transform.TransformPoint(point);

               // Debug.DrawLine(wp, wp + Vector3.up * 15, Color.magenta, 16);

                float waterLevel  = GetWaterLevel(wp);
                //Debug.Log("voxelHalfHeight=" + voxelHalfHeight+ ", wp.y = " + wp.y + ", waterLevel = " + waterLevel + ", Point = "+ point);
		if (wp.y - voxelHalfHeight < waterLevel)
		{
			float k  = (waterLevel - wp.y) / (2 * voxelHalfHeight) + 0.5f;
			if (k > 1)
			{
				k = 1f;
			}
			else if (k < 0)
			{
				k = 0f;
			}
			
			Vector3 velocity  = transform.GetComponent<Rigidbody>().GetPointVelocity(wp);
			Vector3 localDampingForce  = -velocity * DAMPFER * GetComponent<Rigidbody>().mass;
			Vector3 force  = localDampingForce + Mathf.Sqrt(k) * localArchimedesForce;
            //	GetComponent<Rigidbody>().AddForceAtPosition(force, wp);
          //  GetComponent<Rigidbody>().AddForceAtPosition(force * 0.003f * upForceFactor, wp);

                    GetComponent<Rigidbody>().AddForceAtPosition(force  * upForceFactor, wp,ForceMode.Force);

                    // Debug.Log("apply force=" + force);
                    Vector3[] adding = new Vector3[2];
			adding[0] = wp;
			adding[1] = force;
			//forces.Push(adding); // For drawing force gizmos
		    forces.Add(adding);
		}
	}
}

        //v0.1
        public GlobalOceanisControllerURP ocean;
        public bool useOceanHeight = false;
	float GetWaterLevel(Vector3 pos)
{
            //Debug.DrawLine(pos, pos + Vector3.up * 15, Color.red, 16);
            float positionY = waterLevel;//ocean.GetWaterHeight(pos);// ocean.GetWaterDisplacement(pos);

            if (useOceanHeight)
            {
                Vector3 displacment = ocean.GetWaterDisplacement(pos);
                //positionY = ocean.GetWaterHeight(pos);
                //if (Mathf.Abs(displacment.y) < 40)
                //{
                Vector3 heightSampler_position = new Vector3(pos.x + displacment.x,
                                                                    ocean.GetWaterHeight(pos),
                                                                    pos.z + +displacment.z);
                //heightSampler.position = Vector3.Lerp(heightSampler.position, heightSampler_position, Time.deltaTime * lerpSpeed);
                //heightSampler.position = new Vector3(heightSampler.position.x, GetWaterHeight(heightSampler.transform.position), heightSampler.position.z);
                positionY = ocean.GetWaterHeight(heightSampler_position);
                //}
            }

            //Debug.DrawLine(new Vector3(pos.x, positionY, pos.z), new Vector3(pos.x, positionY, pos.z) + Vector3.up * 15, Color.green, 19);
            //		return ocean == null ? 0.0f : ocean.GetWaterHeightAtLocation(x, z);
            return positionY; // positionY;// waterLevel;
}

	void OnDrawGizmos()
{
		float gizmoSize = 0.0005f;
		if(GetComponent<Rigidbody>()!=null){
			gizmoSize = 0.0005f* GetComponent<Rigidbody>().mass;
		}
	
	// draw forces and application points
	if (voxels != null && forces.Count >0)
	{
		Gizmos.color = Color.yellow;
		
		foreach (Vector3 p in voxels)
		{
			Gizmos.DrawCube(transform.TransformPoint(p), new Vector3(gizmoSize, gizmoSize, gizmoSize));
		}
		
		Gizmos.color = Color.cyan;
		
		foreach (Vector3[] force in forces)
		{
			Gizmos.DrawCube(force[0], new Vector3(gizmoSize, gizmoSize, gizmoSize));
				Gizmos.DrawLine(force[0], force[0] + force[1] / GetComponent<Rigidbody>().mass);
		}
	}
}








/*function SetLayerRecursively(obj : GameObject, newLayer : int) // limited to Rigidbody area
			        {
			            if (!obj) return;
			            obj.layer = newLayer;
			           
			            for (var child : Transform in obj.transform)
			            {
			                if (!child) continue;
			                if (child.GetComponent(Rigidbody)) continue; // get rid of children physically independent
			                SetLayerRecursively(child.gameObject, newLayer);
			            }
			        }//*/




}
}