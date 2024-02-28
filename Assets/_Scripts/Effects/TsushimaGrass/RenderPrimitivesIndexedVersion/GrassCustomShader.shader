Shader "Custom/GrassCustomShader"
{
    Properties
    {
        
    }
    SubShader
    {
        Cull Off
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            struct FragData
            {
                float4 vertexPosition : SV_POSITION;
                float4 vertexColor : COLOR0;
            };

            StructuredBuffer<float3> _meshVertPositions;
            StructuredBuffer<float4x4> _instancePositionMatrices;
            StructuredBuffer<float> _debugBuffer;
            float _instanceCount;

            FragData vert(uint triIndex: SV_VertexID, uint instanceID : SV_InstanceID)
            {
                FragData output;
                float3 vertexPosition = _meshVertPositions[triIndex];
                // local space - do wind curvature here
                
                // world space
                float4 worldPos = mul(_instancePositionMatrices[instanceID], float4(vertexPosition, 1.0f));
                // view space / clip space(?) -- do clip space vert adjustments here
                output.vertexPosition = mul(UNITY_MATRIX_VP, worldPos);
                output.vertexColor = float4(0.0f, 100.3f, 0.0f, 1.0f);
                return output;
            }

            float4 frag(FragData i) : SV_Target
            {
                return i.vertexColor;
            }
            ENDCG
        }
    }
}
