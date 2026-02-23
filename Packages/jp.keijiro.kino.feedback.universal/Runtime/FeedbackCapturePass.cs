using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.Universal;

namespace Kino.Feedback.Universal {

// Capture pass (before post-processing)
// Copies the frame buffer into the feedback texture.
sealed class FeedbackCapturePass : ScriptableRenderPass
{
    public override void RecordRenderGraph
      (RenderGraph graph, ContextContainer context)
    {
        var resource = context.Get<UniversalResourceData>();
        if (resource.isActiveTargetBackBuffer) return;

        var camera = context.Get<UniversalCameraData>().camera;
        var driver = camera.GetComponent<FeedbackController>();
        if (driver == null || !driver.enabled || !driver.IsReady) return;

        var source = resource.activeColorTexture;

        var desc = graph.GetTextureDesc(source);
        driver.PrepareBuffer(desc.width, desc.height, desc.format);
        var buffer = graph.ImportTexture(driver.FeedbackTexture);

        var mat = Blitter.GetBlitMaterial(TextureDimension.Tex2D);
        var param = new RenderGraphUtils.BlitMaterialParameters(source, buffer, mat, 0);
        graph.AddBlitPass(param, passName: "KinoFeedback (capture)");
    }
}

} // namespace Kino.Feedback.Universal
