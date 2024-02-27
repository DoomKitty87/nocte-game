Shader "ExampleShader"
{
    Properties
    {
        _Color ("Color", Color) = (0, 0, 0, 1)
    }
    SubShader
    {
        Pass
        {
            Tags
            {
                "RenderType"="Opaque"
            }
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing

            #include "UnityCG.cginc"
            #include "UnityIndirect.cginc"

            struct v2f
            {
                float4 pos : SV_POSITION;
                float4 color : COLOR0;
            };

            fixed4 _Color;

            StructuredBuffer<float4> position_buffer;

            v2f vert(appdata_base v, uint svInstanceID : SV_InstanceID)
            {
                float4 position = position_buffer[svInstanceID];

                v2f o;
                o.pos = mul(UNITY_MATRIX_VP, float4(v.vertex.xyz + position.xyz, 1.0f));
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