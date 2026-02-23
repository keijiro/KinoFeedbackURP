using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

namespace Kino.Feedback.Universal {

// Injection pass (after opaque objects)
// Draws a far-plane quad with the feedback texture.
sealed class FeedbackInjectionPass : ScriptableRenderPass
{
    class PassData
    {
        public FeedbackController Driver;
        public Material Material;
    }

    public override void RecordRenderGraph
      (RenderGraph graph, ContextContainer context)
    {
        var camera = context.Get<UniversalCameraData>().camera;
        var driver = camera.GetComponent<FeedbackController>();
        if (driver == null || !driver.enabled || !driver.IsReady) return;
        if (driver.FeedbackTexture == null) return; // First frame rejection

        using var builder = graph.AddRasterRenderPass<PassData>
          ("KinoFeedback (Injection)", out var passData);

        passData.Driver = driver;
        passData.Material = driver.UpdateMaterial();

        var resource = context.Get<UniversalResourceData>();
        builder.SetRenderAttachment(resource.activeColorTexture, 0);
        builder.SetRenderAttachmentDepth(resource.activeDepthTexture, AccessFlags.Read);

        builder.SetRenderFunc<PassData>((data, ctx) => ExecutePass(data, ctx));
    }

    static void ExecutePass(PassData data, RasterGraphContext context)
      => CoreUtils.DrawFullScreen(context.cmd, data.Material, data.Driver.Properties, 0);
}

} // namespace Kino.Feedback.Universal
