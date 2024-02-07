StructuredBuffer<float4x4> _instancePositionMatrices;
float _instanceGroupID;
void GetMatrixBuffer_float (float InstanceID, out float4x4 WorldPosition)
{
	// temp
	if (InstanceID == 0)
	{
		WorldPosition = float4x4(
			1, 0, 0, 0,
			0, 1, 0, 0,
			0, 0, 1, 0,
			0, 0, 0, 1
		);
	}
	WorldPosition = _instancePositionMatrices[(uint)InstanceID];
}