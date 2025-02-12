using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering;

namespace Kino.Feedback.Universal {

[RequireComponent(typeof(Camera))]
[AddComponentMenu("Kino/Feedback Effect")]
public sealed class FeedbackEffect : MonoBehaviour
{
    #region Public members exposed for render passes

    public bool IsReady => Properties != null;

    public MaterialPropertyBlock Properties { get; private set; }

    public void PrepareBuffer(int width, int height)
    {
        if (_buffer == null)
            _buffer = RTHandles.Alloc(width, height,
                                      GraphicsFormat.R8G8B8A8_SRGB,
                                      name: "KinoFeedback Buffer");
    }

    public RTHandle Buffer => _buffer;

    #endregion

    #region Private members

    RTHandle _buffer;

    #endregion

    #region MonoBehaviour implementation

    void OnDestroy()
      => _buffer?.Release();

    void LateUpdate()
    {
        if (Properties == null) Properties = new MaterialPropertyBlock();
        Properties.SetFloat("_Test", 1);
    }

    #endregion
}

} // namespace Kino.Feedback.Universal
