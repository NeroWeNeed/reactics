using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;

namespace NeroWeNeed.UIDots.Editor {

    [CreateAssetMenu(fileName = "UIAssetGroup", menuName = "UIDots/UI Asset Group", order = 0)]
    public class UIAssetGroup : ScriptableObject {
#if UNITY_EDITOR
        public static UIAssetGroup Find(string name) {
            return UnityEditor.AssetDatabase.FindAssets($"t:{nameof(UIAssetGroup)}").Select(a =>
            UnityEditor.AssetDatabase.LoadAssetAtPath<UIAssetGroup>(
                UnityEditor.AssetDatabase.GUIDToAssetPath(a)
                )
                ).FirstOrDefault(a => a?.identifier == name);
        }
        public const string SHADER_ASSET = "Packages/github.neroweneed.uidots/Runtime/Resources/UIShader.shadergraph";
#endif
        public string identifier;
        [SerializeField]
        private List<Reference> references = new List<Reference>();

        [SerializeField]
        private int maxSize = 1024;
        [SerializeField, HideInInspector]
        private Texture2DArray sdfs;
        [SerializeField, HideInInspector]
        private Texture2D atlas;
        [SerializeField, HideInInspector]
        private Material material;
        [SerializeField, HideInInspector]
        private UV[] uvs;

        [SerializeField, HideInInspector]
        private int hash;
        public bool IsEmpty { get => references.Count <= 0; }
        public int2 TextureSize { get => sdfs == null ? int2.zero : new int2(sdfs.width, sdfs.height); }
        public int GetAtlasIndex(string guid) {
            int index = 0;
            foreach (var reference in this.references) {
                if (reference.guid == guid) {
                    return index;
                }
                index += reference.isAtlas ? 1 : 0;
            }
            return -1;

        }
        public bool IsTextureDirty(out int generatedHash) {
            generatedHash = GenerateHash();
            return hash != generatedHash || (sdfs != null && this.references.Count > 0);
        }
        private int GenerateHash() {
            int hash = -553370464;
            this.references.Sort();
            foreach (var texture in references) {
                hash = (hash * -1521134295) + texture.guid.GetHashCode();
            }
            return hash;
        }
        public Rect this[string guid]
        {
            get
            {
#if UNITY_EDITOR
                if (IsTextureDirty(out int _)) {
                    UpdateMaterial();
                }
#endif
                return (uvs?.FirstOrDefault(uv => uv.guid == guid).rect) ?? default;
            }
        }
        public bool TryGetUVs(string guid, out Rect uv) {
#if UNITY_EDITOR
            if (IsTextureDirty(out int _)) {
                UpdateMaterial();
            }
#endif
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
#if UNITY_EDITOR
        public void Add(UIModel source, params Texture2D[] textures) => Add(source, textures.Select(t => UnityEditor.AssetDatabase.GetAssetPath(t)).Where(t => t != null).Select(t => UnityEditor.AssetDatabase.GUIDFromAssetPath(t).ToString()));
        public void Add(UIModel source, params string[] guids) => Add(source, guids.AsEnumerable());
        public void Add(UIModel source, IEnumerable<string> guids) => Add(UnityEditor.AssetDatabase.GUIDFromAssetPath(UnityEditor.AssetDatabase.GetAssetPath(source)).ToString(), guids);
        public void Add(string modelGuid, IEnumerable<string> guids) {
            foreach (var guid in guids) {
                var srcIndex = this.references.FindIndex(a => a?.guid == guid);
                if (srcIndex < 0) {
                    var e = new Reference(guid);
                    e.referencedBy.Add(modelGuid);
                    e.isAtlas = UnityEditor.AssetDatabase.GetMainAssetTypeAtPath(UnityEditor.AssetDatabase.GUIDToAssetPath(guid)) == typeof(TMP_FontAsset);
                    this.references.Add(e);
                }
                else {
                    var e = this.references[srcIndex];
                    if (!e.referencedBy.Contains(modelGuid))
                        e.referencedBy.Add(modelGuid);
                }
            }
            this.references.Sort();
        }
        public void Remove(UIModel source, params string[] guids) => Remove(source, guids.AsEnumerable());
        public void Remove(UIModel source, IEnumerable<string> guids) {
            var modelGuid = UnityEditor.AssetDatabase.GUIDFromAssetPath(UnityEditor.AssetDatabase.GetAssetPath(source)).ToString();
            foreach (var guid in guids) {
                var srcIndex = this.references.FindIndex(a => a?.guid == guid);
                if (srcIndex >= 0) {
                    var e = this.references[srcIndex];
                    if (e.referencedBy.Remove(modelGuid) && e.referencedBy.Count <= 0) {
                        this.references[srcIndex] = null;
                    }
                }
            }
            this.references.RemoveAll(a => a == null);
            this.references.Sort();
        }
        public void Remove(UIModel source, params Texture2D[] textures) {
            Remove(source, textures.Select(t => UnityEditor.AssetDatabase.GetAssetPath(t)).Where(t => t != null).Select(t => UnityEditor.AssetDatabase.GUIDFromAssetPath(t).ToString()));
        }
        public Material Material
        {
            get => material;
        }
        public unsafe Material UpdateMaterial() {
            if (IsTextureDirty(out int hash)) {
                if (this.sdfs != null) {
                    this.sdfs = null;
                }
                bool reAddMaterial = true;
                bool saveAssets = false;
                foreach (var subObj in UnityEditor.AssetDatabase.LoadAllAssetRepresentationsAtPath(UnityEditor.AssetDatabase.GetAssetPath(this))) {
                    if (subObj == null)
                        continue;
                    if (subObj is Material m) {
                        this.material = m;
                        reAddMaterial = false;
                        continue;
                    }

                    UnityEditor.AssetDatabase.RemoveObjectFromAsset(subObj);
                }
                if (this.material == null) {
                    this.material = null;
                }
                this.uvs = Array.Empty<UV>();
                var mat = this.material ?? new Material(UnityEditor.AssetDatabase.LoadAssetAtPath<Shader>(SHADER_ASSET));
                mat.enableInstancing = true;
                mat.name = "UI Material";
                var looseTextures = new List<Texture2D>();
                var atlases = new List<Texture2D>();
                this.references?.Sort();
                foreach (var reference in references) {
                    var path = UnityEditor.AssetDatabase.GUIDToAssetPath(reference.guid);
                    var refType = UnityEditor.AssetDatabase.GetMainAssetTypeAtPath(path);
                    var asset = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>(UnityEditor.AssetDatabase.GUIDToAssetPath(reference.guid));
                    if (asset == null)
                        continue;
                    if (refType == typeof(Texture2D)) {
                        looseTextures.Add(asset);
                    }
                    if (refType == typeof(TMP_FontAsset)) {
                        atlases.Add(asset);
                    }
                }

                if (looseTextures.Count > 0) {

                    var texture = new Texture2D(maxSize, maxSize);
                    texture.name = "Atlas";
                    var rects = texture.PackTextures(looseTextures.ToArray(), 0, maxSize);
                    var uvs = new UV[looseTextures.Count];
                    for (int i = 0; i < uvs.Length; i++) {
                        uvs[i] = new UV(UnityEditor.AssetDatabase.GUIDFromAssetPath(UnityEditor.AssetDatabase.GetAssetPath(looseTextures[i])).ToString(), rects[i]);
                    }
                    this.uvs = uvs;
                    this.atlas = texture;
                    mat.SetTexture("_Atlas", texture);
                    UnityEditor.AssetDatabase.AddObjectToAsset(texture, UnityEditor.AssetDatabase.GetAssetPath(this));
                    saveAssets = true;
                    this.hash = hash;
                }
                if (atlases.Count > 0) {

                    var arr = new Texture2DArray(maxSize, maxSize, atlases.Count, atlases[0].format, atlases[0].mipmapCount, true);
                    arr.name = "SDFs";
                    var size = new int2(atlases[0].width, atlases[0].height);
                    bool success = true;
                    for (int i = 0; i < atlases.Count; i++) {
                        if (atlases[i].width != size.x || atlases[i].height != size.y) {
                            Debug.LogError("SDF Atlases must be of the same size.");
                            success = false;
                            break;
                        }
                        Graphics.CopyTexture(atlases[i], 0, arr, i);
                    }
                    if (success) {
                        this.sdfs = arr;
                        mat.SetTexture("_SDFs", arr);
                        this.material = mat;
                        UnityEditor.AssetDatabase.AddObjectToAsset(arr, UnityEditor.AssetDatabase.GetAssetPath(this));
                        saveAssets = true;
                    }
                }
                if (reAddMaterial) {
                    UnityEditor.AssetDatabase.AddObjectToAsset(mat, UnityEditor.AssetDatabase.GetAssetPath(this));
                    saveAssets = true;
                    
                    this.material = mat;
                }
                if (saveAssets) {
                    UnityEditor.AssetDatabase.SaveAssets();
                }
                this.hash = hash;

                
                return mat;
            }
            else {
                return this.material;
            }


        }
#endif
        [Serializable]
        public class Reference : IComparable<Reference> {
            public string guid;
            public bool isAtlas;
            public List<string> referencedBy;
            public Reference(string guid) {
                this.guid = guid ?? string.Empty;
                this.referencedBy = new List<string>();
            }

            public int CompareTo(Reference other) {
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
            public UV scale(float2 scale) {
                return new UV(guid, new Rect(this.rect.x * scale.x, this.rect.y * scale.y, this.rect.width * scale.x, this.rect.height * scale.y));
            }

        }


    }
}