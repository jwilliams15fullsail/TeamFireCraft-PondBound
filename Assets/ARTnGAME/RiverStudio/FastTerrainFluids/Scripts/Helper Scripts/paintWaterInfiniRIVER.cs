using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Artngame.RiverStudio.FastFluids
{
    public class paintWaterInfiniRIVER : MonoBehaviour
    {
        [Range(32, 1024)]
        public int Width = 256;
        [Range(32, 1024)]
        public int Height = 256;
        public float BrushAmount = 0f;
        // Brush
        private Plane _floor = new Plane(Vector3.up, Vector3.zero);
        private float _brushRadius = 0.05f;
        private Vector4 _inputControls;
        private Material _copyMat;

        public List<Transform> waterSources = new List<Transform>();
        public int currentSource = 0;

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            for (int i = 0; i < waterSources.Count; i++)
            {
                if (i == currentSource)
                {
                    // Controls
                    _brushRadius = Mathf.Clamp(_brushRadius, 0.01f, 1f);

                    var ray = new Ray(waterSources[i].position + Vector3.up * 100, -Vector3.up);// Camera.main.ScreenPointToRay(Input.mousePosition);
                    var amount = 0f;
                    var brushX = 0f;
                    var brushY = 0f;

                    if (_floor.Raycast(ray, out var enter))
                    {
                        var hitPoint = ray.GetPoint(enter);
                        brushX = hitPoint.x / Width;
                        brushY = hitPoint.z / Height;
                        //Debug.Log(hitPoint);
                        // if (Input.GetMouseButton(0))
                        amount = BrushAmount;
                    }
                    else
                    {
                        amount = 0f;
                    }

                    brushX = waterSources[i].position.x / Width;
                    brushY = waterSources[i].position.z / Height;

                    _inputControls = new Vector4(brushX, brushY, _brushRadius, amount);
                    //Debug.Log(_inputControls + "AAA");
                    Shader.SetGlobalVector("_InputControls", _inputControls);
                }
            }
            currentSource++;
            if (currentSource > waterSources.Count)
            {
                currentSource = 0;
            }
        }
    }
}