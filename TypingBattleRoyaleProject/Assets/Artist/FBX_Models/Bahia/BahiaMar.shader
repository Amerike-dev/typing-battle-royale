Shader "Custom/BahiaMar"
{
    Properties
    {
        _MainTex ("Water Texture", 2D) = "white" {}
        _BaseColor ("Water Color", Color) = (1, 1, 1, 1)
        _WaveSpeed ("Wave Speed", Float) = 2.0
        _WaveAmp ("Wave Amplitude", Float) = 0.2
        _WaveFreq ("Wave Frequency", Float) = 1.5
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
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _BaseColor;
            float _WaveSpeed;
            float _WaveAmp;
            float _WaveFreq;

            Varyings vert (Attributes input)
            {
                Varyings output;
                
                float3 worldPos = TransformObjectToWorld(input.positionOS.xyz);
                
                float waveX = sin(worldPos.x * _WaveFreq + _Time.y * _WaveSpeed) * _WaveAmp;
                float waveZ = cos(worldPos.z * (_WaveFreq * 0.8) + _Time.y * (_WaveSpeed * 1.1)) * _WaveAmp;
                worldPos.y += waveX + waveZ;

                output.positionCS = TransformWorldToHClip(worldPos);
                output.positionWS = worldPos;
                output.uv = input.uv;
                
                return output;
            }

            half4 frag (Varyings input) : SV_Target
            {
                float3 dX = ddx(input.positionWS);
                float3 dY = ddy(input.positionWS);
                float3 faceNormal = normalize(cross(dX, dY));

                half4 texColor = tex2D(_MainTex, input.uv) * _BaseColor;

                Light mainLight = GetMainLight();
                
                float NdotL = saturate(dot(faceNormal, mainLight.direction));
                
                float3 finalLight = mainLight.color * NdotL + half3(0.2, 0.2, 0.2); 

                return texColor * half4(finalLight, 1);
            }
            ENDHLSL
        }
    }
}