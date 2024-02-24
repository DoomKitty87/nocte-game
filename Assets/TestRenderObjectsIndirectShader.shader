Shader "ExampleShader"
{
    Properties
    {
        _Color ("Color", Color) = (0, 0, 0, 0)
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

            struct v2f
            {
                float4 pos : SV_POSITION;
                float4 color : COLOR0;
            };

            fixed4 _Color;

            uniform float4x4 _ObjectToWorld;

            float3 _Positions[1000];

            v2f vert(appdata_base v, uint svInstanceID : SV_InstanceID)
            {
                InitIndirectDrawArgs(0);
                v2f o;
                uint instanceID = GetIndirectInstanceID(svInstanceID);
                const float4 wpos = mul(_ObjectToWorld, v.vertex + float4(float3(_Positions[instanceID]), 0));
                o.pos = mul(UNITY_MATRIX_VP, wpos);
                o.color = _Color;
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