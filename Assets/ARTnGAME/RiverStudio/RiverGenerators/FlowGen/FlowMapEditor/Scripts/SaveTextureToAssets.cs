using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

namespace Artngame.RiverStudio.FlowMap
{
    public class SaveTextureToAssets
    {

        public static void Save(Texture2D image)
        {
            Texture2D tex = image;
            byte[] bytes = tex.EncodeToPNG();
            File.WriteAllBytes("Assets/FlowMap.png", bytes);
        }
    }
}