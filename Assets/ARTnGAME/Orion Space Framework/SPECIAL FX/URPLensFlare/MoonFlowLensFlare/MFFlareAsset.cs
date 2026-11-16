
using System;
using System.Collections.Generic;
using UnityEngine;
namespace Artngame.Orion.LensFlaresMF
{
    [Serializable]
    public class MFFlareAsset : ScriptableObject
    {
        public Texture2D flareSprite;
        public bool fadeWithScale;
        public bool fadeWithAlpha;
        public List<MFFlareSpriteData> spriteBlocks;
    }
}