using UnityEngine;

namespace Kino.Feedback.Universal {

[RequireComponent(typeof(Camera))]
[AddComponentMenu("Kino/Feedback Effect")]
public sealed class FeedbackEffect : MonoBehaviour
{
    #region Public properties

    public bool IsReady => Properties != null;
    public MaterialPropertyBlock Properties { get; private set; }

    #endregion

    #region MonoBehaviour implementation

    void LateUpdate()
    {
        if (Properties == null) Properties = new MaterialPropertyBlock();
        Properties.SetFloat("_Test", 1);
    }

    #endregion
}

} // namespace Kino.Feedback.Universal
