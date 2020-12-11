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
    public class EmitUIMesh : EndNameEditAction {
        [MenuItem("Assets/Compile with Mesh")]
        public unsafe static void EmitMesh() {
            if (Selection.activeObject is UIModel model) {
                model.Compile(forceCompilation: true);
                var asset = AssetDatabase.LoadAssetAtPath<UICompiledModel>($"{model.outputDirectory}/{model.outputFileName}.asset");
                BlobAssetReference<UIGraph> blob;
                fixed (byte* graphPtr = asset.graph) {
                    using (var reader = new MemoryBinaryReader(graphPtr)) {
                        blob = reader.Read<UIGraph>();
                    }
                }
                var configPtr = UnsafeUtility.PinGCArrayAndGetDataAddress(asset.initialConfiguration, out ulong configHandle);
                var length = asset.initialConfiguration.Length;
                var mesh = new Mesh();
                var meshData = Mesh.AllocateWritableMeshData(1);
                var compileJob = new UILayoutJob
                {
                    graph = blob,
                    configuration = configPtr,
                    meshData = meshData
                };
                compileJob.Execute();
                Mesh.ApplyAndDisposeWritableMeshData(meshData, mesh, UnityEngine.Rendering.MeshUpdateFlags.Default);
                mesh.name = "Mesh";
                mesh.RecalculateBounds();
                if (asset.mesh != null) {
                    AssetDatabase.RemoveObjectFromAsset(asset.mesh);
                }
                asset.mesh = mesh;
                UnsafeUtility.ReleaseGCObject(configHandle);
                AssetDatabase.AddObjectToAsset(mesh, $"{model.outputDirectory}/{model.outputFileName}.asset");
                blob.Dispose();
                AssetDatabase.SaveAssets();
            }
            //ProjectWindowUtil.StartNameEditingIfProjectWindowExists(Selection.activeInstanceID, InitUIModel.Value, AssetDatabase.GetAssetPath(Selection.activeObject), (Texture2D)EditorGUIUtility.IconContent("d_TextAsset Icon").image, null);
        }
        public override void Action(int instanceId, string pathName, string resourceFile) {
            if (EditorUtility.InstanceIDToObject(instanceId) is UIModel model) {
                model.Compile(forceCompilation: true);
                using var reader = new StreamBinaryReader($"{model.outputDirectory}/{model.outputFileName}.bytes");
                var blob = reader.Read<UIGraph>();
                var mesh = new Mesh();
                var meshData = Mesh.AllocateWritableMeshData(1);
                var compileJob = new UILayoutJob
                {
                    graph = blob,
                    meshData = meshData
                };
                compileJob.Execute();
                Mesh.ApplyAndDisposeWritableMeshData(meshData, mesh, UnityEngine.Rendering.MeshUpdateFlags.Default);
                mesh.RecalculateBounds();
                mesh.RecalculateTangents();
                AssetDatabase.CreateAsset(mesh, $"{model.outputDirectory}/{model.outputFileName}.mesh");
                AssetDatabase.SaveAssets();
            }

        }

    }
}