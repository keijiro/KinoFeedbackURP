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

    public void PrepareBuffer(int width, int height, GraphicsFormat format)
    {
        if (_buffer == null)
            _buffer = RTHandles.Alloc(width, height, format,
                                      name: "KinoFeedback Buffer");
    }

    public RTHandle FeedbackTexture => _buffer;

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
        if (_buffer != null)
            Properties.SetTexture("_FeedbackTexture", _buffer);
    }

    #endregion
}

} // namespace Kino.Feedback.Universal
