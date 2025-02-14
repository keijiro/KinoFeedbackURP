using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering;
using Unity.Mathematics;

namespace Kino.Feedback.Universal {

public enum SampleMode { Point, Bilinear };

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

    [field:SerializeField]
    public SampleMode SampleMode { get; set; } = SampleMode.Point;

    #endregion

    #region Public members exposed for render passes

    public bool IsReady => Properties != null;

    public MaterialPropertyBlock Properties { get; private set; }

    public void PrepareBuffer(int width, int height, GraphicsFormat format)
    {
        if (_buffer == null)
            _buffer = RTHandles.Alloc(Vector3.one, format,
                                      name: "KinoFeedback Buffer");
    }

    public RTHandle FeedbackTexture => _buffer;

    #endregion

    #region Private members

    RTHandle _buffer;

    float3x3 Construct3x3(float m00, float m01, float m02,
                          float m10, float m11, float m12)
      => math.float3x3(m00, m01, m02, m10, m11, m12, 0, 0, 1);

    float4x4 Construct4x4(float3x3 m)
      => math.float4x4(math.float4(m.c0, 0),
                       math.float4(m.c1, 0),
                       math.float4(m.c2, 0),
                       math.float4(0, 0, 0, 1));

    float4x4 CalculateTransformMatrix()
    {
        var rad = math.radians(Rotation);
        var inv_scale = 1.0f / Scale;

        var a = GetComponent<Camera>().aspect;
        var s = math.sin(rad) * inv_scale;
        var c = math.cos(rad) * inv_scale;
        var o = Offset * inv_scale;

        var pre = Construct3x3(a, 0, -0.5f * a, 0, 1, -0.5f);
        var xform = Construct3x3(c, -s, o.x, s, c, o.y);
        var post = Construct3x3(1 / a, 0, 0.5f, 0, 1, 0.5f);
        return Construct4x4(math.mul(math.mul(post, xform), pre));
    }

    #endregion

    #region MonoBehaviour implementation

    void OnDisable()
      => OnDestroy();

    void OnDestroy()
    {
        _buffer?.Release();
        _buffer = null;
    }

    void LateUpdate()
    {
        if (Properties == null) Properties = new MaterialPropertyBlock();

        if (_buffer != null) Properties.SetTexture("_FeedbackTexture", _buffer);

        Properties.SetMatrix("_Transform", CalculateTransformMatrix());
        Properties.SetColor("_Tint", Tint);
        Properties.SetFloat("_HueShift", HueShift);
        Properties.SetFloat("_SampleMode", (int)SampleMode);
    }

    #endregion
}

} // namespace Kino.Feedback.Universal
