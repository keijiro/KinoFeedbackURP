using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Kino.Feedback.Universal {
    
[CustomEditor(typeof(FeedbackController))]
public sealed class FeedbackControllerInspector : Editor
{
    public VisualTreeAsset _uxml;
    public override VisualElement CreateInspectorGUI() => _uxml.CloneTree();
}

} // namespace Kino.Feedback.Universal
