Shader "ExampleShader"
{
    Properties 
    {
        _Albedo1 ("Albedo 1", Color) = (1, 1, 1)
        _Albedo2 ("Albedo 2", Color) = (1, 1, 1)
        _AOColor ("Ambient Occlusion", Color) = (1, 1, 1)
        _TipColor ("Tip Color", Color) = (1, 1, 1)
    }
    SubShader
    {
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #define UNITY_INDIRECT_DRAW_ARGS IndirectDrawIndexedArgs
            #include "UnityIndirect.cginc"

            struct VertexData
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };
            
            struct v2f
            {
                float2 uv : TEXCOORD0;
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
            StructuredBuffer<GrassData> _grassData;
            int _numberOfInstances;
            float4 _Albedo1, _Albedo2, _AOColor, _TipColor;

            float4 RotateAroundXInDegrees (float4 vertex, float degrees) {
                float alpha = degrees * UNITY_PI / 180.0;
                float sina, cosa;
                sincos(alpha, sina, cosa);
                float2x2 m = float2x2(cosa, -sina, sina, cosa);
                return float4(mul(m, vertex.yz), vertex.xw).zxyw;
            }
            
            v2f vert(VertexData v, uint svInstanceID : SV_InstanceID)
            {
                InitIndirectDrawArgs(0);
                v2f o;
                uint cmdID = GetCommandID(0);
                uint instanceID = GetIndirectInstanceID(svInstanceID);

                float4 instancePosition = _grassData[instanceID].position;
                float4 localPosition = RotateAroundXInDegrees(v.vertex, 90.0f);
                
                float4 wpos = mul(_ObjectToWorld, v.vertex + float4(instanceID % _numberOfInstances * 1.5, floor(instanceID / (_numberOfInstances * _numberOfInstances)) * 1.5, (floor(instanceID / _numberOfInstances) % _numberOfInstances) * 1.5, 0));
                o.pos = mul(UNITY_MATRIX_VP, wpos);
                o.uv = v.uv;
                o.color = float4(cmdID & 1 ? 0.0f : 1.0f, cmdID & 1 ? 1.0f : 0.0f, instanceID / float(GetIndirectInstanceCount()), 0.0f);
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                float4 col = lerp(_Albedo1, _Albedo2, i.uv.y);
                float4 ao = lerp(_AOColor, 1.0f, i.uv.y);
                float4 tip = lerp(0.0f, _TipColor, i.uv.y * i.uv.y);
                float4 grassColor = (col + tip) * ao;
                return grassColor;
            }
            ENDCG
        }
    }
}