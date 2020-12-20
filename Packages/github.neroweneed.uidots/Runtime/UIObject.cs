using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Entities.Serialization;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Rendering;

namespace NeroWeNeed.UIDots {

    public class UIObject : MonoBehaviour {
        [SerializeField]
        public UIModel model;
        [SerializeField]
        public UICamera uiCamera;
        [SerializeField]
        public bool faceCamera;
        [SerializeField]
        public bool screenUI;
        [SerializeField, HideInInspector]
        internal Mesh cachedMesh;
        [SerializeField, HideInInspector]
        internal Material cachedMaterial;
        [SerializeField, HideInInspector]
        internal BlobAssetReference<UIGraph> cachedBlob;
        internal List<UINodeDecompositionJob.NodeInfo> cachedNodeData = new List<UINodeDecompositionJob.NodeInfo>();
        public Bounds Bounds { get => cachedMesh == null ? default : cachedMesh.bounds; }
        public Mesh CachedMesh { get => cachedMesh; }
    }
}