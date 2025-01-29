// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

//Copyright (c) 2014 Tilman Schmidt (@KeyMaster_)

//Permission is hereby granted, free of charge, to any person obtaining a copy
//of this software and associated documentation files (the "Software"), to deal
//in the Software without restriction, including without limitation the rights
//to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//copies of the Software, and to permit persons to whom the Software is
//furnished to do so, subject to the following conditions:

//The above copyright notice and this permission notice shall be included in
//all copies or substantial portions of the Software.

//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
//THE SOFTWARE.
Shader "Sprites/LitGlitch"
{
    Properties
    {
        [PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
        _Color("Tint", Color) = (1,1,1,1)
        _GlitchInterval("Glitch interval time [seconds]", Float) = 0.16
        _DispProbability("Displacement Glitch Probability", Float) = 0.022
        _DispIntensity("Displacement Glitch Intensity", Float) = 0.09
        _ColorProbability("Color Glitch Probability", Float) = 0.02
        _ColorIntensity("Color Glitch Intensity", Float) = 0.07
        [MaterialToggle] _DispGlitchOn("Displacement Glitch On", Float) = 1
        [MaterialToggle] _ColorGlitchOn("Color Glitch On", Float) = 1
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
        Lighting On
        ZWrite Off
        Blend One OneMinusSrcAlpha

        Pass
        {
            Tags { "LightMode"="Universal2D" }
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #pragma multi_compile _ _PIXELSNAP_ON
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                half2 texcoord : TEXCOORD0;
                half4 color : COLOR;
                float4 worldPosition : TEXCOORD1;
            };
            
            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            float4 _MainTex_ST;
            half4 _Color;
            float _GlitchInterval;
            float _DispIntensity;
            float _DispProbability;
            float _ColorIntensity;
            float _ColorProbability;
            float _DispGlitchOn;
            float _ColorGlitchOn;

            float rand(float x, float y)
            {
                return frac(sin(x * 12.9898 + y * 78.233) * 43758.5453);
            }

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                OUT.vertex = TransformObjectToHClip(IN.vertex.xyz);
                OUT.texcoord = TRANSFORM_TEX(IN.texcoord, _MainTex);
                OUT.color = IN.color * _Color;
                OUT.worldPosition = mul(unity_ObjectToWorld, IN.vertex);
                return OUT;
            }

            half4 frag(v2f IN) : SV_Target
            {
                float intervalTime = floor(_Time.y / _GlitchInterval) * _GlitchInterval;
                float timePositionVal = intervalTime + IN.worldPosition.x + IN.worldPosition.y;

                float dispGlitchRandom = rand(timePositionVal, -timePositionVal);
                float colorGlitchRandom = rand(timePositionVal, timePositionVal);

                if (dispGlitchRandom < _DispProbability && _DispGlitchOn == 1)
                {
                    IN.texcoord.x += (rand(IN.texcoord.y - timePositionVal, IN.texcoord.y + timePositionVal) - 0.5) * _DispIntensity;
                    IN.texcoord.x = clamp(IN.texcoord.x, 0, 1);
                }
                
                half4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.texcoord) * IN.color;
                col *= col.a;
                
                Light mainLight = GetMainLight();
                col.rgb *= mainLight.color.rgb * max(0.0, dot(mainLight.direction, float3(0, 0, 1)));
                
                return col;
            }
            ENDHLSL
        }
    }
}
