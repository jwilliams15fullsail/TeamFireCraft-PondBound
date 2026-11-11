using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Artngame.RiverStudio.FastFluids.Assets.Scripts.Utils
{
    [RequireComponent(typeof(Renderer))]
    public class ShowRendererBoundsInfiniRIVER : MonoBehaviour
    {
        public Color BoundsColor = Color.yellow;

        private Renderer _renderer;

        void OnDrawGizmos()
        {
            if (_renderer == null)
                _renderer = GetComponent<Renderer>();

            Gizmos.color = BoundsColor;
            var bounds = _renderer.bounds;
            Gizmos.DrawWireCube(bounds.center, bounds.size);
        }
    }
}
