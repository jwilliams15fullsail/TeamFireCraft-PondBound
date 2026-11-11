using System.Collections;
using System.Collections.Generic;
//using Toguchi.Rendering;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Artngame.Oceanis
{
    public class GradientFogOCEANIS : VolumeComponent
    {
        public TextureParameter gradientTexture = new TextureParameter(null);
        public TextureParameter maskTexture = new TextureParameter(null);

        //v0.1
        public FloatParameter cutoffHeigth = new FloatParameter(0);
        public FloatParameter depthFade = new FloatParameter(0);
        public ColorParameter fogTint = new ColorParameter(Color.white);//tint

        public ClampedFloatParameter intensity = new ClampedFloatParameter(0f, 0f, 1f);

        public bool IsActive => intensity.value > 0f;
    }
}