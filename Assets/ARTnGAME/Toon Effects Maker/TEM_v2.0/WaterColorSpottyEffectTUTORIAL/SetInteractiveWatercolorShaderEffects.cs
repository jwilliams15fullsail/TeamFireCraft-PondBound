using System.Collections;
using System.Collections.Generic;
using UnityEngine;
////TUTORIAL
/////// https://www.bruteforce-games.com/post/watercolor-shader-devblog-13
////[ExecuteInEditMode]
public class SetInteractiveWatercolorShaderEffects : MonoBehaviour
{
    public RenderTexture rt;
    public string GlobalTexName = "_GlobalEffectRT";
    public string GlobalOrthoName = "_OrthographicCamSize";
    public Transform Player;
    private bool IsPlaying = false;
    
    private void Awake()
    {
            Shader.SetGlobalTexture(GlobalTexName, rt);
            Shader.SetGlobalFloat(GlobalOrthoName, GetComponent<Camera>().orthographicSize);
     
    }

    private void Update()
    {
        transform.position = new Vector3(Player.position.x, Player.position.y+20, Player.transform.position.z);
        Shader.SetGlobalVector("_Position", transform.position);
        Shader.SetGlobalFloat(GlobalOrthoName, GetComponent<Camera>().orthographicSize);

       if(!Application.isPlaying && IsPlaying)
        {
            IsPlaying = false;
            this.GetComponent<Camera>().backgroundColor = Color.white;
            this.GetComponent<Camera>().cullingMask = 1<<31;
        }
       else if(Application.isPlaying && !IsPlaying)
        {
            IsPlaying = true;
            this.GetComponent<Camera>().backgroundColor = Color.black;
            this.GetComponent<Camera>().cullingMask = 1 << 8;
        }
    }
}