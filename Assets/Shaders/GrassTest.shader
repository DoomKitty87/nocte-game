          Shader "ExampleShader"
{
    Properties
    {
        _Color1("Color 1", Color) = (1, 1, 1, 1)
        _Color2("Color 2", Color) = (1, 1, 1, 1)
        _AOColor("Ambient Occlusion Color", Color) = (1, 1, 1, 1)
        _TipColor("Tip Color", Color) = (1, 1, 1, 1)
        _Scale("Scale", Range(0.0, 2.0)) = 0.0
        _DisplacementAmplitude("Displacement Amplitude", Range(0.0, 1.0)) = 0.0
        _WindIntensity("Wind Intensity", Range(0.0, 0.1)) = 0.02
    }
    SubShader
    {
        Pass
        {
            Cull Off
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #define UNITY_INDIRECT_DRAW_ARGS IndirectDrawIndexedArgs
            #include "UnityIndirect.cginc"

            struct v2f
            {
                float4 pos : SV_POSITION;
                float4 color : COLOR0;
            };

            struct GrassData
            {
                float4 position;
                float2 uv;
                float displacement;
            };

            uniform float4x4 _ObjectToWorld;
            uniform StructuredBuffer <GrassData> _PositionsBuffer;

            float4 _Color1, _Color2, _AOColor, _TipColor;
            float _Scale, _DisplacementAmplitude, _WindIntensity;
            sampler2D _Wind;

            float4 RotateAroundYInDegrees (float4 vertex, float degrees) {
                float alpha = degrees * UNITY_PI / 180.0;
                float sina, cosa;
                sincos(alpha, sina, cosa);
                float2x2 m = float2x2(cosa, -sina, sina, cosa);
                return float4(mul(m, vertex.xz), vertex.yw).xzyw;
            }

            v2f vert(appdata_base v, uint svInstanceID : SV_InstanceID)
            {
                uint instanceID = GetIndirectInstanceID(svInstanceID);
                uint cmdID = GetCommandID(0);
                float4 pos = _PositionsBuffer[instanceID].position;
                InitIndirectDrawArgs(0);
                v2f o;
                float posHash = sin(pos.x * 257.0f) * cos(pos.y * 257.0f * 2.0f) * sin(pos.z * 257.0f * 3.0f);
                float2 displacement = (sin(posHash), cos(posHash)) * _DisplacementAmplitude;
                //7 is height of tallest vertex (change if mesh changes)
                float uvy = v.vertex.y / _Scale / 7;
                float movement = uvy * uvy * (sin(tex2Dlod(_Wind, float4(_PositionsBuffer[instanceID].uv, 0, 0)).r * _Time.y)) * abs(posHash) * _WindIntensity;
                float4 lpos = RotateAroundYInDegrees(float4(v.vertex.x * _Scale + displacement.x, v.vertex.y * _Scale, v.vertex.z + displacement.y, v.vertex.w), posHash * 180.0f);
                float4 wpos = mul(_ObjectToWorld, (float4(lpos.x + movement, lpos.y, lpos.z + movement, lpos.w)) + pos);
                o.pos = mul(UNITY_MATRIX_VP, wpos);
                o.color = (lerp(_Color1, _Color2, uvy) + lerp(0.0f, _TipColor, uvy * uvy * (1.0f * _Scale))) * lerp(_AOColor, 1.0f, uvy);
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                return i.color;
            }
            ENDCG
        }
    }
}