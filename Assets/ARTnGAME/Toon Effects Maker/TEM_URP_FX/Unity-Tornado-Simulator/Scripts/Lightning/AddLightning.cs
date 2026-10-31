using UnityEngine;
using System.Collections;
namespace Artngame.TEM
{
    public class AddLightning : MonoBehaviour
    {
        public GameObject lightningObj;

        float timer = 0f;

        public float timeUntilLightningMin = 1f;
        public float timeUntilLightningMax = 5f;
        float timeUntilLightning = 1f;

        void Update()
        {
            timer += Time.deltaTime;

            if (timer > timeUntilLightning)
            {
                timer = 0f;

                timeUntilLightning = Random.Range(timeUntilLightningMin, timeUntilLightningMax);

                //Add a new lightning
                float mapSize = 500f;

                float randomX = Random.Range(-mapSize, mapSize);
                float randomZ = Random.Range(-mapSize, mapSize);

                float y = 230f;

                Vector3 pos = new Vector3(randomX, y, randomZ);

                GameObject newLightning = Instantiate(lightningObj, pos, Quaternion.identity) as GameObject;

                newLightning.transform.parent = transform;
            }
        }
    }
}