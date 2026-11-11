using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Artngame.CommonTools;
using Artngame.SplineMesh;

namespace Artngame.RiverStudio
{
    public class riverCurverInfiniRIVER : MonoBehaviour
    {
        // public SplineMesh splineMesh;
        public SplineExtrusion extrudedMeshHolder;
        public Spline splineHolder;
        public Terrain terrainToCurve;

        //public bool dropToGround = false;
        [HideInInspector]
        public bool adaptToGround = false;

        public float Dist_Above_Terrain = 0;

        //provide positions to plant based on ground height, also adapt spline as option first, option to conform to all meshes woth raycasting
        public void conformToGround(Spline splineHolder, bool Use_curve, float Dist_Above_Terrain, bool adaptOriginalSpline, bool conformToEverything)
        {          

            bool Conform_to_ground = true;
            int Spline_points = splineHolder.nodes.Count;//SplinePath.SplinePoints.Count;
            if (Use_curve)
            {
                //Spline_points = SplinePath.Curve.Count;
            }
            Vector3 position = Vector3.zero;
            //if spline exists,overite position
            for (int k = 0; k < Spline_points; k++)
            {
                if (Use_curve)
                {
                    //position = splineHolder.nodes[k].Position;// SplinePath.Curve[k].position;
                    position = splineHolder.transform.TransformVector(splineHolder.nodes[k].Position) + splineHolder.transform.position;
                    //Debug.DrawLine(position, position + Vector3.up * 110, Color.red, 20);
                    //Debug.Log(splineHolder.nodes[k].Position + "," + position);
                    if (Conform_to_ground)
                    {
                        if (terrainToCurve != null)
                        {
                            float find_y = 0;
                            find_y = terrainToCurve.SampleHeight(new Vector3(position.x, 0, position.z)) + Dist_Above_Terrain
                                + terrainToCurve.transform.position.y;
                            position = new Vector3(position.x, find_y, position.z);
                        }
                        else
                        {
                            //project to plane
                            position = new Vector3(position.x, Dist_Above_Terrain, position.z);
                        }
                    }
                }
                else
                {
                    position = splineHolder.nodes[k].Position;//SplinePath.SplinePoints[k].position;
                }
                Debug.DrawLine(position, position + Vector3.up * 10, Color.cyan, 20);
                splineHolder.nodes[k].Position = splineHolder.transform.InverseTransformVector(position - splineHolder.transform.position);
            }
        }
        public float curvePerMeters = 0.1f;
        //Change ground On Splines 
        public void changeGround(Terrain terrain)
        {
            if (splineHolder != null)
            {//& PlaceOnSpline) {

                //v2.1.25
                TerrainData tData = terrain.terrainData;
                TerrainData tData_SAVED = tData;
                xResHEIGHTS = tData.heightmapResolution;
                yResHEIGHTS = tData.heightmapResolution;
                //savedHEIGHTS= tData.GetHeights(0,0,xResHEIGHTS,yResHEIGHTS);
                int layers = tData.alphamapLayers;
               // CustomTexturePIXELS = CustomTexture.GetPixels();
                yRes = tData.alphamapHeight;
                xRes = tData.alphamapWidth;

                //change mesh terrain
                //Vector3[] Vertices = UnityTerrainMesh.vertices;

                float splineLength = splineHolder.Length;
                int sampleCount = (int)(splineHolder.Length / curvePerMeters);

                //for (int m = 0; m < splines.Count; m++)
                {
                    //for (int n=0;n<TerraSplines[m].SplinePoints.Count;n++){	

                    //if (AllSplines | (!AllSplines & m == splineUsed))
                    {

                        for (float n = 0; n < splineLength; n = n + curvePerMeters)// n + 1)
                        {
                            //change texture if needed to scale !!! here
                            //BrushSize = CustomTexture.width;
                            //BrushSize = BrushSize
                            int WIDTH = BrushSize;
                            int HEIGHT = BrushSize;

                            //Vector3 Pos =  TerraSplines[m].SplinePoints[n].position;
                            Vector3 Pos = splineHolder.transform.TransformVector(splineHolder.GetSampleAtDistance(n).location) + splineHolder.transform.position;                          
                                    float find_y = 0;
                                    find_y = terrainToCurve.SampleHeight(new Vector3(Pos.x, 0, Pos.z)) + Dist_Above_Terrain
                                        + terrainToCurve.transform.position.y;
                            Pos = new Vector3(Pos.x, find_y, Pos.z);
                            Debug.DrawLine(Pos, Pos + Vector3.up * 15, Color.cyan, 20);


                            if (operation_type > 0)
                            {
                                int x11 = (int)Mathf.Lerp(0, xResHEIGHTS, Mathf.InverseLerp(0, tData.size.x, Pos.x));
                                int z11 = (int)Mathf.Lerp(0, yResHEIGHTS, Mathf.InverseLerp(0, tData.size.z, Pos.z));

                                x11 = Mathf.Clamp(x11, WIDTH / 2, xResHEIGHTS - WIDTH / 2);
                                z11 = Mathf.Clamp(z11, HEIGHT / 2, yResHEIGHTS - HEIGHT / 2);
                                float[,] Heights = tData.GetHeights(x11 - WIDTH / 2, z11 - HEIGHT / 2, WIDTH, HEIGHT);
                                float[,] savedHEIGHT = tData_SAVED.GetHeights(x11 - WIDTH / 2, z11 - HEIGHT / 2, WIDTH, HEIGHT); //tData_SAVED.GetHeights (x11 - WIDTH / 2, z11 - HEIGHT / 2, WIDTH, HEIGHT);
                                for (int i = 0; i < HEIGHT; i++)
                                {
                                    for (int j = 0; j < WIDTH; j++)
                                    {
                                        //GRASS
                                        //HEIGHMAP
                                        if (Heights[i, j] > savedHEIGHT[i, j] - 1f)
                                        {
                                            //float max_height = 500;
                                            if (operation_type == 1)
                                            {
                                                //Heights[i, j] = savedHEIGHT[i, j] + Mathf.Clamp(CustomTexturePIXELS[j * WIDTH + i].a, 0, ForceSpeedThres) * 0.0045f * Forcepower; //0.0045f; perf >1 - 0.063f; - perf >2
                                                                                               
                                            }
                                            else if (operation_type == 2)
                                            {
                                                //Terrain.activeTerrain.
                                                Heights[i, j] = (Pos.y - terrain.transform.position.y) / MaxHeight; //0.0045f; perf >1 - 0.063f; - perf >2
                                            }
                                            else if (operation_type == 3)
                                            {
                                                //Terrain.activeTerrain.
                                                if (i < (HEIGHT / 2 + FlatRange) & i > (HEIGHT / 2 - FlatRange) & j < (WIDTH / 2 + FlatRange) & j > (WIDTH / 2 - FlatRange))
                                                {
                                                    Heights[i, j] = (Pos.y - terrain.transform.position.y) / (MaxHeight); //0.0045f; perf >1 - 0.063f; - perf >2
                                                }
                                                else
                                                {
                                                    float DistI = Mathf.Abs(i - HEIGHT / 2);
                                                    float DistJ = Mathf.Abs(j - WIDTH / 2);
                                                    Heights[i, j] = (Pos.y - savedHEIGHT[i, j]) / (MaxHeight + (DistI * MaxHeight * SlopeDropFactor) + (DistJ * MaxHeight * SlopeDropFactor) + Random.Range(0, SimpleNoiseFac));
                                                }
                                            }

                                        }
                                    }
                                }
                                tData.SetHeights(x11 - WIDTH / 2, z11 - HEIGHT / 2, Heights);
                            }
                            //END HEIGHTS

                            /////// ADD TEXTURING and PLANTING						
                            //int CHOSEN_TEXTURE =1;
                            //if (AddTexturing)
                            //{
                            //    int g = (int)Mathf.Lerp(0, xRes, Mathf.InverseLerp(0, tData.size.x, Pos.x));
                            //    int b = (int)Mathf.Lerp(0, yRes, Mathf.InverseLerp(0, tData.size.z, Pos.z));
                            //    g = Mathf.Clamp(g, CustomTexture.width / 2, xRes - CustomTexture.width / 2);
                            //    b = Mathf.Clamp(b, CustomTexture.height / 2, yRes - CustomTexture.height / 2);
                            //    float[,,] Alphamaps = tData.GetAlphamaps(g - CustomTexture.width / 2, b - CustomTexture.height / 2, CustomTexture.width, CustomTexture.height);

                            //    for (int x = 0; x < CustomTexture.height; x++)
                            //    {
                            //        for (int y = 0; y < CustomTexture.width; y++)
                            //        {
                            //            for (int z = 0; z < layers; z++)
                            //            {

                            //                //if (z == CHOSEN_TEXTURE){
                            //                if (TerraSplineSplatA.Count > m && z == TerraSplineSplatA[m])
                            //                {
                            //                    Alphamaps[x, y, z] += CustomTexturePIXELS[x * CustomTexture.width + y].a;
                            //                }
                            //                else
                            //                {
                            //                    Alphamaps[x, y, z] -= CustomTexturePIXELS[x * CustomTexture.width + y].a;
                            //                }

                            //            }
                            //        }
                            //    }

                            //    tData.SetAlphamaps(g - CustomTexture.width / 2, b - CustomTexture.height / 2, Alphamaps);
                            //}
                        }
                    }
                }
            }
        }//end change ground
        public bool AllSplines = false;
        public int splineUsed = 0;
        //public bool AddUntiyGrass = false;
        public bool AddTexturing = false;
        public int xResHEIGHTS;
        public int yResHEIGHTS;
        public Texture2D CustomTexture;
        Color[] CustomTexturePIXELS;
        public float Forcepower = 1;
        public float ForceSpeedThres = 1;
        public List<int> TerraSplineSplatA = new List<int>(); //Splat map to paint 
        int xRes;
        int yRes;



        //v2.1.25
        public bool ChangeGroundOnSplines;
        public float MaxHeight = 600;
        public int BrushSize = 32;
        public float SlopeDropFactor = 0.01f;
        public int FlatRange = 15;
        public float SimpleNoiseFac = 25;

        [Tooltip("0 =raise_ground , 1 = isoheight_ground , 2 = sloped_ground")]
        public int operation_type = 0; // 1 =raise_ground , 2 = isoheight_ground , 3 = sloped_ground, 4 = Adjust spline(s) to ground


        // Start is called before the first frame update
        void Start()
        {

        }
      
        // Update is called once per frame
        void Update()
        {
            if (adaptToGround)
            {
                adaptToGround = false;
                conformToGround(splineHolder, true, Dist_Above_Terrain, false,false);
            }
            if (ChangeGroundOnSplines)
            {
                ChangeGroundOnSplines = false;
                changeGround(terrainToCurve);
            }

        }
    }
}


/*
//v2.1.22
		public bool useSpline = false;
		public List<SplinerPInfiniGRASS> splines = new List<SplinerPInfiniGRASS>();
		public bool dropToGround = false;
		public bool OnSplineVolume = false;
		public bool removeOnSpline = false;
		public float firstOffset = 0; // add grass on an offset from the spline, in add mode
		public float lowerOnSpline = 0; //lower grass on shader level (comb)
		public float scaleOnSpline = 0; // scale on grower level
		public float MinDistToSpline = 10;

		//v2.1.25
		public float removeAfterOffset = 0; // if zero not use, if above MinDistToSpline use

		//provide positions to plant based on ground height, also adapt spline as option first, option to conform to all meshes woth raycasting
		public void conformToGround(SplinerPInfiniGRASS SplinePath, bool Use_curve, float Dist_Above_Terrain, bool adaptOriginalSpline, bool conformToEverything){ 
			bool Conform_to_ground = true;
			int Spline_points = SplinePath.SplinePoints.Count;

			if(Use_curve){
				Spline_points = SplinePath.Curve.Count;
			}
			Vector3 position;
			//if spline exists,overite position
			for (int k = 0; k < Spline_points; k++) {
				
				if (Use_curve) {
					position = SplinePath.Curve [k].position;

					if (Conform_to_ground) {
						if (Terrain.activeTerrain != null) {
							float find_y = 0;
							find_y = Terrain.activeTerrain.SampleHeight (new Vector3 (position.x, 0, position.z)) + Dist_Above_Terrain + Terrain.activeTerrain.transform.position.y;
							position = new Vector3 (position.x, find_y, position.z);
						} else {
							//project to plane
							position = new Vector3 (position.x, Dist_Above_Terrain, position.z);
						}
					}

				} else {
					position = SplinePath.SplinePoints [k].position;
				}
			}

		}

		//v2.1.25
		public bool ChangeGroundOnSplines;
		public float MaxHeight = 600;
		public int BrushSize = 32;
		public float SlopeDropFactor = 0.01f;
		public int FlatRange = 15;
		public float SimpleNoiseFac = 25;

		[Tooltip("0 =raise_ground , 1 = isoheight_ground , 2 = sloped_ground")]
		public	int operation_type = 0; // 1 =raise_ground , 2 = isoheight_ground , 3 = sloped_ground, 4 = Adjust spline(s) to ground

		public bool AllSplines = false;
		public int splineUsed = 0;
		//public bool AddUntiyGrass = false;
		public bool AddTexturing = false;
		public int xResHEIGHTS;
		public int yResHEIGHTS;
		public Texture2D CustomTexture;
		Color[] CustomTexturePIXELS;
		public float Forcepower = 1;
		public float ForceSpeedThres = 1;
		public List<int> TerraSplineSplatA = new List<int> (); //Splat map to paint 
		int xRes;
		int yRes;

		//Change ground On Splines 
		public void changeGround(Terrain terrain){
			if (splines.Count > 0 ){//& PlaceOnSpline) {

				//v2.1.25
				TerrainData tData = terrain.terrainData;
				TerrainData tData_SAVED = tData;
				xResHEIGHTS = tData.heightmapResolution;
				yResHEIGHTS = tData.heightmapResolution;
				//savedHEIGHTS= tData.GetHeights(0,0,xResHEIGHTS,yResHEIGHTS);
				int layers = tData.alphamapLayers;
				CustomTexturePIXELS = CustomTexture.GetPixels();
				yRes = tData.alphamapHeight;
				xRes = tData.alphamapWidth;

				//change mesh terrain
				//Vector3[] Vertices = UnityTerrainMesh.vertices;

				for (int m = 0; m < splines.Count; m++) {
					//for (int n=0;n<TerraSplines[m].SplinePoints.Count;n++){	

					if (AllSplines | (!AllSplines & m == splineUsed)) {
						//Loom.RunAsync(()=>{

						// Check whether the spline points have changed position.

						for (int n = 0; n < splines [m].Curve.Count; n = n + 1) {

							//change texture if needed to scale !!! here


							BrushSize = CustomTexture.width;

							int WIDTH = BrushSize;
							int HEIGHT = BrushSize;

							//Vector3 Pos =  TerraSplines[m].SplinePoints[n].position;
							Vector3 Pos = splines [m].Curve [n].position;

							if (operation_type > 0) {

								int x11 = (int)Mathf.Lerp (0, xResHEIGHTS, Mathf.InverseLerp (0, tData.size.x, Pos.x));
								int z11 = (int)Mathf.Lerp (0, yResHEIGHTS, Mathf.InverseLerp (0, tData.size.z, Pos.z));

								x11 = Mathf.Clamp (x11, WIDTH / 2, xResHEIGHTS - WIDTH / 2);
								z11 = Mathf.Clamp (z11, HEIGHT / 2, yResHEIGHTS - HEIGHT / 2);
								float[,] Heights = tData.GetHeights (x11 - WIDTH / 2, z11 - HEIGHT / 2, WIDTH, HEIGHT);
								float[,] savedHEIGHT = tData_SAVED.GetHeights (x11 - WIDTH / 2, z11 - HEIGHT / 2, WIDTH, HEIGHT); //tData_SAVED.GetHeights (x11 - WIDTH / 2, z11 - HEIGHT / 2, WIDTH, HEIGHT);
								for (int i = 0; i < HEIGHT; i++) {
									for (int j = 0; j < WIDTH; j++) {
										
										//GRASS
										//HEIGHMAP
										if (Heights [i, j] > savedHEIGHT [i, j] - 1f) {

											//float max_height = 500;
											if (operation_type == 1) {
												Heights [i, j] = savedHEIGHT [i, j] + Mathf.Clamp(CustomTexturePIXELS [j * WIDTH + i].a,0,ForceSpeedThres) * 0.0045f * Forcepower; //0.0045f; perf >1 - 0.063f; - perf >2

												//change mesh terrain
												//for (int o = 0; o < Vertices.Length; o++) {
												//Vertices [o] = Vertices [o] + new Vector3 (0, 15, 0);
												//}
												//	Vertices [MeshTerrainRes * i + j] = Vertices [MeshTerrainRes * i + j] + new Vector3 (0, CustomTexturePIXELS [i * WIDTH + j].a * 0.0045f * Forcepower, 0);


											} else if (operation_type == 2) {
												//Terrain.activeTerrain.
												Heights [i, j] = (Pos.y - terrain.transform.position.y) / MaxHeight; //0.0045f; perf >1 - 0.063f; - perf >2
											} else if (operation_type == 3) {
												//Terrain.activeTerrain.
												if (i < (HEIGHT / 2 + FlatRange) & i > (HEIGHT / 2 - FlatRange) & j < (WIDTH / 2 + FlatRange) & j > (WIDTH / 2 - FlatRange)) { 
													Heights [i, j] = (Pos.y - terrain.transform.position.y) / (MaxHeight); //0.0045f; perf >1 - 0.063f; - perf >2
												} else {
													float DistI = Mathf.Abs (i - HEIGHT / 2);
													float DistJ = Mathf.Abs (j - WIDTH / 2);
													//Heights [i,j] = (Pos.y - this.transform.position.y)/(MaxHeight + (DistI*MaxHeight*SlopeDropFactor) + (DistJ*MaxHeight*SlopeDropFactor));
													//Heights [i,j] = (Pos.y - Heights [i,j])/(MaxHeight + (DistI*MaxHeight*SlopeDropFactor) + (DistJ*MaxHeight*SlopeDropFactor));
													//Heights [i,j] = (Pos.y - this.transform.position.y)/(MaxHeight + (DistI*MaxHeight*SlopeDropFactor) + (DistJ*MaxHeight*SlopeDropFactor) + Random.Range(0,SimpleNoiseFac));

													Heights [i, j] = (Pos.y - savedHEIGHT [i, j]) / (MaxHeight + (DistI * MaxHeight * SlopeDropFactor) + (DistJ * MaxHeight * SlopeDropFactor) + Random.Range (0, SimpleNoiseFac));
												}
											}

											if (i == 1) {
												//Debug.Log("gg");
											}
										}
									}
								}
								tData.SetHeights (x11 - WIDTH / 2, z11 - HEIGHT / 2, Heights);	
							}
							//END HEIGHTS

							/////// ADD TEXTURING and PLANTING						
							//int CHOSEN_TEXTURE =1;
							if (AddTexturing) {
								int g = (int)Mathf.Lerp (0, xRes, Mathf.InverseLerp (0, tData.size.x, Pos.x));
								int b = (int)Mathf.Lerp (0, yRes, Mathf.InverseLerp (0, tData.size.z, Pos.z));
								g = Mathf.Clamp (g, CustomTexture.width / 2, xRes - CustomTexture.width / 2);
								b = Mathf.Clamp (b, CustomTexture.height / 2, yRes - CustomTexture.height / 2);
								float[,,] Alphamaps = tData.GetAlphamaps (g - CustomTexture.width / 2, b - CustomTexture.height / 2, CustomTexture.width, CustomTexture.height);

								for (int x = 0; x < CustomTexture.height; x++) {
									for (int y = 0; y < CustomTexture.width; y++) {
										for (int z = 0; z < layers; z++) {	

											//if (z == CHOSEN_TEXTURE){
											if (TerraSplineSplatA.Count > m && z == TerraSplineSplatA [m]) {
												Alphamaps [x, y, z] += CustomTexturePIXELS [x * CustomTexture.width + y].a;
											} else {	
												Alphamaps [x, y, z] -= CustomTexturePIXELS [x * CustomTexture.width + y].a;	
											}

										}
									}
								}

								tData.SetAlphamaps (g - CustomTexture.width / 2, b - CustomTexture.height / 2, Alphamaps);
							}
//
//							if (AddUntiyGrass) {
//								int[,] map0 = tData.GetDetailLayer (0, 0, tData.detailWidth, tData.detailHeight, FLOWER1);
//								int[,] map1 = tData.GetDetailLayer (0, 0, tData.detailWidth, tData.detailHeight, FLOWER2);
//
//								if (ENABLE_FLOWER_GROW == 1 & 1 == 1) {
//
//									map0_temp = (int[,])map0.Clone ();
//									map1_temp = (int[,])map1.Clone ();
//
//									for (int i = 0; i < tData.detailWidth; i++) {
//										for (int j = 0; j < tData.detailHeight; j++) {
//											map0_temp [i, j] = 0;
//											map1_temp [i, j] = 0;
//										}
//									}
//								}							
//
//								int g1 = (int)Mathf.Lerp (0, xRes2, Mathf.InverseLerp (0, tData.size.x, Pos.x));
//								int b1 = (int)Mathf.Lerp (0, yRes2, Mathf.InverseLerp (0, tData.size.z, Pos.z));
//
//								for (int y1 = g1; y1 < g1 + 20; y1 = y1 + 6) {
//									for (int x1 = b1; x1 < b1 + 20; x1 = x1 + 8) {
//										map0 [x1, y1] = 1;
//										map1 [x1, y1] = 1;
//
//										if (ENABLE_FLOWER_GROW == 1) {
//											map0_temp [x1, y1] = 1;
//											map1_temp [x1, y1] = 1;
//										}		      
//									}
//								}
//
//								if (ENABLE_FLOWER_GROW == 1 & 1 == 0) {	 
//									int numberDetails = tData.detailPrototypes.Length; // 2 - places 0 - 1 in array		  
//									if (1 == 0) {
//										DetailPrototype[] detailPrototypes_BIG;
//
//										detailPrototypes_BIG = new DetailPrototype[tData.detailPrototypes.Length + 1];
//
//										for (int k = 0; k < numberDetails; k++) {
//											detailPrototypes_BIG [k] = tData.detailPrototypes [k];
//										}
//
//										detailPrototypes_BIG [numberDetails] = tData.detailPrototypes [FLOWER1];
//										tData.detailPrototypes = detailPrototypes_BIG;				 
//										tData.RefreshPrototypes ();
//									}
//									GROWTH_Start = 1;
//									map0_ext = map0;
//									map1_ext = map1;	 
//								}
//
//								if (ENABLE_FLOWER_GROW != 1) {	  
//									tData.SetDetailLayer (0, 0, FLOWER1, map0);		  
//									tData.SetDetailLayer (0, 0, FLOWER2, map1);
//								}
//							}
							////// END TEXTURING and PLANTING
							//END TEXTURE
						}
					}
				}

				//change mesh terrain
				//UnityTerrainMesh.vertices = Vertices;

				//PlaceOnSpline = false;
			}
		}//end change ground

		//v2.1.25 - assign layer to planted, directly pass at creation and draw from pool in infinigrassField grower.
		public int placeInLayer = 0;

	}






    //v2.1.22
		public bool insideSplineCheck(Vector3 v, List<LandSplineNodeInfiniGRASS> p1)
		{
			int j = p1.Count-1;
			bool outputSign = false;
			for(int i=0;i<p1.Count;j=i++){
				Vector3 pi = p1 [i].position;
				Vector3 pj = p1 [j].position;
				outputSign ^= pi.z > v.z ^ pj.z > v.z && v.x < (pj.x - pi.x) * (v.z - pi.z) / (pj.z - pi.z) + pi.x;
			}
			return outputSign;
		}








    if (1==1) {
									if (PlantingRules [m].OnSplineVolume) { //do volume check

										//if add mode and is inside any of the splines, break and plant on the first found
										//if remove mode and is inside any, break and dont plant
										for (int p=0;p<PlantingRules[m].splines.Count;p++){
											//int signsLeft = 0;

											bool sign1 = insideSplineCheck (hit.point, PlantingRules [m].splines [p].SplinePoints);
											if (sign1) {
												if (PlantingRules [m].removeOnSpline) {
													plantBasedOnSpline = false;
													break;
												}else{
													plantBasedOnSpline = true;
													break;
												}
											}

											//
											//for (int p1=0;p1<PlantingRules[m].splines[p].SplinePoints.Count-1;p1++){
											//	//if on left half plane of each line
											//	//if( Vector3.Distance PlantingRules[m].splines[p].Curve[p1] - PlantingRules[m].splines[p].Curve[p1+1])
											//	//https://stackoverflow.com/questions/15688232/check-which-side-of-a-plane-points-are-on
											//	//dot( (a,b,c,d), (x,y,z,1) )
											//	Vector3 lineStart = PlantingRules[m].splines[p].SplinePoints[p1].position;
											//	Vector3 lineEnd = PlantingRules[m].splines[p].SplinePoints[p1+1].position;
											//	float sign = Mathf.Sign(Vector4.Dot(new Vector4(lineStart.x,0,lineStart.z,1),new Vector4(hit.point.x,0,hit.point.z,1)));
											//	//float sign = insideSplineCheck(lineEnd-lineStart,
											//	if (sign > 0) {
											//		signsLeft++;
											//	}
											//}
											//Debug.Log ("spline signs = " + signsLeft);
											//if(signsLeft == PlantingRules[m].splines[p].SplinePoints.Count-1){ //if all left
											//	plantBasedOnSpline = true;
											//}
											//
										}//END FOR LOOP

									} else { //do distance check
										//int smallestDistanceSplineID = -1;
										//int smallestDistanceID = -1;
										float smallestDist = 1000000;
										for (int p = 0; p<PlantingRules[m].splines.Count; p++) {
											
											for (int p1 = 0; p1<PlantingRules[m].splines[p].Curve.Count - 1; p1++) {
												float dist = Vector3.Distance(PlantingRules[m].splines[p].Curve[p1].position, hit.point);
												if (dist<smallestDist) {
													smallestDist = dist;
												}
											}

										}//END FOR LOOP
										if (smallestDist<PlantingRules[m].MinDistToSpline) {
											if (PlantingRules[m].removeOnSpline) {
												plantBasedOnSpline = false;
											} else {
//												if (PlantingRules [m].removeAfterOffset > PlantingRules [m].MinDistToSpline) {
//													plantBasedOnSpline = false;
//												} else {
//													plantBasedOnSpline = true;
//												}
												plantBasedOnSpline = true;
											}
										} else {
											if (PlantingRules[m].removeAfterOffset > 0 && smallestDist > PlantingRules[m].removeAfterOffset) {
												plantBasedOnSpline = false;
											} 
										}
									}
								}

 */
