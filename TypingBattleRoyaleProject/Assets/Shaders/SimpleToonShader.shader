Shader "Custom/SimpleToonShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _BaseColor ("Base Color", Color) = (1,1,1,1)
        _StepAmount ("Shadow Steps", Range(1, 10)) = 2
        _Intensity ("Shadow Intensity", Range(0, 1)) = 0.5
        [HDR] _EmissionColor ("Emission Color (HDR)", Color) = (0,0,0,1)
        _EmissionStrength ("Emission Strength", Range(0, 20)) = 0
        _FresnelPower ("Fresnel (Rim) Power", Range(0.5, 8)) = 3
        _FresnelStrength ("Fresnel (Rim) Strength", Range(0, 10)) = 0
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
                float3 positionWS : TEXCOORD2;
            };

            sampler2D _MainTex;
            float4 _BaseColor;
            float _StepAmount;
            float _Intensity;
            half4 _EmissionColor;
            float _EmissionStrength;
            float _FresnelPower;
            float _FresnelStrength;

            Varyings vert (Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.positionWS = TransformObjectToWorld(input.positionOS.xyz);
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

                half3 litColor = texColor.rgb * toonLight * mainLight.color;

                // Emisión base (glow controlado desde el material, alimenta el Bloom de URP)
                half3 emission = _EmissionColor.rgb * _EmissionStrength;

                // Fresnel / rim para un brillo místico en los bordes
                float3 viewDir = normalize(GetCameraPositionWS() - input.positionWS);
                float fresnel = pow(saturate(1.0 - dot(normal, viewDir)), _FresnelPower);
                emission += _EmissionColor.rgb * fresnel * _FresnelStrength;

                return half4(litColor + emission, texColor.a);
            }
            ENDHLSL
        }
    }
}