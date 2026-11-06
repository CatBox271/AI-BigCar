// 日冕横向渐变 · Corona Horizontal Fade（极简版）
// 只有光，没有噪点纹理：左边白热 -> 向右衰减到透明，纵向有很轻的层次起伏。
Shader "Sprites/Corona Horizontal Fade"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("颜色 Color", Color) = (1, 0.72, 0.36, 1)
        [Toggle] _Mirror ("镜像反转 Mirror", Float) = 0
        _FadeLength ("渐变消失长度 Fade Length", Range(0.001, 1)) = 0.9
        _Falloff ("衰减曲线 Falloff", Range(0.3, 4)) = 1.6
        _CorePow ("核心白热形状 Core Power", Range(0.5, 6)) = 2.2
        _CoreAmount ("核心白热强度 Core Amount", Range(0, 1)) = 1
        _Layers ("纵向层次 Layers", Range(0.0, 2)) = 0.35
        _Speed ("呼吸速度 Breathing", Range(0, 2)) = 0.35
        _Brightness ("亮度 Brightness", Range(0, 8)) = 1.7
        _EdgeSoftness ("上下柔化 Edge Softness", Range(0.001, 0.5)) = 0.03

        [HideInInspector] _RendererColor ("RendererColor", Color) = (1,1,1,1)
        [HideInInspector] _Flip ("Flip", Vector) = (1,1,1,1)
        [HideInInspector] _UVRect ("Sprite UV Rect", Vector) = (0,0,1,1)
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend One OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0

            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 uv       : TEXCOORD0;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _UVRect;
            fixed4 _Color;
            fixed4 _RendererColor;
            float4 _Flip;

            float _Mirror;
            float _FadeLength;
            float _Falloff;
            float _CorePow;
            float _CoreAmount;
            float _Layers;
            float _Speed;
            float _Brightness;
            float _EdgeSoftness;

            v2f vert (appdata_t IN)
            {
                v2f OUT;
                IN.vertex.xy *= _Flip.xy;
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.uv = TRANSFORM_TEX(IN.texcoord, _MainTex);
                OUT.color = IN.color * _Color * _RendererColor;
                return OUT;
            }

            fixed4 frag (v2f IN) : SV_Target
            {
                float2 uvR = (IN.uv - _UVRect.xy) / max(_UVRect.zw, float2(1e-4, 1e-4));
                float2 uv = saturate(uvR);
                if (_Mirror > 0.5) uv.x = 1.0 - uv.x;

                float t = _Time.y * _Speed;

                // 横向：左边满、向右衰减
                float lateral = saturate((1.0 - uv.x) / max(_FadeLength, 1e-4));
                float glow = pow(lateral, _Falloff);

                // 纵向层次：很轻的起伏，让上下不是一条平线
                float y = uv.y * 6.2831853;
                float layers = 0.5
                             + 0.5 * sin(y + t * 1.7)
                             * 0.6
                             + 0.4 * sin(y * 2.3 - t * 1.1);
                layers = lerp(1.0, saturate(layers), _Layers);

                // 呼吸（整体亮度缓慢起伏）
                float breath = 0.92 + 0.08 * sin(t * 2.1);

                // 不加上下淡出：效果必须铺满整个 sprite，否则边缘会缺一条
                float edge = 1.0;

                float cover = saturate(glow * layers * breath * edge);

                // 颜色：基色，核心处烧白
                float3 base = IN.color.rgb;
                float coreAmount = pow(cover, _CorePow) * _CoreAmount;
                float3 col = lerp(base, float3(1.0, 0.97, 0.92), coreAmount);

                float3 rgb = col * cover * _Brightness;   // 预乘

                fixed4 tex = tex2D(_MainTex, IN.uv);
                rgb *= tex.a;
                // 预乘混合下 alpha 必须跟着亮度走，否则 _Brightness=0 会画成一块黑
                float a = cover * tex.a * saturate(_Brightness);

                return fixed4(rgb, a);
            }
            ENDCG
        }
    }

    Fallback "Sprites/Default"
}
