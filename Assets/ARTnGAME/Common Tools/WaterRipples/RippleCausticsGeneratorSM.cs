using UnityEngine;
using System.Collections;
using System.Collections.Generic;
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem;
#endif

//namespace Artngame. {
namespace Artngame.CommonTools.FluidSim2D
{

    public class RippleCausticsGeneratorSM : MonoBehaviour
    {
        //v0.2
        public bool adjust3DWaves = false;
        public Vector4 waves3DAdjust = new Vector4(1.31f,3.27f,0.27f,0.23f);
        public bool adjustRippleFoam = false;
        public Vector4 RippleFoamAdjust = new Vector4(1f, 1f, 1f, 1f);
        public bool adjustRippleSpecular = false;
        public bool applyRippleTexture = true; //add texture to material if not already there
        public Vector4 RippleSpecularAdjust = new Vector4(1f, 1f, 1f, 1f);

        //v0.1
        public bool useFlow = false; //use Flow rendertexture, combine it with ripples and create caustics with combined target
        public RenderTexture causticsOUT;
        public RenderTexture flowIN;
        public RenderTexture heightFieldOUT;

        public float speedFactor = 1;

        public bool createCaustics = true;
        public float distrubanceSize = 0.5f;
        public Vector2 displaceCaustic = new Vector2(-4, 2);
        public float extraRippleFactor = 0;

        public Material fluid;
        public Texture2D brushTexture;
        public int resolution = 256;

        public float thickness = 3;
        public float mass = 10;
        public float Lamda = 4;
        public Vector4 dumper = new Vector4(0, 0, 0, 0);
        public float Amplitude = 1;

        private RenderTexture TexA, TexB;
        private RenderTexture TexMain;

        //Caustics
        public float RefractionAmount = 0.87f;
        public float Ydistance = 0.06f;
        public Vector3 Light = new Vector3(0.1f, 0.15f, 0.98f);
        public int resolutionX = 256;
        public int resolutionY = 256;

        public Material causticPatternMat;

        private Vector4 _texWidth;
        private RenderTexture causticSurf1;
        private RenderTexture causticSurf2;
        private RenderTexture causticSurfMain;

        public List<Material> passPostoMats = new List<Material>();
        public List<Transform> moveWithCamera = new List<Transform>();
        public float passMatScaler = 10;

        void Start()
        {

        }
        void OnDestroy()
        {
            ReleaseRipples();
        }
        void OnDisable()
        {
            ReleaseCausticTextures();
        }

        public Transform interactor;
        public bool useInteractor = false;

        //v0.3
        public bool useMouse = true;

        //v0.2
        public bool useInteractors = false;
        public List<Transform> interactors = new List<Transform>();
        public List<Vector3> interactorsPrevPos = new List<Vector3>();

        Vector3 prevInteractorPos;

        //v0.2
        public float BrushWidth = 0.05f;

        void doBrush()
        {

#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
            Vector3 mousePos = Mouse.current.position.ReadValue();
#else
			Vector3 mousePos = Input.mousePosition;
#endif

#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
            if ( ((Mouse.current != null && Mouse.current.leftButton.isPressed && useMouse)) || (useInteractor && interactor.position != prevInteractorPos)
            )
            {
#else
            if ((Input.GetMouseButton(0) && useMouse) || (useInteractor && interactor.position != prevInteractorPos))
            {
#endif
                var ray = Camera.main.ScreenPointToRay(mousePos);
                RaycastHit hit;
                Collider collisionBox = this.GetComponent<Collider>();

                if (useInteractor && interactor.position != prevInteractorPos)
                {
                    ray.direction = interactor.position - Camera.main.transform.position;
                    ray.origin = Camera.main.transform.position;
                    prevInteractorPos = interactor.position;
                }


                if (collisionBox != null && collisionBox.Raycast(ray, out hit, 10000000))
                {
                    Vector2 pos = hit.textureCoord * resolution;
                    Matrix4x4 projextionM = Matrix4x4.Ortho(0f, resolution, 0f, resolution, -1f, 1f);
                    Texture2D brushTex = brushTexture;
                    float brushWidth = BrushWidth * resolution;
                    Rect areaToPaint = new Rect(pos.x - brushWidth, pos.y - brushWidth, brushWidth * 2, brushWidth * 2);
                    GL.PushMatrix();
                    GL.LoadIdentity();
                    GL.LoadProjectionMatrix(projextionM);
                    RenderTexture.active = TexA;
                    Graphics.DrawTexture(areaToPaint, brushTex);
                    RenderTexture.active = null;
                    GL.PopMatrix();
                }
            }

            //v0.2
            if (useInteractors)
            {
                var ray = Camera.main.ScreenPointToRay(mousePos);
                RaycastHit hit;
                Collider collisionBox = this.GetComponent<Collider>();

                

                for (int i = 0; i < interactors.Count; i++)
                {
                    if (interactorsPrevPos.Count < interactors.Count)
                    {
                        interactorsPrevPos.Add(interactors[i].position);
                    }

                    ray.direction = interactors[i].position - Camera.main.transform.position;
                    ray.origin = Camera.main.transform.position;
                    //prevInteractorPos = interactors[i].position;

                    if (collisionBox != null && collisionBox.Raycast(ray, out hit, 10000000) && interactorsPrevPos[i] != interactors[i].position)
                    {
                        interactorsPrevPos[i] = interactors[i].position;

                        //Debug.Log("AAA="+i);

                        Vector2 pos = hit.textureCoord * resolution;
                        Matrix4x4 projextionM = Matrix4x4.Ortho(0f, resolution, 0f, resolution, -1f, 1f);
                        Texture2D brushTex = brushTexture;
                        float brushWidth = BrushWidth * resolution;
                        Rect areaToPaint = new Rect(pos.x - brushWidth, pos.y - brushWidth, brushWidth * 2, brushWidth * 2);
                        GL.PushMatrix();
                        GL.LoadIdentity();
                        GL.LoadProjectionMatrix(projextionM);
                        RenderTexture.active = TexA;
                        Graphics.DrawTexture(areaToPaint, brushTex);
                        RenderTexture.active = null;
                        GL.PopMatrix();
                    }
                }
            }

        }

        public Material CausticsMaterial;

        Vector3 prevCamPos;
        public float camPosScaler = 0;
        public bool useExtraWaves = false;
        public float camDistUpdateThreas = -1;

        void Update()
        {

            CheckInitRipples();
            float timerDelta = Time.deltaTime;
            float delta = Lamda / resolution;
            float accel = mass * Mathf.Pow(delta, 2);

            fluid.SetFloat("_thickness", thickness / accel);
            fluid.SetFloat("delta", delta);
            fluid.SetVector("dumper", dumper);
            fluid.SetFloat("Amplitude", Amplitude);
            fluid.SetFloat("extraRipples", extraRippleFactor);
            fluid.SetFloat("distrubanceSize", distrubanceSize);

            doBrush();

            // float camDiff2D = new Vector2(Camera.main.transform.position.x - prevCamPos.x, Camera.main.transform.position.z - prevCamPos.z).magnitude;
            //if (camDiff2D > camDistUpdateThreas)
            {
              

                //v0.2
                Vector3 diffCamPos = Camera.main.transform.position - prevCamPos;
                prevCamPos = Camera.main.transform.position;
                fluid.SetVector("diffCamPos", new Vector4(diffCamPos.x, diffCamPos.y, diffCamPos.z, camPosScaler));
            }

            if (speedFactor <= 0)
            {
                speedFactor = 0.01f;
            }
            for (int i = 0; i < timerDelta * speedFactor; i = i + 1)
            {
                Graphics.Blit(TexA, TexB, fluid, 0);
                ToggleTextures();
                fluid.SetVector("diffCamPos", new Vector4(0,0,0, camPosScaler));
                Graphics.Blit(TexA, TexMain, fluid, 1);
            }

            //v0.2a
            if (useExtraWaves)
            {
                Graphics.Blit(TexMain, TexB);
            }

            //v0.2
            for (int i = 0; i < passPostoMats.Count; i++)
            {
                //passPostoMats[i].SetVector("ripplesPos", new Vector4(Camera.main.transform.position.x, Camera.main.transform.position.z, 0, passMatScaler * this.transform.localScale.x));
                passPostoMats[i].SetVector("ripplesPos", 
                    new Vector4(this.transform.position.x, this.transform.position.z, 0, 
                    passMatScaler * this.transform.localScale.x));

                if (adjust3DWaves)
                {
                    //if (passPostoMats[i].HasProperty("rippleWaveParams"))
                    //{
                    passPostoMats[i].SetVector("rippleWaveParams", waves3DAdjust);
                    //}
                }
                if (adjustRippleFoam)
                {
                    passPostoMats[i].SetVector("RippleFoamAdjust", RippleFoamAdjust);
                }
                if (adjustRippleSpecular)
                {
                    passPostoMats[i].SetVector("RippleSpecularAdjust", RippleSpecularAdjust);
                }
                if (applyRippleTexture)
                {
                    passPostoMats[i].SetTexture("ripplesTex", heightFieldOUT);
                }
            }
            for (int i = 0; i < moveWithCamera.Count; i++)
            {
                moveWithCamera[i].transform.position = new Vector3(Camera.main.transform.position.x, moveWithCamera[i].transform.position.y, Camera.main.transform.position.z);
            }

            //v0.1
            if (useFlow)
            {
                //RenderTexture tempRendertarget = TexMain; 
                fluid.SetTexture("_FlowTex", flowIN);
                fluid.SetTexture("_PrevTex", TexMain);
                Graphics.Blit(null, TexB, fluid, 2);
                //TexMain = TexB;
            }

            if (CausticsMaterial != null)
            {
                CausticsMaterial.SetTexture("_BumpTexture", TexB); //set ripples to caustic material for debug //v0.1 was TexMain				
                Graphics.Blit(TexB, heightFieldOUT);
            }

            if (createCaustics)
            {
                CheckInitCaustics();

                causticPatternMat.SetTexture("_BumpTexture", TexB); //v0.1 was TexMain
                causticPatternMat.SetFloat("RefractionAmount", RefractionAmount);
                causticPatternMat.SetFloat("Ydistance", Ydistance);
                causticPatternMat.SetVector("Light", Light);
                causticPatternMat.SetVector("_texWidth", _texWidth);

                causticPatternMat.SetFloat("_Displace", displaceCaustic.x);
                causticSurf1.DiscardContents();
                Graphics.Blit(null, causticSurf1, causticPatternMat, 0);

                causticPatternMat.SetFloat("_Displace", displaceCaustic.y);
                causticSurf2.DiscardContents();
                Graphics.Blit(null, causticSurf2, causticPatternMat, 0);

                causticPatternMat.SetTexture("causticSurf1", causticSurf1);
                causticPatternMat.SetTexture("causticSurf2", causticSurf2);
                causticSurfMain.DiscardContents();
                Graphics.Blit(null, causticSurfMain, causticPatternMat, 1);

                if (causticsOUT != null)
                {
                    Graphics.Blit(causticSurfMain, causticsOUT);
                }

                if (CausticsMaterial != null)
                {
                    CausticsMaterial.SetFloat("Ydistance", Ydistance);
                    CausticsMaterial.SetFloat("RefractionAmount", RefractionAmount);
                    CausticsMaterial.SetTexture("causticSurfMain", causticSurfMain);
                }
            }
        }

        void ReleaseRipples()
        {
            if (TexMain != null)
            {
                TexMain.Release();
            }
            if (TexA != null)
            {
                TexA.Release();
            }
            if (TexB != null)
            {
                TexB.Release();
            }
        }
        void ReleaseCausticTextures()
        {
            if (causticSurf1 != null)
            {
                causticSurf1.Release();
            }
            if (causticSurf2 != null)
            {
                causticSurf2.Release();
            }
            if (causticSurfMain != null)
            {
                causticSurfMain.Release();
            }
        }
        void CheckInitCaustics()
        {
            if (causticSurf1 != null && causticSurf1.width == resolutionX && causticSurf1.height == resolutionY)
                return;

            ReleaseCausticTextures();
            _texWidth = new Vector4((1f / resolutionX), (1f / resolutionY), resolutionX, resolutionY);

            causticSurfMain = new RenderTexture(resolutionX, resolutionY, 0, RenderTextureFormat.RHalf, RenderTextureReadWrite.Default);
            causticSurf1 = new RenderTexture(resolutionX, resolutionY, 0, RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.Default);
            causticSurf2 = new RenderTexture(resolutionX, resolutionY, 0, RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.Default);
            causticSurfMain.wrapMode = TextureWrapMode.Repeat;

            causticSurfMain.filterMode = FilterMode.Bilinear;
            causticSurf1.filterMode = FilterMode.Bilinear;
            causticSurf2.filterMode = FilterMode.Bilinear;

            causticSurfMain.Create();
            causticSurf1.Create();
            causticSurf2.Create();
        }
        void ToggleTextures()
        {
            RenderTexture tempRendertarget = TexA;
            TexA = TexB;
            TexB = tempRendertarget;
        }
        void CheckInitRipples()
        {

            if (TexA != null && TexA.width == resolution)
            {
                return;
            }

            ReleaseRipples();
            TexA = new RenderTexture(resolution, resolution, 0, RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.Default);
            TexB = new RenderTexture(resolution, resolution, 0, RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.Default);
            TexMain = new RenderTexture(resolution, resolution, 0, RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.Default);
            TexMain.wrapMode = TextureWrapMode.Clamp;
            TexA.wrapMode = TextureWrapMode.Clamp;
            TexB.wrapMode = TextureWrapMode.Clamp;

            TexMain.filterMode = FilterMode.Bilinear;
            TexA.filterMode = FilterMode.Bilinear;
            TexB.filterMode = FilterMode.Bilinear;

            TexMain.Create();
            TexA.Create();
            TexB.Create();

            RenderTexture.active = TexA;
            GL.Clear(true, true, Color.black);
            RenderTexture.active = TexB;
            GL.Clear(true, true, Color.black);
            RenderTexture.active = null;
        }
    }
    //
}