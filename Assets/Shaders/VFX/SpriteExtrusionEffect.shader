Shader "Custom/SpriteExtrusionEffect"
{
    Properties
    {
        _MainTex("Sprite Texture", 2D) = "white" {}
        _Color("Tint", Color) = (1,1,1,1)
        _ExtrusionHeight("Extrusion Height", Range(0, 10)) = 1.0
        _NoiseTexture("Noise Texture", 2D) = "white" {}
        _NoiseSpeed("Noise Speed", Vector) = (0, 1, 0, 0)
        _NoiseStrength("Noise Strength", Range(0, 1)) = 0.5
        _EffectColor1("Effect Color 1", Color) = (1, 0.5, 0, 1)
        _EffectColor2("Effect Color 2", Color) = (1, 0, 0, 0)
        _AlphaCutoff("Alpha Cutoff", Range(0, 1)) = 0.1
    }

        SubShader
        {
            Tags
            {
                "Queue" = "Transparent"
                "IgnoreProjector" = "True"
                "RenderType" = "Transparent"
                "PreviewType" = "Plane"
                "CanUseSpriteAtlas" = "True"
            }

            Cull Off
            Lighting Off
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha

            // Pasada 1: Renderizar el sprite original
            Pass
            {
                CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                #include "UnityCG.cginc"

                struct appdata
                {
                    float4 vertex : POSITION;
                    float4 color : COLOR;
                    float2 texcoord : TEXCOORD0;
                };

                struct v2f
                {
                    float4 vertex : SV_POSITION;
                    fixed4 color : COLOR;
                    float2 texcoord : TEXCOORD0;
                };

                sampler2D _MainTex;
                fixed4 _Color;
                float _AlphaCutoff;

                v2f vert(appdata v)
                {
                    v2f o;
                    o.vertex = UnityObjectToClipPos(v.vertex);
                    o.texcoord = v.texcoord;
                    o.color = v.color * _Color;
                    return o;
                }

                fixed4 frag(v2f i) : SV_Target
                {
                    fixed4 c = tex2D(_MainTex, i.texcoord) * i.color;
                    clip(c.a - _AlphaCutoff);
                    return c;
                }
                ENDCG
            }

            // Pasada 2: Generar el efecto perpendicular
            Pass
            {
                Blend SrcAlpha One // Aditivo para efecto de fuego/brillo
                Cull Back

                CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                #pragma geometry geom
                #include "UnityCG.cginc"

                struct appdata
                {
                    float4 vertex : POSITION;
                    float2 texcoord : TEXCOORD0;
                    float alpha : TEXCOORD1; // Para almacenar el valor alfa
                };

                struct v2f
                {
                    float4 vertex : SV_POSITION;
                    float2 texcoord : TEXCOORD0;
                    float4 color : COLOR;
                };

                sampler2D _MainTex;
                sampler2D _NoiseTexture;
                float4 _NoiseSpeed;
                float _NoiseStrength;
                float _ExtrusionHeight;
                fixed4 _EffectColor1;
                fixed4 _EffectColor2;
                float _AlphaCutoff;

                v2f vert(appdata v)
                {
                    v2f o;
                    o.vertex = v.vertex;
                    o.texcoord = v.texcoord;

                    // Muestrear la textura y guardar el alpha
                    float4 texColor = tex2D(_MainTex, v.texcoord);
                    v.alpha = texColor.a; // Guardar el alpha para usarlo en geom

                    o.color = float4(1, 1, 1, 1);
                    return o;
                }

                // Generación de la geometría para el efecto
                [maxvertexcount(24)]
                void geom(triangle appdata input[3], inout TriangleStream<v2f> triStream)
                {
                    float3 normal = float3(0, 0, -1); // Normal del sprite (hacia la cámara)
                    float3 directionUp = float3(0, 0, 1); // Dirección perpendicular al sprite

                    // Procesar cada vértice del triángulo
                    for (int i = 0; i < 3; i++)
                    {
                        appdata p = input[i];

                        // Verificar si el vértice forma parte de la silueta
                        // (simplificado - en un shader real necesitaríamos más lógica aquí)
                        float4 basePos = p.vertex;
                        float2 uv = p.texcoord;
                        

                        if (p.alpha > _AlphaCutoff)
                        {
                            // Generar cuadriláteros extruidos
                            v2f o[4];

                            // Base
                            o[0].vertex = UnityObjectToClipPos(basePos);
                            o[0].texcoord = uv;
                            o[0].color = _EffectColor1;

                            // Punta extruida
                            float4 extrudedPos = basePos + float4(directionUp * _ExtrusionHeight, 0);
                            o[1].vertex = UnityObjectToClipPos(extrudedPos);
                            o[1].texcoord = uv;
                            o[1].color = _EffectColor2;

                            // Para el siguiente vértice (formando un cuadrilátero)
                            int nextIdx = (i + 1) % 3;
                            float4 baseNextPos = input[nextIdx].vertex;
                            float2 nextUV = input[nextIdx].texcoord;

                            // Base del siguiente
                            o[2].vertex = UnityObjectToClipPos(baseNextPos);
                            o[2].texcoord = nextUV;
                            o[2].color = _EffectColor1;

                            // Punta extruida del siguiente
                            float4 extrudedNextPos = baseNextPos + float4(directionUp * _ExtrusionHeight, 0);
                            o[3].vertex = UnityObjectToClipPos(extrudedNextPos);
                            o[3].texcoord = nextUV;
                            o[3].color = _EffectColor2;

                            // Añadir vértices al stream
                            triStream.Append(o[0]);
                            triStream.Append(o[1]);
                            triStream.Append(o[2]);
                            triStream.Append(o[3]);
                            triStream.RestartStrip();
                        }
                    }
                }

                fixed4 frag(v2f i) : SV_Target
                {
                    // Añadir efecto de ruido para simular movimiento (como llamas)
                    float2 noiseUV = i.texcoord + _Time.y * _NoiseSpeed.xy;
                    float noise = tex2D(_NoiseTexture, noiseUV).r * _NoiseStrength;

                    // Color con degradado y ruido
                    fixed4 col = lerp(i.color, i.color * noise, noise);

                    // Atenuación basada en la distancia a la base
                    float heightGradient = 1.0 - i.texcoord.y;
                    col.a *= heightGradient;

                    return col;
                }
                ENDCG
            }
        }
            Fallback "Sprites/Default"
}