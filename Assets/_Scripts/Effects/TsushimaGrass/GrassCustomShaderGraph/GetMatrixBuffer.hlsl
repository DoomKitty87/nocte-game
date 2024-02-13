StructuredBuffer<float4x4> _instancePositionMatrices;
float _instanceGroupID;
void GetMatrixBuffer_float (float InstanceID, out float4x4 WorldPosition)
{
	WorldPosition = _instancePositionMatrices[(uint)InstanceID];
}