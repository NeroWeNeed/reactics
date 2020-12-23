using System;
using System.Collections.Generic;
using NeroWeNeed.Commons;
using NeroWeNeed.UIDots.Editor;
using UnityEngine;
namespace NeroWeNeed.UIDots {
    public class UIModel : ScriptableObject {
        [HideInInspector]
        public List<string> assets = new List<string>();
        [HideInInspector]
        public List<Node> nodes = new List<Node>();
        public UIAssetGroup group;
        private void OnDestroy() {
#if UNITY_EDITOR
            group?.Remove(this, this.assets);
#endif
        }
        
        [Serializable]
        public struct Node {
            public string identifier;
            public string name;
            public SerializableMethod pass;
            public List<Property> properties;
            public List<int> children;
            public int parent;
            public ulong mask;

            [Serializable]
            public struct Property {
                public string path;
                public string value;
                public string Path { get => path; }
                public string Value { get => value; }

            }
        }

    }
}