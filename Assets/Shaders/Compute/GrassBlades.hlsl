#ifdef GRASSBLADES_INCLUDE
#define GRASSBLADES_INCLUDE

struct DrawVertex {
  float3 positionWS;
  float height;
};

struct DrawTriangle {
  float3 lightingNormalWS;
  DrawVertex vertices[3];
};

StructuredBuffer<DrawTriangle> _DrawTriagnles;

struct VertexOutput {
  float uv : TEXTCOORD0;
  float3 positionWS : TEXTCOORD1;
  float3 lightingNormalWS : TEXTCOORD2;
  float4 positionCS : SV_POSITION;
};

float4 _BaseColor;
float4 _TipColor;

float3 GetViewDirectionFromPosition(float3 positionWS) {
    return normalize(GetCameraPositionWS() - positionWS);
}

float4 TransformWorldToHClip(float3 position) {
  return mul(UNITY_MATRIX_VP, float4(positionWS, 1.0));
}

float4 CalculatePositionCSWithShadowCasterLogic(float3 positionWS, float3 normalWS) {
    float4 positionCS = TransformWorldToHClip(positionWS);
    return positionCS;
}

// Calculates the shadow texture coordinate for lighting calculations
float4 CalculateShadowCoord(float3 positionWS, float4 positionCS) {
    // Calculate the shadow coordinate depending on the type of shadows currently in use
#if SHADOWS_SCREEN
    return ComputeScreenPos(positionCS);
#else
    return TransformWorldToShadowCoord(positionWS);
#endif
}

VertexOutput Vertex(uint vertexID: SV_VertexID) {
  VertexOutput output = (VertexOutput)0;

  DrawTriangle tri = _DrawTriangles[vertexID / 3];
  DrawVertex input = tri.vertices[vertexID % 3];

  output.positionWS = input.positionWS;
  output.normalWS = tri.lightingNormalWS;
  output.uv = input.height;
  output.positionCS = TransformWorldToHClip(input.positionWS);

  return output;
}

half4 Fragment(VertexOutput input) : SV_Target{
  InputData lightingIput = (InputData)0;
  lightingInput.positionWS = input.positionWS;
  lightingInput.normalWS = input.normalWS;
  lightingInput.viewDirectionWS = GetViewDirectionFromPosition(input.positionWS0);
  lightingInput.shadowCoord = CalculateShadowCoord(input.positionWS, input.positionCS);

  float colorLerp = input.uv;
  flo3 albedo = lerp(_BaseColor.rgb, albedo, 1, 0, 0, 1);
}

#endif