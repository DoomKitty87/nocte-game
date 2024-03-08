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

    // scan
    uniform float _intensity;
    uniform float3 _scanCenterPos;
    uniform float2 _scanDirectionXZ;
    uniform float _scanDegrees;
    uniform float _scanDistance;
    uniform float4 _scanLineColor;
    uniform float4 _lastScanLineColor;
    uniform float _scanLineWidth;
    uniform float _scanLineDistBetween;
    uniform float _scanLineDistFromEdgeShown;
    uniform float4 _edgeGlowColor;
    uniform float4 _edgeGlowAccentColor;
    uniform float _edgeGlowWidth;
    uniform float _darkenOpacity;
    uniform float _darkenBaseValue;
    uniform float _darkenWidth;
    uniform float _sideFadeMagnitude;
    // sobel
    uniform float4 _sobelColor;
    uniform float _sobelThreshold;
    uniform float _sobelDistFromEdgeShown;
    
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
        float2 dirToFrag = normalize(absWorldPos.xz - _scanCenterPos.xz);
        float2 dirScanForward = normalize(_scanDirectionXZ);
        
        float angleToFrag = atan2(dirToFrag.y, dirToFrag.x);
        float angleForward = atan2(dirScanForward.y, dirScanForward.x);

        float angleDifference = angleForward - angleToFrag;
        angleDifference = (angleDifference + PI) % (2.0 * PI) - PI;
        angleDifference = abs(angleDifference);
        angleDifference = radiansToDegrees(angleDifference);
        
        uv.x = 1 - (angleDifference / (_scanDegrees / 2));
        uv.y = distance(_scanCenterPos.xz, absWorldPos.xz);
        // uv.x > Eps_float() &&
        if ( uv.y < _scanDistance)
        {
            // ----- SCAN
            float newScanLineWidth = _scanLineWidth / 1000 * posInput.linearDepth;
            float scanLineDistanceNormalized = _scanLineDistBetween / newScanLineWidth;
            float distanceFromLine = frac(uv.y / newScanLineWidth / scanLineDistanceNormalized);
            float scanLineMask = 1 - smoothstep(0.0, newScanLineWidth, abs(distanceFromLine - 0.5));
            // -----
            float furthestLineMask = step(_scanDistance - _scanLineDistBetween, uv.y);
            // ----
            float edgeGlowMask = smoothstep(_scanDistance - _edgeGlowWidth, _scanDistance, uv.y);
            // ----
            float darkenMask = smoothstep(_scanDistance - _scanDistance * _darkenWidth, _scanDistance, uv.y);
            // ----
            float sideFadeMask = smoothstep(0, _sideFadeMagnitude, uv.x);

            float scanLineDarkenMask = smoothstep(_scanDistance - _scanLineDistFromEdgeShown, _scanDistance, uv.y);
            float sobelDarkenMask = smoothstep(_scanDistance - _sobelDistFromEdgeShown, _scanDistance, uv.y);
            
            // Sobel Outline
            float sobelMask;
            float pixel = LoadCameraDepth(posInput.positionSS) * 10;
            float northPixel = LoadCameraDepth(posInput.positionSS + float2(0, 1)) * 10;
            float westPixel = LoadCameraDepth(posInput.positionSS + float2(-1, 0)) * 10;
            float southPixel = LoadCameraDepth(posInput.positionSS + float2(0,-1)) * 10;
            float eastPixel = LoadCameraDepth(posInput.positionSS + float2(1, 0)) * 10;
            float differences = (pixel - northPixel) + (pixel - southPixel) + (pixel - westPixel) + (pixel - eastPixel);
            sobelMask = step(_sobelThreshold, differences);
            
            // ---- Final Color

            float3 rgb;
            float a;
            
            float scanLineWithoutFurthest = scanLineMask * (1 - furthestLineMask);
            float scanLineFurthest = step(Eps_float(), furthestLineMask) * scanLineMask;
            
            rgb = (scanLineFurthest * _lastScanLineColor + scanLineWithoutFurthest * _scanLineColor) * scanLineDarkenMask + edgeGlowMask * _edgeGlowColor + (sobelMask * _sobelColor) * sobelDarkenMask;
            a = (scanLineMask * sideFadeMask * scanLineDarkenMask + edgeGlowMask * sideFadeMask + saturate(darkenMask + _darkenBaseValue) * sideFadeMask * _darkenOpacity + sobelMask * sideFadeMask * sobelDarkenMask) * _intensity;

            rgb = lerp(color.rgb, rgb, a);
            color = float4(rgb, 1.0f);
        }
        
        // Fade value allow you to increase the strength of the effect while the camera gets closer to the custom pass volume
        // float f = 1 - abs(_FadeValue * 2 - 1);
        return float4(color);
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
            Blend Off
            Cull Off

            HLSLPROGRAM
                #pragma fragment FullScreenPass
            ENDHLSL
        }
    }
    Fallback Off
}
