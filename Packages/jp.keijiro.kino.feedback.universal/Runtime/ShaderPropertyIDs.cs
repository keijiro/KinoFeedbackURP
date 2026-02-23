using UnityEngine;

namespace Kino.Feedback.Universal {

static class ShaderPropertyIDs
{
    public static readonly int FeedbackTexture = Shader.PropertyToID("_FeedbackTexture");
    public static readonly int Transform = Shader.PropertyToID("_Transform");
    public static readonly int Tint = Shader.PropertyToID("_Tint");
    public static readonly int HueShift = Shader.PropertyToID("_HueShift");
    public static readonly int SampleMode = Shader.PropertyToID("_SampleMode");
}

} // namespace Kino.Feedback.Universal
