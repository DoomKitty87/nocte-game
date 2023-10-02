Shader "GrassShader"
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
        _TrampleIntensity("Trample Intensity", Float) = 1
        _ScaleRandomization("Scale Randomization", Float) = 0.5
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" }
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
            uniform float4 _PlayerPosition;

            float4 _Color1, _Color2, _AOColor, _TipColor;
            float _Scale, _DisplacementAmplitude, _WindIntensity, _TrampleRadius, _ScaleRandomization;
            sampler2D _Wind;
            float4 RotateAroundYInDegrees (float4 vertex, float degrees) {
                float alpha = degrees * UNITY_PI / 180.0;
                float sina, cosa;
                sincos(alpha, sina, cosa);
                float2x2 m = float2x2(cosa, -sina, sina, cosa);
                return float4(mul(m, vertex.xz), vertex.yw).xzyw;
            };
            v2f vert(appdata_base v, uint svInstanceID : SV_InstanceID)
            {
                uint instanceID = GetIndirectInstanceID(svInstanceID);
                uint cmdID = GetCommandID(0);
                float4 pos = _PositionsBuffer[instanceID].position;
                InitIndirectDrawArgs(0);
                v2f o;
                float posHash = (pos.x * 219.87) % 1 * (pos.z * 2140.21 % 1) * (dot(pos, pos + 23.6f) % 1);
                float scale = _Scale * (1 + posHash * _ScaleRandomization);

                // 7 is height of tallest vertex (change if mesh changes)
                float uvy = v.vertex.y / scale / 7;
                float2 trampleVector = float2((pos.x - _PlayerPosition.x), (pos.z - _PlayerPosition.z));
                float trampleVectorScaler = 1 / sqrt(trampleVector.x * trampleVector.x + trampleVector.y * trampleVector.y);
                float2 trampleVectorNormalized = float2((trampleVector.x / trampleVectorScaler), (trampleVector.y / trampleVectorScaler));
                float trampleScaler = 1 / (pow(distance(pos.xz, _PlayerPosition.xz), 5));
                if (trampleScaler > .1) trampleScaler = .1;
                // float trampleValue = lerp(0.15f, 1, min((abs(_PlayerPosition.x - pos.x) + abs(_PlayerPosition.z - pos.z)) / 2 / _TrampleRadius, 1));
                float movement = uvy * uvy * (sin(tex2Dlod(_Wind, float4(_PositionsBuffer[instanceID].uv, 0, 0)).r)) * lerp(0.5f, 1.0f, abs(posHash)) * _WindIntensity;
                // float2 trampleMovement = uvy * uvy * float4(_PositionsBuffer[instanceID].uv, 0, 0) * trampleVector * trampleScaler * _TrampleIntensity;

                float4 lpos = RotateAroundYInDegrees(float4(
                    v.vertex.x * scale, // + (trampleVector.x), * trampleScaler.x),
                    v.vertex.y * scale, // * lerp(trampleValue, 1, abs(_PlayerPosition.y - pos.y) / (7 * scale * 2)), 
                    v.vertex.z * scale, // + (trampleVector.y * trampleScaler),
                    v.vertex.w), posHash * 180.0f);
                
                float2 trample = (uvy * trampleVectorNormalized.x * trampleScaler, uvy * trampleVectorNormalized.y * trampleScaler);
                float4 wpos = mul(_ObjectToWorld, (float4(lpos.x + movement + trample.x, lpos.y, lpos.z + movement + trample.y, lpos.w)) + pos);

                o.pos = mul(UNITY_MATRIX_VP, wpos);
                o.color = (lerp(_Color1, _Color2, uvy) + lerp(0.0f, _TipColor, uvy * uvy * (1.0f * scale))) * lerp(_AOColor, 1.0f, uvy);
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