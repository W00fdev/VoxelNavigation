
void TextureArrayBlendLayers_Triplanar_Normal_float(UnityTexture2D BaseNormal, UnityTexture2DArray Normal, float2 UV, UnitySamplerState Sampler, float4 blendFactorsUV2, float4 blendFactorsUV3, float baseLayerFactor,
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

// Triplanar sampling function for float4 textures
float4 TriplanarSample_float4(UnityTexture2D tex, UnitySamplerState Sampler, float3 worldPos, float3 worldNormal)
{
    // Compute blending weights based on the surface normal
    float3 absWorldNormal = abs(worldNormal);
    float3 blendingWeights = absWorldNormal / (absWorldNormal.x + absWorldNormal.y + absWorldNormal.z);

    // Sample the texture along the X, Y, and Z axes
    float4 sampleX = SAMPLE_TEXTURE2D(tex, 
    Sampler, worldPos.
    yz); // X axis uses YZ plane
    float4 sampleY = SAMPLE_TEXTURE2D(tex, 
    Sampler, worldPos.
    zx); // Y axis uses ZX plane
    float4 sampleZ = SAMPLE_TEXTURE2D(tex, 
    Sampler, worldPos.
    xy); // Z axis uses XY plane

    // Return the blended result
    return sampleX * blendingWeights.x + sampleY * blendingWeights.y + sampleZ * blendingWeights.z;
}

// Triplanar sampling function for float (used for ambient occlusion)
float TriplanarSample_float(UnityTexture2D tex, UnitySamplerState Sampler, float3 worldPos, float3 worldNormal)
{
    float3 absWorldNormal = abs(worldNormal);
    float3 blendingWeights = absWorldNormal / (absWorldNormal.x + absWorldNormal.y + absWorldNormal.z);

    float sampleX = SAMPLE_TEXTURE2D(tex, 
    Sampler, worldPos.
    yz).
    r;
    float sampleY = SAMPLE_TEXTURE2D(tex, 
    Sampler, worldPos.
    zx).
    r;
    float sampleZ = SAMPLE_TEXTURE2D(tex, 
    Sampler, worldPos.
    xy).
    r;

    return sampleX * blendingWeights.x + sampleY * blendingWeights.y + sampleZ * blendingWeights.z;
}

// Triplanar sampling function for texture arrays (float4)
float4 TriplanarSampleArray_float4(UnityTexture2DArray texArray, UnitySamplerState Sampler, float3 worldPos, float3 worldNormal, int layer)
{
    float3 absWorldNormal = abs(worldNormal);
    float3 blendingWeights = absWorldNormal / (absWorldNormal.x + absWorldNormal.y + absWorldNormal.z);

    float4 sampleX = SAMPLE_TEXTURE2D_ARRAY(texArray, 
    Sampler, worldPos.
    yz, layer);
    float4 sampleY = SAMPLE_TEXTURE2D_ARRAY(texArray, 
    Sampler, worldPos.
    zx, layer);
    float4 sampleZ = SAMPLE_TEXTURE2D_ARRAY(texArray, 
    Sampler, worldPos.
    xy, layer);

    return sampleX * blendingWeights.x + sampleY * blendingWeights.y + sampleZ * blendingWeights.z;
}

// Triplanar sampling function for texture arrays (float)
float TriplanarSampleArray_float(UnityTexture2DArray texArray, UnitySamplerState Sampler, float3 worldPos, float3 worldNormal, int layer)
{
    float3 absWorldNormal = abs(worldNormal);
    float3 blendingWeights = absWorldNormal / (absWorldNormal.x + absWorldNormal.y + absWorldNormal.z);

    float sampleX = SAMPLE_TEXTURE2D_ARRAY(texArray, 
    Sampler, worldPos.yz, layer).r;
    float sampleY = SAMPLE_TEXTURE2D_ARRAY(texArray, 
    Sampler, worldPos.zx, layer).r;
    float sampleZ = SAMPLE_TEXTURE2D_ARRAY(texArray, 
    Sampler, worldPos.
    xy, layer).
    r;

    return sampleX * blendingWeights.x + sampleY * blendingWeights.y + sampleZ * blendingWeights.z;
}

// Triplanar sampling function for height maps
float TriplanarSampleHeight(UnityTexture2D tex, UnitySamplerState Sampler, float3 worldPos, float3 worldNormal)
{
    float3 absWorldNormal = abs(worldNormal);
    float3 blendingWeights = absWorldNormal / (absWorldNormal.x + absWorldNormal.y + absWorldNormal.z);

    // Sample the height map along the X, Y, and Z axes
    float heightX = SAMPLE_TEXTURE2D_LOD(tex, 
    Sampler, worldPos.
    yz, 0).
    r;
    float heightY = SAMPLE_TEXTURE2D_LOD(tex, 
    Sampler, worldPos.
    zx, 0).
    r;
    float heightZ = SAMPLE_TEXTURE2D_LOD(tex, 
    Sampler, worldPos.
    xy, 0).
    r;

    // Blend the heights and return the result
    return heightX * blendingWeights.x + heightY * blendingWeights.y + heightZ * blendingWeights.z;
}

// Triplanar sampling function for height maps in texture arrays
float TriplanarSampleArrayHeight(UnityTexture2DArray texArray, UnitySamplerState Sampler, float3 worldPos, float3 worldNormal, int layer)
{
    float3 absWorldNormal = abs(worldNormal);
    float3 blendingWeights = absWorldNormal / (absWorldNormal.x + absWorldNormal.y + absWorldNormal.z);

    // Sample the height map along the X, Y, and Z axes from the texture array
    float heightX = SAMPLE_TEXTURE2D_ARRAY_LOD(texArray, 
    Sampler, worldPos.
    yz, layer, 0).
    r;
    float heightY = SAMPLE_TEXTURE2D_ARRAY_LOD(texArray, 
    Sampler, worldPos.
    zx, layer, 0).
    r;
    float heightZ = SAMPLE_TEXTURE2D_ARRAY_LOD(texArray, 
    Sampler, worldPos.
    xy, layer, 0).
    r;

    // Blend the heights and return the result
    return heightX * blendingWeights.x + heightY * blendingWeights.y + heightZ * blendingWeights.z;
}


// Main function with triplanar sampling for both base and array layers
void TextureArrayBlendLayers_Triplanar_float(UnityTexture2D BaseDiffuse, UnityTexture2D BaseMetallic, UnityTexture2D BaseEmission, UnityTexture2D BaseAmbientOcclusion,
                                   UnityTexture2DArray Diffuse, UnityTexture2DArray MetallicGloss, UnityTexture2DArray Emission, UnityTexture2DArray AmbientOcclusion,
                                   float3 TriplanarPosition, float3 TriplanarNormal, UnitySamplerState Sampler, float4 blendFactorsUV2, float4 blendFactorsUV3, float baseLayerFactor,
                                   out float4 outDiffuse, out float4 outMetallicGlossiness, out float3 outEmission, out float outAmbientOcclusion)
{
    // Base layer sampling using triplanar mapping
    float4 resultDiffuse = TriplanarSample_float4(BaseDiffuse, Sampler, TriplanarPosition, TriplanarNormal) * baseLayerFactor;
    float4 resultMetallicGloss = TriplanarSample_float4(BaseMetallic, Sampler, TriplanarPosition, TriplanarNormal) * baseLayerFactor;
    float3 resultEmission = TriplanarSample_float4(BaseEmission, Sampler, TriplanarPosition, TriplanarNormal).rgb * baseLayerFactor;
    float resultAmbientOcclusion = TriplanarSample_float(BaseAmbientOcclusion, Sampler, TriplanarPosition, TriplanarNormal) * baseLayerFactor;

    // Sample and blend textures from the texture array (first 4 layers with blendFactorsUV2)
    for (int i = 0; i < 4; i++)
    {
        // Sample and blend diffuse textures using triplanar mapping
        float4 diffuse = TriplanarSampleArray_float4(Diffuse, Sampler, TriplanarPosition, TriplanarNormal, i) * blendFactorsUV2[i];
        resultDiffuse += diffuse;

        // Sample and blend metallic gloss textures using triplanar mapping
        float4 metallicGloss = TriplanarSampleArray_float4(MetallicGloss, Sampler, TriplanarPosition, TriplanarNormal, i) * blendFactorsUV2[i];
        resultMetallicGloss += metallicGloss;

        // Sample and blend emission textures using triplanar mapping
        float3 emission = TriplanarSampleArray_float4(Emission, Sampler, TriplanarPosition, TriplanarNormal, i).rgb * blendFactorsUV2[i];
        resultEmission += emission;

        // Sample and blend ambient occlusion textures using triplanar mapping
        float ambientOcclusion = TriplanarSampleArray_float(AmbientOcclusion, Sampler, TriplanarPosition, TriplanarNormal, i) * blendFactorsUV2[i];
        resultAmbientOcclusion += ambientOcclusion;
    }

    // Sample and blend textures from the texture array (next 4 layers with blendFactorsUV3)
    for (int k = 0; k < 4; k++)
    {
        // Sample and blend diffuse textures using triplanar mapping
        float4 diffuse = TriplanarSampleArray_float4(Diffuse, Sampler, TriplanarPosition, TriplanarNormal, k + 4) * blendFactorsUV3[k];
        resultDiffuse += diffuse;

        // Sample and blend metallic gloss textures using triplanar mapping
        float4 metallicGloss = TriplanarSampleArray_float4(MetallicGloss, Sampler, TriplanarPosition, TriplanarNormal, k + 4) * blendFactorsUV3[k];
        resultMetallicGloss += metallicGloss;

        // Sample and blend emission textures using triplanar mapping
        float3 emission = TriplanarSampleArray_float4(Emission, Sampler, TriplanarPosition, TriplanarNormal, k + 4).rgb * blendFactorsUV3[k];
        resultEmission += emission;

        // Sample and blend ambient occlusion textures using triplanar mapping
        float ambientOcclusion = TriplanarSampleArray_float(AmbientOcclusion, Sampler, TriplanarPosition, TriplanarNormal, k + 4) * blendFactorsUV3[k];
        resultAmbientOcclusion += ambientOcclusion;
    }

    // Output the blended results
    outDiffuse = resultDiffuse;
    outMetallicGlossiness = resultMetallicGloss;
    outEmission = resultEmission;
    outAmbientOcclusion = resultAmbientOcclusion;
}

// Main function for blending parallax height layers with triplanar mapping
void ParallaxLayer_Triplanar_float(UnityTexture2D BaseHeightMap, UnityTexture2DArray HeightMap, float3 TriplanarPosition, float3 TriplanarNormal, UnitySamplerState Sampler, float4 blendFactorsUV2, float4 blendFactorsUV3, float baseLayerFactor,
                         out float outHeight)
{
    // Base layer height sampling using triplanar mapping
    float resultHeight = TriplanarSampleHeight(BaseHeightMap, Sampler, TriplanarPosition, TriplanarNormal) * baseLayerFactor;

    // Sample and blend height maps from the texture array (first 4 layers with blendFactorsUV2)
    for (int i = 0; i < 4; i++)
    {
        float height = TriplanarSampleArrayHeight(HeightMap, Sampler, TriplanarPosition, TriplanarNormal, i) * blendFactorsUV2[i];
        resultHeight += height;
    }

    // Sample and blend height maps from the texture array (next 4 layers with blendFactorsUV3)
    for (int k = 0; k < 4; k++)
    {
        float height = TriplanarSampleArrayHeight(HeightMap, Sampler, TriplanarPosition, TriplanarNormal, k + 4) * blendFactorsUV3[k];
        resultHeight += height;
    }

    // Output the blended height result
    outHeight = resultHeight;
}

// Triplanar sampling function for normal maps (unpacking normal)
float3 TriplanarSampleNormal(UnityTexture2D tex, UnitySamplerState Sampler, float3 worldPos, float3 worldNormal)
{
    float3 absWorldNormal = abs(worldNormal);
    float3 blendingWeights = absWorldNormal / (absWorldNormal.x + absWorldNormal.y + absWorldNormal.z);

    // Sample the normal map along the X, Y, and Z axes
    float4 normalX = SAMPLE_TEXTURE2D(tex, 
    Sampler, worldPos.
    yz);
    float4 normalY = SAMPLE_TEXTURE2D(tex, 
    Sampler, worldPos.
    zx);
    float4 normalZ = SAMPLE_TEXTURE2D(tex, 
    Sampler, worldPos.
    xy);

    // Unpack and blend the normals
    float3 unpackedNormalX = UnpackNormalmapRGorAG(normalX);
    float3 unpackedNormalY = UnpackNormalmapRGorAG(normalY);
    float3 unpackedNormalZ = UnpackNormalmapRGorAG(normalZ);

    // Return the blended result
    return unpackedNormalX * blendingWeights.x + unpackedNormalY * blendingWeights.y + unpackedNormalZ * blendingWeights.z;
}

// Triplanar sampling function for normal maps in texture arrays
float3 TriplanarSampleArrayNormal(UnityTexture2DArray texArray, UnitySamplerState Sampler, float3 worldPos, float3 worldNormal, int layer)
{
    float3 absWorldNormal = abs(worldNormal);
    float3 blendingWeights = absWorldNormal / (absWorldNormal.x + absWorldNormal.y + absWorldNormal.z);

    // Sample the normal map along the X, Y, and Z axes from the texture array
    float4 normalX = SAMPLE_TEXTURE2D_ARRAY(texArray, 
    Sampler, worldPos.
    yz, layer);
    float4 normalY = SAMPLE_TEXTURE2D_ARRAY(texArray, 
    Sampler, worldPos.
    zx, layer);
    float4 normalZ = SAMPLE_TEXTURE2D_ARRAY(texArray, 
    Sampler, worldPos.
    xy, layer);

    // Unpack and blend the normals
    float3 unpackedNormalX = UnpackNormalmapRGorAG(normalX);
    float3 unpackedNormalY = UnpackNormalmapRGorAG(normalY);
    float3 unpackedNormalZ = UnpackNormalmapRGorAG(normalZ);

    // Return the blended result
    return unpackedNormalX * blendingWeights.x + unpackedNormalY * blendingWeights.y + unpackedNormalZ * blendingWeights.z;
}

// Main function for blending normals with triplanar mapping
void TextureArrayBlendLayers_Triplanar_Normal_float(UnityTexture2D BaseNormal, UnityTexture2DArray Normal, float3 TriplanarPosition, float3 TriplanarNormal, UnitySamplerState Sampler, float4 blendFactorsUV2, float4 blendFactorsUV3, float baseLayerFactor,
                                          out float3 outNormal)
{
    // Base layer normal sampling using triplanar mapping
    float3 resultNormal = TriplanarSampleNormal(BaseNormal, Sampler, TriplanarPosition, TriplanarNormal) * baseLayerFactor;

    // Sample and blend normal maps from the texture array (first 4 layers with blendFactorsUV2)
    for (int i = 0; i < 4; i++)
    {
        float3 normal = TriplanarSampleArrayNormal(Normal, Sampler, TriplanarPosition, TriplanarNormal, i) * blendFactorsUV2[i];
        resultNormal += normal;
    }

    // Sample and blend normal maps from the texture array (next 4 layers with blendFactorsUV3)
    for (int k = 0; k < 4; k++)
    {
        float3 normal = TriplanarSampleArrayNormal(Normal, Sampler, TriplanarPosition, TriplanarNormal, k + 4) * blendFactorsUV3[k];
        resultNormal += normal;
    }

    // Normalize the final normal vector to ensure it's a valid normal
    outNormal = normalize(resultNormal);
}


// Modified Parallax Offset for Triplanar Mapping
void ParallaxOffsetTriplanar_float(float h, float height, float3 viewDir, float3 worldNormal, float3 worldPos,
                                   out float3 outOffsetPosition)
{
    // Normalize the view direction
    float3 v = normalize(viewDir);
    v.z += 0.42; // Small offset to prevent parallax artifacts

    // Compute blending weights based on the surface normal
    float3 absWorldNormal = abs(worldNormal);
    float3 blendingWeights = absWorldNormal / (absWorldNormal.x + absWorldNormal.y + absWorldNormal.z);

    // Apply parallax offset to each axis
    float2 offsetX = h * (v.yz / v.z); // Parallax offset for X projection (on the YZ plane)
    float2 offsetY = h * (v.zx / v.z); // Parallax offset for Y projection (on the ZX plane)
    float2 offsetZ = h * (v.xy / v.z); // Parallax offset for Z projection (on the XY plane)

    // Adjust the world position by applying the offsets
    float3 offsetPositionX = float3(worldPos.x, worldPos.y + offsetX.x, worldPos.z + offsetX.y);
    float3 offsetPositionY = float3(worldPos.x + offsetY.x, worldPos.y, worldPos.z + offsetY.y);
    float3 offsetPositionZ = float3(worldPos.x + offsetZ.x, worldPos.y + offsetZ.y, worldPos.z);

    // Blend the offset positions based on the surface normal
    outOffsetPosition = offsetPositionX * blendingWeights.x +
                        offsetPositionY * blendingWeights.y +
                        offsetPositionZ * blendingWeights.z;
}
