using System;
using Reactics.Battle;
using Reactics.Battle.Unit;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace Reactics.Editor.Graph {
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
            var effectAssetReference = obj.FindProperty("effectAsset");

            if (effectAssetReference?.objectReferenceValue != null) {

                SerializedObject effectAssetObj = new SerializedObject(effectAssetReference.objectReferenceValue);
                effectGraphModule.Deserialize(effectAssetObj, graphView);
            }
            var targetFilterAssetReference = obj.FindProperty("targetAsset");
            if (targetFilterAssetReference?.objectReferenceValue != null) {
                SerializedObject targetFilterObj = new SerializedObject(targetFilterAssetReference.objectReferenceValue);
                targetFilterGraphModule.Deserialize(targetFilterObj, graphView);
            }
        }
        protected override void DoSave(SerializedObject obj) {
            var effectAssetReference = obj.FindProperty("effectAsset");
            var targetFilterAssetReference = obj.FindProperty("targetAsset");
            SerializedObject effectAssetObj;
            SerializedObject targetFilterObj;
            var path = AssetDatabase.GetAssetPath(obj.targetObject);
            var subObjs = AssetDatabase.LoadAllAssetRepresentationsAtPath(path);
            if (effectAssetReference?.objectReferenceValue == null) {
                var effectAsset = Array.Find(subObjs, (s) => s.name == "effect");
                if (effectAsset == null) {
                    effectAsset = CreateInstance<EffectAsset>();
                    effectAsset.name = "effect";
                    AssetDatabase.AddObjectToAsset(effectAsset, path);
                    AssetDatabase.ImportAsset(path);
                }
                effectAssetReference.objectReferenceValue = effectAsset;
                obj.ApplyModifiedPropertiesWithoutUndo();
                effectAssetObj = new SerializedObject(effectAsset);
            }
            else {
                effectAssetObj = new SerializedObject(effectAssetReference.objectReferenceValue);
            }

            if (targetFilterAssetReference?.objectReferenceValue == null) {
                var targetFilterAsset = Array.Find(subObjs, (s) => s.name == "target_filter");
                if (targetFilterAsset == null) {
                    targetFilterAsset = CreateInstance<TargetFilterAsset>();
                    targetFilterAsset.name = "target_filter";
                    AssetDatabase.AddObjectToAsset(targetFilterAsset, path);
                    AssetDatabase.ImportAsset(path);
                }
                targetFilterAssetReference.objectReferenceValue = targetFilterAsset;
                obj.ApplyModifiedPropertiesWithoutUndo();
                targetFilterObj = new SerializedObject(targetFilterAsset);
            }
            else {
                targetFilterObj = new SerializedObject(targetFilterAssetReference.objectReferenceValue);
            }
            //TODO: Delete SubAssets if they aren't referenced by the main asset anymore.   
            effectGraphModule.Serialize(effectAssetObj, graphView);
            targetFilterGraphModule.Serialize(targetFilterObj, graphView);
        }

    }
}