using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Artngame.SKYMASTER;
namespace Artngame.Oceanis
{
    public class OceanisDemoPresetsSliderControl : MonoBehaviour
    {
        private Slider presetsChanger;
        public GlobalOceanisControllerURP water;
        int prevPreset = 0;

        void Awake()
        {
            presetsChanger = GetComponent<Slider>();
        }

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            if (presetsChanger.value != prevPreset)
            {
                //load preset
                water.copyID = (int)presetsChanger.value;
                water.copyPreset = true;
            }
        }
    }
}