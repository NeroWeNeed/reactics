using System.Linq;
using Reactics.Core.Battle;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UIElements;

namespace Reactics.Core.Editor.Graph {
    public class ActionGraphModule : IObjectGraphModule, IInspectorConfigurator {
        public const string TARGET_FILTER_ASSET_PATH = "targetFilterAsset";

        public const string EFFECT_ASSET_PATH = "effectAsset";
        private SerializedObject serializedObject;
        public VisualElement CreateInspectorSection(SerializedObject obj, ObjectGraphView graphView) {
            var container = new VisualElement();
            var infoField = obj.FindProperty("info");
            container.Add(new PropertyField(infoField));
            var effectAssetProperty = obj.FindProperty(EFFECT_ASSET_PATH);
            var effectAssetField = new AssetReferenceSearchField(effectAssetProperty);
            effectAssetField.RegisterValueChangedCallback((evt) => OnAssetReferenceSearchFieldValueChange<EffectGraphNode>(evt, obj));
            effectAssetField.AddToClassList("property-field");

            container.Add(effectAssetField);
            var targetFilterAssetProperty = obj.FindProperty(TARGET_FILTER_ASSET_PATH);
            var targetFilterField = new AssetReferenceSearchField(targetFilterAssetProperty);
            targetFilterField.RegisterValueChangedCallback((evt) => OnAssetReferenceSearchFieldValueChange<TargetFilterGraphNode>(evt, obj));
            targetFilterField.AddToClassList("property-field");
            container.Add(targetFilterField);
            container.Bind(obj);

            Debug.Log(effectAssetField.value);
            return container;
        }
        private void OnAssetReferenceSearchFieldValueChange<TNode>(ChangeEvent<AssetReference> evt, SerializedObject obj) where TNode : ObjectGraphNode {
            var asset = evt.newValue?.ResolveEditorAsset();
            var element = evt.currentTarget as BindableElement;
            if (asset != null && EditorUtility.DisplayDialog("", $"Would you like to load the new Target {asset.GetType().Name}?", "Yes", "No")) {
                var graphView = element?.GetFirstAncestorOfType<ObjectGraphView>();

                graphView?.Clean<TNode>();
                if (asset != null) {
                    SerializedObject effectAssetObj = new SerializedObject(asset);
                    graphView.Modules.OfType<EffectGraphModule>().First().Deserialize(effectAssetObj, graphView);
                }
            }
            if (element != null) {
                var prop = obj.FindProperty(element.bindingPath);
                prop.FindPropertyRelative("m_AssetGUID").stringValue = evt.newValue.AssetGUID;
                prop.FindPropertyRelative("m_SubObjectName").stringValue = evt.newValue.SubObjectName;
                obj.ApplyModifiedProperties();
            }


        }
        /*         private BindableElement ConstructInspector(SerializedObject obj, params string[] propertyNames) {
                    var container = new BindableElement();
                    foreach (var name in propertyNames) {
                        var prop = obj.FindProperty(name);
                        if (prop != null) {
                            container.Add(new PropertyField(prop)
                            {
                                name = name
                            });
                        }
                    }
                    container.Bind(obj);
                    return container;
                } */
    }
}