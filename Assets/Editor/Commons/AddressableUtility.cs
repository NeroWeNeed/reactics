using System;
using System.Linq;
using Reactics.Core.Battle;
using Reactics.Core.Commons;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Reactics.Editor {

    public static class AddressableUtility {
        public static AssetReference GetAssetReference(this SerializedProperty property) {
            var guid = property.FindPropertyRelative("m_AssetGUID").stringValue;
            var subObjectName = property.FindPropertyRelative("m_SubObjectName").stringValue;
            Debug.Log(subObjectName);
            return new AssetReference(guid)
            {
                SubObjectName = subObjectName
            };
        }
        public static TResult LoadAsset<TResult>(this SerializedProperty property) where TResult : UnityEngine.Object {
            var reference = GetAssetReference(property);
            if (string.IsNullOrEmpty(reference.SubObjectName)) {
                return reference.editorAsset as TResult;
            }
            else {

                return System.Array.Find(AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GUIDToAssetPath(reference.AssetGUID)), (a) => a is TResult && a.name == reference.SubObjectName) as TResult;
            }

        }
        public static UnityEngine.Object ResolveEditorAsset(this AssetReference reference) {
            if (reference?.RuntimeKeyIsValid() != true)
                return null;
            if (string.IsNullOrEmpty(reference.SubObjectName)) {
                return reference.editorAsset;
            }
            else {
                var name = reference.SubObjectName;
                return System.Array.Find(AssetDatabase.LoadAllAssetRepresentationsAtPath(AssetDatabase.GetAssetPath(reference.editorAsset)), (obj) => obj.name == reference.SubObjectName);
            }
        }
        public static UnityEngine.Object ResolveEditorAsset(this SerializedProperty property) {
            var reference = property.GetAssetReference();
            if (reference?.RuntimeKeyIsValid() != true)
                return null;
            if (string.IsNullOrEmpty(reference.SubObjectName)) {
                return reference.editorAsset;
            }
            else {
                var name = reference.SubObjectName;
                return System.Array.Find(AssetDatabase.LoadAllAssetRepresentationsAtPath(AssetDatabase.GetAssetPath(reference.editorAsset)), (obj) => obj.name == reference.SubObjectName);
            }
        }
        public static void WriteAsset(this SerializedProperty property, UnityEngine.Object asset) {
            if (asset == null)
                return;
            var reference = AddressableAssetSettingsDefaultObject.Settings.CreateAssetReference(AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(asset)));
            var guid = property.FindPropertyRelative("m_AssetGUID");
            var subObject = property.FindPropertyRelative("m_SubObjectName");
            guid.stringValue = reference.AssetGUID;
            subObject.stringValue = AssetDatabase.IsMainAsset(asset) ? null : asset.name;

        }
        public static void CreateOrAttachObject<TObject>(this AssetReference reference, SerializedProperty property, string root, out SerializedObject obj) where TObject : UnityEngine.ScriptableObject {
            if (!reference.RuntimeKeyIsValid()) {
                var subObjs = AssetDatabase.LoadAllAssetRepresentationsAtPath(root);
                var asset = Array.Find(subObjs, (s) => s.name == property.name);
                if (asset == null) {
                    asset = ScriptableObject.CreateInstance<TObject>();
                    asset.name = property.name;
                    AssetDatabase.AddObjectToAsset(asset, root);
                    AssetDatabase.ImportAsset(root);
                }
                property.WriteAsset(asset);
                obj = new SerializedObject(asset);

            }
            else {
                obj = new SerializedObject(reference.ResolveEditorAsset());

            }
        }

    }
}