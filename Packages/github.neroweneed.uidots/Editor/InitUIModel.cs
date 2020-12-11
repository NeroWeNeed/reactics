using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using NeroWeNeed.Commons;
using NeroWeNeed.Commons.Editor;
using TMPro;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Entities.Serialization;
using Unity.Mathematics;
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEditor.ProjectWindowCallback;
using UnityEditor.Sprites;
using UnityEditor.U2D;
using UnityEngine;
using UnityEngine.U2D;

namespace NeroWeNeed.UIDots.Editor {
    public class InitUIModel : EndNameEditAction {
        [MenuItem("Assets/Create/Dots/UI Model")]
        public static void Create() {
            var path = AssetDatabase.GetAssetPath(Selection.activeObject) + "/UIModel.uidml";
            //var path = AssetDatabase.GetAssetPath(Selection.activeObject) + $"/{name}.asset";
            //ProjectWindowUtil.StartNameEditingIfProjectWindowExists(asset.GetInstanceID(), InitBehaviourGraphModel.Value, path, (Texture2D)EditorGUIUtility.IconContent("ScriptableObject Icon").image, null);
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0, InitUIModel.Value, path, (Texture2D)EditorGUIUtility.IconContent("d_TextAsset Icon").image, null);

        }
        [MenuItem("Assets/Compile")]
        public static void DoCompile() {
            (Selection.activeObject as CompileableObject)?.Compile(forceCompilation: true);
        }

        private static InitUIModel value;
        public static InitUIModel Value => value ?? CreateInstance<InitUIModel>();
        public override void Action(int instanceId, string pathName, string resourceFile) {

            //var model = EditorUtility.InstanceIDToObject(instanceId) as UIModel;
            using (FileStream fs = File.Create(pathName)) {
                var bytes = Encoding.Unicode.GetBytes("<UIGraph></UIGraph>");
                fs.Write(bytes, 0, bytes.Length);
            }
            AssetDatabase.ImportAsset(pathName);
            /* if (model != null) {
                File.Create(pathName);
                File.WriteAllText(pathName, "<UIGraph></UIGraph>");
                AssetDatabase.ImportAsset(pathName);
                //EditorUtility.SetDirty(model);
            } */
        }
        private string GetName(string pathName) {
            var index1 = pathName.LastIndexOf('/');
            var index2 = pathName.LastIndexOf('.');
            return pathName.Substring(index1 + 1, index2 - (index1 + 1));
        }
    }
}