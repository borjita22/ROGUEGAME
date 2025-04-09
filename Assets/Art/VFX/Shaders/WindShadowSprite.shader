Shader "Custom/WindSpriteWithShadows"
{
    Properties{
        _MainTex("Texture", 2D) = "white" {}
        _WindSpeed("Wind Speed", Range(0, 10)) = 1
        _WindStrength("Wind Strength", Range(0, 1)) = 0.1
        _WindFrequency("Wind Frequency", Range(0, 10)) = 1
        _TrunkHeight("Trunk Height", Range(0, 1)) = 0.4 // Define la altura relativa del tronco (0-1)
        _HeightFalloff("Height Falloff", Range(0, 1)) = 0.1 // Suaviza la transición entre tronco y copa
        _Cutoff("Alpha cutoff", Range(0,1)) = 0.5
    }

        SubShader{
            Tags {
                "RenderType" = "TransparentCutout"
                "Queue" = "AlphaTest"
                "DisableBatching" = "True"
            }

            // Pasada principal - Renderiza el sprite con efecto de viento
            Pass {
                ZWrite On
                Cull Off
                

                CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                #include "UnityCG.cginc"

                struct appdata {
                    float4 vertex : POSITION;
                    float2 uv : TEXCOORD0;
                    float4 color : COLOR;
                };

                struct v2f {
                    float2 uv : TEXCOORD0;
                    float4 vertex : SV_POSITION;
                    float4 color : COLOR;
                };

                sampler2D _MainTex;
                float4 _MainTex_ST;
                float _WindSpeed;
                float _WindStrength;
                float _WindFrequency;
                float _TrunkHeight;
                float _HeightFalloff;
                float _Cutoff;

                // Aplica el efecto de viento a una posición de vértice
                float4 ApplyWind(float4 vertexPos)
                {
                    // Normaliza la posición Y del vértice
                    float normalizedHeight = vertexPos.y;

                    // Factor basado en altura
                    float heightFactor = smoothstep(_TrunkHeight - _HeightFalloff,
                                                  _TrunkHeight + _HeightFalloff,
                                                  normalizedHeight);

                    // Efecto de viento
                    float windTime = _Time.y * _WindSpeed;
                    float windSwing = sin(windTime + vertexPos.x * _WindFrequency);

                    // Aplica el efecto según la altura
                    vertexPos.x += windSwing * _WindStrength * heightFactor;
                    vertexPos.y += windSwing * _WindStrength * 0.2 * heightFactor;

                    return vertexPos;
                }

                v2f vert(appdata v) {
                    v2f o;

                    // Aplica el efecto de viento
                    v.vertex = ApplyWind(v.vertex);

                    o.vertex = UnityObjectToClipPos(v.vertex);
                    o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                    o.color = v.color;
                    return o;
                }

                fixed4 frag(v2f i) : SV_Target {
                    fixed4 col = tex2D(_MainTex, i.uv) * i.color;
                    clip(col.a - _Cutoff);
                    return col;
                }
                ENDCG
            }

            // Pasada para las sombras
            Pass {
                Name "ShadowCaster"
                Tags { "LightMode" = "ShadowCaster" }

                ZWrite On
                ZTest LEqual
                Cull Off

                CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                #pragma multi_compile_shadowcaster
                #include "UnityCG.cginc"

                struct appdata {
                    float4 vertex : POSITION;
                    float2 uv : TEXCOORD0;
                    float3 normal : NORMAL;
                };

                struct v2f {
                    V2F_SHADOW_CASTER;
                    float2 uv : TEXCOORD1;
                };

                sampler2D _MainTex;
                float4 _MainTex_ST;
                float _Cutoff;
                float _WindSpeed;
                float _WindStrength;
                float _WindFrequency;
                float _TrunkHeight;
                float _HeightFalloff;

                // Duplicado de la función de viento para mantener consistencia
                float4 ApplyWind(float4 vertexPos)
                {
                    float normalizedHeight = vertexPos.y;
                    float heightFactor = smoothstep(_TrunkHeight - _HeightFalloff,
                                                  _TrunkHeight + _HeightFalloff,
                                                  normalizedHeight);

                    float windTime = _Time.y * _WindSpeed;
                    float windSwing = sin(windTime + vertexPos.x * _WindFrequency);

                    vertexPos.x += windSwing * _WindStrength * heightFactor;
                    vertexPos.y += windSwing * _WindStrength * 0.2 * heightFactor;

                    return vertexPos;
                }

                v2f vert(appdata v)
                {
                    v2f o;

                    // Aplica el mismo efecto de viento para consistencia
                    v.vertex = ApplyWind(v.vertex);

                    o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                    TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)
                    return o;
                }

                float4 frag(v2f i) : SV_Target
                {
                    fixed4 texColor = tex2D(_MainTex, i.uv);
                    clip(texColor.a - _Cutoff);

                    SHADOW_CASTER_FRAGMENT(i)
                }
                ENDCG
            }
        }

            Fallback "Sprites/Default"
}