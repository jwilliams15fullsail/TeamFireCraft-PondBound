using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Artngame.RiverStudio
{
    public class activateSoundInfiniRIVER : MonoBehaviour
    {
        AudioSource audioS;
        public float increaseVolumeSpeed = 1;
        public float decreaseVolumeSpeed = 1;
        public float maxVolume = 0.35f;
        // Start is called before the first frame update
        void Start()
        {
            audioS = GetComponent<AudioSource>();
            prevPos = transform.position;
        }
        Vector3 prevPos;
        // Update is called once per frame
        void Update()
        {
            if (audioS != null)
            {
                if (audioS.clip != null)
                {
                    if (prevPos != transform.position)
                    {
                        audioS.volume = Mathf.Lerp(audioS.volume, maxVolume, Time.deltaTime * 5 * increaseVolumeSpeed);
                        prevPos = transform.position;
                    }
                    else
                    {
                        audioS.volume = Mathf.Lerp(audioS.volume, 0, Time.deltaTime * 5 * decreaseVolumeSpeed);
                    }
                }
            }
        }
    }
}
