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
using UnityEngine.Rendering;

namespace NeroWeNeed.UIDots {

    public class UIObject : MonoBehaviour {
        public UIModel model;
        public new Camera camera;
        public float pixelScale = 1f;


        [SerializeField, HideInInspector]
        public string cachedGuid;
        [SerializeField, HideInInspector]
        public Mesh cachedMesh;
        public Bounds Bounds { get => cachedMesh == null ? default : cachedMesh.bounds; }
        public Alignment alignment;
        public UILength offsetX, offsetY;
    }
}