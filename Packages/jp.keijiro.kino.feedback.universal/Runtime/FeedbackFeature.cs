using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.Universal;

namespace Kino.Feedback.Universal {

// Context data for sharing the feedback buffer between the passes
sealed class FeedbackContextData : ContextItem
{
    public TextureHandle Buffer { get; set; }
    public override void Reset() => Buffer = TextureHandle.nullHandle;
}

// Injection pass: Draws the content of the feedback buffer
sealed class FeedbackInjectionPass : ScriptableRenderPass
{
    class PassData
    {
        public Material Material;
        public FeedbackEffect Driver;
        public TextureHandle Buffer;
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
        if (driver.Buffer == null) return; // First frame skip

        // Render pass building
        using var builder = graph.AddRasterRenderPass<PassData>
          ("KinoFeedback (Injection)", out var data);

        // Pass data setup
        data.Material = _material;
        data.Driver = driver;
        data.Buffer = graph.ImportTexture(driver.Buffer);

        // Context data for sharing the feedback buffer
        var contextData = context.Create<FeedbackContextData>();
        contextData.Buffer = data.Buffer;

        // Color/depth attachments
        var resrc = context.Get<UniversalResourceData>();
        builder.SetRenderAttachment(resrc.activeColorTexture, 0);
        builder.SetRenderAttachmentDepth(resrc.activeDepthTexture, AccessFlags.Read);

        // Render function registration
        builder.SetRenderFunc<PassData>((data, ctx) => ExecutePass(data, ctx));
    }

    static void ExecutePass(PassData data, RasterGraphContext ctx)
    {
        data.Material.SetTexture("_FeedbackTexture", data.Buffer);
        CoreUtils.DrawFullScreen(ctx.cmd, data.Material, data.Driver.Properties, 1);
    }
}

// Capture pass: Captures the frame buffer before post-processing
sealed class FeedbackCapturePass : ScriptableRenderPass
{
    Material _material;

    public FeedbackCapturePass(Material material)
      => _material = material;

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

        // Feedback source
        var source = resource.activeColorTexture;

        // Feedback buffer setup
        TextureHandle buffer;
        if (context.Contains<FeedbackContextData>())
        {
            // Retrieval from the context data when it's available
            var contextData = context.Get<FeedbackContextData>();
            buffer = contextData.Buffer;
        }
        else
        {
            // New buffer allocation
            var desc = graph.GetTextureDesc(source);
            driver.PrepareBuffer(desc.width, desc.height);
            buffer = graph.ImportTexture(driver.Buffer);
        }

        // Blit
        var param = new RenderGraphUtils.
          BlitMaterialParameters(source, buffer, _material, 0);
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
        _capture = new FeedbackCapturePass(_material);

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
