using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace NeroWeNeed.UIDots {
    public class UIGlobalSettingsProvider : SettingsProvider {
        public const string UXML = "Packages/github.neroweneed.uidots/Editor/Resources/UIGlobalSettings.uxml";
        [DidReloadScripts]
        private static void UpdateSchema() {
            UIGlobalSettings.GetOrCreateSettings().Refresh();
        }
        private SerializedObject serializedObject;
        public UIGlobalSettingsProvider(string path, SettingsScope scopes, System.Collections.Generic.IEnumerable<string> keywords = null) : base(path, scopes, keywords) { }

        [SettingsProvider]
        public static SettingsProvider CreateCustomSettingsProvider() {
            return new UIGlobalSettingsProvider("Project/UI Settings", SettingsScope.Project)
            {
                label = "UI Settings"
            };
        }
        public override void OnActivate(string searchContext, VisualElement rootElement) {
            serializedObject = UIGlobalSettings.GetOrCreateSerializedSettings();
            var uxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(UXML);
            if (rootElement == null) {
                rootElement = new VisualElement();
            }
            else {
                rootElement.Clear();
            }
            uxml.CloneTree(rootElement);
            rootElement.Bind(serializedObject);
            rootElement.Q<Button>("refresh-schema").clicked += () => (serializedObject.targetObject as UIGlobalSettings)?.Refresh();
        }
    }
}