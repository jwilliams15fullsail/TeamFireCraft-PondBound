//sampler3D Volume;
//sampler2D BackGroundTexture;

//v0.1

//sampler2D normalTex;
//sampler2D rampTex;

//v0.3


#define NUM_SAMPLES 16
struct Ray
{
	float3 origin;
	float3 dir;
};

struct AABB
{
	float3 Min;
	float3 Max;
};

//find intersection points of a ray with a box
bool IntersectBox(Ray r, AABB aabb, out float t0, out float t1)
{
	float3 invR = 1.0 / r.dir;
	float3 tbot = invR * (aabb.Min - r.origin);
	float3 ttop = invR * (aabb.Max - r.origin);
	float3 tmin = min(ttop, tbot);
	float3 tmax = max(ttop, tbot);
	float2 t = max(tmin.xx, tmin.yz);
	t0 = max(t.x, t.y);
	t = min(tmax.xx, tmax.yz);
	t1 = min(t.x, t.y);
	return t0 <= t1;
}

//float AbsorptionScale;
//float3 AbsorptionCoff;
//float3 Translate, Scale, Size;
//float3 lightDir;
//float brightA;
//float brightB;
//float4 colorB;

//void Fluid_float(in UnityTexture3D MainTex,in UnityTexture2D BackGroundTexture, in UnityTexture2D normalTex, in UnityTexture2D rampTex, in float3 UV, in SamplerState SS, in float3 WPOS,
//	in float4 grabPos, 
//	in float AbsorptionScale, in float AbsorptionCoff, in float3 Translate, in float3 Scale, in  float3 Size, in float3 lightDir, in float brightA, in float brightB, in float4 colorB,
void Fluid_float(in UnityTexture3D MainTex,  in SamplerState SS, in float3 WPOS,
	 in float3 Translate, in float3 Scale, 
	out float densityA, out float2 UVoffsetA, out float2 UVoffsetB)
	
	//out float4 Out)
{
	float3 pos = _WorldSpaceCameraPos;
	//float3 grab = tex2Dproj(BackGroundTexture, IN.grabPos).rgb;

	Ray r;
	r.origin = pos;
	r.dir = normalize(WPOS - pos);

	AABB aabb;
	aabb.Min = float3(-0.5, -0.5, -0.5) * Scale + Translate;
	aabb.Max = float3(0.5, 0.5, 0.5) * Scale + Translate;

	//figure out where ray from eye hit front of cube
	float tnear, tfar;
	IntersectBox(r, aabb, tnear, tfar);

	//if eye is in cube then start ray at eye
	if (tnear < 0.0) tnear = 0.0;

	float3 rayStart = r.origin + r.dir * tnear;
	float3 rayStop = r.origin + r.dir * tfar;

	//convert to texture space
	rayStart -= Translate;
	rayStop -= Translate;
	rayStart = (rayStart + 0.5 * Scale) / Scale;
	rayStop = (rayStop + 0.5 * Scale) / Scale;

	float3 start = rayStart;
	float dist = distance(rayStop, rayStart);
	float stepSize = dist / float(NUM_SAMPLES);
	float3 ds = normalize(rayStop - rayStart) * stepSize;
	float density = 0;
	int foundNormal = 0;//v0.1
	float3 normal = 0;
	float fluidHeight = 0;
	for (int i = 0; i < NUM_SAMPLES; i++, start += ds)
	{
		float4 sampleInVolume = SAMPLE_TEXTURE3D(MainTex, SS, start);  //tex3D(Volume, start);
		density += sampleInVolume.x;
		if (foundNormal == 0 && density > 0) {
			normal += 0.05*sampleInVolume.yzw;
			if (start.y > fluidHeight) {
				fluidHeight = start.y;
			}
		}
		if (density > 0) {
			density = density * 1.2;
		}
	}
	//float dotOfLight = dot(lightDir, normal);
	int normalPower = 1;

	//1.  float4 color = tex2D(sampler, float2(texX / texW, texY / texW);
	//2.  float4 color = tex2Dporj(sampler, float4(texX, texY, texZ, texW));

	densityA = density;
	UVoffsetA = saturate(float2(density*pow(normal.x, normalPower), density*pow(normal.y, normalPower)));
	UVoffsetB = float2(density*pow(normal.x, normalPower) / 115, density*pow(normal.y, normalPower) / 115);


	//float3 grab = tex2Dproj(BackGroundTexture, grabPos + float4(density*pow(normal.x, normalPower), density*pow(normal.y, normalPower), 0, 0)).rgb;
//	float3 grab = SAMPLE_TEXTURE2D(BackGroundTexture, SS, grabPos.xy + float2(density*pow(normal.x, normalPower), density*pow(normal.y, normalPower))).rgb;
//	float3 grab2 = SAMPLE_TEXTURE2D(BackGroundTexture, SS, grabPos.xy
//		+ float2(density*pow(normal.x, normalPower) / 115, density*pow(normal.y, normalPower) / 115)).rgb;
		//+ float4(density*pow(normal.x, normalPower) / 115, density*pow(normal.y, normalPower) / 115, 0, 0)).rgb;

	/*
	float denisty_glow = saturate(grab / (density*0.99));
	if (density < 1) {
		denisty_glow = 0;
	}
	//v0.3
	float3 col = clamp(grab * exp(-AbsorptionCoff * density * AbsorptionScale) / 2
		+ grab2 * exp(-AbsorptionCoff * density * AbsorptionScale) / 2 + 0 * (denisty_glow)*(denisty_glow), 0, 1);
	Out =  float4(col * brightA + float3(1, 1, 1)*density* brightB * colorB*col, 1);
	*/

	//half4 col =0;
	//half u_off =0;
	//half v_off = 0;
	//half samples = Samples;
	//
	//for (int s = 0; s < samples; s++) {
	//	float2 uvs = float2(u_off, v_off);
	//	col += SAMPLE_TEXTURE2D_LOD(MainTex, SS, uvs + UV, LOD);
	//	u_off += Offset * (_WorldSpaceCameraPos.x - WPOS.x);
	//	v_off += Offset * (_WorldSpaceCameraPos.z - WPOS.z);
	//}
	//
	//half4 render = (col /= samples);
	//Out = lerp (SAMPLE_TEXTURE2D(MainTex,SS,UV),render, Lerp);
}