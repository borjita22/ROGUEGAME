Shader "Custom/CelShadingOutline"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _Color("Color", Color) = (1,1,1,1)

            // Propiedades de Cel Shading
            [Space(10)]
            [Header(Cel Shading Properties)]
            _LightingRamp("Lighting Ramp", Range(0, 1)) = 0.5
            _ShadowColor("Shadow Color", Color) = (0.5,0.5,0.5,1)
            _SampleTexture2D("Texture Sample", 2D) = "white" {}

            // Propiedades de Outline
            [Space(10)]
            [Header(Outline Properties)]
            _OutlineColor("Outline Color", Color) = (0,0,0,1)
            _OutlineThickness("Outline Thickness", Range(0, 0.1)) = 0.01
            _WorldBoundsMagnitude("World Bounds Scale", Float) = 1
    }

        SubShader
            {
                Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "UniversalMaterialType" = "Lit" }

                // Paso 1: Renderizado principal con Cel Shading
                Pass
                {
                    Name "ForwardLit"
                    Tags { "LightMode" = "UniversalForward" }

                    HLSLPROGRAM
                    #pragma target 4.5
                    #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
                    #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
                    #pragma multi_compile _ _SHADOWS_SOFT
                    #pragma multi_compile_fog

                    #pragma vertex CelShadingVertex
                    #pragma fragment CelShadingFragment

                    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
                    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

                    struct Attributes
                    {
                        float4 positionOS : POSITION;
                        float3 normalOS : NORMAL;
                        float2 uv : TEXCOORD0;
                    };

                    struct Varyings
                    {
                        float4 positionCS : SV_POSITION;
                        float2 uv : TEXCOORD0;
                        float3 normalWS : NORMAL;
                        float3 positionWS : TEXCOORD1;
                        float fogFactor : TEXCOORD2;
                    };

                    TEXTURE2D(_MainTex);
                    SAMPLER(sampler_MainTex);
                    TEXTURE2D(_SampleTexture2D);
                    SAMPLER(sampler_SampleTexture2D);

                    CBUFFER_START(UnityPerMaterial)
                        float4 _MainTex_ST;
                        float4 _Color;
                        float _LightingRamp;
                        float4 _ShadowColor;
                        float4 _OutlineColor;
                        float _OutlineThickness;
                        float _WorldBoundsMagnitude;
                    CBUFFER_END

                    Varyings CelShadingVertex(Attributes input)
                    {
                        Varyings output = (Varyings)0;

                        // Transformaciones estándar para URP
                        VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                        VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS);

                        output.positionCS = vertexInput.positionCS;
                        output.positionWS = vertexInput.positionWS;
                        output.normalWS = normalInput.normalWS;
                        output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                        output.fogFactor = ComputeFogFactor(vertexInput.positionCS.z);

                        return output;
                    }

                    half4 CelShadingFragment(Varyings input) : SV_Target
                    {
                        // Muestra la textura base
                        half4 albedo = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv) * _Color;
                        half4 textureSample = SAMPLE_TEXTURE2D(_SampleTexture2D, sampler_SampleTexture2D, input.uv);

                        // Obtener información de luz para URP
                        Light mainLight = GetMainLight();
                        float3 normalizedLightDir = normalize(mainLight.direction);
                        float NdotL = dot(normalize(input.normalWS), normalizedLightDir);

                        // Cálculo de sombras para URP
                        float shadowAttenuation = 1.0;
                        #if defined(_MAIN_LIGHT_SHADOWS) || defined(_MAIN_LIGHT_SHADOWS_CASCADE)
                            shadowAttenuation = mainLight.shadowAttenuation;
                        #endif

                            // Aplica cel shading con step function
                            float lightIntensity = step(_LightingRamp, NdotL * shadowAttenuation);

                            // Combina el color base con el color de sombra según la intensidad de la luz
                            half3 finalColor = lerp(albedo.rgb * _ShadowColor.rgb, albedo.rgb, lightIntensity);

                            // Aplica textura adicional si es necesario
                            finalColor *= textureSample.rgb;

                            // Aplicar niebla
                            float fogFactor = input.fogFactor;
                            finalColor = MixFog(finalColor, fogFactor);

                            return half4(finalColor, albedo.a);
                        }
                        ENDHLSL
                    }

                // Paso 2: Outline
                Pass
                {
                    Name "Outline"
                    Tags { "LightMode" = "SRPDefaultUnlit" }
                    Cull Front

                    HLSLPROGRAM
                    #pragma target 4.5
                    #pragma vertex OutlineVertex
                    #pragma fragment OutlineFragment

                    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

                    struct Attributes
                    {
                        float4 positionOS : POSITION;
                        float3 normalOS : NORMAL;
                    };

                    struct Varyings
                    {
                        float4 positionCS : SV_POSITION;
                    };

                    CBUFFER_START(UnityPerMaterial)
                        float4 _OutlineColor;
                        float _OutlineThickness;
                        float _WorldBoundsMagnitude;
                    CBUFFER_END

                    Varyings OutlineVertex(Attributes input)
                    {
                        Varyings output;

                        float3 normalOS = normalize(input.normalOS);
                        float3 posOS = input.positionOS.xyz;

                        // Calcula el desplazamiento del outline
                        float3 offset = normalOS * _OutlineThickness * _WorldBoundsMagnitude;
                        posOS += offset;

                        // Transforma a espacio de clip
                        output.positionCS = TransformObjectToHClip(posOS);

                        return output;
                    }

                    half4 OutlineFragment(Varyings input) : SV_Target
                    {
                        return _OutlineColor;
                    }
                    ENDHLSL
                }

                            // Pases de sombra
                            UsePass "Universal Render Pipeline/Lit/ShadowCaster"
                            UsePass "Universal Render Pipeline/Lit/DepthOnly"
            }
}