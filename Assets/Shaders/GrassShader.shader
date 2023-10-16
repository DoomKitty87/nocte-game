Shader "GrassShader"
{
    Properties
    {
        _Color1("Color 1", Color) = (1, 1, 1, 1)
        _Color2("Color 2", Color) = (1, 1, 1, 1)
        _AOColor("Ambient Occlusion Color", Color) = (1, 1, 1, 1)
        _TipColor("Tip Color", Color) = (1, 1, 1, 1)
        _Scale("Scale", Range(0.0, 0.5)) = 0.2
        _DisplacementAmplitude("Displacement Amplitude", Range(0.0, 1.0)) = 0.0
        _WindIntensity("Wind Intensity", Range(0.0, 0.1)) = 0.02
        _TrampleIntensity("Trample Intensity", Float) = 1
        _ScaleRandomization("Scale Randomization", Float) = 0.5
    }
    SubShader
    {
        Tags { "RenderType" = "ForwardPass" }
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
            uniform float3 _PlayerPosition;

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
                float4 pos = _PositionsBuffer[instanceID].position;
                InitIndirectDrawArgs(0);
                v2f o;
                const float scale = _Scale;
                const float posHash = frac(sin(dot(pos.xz,float2(12.9898,78.233)))*43758.5453123);

                // Mesh dependant high correction
                const float uvy = v.vertex.y / 1.442;
                
                float3 trample = (1, 1, 1);
                float trampleDistance = distance(pos.xyz, _PlayerPosition);
                if (trampleDistance < 1) {
                    if (trampleDistance < 1) {
                        trample.y = trampleDistance;
                    }
                    float2 trampleVector = (pos.xz - _PlayerPosition.xz);
                    if (1 / (pow(trampleDistance, 50)) > .05) trampleDistance = .05;
                    else trampleDistance = 1 / (pow(trampleDistance, 50));
                    trample.xz = uvy * scale * (trampleVector * trampleDistance);
                }
                
                float movement = uvy * (sin(tex2Dlod(_Wind, float4(_PositionsBuffer[instanceID].uv, 0, 0)).r)) * lerp(0.5f, 1.0f, abs(posHash)) * _WindIntensity;
                float4 lpos = RotateAroundYInDegrees(float4(
                    v.vertex.x * scale,
                    v.vertex.y * scale,
                    v.vertex.z * scale,
                    v.vertex.w), posHash * 180.0f);
                
                float4 wpos = mul(_ObjectToWorld, (float4(lpos.x + movement, lpos.y, lpos.z + movement, lpos.w)) + pos);

                o.pos = mul(UNITY_MATRIX_VP, wpos);
                o.color = (lerp(_Color1, _Color2, uvy) + lerp(0.0f, _TipColor, uvy * uvy * (scale))) * lerp(_AOColor, 1.0f, uvy);
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