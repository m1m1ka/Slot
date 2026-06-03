Shader "UI/Solid Outline"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _OutlineColor ("Outline Color", Color) = (1,0.82,0.18,1)
        _OutlineWidth ("Outline Width", Range(0, 16)) = 3
        _OutlineAlpha ("Outline Alpha", Range(0, 1)) = 1
        _OutlineBrightness ("Outline Brightness", Range(0, 5)) = 1
        _Softness ("Softness", Range(0, 1)) = 0.15

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
            Name "SolidOutline"

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
            fixed4 _OutlineColor;
            float _OutlineWidth;
            float _OutlineAlpha;
            float _OutlineBrightness;
            float _Softness;
            float4 _ClipRect;

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

            float SampleAlpha(float2 uv)
            {
                return tex2D(_MainTex, uv).a;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                fixed4 sprite = (tex2D(_MainTex, IN.texcoord) + _TextureSampleAdd) * IN.color;
                float2 stepUv = _MainTex_TexelSize.xy * _OutlineWidth;

                float outlineAlpha = 0;
                outlineAlpha = max(outlineAlpha, SampleAlpha(IN.texcoord + float2(stepUv.x, 0)));
                outlineAlpha = max(outlineAlpha, SampleAlpha(IN.texcoord + float2(-stepUv.x, 0)));
                outlineAlpha = max(outlineAlpha, SampleAlpha(IN.texcoord + float2(0, stepUv.y)));
                outlineAlpha = max(outlineAlpha, SampleAlpha(IN.texcoord + float2(0, -stepUv.y)));
                outlineAlpha = max(outlineAlpha, SampleAlpha(IN.texcoord + stepUv));
                outlineAlpha = max(outlineAlpha, SampleAlpha(IN.texcoord - stepUv));
                outlineAlpha = max(outlineAlpha, SampleAlpha(IN.texcoord + float2(stepUv.x, -stepUv.y)));
                outlineAlpha = max(outlineAlpha, SampleAlpha(IN.texcoord + float2(-stepUv.x, stepUv.y)));

                float edge = saturate(outlineAlpha - sprite.a);
                float softness = max(0.0001, _Softness);
                edge = smoothstep(0.0, softness, edge) * _OutlineAlpha;

                fixed4 outline = _OutlineColor;
                outline.rgb *= _OutlineBrightness;
                outline.a *= edge * IN.color.a;

                fixed4 color;
                color.rgb = lerp(outline.rgb, sprite.rgb, sprite.a);
                color.a = saturate(sprite.a + outline.a);

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
