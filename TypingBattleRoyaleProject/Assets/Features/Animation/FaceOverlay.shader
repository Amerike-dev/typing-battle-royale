Shader "TBR/FaceOverlay"
{
    // Dibuja un PNG de cara "suelto" sobre la malla de la cabeza, reubicándolo dentro de un
    // rectángulo de UV (_FaceRect) que corresponde a la isla UV de la cara en la textura del cuerpo.
    // Solo se pinta donde la UV de la malla cae dentro de _FaceRect; el resto es transparente.
    // Como va sobre una copia de la SkinnedMeshRenderer, se deforma igual que el modelo.
    Properties
    {
        [MainTexture] _BaseMap ("Cara (PNG)", 2D) = "white" {}
        _FaceRect ("Rect UV de la cara (x, y, ancho, alto)", Vector) = (0, 0, 1, 1)
        _AlphaCutoff ("Recorte de alfa", Range(0, 1)) = 0.02
        [Toggle] _FlipX ("Espejar en X", Float) = 0
        [Enum(UnityEngine.Rendering.CullMode)] _Cull ("Cull", Float) = 2
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
            "IgnoreProjector" = "True"
        }

        Pass
        {
            Name "FaceOverlay"
            Tags { "LightMode" = "UniversalForward" }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            ZTest LEqual
            Cull [_Cull]

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                float4 _FaceRect;
                float _AlphaCutoff;
                float _FlipX;
                float _Cull;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            Varyings vert (Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                return OUT;
            }

            half4 frag (Varyings IN) : SV_Target
            {
                // UV de la malla -> espacio [0,1] dentro del rect de la cara.
                float2 fuv = (IN.uv - _FaceRect.xy) / max(_FaceRect.zw, float2(1e-5, 1e-5));

                // Fuera del rect: no pintamos (el resto de la cabeza/cuerpo se ve normal).
                if (fuv.x < 0.0 || fuv.x > 1.0 || fuv.y < 0.0 || fuv.y > 1.0)
                    discard;

                if (_FlipX > 0.5) fuv.x = 1.0 - fuv.x;

                half4 c = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, fuv);
                if (c.a < _AlphaCutoff)
                    discard;

                return c;
            }
            ENDHLSL
        }
    }

    Fallback Off
}
