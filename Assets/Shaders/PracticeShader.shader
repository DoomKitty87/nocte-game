  Shader "Elliot/PracticeShader"
{
    Properties
    {
        _TintColor ("Tint Color", Color) = (1, 1, 1, 1)
        _Transparency("Transparency", Range(0.0, 1)) = 0.5
        _CutoutThresh("Cutout Threshold", Range(0.0, 1.0)) = 0.2
        _Amplitude("Amplitude", float) = 1
        _Speed("Speed", float) = 1
    }
    SubShader
    {
        Tags {"Queue"="Transparent" "RenderType"="Transparent" }
        LOD 100

        ZWrite Off
        Cull Off
        Blend SrcAlpha OneMinusSrcAlpha
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma randomValue

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

            float4 _MainTex_ST;
            float4 _TintColor;
            float _Transparency;
            float _CutoutThresh;

            float _Amplitude;
            float _Speed;

            float randomValue (int seed1)
            {
                return frac(sin(dot(seed1, float2(12.9898, 78.233)))*43758.5453);
            }

            
            v2f vert (appdata v)
            {
                v2f o;
                v.vertex.y += sin(_Time.y + randomValue(v.vertex.x) * _Speed) * _Amplitude;
                o.vertex = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = lerp((1, 1, 1, 0), _TintColor, i.vertex.y);
                col.a = _Transparency;
                clip(col.r - _CutoutThresh);
                return col;
            }
            ENDCG
        }
    }
}
