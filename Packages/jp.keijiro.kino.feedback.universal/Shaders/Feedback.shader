Shader "Hidden/KinoFeedbackURP"
{
HLSLINCLUDE

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

TEXTURE2D_X(_FeedbackTexture);
float4x4 _Transform;
float4 _Tint;
float _HueShift;
float _SampleMode;

half3 ModifyColor(half3 rgb)
{
    #ifndef UNITY_COLORSPACE_GAMMA
    rgb = LinearToSRGB(rgb);
    #endif
    rgb = RgbToHsv(rgb);
    rgb.x = frac(rgb.x + _HueShift);
    rgb = HsvToRgb(rgb);
    #ifndef UNITY_COLORSPACE_GAMMA
    rgb = SRGBToLinear(rgb);
    #endif
    rgb *= _Tint.rgb;
    return rgb;
}

void VertInjection(uint vertexID : VERTEXID_SEMANTIC,
                   out float4 outPosition : SV_Position,
                   out float2 outTexCoord : TEXCOORD0)
{
    outPosition = GetFullScreenTriangleVertexPosition(vertexID);
    outPosition.z = UNITY_RAW_FAR_CLIP_VALUE;
    outTexCoord = GetFullScreenTriangleTexCoord(vertexID);
}

half4 FragInjection(float4 position : SV_Position,
                    float2 texCoord : TEXCOORD) : SV_Target0
{
    float2 uv = mul((float3x3)_Transform, float3(texCoord, 1)).xy;
    half4 s0 = SAMPLE_TEXTURE2D_X(_FeedbackTexture, sampler_PointClamp, uv);
    half4 s1 = SAMPLE_TEXTURE2D_X(_FeedbackTexture, sampler_LinearClamp, uv);
    half4 c = lerp(s0, s1, _SampleMode);
    return half4(ModifyColor(c.rgb), c.a);
}

ENDHLSL

    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" }
        Pass
        {
            Name "KinoFeedback Injection"
            ZTest LEqual ZWrite Off Cull Off Blend Off
            HLSLPROGRAM
            #pragma vertex VertInjection
            #pragma fragment FragInjection
            ENDHLSL
        }
    }
}
