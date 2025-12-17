//WIP Grass shader

Shader "Fraktalia/Core/StandardExpansion/Foliage_UV3Pinned_V2"
{
	Properties
	{
		 _Color("Color", Color) = (1,1,1,1)
		 //[HDR]_ColorLighted ("ColorLighted", Color) = (1,1,1,1)
		 _MainTex("Albedo (RGB)", 2D) = "white" {}
		 _Lambert90deg("Lambert90deg", Range(0,1)) = 0.6
		 _Lambert180deg("Lambert180deg", Range(0,1)) = 0.8
		 _AmbientAmount("AmbientAmount", Range(0,1)) = 0.5
		 _Ambient0deg("Ambient0deg", Range(0,1)) = 0.0
		 _Ambient90deg("Ambient90deg", Range(0,1)) = 0.5
		 _Ambient180deg("Ambient180deg", Range(0,1)) = 1.0
		 _OffsetAmbient("OffsetAmbient", Range(-1,1)) = -0.1
			 //_Glossiness ("Smoothness", Range(0,1)) = 0.5
			 //_Metallic ("Metallic", Range(0,1)) = 0.0
			 _Cutoff("Transparency (Light transmission)", Range(0.01,1)) = 0.5
			 _AlphaOffset("Alpha offset", Range(-1,1)) = 0.1

		// Wind effect parameteres
		_WindFrecuency("Wind Frecuency",Range(0.001,100)) = 1
		_WindStrength("Wind Strength", Range(0, 2)) = 0.3
		_WindGustDistance("Distance between gusts",Range(0.001,50)) = .25
		_WindDirection("Wind Direction", vector) = (1,0, 1,0)

	}
	SubShader
	{
		  Tags {
			"RenderType" = "TransparentCutout"
			"Queue" = "AlphaTest"
			"IgnoreProjector" = "True"
		}
		LOD 200
		AlphaToMask On
		Cull Off
		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		 #pragma surface surf MyFoliage alphatest:_Cutoff vertex:vert

				#define SHIFT_MUL 8.0

				float4 _Color;
				float4 _ColorLighted;
				float _Lambert90deg;
				float _Lambert180deg;
				float _OffsetAmbient;
				float _AmbientAmount;
				float _Ambient0deg;
				float _Ambient90deg;
				float _Ambient180deg;
				float _AlphaOffset;
				//float _Metallic;
				float _Glossiness;
				//float _Cutoff;
				sampler2D _MainTex;

		half _WindFrecuency;
		half _WindGustDistance;
		half _WindStrength;
		float3 _WindDirection;

		// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
		// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
		// #pragma instancing_options assumeuniformscaling
		UNITY_INSTANCING_BUFFER_START(Props)
		// put more per-instance properties here
		UNITY_INSTANCING_BUFFER_END(Props)


		void vert(inout appdata_full v)
		{
			float4 localSpaceVertex = v.vertex;
			// Takes the mesh's verts and turns it into a point in world space
			// this is the equivalent of Transform.TransformPoint on the scripting side
			float4 worldSpaceVertex = mul(unity_ObjectToWorld, localSpaceVertex);

			// Height of the vertex in the range (0,1)
			float height = v.texcoord2.x / 256;

			worldSpaceVertex.x += sin(_Time.x * _WindFrecuency + worldSpaceVertex.x * _WindGustDistance) * height *
				_WindStrength * _WindDirection.x;
			worldSpaceVertex.z += sin(_Time.x * _WindFrecuency + worldSpaceVertex.z * _WindGustDistance) * height *
				_WindStrength * _WindDirection.z;

			// takes the new modified position of the vert in world space and then puts it back in local space
			v.vertex = mul(unity_WorldToObject, worldSpaceVertex);
		}

		half4 LightingMyFoliage(SurfaceOutput s, half3 lightDir, half atten) {

			// info at https://github.com/teadrinker/foliage-shader
			// Martin Eklund 2021

			half NdotL = dot(s.Normal, lightDir);
			NdotL = clamp(NdotL, -1.0, 1.0); // needed to avoid salt artifacts 
			half diff = NdotL >= 0 ? lerp(_Lambert90deg, 1., NdotL) : lerp(_Lambert90deg, _Lambert180deg, -NdotL);
			half amb = NdotL >= 0 ? lerp(_Ambient90deg, _Ambient0deg, NdotL) : lerp(_Ambient90deg, _Ambient180deg, -NdotL);
			half4 c;
			c.rgb = SHIFT_MUL * s.Albedo * _LightColor0.rgb * (diff * max(0, _OffsetAmbient + atten + _AmbientAmount * amb));
			c.a = s.Alpha;
			return c;
		}

		struct Input {
			float2 uv_MainTex;
		};

		void surf(Input IN, inout SurfaceOutput o) {
			float4 c = tex2D(_MainTex, IN.uv_MainTex);
			o.Albedo = c.rgb * _Color.rgb * (1.0 / SHIFT_MUL); // divide by SHIFT_MUL to get a darker baseline, otherwise the range of _OffsetAmbient feels too limited
			o.Alpha = c.a * _Color.a + _AlphaOffset;
		}
		ENDCG
	}

	FallBack "Diffuse"
}