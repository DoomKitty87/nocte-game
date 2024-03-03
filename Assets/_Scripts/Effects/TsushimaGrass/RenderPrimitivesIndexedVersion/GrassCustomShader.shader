Shader "Custom/GrassCustomShader"
{
    Properties
    {
      _BaseColor ("Base Color", Color) = (0, 0, 0, 1)
      _TipColor ("Tip Color", Color) = (1, 1, 1, 1)
      _DarkColor ("Dark Color", Color) = (0, 0, 0, 1)
      _RimColor ("Rim Color", Color) = (1, 1, 1, 1)
      _BladeHeight ("Blade Height", Range(0.1, 1)) = 0.5
      _TrampleDist ("Trample Distance", Range(0.1, 10)) = 1
      _TrampleDownStrength ("Trample Down Strength", Range(0.1, 10)) = 1
      _TrampleOutStrength ("Trample Out Strength", Range(0.1, 10)) = 1
    }
    SubShader
    {

        Tags { "RenderType"="Opaque" "Queue" = "Transparent"}

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog

            StructuredBuffer<uint> _lodIndexBuffer;
            StructuredBuffer<float4x4> _instancePositionMatrices;
            StructuredBuffer<float> _debugBuffer;
            float _instanceCount;
            StructuredBuffer<float3> _vertexPositions;
            float3 _MainLightDir;
            float _WindStrength;
            float _WindDirection;
            float4 _BaseColor;
            float4 _TipColor;
            float4 _DarkColor;
            float4 _RimColor;
            float _BladeHeight;
            float4 _PlayerPosition;
            float _TrampleDist, _TrampleDownStrength, _TrampleOutStrength;

            struct FragData
            {
                float4 vertexPosition : SV_POSITION;
                float4 vertexColor : COLOR0;
                float2 uv : TEXCOORD0;
                // Add normals passed in
            };

            float randomRange(float2 seed, float min, float max)
            {
              const float randNum = frac(sin(dot(seed, float2(12.9898, 78.233)))*143758.5453);
              return lerp(min, max, randNum);
            }
            
            FragData vert(uint triIndex: SV_VertexID, uint instanceID : SV_InstanceID)
            {
                FragData output;
                float3 vertexPosition = _vertexPositions[triIndex];
                output.vertexColor = lerp(_BaseColor, _TipColor, vertexPosition.y);
                // Local transform
                float2 uv = float2(0, vertexPosition.y);
                vertexPosition.y *= _BladeHeight;
                // World space
                float4 worldPos = mul(_instancePositionMatrices[instanceID], float4(vertexPosition, 1.0f));
                float4 objWorldPos = mul(_instancePositionMatrices[instanceID], float4(0, 0, 0, 1));

                // Wind
                float windStr = randomRange(objWorldPos.xz, 0.6f, 1.3f);
                float windOffset = randomRange(objWorldPos.zx, -0.5f, 0.5f);
                worldPos.x += sin(_Time.y * 1 + windOffset) * 0.1f * windStr * uv.y * uv.y * cos(1);
                worldPos.z += sin(_Time.y * 1 + windOffset) * 0.1f * windStr * uv.y * uv.y * sin(1);

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

            float4 frag(FragData i) : SV_Target
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
