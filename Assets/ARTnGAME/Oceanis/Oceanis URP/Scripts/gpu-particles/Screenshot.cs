using UnityEngine;
namespace Artngame.Oceanis.GPUParticles
{
    public class Screenshot : MonoBehaviour
    {
        private static int count = 0;

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                ScreenCapture.CaptureScreenshot("Assets/Screenshots/screenshot" + count + ".png");
                count++;
            }
        }
    }
}