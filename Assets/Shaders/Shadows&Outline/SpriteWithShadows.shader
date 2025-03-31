Shader "Custom/SpriteWithOutlineAndShadows"
{
    Properties
    {
            [PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
            _Color("Tint", Color) = (1,1,1,1)
            _Cutoff("Alpha cutoff", Range(0,1)) = 0.5
            [MaterialToggle] PixelSnap("Pixel snap", Float) = 0

            // Propiedades para el outline
            [MaterialToggle] _EnableOutline("Enable Outline", Float) = 1
            [PerRendererData] _Outline("Outline", Float) = 0
            [PerRendererData] _OutlineColor("Outline Color", Color) = (1,1,1,1)
            [PerRendererData] _OutlineSize("Outline Size", int) = 1

            [HDR] _GlowColor("Glow Color", Color) = (1,1,1,0.2)
            _GlowIntensity("Glow Intensity", Range(0,1)) = 0.5
    }

        SubShader
        {
            Tags
            {
                "RenderType" = "TransparentCutout"
                "Queue" = "AlphaTest"
                "IgnoreProjector" = "True"
                "PreviewType" = "Plane"
                "CanUseSpriteAtlas" = "True"
            }

            // Pasada principal - Renderiza el sprite con outline
            Pass
            {
                Cull Off
                ZWrite On
                Blend One OneMinusSrcAlpha

                CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                #pragma multi_compile _ PIXELSNAP_ON
                #pragma shader_feature ETC1_EXTERNAL_ALPHA
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
                sampler2D _AlphaTex;
                float4 _MainTex_TexelSize;
                fixed4 _Color;
                float _Outline;
                fixed4 _OutlineColor;
                int _OutlineSize;
                float _Cutoff;
                float _EnableOutline;
                fixed4 _GlowColor;
                float _GlowIntensity;

                v2f vert(appdata v)
                {
                    v2f o;
                    o.vertex = UnityObjectToClipPos(v.vertex);
                    o.texcoord = v.texcoord;
                    o.color = v.color * _Color;
                    #ifdef PIXELSNAP_ON
                    o.vertex = UnityPixelSnap(o.vertex);
                    #endif
                    return o;
                }

                fixed4 SampleSpriteTexture(float2 uv)
                {
                    fixed4 color = tex2D(_MainTex, uv);
                    #if ETC1_EXTERNAL_ALPHA
                    color.a = tex2D(_AlphaTex, uv).r;
                    #endif
                    return color;
                }

                fixed4 frag(v2f i) : SV_Target
                {
                    fixed4 c = SampleSpriteTexture(i.texcoord) * i.color;

                // Si no hay contenido en este pixel, descartarlo
                clip(c.a - _Cutoff);

                // Verificar si el outline está habilitado tanto a nivel global como por instancia
                if (_EnableOutline > 0.5 && _Outline > 0.5 && c.a >= _Cutoff) {
                    float totalAlpha = 1.0;

                    [unroll(16)]
                    for (int j = 1; j < _OutlineSize + 1; j++) {
                        fixed4 pixelUp = tex2D(_MainTex, i.texcoord + fixed2(0, j * _MainTex_TexelSize.y));
                        fixed4 pixelDown = tex2D(_MainTex, i.texcoord - fixed2(0, j * _MainTex_TexelSize.y));
                        fixed4 pixelRight = tex2D(_MainTex, i.texcoord + fixed2(j * _MainTex_TexelSize.x, 0));
                        fixed4 pixelLeft = tex2D(_MainTex, i.texcoord - fixed2(j * _MainTex_TexelSize.x, 0));

                        totalAlpha = totalAlpha * pixelUp.a * pixelDown.a * pixelRight.a * pixelLeft.a;
                    }

                    if (totalAlpha == 0) {
                        c.rgba = fixed4(1, 1, 1, 1) * _OutlineColor;
                    }

                    c.rgb = lerp(c.rgb, _GlowColor.rgb, _GlowIntensity * _GlowColor.a);
                }

                c.rgb *= c.a;
                return c;
            }
            ENDCG
        }

            // Pasada para las sombras
            Pass
            {
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

                struct appdata
                {
                    float4 vertex : POSITION;
                    float2 texcoord : TEXCOORD0;
                    float3 normal : NORMAL;
                };

                struct v2f
                {
                    V2F_SHADOW_CASTER;
                    float2 texcoord : TEXCOORD1;
                };

                sampler2D _MainTex;
                float4 _MainTex_ST;
                float _Cutoff;

                v2f vert(appdata v)
                {
                    v2f o;
                    o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                    TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)
                    return o;
                }

                float4 frag(v2f i) : SV_Target
                {
                    fixed4 texColor = tex2D(_MainTex, i.texcoord);
                    clip(texColor.a - _Cutoff);
                    SHADOW_CASTER_FRAGMENT(i)
                }
                ENDCG
            }
        }

            Fallback "Sprites/Default"
}