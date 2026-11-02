using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Artngame.TEM
{
    public class CameraController : MonoBehaviour
    {
        [SerializeField] private AnimationCurve shakeCurve;

        private Vector3 targetPosition;
        private Vector3 offset;
        public bool enableShake = false;
        public bool relativeShake = true;

        private void Awake()
        {
            targetPosition = transform.position;
        }

        private void Update()
        {
            if (enableShake)
            {
                if (relativeShake)
                {
                    transform.position = transform.position + offset;
                }
                else
                {
                    transform.position = targetPosition + offset;
                }
            }
        }

        public void StartExplosion()
        {
            StartCoroutine(ScreenShake());
        }

        private IEnumerator ScreenShake()
        {
            for (float t = 0; t < 0.5f; t += Time.deltaTime)
            {
                float y = shakeCurve.Evaluate(t * 2.0f);
                offset = new Vector3(0.0f, y, 0.0f);
                yield return null;
            }

            offset = Vector3.zero;
        }
    }
}