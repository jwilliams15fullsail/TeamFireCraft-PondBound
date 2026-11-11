using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Artngame.PDM {
public class DeactivatorOnLowQuality : MonoBehaviour {
		
		public QualityManager.Quality qualityThreshhold = QualityManager.Quality.High;  //Quality qualityThreshhold = Quality.High;

		void Start () {
			if (QualityManager.quality < qualityThreshhold)
			{
				gameObject.SetActive (false);
			}
			enabled = false;
		}
}
}