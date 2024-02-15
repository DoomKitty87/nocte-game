StructuredBuffer<float4x4> _instancePositionMatrices;
// float _instancesPerGroup;
// float _instanceGroupID;
void GetMatrixBuffer_float (float InstanceID, out float4x4 WorldPosition)
{
	// InstanceID = InstanceID + _instanceGroupID * _instancesPerGroup;
	WorldPosition = _instancePositionMatrices[(uint)InstanceID];
}