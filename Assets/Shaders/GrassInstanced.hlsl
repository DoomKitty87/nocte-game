// https://twitter.com/Cyanilux/status/1396848736022802435?s=20

#ifndef GRASS_INSTANCED_INCLUDED
#define GRASS_INSTANCED_INCLUDED

// ----------------------------------------------------------------------------------

// Graph should contain Boolean Keyword, "PROCEDURAL_INSTANCING_ON", Global, Multi-Compile.
// Must have two Custom Functions in vertex stage. One is used to attach this file (see Instancing_float below),
// and another to set #pragma instancing_options :

// It must use the String mode as this cannot be defined in includes.
// Without this, you will get "UNITY_INSTANCING_PROCEDURAL_FUNC must be defined" Shader Error.
/*
Out = In;
#pragma instancing_options procedural:vertInstancingSetup
*/
// I've found this works fine, but it might make sense for the pragma to be defined outside of a function,
// so could also use this slightly hacky method too
/*
Out = In;
}
#pragma instancing_options procedural:vertInstancingSetup
void dummy(){
*/

// ----------------------------------------------------------------------------------

StructuredBuffer<float4> _instancePositions;

#if UNITY_ANY_INSTANCING_ENABLED

  void vertInstancingSetup(){
  }

#endif

// Obtain InstanceID. e.g. Can be used as a Seed into Random Range node to generate random data per instance
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

// Just passes the position through, allows us to actually attach this file to the graph.
// Should be placed somewhere in the vertex stage, e.g. right before connecting the object space position.
void Instancing_float(float3 Position, out float3 Out){
  float id = 0;
  GetInstanceID_float(id);
  float3 pos = _instancePositions[(int) id].xyz;
  Position = RotateAroundYInDegrees(float4(Position, 1), randomRange(pos.xz, -3.14, 3.14)).xyz;
	Out = Position + pos;
}

#endif