Shader "Grass/GrassBlades"
{
    Properties
    {
        _BaseColor("Base Color", Color) = (0, 0.5, 0, 1)
        _TipColor("Tip Color", Color) = (0, 1, 0, 1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "IgnoreProjector"="True" }
        LOD 100

        Pass
        {
            
            Name "ForwardLit"
            Tags { "LightMode" = "ForwardBase" } // ???
            Cull Off
            
            HLSLPROGRAM

            // #pragma exclude_renderers d3d11_9x

            #pragma target 5.0
             
            #pragma multi_compile _ _MAIN_LIGH_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS

            #pragma vertex Vertex
            #pragma fragment Fragment
             
            #include "GrassBlades.hlsl"

            ENDHLSL
        }
    }
}
