Shader "UI/PixelOutlineUI"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _OutlineColor ("Outline Color", Color) = (0,0,0,1)
        _PixelSize ("Pixel Size", Float) = 1.0
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" "PreviewType"="Plane" "CanUseSpriteAtlas"="False" }
        LOD 100

        Pass
        {
            Name "Default"
            Blend SrcAlpha OneMinusSrcAlpha
            Cull Off
            ZWrite Off
            ZTest Always

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _OutlineColor;
            float _PixelSize;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                float2 texelSize = float2(_PixelSize / _ScreenParams.x, _PixelSize / _ScreenParams.y);

                float alpha = tex2D(_MainTex, i.uv).a;
                float outline = 0.0;

                // 4 направления
                outline += step(0.01, tex2D(_MainTex, i.uv + float2(texelSize.x, 0)).a);
                outline += step(0.01, tex2D(_MainTex, i.uv - float2(texelSize.x, 0)).a);
                outline += step(0.01, tex2D(_MainTex, i.uv + float2(0, texelSize.y)).a);
                outline += step(0.01, tex2D(_MainTex, i.uv - float2(0, texelSize.y)).a);

                if (alpha < 0.01 && outline > 0.0)
                {
                    return _OutlineColor;
                }

                float4 col = tex2D(_MainTex, i.uv);
                return col;
            }
            ENDHLSL
        }
    }
}
