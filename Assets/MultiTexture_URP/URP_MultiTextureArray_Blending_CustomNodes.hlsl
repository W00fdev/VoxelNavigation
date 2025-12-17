void CalculateBlendFactors_float(float4 UV2, float4 UV3, float basefactor, float baseExponent, out float4 BlendFactorsUV2, out float4 BlendFactorsUV3, out float baseLayerFactor)
{ 
    float layer1factor = max(UV2.x, 0);
    float layer2factor = max(UV2.y, 0);
    float layer3factor = max(UV2.z, 0);
    float layer4factor = max(UV2.w, 0);
    float layer5factor = max(UV3.x, 0);
    float layer6factor = max(UV3.y, 0);
    float layer7factor = max(UV3.z, 0);
    float layer8factor = max(UV3.w, 0);

    float sumPower = max(0.000001, layer1factor + layer2factor + layer3factor + layer4factor + layer5factor + layer6factor + layer7factor + layer8factor);
           
    layer1factor /= sumPower;
    layer2factor /= sumPower;
    layer3factor /= sumPower;
    layer4factor /= sumPower;
    layer5factor /= sumPower;
    layer6factor /= sumPower;
    layer7factor /= sumPower;
    layer8factor /= sumPower;
   
    float base = basefactor;
    sumPower += base;
    base /= sumPower;
    base = clamp(pow(base, baseExponent), 0, 1);
   
    BlendFactorsUV2 = float4(layer1factor, layer2factor, layer3factor, layer4factor);
    BlendFactorsUV3 = float4(layer5factor, layer6factor, layer7factor, layer8factor); 
    
    baseLayerFactor = base;
}

void ParallaxOffset_float(float h, float height, float3 viewDir, out float2 result)
{
    h = h * height - height / 2.0;
    float3 v = normalize(viewDir);
    v.z += 0.42;
    result = h * (v.xy / v.z); 
}

void ParallaxLayer_float(UnityTexture2D BaseHeightMap, UnityTexture2DArray HeightMap, float2 UV, UnitySamplerState Sampler, float4 blendFactorsUV2, float4 blendFactorsUV3, float baseLayerFactor, out float outHeight)
{
    float resultHeight = SAMPLE_TEXTURE2D_LOD(BaseHeightMap, Sampler, UV, 0).r * baseLayerFactor;

    // Sample the first 4 layers and blend using blendFactorsUV2
    for (int i = 0; i < 4; i++)
    {
        // Sample height map and blend
        float height = SAMPLE_TEXTURE2D_ARRAY_LOD(HeightMap, Sampler, UV, i, 0).r * blendFactorsUV2[i];
        resultHeight += height;
    }

    // Sample the next 4 layers and blend using blendFactorsUV3
    for (int k = 0; k < 4; k++)
    {
        // Sample height map and blend
        float height = SAMPLE_TEXTURE2D_ARRAY_LOD(HeightMap, Sampler, UV, k + 4, 0).r * blendFactorsUV3[k];
        resultHeight += height;
    }

    // Output the blended height result
    outHeight = resultHeight;
}

void UnpackNormal_float(float4 packedNormal, float bumpPower, out float3 outNormal)
{
    // Unpack the normal from the RGB channels of the texture array (DXT5 format)
    float3 normal;

    // Normals in standard DXT5 format are stored in the RGB channels
    normal.xyz = packedNormal.rgb * 2.0 - 1.0;

    // Output the unpacked normal
    outNormal = normalize(normal);
}

void TextureArrayBlendLayers_Normal_float(UnityTexture2D BaseNormal, UnityTexture2DArray Normal, float2 UV, UnitySamplerState Sampler, float4 blendFactorsUV2, float4 blendFactorsUV3, float baseLayerFactor,
out float3 outNormal)
{
    float4 basenormal = SAMPLE_TEXTURE2D(BaseNormal, Sampler, UV);
    float3 resultNormal = UnpackNormalmapRGorAG(basenormal) * baseLayerFactor;
   
    for (int i = 0; i < 4; i++)
    {
        float4 normal = SAMPLE_TEXTURE2D_ARRAY(Normal, Sampler, UV, i) ;
        resultNormal += UnpackNormalmapRGorAG(normal) * blendFactorsUV2[i];
    }

    for (int k = 0; k < 4; k++)
    {        
        // Sample and blend normal textures
        float4 normal = SAMPLE_TEXTURE2D_ARRAY(Normal, Sampler, UV, k + 4);
        resultNormal += UnpackNormalmapRGorAG(normal) * blendFactorsUV3[k];
    }
    
    outNormal = normalize(resultNormal);
}



void TextureArrayBlendLayers_float(UnityTexture2D BaseDiffuse, UnityTexture2D BaseMetallic, UnityTexture2D BaseEmission, UnityTexture2D BaseAmbientOcclusion,
UnityTexture2DArray Diffuse, UnityTexture2DArray MetallicGloss, UnityTexture2DArray Emission, UnityTexture2DArray AmbientOcclusion,
float2 UV, UnitySamplerState Sampler, float4 blendFactorsUV2, float4 blendFactorsUV3, float baseLayerFactor,
out float4 outDiffuse, out float4 outMetallicGlossiness, out float3 outEmission, out float outAmbientOcclusion)
{
    float4 resultDiffuse = SAMPLE_TEXTURE2D(BaseDiffuse, Sampler, UV) * baseLayerFactor;
    float4 resultMetallicGloss = SAMPLE_TEXTURE2D(BaseMetallic, Sampler, UV) * baseLayerFactor;
    float3 resultEmission = SAMPLE_TEXTURE2D(BaseEmission, Sampler, UV).rgb * baseLayerFactor;
    float resultAmbientOcclusion = SAMPLE_TEXTURE2D(BaseAmbientOcclusion, Sampler, UV).r * baseLayerFactor;

    for (int i = 0; i < 4; i++)
    {
        // Sample and blend diffuse textures
        float4 diffuse = SAMPLE_TEXTURE2D_ARRAY(Diffuse, Sampler, UV, i) * blendFactorsUV2[i];
        resultDiffuse += diffuse;
            
        // Sample and blend metallic gloss textures
        float4 metallicGloss = SAMPLE_TEXTURE2D_ARRAY(MetallicGloss, Sampler, UV, i) * blendFactorsUV2[i];
        resultMetallicGloss += metallicGloss;

        // Sample and blend emission textures
        float3 emission = SAMPLE_TEXTURE2D_ARRAY(Emission, Sampler, UV, i).rgb * blendFactorsUV2[i];
        resultEmission += emission;

        // Sample and blend ambient occlusion textures
        float ambientOcclusion = SAMPLE_TEXTURE2D_ARRAY(AmbientOcclusion, Sampler, UV, i).r * blendFactorsUV2[i];
        resultAmbientOcclusion += ambientOcclusion;
    }

    for (int k = 0; k < 4; k++)
    {
        // Sample and blend diffuse textures
        float4 diffuse = SAMPLE_TEXTURE2D_ARRAY(Diffuse, Sampler, UV, k + 4) * blendFactorsUV3[k];
        resultDiffuse += diffuse;
    
        // Sample and blend metallic gloss textures
        float4 metallicGloss = SAMPLE_TEXTURE2D_ARRAY(MetallicGloss, Sampler, UV, k + 4) * blendFactorsUV3[k];
        resultMetallicGloss += metallicGloss;

        // Sample and blend emission textures
        float3 emission = SAMPLE_TEXTURE2D_ARRAY(Emission, Sampler, UV, k + 4).rgb * blendFactorsUV3[k];
        resultEmission += emission;

        // Sample and blend ambient occlusion textures
        float ambientOcclusion = SAMPLE_TEXTURE2D_ARRAY(AmbientOcclusion, Sampler, UV, k + 4).r * blendFactorsUV3[k];
        resultAmbientOcclusion += ambientOcclusion;
    }
    
    // Output the blended results
    outDiffuse = resultDiffuse;
    outMetallicGlossiness = resultMetallicGloss;
    outEmission = resultEmission;
    outAmbientOcclusion = resultAmbientOcclusion;  
}

void TextureArrayBlend_float(UnityTexture2DArray Texture, float Slice, float2 UV, UnitySamplerState Sampler, float BlendFactor, out float4 Out, out float R, out float G, out float B, out float A)
{
    float slice = Slice;
    int textureindex = slice;

    float4 c1 = SAMPLE_TEXTURE2D_ARRAY(Texture, Sampler, UV, textureindex) * BlendFactor;
    Out = c1;
    R = Out.x;
    G = Out.y;
    B = Out.z;
    A = Out.w;
}

void MergeResult_float(float4 Diffuse, float Metallic, float Smoothness, float3 Normal, float AmbientOcclusion, float3 Emission,
float4 Diffuse2, float Metallic2, float Smoothness2, float3 Normal2, float AmbientOcclusion2, float3 Emission2,
out float4 outDiffuse, out float outMetallic, out float outSmoothness, out float3 outNormal, out float outAmbientOcclusion, out float3 outEmission)
{   
    outDiffuse = saturate(Diffuse + Diffuse2);
    outMetallic = saturate(Metallic + Metallic2);
    outSmoothness = saturate(Smoothness + Smoothness2);
    outNormal = (saturate(Normal + Normal2));
    outAmbientOcclusion = saturate(AmbientOcclusion + AmbientOcclusion2);
    outEmission = saturate(Emission + Emission2);
}


void TextureArrayBlend_triplanar_float(UnityTexture2DArray Texture, float Slice, float3 position, float3 normal, UnitySamplerState Sampler, float2 UV3, float UV3Power, out float4 Out, out float R, out float G, out float B, out float A)
{  
    float slice = Slice + UV3.x * UV3Power;
    slice = max(0, slice);
    int textureindex = slice;

    float blendfactor = slice % 1;

    float4 x =  SAMPLE_TEXTURE2D_ARRAY(Texture, Sampler, position.yz, textureindex) *normal.x;
    float4 y = SAMPLE_TEXTURE2D_ARRAY(Texture, Sampler, position.xz, textureindex) * normal.y;
    float4 z = SAMPLE_TEXTURE2D_ARRAY(Texture, Sampler, position.xy, textureindex) * normal.z;
   
    
    float4 c1 = (x + y + z) * (1 - blendfactor);
    
    x = SAMPLE_TEXTURE2D_ARRAY(Texture, Sampler, position.yz, textureindex + 1) * normal.x;
    y = SAMPLE_TEXTURE2D_ARRAY(Texture, Sampler, position.xz, textureindex + 1) * normal.y;
    z = SAMPLE_TEXTURE2D_ARRAY(Texture, Sampler, position.xy, textureindex + 1) * normal.z;
     
    float4 c2 = (x + y + z) * (blendfactor);
    Out = c1 + c2;
    R = Out.x;
    G = Out.y;
    B = Out.z;
    A = Out.w;
}