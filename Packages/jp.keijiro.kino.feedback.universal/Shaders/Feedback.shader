Shader "Hidden/KinoFeedbackURP"
{
HLSLINCLUDE

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

TEXTURE2D(_FeedbackTexture);

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
    uv = (uv - 0.5) * 0.9 + 0.5;
    return SAMPLE_TEXTURE2D(_FeedbackTexture, sampler_LinearClamp, uv);
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
