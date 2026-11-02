Shader "TEM/GAP/DisableZWrite"
{
    Properties
    {       
    }

    SubShader
    {
        Tags {
            "RenderType" = "Opaque"
        }

        Pass{
            ZWrite Off
        }
    }
}
