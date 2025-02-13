Shader "Hidden/KinoFeedbackURP"
{
HLSLINCLUDE

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

TEXTURE2D(_FeedbackTexture);

float4 _Transform;
float4 _Tint;
float _HueShift;

float3x3 ConstructTransformMatrix()
{
    return float3x3(_Transform.y, -_Transform.x, _Transform.z,
                    _Transform.x,  _Transform.y, _Transform.w,
                               0,             0,            1);
}

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
    uv = mul(ConstructTransformMatrix(), float3(uv - 0.5, 1)).xy + 0.5;

    // Feedback sample
    float4 c = SAMPLE_TEXTURE2D(_FeedbackTexture, sampler_LinearClamp, uv);

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
