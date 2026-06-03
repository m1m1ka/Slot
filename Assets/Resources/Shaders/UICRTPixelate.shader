Shader "UI/CRT Pixelate"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        _PixelColumns ("Pixel Columns", Range(32, 640)) = 240
        _PixelRows ("Pixel Rows", Range(32, 480)) = 160
        _PixelStrength ("Pixel Strength", Range(0, 1)) = 1

        _ScanlineCount ("Scanline Count", Range(64, 720)) = 240
        _ScanlineColor ("Scanline Color", Color) = (0.08,0.95,0.75,1)
        _ScanlineBrightness ("Scanline Brightness", Range(0, 2)) = 0.55
        _ScanlineStrength ("Scanline Strength", Range(0, 1)) = 0.32
        _ScanlineThickness ("Scanline Thickness", Range(0.05, 0.95)) = 0.45
        _ScanlineSpeed ("Scanline Speed", Range(-5, 5)) = 0.2
        _GlowColor ("Glow Color", Color) = (0.35,0.95,0.8,1)
        _GlowStrength ("Glow Strength", Range(0, 1.5)) = 0.12

        _ChromaticAberration ("RGB Split", Range(0, 0.02)) = 0.003
        _Curvature ("Curvature", Range(0, 0.35)) = 0.08
        _VignetteStrength ("Vignette Strength", Range(0, 1)) = 0.35
        _VignetteSoftness ("Vignette Softness", Range(0.1, 1.5)) = 0.65

        _NoiseStrength ("Noise Strength", Range(0, 0.25)) = 0.035
        _FlickerStrength ("Flicker Strength", Range(0, 0.25)) = 0.035
        _Brightness ("Brightness", Range(0, 2)) = 1.05
        _Contrast ("Contrast", Range(0, 2)) = 1.08
        _Saturation ("Saturation", Range(0, 2)) = 1.08

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
            Name "CRTPixelate"
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
            fixed4 _Color;
            fixed4 _TextureSampleAdd;
            float4 _ClipRect;

            float _PixelColumns;
            float _PixelRows;
            float _PixelStrength;
            float _ScanlineCount;
            fixed4 _ScanlineColor;
            float _ScanlineBrightness;
            float _ScanlineStrength;
            float _ScanlineThickness;
            float _ScanlineSpeed;
            fixed4 _GlowColor;
            float _GlowStrength;
            float _ChromaticAberration;
            float _Curvature;
            float _VignetteStrength;
            float _VignetteSoftness;
            float _NoiseStrength;
            float _FlickerStrength;
            float _Brightness;
            float _Contrast;
            float _Saturation;

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

            float Hash12(float2 value)
            {
                float3 p = frac(float3(value.xyx) * 0.1031);
                p += dot(p, p.yzx + 33.33);
                return frac((p.x + p.y) * p.z);
            }

            float2 ApplyCurvature(float2 uv)
            {
                float2 centered = uv * 2.0 - 1.0;
                float radius = dot(centered, centered);
                centered *= 1.0 + radius * _Curvature;
                return centered * 0.5 + 0.5;
            }

            float Inside01(float2 uv)
            {
                float2 lower = step(0.0, uv);
                float2 upper = step(uv, 1.0);
                return lower.x * lower.y * upper.x * upper.y;
            }

            fixed4 SampleSprite(float2 uv)
            {
                return tex2D(_MainTex, uv) + _TextureSampleAdd;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                float2 curvedUv = ApplyCurvature(IN.texcoord);
                float inside = Inside01(curvedUv);

                float2 pixelGrid = max(float2(_PixelColumns, _PixelRows), float2(1.0, 1.0));
                float2 pixelUv = (floor(curvedUv * pixelGrid) + 0.5) / pixelGrid;
                float2 sampleUv = lerp(curvedUv, pixelUv, saturate(_PixelStrength));

                float split = _ChromaticAberration;
                fixed4 baseColor = SampleSprite(sampleUv);
                fixed4 redSample = SampleSprite(sampleUv + float2(split, 0.0));
                fixed4 blueSample = SampleSprite(sampleUv - float2(split, 0.0));

                fixed4 color = baseColor;
                color.r = redSample.r;
                color.b = blueSample.b;
                color *= IN.color;

                float scanPhase = sampleUv.y * _ScanlineCount + _Time.y * _ScanlineSpeed;
                float scanWave = abs(frac(scanPhase) - 0.5) * 2.0;
                float scanMask = smoothstep(_ScanlineThickness, 1.0, scanWave);
                float scanlineAmount = scanMask * _ScanlineStrength;
                fixed3 scanlineColor = color.rgb * _ScanlineColor.rgb * _ScanlineBrightness;
                color.rgb = lerp(color.rgb, scanlineColor, scanlineAmount);

                float glowAmount = (1.0 - scanMask) * _GlowStrength;
                color.rgb += _GlowColor.rgb * glowAmount * color.a;

                float2 vignetteUv = IN.texcoord * (1.0 - IN.texcoord.yx);
                float vignette = pow(saturate(vignetteUv.x * vignetteUv.y * 16.0), _VignetteSoftness);
                color.rgb *= lerp(1.0 - _VignetteStrength, 1.0, vignette);

                float noise = Hash12(pixelUv * pixelGrid + floor(_Time.y * 60.0));
                color.rgb += (noise - 0.5) * _NoiseStrength;

                float flicker = Hash12(float2(floor(_Time.y * 30.0), 17.0));
                color.rgb *= 1.0 + (flicker - 0.5) * _FlickerStrength;

                float luminance = dot(color.rgb, fixed3(0.299, 0.587, 0.114));
                color.rgb = lerp(fixed3(luminance, luminance, luminance), color.rgb, _Saturation);
                color.rgb = (color.rgb - 0.5) * _Contrast + 0.5;
                color.rgb *= _Brightness;
                color.rgb = saturate(color.rgb);
                color.a *= inside;

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
