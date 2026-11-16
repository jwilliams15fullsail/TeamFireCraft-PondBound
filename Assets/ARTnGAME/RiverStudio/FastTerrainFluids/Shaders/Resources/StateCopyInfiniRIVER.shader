Shader "Hidden/CopyInfiniRIVER"
{
    SubShader
    {
        Cull Off 
        ZWrite Off 
        ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert//vert_img
            #pragma fragment frag

            #include "UnityCG.cginc"
            
            sampler2D _MainTex;
            sampler2D _HTex;
			sampler2D _PrevTex;			
			float _Scale;
			float _Bias;			
			int _SrcChannel;
			int _TgtChannel;

			sampler2D _AppendTex;
			float _Append;
			float depthFactor;float depthOffset;

			
			//v0.1 - RIVER
			float3 _DepthCameraPos;
			float _TerrainScale;
			float depthCamFactor;
			float depthScale;
			struct VertexInput
			{
				float4 vertex : POSITION;
				float4 texcoord : TEXCOORD0;
			};
			struct VertexOutput
			{
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
				float3 worldPos:  TEXCOORD1;
			};
			VertexOutput vert(VertexInput v)
			{
				VertexOutput o;
				float4 worldPos = mul(unity_ObjectToWorld, v.vertex);
				//float3 worldPos = mul(_Object2World, vertex).xyz;
				o.worldPos = worldPos.xyz;
				//v.vertex = mul(unity_WorldToObject, worldPos);			
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = v.texcoord.xy;
				//COMPUTE_EYEDEPTH(o.screenPos.z);//
				return o;
			}


            float4 frag (VertexOutput i) : SV_Target//fixed4 frag (v2f_img i) : SV_Target
            {
                float colSrc = UnpackHeightmap(tex2D(_HTex, i.uv));
                float4 colPrev = tex2D(_PrevTex, i.uv);


				///v0.1
				//RIVER
				float2 flowCOORD = i.uv.xy;
				//_DepthCameraPos
				float offsetPos = 0;
				float offsetScale = 1;
				if (_TerrainScale > 0) {
					offsetPos = 0.5;
					offsetScale = 0.5;
					//flowCOORD = 10*(i.worldPos.xz - _DepthCameraPos.xz)*0.015*1.12 * _TerrainScale* offsetScale*depthFactor - float2(offsetPos, offsetPos)*4;//  texcoord.xy;
					//flowCOORD =  (i.uv.xy*depthFactor - 10 * _DepthCameraPos.xz*0.015*1.12 * _TerrainScale* offsetScale*depthCamFactor) - float2(offsetPos, offsetPos) * 1;//  texcoord.xy;
					//offsetPos = 0.5;
					//offsetScale = 0.5;
					//flowCOORD = 10 * (float2(i.uv.x - 0.5, i.uv.y - 0.5)*depthFactor)*0.015*1.12 * _TerrainScale* offsetScale;// -float2(1, 1)*depthCamFactor;
					flowCOORD = float2(i.uv.x, i.uv.y)*depthFactor;// +float2(_DepthCameraPos.x, _DepthCameraPos.z)*depthCamFactor;// -float2(1, 1)*depthCamFactor;
				}
				//v0.2
				//float2 flowBumpMYFLOWERC = flowCOORD.xy * float2(fluidSize.z, fluidSize.w) * offsetScale + float2(fluidSize.x + offsetPos, fluidSize.y + offsetPos);

				//float4 colAppend = tex2D(_AppendTex, i.uv);
				//get depth from depth texture
				float depth =  tex2D(_AppendTex, flowCOORD).r;
				depth = 1 - depth;
				//linear depth between camera and far clipping plane
				if (depth <= 0 || depth >=1 || flowCOORD.x > 1 || flowCOORD.x < 0 || flowCOORD.y > 1 || flowCOORD.y < 0) {
					depth = 0;// Linear01Depth(depth)*depthFactor;
				}
				else {
					depth = Linear01Depth(depth * depthScale); //+ depthOffset;//depth = Linear01Depth(depth)*depthFactor + depthOffset;
					depth = depth * _ProjectionParams.z*depthCamFactor + depthOffset;
				}
				//depth as distance from camera in units
				//depth = depth * _ProjectionParams.z*depthFactor;
				//skip wave and return source color if we're at the skybox
				//if (depth >= _ProjectionParams.z)

                
                colPrev.r = colSrc * _Scale + _Bias;	

				if (_Append == 0) {
					return colPrev;
				}
				else {
					//return colPrev + float4(depth, depth, depth, 0)*depthFactor;// colAppend;// colPrev + colAppend;
					return float4(max(depth, colPrev.x), colPrev.y, colPrev.z, colPrev.a);
				}
            }
            ENDCG
        }
    }
}
