using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Kino.Feedback.Universal {

public sealed class FeedbackRendererFeature : ScriptableRendererFeature
{
    FeedbackInjectionPass _injection;
    FeedbackCapturePass _capture;

    public override void Create()
    {
        _injection = new FeedbackInjectionPass();
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
