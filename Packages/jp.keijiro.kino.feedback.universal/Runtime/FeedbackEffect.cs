using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering;

namespace Kino.Feedback.Universal {

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
[AddComponentMenu("Kino/Feedback Effect")]
public sealed class FeedbackEffect : MonoBehaviour
{
    #region Public properties (serialized)

    [field:SerializeField, ColorUsage(false)]
    public Color Tint { get; set; } = Color.white;

    [field:SerializeField]
    public float HueShift { get; set; }

    [field:SerializeField]
    public Vector2 Offset { get; set; }

    [field:SerializeField]
    public float Rotation { get; set; }

    [field:SerializeField]
    public float Scale { get; set; } = 1.1f;

    #endregion

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

    Vector4 GetTransformVector()
    {
        var r = Mathf.Deg2Rad * Rotation;
        return new Vector4(Mathf.Sin(r), Mathf.Cos(r), Offset.x, Offset.y) / Scale;
    }

    #endregion

    #region MonoBehaviour implementation

    void OnDestroy()
      => _buffer?.Release();

    void LateUpdate()
    {
        if (Properties == null) Properties = new MaterialPropertyBlock();

        if (_buffer != null) Properties.SetTexture("_FeedbackTexture", _buffer);

        Properties.SetVector("_Transform", GetTransformVector());
        Properties.SetColor("_Tint", Tint);
        Properties.SetFloat("_HueShift", HueShift);
    }

    #endregion
}

} // namespace Kino.Feedback.Universal
