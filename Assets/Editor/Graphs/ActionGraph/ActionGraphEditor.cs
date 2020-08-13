using System;
using Reactics.Core.Battle;
using Reactics.Core.Effects;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.Callbacks;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Reactics.Core.Editor.Graph {
    public class ActionGraphEditor : ObjectGraphEditor<ActionAsset> {
        [OnOpenAsset(1)]
        public static bool OnOpen(int instanceId, int line) => OnOpen<ActionGraphEditor>(instanceId, line, (asset) => $"Action Graph ({asset.name})");

        protected override string SaveFileInPanelTitle => "Save Action As...";

        protected override string SaveFileInPanelDefaultName => "ActionAsset";

        protected override string SaveFileInPanelPath => "Assets/ResourceData/Actions";

        private EffectGraphModule effectGraphModule => Modules[0] as EffectGraphModule;
        private TargetFilterGraphModule targetFilterGraphModule => Modules[1] as TargetFilterGraphModule;
        private ActionGraphModule actionGraphModule => Modules[2] as ActionGraphModule;

        public ActionGraphEditor() : base(new EffectGraphModule(), new TargetFilterGraphModule(), new ActionGraphModule()) {

        }

        protected override void DoLoad(SerializedObject obj) {
            var effectAssetReference = obj.FindProperty(ActionGraphModule.EFFECT_ASSET_PATH);

            if (effectAssetReference != null) {
                var effectAsset = effectAssetReference.ResolveEditorAsset();

                if (effectAsset != null) {
                    SerializedObject effectAssetObj = new SerializedObject(effectAsset);
                    effectGraphModule.Deserialize(effectAssetObj, graphView);
                }
            }
            var targetFilterAssetReference = obj.FindProperty(ActionGraphModule.TARGET_FILTER_ASSET_PATH);
            if (targetFilterAssetReference != null) {
                var targetFilterAsset = AddressableUtility.LoadAsset<TargetFilterAsset>(targetFilterAssetReference);
                if (targetFilterAsset != null) {
                    SerializedObject targetFilterObj = new SerializedObject(targetFilterAsset);
                    targetFilterGraphModule.Deserialize(targetFilterObj, graphView);
                }
            }
        }

        protected async override void DoSave(SerializedObject obj) {
            var effectAssetReferenceProperty = obj.FindProperty(ActionGraphModule.EFFECT_ASSET_PATH);
            var targetFilterAssetReferenceProperty = obj.FindProperty(ActionGraphModule.TARGET_FILTER_ASSET_PATH);
            var effectAssetReference = effectAssetReferenceProperty.GetAssetReference();
            var targetFilterAssetReference = targetFilterAssetReferenceProperty.GetAssetReference();
            var path = AssetDatabase.GetAssetPath(obj.targetObject);
            effectAssetReference.CreateOrAttachObject<EffectAsset>(effectAssetReferenceProperty, path, out SerializedObject effectAssetObj);
            targetFilterAssetReference.CreateOrAttachObject<TargetFilterAsset>(targetFilterAssetReferenceProperty, path, out SerializedObject targetFilterObj);
            obj.ApplyModifiedProperties();
            //TODO: Delete SubAssets if they aren't referenced by the main asset anymore.   
            effectGraphModule.Serialize(effectAssetObj, graphView);
            targetFilterGraphModule.Serialize(targetFilterObj, graphView);
        }

    }
}