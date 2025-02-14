using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.Universal;

namespace Kino.Feedback.Universal {

// Injection pass (after opaque objects)
// Draws a far-plane quad with the feedback texture.
sealed class FeedbackInjectionPass : ScriptableRenderPass
{
    class PassData
    {
        public FeedbackEffect Driver;
        public Material Material;
    }

    Material _material;

    public FeedbackInjectionPass(Material material)
      => _material = material;

    public override void RecordRenderGraph
      (RenderGraph graph, ContextContainer context)
    {
        // Driver component retrieval
        var camera = context.Get<UniversalCameraData>().camera;
        var driver = camera.GetComponent<FeedbackEffect>();
        if (driver == null || !driver.enabled || !driver.IsReady) return;
        if (driver.FeedbackTexture == null) return; // First frame rejection

        // Raster render pass builder
        using var builder = graph.AddRasterRenderPass<PassData>
          ("KinoFeedback (Injection)", out var passData);

        // Pass data setup
        passData.Driver = driver;
        passData.Material = _material;

        // Color/depth attachments
        var resrc = context.Get<UniversalResourceData>();
        builder.SetRenderAttachment(resrc.activeColorTexture, 0);
        builder.SetRenderAttachmentDepth(resrc.activeDepthTexture, AccessFlags.Read);

        // Render function registration
        builder.SetRenderFunc<PassData>((data, ctx) => ExecutePass(data, ctx));
    }

    static void ExecutePass(PassData data, RasterGraphContext context)
      => CoreUtils.DrawFullScreen(context.cmd, data.Material, data.Driver.Properties, 0);
}

// Capture pass (before post-processing)
// Copies the frame buffer into the feedback texture.
sealed class FeedbackCapturePass : ScriptableRenderPass
{
    public override void RecordRenderGraph
      (RenderGraph graph, ContextContainer context)
    {
        // Not supported: Back buffer source
        var resource = context.Get<UniversalResourceData>();
        if (resource.isActiveTargetBackBuffer) return;

        // Driver component retrieval
        var camera = context.Get<UniversalCameraData>().camera;
        var driver = camera.GetComponent<FeedbackEffect>();
        if (driver == null || !driver.enabled || !driver.IsReady) return;

        // Feedback source (camera texture)
        var source = resource.activeColorTexture;

        // Feedback texture setup
        var desc = graph.GetTextureDesc(source);
        driver.PrepareBuffer(desc.width, desc.height, desc.format);
        var buffer = graph.ImportTexture(driver.FeedbackTexture);

        // Blit
        var mat = Blitter.GetBlitMaterial(TextureDimension.Tex2D);
        var param = new RenderGraphUtils.BlitMaterialParameters(source, buffer, mat, 0);
        graph.AddBlitPass(param, passName: "KinoFeedback (capture)");
    }
}

public sealed class FeedbackFeature : ScriptableRendererFeature
{
    [SerializeField, HideInInspector] Shader _shader = null;

    Material _material;
    FeedbackInjectionPass _injection;
    FeedbackCapturePass _capture;

    public override void Create()
    {
        _material = CoreUtils.CreateEngineMaterial(_shader);

        _injection = new FeedbackInjectionPass(_material);
        _capture = new FeedbackCapturePass();

        _injection.renderPassEvent = RenderPassEvent.AfterRenderingOpaques;
        _capture.renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData data)
    {
        if (data.cameraData.cameraType != CameraType.Game) return;

        renderer.EnqueuePass(_injection);
        renderer.EnqueuePass(_capture);
    }
}

} // namespace Kino.Feedback.Universal
