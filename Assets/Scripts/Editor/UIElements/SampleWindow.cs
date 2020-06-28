using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using Reactics.Editor;
using UnityEngine.UIElements;

public class SampleWindow : EditorWindow
{

    [MenuItem("Reactics/SampleWindow")]
    private static void ShowWindow()
    {
        var window = GetWindow<SampleWindow>();
        window.titleContent = new GUIContent("SampleWindow");
        window.Show();
    }

    private void OnEnable()
    {
        rootVisualElement.Clear();
        rootVisualElement.Add(new TypeSearchField("Sample"));
        rootVisualElement.Add(new AssetReferenceSearchField());
        rootVisualElement.Add(new Label("Sample"));
        rootVisualElement.style.flexGrow = 1;
        rootVisualElement.style.flexShrink = 0;

    }

}