using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using System.Collections.Generic;
using UnityEngine.UI;

namespace Artngame.RiverStudio.RiverCreator
{
    public class RoadifierExample : MonoBehaviour
    {
        public Slider smoothingSlider;
        public Slider smoothingIterationsSlider;
        public Slider widthSlider;
        public GameObject flagPrefab;
        private List<GameObject> flags = new List<GameObject>();
        private List<Vector3> points = new List<Vector3>();

        // Start is called before the first frame update
        void Start()
        {
            // hardcoded data to illustrate usage
            for (float x = -5.0f; x <= 5.0f; x += 0.5f)
            {
                // just a silly function to demonstrate capabilities of the road
                // renderer... ignore it, please. :)
                float z = Mathf.Sin(x * 2.0f) + x * 0.8f + x * x * 0.035f;
                points.Add(new Vector3(x, 0.0f, z));

                GameObject flag = Instantiate(flagPrefab);
                flag.transform.position = new Vector3(x, Terrain.activeTerrain.SampleHeight(new Vector3(x, 0, z)), z);
                flags.Add(flag);
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetMouseButtonDown(1))
            {
                PlacePoint();
            }
        }

        void ClearAll()
        {
            ClearOldRoad();
            points.Clear();

            foreach (GameObject f in flags)
            {
                Destroy(f);
            }
            flags.Clear();
        }

        void ClearOldRoad()
        {
            GameObject previousRoad = GameObject.Find("Roadifier Road");
            if (previousRoad)
            {
                Destroy(previousRoad);
            }
        }

        void PlacePoint()
        {
            Vector3 pos = new Vector3(0, 0, 0);
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Terrain.activeTerrain.GetComponent<Collider>().Raycast(ray, out hit, Mathf.Infinity))
            {
                pos = hit.point;
            }

            if (pos != Vector3.zero)
            {
                points.Add(pos);
                GameObject flag = Instantiate(flagPrefab);
                flag.transform.position = pos;
                flags.Add(flag);
            }
        }

        void RunExample()
        {
            ClearOldRoad();

            // make a copy of the list so we don't modify the user generated one
            // during smoothing processes and whatnot
            List<Vector3> pointsToSend = new List<Vector3>(points);

            GetComponent<Roadifier>().smoothingFactor = smoothingSlider.value;
            GetComponent<Roadifier>().smoothingIterations = (int)smoothingIterationsSlider.value;
            GetComponent<Roadifier>().roadWidth = widthSlider.value;
            GetComponent<Roadifier>().GenerateRoad(pointsToSend, Terrain.activeTerrain);
        }

    }
}
