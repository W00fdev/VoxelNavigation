Shader "Fraktalia/Core/Sky/RotatingSky_Enhanced" {
    Properties{
        _Tint("Tint Color", Color) = (.5, .5, .5, .5)

        _Hue("Hue", Range(0, 1)) = 0.0
        _Saturation("Saturation", Range(0, 1)) = 0.0
        _Value("Value", Range(0, 1)) = 0.0

        [Gamma] _Exposure("Exposure", Range(0, 8)) = 1.0
        _Rotation("Rotation", Range(0, 360)) = 0
        [NoScaleOffset] _Tex("Cubemap   (HDR)", Cube) = "grey" {}
        _CubemapRotationSpeed("Cubemap Rotation Speed", float) = 1


        _BlendMap("Blend Map", CUBE) = "white" {}
        _BlendMap_Size("Blend Size", float) = 1.0
        _RotationSpeed("Rotation Speed", float) = 1
        _RotationOffset("Rotation Offset", float) = 1
        _MRStepsX("Steps X", Range(0, 20)) = 1
        _MRStepsY("Steps Y", Range(0, 20)) = 1
        _MRDistX("Dist X", int) = 1
        _MRDistY("Dist Y", int) = 1
        _MRSkyExponent("Exponent", int) = 1
    }

        SubShader{
            Tags { "Queue" = "Background" "RenderType" = "Background" "PreviewType" = "Skybox" }
            Cull Off ZWrite Off

            Pass {
                CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                #pragma target 2.0

                #include "UnityCG.cginc"

                samplerCUBE _Tex;
                half4 _Tex_HDR;
                half4 _Tint;
                float _Hue;
                float _Saturation;
                float _Value;
                half _Exposure;
                float _Rotation;
                float _CubemapRotationSpeed;

                float3 RotateAroundYInDegrees(float3 vertex, float degrees) {
                    float alpha = degrees * UNITY_PI / 180.0;
                    float sina, cosa;
                    sincos(alpha, sina, cosa);
                    float2x2 m = float2x2(cosa, -sina, sina, cosa);
                    return float3(mul(m, vertex.xz), vertex.y).xzy;
                }

                struct appdata_t {
                    float4 vertex : POSITION;
                    UNITY_VERTEX_INPUT_INSTANCE_ID
                };

                struct v2f {
                    float4 vertex : SV_POSITION;
                    float3 texcoord : TEXCOORD0;
                    UNITY_VERTEX_OUTPUT_STEREO
                    float3 viewDir : TEXCOORD2;
                };

                samplerCUBE _BlendMap;
                float _BlendMap_Size;
                uniform int _MRStepsX;
                uniform int _MRStepsY;
                uniform float _MRDistX;
                uniform float _MRDistY;
                uniform float _MRSkyExponent;
                uniform float _RotationSpeed;
                uniform float _RotationOffset;

                float3 RGBtoHSV(float3 rgb) {
                    float cmax = max(rgb.r, max(rgb.g, rgb.b));
                    float cmin = min(rgb.r, min(rgb.g, rgb.b));
                    float delta = cmax - cmin;

                    float h = 0.0;
                    if (delta == 0.0) {
                        h = 0.0;
                    }
                    else if (cmax == rgb.r) {
                        h = 60.0 * fmod((rgb.g - rgb.b) / delta, 6.0);
                    }
                    else if (cmax == rgb.g) {
                        h = 60.0 * ((rgb.b - rgb.r) / delta + 2.0);
                    }
                    else if (cmax == rgb.b) {
                        h = 60.0 * ((rgb.r - rgb.g) / delta + 4.0);
                    }

                    if (h < 0.0) h += 360.0; // Ensure hue is positive

                    float s = (cmax == 0.0) ? 0.0 : delta / cmax;
                    float v = cmax;

                    return float3(h, s, v);
                }

                float3 HSVtoRGB(float3 hsv) {
                    float C = hsv.z * hsv.y;
                    float X = C * (1.0 - abs(fmod(hsv.x / 60.0, 2.0) - 1.0));
                    float m = hsv.z - C;

                    float3 rgb;

                    if (hsv.x >= 0.0 && hsv.x < 60.0)
                        rgb = float3(C, X, 0.0);
                    else if (hsv.x >= 60.0 && hsv.x < 120.0)
                        rgb = float3(X, C, 0.0);
                    else if (hsv.x >= 120.0 && hsv.x < 180.0)
                        rgb = float3(0.0, C, X);
                    else if (hsv.x >= 180.0 && hsv.x < 240.0)
                        rgb = float3(0.0, X, C);
                    else if (hsv.x >= 240.0 && hsv.x < 300.0)
                        rgb = float3(X, 0.0, C);
                    else
                        rgb = float3(C, 0.0, X);

                    return rgb + m;
                }

                float3 AdjustHSV(float3 color, float hueShift, float saturationMultiplier, float valueMultiplier) {
                    float3 hsv = RGBtoHSV(color);

                    // Apply hue shift (wrap between 0-360)
                    hsv.x = fmod(hsv.x + hueShift * 360.0, 360.0);
                    if (hsv.x < 0.0) hsv.x += 360.0;

                    // Adjust saturation (clamp to [0,1])
                    hsv.y = saturate(hsv.y * saturationMultiplier);

                    // Adjust value/brightness (clamp to [0,1])
                    hsv.z = saturate(hsv.z * valueMultiplier);

                    return HSVtoRGB(hsv);
                }

                v2f vert(appdata_t v) {
                    v2f o;
                    UNITY_SETUP_INSTANCE_ID(v);
                    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                    o.vertex = UnityObjectToClipPos(v.vertex);
                    o.texcoord = v.vertex.xyz;
                    float4x4 modelMatrix = unity_ObjectToWorld;
                    o.viewDir = mul(modelMatrix, v.vertex).xyz - _WorldSpaceCameraPos;
                    return o;
                }

                fixed4 frag(v2f i) : SV_Target {
                    float rotation = _Rotation + (_CubemapRotationSpeed * _Time.y) + _RotationOffset;
                    float3 rotatedTexcoord = RotateAroundYInDegrees(i.texcoord, rotation);
                    float4 tex = texCUBE(_Tex, rotatedTexcoord);
                    
           
                    float3 c = DecodeHDR(tex, _Tex_HDR);
                    c = AdjustHSV(c, _Hue, _Saturation, _Value);
                    c = c * _Tint.rgb * unity_ColorSpaceDouble.rgb;
                    c *= _Exposure;

                    float3 M = float3(0, 0, 0);

                    for (int r = 0; r < _MRStepsX; r++) {
                        float sin45 = sin(r * 0.1 * _MRDistX);
                        float cos45 = cos(r * 0.1 * _MRDistX);

                        float3x3 rot_45 = float3x3(
                            cos45, 0, sin45,
                            0, 1, 0,
                            -sin45, 0, cos45
                        );

                        for (int k = 0; k < _MRStepsY; k++) {
                            float sinX = sin(_RotationSpeed * -_Time + k * 0.1 * _MRDistY + _RotationOffset);
                            float cosX = cos(_RotationSpeed * -_Time + k * 0.1 * _MRDistY + _RotationOffset);
                            float sinY = sin(_RotationSpeed * -_Time + k * 0.1 * _MRDistY + _RotationOffset);
                            float3x3 rot_A = float3x3(
                                1, 0, 0,
                                0, cosX, -sinY,
                                0, sinX, cosX
                            );

                            float3x3 rotationMatrix = mul(rot_45, rot_A);
                            float3 scroll = i.viewDir;
                            M += texCUBE(_BlendMap, mul(scroll, rotationMatrix));
                        }
                    }

                    

                    M = M * _BlendMap_Size;
                    float3 I = c;
                    c = (I) * (I + (2 * M) * (1 - I));

                    c.r = pow(c.r, _MRSkyExponent);
                    c.g = pow(c.g, _MRSkyExponent);
                    c.b = pow(c.b, _MRSkyExponent);

                    return half4(c, 1);
                }
                ENDCG
            }
        }
            Fallback Off
}
