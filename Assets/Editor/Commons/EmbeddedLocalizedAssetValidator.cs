using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Reactics.Core.Commons;
using Reactics.Core.Commons.Reflection;
using UnityEditor;
//using UnityEditor.Localization;
using UnityEngine;
/* using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables; */

namespace Reactics.Editor {/* 
    public static class EmbeddedLocalizedAssetValidator {
        public const string LOCALE_ASSET_PATH = "Assets/ResourceData/Locale/Assets";
        public const string LOCALE_PATH = "Assets/ResourceData/Locale";

        public static bool STRICT_ASSET_LOCATION = true;

        [InitializeOnLoadMethod]
        public static void EnsureFoldersExist() {
            if (!AssetDatabase.IsValidFolder(LOCALE_ASSET_PATH)) {
                AssetDatabase.CreateFolder(LOCALE_ASSET_PATH.Substring(0, LOCALE_ASSET_PATH.LastIndexOf("/")), LOCALE_ASSET_PATH.Substring(LOCALE_ASSET_PATH.LastIndexOf("/") + 1));

            }
        }
        public static async void Validate(SerializedProperty property) {
            if (AssetDatabase.TryGetGUIDAndLocalFileIdentifier(property.serializedObject.targetObject, out string guid, out long localId)) {
                var field = property.GetField();
                var assetType = field.FieldType.GenericTypeArguments[0];
                var tableName = field.GetLocalizationTableName();


                foreach (var table in GetOrCreateTableCollection(property, tableName).Tables.Select((table) => table.asset as AssetTable)) {
                    var key = table.SharedData.GetId(guid, true);

                    if (table.ContainsKey(key)) {
                        if (STRICT_ASSET_LOCATION) {
                            var entry = table.GetEntry(key);
                            if (AssetDatabase.GUIDToAssetPath(entry.LocalizedValue) != GetAssetPath(guid, tableName, table.LocaleIdentifier.Code)) {
                                UpdateEmbeddedAsset(property, table, guid, key, table.LocaleIdentifier.Code);
                            }
                        }
                    }
                    else {
                        UpdateEmbeddedAsset(property, table, guid, key, table.LocaleIdentifier.Code);
                    }
                }


            }
        }
        public static AssetTableCollection GetOrCreateTableCollection(SerializedProperty property, string tableName) {
            var tableCollection = LocalizationEditorSettings.GetAssetTableCollection(tableName);
            if (tableCollection != null) {

                if (property.FindPropertyRelative("m_TableReference.m_TableCollectionName").stringValue != tableName)
                    property.FindPropertyRelative("m_TableReference.m_TableCollectionName").stringValue = tableName;
                return tableCollection;
            }

            var result = LocalizationEditorSettings.CreateAssetTableCollection(tableName, "Assets/ResourceData/Locale");
            if (!AssetDatabase.IsValidFolder($"{LOCALE_ASSET_PATH}/{tableName}")) {
                AssetDatabase.CreateFolder(LOCALE_ASSET_PATH, tableName);
            }
            property.FindPropertyRelative("m_TableReference.m_TableCollectionName").stringValue = tableName;
            return result;

        }
        private static string GetAssetPath(string sourceGuid, string tableName, string localeCode) => $"{LOCALE_ASSET_PATH}/{tableName}/{sourceGuid}_{localeCode}.asset";
        private static string GetAssetName(string sourceGuid, string localeCode) => $"{sourceGuid}_{localeCode}.asset";
        private static void UpdateEmbeddedAsset(SerializedProperty property, AssetTable table, string sourceGuid, long key, string localeCode) {
            var type = property.GetFieldType().GenericTypeArguments[0];
            var name = GetAssetName(sourceGuid, localeCode);
            var path = GetAssetPath(sourceGuid, table.TableCollectionName, localeCode);
            UnityEngine.Object localeAsset = AssetDatabase.LoadAssetAtPath(path, type);
            if (localeAsset == null) {
                localeAsset = UnityEngine.ScriptableObject.CreateInstance(type);
                localeAsset.name = name;
                AssetDatabase.CreateAsset(localeAsset, path);
            }
            else {
                AssetDatabase.TryGetGUIDAndLocalFileIdentifier(localeAsset.GetInstanceID(), out string assetGuid, out long _);
                table.AddEntry(key.ToString(), assetGuid);
                if (property.FindPropertyRelative("m_TableEntryReference.m_KeyId").longValue != key) {
                    property.FindPropertyRelative("m_TableEntryReference.m_KeyId").longValue = key;
                    AssetDatabase.Refresh();
                }
            }








        }

        public static void ValidateAll(SerializedObject target) {


            foreach (var field in target.GetType().GetFields()) {

                if (typeof(EmbeddedLocalizedAsset<>).IsAssignableFrom(field.FieldType) && field.IsSerializableField()) {

                }
            }
        }
    } */
}