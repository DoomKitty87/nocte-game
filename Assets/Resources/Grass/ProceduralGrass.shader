Shader "Custom/ProceduralGrass"
{
    Properties
    {
        _BaseColor ("Base Color", Color) = (0, 0, 0, 1)
        _TipColor ("Tip Color", Color) = (1, 1, 1, 1)
        _BaseTex ("Base Texture", 2d) = "white"
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue" = "Transparent"}

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog
            
            struct appdata
            {
                uint vertexID : SV_VertexID;
                uint instanceID : SV_InstanceID;
            };

            struct v2f
            {
				float4 positionCS : SV_Position;
				float4 positionWS : TEXCOORD0;
				float2 uv : TEXCOORD1;
            };

            sampler2D _BaseTex;
            float4 _BaseColor, _TipColor;
            StructuredBuffer<float3> _Positions;
            StructuredBuffer<float3> _Normals;
            StructuredBuffer<float2> _UVs;
            StructuredBuffer<float4x4> _TransformMatrices;

            v2f vert (appdata v)
            {
                v2f o;

                float4 positionOS = float4(_Positions[v.vertexID], 1.0f);
                float4x4 objectToWorld = _TransformMatrices[v.instanceID];

                o.positionWS = mul(objectToWorld, positionOS);
                o.positionCS = mul(UNITY_MATRIX_VP, o.positionWS);
                o.uv = _UVs[v.vertexID];
                
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float4 col = tex2D(_BaseTex, i.uv);
                
                return col * lerp(_BaseColor, _TipColor, i.uv.y);
            }
            ENDCG
        }
    }
}
