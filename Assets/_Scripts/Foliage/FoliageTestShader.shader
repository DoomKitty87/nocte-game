Shader "Custom/FoliageShader"
{
    Properties
    {
        _Color("Color", Color) = (.2, .2, .2, 1)
    }
    SubShader
    {
        Pass
        {
            Tags
            {
                "RenderType"="Opaque"
            }

            HLSLPROGRAM
            #include "UnityCG.cginc"
            #define UNITY_INDIRECT_DRAW_ARGS IndirectDrawIndexedArgs
            #include "UnityIndirect.cginc"

            StructuredBuffer<float4> position_buffer;

            struct attributes
            {
                float3 normal : NORMAL;
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct varyings
            {
                float4 vertex : SV_POSITION;
                float3 diffuse : TEXCOORD2;
                float3 color : TEXCOORD3;
            };

            varyings vert(attributes v, const uint instance_id : SV_InstanceID)
            {
                const float3 position = position_buffer[instance_id].xyz + v.vertex.xyz;
                
                varyings o;
                o.vertex = mul(UNITY_MATRIX_VP, float4(position, 1.0f));
                return o;
            }

            half4 frag(const varyings i) : SV_Target
            {
                const float3 lighting = i.diffuse *  1.7;
                return half4(i.color * lighting, 1);;
            }
            ENDHLSL
        }
    }
}