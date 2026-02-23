using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Kino.Feedback.Universal {
    
[CustomEditor(typeof(FeedbackController))]
public sealed class FeedbackControllerInspector : Editor
{
    public VisualTreeAsset _xml;

    public override VisualElement CreateInspectorGUI()
    {
        _xml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>
          ("Packages/jp.keijiro.kino.feedback.universal/Editor/FeedbackController.uxml");
        return _xml.Instantiate();
    }
}

} // namespace Kino.Feedback.Universal
