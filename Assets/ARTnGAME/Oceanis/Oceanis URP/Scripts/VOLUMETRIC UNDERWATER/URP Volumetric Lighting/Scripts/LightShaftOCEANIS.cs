using UnityEngine.Rendering;

namespace Artngame.Oceanis
{
    public class LightShaftOCEANIS : VolumeComponent
    {
        public IntParameter maxIterations = new IntParameter(64);
        public FloatParameter maxDistance = new FloatParameter(12f);
        public FloatParameter minDistance = new FloatParameter(0.4f);

        public TextureParameter maskTexture = new TextureParameter(null);

        //v0.2
        //public TextureParameter cookieTexture = new TextureParameter(null);

        public Vector4Parameter shaftsDensity = new Vector4Parameter(new UnityEngine.Vector4(1, 1, 1, 1));

        //v0.1
        public FloatParameter cutoffHeigth = new FloatParameter(0);

        public ClampedFloatParameter intensity = new ClampedFloatParameter(0f, 0f, 1f);

        public bool IsActive => intensity.value > 0f;
    }
}