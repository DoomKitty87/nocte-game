Shader "Custom/GrassCustomShader"
{
  Properties
  {
    _BaseColor ("Base Color", Color) = (0, 0, 0, 1)
    _TipColor ("Tip Color", Color) = (1, 1, 1, 1)
    _DarkColor ("Dark Color", Color) = (0, 0, 0, 1)
    _RimColor ("Rim Color", Color) = (1, 1, 1, 1)
    _BladeHeight ("Blade Height", Range(0.1, 1)) = 0.5
    _WindImpact ("Wind Impact", Range(0.1, 10)) = 1
    _WindSpeed ("Wind Speed", Range(0.1, 10)) = 1
    _TrampleDist ("Trample Distance", Range(0.1, 10)) = 1
    _TrampleDownStrength ("Trample Down Strength", Range(0.1, 10)) = 1
    _TrampleOutStrength ("Trample Out Strength", Range(0.1, 10)) = 1
  }
  SubShader
  {

    Cull Off

    Tags { "RenderType"="Opaque" }

    Pass
    {
      CGPROGRAM
      #pragma vertex vert
      #pragma fragment frag

      StructuredBuffer<float4> _instancePositions;
      float3 _MainLightDir;
      float _WindStrength;
      float _WindDirection;
      float4 _BaseColor;
      float4 _TipColor;
      float4 _DarkColor;
      float4 _RimColor;
      float _BladeHeight;
      float4 _PlayerPosition;
      float _WindImpact, _WindSpeed;
      float _TrampleDist, _TrampleDownStrength, _TrampleOutStrength;

      struct appdata {
        float4 vertex : POSITION;
      };

      struct v2f
      {
        float4 vertexPosition : SV_POSITION;
        float4 vertexColor : COLOR0;
        float2 uv : TEXCOORD0;
      };

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
      
      v2f vert(appdata v, uint instanceID : SV_INSTANCEID)
      {
        v2f output;
        float3 vertexPosition = v.vertex.xyz;
        output.vertexColor = lerp(_BaseColor, _TipColor, vertexPosition.y);
        float4 objWorldPos = _instancePositions[instanceID];
        float rot = randomRange(objWorldPos.xz, -3.14, 3.14);
        vertexPosition = RotateAroundYInDegrees(float4(vertexPosition, 1), rot);
        // Local transform
        float2 uv = float2(0, vertexPosition.y);
        float heightMod = randomRange(objWorldPos.xz, 0.8f, 1.2f);
        heightMod *= _BladeHeight;
        vertexPosition.y *= heightMod;
        // World space
        float4 worldPos = float4(vertexPosition.xyz + objWorldPos.xyz, 1);

        // Wind
        float windStr = randomRange(objWorldPos.xz, 0.6f, 1.3f);
        float windOffset = randomRange(objWorldPos.zx, -0.5f, 0.5f);
        float xDisp = (sin(_Time.y * _WindStrength * _WindSpeed + windOffset + objWorldPos.x + objWorldPos.z) + 1) * _WindStrength * _WindImpact * windStr * uv.y * uv.y * cos(_WindDirection);
        float zDisp = (sin(_Time.y * _WindStrength * _WindSpeed + windOffset) + objWorldPos.x + objWorldPos.z + 1) * _WindStrength * _WindImpact * windStr * uv.y * uv.y * sin(_WindDirection);
        worldPos.x += xDisp;
        worldPos.z += zDisp;
        worldPos.y -= sqrt(heightMod * heightMod + xDisp * xDisp + zDisp * zDisp) - heightMod;

        float dist = distance(worldPos.xyz, _PlayerPosition.xyz);
        float trample = 1 - saturate(dist / _TrampleDist);
        trample *= uv.y * uv.y;
        worldPos.y -= trample * 2 * _TrampleDownStrength;
        float3 dir = normalize(worldPos.xyz - _PlayerPosition.xyz);
        worldPos.x += trample * dir.x * _TrampleOutStrength;
        worldPos.z += trample * dir.z * _TrampleOutStrength;
        // Trampling
        output.vertexPosition = mul(UNITY_MATRIX_VP, worldPos);
        return output;
      }

      float4 frag(v2f i) : SV_Target
      {
        float4 c = i.vertexColor;
        float dotL = dot(float3(0, -1, 0), normalize(_MainLightDir));
        dotL = max(0, dotL);
        dotL = 1 - dotL;
        dotL = pow(dotL, 2);
        c = lerp(c, _DarkColor, dotL);
        return c;
      }
      ENDCG
    }
  }
}
