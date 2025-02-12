using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.Universal;
using UnityEngine.Experimental.Rendering;
using System;

namespace Kino.Feedback.Universal {

sealed class FeedbackCapturePass : ScriptableRenderPass, IDisposable
{
    class PassData
    {
        public Material Material;
        public FeedbackEffect Driver;
    }

    Material _material;

    public RTHandle Buffer { get; private set; }
    public TextureHandle BufferHandle { get; private set; }

    public FeedbackCapturePass(Material material)
      => _material = material;

    public void Dispose()
    {
        Buffer?.Release();
        Buffer = null;
    }

    public override void RecordRenderGraph(RenderGraph graph, ContextContainer context)
    {
        // Not supported: Back buffer source
        var resource = context.Get<UniversalResourceData>();
        if (resource.isActiveTargetBackBuffer) return;

        // Driver component retrieval
        var camera = context.Get<UniversalCameraData>().camera;
        var driver = camera.GetComponent<FeedbackEffect>();
        if (driver == null || !driver.enabled || !driver.IsReady) return;

        var source = resource.activeColorTexture;
        var desc = graph.GetTextureDesc(source);

        if (Buffer == null)
            Buffer = RTHandles.Alloc(desc.width, desc.height, GraphicsFormat.R8G8B8A8_SRGB);

        BufferHandle = graph.ImportTexture(Buffer);

        // Blit
        var param = new RenderGraphUtils.
          BlitMaterialParameters(source, BufferHandle, _material, 0);
        graph.AddBlitPass(param, passName: "KinoFeedback (capture)");
    }
}

sealed class FeedbackInjectionPass : ScriptableRenderPass
{
    class PassData
    {
        public Material Material;
        public FeedbackEffect Driver;
        public TextureHandle Feedback;
    }

    Material _material;
    FeedbackCapturePass _capture;

    public FeedbackInjectionPass(Material material, FeedbackCapturePass capture)
      => (_material, _capture) = (material, capture);

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

        // Texture registration
        //builder.UseTexture(_capture.BufferHandle);
        var bufferHandle = graph.ImportTexture(_capture.Buffer);

        // Custom pass data
        data.Material = _material;
        data.Driver = driver;
        data.Feedback = bufferHandle;

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

public sealed class FeedbackFeature : ScriptableRendererFeature
{
    [SerializeField, HideInInspector] Shader _shader = null;

    Material _material;
    FeedbackCapturePass _capture;
    FeedbackInjectionPass _injection;

    public override void Create()
    {
        _material = CoreUtils.CreateEngineMaterial(_shader);

        _capture = new FeedbackCapturePass(_material);
        _injection = new FeedbackInjectionPass(_material, _capture);

        _capture.renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
        _injection.renderPassEvent = RenderPassEvent.AfterRenderingOpaques;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _capture.Dispose();
            _capture = null;
        }
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData data)
    {
        if (data.cameraData.cameraType != CameraType.Game) return;
        renderer.EnqueuePass(_capture);
        renderer.EnqueuePass(_injection);
    }
}

} // namespace Kino.Feedback.Universal
