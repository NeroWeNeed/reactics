using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Sprites;
using UnityEditor.U2D;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UIElements;

namespace NeroWeNeed.UIDots.Editor {

    [CreateAssetMenu(fileName = "UISpriteGraphTable", menuName = "DOTS/UI/Sprite Table", order = 0)]
    public class UISpriteTable : ScriptableObject {

        [SerializeField]
        private List<string> sprites;
        [SerializeField]
        private List<string> models;
        [SerializeField]
        private string output;

        [SerializeField]
        private Texture2D atlas;
        public Texture2D Atlas { get => atlas; }
        public void AddModel(string modelReference) {
            if (models == null) {
                models = new List<string>();
            }
            if (!models.Contains(modelReference)) {
                models.Add(modelReference);
                EditorUtility.SetDirty(this);
            }

        }

        public void AddSprite(string spriteReference) {
            if (sprites == null) {
                sprites = new List<string>();
            }
            var path = spriteReference.StartsWith("guid:") ? AssetDatabase.GUIDToAssetPath(spriteReference.Substring(5)) : spriteReference;
            var assetType = AssetDatabase.GetMainAssetTypeAtPath(path);
            if ((assetType == typeof(UIModel) || assetType == typeof(Texture2D)) && !sprites.Contains(spriteReference)) {
                sprites.Add(spriteReference);
                EditorUtility.SetDirty(this);
            }
        }
        private void OnValidate() {
            if (string.IsNullOrEmpty(output) && !string.IsNullOrEmpty(name)) {
                output = $"Assets/Resources/UI/{name}";
            }
        }
        public void RefreshSprites() {
            var sprites = new List<Sprite>();
            var paths = new List<string>();
            /*             foreach (var model in models) {
                            foreach (var spriteLocation in UIGraphCompiler.GetAssets(AssetDatabase.GUIDToAssetPath(model))) {
                                var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spriteLocation.StartsWith("guid:") ? AssetDatabase.GUIDToAssetPath(spriteLocation.Substring(5)) : spriteLocation);
                                if (sprite != null) {
                                    sprites.Add(sprite);
                                    paths.Add(spriteLocation);
                                }
                            }
                        } */

            this.sprites.Clear();
            this.sprites.AddRange(paths);
        }
        public SpriteAtlas BuildSpriteAtlas() {

            if (string.IsNullOrEmpty(output))
                throw new Exception("Output File not specified.");
            var spriteAtlas = new SpriteAtlas();
            spriteAtlas.Add(sprites.Select(spriteReference => Array.Find(AssetDatabase.LoadAllAssetsAtPath(spriteReference.StartsWith("guid:") ? AssetDatabase.GUIDToAssetPath(spriteReference.Substring(5)) : spriteReference), s => s.GetType() == typeof(Sprite))).Where(sprite => sprite != null && (sprite is Sprite)).ToArray());
            foreach (var item in spriteAtlas.GetPackables()) {
                EditorUtility.SetDirty(item);
            }
            AssetDatabase.CreateAsset(spriteAtlas, $"{output}.spriteatlas");

            AssetDatabase.SaveAssets();
            SpriteAtlasUtility.PackAllAtlases(EditorUserBuildSettings.activeBuildTarget);
            var texture = SpriteUtility.GetSpriteTexture(spriteAtlas.GetPackables()[0] as Sprite, true);
            var t = RenderTexture.GetTemporary(texture.width, texture.height, 0, RenderTextureFormat.Default, RenderTextureReadWrite.Linear);
            Graphics.Blit(texture, t);
            var atlased = new Texture2D(texture.width, texture.height);
            var p = RenderTexture.active;
            RenderTexture.active = t;
            atlased.ReadPixels(new Rect(0, 0, t.width, t.height), 0, 0, false);
            atlased.Apply();
            RenderTexture.active = p;
            RenderTexture.ReleaseTemporary(t);
            if (this.atlas != null) {
                AssetDatabase.RemoveObjectFromAsset(this.atlas);
            }
            AssetDatabase.AddObjectToAsset(atlased, AssetDatabase.GetAssetPath(this));
            atlased.name = "Atlas";
            this.atlas = atlased;
            return spriteAtlas;
        }
    }
}