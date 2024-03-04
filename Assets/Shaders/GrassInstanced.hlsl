#ifndef GRASS_INSTANCED_INCLUDED
#define GRASS_INSTANCED_INCLUDED

StructuredBuffer<float4> _instancePositions;

#if UNITY_ANY_INSTANCING_ENABLED

  void vertInstancingSetup(){
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

float randomRange(float2 seed, float min, float max)
{
  const float randNum = frac(sin(dot(seed, float2(12.9898, 78.233)))*143758.5453);
  return lerp(min, max, randNum);
}

float4 RotateAroundYInDegrees (float4 vertex, float rads) {
  float sina, cosa;
  sincos(rads, sina, cosa);
  float2x2 m = float2x2(cosa, -sina, sina, cosa);
  return float4(mul(m, vertex.xz), vertex.yw).xzyw;
}

void Instancing_float(float3 Position, out float3 Out, out float3 World){
  float id = 0;
  GetInstanceID_float(id);
  float3 pos = _instancePositions[(int) id].xyz;
  Position = RotateAroundYInDegrees(float4(Position, 0), randomRange(pos.xz, -3.14, 3.14)).xyz;
      Out = Position + pos;
      World = pos;
}

#endif