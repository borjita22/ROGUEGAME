Shader "Hidden/CelShadingEffect"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _Levels("Cel Shading Levels", Range(2, 8)) = 3
        _ShadowThreshold("Shadow Threshold", Range(0, 1)) = 0.5
        _ShadowSmoothness("Shadow Smoothness", Range(0, 0.5)) = 0.01
        _OutlineThickness("Outline Thickness", Range(0, 5)) = 1
        _OutlineColor("Outline Color", Color) = (0,0,0,1)
    }
        SubShader
        {
            // No culling or depth
            Cull Off ZWrite Off ZTest Always

            Pass
            {
                CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag

                #include "UnityCG.cginc"

                struct appdata
                {
                    float4 vertex : POSITION;
                    float2 uv : TEXCOORD0;
                };

                struct v2f
                {
                    float2 uv : TEXCOORD0;
                    float4 vertex : SV_POSITION;
                };

                v2f vert(appdata v)
                {
                    v2f o;
                    o.vertex = UnityObjectToClipPos(v.vertex);
                    o.uv = v.uv;
                    return o;
                }

                sampler2D _MainTex;
                sampler2D _CameraDepthNormalsTexture;
                float _Levels;
                float _ShadowThreshold;
                float _ShadowSmoothness;
                float _OutlineThickness;
                float4 _OutlineColor;
                float4 _MainTex_TexelSize;

                float3 Posterize(float3 color, float levels)
                {
                    return floor(color * levels) / levels;
                }

                float SobelEdge(float2 uv)
                {
                    float2 delta = _MainTex_TexelSize.xy * _OutlineThickness;

                    // Sobel operator for edge detection
                    float3 sobelX = 0;
                    sobelX += tex2D(_CameraDepthNormalsTexture, uv + float2(-delta.x, -delta.y)).rgb * -1.0;
                    sobelX += tex2D(_CameraDepthNormalsTexture, uv + float2(-delta.x, 0)).rgb * -2.0;
                    sobelX += tex2D(_CameraDepthNormalsTexture, uv + float2(-delta.x, delta.y)).rgb * -1.0;
                    sobelX += tex2D(_CameraDepthNormalsTexture, uv + float2(delta.x, -delta.y)).rgb * 1.0;
                    sobelX += tex2D(_CameraDepthNormalsTexture, uv + float2(delta.x, 0)).rgb * 2.0;
                    sobelX += tex2D(_CameraDepthNormalsTexture, uv + float2(delta.x, delta.y)).rgb * 1.0;

                    float3 sobelY = 0;
                    sobelY += tex2D(_CameraDepthNormalsTexture, uv + float2(-delta.x, -delta.y)).rgb * -1.0;
                    sobelY += tex2D(_CameraDepthNormalsTexture, uv + float2(0, -delta.y)).rgb * -2.0;
                    sobelY += tex2D(_CameraDepthNormalsTexture, uv + float2(delta.x, -delta.y)).rgb * -1.0;
                    sobelY += tex2D(_CameraDepthNormalsTexture, uv + float2(-delta.x, delta.y)).rgb * 1.0;
                    sobelY += tex2D(_CameraDepthNormalsTexture, uv + float2(0, delta.y)).rgb * 2.0;
                    sobelY += tex2D(_CameraDepthNormalsTexture, uv + float2(delta.x, delta.y)).rgb * 1.0;

                    float edge = sqrt(dot(sobelX, sobelX) + dot(sobelY, sobelY));
                    return edge;
                }

                float4 frag(v2f i) : SV_Target
                {
                    float4 col = tex2D(_MainTex, i.uv);
                    float edge = SobelEdge(i.uv);

                    // Cel shading
                    float3 celColor = Posterize(col.rgb, _Levels);

                    // Shadow effect
                    float luminance = dot(celColor, float3(0.299, 0.587, 0.114));
                    float shadow = smoothstep(_ShadowThreshold - _ShadowSmoothness, _ShadowThreshold + _ShadowSmoothness, luminance);
                    celColor = lerp(celColor * 0.5, celColor, shadow);

                    // Outline
                    float3 finalColor = lerp(_OutlineColor.rgb, celColor, 1 - step(0.8, edge));

                    return float4(finalColor, col.a);
                }
                ENDCG
            }
        }
}
