Shader "Fraktalia/Core/MultiTexture_V2/Texture2D_UVBlend_1Layer_4Color"
{
    Properties
    {
        _TransformTex("Texture Transform", Vector) = (0, 0, 1, 1)

        [Header(Main Maps)]
        [LayerMaps(Layer 1, _DiffuseMap0, _MetallicGlossMap0, _BumpMap0, _ParallaxMap0, _OcclusionMap0, _EmissionMap0)]
        _DiffuseMap0("Diffuse (Albedo) Maps 0", 2D) = "white" {}
        [LayerMaps(Layer 2, _DiffuseMap1, _MetallicGlossMap1, _BumpMap1, _ParallaxMap1, _OcclusionMap1, _EmissionMap1)]
        _DiffuseMap1("Diffuse (Albedo) Maps 1", 2D) = "white" {}
            
        [Header(Color Layers)]
        _Color_Layer_9 ("Color Layer 9", Color) = (1,1,1,1)
        _Color_Layer_10("Color Layer 10", Color) = (1,1,1,1)
        _Color_Layer_11("Color Layer 11", Color) = (1,1,1,1)
        _Color_Layer_12("Color Layer 12", Color) = (1,1,1,1)
        _Emission_Layer_9("Emission Layer 9", Color) = (0,0,0,0)
        _Emission_Layer_10("Emission Layer 10", Color) = (0,0,0,0)
        _Emission_Layer_11("Emission Layer 11", Color) = (0,0,0,0)
        _Emission_Layer_12("Emission Layer 12", Color) = (0,0,0,0)

        _Color("Diffuse Color", Color) = (1,1,1,1)
        
        [HideInInspector] _MetallicGlossMap0("Metallic Maps 0", 2D) = "white" {}
        [HideInInspector] _MetallicGlossMap1("Metallic Maps 1", 2D) = "white" {}
        
        _Glossiness("Smoothness", Range(0.0, 1.0)) = 0.5
        _Metallic("Metallic", Range(0,1)) = 0.0
        
        [HideInInspector] _BumpMap0("Normal Maps 0", 2D) = "bump" {}
        [HideInInspector] _BumpMap1("Normal Maps 1", 2D) = "bump" {}
        
        _BumpScale("Normal Scale", Float) = 1.0
        
        [HideInInspector] _ParallaxMap0("Height Maps 0", 2D) = "grey" {}
        [HideInInspector] _ParallaxMap1("Height Maps 1", 2D) = "grey" {}
        
        _Parallax("Height Scale", Range(0.005, 0.08)) = 0.02
        
        [HideInInspector] _OcclusionMap0("Occlusion Maps 0", 2D) = "white" {}
        [HideInInspector] _OcclusionMap1("Occlusion Maps 1", 2D) = "white" {}
        
        _OcclusionStrength("Occlusion Strength", Range(0.0, 1.0)) = 1.0
        
        [HideInInspector] _EmissionMap0("Emission Maps 0", 2D) = "black" {}
        [HideInInspector] _EmissionMap1("Emission Maps 1", 2D) = "black" {}
        
        _EmissionColor("Emission Color", Color) = (0,0,0)
        
        [HideInInspector] _IndexTex("Albedo (RGB)", 2D) = "black" {}
        [HideInInspector] _IndexTex2("Albedo (RGB)", 2D) = "black" {}
        
        [Header(Texture Blending)]
        _Layer1Power("Layer 1 Power", Vector) = (1,0,0)
        _Layer2Power("Layer 2 Power", Vector) = (1,0,0)
        _Layer3Power("Layer 3 Power", Vector) = (1,0,0)
        _Layer4Power("Layer 4 Power", Vector) = (1,0,0)
        _Layer5Power("Layer 5 Power", Vector) = (1,0,0)
        _Layer6Power("Layer 6 Power", Vector) = (1,0,0)
        _Layer7Power("Layer 7 Power", Vector) = (1,0,0)
        _Layer8Power("Layer 8 Power", Vector) = (1,0,0)
        _Layer9Power("Layer 9 Power", Vector) = (1,0,0)
        _Layer10Power("Layer 10 Power", Vector) = (1, 0, 0)
        _Layer11Power("Layer 11 Power", Vector) = (1, 0, 0)
        _Layer12Power("Layer 12 Power", Vector) = (1, 0, 0)

        [MultiCompileOption(USEBASEMATERIAL)]
        USEBASEMATERIAL("Use base material", float) = 0
        [KeywordDependent(USEBASEMATERIAL)] _BaseSupression("Base Supression", float) = 1
        [KeywordDependent(USEBASEMATERIAL)] _BaseUVMultiplier("Base UV Multiplier", float) = 1
        [SingleLine(_BaseColor, USEBASEMATERIAL)] _BaseDiffuseMap("Base Diffuse Map", 2D) = "white" {}
        [HideInInspector] _BaseColor("Color", Color) = (1,1,1,1)
        [SingleLine(_BaseMetallic, USEBASEMATERIAL)] _BaseMetallicGlossMap("Base Metallic Maps", 2D) = "white" {}
        [KeywordDependent(USEBASEMATERIAL)]_BaseGlossiness("Smoothness", Range(0.0, 1.0)) = 0.5
        [HideInInspector] _BaseMetallic("Metallic", Range(0,1)) = 0.0
        [SingleLine(_BaseBumpScale, USEBASEMATERIAL)] _BaseBumpMap("Normal Maps", 2D) = "bump" {}
        [HideInInspector] _BaseBumpScale("Normal Scale", Float) = 1.0
        [SingleLine(_BaseParallax, USEBASEMATERIAL)]
        _BaseParallaxMap("Height Maps", 2D) = "grey" {}
        [HideInInspector]_BaseParallax("Height Scale", Range(0.005, 0.08)) = 0.02
        [SingleLine(_BaseOcclusionStrength, USEBASEMATERIAL)]
        _BaseOcclusionMap("Occlusion Maps", 2D) = "white" {}
        [HideInInspector]_BaseOcclusionStrength("Occlusion Strength", Range(0.0, 1.0)) = 1.0
        [SingleLine(_BaseEmissionColor, USEBASEMATERIAL)]
        _BaseEmissionMap("Emission Maps", 2D) = "black" {}
        [HideInInspector]_BaseEmissionColor("Emission Color", Color) = (0,0,0)
        
        [Header(Triplanar Settings)]
        _MapScale("Triplanar Scale", Float) = 1
        
        [MultiCompileOption(TESSELLATION)]
        TESSELLATION("Tessellation", float) = 0
        [KeywordDependent(TESSELLATION)] _TesselationPower("Power Tesselation", Range(-100, 100)) = 1
        [KeywordDependent(TESSELLATION)] _TesselationOffset("Offset Tesselation", Range(-100, 100)) = 1
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" }

        CGPROGRAM
        #pragma shader_feature USE_COLOR_FOR_SEAMLESS_TESSELLATION
        #pragma shader_feature TESSELLATION
        #pragma shader_feature USEBASEMATERIAL
       

        #pragma surface surf Standard fullforwardshadows vertex:disp addshadow   

        #pragma target 5.0

        #include "Tessellation.cginc"
        
        UNITY_DECLARE_TEX2D(_DiffuseMap0);
        UNITY_DECLARE_TEX2D_NOSAMPLER(_DiffuseMap1);

        UNITY_DECLARE_TEX2D(_MetallicGlossMap0);
        UNITY_DECLARE_TEX2D_NOSAMPLER(_MetallicGlossMap1);

        UNITY_DECLARE_TEX2D(_BumpMap0);
        UNITY_DECLARE_TEX2D_NOSAMPLER(_BumpMap1);

        UNITY_DECLARE_TEX2D(_ParallaxMap0);
        UNITY_DECLARE_TEX2D_NOSAMPLER(_ParallaxMap1);

        UNITY_DECLARE_TEX2D(_OcclusionMap0);
        UNITY_DECLARE_TEX2D_NOSAMPLER(_OcclusionMap1);       

        UNITY_DECLARE_TEX2D(_EmissionMap0);
        UNITY_DECLARE_TEX2D_NOSAMPLER(_EmissionMap1);
        
        float4 _Color_Layer_9;
        float4 _Color_Layer_10;
        float4 _Color_Layer_11;
        float4 _Color_Layer_12;
        float4 _Emission_Layer_9;
        float4 _Emission_Layer_10;
        float4 _Emission_Layer_11;
        float4 _Emission_Layer_12;

        float _Glossiness;
        float _Metallic;
        float4 _Color;
        float _BumpScale;
        float4 _EmissionColor;
        float _OcclusionStrength;
        float _Parallax;

#if USEBASEMATERIAL
        sampler2D _BaseDiffuseMap;
        sampler2D _BaseBumpMap;
        sampler2D _BaseParallaxMap;
        sampler2D _BaseMetallicGlossMap;
        sampler2D _BaseOcclusionMap;
        sampler2D _BaseEmissionMap;   
        half _BaseGlossiness;
        half _BaseMetallic;
        float4 _BaseColor;
        float _BaseBumpScale;
        float4 _BaseEmissionColor;
        float _BaseOcclusionStrength;
        float _BaseParallax;
        float _BaseSupression;
        float _BaseUVMultiplier;
#endif

        float4 _TransformTex;
      
        float3 _Layer1Power;
        float3 _Layer2Power;
        float3 _Layer3Power;
        float3 _Layer4Power;
        float3 _Layer5Power;
        float3 _Layer6Power;
        float3 _Layer7Power;
        float3 _Layer8Power;
        float3 _Layer9Power;
        float3 _Layer10Power;
        float3 _Layer11Power;
        float3 _Layer12Power;

        float _MapScale;

        struct appdata
        {
            float4 vertex    : POSITION;  // The vertex position in model space.
            float3 normal    : NORMAL;    // The vertex normal in model space.
            float4 texcoord  : TEXCOORD0; // The first UV coordinate.
            float4 texcoord1 : TEXCOORD1; // The second UV coordinate.
            float4 texcoord2 : TEXCOORD2; // The third UV coordinate.
            float4 texcoord3 : TEXCOORD3; // The fourfth UV coordinate.
            float4 texcoord4 : TEXCOORD4; // The fifth UV coordinate. // requires Unity 2018.2+
            float4 texcoord5 : TEXCOORD5; // The sixth UV coordinate. // requires Unity 2018.2+          
            float4 tangent   : TANGENT;   // The tangent vector in Model Space (used for normal mapping).
            float4 color     : COLOR;     // Per-vertex color
        };

        struct Input
        {
            float3 worldPos;
            float3 worldNormal;
            float2 uv_DiffuseMap0;
            float4 texcoord2;
            float4 texcoord3;
            float4 texcoord4;
            float4 texcoord5;
            float3 viewDir;        
            INTERNAL_DATA
        };

        float _Tess;
        float _TesselationPower;
        float _Tess_MinEdgeDistance;
        float _TesselationOffset;
        float minDist;
        float maxDist;

        UNITY_INSTANCING_BUFFER_START(Props)
        UNITY_INSTANCING_BUFFER_END(Props)



#pragma shader_feature SEAMLESS_TESSELATION    
        void disp(inout appdata v, out Input o)
        {       
            UNITY_INITIALIZE_OUTPUT(Input, o);
            o.texcoord2 = v.texcoord2;
            o.texcoord3 = v.texcoord3;
            o.texcoord4 = v.texcoord4;
            o.texcoord5 = v.texcoord5;

#if !TESSELLATION
            return;
#endif
              
            float2 uv = v.texcoord.xy * _MapScale;

            float3 position = v.vertex.xyz * _MapScale;
            float3 normal = v.normal;  
                 
            float layer1factor = max(v.texcoord2.x * _Layer1Power.x, 0);
            float layer2factor = max(v.texcoord2.y * _Layer2Power.x, 0);
            float layer3factor = max(v.texcoord2.z * _Layer3Power.x, 0);
            float layer4factor = max(v.texcoord2.w * _Layer4Power.x, 0);
            float layer5factor = max(v.texcoord3.x * _Layer5Power.x, 0);
            float layer6factor = max(v.texcoord3.y * _Layer6Power.x, 0);
            float layer7factor = max(v.texcoord3.z * _Layer7Power.x, 0);
            float layer8factor = max(v.texcoord3.w * _Layer8Power.x, 0);
            float layer9factor = max(v.texcoord4.x * _Layer9Power.x, 0);
            float sumPower = (layer1factor + layer2factor + layer3factor + layer4factor);
            sumPower += (layer5factor + layer6factor + layer7factor + layer8factor);

#if USEBASEMATERIAL
            float layer0factor = _BaseSupression;
            sumPower += layer0factor;
            layer0factor /= sumPower;
            float base = saturate(tex2Dlod(_BaseParallaxMap, float4((uv), 0, 0)).r * layer0factor);
#endif

            layer1factor /= sumPower;
            layer2factor /= sumPower;
            layer3factor /= sumPower;
            layer4factor /= sumPower;
            layer5factor /= sumPower;
            layer6factor /= sumPower;
            layer7factor /= sumPower;
            layer8factor /= sumPower;

            float layer_1 = saturate(UNITY_SAMPLE_TEX2D_LOD(_ParallaxMap0, uv, 0).r * layer1factor);
            float layer_2 = saturate(UNITY_SAMPLE_TEX2D_SAMPLER_LOD(_ParallaxMap1, _ParallaxMap0, uv, 0).r * layer2factor);
                
            float displacement = layer_1 + layer_2;

#if USEBASEMATERIAL
            displacement += base;
#endif

            displacement *= _TesselationPower;
            displacement += _TesselationOffset;


#if USE_COLOR_FOR_SEAMLESS_TESSELLATION
            float factor = min(_Tess_MinEdgeDistance, min(v.color.r, min(v.color.g, v.color.b)));
            displacement *= factor;
#endif     
            
            v.vertex.xyz += normal * displacement;
        }

        struct Result {
            float4  Diffuse;
            half3   Normal;
            float4  Metallic;
            float   Smoothness;
            float4  Occlusion;
            float4  Emission;
        };

        Result MergeResults(Result result1, Result result2)
        {
            Result output;

            output.Diffuse.rgb = saturate(result1.Diffuse.rgb + result2.Diffuse.rgb);
            output.Normal = saturate(result1.Normal + result2.Normal);
            output.Metallic = saturate(result1.Metallic + result2.Metallic);
            output.Smoothness = saturate(result1.Smoothness + result2.Smoothness);
            output.Occlusion = saturate(result1.Occlusion + result2.Occlusion);
            output.Emission = saturate(result1.Emission + result2.Emission);
            output.Diffuse.a = saturate(result1.Diffuse.a + result2.Diffuse.a);

            return output;
        }


        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            float2 uv = float2(IN.uv_DiffuseMap0.x, IN.uv_DiffuseMap0.y) * _MapScale;
            float4 result;
                             
            float3 position = mul(unity_WorldToObject, float4(IN.worldPos, 1)) * _MapScale;
            float3 normal = WorldNormalVector(IN, o.Normal);
            float3 localNormal = mul(unity_WorldToObject, float4(normal, 0));           

            // Extract the 8-bit values for each layer from packedLayers
            float layer1factor = max(IN.texcoord2.x, 0);
            float layer2factor = max(IN.texcoord2.y, 0);
            float layer3factor = max(IN.texcoord2.z, 0);
            float layer4factor = max(IN.texcoord2.w, 0);
            float layer5factor = max(IN.texcoord3.x, 0);
            float layer6factor = max(IN.texcoord3.y, 0);
            float layer7factor = max(IN.texcoord3.z, 0);
            float layer8factor = max(IN.texcoord3.w, 0);
            float layer9factor = max(IN.texcoord4.x, 0);
            float layer10factor = max(IN.texcoord4.y, 0);
            float layer11factor = max(IN.texcoord4.z, 0);
            float layer12factor = max(IN.texcoord4.w, 0);


            // Apply the layer powers as before
            layer1factor = max(layer1factor * _Layer1Power.x, 0);          
            layer9factor = max(layer9factor * _Layer9Power.x, 0);
            layer10factor = max(layer10factor * _Layer10Power.x, 0);
            layer11factor = max(layer11factor * _Layer11Power.x, 0);
            layer12factor = max(layer12factor * _Layer12Power.x, 0);

            float sumPower = (layer1factor + layer9factor + layer10factor + layer11factor + layer12factor);

#if USEBASEMATERIAL
            float baseLayerFactor = _BaseSupression;
            sumPower += baseLayerFactor;
            baseLayerFactor /= sumPower;
#endif

            layer1factor /= sumPower;        
            layer9factor /= sumPower;
            layer10factor /= sumPower;
            layer11factor /= sumPower;
            layer12factor /= sumPower;

            Result layeruv0; 
            Result layeruv1;
            Result layeruv2;
            Result layeruv3;
            Result layeruv4;
            Result layeruv5;
            Result layeruv6;
            Result layeruv7;

            float4 parallax, metallicGloss;
            float2 uv_parallax;

            Result baseLayer;
            baseLayer.Diffuse = float4(0, 0, 0, 0);
            baseLayer.Normal = UnpackScaleNormal(0, 0);
            baseLayer.Metallic = float4(0, 0, 0, 0);
            baseLayer.Smoothness = 0;
            baseLayer.Occlusion = float4(0, 0, 0, 0);
            baseLayer.Emission = float4(0, 0, 0, 0);

#if USEBASEMATERIAL
            // Base Layer (using baseLayerFactor)
            parallax = tex2D(_BaseParallaxMap, uv) * baseLayerFactor;
            uv_parallax = uv + ParallaxOffset(parallax.w, _BaseParallax, IN.viewDir);

            baseLayer.Diffuse = tex2D(_BaseDiffuseMap, uv_parallax) * baseLayerFactor * _BaseColor;
            baseLayer.Normal = UnpackScaleNormal(tex2D(_BaseBumpMap, uv_parallax), _BaseBumpScale * baseLayerFactor);
            float4 basemetallicGloss = tex2D(_BaseMetallicGlossMap, uv_parallax);
            baseLayer.Metallic = basemetallicGloss * _BaseMetallic * baseLayerFactor;
            baseLayer.Smoothness = basemetallicGloss.a * _BaseGlossiness * baseLayerFactor;
            baseLayer.Occlusion = tex2D(_BaseOcclusionMap, uv_parallax) * _BaseOcclusionStrength * baseLayerFactor;
            baseLayer.Emission = tex2D(_BaseEmissionMap, uv_parallax) * _BaseEmissionColor * baseLayerFactor;
#endif

            // Layer 1
            parallax = UNITY_SAMPLE_TEX2D(_ParallaxMap0, uv) * layer1factor;
            uv_parallax = uv + ParallaxOffset(parallax.w, _Parallax, IN.viewDir);
            layeruv0.Diffuse = UNITY_SAMPLE_TEX2D(_DiffuseMap0, uv_parallax) * layer1factor;
            layeruv0.Normal = UnpackScaleNormal(UNITY_SAMPLE_TEX2D(_BumpMap0, uv_parallax), _BumpScale * layer1factor);
            metallicGloss = UNITY_SAMPLE_TEX2D(_MetallicGlossMap0, uv_parallax) * layer1factor;
            layeruv0.Metallic = metallicGloss;
            layeruv0.Smoothness = metallicGloss.a;
            layeruv0.Occlusion = UNITY_SAMPLE_TEX2D(_OcclusionMap0, uv_parallax) * layer1factor;
            layeruv0.Emission = UNITY_SAMPLE_TEX2D(_EmissionMap0, uv_parallax) * layer1factor;          

            // Merging the results
            Result merged = layeruv0;

            //Color Layers:
            Result colorlayeruv8;
            colorlayeruv8.Diffuse = _Color_Layer_9 * layer9factor;
            colorlayeruv8.Normal = UnpackScaleNormal(1, _BumpScale * layer9factor);
            colorlayeruv8.Metallic = float4(1,1,1,1) * layer9factor;
            colorlayeruv8.Smoothness = 1 * layer9factor;
            colorlayeruv8.Occlusion = float4(1, 1, 1, 1) * layer9factor;
            colorlayeruv8.Emission = _Emission_Layer_9 * layer9factor;
            merged = MergeResults(merged, colorlayeruv8);

            // Layer 9
            Result colorlayeruv9;
            colorlayeruv9.Diffuse = _Color_Layer_10 * layer10factor;
            colorlayeruv9.Normal = UnpackScaleNormal(1, _BumpScale * layer10factor);
            colorlayeruv9.Metallic = float4(1, 1, 1, 1) * layer10factor;
            colorlayeruv9.Smoothness = 1 * layer10factor;
            colorlayeruv9.Occlusion = float4(1, 1, 1, 1) * layer10factor;
            colorlayeruv9.Emission = _Emission_Layer_10 * layer10factor;
            merged = MergeResults(merged, colorlayeruv9);

            // Layer 10
            Result colorlayeruv10;
            colorlayeruv10.Diffuse = _Color_Layer_11 * layer11factor;
            colorlayeruv10.Normal = UnpackScaleNormal(1, _BumpScale * layer11factor);
            colorlayeruv10.Metallic = float4(1, 1, 1, 1) * layer11factor;
            colorlayeruv10.Smoothness = 1 * layer11factor;
            colorlayeruv10.Occlusion = float4(1, 1, 1, 1) * layer11factor;
            colorlayeruv10.Emission = _Emission_Layer_11 * layer11factor;
            merged = MergeResults(merged, colorlayeruv10);

            // Layer 11
            Result colorlayeruv11;
            colorlayeruv11.Diffuse = _Color_Layer_12 * layer12factor;
            colorlayeruv11.Normal = UnpackScaleNormal(1, _BumpScale * layer12factor);
            colorlayeruv11.Metallic = float4(1, 1, 1, 1) * layer12factor;
            colorlayeruv11.Smoothness = 1 * layer12factor;
            colorlayeruv11.Occlusion = float4(1, 1, 1, 1) * layer12factor;
            colorlayeruv11.Emission = _Emission_Layer_12 * layer12factor;
            merged = MergeResults(merged, colorlayeruv11);

            merged = MergeResults(baseLayer, merged);
     
            o.Albedo = (merged.Diffuse.rgb) * _Color.rgb;
            o.Normal = merged.Normal;
            o.Metallic = (merged.Metallic) * _Metallic;
            o.Smoothness = (merged.Smoothness) * _Glossiness;
            o.Occlusion = (merged.Occlusion) * _OcclusionStrength;
            o.Emission = merged.Emission * _EmissionColor;
            o.Alpha = (merged.Diffuse.a) * _Color.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
