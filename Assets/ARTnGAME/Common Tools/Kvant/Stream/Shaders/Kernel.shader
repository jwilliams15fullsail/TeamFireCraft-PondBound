//
// GPGPU kernels for Stream
//
// Texture format:
// .xyz = particle position
// .w   = particle life
//
Shader "Hidden/Kvant/Stream/Kernel"
{
    Properties
    {
		//v0.1
		fluidTex("-", 2D) = ""{}

        _MainTex     ("-", 2D)     = ""{}
        _EmitterPos  ("-", Vector) = (0, 0, 20, 0)
        _EmitterSize ("-", Vector) = (40, 40, 40, 0)
        _Direction   ("-", Vector) = (0, 0, -1, 0.2)
        _SpeedParams ("-", Vector) = (5, 10, 0, 0)
        _NoiseParams ("-", Vector) = (0.2, 0.1, 1)  // (frequency, amplitude, animation)
        _Config      ("-", Vector) = (1, 2, 0, 0)   // (throttle, life, random seed, dT)
    }

    CGINCLUDE

    #pragma multi_compile NOISE_OFF NOISE_ON

    #include "UnityCG.cginc"
    #include "ClassicNoise3D.cginc"

    sampler2D _MainTex;
	sampler2D fluidTex; //v0.1

    float3 _EmitterPos;
    float3 _EmitterSize;
    float4 _Direction;
    float2 _SpeedParams;
    float4 _NoiseParams;
    float4 _Config;

	//v0.2
	float4 scaleFluid;

    // PRNG function.
    float nrand(float2 uv, float salt)
    {
        uv += float2(salt, _Config.z);
        return frac(sin(dot(uv, float2(12.9898, 78.233))) * 43758.5453);
    }

    // Get a new particle.
    float4 new_particle(float2 uv)
    {

		

        float t = _Time.x;

        // Random position.
        float3 p = float3(nrand(uv, t + 1), nrand(uv, t + 2), nrand(uv, t + 3));
        p = (p - (float3)0.5) * _EmitterSize + _EmitterPos;


		//v0.1
		p.y = p.y * 0.005;
		//float4 pA = tex2D(fluidTex, uv * 0.1);
		//float4 pA = tex2D(fluidTex, float2(p.x, p.z)*0.07);
		//p.y = 1*pow(pA.r,5);

        // Life duration.
        float l = _Config.y * (0.5 + nrand(uv, t + 0));

        // Throttling: discard particle emission by adding offset.
        float4 offs = float4(1e8, 1e8, 1e8, -1e8) * (uv.x > _Config.x);

        return float4(p, l) + offs;
    }

    // Position dependant velocity field.
    float3 get_velocity(float3 p, float2 uv)
    {
		
		//v0.2
		float2 uvsFLUID = float2(1-(p.x) * scaleFluid.x, p.z * scaleFluid.y) * 0.07 + float2(scaleFluid.z, scaleFluid.w) * 0.07;

		//v0.1
		float4 pA = tex2D(fluidTex, uvsFLUID);


        // Random vector.
        float3 v = float3(nrand(uv, 4), nrand(uv, 5), nrand(uv, 6));
        v = (v - (float3)0.5) * 2;

        // Apply the spread parameter.
        v = lerp(_Direction.xyz, v, _Direction.w);

        // Apply the speed parameter.
        v = normalize(v) * lerp(_SpeedParams.x, _SpeedParams.y, nrand(uv, 7)) ;

#ifdef NOISE_ON
        // Add noise vector.
        p = (p + _Time.y * _NoiseParams.z) * _NoiseParams.x ;
        float nx = cnoise(p + float3(138.2 / pow(scaleFluid.x ,2), 0, 0));
        float ny = cnoise(p + float3(0, 138.2 / pow(scaleFluid.x, 2), 0));
        float nz = cnoise(p + float3(0, 0, 138.2 / pow(scaleFluid.x, 2)));
        v += float3(nx, ny, nz) * _NoiseParams.y / (scaleFluid.x);
#endif
		
		//v0.1
		v.y = 0;

		
		v.x += v.x * pow(pA.r, 6)*11.5 ;
		v.z += v.z * pow(pA.r, 6)*11.5 ;
	
		float thres = 0.565;
		if (pA.r < thres && pA.g < thres  && pA.b < thres) {
			float diff = thres - pA.r;
			//v.xyz = float3(0, 0, 0) ;
			v.xyz = float3(v.x , v.y, v.z) / (diff*220);
			
		}
		if (abs(v.x) > 25) {
			v.x = sign(v.x) * 25;
		}
		if (abs(v.z) > 25) {
			v.z = sign(v.z) * 25;
		}
		

		//if (pA.r < 0.5) {
		//	v.xyz = float3(0, 0, 0);
		//}

		//if (pA.r > 0.9) {
		//	v.xyz = float3(0, 0, 0);
		//s}
		//v.x = v.x * pA.r * 0.1 + pA.r*20;
		//v.z = v.z * pA.r * 0.1 + pA.r*20;

		//if (pA.r > 0.74) {//if (pA.r < 0.4 && pA.g < 0.4  && pA.b < 0.4) {
			//v.xyz = float3(0,0,0);
		//}
		//else {
		//	v.xyz = float3(0, 0, 0);
		//}

			//v.x = v.x*pA.r * 1;
			//v.y =  pA.r * 2;
			//v.z = v.z*pA.r * 1;

		//v.y = pA.r * 4;
		//v.y = pow(pA.r,2) * 3;
		//v.x = (v.x * pow(pA.r, 2))*15;
		//v.z = (v.z * pow(pA.r, 2)) * 15;
		//v.x = (  pow(pA.r, 2)*110);
		//v.z = (  pow(pA.r, 2) * 101);

		v /= scaleFluid.x;
		if (abs(v.x) > (10 / scaleFluid.x)) {
			v.x = sign(v.x) * (10 / scaleFluid.x);
		}
		if (abs(v.z) > 1) {
			v.z = sign(v.z) * (10 / scaleFluid.x);
		}

        return v;
    }

    // Pass 0: Initialization
    float4 frag_init(v2f_img i) : SV_Target 
    {
        return new_particle(i.uv);
    }

    // Pass 1: Update
    float4 frag_update(v2f_img i) : SV_Target 
    {
        float4 p = tex2D(_MainTex, i.uv);
        if (p.w > 0)
        {
            float dt = _Config.w;


			//v0.2
			float2 uvsFLUID = float2(p.x * 2, p.z* 2)*0.07;

			//v0.1
			//float4 pA = tex2D(fluidTex, i.uv * 0.1);
			float4 pA = tex2D(fluidTex, uvsFLUID);


			

			//p.xyz += get_velocity(p.xyz, i.uv) * dt  * float3(pow(pA.r, 4) * 2,pow(pA.r,35)*10, pow(pA.r, 4) * 2);// +pA.r * dt; // position
			p.xyz += get_velocity(p.xyz, i.uv) * dt ;

			//v0.1
			//p.y = p.y + p.y * pA.r * 0.0005 + pA.r * 0.01;
			//p.y = p.y *  pA.r * 0.0001 + 0.01;
			//p.x = p.x + pA.r * dt * 10;
			//p.z = p.z + pA.r * dt * 10;
			//p.y = 1 + pA.r * 2;

            p.w -= dt;                               // life
            return p;
        }
        else
        {
            return new_particle(i.uv);
        }
    }

    ENDCG

    SubShader
    {
        // Pass 0: Initialization
        Pass
        {
            CGPROGRAM
            #pragma target 3.0
            #pragma vertex vert_img
            #pragma fragment frag_init
            ENDCG
        }
        // Pass 1: Update
        Pass
        {
            CGPROGRAM
            #pragma target 3.0
            #pragma vertex vert_img
            #pragma fragment frag_update
            ENDCG
        }
    }
}
