using Unity.Collections;
using UnityEngine;
namespace NeroWeNeed.UIDots {

    [PreferBinarySerialization]
    public class UICompiledModel : ScriptableObject {
        [SerializeField]
        public byte[] graph;
        [SerializeField]
        public byte[] initialConfiguration;
        public Mesh mesh;
        public Texture2D atlas;

    }
}