Shader "Custom/SimpleToonShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _BaseColor ("Base Color", Color) = (1,1,1,1)
        _StepAmount ("Shadow Steps", Range(1, 10)) = 2
        _Intensity ("Shadow Intensity", Range(0, 1)) = 0.5
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        LOD 100

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _BaseColor;
            float _StepAmount;
            float _Intensity;

            Varyings vert (Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                return output;
            }

            half4 frag (Varyings input) : SV_Target
            {
                half4 texColor = tex2D(_MainTex, input.uv) * _BaseColor;

                Light mainLight = GetMainLight();
                float3 normal = normalize(input.normalWS);
                
                float NdotL = dot(normal, mainLight.direction);

                float lightStep = floor((NdotL * 0.5 + 0.5) * _StepAmount) / _StepAmount;
                
                float toonLight = lerp(_Intensity, 1.0, lightStep);

                return texColor * toonLight * half4(mainLight.color, 1);
            }
            ENDHLSL
        }
    }
}