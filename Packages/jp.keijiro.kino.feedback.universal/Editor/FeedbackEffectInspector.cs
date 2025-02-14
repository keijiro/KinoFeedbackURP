using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Kino.Feedback.Universal {
    
[CustomEditor(typeof(FeedbackEffect))]
public sealed class FeedbackEffectInspector : Editor
{
    public VisualTreeAsset _xml;

    public override VisualElement CreateInspectorGUI()
    {
        _xml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>
          ("Packages/jp.keijiro.kino.feedback.universal/Editor/FeedbackEffect.uxml");
        return _xml.Instantiate();
    }
}

} // namespace Kino.Feedback.Universal
