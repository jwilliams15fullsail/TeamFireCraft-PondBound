using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Artngame.TEM
{
    public class NukeControlTEM : MonoBehaviour
    {
        public float nukeDelay;
        public float cameraShakeDelay;
        public GameObject missileVFX;
        public GameObject nukeVFX;
        public Animation camAnim;
        public AudioClip nukeSFX;
        public AudioClip missileSFX;

        private AudioSource audioSource;

        void Start()
        {
            audioSource = GetComponent<AudioSource>();
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                StartCoroutine(Nuke());
                audioSource.PlayOneShot(missileSFX);
                GameObject vfxMissile = Instantiate(missileVFX) as GameObject;
                Destroy(vfxMissile, nukeDelay + 1);
            }
        }

        IEnumerator Nuke()
        {
            yield return new WaitForSeconds(nukeDelay);
            audioSource.PlayOneShot(nukeSFX);
            GameObject vfxNuke = Instantiate(nukeVFX) as GameObject;
            Destroy(vfxNuke, 15);

            yield return new WaitForSeconds(cameraShakeDelay);
            camAnim.Play();
        }
    }
}