using Unity.Collections;
using UnityEngine;
namespace NeroWeNeed.UIDots {

    [PreferBinarySerialization]
    public class UICompiledModel : ScriptableObject {
        [SerializeField, HideInInspector]
        public byte[] graph;
        [SerializeField, HideInInspector]
        public byte[] initialConfiguration;

        public Material material;
        public Mesh mesh;
        public Texture2D atlas;

    }
}