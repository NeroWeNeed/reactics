using System;
using System.IO;
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEngine;

namespace NeroWeNeed.UIDots.Editor {
    [ScriptedImporter(1, "uidml")]
    public class UIModelImporter : ScriptedImporter {
        public override void OnImportAsset(AssetImportContext ctx) {
            var asset = ScriptableObject.CreateInstance<UIModel>();
            asset.Initialize(new StringReader(File.ReadAllText(ctx.assetPath)), AssetDatabase.GUIDFromAssetPath(ctx.assetPath).ToString(),ctx.assetPath);
            ctx.AddObjectToAsset("UI Model", asset);
            ctx.SetMainObject(asset);
        }
    }
}