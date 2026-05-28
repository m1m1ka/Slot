Shader "UI/Rainbow Outline Flow"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _OutlineWidth ("Outline Width", Range(0, 12)) = 3
        _OutlineAlpha ("Outline Alpha", Range(0, 1)) = 1
        _GlowStrength ("Glow Strength", Range(0, 3)) = 0.8
        _FlowSpeed ("Flow Speed", Range(-5, 5)) = 1.3
        _FlowScale ("Flow Scale", Range(0.5, 16)) = 5
        _FlowAngle ("Flow Angle", Range(0, 360)) = 35
        _Saturation ("Saturation", Range(0, 1)) = 0.95
        _Value ("Value", Range(0, 2)) = 1.2
        _HueOffset ("Hue Offset", Range(0, 1)) = 0
        _HighlightAmount ("Highlight Amount", Range(0,1)) = 0

        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
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

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            Name "RainbowOutlineFlow"
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

            struct appdata_t
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float2 texcoord : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            fixed4 _Color;
            fixed4 _TextureSampleAdd;
            float4 _ClipRect;
            float _OutlineWidth;
            float _OutlineAlpha;
            float _GlowStrength;
            float _FlowSpeed;
            float _FlowScale;
            float _FlowAngle;
            float _Saturation;
            float _Value;
            float _HueOffset;
            float _HighlightAmount;

            fixed3 HsvToRgb(float3 hsv)
            {
                float4 k = float4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
                float3 p = abs(frac(hsv.xxx + k.xyz) * 6.0 - k.www);
                return hsv.z * lerp(k.xxx, saturate(p - k.xxx), hsv.y);
            }

            v2f vert(appdata_t v)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                OUT.worldPosition = v.vertex;
                OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);
                OUT.texcoord = v.texcoord;
                OUT.color = v.color * _Color;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                fixed4 sprite = (tex2D(_MainTex, IN.texcoord) + _TextureSampleAdd) * IN.color;
                float2 stepUv = _MainTex_TexelSize.xy * _OutlineWidth;

                float outlineAlpha = 0;
                outlineAlpha = max(outlineAlpha, tex2D(_MainTex, IN.texcoord + float2(stepUv.x, 0)).a);
                outlineAlpha = max(outlineAlpha, tex2D(_MainTex, IN.texcoord + float2(-stepUv.x, 0)).a);
                outlineAlpha = max(outlineAlpha, tex2D(_MainTex, IN.texcoord + float2(0, stepUv.y)).a);
                outlineAlpha = max(outlineAlpha, tex2D(_MainTex, IN.texcoord + float2(0, -stepUv.y)).a);
                outlineAlpha = max(outlineAlpha, tex2D(_MainTex, IN.texcoord + stepUv).a);
                outlineAlpha = max(outlineAlpha, tex2D(_MainTex, IN.texcoord - stepUv).a);
                outlineAlpha = max(outlineAlpha, tex2D(_MainTex, IN.texcoord + float2(stepUv.x, -stepUv.y)).a);
                outlineAlpha = max(outlineAlpha, tex2D(_MainTex, IN.texcoord + float2(-stepUv.x, stepUv.y)).a);
                outlineAlpha = saturate((outlineAlpha - sprite.a) * _OutlineAlpha);

                float radians = _FlowAngle * 0.01745329252;
                float2 direction = normalize(float2(cos(radians), sin(radians)));
                float flow = dot(IN.texcoord - 0.5, direction) * _FlowScale + _Time.y * _FlowSpeed;
                fixed3 rainbow = HsvToRgb(float3(frac(flow + _HueOffset), _Saturation, _Value));

                fixed3 highlightedSpriteRgb = lerp(sprite.rgb, fixed3(1, 1, 1), saturate(_HighlightAmount));
                fixed4 color = sprite;
                fixed3 outlineColor = rainbow * (1.0 + _GlowStrength);
                color.rgb = lerp(outlineColor, highlightedSpriteRgb, sprite.a);
                color.a = saturate(sprite.a + outlineAlpha * IN.color.a);

                #ifdef UNITY_UI_CLIP_RECT
                color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                clip(color.a - 0.001);
                #endif

                return color;
            }
            ENDCG
        }
    }
}
