Shader "Fraktalia/Core/MultiTexture_V2/Texture2D_UVBlend_4Layers"
{
    Properties
    {
        _TransformTex("Texture Transform", Vector) = (0, 0, 1, 1)

        [Header(Main Maps)]    
        [LayerMaps(Layer 0, _DiffuseMap0, _MetallicGlossMap0, _BumpMap0, _ParallaxMap0, _OcclusionMap0, _EmissionMap0)]
        _DiffuseMap0("Diffuse (Albedo) Maps 0", 2D) = "white" {}
        [LayerMaps(Layer 1, _DiffuseMap1, _MetallicGlossMap1, _BumpMap1, _ParallaxMap1, _OcclusionMap1, _EmissionMap1)]
        _DiffuseMap1("Diffuse (Albedo) Maps 1", 2D) = "white" {}
        [LayerMaps(Layer 2, _DiffuseMap2, _MetallicGlossMap2, _BumpMap2, _ParallaxMap2, _OcclusionMap2, _EmissionMap2)]
        _DiffuseMap2("Diffuse (Albedo) Maps 2", 2D) = "white" {}
        [LayerMaps(Layer 3, _DiffuseMap3, _MetallicGlossMap3, _BumpMap3, _ParallaxMap3, _OcclusionMap3, _EmissionMap3)]
        _DiffuseMap3("Diffuse (Albedo) Maps 3", 2D) = "white" {}      
        _Color("Diffuse Color", Color) = (1,1,1,1)
       
        [HideInInspector] _MetallicGlossMap0("Metallic Maps 0", 2D) = "white" {}
        [HideInInspector] _MetallicGlossMap1("Metallic Maps 1", 2D) = "white" {}
        [HideInInspector] _MetallicGlossMap2("Metallic Maps 2", 2D) = "white" {}
        [HideInInspector] _MetallicGlossMap3("Metallic Maps 3", 2D) = "white" {}    
        _Glossiness("Smoothness", Range(0.0, 1.0)) = 0.5 
        _Metallic("Metallic", Range(0,1)) = 0.0
        
        [HideInInspector] _BumpMap0("Normal Maps 0", 2D) = "bump" {}
        [HideInInspector] _BumpMap1("Normal Maps 1", 2D) = "bump" {}
        [HideInInspector] _BumpMap2("Normal Maps 2", 2D) = "bump" {}
        [HideInInspector] _BumpMap3("Normal Maps 3", 2D) = "bump" {}
        _BumpScale("Normal Scale", Float) = 1.0
            
        [HideInInspector]_ParallaxMap0("Height Maps 0", 2D) = "grey" {}
        [HideInInspector]_ParallaxMap1("Height Maps 1", 2D) = "grey" {}
        [HideInInspector]_ParallaxMap2("Height Maps 2", 2D) = "grey" {}
        [HideInInspector]_ParallaxMap3("Height Maps 3", 2D) = "grey" {}
        _Parallax("Height Scale", Range(0.005, 0.08)) = 0.02
        
        [HideInInspector] _OcclusionMap0("Occlusion Maps 0", 2D) = "white" {}
        [HideInInspector] _OcclusionMap1("Occlusion Maps 1", 2D) = "white" {}
        [HideInInspector] _OcclusionMap2("Occlusion Maps 2", 2D) = "white" {}
        [HideInInspector] _OcclusionMap3("Occlusion Maps 3", 2D) = "white" {}    
        _OcclusionStrength("Occlusion Strength", Range(0.0, 1.0)) = 1.0
           
        [HideInInspector] _EmissionMap0("Emission Maps 0", 2D) = "black" {} 
        [HideInInspector] _EmissionMap1("Emission Maps 1", 2D) = "black" {}
        [HideInInspector] _EmissionMap2("Emission Maps 2", 2D) = "black" {}
        [HideInInspector] _EmissionMap3("Emission Maps 3", 2D) = "black" {}
        _EmissionColor("Emission Color", Color) = (0,0,0)
    
        [HideInInspector]_IndexTex("Albedo (RGB)", 2D) = "black" {}
        [HideInInspector]_IndexTex2("Albedo (RGB)", 2D) = "black" {}

        [Header(Texture Blending)]
        _Layer0Power("Layer 0 Power, Shift, Slice", Vector) = (0.025,0,0)
        _Layer1Power("Layer 1 Power, Shift, Slice", Vector) = (0,0,1)
        _Layer2Power("Layer 2 Power, Shift, Slice", Vector) = (0,0,2)
        _Layer3Power("Layer 3 Power, Shift, Slice", Vector) = (0,0,3)
        
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
       
        [KeywordDependent(TESSELLATION)] _Tess("Tessellation", Range(1,32)) = 4
        [KeywordDependent(TESSELLATION)] minDist("Tess Min Distance", float) = 10
        [KeywordDependent(TESSELLATION)] maxDist("Tess Max Distance", float) = 25
        [KeywordDependent(TESSELLATION)] _TesselationPower("Power Tesselation", Range(-100, 100)) = 1
        [KeywordDependent(TESSELLATION)] _TesselationOffset("Offset Tesselation", Range(-100, 100)) = 1

        [MultiCompileToggle(USE_COLOR_FOR_SEAMLESS_TESSELLATION, TESSELLATION)]
        USE_COLOR_FOR_SEAMLESS_TESSELLATION("Tessellation", float) = 0
        [KeywordDependent(TESSELLATION)] _Tess_MinEdgeDistance("MinEdge Tesselation", Range(0, 1)) = 1       
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" }

        CGPROGRAM
        #pragma shader_feature USE_COLOR_FOR_SEAMLESS_TESSELLATION
        #pragma shader_feature TESSELLATION
        #pragma shader_feature USEBASEMATERIAL
       
#if TESSELLATION
        #pragma surface surf Standard fullforwardshadows vertex:disp tessellate:tessDistance addshadow   
#else
        #pragma surface surf Standard fullforwardshadows
#endif

        #pragma target 5.0

        #include "Tessellation.cginc"
        UNITY_DECLARE_TEX2D(_DiffuseMap0);
        UNITY_DECLARE_TEX2D_NOSAMPLER(_DiffuseMap1);
        UNITY_DECLARE_TEX2D_NOSAMPLER(_DiffuseMap2);
        UNITY_DECLARE_TEX2D_NOSAMPLER(_DiffuseMap3);     

        UNITY_DECLARE_TEX2D(_MetallicGlossMap0);
        UNITY_DECLARE_TEX2D_NOSAMPLER(_MetallicGlossMap1);
        UNITY_DECLARE_TEX2D_NOSAMPLER(_MetallicGlossMap2);
        UNITY_DECLARE_TEX2D_NOSAMPLER(_MetallicGlossMap3);

        UNITY_DECLARE_TEX2D(_BumpMap0);
        UNITY_DECLARE_TEX2D_NOSAMPLER(_BumpMap1);
        UNITY_DECLARE_TEX2D_NOSAMPLER(_BumpMap2);
        UNITY_DECLARE_TEX2D_NOSAMPLER(_BumpMap3);

        UNITY_DECLARE_TEX2D(_ParallaxMap0);
        UNITY_DECLARE_TEX2D_NOSAMPLER(_ParallaxMap1);
        UNITY_DECLARE_TEX2D_NOSAMPLER(_ParallaxMap2);
        UNITY_DECLARE_TEX2D_NOSAMPLER(_ParallaxMap3);

        UNITY_DECLARE_TEX2D(_OcclusionMap0);
        UNITY_DECLARE_TEX2D_NOSAMPLER(_OcclusionMap1);
        UNITY_DECLARE_TEX2D_NOSAMPLER(_OcclusionMap2);
        UNITY_DECLARE_TEX2D_NOSAMPLER(_OcclusionMap3);

        UNITY_DECLARE_TEX2D(_EmissionMap0);
        UNITY_DECLARE_TEX2D_NOSAMPLER(_EmissionMap1);
        UNITY_DECLARE_TEX2D_NOSAMPLER(_EmissionMap2);
        UNITY_DECLARE_TEX2D_NOSAMPLER(_EmissionMap3);

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
        sampler2D _IndexTex;
        sampler2D _IndexTex2;      
        float3 _Layer0Power;
        float3 _Layer1Power;
        float3 _Layer2Power;
        float3 _Layer3Power;
          
        float _MapScale;
        struct Input
        {
            float3 worldPos;
            float3 worldNormal;
            float2 uv_DiffuseMap0;
            float2 uv3_IndexTex;
            float2 uv4_IndexTex2; //2 at end to match defined _IndexTex2 to prevent redefinition
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

        float4 tessDistance(appdata_full v0, appdata_full v1, appdata_full v2) {        
#if !TESSELLATION
            return 1;
#endif
            return UnityDistanceBasedTess(v0.vertex, v1.vertex, v2.vertex, minDist, maxDist, _Tess);
        }

        void disp(inout appdata_full v)
        {           
#if !TESSELLATION
            return;
#endif
              
            float2 uv = v.texcoord.xy * _MapScale;

            float3 position = v.vertex.xyz * _MapScale;
            float3 normal = v.normal;  
            
            float2 uv3_IndexTex = float2(v.texcoord2.x, v.texcoord2.y);
            float2 uv4_IndexTex = float2(v.texcoord3.x, v.texcoord3.y);   

            float layer1factor = max(uv3_IndexTex.x * _Layer0Power.x + _Layer0Power.y, 0);
            float layer2factor = max(uv3_IndexTex.y * _Layer1Power.x + _Layer1Power.y, 0);
            float layer3factor = max(uv4_IndexTex.x * _Layer2Power.x + _Layer2Power.y, 0);
            float layer4factor = max(uv4_IndexTex.y * _Layer3Power.x + _Layer3Power.y, 0);
            float sumPower = (layer1factor + layer2factor + layer3factor + layer4factor);

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

            float layeruv3 = saturate(UNITY_SAMPLE_TEX2D_LOD(_ParallaxMap0, uv, 0).r * layer1factor);
            float layeruv4 = saturate(UNITY_SAMPLE_TEX2D_SAMPLER_LOD(_ParallaxMap1, _ParallaxMap0 , uv, 0).r * layer2factor);
            float layeruv5 = saturate(UNITY_SAMPLE_TEX2D_SAMPLER_LOD(_ParallaxMap2, _ParallaxMap0 , uv, 0).r * layer3factor);
            float layeruv6 = saturate(UNITY_SAMPLE_TEX2D_SAMPLER_LOD(_ParallaxMap3, _ParallaxMap0 , uv, 0).r * layer4factor);
            float displacement = layeruv3 + layeruv4 + layeruv5 + layeruv6;

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
            float layer1factor = max(IN.uv3_IndexTex.x * _Layer0Power.x + _Layer0Power.y, 0);
            float layer2factor = max(IN.uv3_IndexTex.y * _Layer1Power.x + _Layer1Power.y, 0);
            float layer3factor = max(IN.uv4_IndexTex2.x * _Layer2Power.x + _Layer2Power.y, 0);
            float layer4factor = max(IN.uv4_IndexTex2.y * _Layer3Power.x + _Layer3Power.y, 0);

            // Apply the layer powers as before
            layer1factor = max(layer1factor * _Layer0Power.x + _Layer0Power.y, 0);
            layer2factor = max(layer2factor * _Layer1Power.x + _Layer1Power.y, 0);
            layer3factor = max(layer3factor * _Layer2Power.x + _Layer2Power.y, 0);
            layer4factor = max(layer4factor * _Layer3Power.x + _Layer3Power.y, 0);
            
            float sumPower = (layer1factor + layer2factor + layer3factor + layer4factor);
           
           

#if USEBASEMATERIAL
            float baseLayerFactor = _BaseSupression;
            sumPower += baseLayerFactor;
            baseLayerFactor /= sumPower;
#endif

            layer1factor /= sumPower;
            layer2factor /= sumPower;
            layer3factor /= sumPower;
            layer4factor /= sumPower;

            Result layeruv3;
            Result layeruv4;
            Result layeruv5;
            Result layeruv6;

            float4 parallax, metallicGloss;
            float2 uv_parallax;

            Result baseLayer;
            baseLayer.Diffuse = float4(0,0,0,0);
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
            baseLayer.Smoothness = basemetallicGloss.a* _BaseGlossiness * baseLayerFactor;
            baseLayer.Occlusion = tex2D(_BaseOcclusionMap, uv_parallax) * _BaseOcclusionStrength * baseLayerFactor;
            baseLayer.Emission = tex2D(_BaseEmissionMap, uv_parallax)* _BaseEmissionColor* baseLayerFactor;
#endif

            // Layer 1
            parallax = UNITY_SAMPLE_TEX2D(_ParallaxMap0, uv) * layer1factor;
            uv_parallax = uv + ParallaxOffset(parallax.w, _Parallax, IN.viewDir);
            layeruv3.Diffuse = UNITY_SAMPLE_TEX2D(_DiffuseMap0, uv_parallax) * layer1factor;
            layeruv3.Normal = UnpackScaleNormal(UNITY_SAMPLE_TEX2D(_BumpMap0, uv_parallax), _BumpScale * layer1factor);
            metallicGloss = UNITY_SAMPLE_TEX2D(_MetallicGlossMap0, uv_parallax) * layer1factor;
            layeruv3.Metallic = metallicGloss;
            layeruv3.Smoothness = metallicGloss.a;
            layeruv3.Occlusion = UNITY_SAMPLE_TEX2D(_OcclusionMap0, uv_parallax) * layer1factor;
            layeruv3.Emission = UNITY_SAMPLE_TEX2D(_EmissionMap0, uv_parallax) * layer1factor;

            // Layer 2
            parallax = UNITY_SAMPLE_TEX2D_SAMPLER(_ParallaxMap1, _ParallaxMap0, uv) * layer2factor;
            uv_parallax = uv + ParallaxOffset(parallax.w, _Parallax, IN.viewDir);
            layeruv4.Diffuse = UNITY_SAMPLE_TEX2D_SAMPLER(_DiffuseMap1, _DiffuseMap0, uv_parallax) * layer2factor;
            layeruv4.Normal = UnpackScaleNormal(UNITY_SAMPLE_TEX2D_SAMPLER(_BumpMap1, _BumpMap0, uv_parallax), _BumpScale * layer2factor);
            metallicGloss = UNITY_SAMPLE_TEX2D_SAMPLER(_MetallicGlossMap1, _MetallicGlossMap0, uv_parallax) * layer2factor;
            layeruv4.Metallic = metallicGloss;
            layeruv4.Smoothness = metallicGloss.a;
            layeruv4.Occlusion = UNITY_SAMPLE_TEX2D_SAMPLER(_OcclusionMap1, _OcclusionMap0, uv_parallax) * layer2factor;
            layeruv4.Emission = UNITY_SAMPLE_TEX2D_SAMPLER(_EmissionMap1, _EmissionMap0, uv_parallax) * layer2factor;

            // Layer 3
            parallax = UNITY_SAMPLE_TEX2D_SAMPLER(_ParallaxMap2, _ParallaxMap0, uv) * layer3factor;
            uv_parallax = uv + ParallaxOffset(parallax.w, _Parallax, IN.viewDir);
            layeruv5.Diffuse = UNITY_SAMPLE_TEX2D_SAMPLER(_DiffuseMap2, _DiffuseMap0, uv_parallax) * layer3factor;
            layeruv5.Normal = UnpackScaleNormal(UNITY_SAMPLE_TEX2D_SAMPLER(_BumpMap2, _BumpMap0, uv_parallax), _BumpScale * layer3factor);
            metallicGloss = UNITY_SAMPLE_TEX2D_SAMPLER(_MetallicGlossMap2, _MetallicGlossMap0, uv_parallax) * layer3factor;
            layeruv5.Metallic = metallicGloss;
            layeruv5.Smoothness = metallicGloss.a;
            layeruv5.Occlusion = UNITY_SAMPLE_TEX2D_SAMPLER(_OcclusionMap2, _OcclusionMap0, uv_parallax) * layer3factor;
            layeruv5.Emission = UNITY_SAMPLE_TEX2D_SAMPLER(_EmissionMap2, _EmissionMap0, uv_parallax) * layer3factor;

            // Layer 4
            parallax = UNITY_SAMPLE_TEX2D_SAMPLER(_ParallaxMap3, _ParallaxMap0, uv) * layer4factor;
            uv_parallax = uv + ParallaxOffset(parallax.w, _Parallax, IN.viewDir);
            layeruv6.Diffuse = UNITY_SAMPLE_TEX2D_SAMPLER(_DiffuseMap3, _DiffuseMap0, uv_parallax) * layer4factor;
            layeruv6.Normal = UnpackScaleNormal(UNITY_SAMPLE_TEX2D_SAMPLER(_BumpMap3, _BumpMap0, uv_parallax), _BumpScale * layer4factor);
            metallicGloss = UNITY_SAMPLE_TEX2D_SAMPLER(_MetallicGlossMap3, _MetallicGlossMap0, uv_parallax) * layer4factor;
            layeruv6.Metallic = metallicGloss;
            layeruv6.Smoothness = metallicGloss.a;
            layeruv6.Occlusion = UNITY_SAMPLE_TEX2D_SAMPLER(_OcclusionMap3, _OcclusionMap0, uv_parallax) * layer4factor;
            layeruv6.Emission = UNITY_SAMPLE_TEX2D_SAMPLER(_EmissionMap3, _EmissionMap0, uv_parallax) * layer4factor;

            Result merged = MergeResults(MergeResults(MergeResults(layeruv3, layeruv4), layeruv5), layeruv6);  

            merged = MergeResults(baseLayer, merged);         
  

            o.Albedo = (merged.Diffuse.rgb) * _Color.rgb;
            o.Normal = merged.Normal;
            o.Metallic =  (merged.Metallic) * _Metallic;
            o.Smoothness = (merged.Smoothness)* _Glossiness;
            o.Occlusion = (merged.Occlusion) * _OcclusionStrength;
            o.Emission = merged.Emission * _EmissionColor;
            o.Alpha = (merged.Diffuse.a) * _Color.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
