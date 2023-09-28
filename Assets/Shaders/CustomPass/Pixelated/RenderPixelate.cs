using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering;

class FancyPass : CustomPass {
	private Shader fullscreenShader;
	private Material fullscreenMaterial;
	private Shader prePassShader;
	private Material prePassMaterial;
	private RTHandle intermediateRenderTarget;
	private LayerMask pixelationLayer = ~0;

	protected override void Setup(ScriptableRenderContext renderContext, CommandBuffer cmd) {
		// Create pre pass shader/material
		prePassShader = Shader.Find("Custom/PrePass");
		prePassMaterial = CoreUtils.CreateEngineMaterial(prePassShader);
		Debug.Assert(fullscreenMaterial != null, "Failed to create fullscreen pass material");

		// Create fullscreen shader/material
		fullscreenShader = Shader.Find("Custom/FancyPass");
		fullscreenMaterial = CoreUtils.CreateEngineMaterial(fullscreenShader);
		Debug.Assert(fullscreenMaterial != null, "Failed to create fullscreen pass material");
		fullscreenMaterial.SetColor(0, Color.red);

		pixelationLayer = LayerMask.GetMask("Pixelate");

		// Create render target for pre-pass
		intermediateRenderTarget = RTHandles.Alloc(
				Vector2.one, TextureXR.slices, dimension: TextureXR.dimension,
				colorFormat: GraphicsFormat.B10G11R11_UFloatPack32,
				useDynamicScale: true, name: "Immediate Test Buffer"
		);
	}

	protected override void Execute(CustomPassContext ctx) {
		CustomPassUtils.DrawRenderers(ctx, pixelationLayer, RenderQueueType.All);
		// Pre pass
		CoreUtils.SetRenderTarget(ctx.cmd, intermediateRenderTarget, ctx.cameraDepthBuffer, ClearFlag.Color);
		CustomPassUtils.DrawRenderers(ctx, pixelationLayer, RenderQueueType.All, prePassMaterial);

		// Fullscreen pass
		ctx.propertyBlock.SetTexture("_InputTexture", intermediateRenderTarget);
		CoreUtils.SetRenderTarget(ctx.cmd, ctx.cameraColorBuffer, ClearFlag.None);
		CoreUtils.DrawFullScreen(ctx.cmd, fullscreenMaterial, ctx.propertyBlock, shaderPassId: 0);
	}

	protected override void Cleanup() {
		CoreUtils.Destroy(prePassMaterial);
		CoreUtils.Destroy(fullscreenMaterial);
		intermediateRenderTarget.Release();
	}
}