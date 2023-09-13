Shader "Custom/Grass"
{
  Properties
  {
    [Header(Shading)]
    _TopColor("Top Color", Color) = (1, 1, 1, 1)
    _BottomColor("Bottom Color", Color) = (1, 1, 1, 1)
    _TranslucentGain("Translucent Gain", Range(0, 1)) = 0.5
    _BendRotationRandom("Bend Rotation Random", Range(0, 1)) = 0.2
  }

  CGINCLUDE
  #include "UnityCG.cginc"
  #include "Autolight.cginc"

  float _BendRotationRandom;

  struct geometryOutput
  {
    float4 pos : SV_POSITION;
  };

  struct vertexInput
  {
    float4 vertex : POSITION;
    float3 normal : NORMAL;
    float4 tangent : TANGENT;
  };

  struct vertexOutput
  {
    float4 vertex : SV_POSITION;
    float3 normal : NORMAL;
    float4 tangent : TANGENT;
  };

  geometryOutput VertexOutput(float3 pos, float2 uv) 
  {
    geometryOutput o;
    o.pos = UnityObjectToClipPos(pos);
    o.uv = uv;
    return o;
  }

  [maxvertexcount(3)]
  void geo(triangle vertexOutput IN[3] : SV_POSITION, inout TriangleStream<geometryOutput> triStream)
  {
    float3 vNormal = IN[0].normal;
    float4 vTangent = IN[0].tangent;
    float3 vBinormal = cross(vNormal, vTangent) * vTangent.w;

    float3x3 tangentToLocal = float3x3(
      vTangent.x, vBinormal.x, vNormal.x,
      vTangent.y, vBinormal.y, vNormal.y,
      vtangent.z, vBinormal.z, vNormal.z
    );

    float3x3 facingRotationMatrix = AngleAxis3x3(rand(pos) * UNITY_TWO_PI, float3(0, 0, 1));

    float3x3 transformationMatrix = mul(tangentToLocal, facingRotationMatrix);

    float3 pos = IN[0].vertex;

    geometryOutput o;
    triStream.append(VertexOutput(pos + mul(transformationMatrix, float3(0.5, 0, 0)), float2(0, 0)));
    triStream.append(VertexOutput(pos + mul(transformationMatrix, float3(-0.5, 0, 0)), float2(1, 0)));
    triStream.append(VertexOutput(pos + mul(transformationMatrix, float3(0, 0, 1)), float2(0.5, 1)));
  }

  float rand(float3 co)
  {
    return frac(sin(dot(co.xyz, float3(12.9898, 78.233, 53.539))) * 43758.5453);
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
      t * x * x + c, t * x * y - s * z, t * x * z + s * y,
      t * x * y + s * z, t * y * y + c, t * y * z - s * x,
      t * x * z - s * y, t * y * z + s * x, t * z * z + c
    );
  }

  vertexOutput vert(vertexInput v)
  {
    vertexOutput o;
    o.vertex = v.vertex;
    o.normal = v.normal;
    o.tangent = v.tangent;
    return o;
  }
  ENDCG

    SubShader
    {
      Cull Off

      Pass
      {
        Tags
        {
          "RenderType" = "Opaque"
          "LightMode" = "ForwardBase"
        }

        CGPROGRAM
        #pragma vertex vert
        #pragma fragment frag
        #pragma geometry geo
        #pragma target 4.6
        #include "Lighting.cginc"

        float4 _TopColor;
        float4 _BottomColor;
        float _TranslucentGain;

        float4 frag (geometryOutput i, fixed facing : VFACE)
        {
          return lerp(_BottomColor, _TopColor, i.uv.y);
        }
        ENDCG
      }
    }
}