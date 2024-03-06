Shader "FullScreen/ScannerEffectFullscreen"
{
    HLSLINCLUDE

    #pragma vertex Vert

    #pragma target 4.5
    #pragma only_renderers d3d11 playstation xboxone xboxseries vulkan metal switch

    #include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/RenderPass/CustomPass/CustomPassCommon.hlsl"

    // The PositionInputs struct allow you to retrieve a lot of useful information for your fullScreenShader:
    // struct PositionInputs
    // {
    //     float3 positionWS;  // World space position (could be camera-relative)
    //     float2 positionNDC; // Normalized screen coordinates within the viewport    : [0, 1) (with the half-pixel offset)
    //     uint2  positionSS;  // Screen space pixel coordinates                       : [0, NumPixels)
    //     uint2  tileCoord;   // Screen tile coordinates                              : [0, NumTiles)
    //     float  deviceDepth; // Depth from the depth buffer                          : [0, 1] (typically reversed)
    //     float  linearDepth; // View space Z coordinate                              : [Near, Far]
    // };

    // To sample custom buffers, you have access to these functions:
    // But be careful, on most platforms you can't sample to the bound color buffer. It means that you
    // can't use the SampleCustomColor when the pass color buffer is set to custom (and same for camera the buffer).
    // float4 CustomPassSampleCustomColor(float2 uv);
    // float4 CustomPassLoadCustomColor(uint2 pixelCoords);
    // float LoadCustomDepth(uint2 pixelCoords);
    // float SampleCustomDepth(float2 uv);

    // There are also a lot of utility function you can use inside Common.hlsl and Color.hlsl,
    // you can check them out in the source code of the core SRP package.

    uniform float3 _scanCenterPos;
    uniform float2 _scanDirectionXZ;
    uniform float _scanDegrees;
    uniform float _scanDistance;
    uniform float4 _scanLineColor;
    uniform float4 _lastScanLineColor;
    uniform float _scanLineWidth;
    uniform float _scanLineDistBetween;
    uniform float4 _edgeGlowColor;
    uniform float _edgeGlowWidth;
    uniform float _darkenStartDistance;
    uniform float _sideFadeMagnitude;
    
    float radiansToDegrees(float radians) {
        return radians * 57.295779513082320876798154814105;
    }
    
    float4 FullScreenPass(Varyings varyings) : SV_Target
    {
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(varyings);
        float depth = LoadCameraDepth(varyings.positionCS.xy);
        PositionInputs posInput = GetPositionInput(varyings.positionCS.xy, _ScreenSize.zw, depth, UNITY_MATRIX_I_VP, UNITY_MATRIX_V);
        float3 camWorldPos = posInput.positionWS;
        float3 absWorldPos = GetAbsolutePositionWS(posInput.positionWS);
        float3 viewDirection = GetWorldSpaceNormalizeViewDir(posInput.positionWS);
        float4 color = float4(0.0, 0.0, 0.0, 0.0);

        // Load the camera color buffer at the mip 0 if we're not at the before rendering injection point
        if (_CustomPassInjectionPoint != CUSTOMPASSINJECTIONPOINT_BEFORE_RENDERING)
            color = float4(CustomPassLoadCameraColor(varyings.positionCS.xy, 0), 1);

        // Add your custom pass code here
        float2 uv = float2(0, 0);
        float2 dirFromCenter = normalize(absWorldPos.xz - _scanCenterPos.xz);
        float2 dirScanForward = normalize(_scanDirectionXZ);
        
        float angleFromCenter = atan2(dirFromCenter.y, dirFromCenter.x);
        float angleFromForward = atan2(dirScanForward.y, dirScanForward.x);
        float angleDifference = angleFromForward - angleFromCenter;
        angleDifference = radiansToDegrees(angleDifference);
        
        uv.x = 1 - (abs(angleDifference) / (_scanDegrees / 2));
        uv.y = distance(_scanCenterPos.xz, absWorldPos.xz);
        
        if (uv.x > Eps_float() && uv.y < _scanDistance)
        {
            // -----
            float scanLineDistanceNormalized = _scanLineDistBetween / _scanLineWidth;
            float distanceFromLine = frac(uv.y / _scanLineWidth / scanLineDistanceNormalized);
            float scanLineMask = 1 - smoothstep(0.0, _scanLineWidth, abs(distanceFromLine - 0.5));
            // -----
            float furthestLineMask = step(_scanDistance - _scanLineDistBetween, uv.y);
            // ----
            float edgeGlowMask = smoothstep(_scanDistance - _edgeGlowWidth, _scanDistance, uv.y);
            // ----
            float darkenMask = smoothstep(_darkenStartDistance, _scanDistance, uv.y);
            // ----
            float sideFadeMask = smoothstep(0, _sideFadeMagnitude, uv.x);
            // ----
            float scanLineWithoutFurthest = scanLineMask * 1 - furthestLineMask;
            float scanLineFurthestOnly = scanLineMask * furthestLineMask;
            float3 scanLines = scanLineWithoutFurthest * _scanLineColor + scanLineFurthestOnly * _lastScanLineColor;
            float a = darkenMask * sideFadeMask;
            // color = lerp(color, float4(rgb, 1.0f), a);
            color = float4(scanLines, 1.0f);
        }
        
        // Fade value allow you to increase the strength of the effect while the camera gets closer to the custom pass volume
        // float f = 1 - abs(_FadeValue * 2 - 1);
        return float4(color.rgb, color.a);
    }

    ENDHLSL

    SubShader
    {
        Tags{ "RenderPipeline" = "HDRenderPipeline" }
        Pass
        {
            Name "Scanner Custom Pass"

            ZWrite Off
            ZTest Always
            Blend SrcAlpha OneMinusSrcAlpha
            Cull Off

            HLSLPROGRAM
                #pragma fragment FullScreenPass
            ENDHLSL
        }
    }
    Fallback Off
}
