using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.Universal;

namespace Kino.Feedback.Universal {

sealed class FeedbackInjectionPass : ScriptableRenderPass
{
    class PassData
    {
        public Material Material;
        public FeedbackEffect Driver;
        public TextureHandle Feedback;
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

        // Render pass building
        using var builder = graph.
          AddRasterRenderPass<PassData>("KinoFeedback (Injection)", out var data);

        // Custom pass data
        data.Material = _material;
        data.Driver = driver;
        data.Feedback = graph.ImportTexture(driver.Buffer);

        // Color/depth attachments
        var resource = context.Get<UniversalResourceData>();
        builder.SetRenderAttachment(resource.activeColorTexture, 0);
        builder.SetRenderAttachmentDepth(resource.activeDepthTexture, AccessFlags.Read);

        // Render function registration
        builder.SetRenderFunc<PassData>((data, ctx) => ExecutePass(data, ctx));
    }

    static void ExecutePass(PassData data, RasterGraphContext ctx)
    {
        data.Material.SetTexture("_FeedbackTexture", data.Feedback);
        CoreUtils.DrawFullScreen(ctx.cmd, data.Material, data.Driver.Properties, 1);
    }
}

// Feedback capture pass: Captures the frame buffer before post-processing
sealed class FeedbackCapturePass : ScriptableRenderPass
{
    Material _material;

    public FeedbackCapturePass(Material material)
      => _material = material;

    public override void RecordRenderGraph(RenderGraph graph, ContextContainer context)
    {
        // Not supported: Back buffer source
        var resource = context.Get<UniversalResourceData>();
        if (resource.isActiveTargetBackBuffer) return;

        // Driver component retrieval
        var camera = context.Get<UniversalCameraData>().camera;
        var driver = camera.GetComponent<FeedbackEffect>();
        if (driver == null || !driver.enabled || !driver.IsReady) return;

        // Feedback buffer allocation
        var source = resource.activeColorTexture;
        var desc = graph.GetTextureDesc(source);
        driver.PrepareBuffer(desc.width, desc.height);

        //
        var clear = new ImportResourceParams()
        {
            clearOnFirstUse = true,
            clearColor = Color.black,
            discardOnLastUse = false
        };

        var buffer = graph.ImportTexture(driver.Buffer, clear);

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
