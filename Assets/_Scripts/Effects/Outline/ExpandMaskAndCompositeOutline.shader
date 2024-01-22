Shader "Custom/Outline/ExpandMaskAndCompositeOutline"
{
    // I have no idea what any of this means or does but I'm noting things down as I go
    // This isn't really supposed to be 100% my own code (even though its modified), just working on a guide rn so i can learn how to do this stuff
    // will rewrite my own way later
    // -Matthew
    
    Properties
    {
        _OutlineWidth("Outline Width", Range(0, 3)) = 0.1
        _InsideOutlineColor("Inside Outline Color", Color) = (0, 0, 0, 0)
        _TakeDiagonalSamples("Take Diagonal Samples", Range(0, 1)) = 1
        _SmoothOutlineToWorldTransition("Smooth Outline/World Transition", Range(0, 1)) = 0
    }
    
    
    HLSLINCLUDE

    #pragma vertex Vert

    #pragma target 4.5
    #pragma only_renderers d3d11 playstation xboxone xboxseries vulkan metal switch
    
    #include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/RenderPass/CustomPass/CustomPassCommon.hlsl"

    // The PositionInputs struct allow you to retrieve a lot of useful information for your fullScreenShader:
    // struct PositionInputs
    // {
    //     float3 positionWS;  // World space position (could be camera-relative)
    //     float2 positionNDC; // Normalized screen coordinates within the viewport    : [0, 1) (with the half-pixel offset) (??)
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

    float _OutlineWidth;
    float4 _InsideOutlineColor;
    float _TakeDiagonalSamples;
    float _SmoothOutlineToWorldTransition;

    // Might add more later if theres sampling problems down the line
    static const float2 OffsetSamples[8] = {
        // up right down left
        float2(0, 1),
        float2(1, 0),
        float2(0, -1),
        float2(-1, 0),
        // diagonals normalized rr, br, bl, ll
        normalize(float2(1, 1)),
        normalize(float2(1, -1)),
        normalize(float2(-1, -1)),
        normalize(float2(-1, 1))
    };
    
    float4 FullScreenPass(Varyings varyings) : SV_Target // Returns float4 color at current pixel
    {
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(varyings); // for vr, not really needed here
        float depth = LoadCameraDepth(varyings.positionCS.xy);  // gets depth at current pixel from Varyings struct to pass into position getter
        PositionInputs posInput = GetPositionInput(varyings.positionCS.xy, _ScreenSize.zw, depth, UNITY_MATRIX_I_VP, UNITY_MATRIX_V); // gets position input from Varyings struct, ref above

        // Gets pixel color from the custom color buffer that the last shader wrote to
        // Difference between Load and Sample is
        // Load: Gets the color at the pixel the thread is assigned in the custom color buffer
        // Sample: Gets the color at the uv between pixels, interpolates
        float4 color = LoadCustomColor(posInput.positionSS); // Gets color at current pixel screen space coords
        float2 uvPerPixel = 1.0 / _ScreenSize.xy; // ScreenSize.wh really, Ex: (1.0 / 1920, 1.0 / 1080)

        float4 colorPlusOutlineWidth = float4(0,0,0,0);

        // doing this weird workaround and using max() is easier than an if statement on the gpu?
        // How bad are if statements really? I'm not sure
        uint sampleCount = 4 + _TakeDiagonalSamples * 4; // 4 + 4 = 8, 4 + 0 = 4 thanks Github Copilot i would've taken far too long to figure that out


        // Use SampleCustomColor here instead of SampleCameraColor because we're sampling from the last shader's output to the _custom_ color buffer
        for (uint i = 0; i < sampleCount; i++)
        {
            colorPlusOutlineWidth = max(SampleCustomColor(posInput.positionNDC + OffsetSamples[i] * uvPerPixel * _OutlineWidth), colorPlusOutlineWidth);
        }

        float4 outputColor = float4(0,0,0,0);
        outputColor = lerp(outputColor, float4(colorPlusOutlineWidth.rgb, 1), colorPlusOutlineWidth.a);
        outputColor = lerp(outputColor, _InsideOutlineColor, color.a);
        return outputColor;
    }

    ENDHLSL

    SubShader
    {
        Tags{ "RenderPipeline" = "HDRenderPipeline" }
        Pass
        {
            Name "Custom Pass 0"

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

