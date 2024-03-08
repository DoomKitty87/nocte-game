#ifndef GRASS_INSTANCED_INCLUDED
#define GRASS_INSTANCED_INCLUDED

#define unity_ObjectToWorld unity_ObjectToWorld
#define unity_WorldToObject unity_WorldToObject

StructuredBuffer<float4> _instancePositions;

float randomRange(float2 seed, float min, float max)
{
  const float randNum = frac(sin(dot(seed, float2(12.9898, 78.233)))*143758.5453);
  return lerp(min, max, randNum);
}

float3x3 AngleAxis3x3(float angle, float3 axis)
{
  float c, s;
  sincos(angle, s, c);

  float t = 1 - c;
  float x = axis.x;
  float y = axis.y;
  float z = axis.z;

  return float3x3(
      t * x * x + c,      t * x * y - s * z,  t * x * z + s * y,
      t * x * y + s * z,  t * y * y + c,      t * y * z - s * x,
      t * x * z - s * y,  t * y * z + s * x,  t * z * z + c
  );
}

#if UNITY_ANY_INSTANCING_ENABLED

  void vertInstancingMatrices(inout float4x4 objectToWorld, out float4x4 worldToObject) {
		float4 data = _instancePositions[unity_InstanceID];

    float3 wPos = data.xyz;

    float3x3 rot = AngleAxis3x3(data.w, float3(0, 1, 0));

    objectToWorld._11_21_31_41 = float4(rot[0], 0.0f);
    objectToWorld._12_22_32_42 = float4(rot[1], 0.0f);
    objectToWorld._13_23_33_43 = float4(rot[2], 0.0f);
    objectToWorld._14_24_34_44 = float4(wPos, 1.0f);

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

	void vertInstancingSetup() {
		vertInstancingMatrices(unity_ObjectToWorld, unity_WorldToObject);
	}

#endif

void GetInstanceID_float(out float Out){
	Out = 0;
	#ifndef SHADERGRAPH_PREVIEW
	#if UNITY_ANY_INSTANCING_ENABLED
	Out = unity_InstanceID;
	#endif
	#endif
}

void Instancing_float(float3 Position, out float3 Out, out float4 World){
  Out = Position;
  float id = 0;
  GetInstanceID_float(id);
  World = _instancePositions[id].xyzw;
}

#endif