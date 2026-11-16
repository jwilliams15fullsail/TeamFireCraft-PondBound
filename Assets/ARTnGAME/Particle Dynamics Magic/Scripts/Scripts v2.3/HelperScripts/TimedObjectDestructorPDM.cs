using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Artngame.PDM {
public class TimedObjectDestructorPDM : MonoBehaviour {

	public float timeOut = 1.0f; //v2.5
    public bool detachChildren = false;

	void Awake ()
	{
		Invoke ("DestroyNow", timeOut);
	}

        void DestroyNow()
        {
            if (detachChildren)
            {
                transform.DetachChildren();
            }
            //DestroyObject (this.gameObject);
            Destroy(this.gameObject); //v2.5
        }
}
}