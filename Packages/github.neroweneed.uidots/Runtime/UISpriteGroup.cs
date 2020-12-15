using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace NeroWeNeed.UIDots {
    [CreateAssetMenu(fileName = "UISpriteGroup", menuName = "UIDots/UI Sprite Group", order = 0)]
    public class UISpriteGroup : ScriptableObject {
        public static UISpriteGroup Find(string name) {
            return AssetDatabase.FindAssets($"t:{nameof(UISpriteGroup)}").Select(a => AssetDatabase.LoadAssetAtPath<UISpriteGroup>(AssetDatabase.GUIDToAssetPath(a))).FirstOrDefault(a => a.identifier == name);
        }
        public string identifier;
        [SerializeField]
        private List<TextureRef> textures = new List<TextureRef>();
        [SerializeField]
        private int maxSize = 1024;
        [SerializeField, HideInInspector]
        private Texture2D texture;
        [SerializeField, HideInInspector]
        private UV[] uvs;

        [SerializeField, HideInInspector]
        private int hash;
        public bool IsEmpty { get => textures.Count <= 0; }
        public int2 TextureSize { get => texture == null ? int2.zero : new int2(texture.width, texture.height); }
        public bool IsTextureDirty(out int generatedHash) {
            generatedHash = GenerateHash();
            return hash != generatedHash || (texture != null && this.textures.Count > 0);
        }
        private int GenerateHash() {
            int hash = -553370464;
            this.textures.Sort();
            foreach (var texture in textures) {
                hash = (hash * -1521134295) + texture.guid.GetHashCode();
            }
            return hash;
        }
        public Rect this[string guid]
        {
            get
            {
                if (IsTextureDirty(out int _)) {
                    GenerateTexture();
                }
                return (uvs?.FirstOrDefault(uv => uv.guid == guid).rect) ?? default;
            }
        }
        public bool TryGetUVs(string guid, out Rect uv) {
            if (IsTextureDirty(out int _)) {
                GenerateTexture();
            }
            var index = Array.FindIndex(uvs, uv => uv.guid == guid);
            if (index < 0) {
                uv = default;
                return false;
            }
            else {
                uv = uvs[index].rect;
                return true;
            }
        }
        public void Add(UIModel source, params Texture2D[] textures) => Add(source, textures.Select(t => AssetDatabase.GetAssetPath(t)).Where(t => t != null).Select(t => AssetDatabase.GUIDFromAssetPath(t).ToString()));
        public void Add(UIModel source, params string[] guids) => Add(source, guids.AsEnumerable());
        public void Add(UIModel source, IEnumerable<string> guids) => Add(AssetDatabase.GUIDFromAssetPath(AssetDatabase.GetAssetPath(source)).ToString(), guids);
        public void Add(string modelGuid, IEnumerable<string> guids) {
            foreach (var guid in guids) {
                var srcIndex = this.textures.FindIndex(a => a?.guid == guid);
                if (srcIndex < 0) {
                    var e = new TextureRef(guid);
                    e.referencedBy.Add(modelGuid);
                    this.textures.Add(e);
                }
                else {
                    var e = this.textures[srcIndex];
                    if (!e.referencedBy.Contains(modelGuid))
                        e.referencedBy.Add(modelGuid);
                }
            }
            this.textures.Sort();
        }
        public void Remove(UIModel source, params string[] guids) => Remove(source, guids.AsEnumerable());
        public void Remove(UIModel source, IEnumerable<string> guids) {
            var modelGuid = AssetDatabase.GUIDFromAssetPath(AssetDatabase.GetAssetPath(source)).ToString();

            foreach (var guid in guids) {
                var srcIndex = this.textures.FindIndex(a => a?.guid == guid);
                if (srcIndex >= 0) {
                    var e = this.textures[srcIndex];
                    if (e.referencedBy.Remove(modelGuid) && e.referencedBy.Count <= 0) {
                        this.textures[srcIndex] = null;
                    }
                }
            }
            this.textures.RemoveAll(a => a == null);
            this.textures.Sort();
        }
        public void Remove(UIModel source, params Texture2D[] textures) {
            Remove(source, textures.Select(t => AssetDatabase.GetAssetPath(t)).Where(t => t != null).Select(t => AssetDatabase.GUIDFromAssetPath(t).ToString()));
        }
        public Texture2D GenerateTexture() {
            if (IsTextureDirty(out int hash)) {
                if (this.texture != null) {
                    AssetDatabase.RemoveObjectFromAsset(this.texture);
                    this.texture = null;
                }

                if (textures?.Count > 0) {
                    Texture2D[] textures = this.textures.Where(t => t != null).Select(t => AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath(t.guid))).ToArray();

                    var texture = new Texture2D(maxSize, maxSize);
                    texture.name = "Atlas";
                    var rects = texture.PackTextures(textures, 0, maxSize);
                    var uvs = new UV[textures.Length];
                    for (int i = 0; i < uvs.Length; i++) {
                        uvs[i] = new UV(AssetDatabase.GUIDFromAssetPath(AssetDatabase.GetAssetPath(textures[i])).ToString(), rects[i]);
                    }
                    this.uvs = uvs;
                    this.texture = texture;
                    AssetDatabase.AddObjectToAsset(this.texture, AssetDatabase.GetAssetPath(this));
                    this.hash = hash;
                    return texture;
                }
                else {
                    this.hash = hash;
                    return null;
                }
            }
            else {
                return texture;
            }

        }
        [Serializable]
        public class TextureRef : IComparable<TextureRef> {
            public string guid;
            public List<string> referencedBy;
            public TextureRef(string guid) {
                this.guid = guid ?? string.Empty;
                this.referencedBy = new List<string>();
            }

            public int CompareTo(TextureRef other) {
                return other == null ? 1 : this.guid.CompareTo(other.guid);

            }
        }
        [Serializable]
        public struct UV {
            public string guid;
            public Rect rect;

            public UV(string guid, Rect rect) {
                this.guid = guid;
                this.rect = rect;
            }
        }


    }
}