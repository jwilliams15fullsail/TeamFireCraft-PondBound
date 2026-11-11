// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

//
// Line shader for Stream
//
// Vertex format:
// position.x  = texture switch (0/1)
// texcoord.xy = uv for ParticleTex
//
Shader "Hidden/Kvant/Stream/Line"
{
    Properties
    {
        _ParticleTex1 ("-", 2D)    = ""{}
        _ParticleTex2 ("-", 2D)    = ""{}
        [HDR] _Color  ("-", Color) = (1, 1, 1, 1)
        _Tail         ("-", Float) = 1
    }

    CGINCLUDE

    #pragma multi_compile_fog

    #include "UnityCG.cginc"

    struct appdata
    {
        float4 position : POSITION;
        float2 texcoord : TEXCOORD0;
    };

    struct v2f
    {
        float4 position : SV_POSITION;
        half4 color : COLOR;
        UNITY_FOG_COORDS(0)
    };

    sampler2D _ParticleTex1;
    float4 _ParticleTex1_TexelSize;

    sampler2D _ParticleTex2;
    float4 _ParticleTex2_TexelSize;

    half4 _Color;
    half _Tail;

    v2f vert(appdata v)
    {
        v2f o;

        float2 uv = v.texcoord.xy + _ParticleTex1_TexelSize.xy / 2;

        float4 p1 = tex2Dlod(_ParticleTex1, float4(uv, 0, 0));
        float4 p2 = tex2Dlod(_ParticleTex2, float4(uv, 0, 0));

		//v0.2
		/*p1.x = p1.x * 0.5;
		p1.z = p1.z * 0.5;
		p2.x = p2.x * 0.5;
		p2.z = p2.z * 0.5;*/

        float sw = v.position.x;

        if (p1.w < 0)
        {
            o.position = UnityObjectToClipPos(float4(p2.xyz, 1));
        }
        else
        {
            float3 p = lerp(p2.xyz, p1.xyz, (1.0 - sw) * _Tail);

			//v0.1 - thickness
			//float3 p3 = 0.01*lerp(p1.xyz, p2.xyz, (1.0 - v.position.x) * _Tail);// tex2Dlod(_ParticleTex2, float4(uv + float2(0.000001, 0.000001), 0, 0));
			//p -= p3;
			//p.x += 0.004*p1.r + 0.005*p2.r + 0.006*p2.g;

            o.position = UnityObjectToClipPos(float4(p, 1));
        }

        o.color = _Color;
        o.color.a *= sw;

		//v0.1
		//o.color.a = 1/(pow(length(p1.xyz-p2.xyz)*120,1)) ;// (1.0 - sw) * _Tail;
		o.color.a = 1 / (pow(length(p1.xyz - p2.xyz) * 10, 1));

        UNITY_TRANSFER_FOG(o, o.position);

        return o;
    }

    half4 frag(v2f i) : SV_Target
    {
		float4 c = i.color;

		//c = float4(1,1,1,1);
		//float brighten = 0;
		//if (c.a <=0.3) {
		//	c =  float4(1,1,1,1);
		//}
		//c = saturate(pow(c,2));
		//c = saturate(pow(c*pow(c.a,0.8), 3)) *0.95*(saturate(c.a)+0.001);
		c = saturate(pow(c, 2)*c.a);
		//c.a = 1;
       // UNITY_APPLY_FOG(i.fogCoord, c);

		c = float4(c.rgb, 1) * 1;

        return c;
    }

    ENDCG

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            CGPROGRAM
            #pragma target 3.0
            #pragma vertex vert
            #pragma fragment frag
            ENDCG
        }
    } 
}
