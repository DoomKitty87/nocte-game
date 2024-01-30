StructuredBuffer<float3> _meshVertPositions;
StructuredBuffer<float4x4> _instancePositionMatrices;

// I have no idea whats going on, pretty much everything except for GetVertexPos_float is from
// https://gist.github.com/Cyanilux/4046e7bf3725b8f64761bf6cf54a16eb#file-grassinstanced-hlsl
// https://www.cyanilux.com/faq/#sg-drawmeshinstancedindirect

// THIS DOES NOT CURRENTLY WORK, DO NOT USE IT

// #if UNITY_ANY_INSTANCING_ENABLED
void vertInstancingMatrices(inout float4x4 objectToWorld, out float4x4 worldToObject) {
	float4 data = _instancePositionMatrices[unity_InstanceID];

	objectToWorld = mul(objectToWorld, data.m);

	// Transform matrix (override current)
	// I prefer keeping positions relative to the bounds passed into DrawMeshInstancedIndirect so use the above instead
	//objectToWorld._11_21_31_41 = float4(data.m._11_21_31, 0.0f);
	//objectToWorld._12_22_32_42 = float4(data.m._12_22_32, 0.0f);
	//objectToWorld._13_23_33_43 = float4(data.m._13_23_33, 0.0f);
	//objectToWorld._14_24_34_44 = float4(data.m._14_24_34, 1.0f);

	// Inverse transform matrix
	float3x3 w2oRotation;
	w2oRotation[0] = objectToWorld[1].yzx * objectToWorld[2].zxy - objectToWorld[1].zxy * objectToWorld[2].yzx;
	w2oRotation[1] = objectToWorld[0].zxy * objectToWorld[2].yzx - objectToWorld[0].yzx * objectToWorld[2].zxy;
	w2oRotation[2] = objectToWorld[0].yzx * objectToWorld[1].zxy - objectToWorld[0].zxy * objectToWorld[1].yzx;

	float det = dot(objectToWorld[0].xyz, w2oRotation[0]);
	w2oRotation = transpose(w2oRotation);
	w2oRotation *= rcp(det);
	float3 w2oPosition = mul(w2oRotation, -objectToWorld._14_24_34);

	worldToObject._11_21_31_41 = float4(w2oRotation._11_21_31, 0.0f);
	worldToObject._12_22_32_42 = float4(w2oRotation._12_22_32, 0.0f);
	worldToObject._13_23_33_43 = float4(w2oRotation._13_23_33, 0.0f);
	worldToObject._14_24_34_44 = float4(w2oPosition, 1.0f);
}


void GetInstanceID_float(out float Out){
	Out = 0;
	#ifndef SHADERGRAPH_PREVIEW
	#if UNITY_ANY_INSTANCING_ENABLED
	Out = unity_InstanceID;
	#endif
	#endif
}

void GetVertexPos_float(float instanceID, float vertexID, out float3 vertPosition)
{
	float3 vertexPosition = _meshVertPositions[vertexID];
	vertPosition = mul(_instancePositionMatrices[instanceID], float4(vertexPosition, 1.0f));
}

void Instancing_float(float3 Position, out float3 Out){
	Out = Position;
}
// #endif