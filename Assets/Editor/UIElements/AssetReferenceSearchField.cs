using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.GUI;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.U2D;
using UnityEngine.UIElements;

namespace Reactics.Core.Editor {

    public class AssetReferenceSearchField : SearchField<AssetReference> {
        internal const string IReferenceQualfiedName = "UnityEditor.AddressableAssets.Settings.IReferenceEntryData, Unity.Addressables.Editor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null";
        internal const string noAssetString = "None (AddressableAsset)";
        public override string NoValueDisplayText => noAssetString;
        private HashSet<SearchResult> resultCandidates = null;
        internal List<AssetReferenceUIRestrictionSurrogate> Restrictions { get; private set; } = null;

        public AssetReferenceSearchField() : base() {
            Init();
        }

        public AssetReferenceSearchField(string label) : base(label) {
            Init();
        }
        public AssetReferenceSearchField(SerializedProperty property) : base(property.displayName) {
            Init();
            var field = property?.serializedObject?.targetObject?.GetType()?.GetField(property.name);
            if (field != null)
                SetFiltersFromField(field);
            SetValueWithoutNotify(new AssetReference(property.FindPropertyRelative("m_AssetGUID")?.stringValue)
            {
                SubObjectName = property.FindPropertyRelative("m_SubObjectName")?.stringValue
            });
            this.BindProperty(property);

        }

        public bool ValidateAsset(string path) {
            if (Restrictions == null)
                return true;
            foreach (var restriction in Restrictions) {
                if (!restriction.ValidateAsset(path))
                    return false;
            }
            return true;
        }
        private void Init() {
            BuildEntries();
            RegisterCallback<DragUpdatedEvent>(OnDragUpdated);
            RegisterCallback<DragPerformEvent>(OnDragPerform);
        }

        private void OnDragUpdated(DragUpdatedEvent evt) {
            if (DragAndDrop.objectReferences != null && DragAndDrop.objectReferences.Length > 0) {
                DragAndDrop.visualMode = DragAndDropVisualMode.Link;
            }
        }
        private void OnDragPerform(DragPerformEvent evt) {
            if (DragAndDrop.objectReferences != null && DragAndDrop.objectReferences.Length > 0) {
                var aaSettings = AddressableAssetSettingsDefaultObject.GetSettings(true);

                var obj = DragAndDrop.objectReferences[0];
                AssetDatabase.TryGetGUIDAndLocalFileIdentifier(obj, out string guid, out long _);
                var entry = aaSettings.FindAssetEntry(guid);
                if (entry != null) {
                    value = new AssetReference(entry.guid)
                    {
                        SubObjectName = AssetDatabase.IsSubAsset(obj) ? obj.name : null
                    };

                }
            }
        }
        private void BuildEntries() {
            var aaSettings = AddressableAssetSettingsDefaultObject.GetSettings(true);

            if (aaSettings == null) {
                var message = "Use 'Window->Addressables' to initialize.";
                Debug.LogError(message);
                return;
            }
            else {
                var allAssets = Activator.CreateInstance(typeof(List<>).MakeGenericType(Type.GetType(IReferenceQualfiedName)));

                if (resultCandidates == null)
                    resultCandidates = new HashSet<SearchResult>();
                else
                    resultCandidates.Clear();
                aaSettings.GetType().GetMethod("GatherAllAssetReferenceDrawableEntries", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public).Invoke(aaSettings, new object[] { allAssets });
                string assetPath;
                string address;


                foreach (var entry in allAssets as IEnumerable) {
                    if (!(bool)entry.GetType().GetProperty("IsInResources").GetValue(entry)) {
                        assetPath = (string)entry.GetType().GetProperty("AssetPath").GetValue(entry);
                        if (ValidateAsset(assetPath)) {
                            address = address = (string)entry.GetType().GetProperty("address").GetValue(entry);

                            foreach (var asset in AssetDatabase.LoadAllAssetsAtPath(assetPath)) {
                                if (AssetDatabase.IsMainAsset(asset)) {
                                    resultCandidates.Add(new SearchResult(address, assetPath, null));
                                }
                                else {
                                    resultCandidates.Add(new SearchResult(address, assetPath, asset.name));
                                }

                            }


                        }

                    }
                }
            }
        }
        public void SetFiltersFromField(FieldInfo field) {
            if (Restrictions == null)
                Restrictions = new List<AssetReferenceUIRestrictionSurrogate>();
            else
                Restrictions.Clear();

            if (field != null) {
                var a = new List<object>();
                a.AddRange(field.GetCustomAttributes(false));
                if (field.FieldType.IsGenericType && typeof(AssetReferenceT<>).IsAssignableFrom(field.FieldType.GetGenericTypeDefinition())) {
                    a.Add(new AssetReferenceTypeRestriction(field.FieldType.GetGenericTypeDefinition().GenericTypeArguments[0]));
                }
                foreach (var attr in a) {
                    var uiRestriction = attr as AssetReferenceUIRestriction;
                    if (uiRestriction != null) {
                        var surrogate = AssetReferenceUtility.GetSurrogate(uiRestriction.GetType());

                        if (surrogate != null) {
                            var surrogateInstance = Activator.CreateInstance(surrogate) as AssetReferenceUIRestrictionSurrogate;
                            if (surrogateInstance != null) {
                                surrogateInstance.Init(uiRestriction);
                                Restrictions.Add(surrogateInstance);
                            }
                        }
                        else {
                            AssetReferenceUIRestrictionSurrogate restriction = new AssetReferenceUIRestrictionSurrogate();
                            restriction.Init(uiRestriction);
                            Restrictions.Add(restriction);
                        }
                    }
                }

                BuildEntries();
            }

        }


        public override IEnumerable<ISearchResult<AssetReference>> OnSearch(string newQuery, string oldQuery, int max = -1) {

            if (string.IsNullOrEmpty(newQuery) || resultCandidates == null) {
                yield break;
            }
            else {
                int sum = 0;
                foreach (var result in resultCandidates) {
                    if (max >= 0 && sum >= max)
                        yield break;
                    if (result.address.IndexOf(newQuery, StringComparison.OrdinalIgnoreCase) >= 0) {
                        yield return result;
                    }


                }
            }

        }


        protected override ISearchResult<AssetReference> CreateFromObject(AssetReference obj) {
            if (obj?.RuntimeKeyIsValid() != true)
                return null;
            var aaSettings = AddressableAssetSettingsDefaultObject.Settings;
            var entry = aaSettings.FindAssetEntry(obj.AssetGUID);

            return new SearchResult(entry.address, entry.AssetPath, obj.SubObjectName);
        }
        public struct SearchResult : ISearchResult<AssetReference>, IEquatable<SearchResult> {

            public string DisplayText { get; }
            public string address;
            public string path;
            public string subObjectName;

            public SearchResult(string address, string path, string subObjectName) {
                this.address = address;
                this.path = path;

                if (string.IsNullOrEmpty(subObjectName)) {
                    this.subObjectName = null;
                    this.DisplayText = address;
                }
                else {
                    this.subObjectName = subObjectName;
                    this.DisplayText = address + " [" + this.subObjectName + "]";
                }
            }

            public SearchResultElement<AssetReference> CreateElement() {
                return new AssetLabel(DisplayText, AssetDatabase.GetCachedIcon(path))
                {
                    Value = this
                };
            }

            public AssetReference LoadValue() {

                return new AssetReference(AssetDatabase.AssetPathToGUID(path))
                {
                    SubObjectName = subObjectName
                };
            }

            public override bool Equals(object obj) {
                return obj is SearchResult result &&
                       address == result.address &&
                       path == result.path &&
                       subObjectName == result.subObjectName;
            }

            public override int GetHashCode() {
                int hashCode = -986157352;
                hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(address);
                hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(path);
                hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(subObjectName);
                return hashCode;
            }

            public bool Equals(SearchResult other) {
                return address == other.address && path == other.path && subObjectName == other.subObjectName;
            }
        }
        public class AssetLabel : SearchResultElement<AssetReference> {
            internal const string USS_GUID = "0eb53066c93d9954ab6cf4881af89014";
            private Image icon;
            private Label text;
            public override ISearchResult<AssetReference> Value { get; set; }
            public AssetLabel(string text, Texture icon) : base() {
                this.icon = new Image
                {
                    image = icon,
                    scaleMode = ScaleMode.ScaleToFit
                };
                this.text = new Label(text);
                this.icon.AddToClassList("asset-reference-label-icon");
                this.text.AddToClassList("asset-reference-label-text");
                this.AddToClassList("asset-reference-label");
                styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>(AssetDatabase.GUIDToAssetPath(USS_GUID)));
                this.icon.pickingMode = PickingMode.Ignore;
                this.text.pickingMode = PickingMode.Ignore;
                this.Add(this.icon);
                this.Add(this.text);
                this.tooltip = text;
            }


        }

    }
    public class AssetReferenceTypeRestriction : AssetReferenceUIRestriction {

        public Type type;

        public AssetReferenceTypeRestriction(Type type) : base() {
            this.type = type;
        }

        public virtual bool ValidateAsset(UnityEngine.Object obj) {
            return type.IsAssignableFrom(obj.GetType());
        }

        public virtual bool ValidateAsset(string path) {
            Debug.Log(path);
            return true;
        }
    }
    /*     [CustomPropertyDrawer(typeof(AssetReferenceT<>))]
        public class AssetReferenceSearchFieldDrawer : PropertyDrawer {
            public override VisualElement CreatePropertyGUI(SerializedProperty property) {
                var field = new AssetReferenceSearchField(property.name);
                var guid = property?.FindPropertyRelative("m_AssetGUID")?.stringValue;
                var subObject = property?.FindPropertyRelative("m_SubObjectName")?.stringValue;
                var reference = new AssetReference(guid)
                {
                    SubObjectName = subObject
                };
                field.SetValueWithoutNotify(reference);
                field.SetFiltersFromField(property?.serializedObject?.targetObject?.GetType()?.GetField(property.name));

                return field;
            }
        } */
}