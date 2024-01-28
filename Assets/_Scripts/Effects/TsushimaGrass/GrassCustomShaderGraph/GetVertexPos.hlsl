#ifndef GETVERTEXPOS_INCLUDED
#define GETVERTEXPOS_INCLUDED

StructuredBuffer<float3> _meshVertPositions;
StructuredBuffer<float4x4> _instancePositionMatrices;

void GetVertexPos_float(float instanceID, float vertexID, out float3 vertPosition)
{
	float3 vertexPosition = _meshVertPositions[vertexID];
	vertPosition = mul(_instancePositionMatrices[instanceID], float4(vertexPosition, 1.0f));
}

#endif //GETVERTEXPOS_INCLUDED