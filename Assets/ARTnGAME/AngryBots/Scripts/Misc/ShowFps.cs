using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

namespace Artngame.PDM {
	[ExecuteInEditMode]
public class ShowFps : MonoBehaviour {

		private Text gui;

		private float updateInterval = 1.0f;
		private double lastInterval; // Last interval end time
		private int frames = 0; // Frames over current interval

		void Start()
		{
			lastInterval = Time.realtimeSinceStartup;
			frames = 0;
		}

		void OnDisable ()
		{
			if (gui)
				DestroyImmediate (gui.gameObject);
		}

		void Update()
		{
			#if !UNITY_FLASH
			++frames;
			float timeNow = Time.realtimeSinceStartup;
			if (timeNow > lastInterval + updateInterval)
			{
				if (!gui)
				{
					GameObject go = new GameObject("FPS Display", typeof(Text));
					go.hideFlags = HideFlags.HideAndDontSave;
					go.transform.position = new Vector3(0,0,0);
					gui = go.GetComponent<Text>();
					//gui.pixelOffset = new Vector2(5,55);
				}
				float fps = (float)(frames / (timeNow - lastInterval));
				float ms = 1000.0f / Mathf.Max (fps, 0.00001f);
				gui.text = ms.ToString("f1") + "ms " + fps.ToString("f2") + "FPS";
				frames = 0;
				lastInterval = timeNow;
			}
			#endif
		}
}
}