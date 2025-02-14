Shader "Hidden/KinoFeedbackURP"
{
HLSLINCLUDE

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

TEXTURE2D(_FeedbackTexture);

float4x4 _Transform;
float4 _Tint;
float _HueShift;
float _SampleMode;

float3 ApplyTint(float3 rgb)
{
    rgb = RgbToHsv(rgb);
    rgb.x = frac(rgb.x + _HueShift);
    rgb = HsvToRgb(rgb);
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

float4 FragInjection(float4 position : SV_Position,
                     float2 texCoord : TEXCOORD) : SV_Target0
{
    float2 uv = texCoord;

    // Feedback transform
    uv = mul(_Transform, float3(uv, 1)).xy;

    // Feedback sample (point/bilinear)
    float4 s0 = SAMPLE_TEXTURE2D(_FeedbackTexture, sampler_PointClamp, uv);
    float4 s1 = SAMPLE_TEXTURE2D(_FeedbackTexture, sampler_LinearClamp, uv);
    float4 c = lerp(s0, s1, _SampleMode);

    // Composition
    c.rgb = ApplyTint(c.rgb);

    return c;
}

ENDHLSL

    SubShader
    {
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
