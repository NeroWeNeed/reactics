using Reactics.Battle;
using Reactics.Battle.Unit;
using Reactics.Editor;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class SampleWindow : EditorWindow {

    [MenuItem("Reactics/SampleWindow")]
    private static void ShowWindow() {
        var window = GetWindow<SampleWindow>();
        window.titleContent = new GUIContent("SampleWindow");
        window.Show();
    }
    public string guid = "121a4480892ccac4aba9c6846296bd50";
    private SerializedObject obj;
    private void OnEnable() {
        obj = new SerializedObject(AssetDatabase.LoadAssetAtPath<ActionAsset>(AssetDatabase.GUIDToAssetPath(guid)));
        rootVisualElement.Clear();

        var inspector = VisualElementDrawers.CreateInspector(obj, "sample");
        rootVisualElement.Add(inspector);

        /*         rootVisualElement.Add(new TypeSearchField("Sample"));
       rootVisualElement.Add(new AssetReferenceSearchField());
       rootVisualElement.Add(new Label("Sample")); */
        rootVisualElement.style.flexGrow = 1;
        rootVisualElement.style.flexShrink = 0;

    }
    private void OnDisable() {

    }


}