using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering;

class OutlineCustomPass : CustomPass
{

    // Need to keep references unity says or the shaders will be removed on build??
    // why
    [SerializeField] private Shader ExpandMaskAndCompositeOutline;
    [SerializeField] private Shader RenderOutlinedAsSolidColor;
    
    // TODO: Use this later to make outlines more configurable if you're bored
    
    // It can be used to configure render targets and their clear state. Also to create temporary render target textures.
    // When empty this render pass will render to the active camera render target.
    // You should never call CommandBuffer.SetRenderTarget. Instead call <c>ConfigureTarget</c> and <c>ConfigureClear</c>.
    // The render pipeline will ensure target setup and clearing happens in an performance manner.
    protected override void Setup(ScriptableRenderContext renderContext, CommandBuffer cmd)
    {
        
    }

    protected override void Execute(CustomPassContext ctx)
    {
        // Executed every frame for all the camera inside the pass volume.
        // The context contains the command buffer to use to enqueue graphics commands.
    }

    protected override void Cleanup()
    {
        // Cleanup code
    }
}