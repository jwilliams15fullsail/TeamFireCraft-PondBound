//IceDepth.hlsl - https://www.youtube.com/watch?v=_dZthyScKF8 ---- https://www.youtube.com/watch?v=rlGNbq5p5CQ
void IceDepthMasked_half(in Texture2D MainTex, in Texture2D MaskTex, in float2 UV, in float4 TilingOffset, in float4 maskTilingOffset, in SamplerState SS, in float Samples, in float Offset, in float3 WPOS, in float Lerp, in int LOD,in float4 maskPower, out float4 Out)
{
half4 col =0; 
half4 colA = 0;
half u_off =0;
half v_off = 0;
half samples = Samples;

for (int s = 0; s < samples; s++) {
	float2 uvs = float2(u_off, v_off);
	col +=  SAMPLE_TEXTURE2D_LOD(MainTex, SS, (uvs + UV)*TilingOffset.xy + TilingOffset.zw, LOD)* maskPower.w;

	colA += SAMPLE_TEXTURE2D_LOD(MaskTex, SS, (uvs + UV)*maskTilingOffset.xy + maskTilingOffset.zw, LOD) * 1;// maskPower.w;

	u_off += Offset * (_WorldSpaceCameraPos.x - WPOS.x);
	v_off += Offset * (_WorldSpaceCameraPos.z - WPOS.z);
}
half4 colB = 1*colA*maskPower.x + col*(maskPower.y) + col*(1-colA)*maskPower.z;
half4 render = (colB /= samples);

half4 surfCol = SAMPLE_TEXTURE2D(MainTex, SS, UV*TilingOffset.xy + TilingOffset.zw) + SAMPLE_TEXTURE2D(MaskTex, SS, UV*maskTilingOffset.xy + maskTilingOffset.zw);
surfCol = 1 * colA*maskPower.x + surfCol * (maskPower.y) + surfCol * (1 - colA)*maskPower.z;
Out = lerp (surfCol,render, Lerp);
}