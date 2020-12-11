using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using NeroWeNeed.BehaviourGraph.Editor;
using NeroWeNeed.Commons;
using NeroWeNeed.Commons.Editor;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditorInternal;
using UnityEngine;
[assembly: SearchableAssembly]



namespace NeroWeNeed.BehaviourGraph.Editor {
    [CreateAssetMenu(fileName = "BehaviourGraphSettings", menuName = "Behaviours/Settings", order = 0)]
    public class BehaviourGraphGlobalSettings : ScriptableObject {
        public const string SETTINGS_PATH = "Assets/BehaviourGraphData/BehaviourGraphGlobalSettings.asset";
        public const string DEFAULT_OUTPUT = "Assets/Resources/Behaviour";
        public const string SETTINGS_FOLDER = "BehaviourGraphData";
        public const string DATA_FOLDER = "Data";
        [SuperTypeFilter(typeof(Delegate))]
        [ConcreteTypeFilter]
        public List<SerializableType> behaviours;
        [HideInInspector]
        [SerializeField]
        private List<Entry> entries;
        private ReadOnlyDictionary<Type, Entry> entryView;
        public ReadOnlyDictionary<Type, Entry> Entries => entryView ??= new ReadOnlyDictionary<Type, Entry>(entries.ToDictionary(t => t.type.Value));
        public string baseOutputDirectory = DEFAULT_OUTPUT;

        public static BehaviourGraphGlobalSettings Settings
        {
            get
            {
                var settings = AssetDatabase.LoadAssetAtPath<BehaviourGraphGlobalSettings>(SETTINGS_PATH);
                if (settings == null) {
                    settings = ScriptableObject.CreateInstance<BehaviourGraphGlobalSettings>();
                    AssetDatabase.CreateFolder("Assets", SETTINGS_FOLDER);
                    AssetDatabase.CreateAsset(settings, SETTINGS_PATH);
                    AssetDatabase.SaveAssets();
                }
                return settings;
            }
        }
        internal static SerializedObject SerializedSettings { get => new SerializedObject(Settings); }

        public void RefreshEntryView() {
            this.entryView = new ReadOnlyDictionary<Type, Entry>(entries.ToDictionary(t => t.type.Value));
        }
        public static BehaviourGraphSettings GetBehaviourGraphSettings(Type type) {
            if (type == null)
                return null;
            return Settings.Entries[type].asset;
        }
        internal static BehaviourGraphSettings GetBehaviourGraphSettings<T>() => GetBehaviourGraphSettings(typeof(T));

        [Serializable]
        public struct Entry {
            public SerializableType type;
            public BehaviourGraphSettings asset;
        }
    }


}