using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Reactics.Core.Commons;
using Reactics.Core.Commons.Reflection;
using UnityEditor;
using UnityEditor.Localization;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Tables;
using UnityEngine.UIElements;

namespace Reactics.Core.Editor {


    [CustomPropertyDrawer(typeof(EmbeddedLocalizedAsset<>))]
    public class EmbeddedLocalizedAssetPropertyDrawer : PropertyDrawer {
        int localeIndex;
        private float verticalPadding = EditorGUIUtility.standardVerticalSpacing * 8;
        private float verticalMargin = EditorGUIUtility.standardVerticalSpacing * 4;
        private Color backgroundColor = GeneralCommons.ParseColor("#A5A5A5");
        private Dictionary<string, SerializedObject> references = new Dictionary<string, SerializedObject>();
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            EditorGUI.BeginProperty(position, label, property);
            float yOffset = verticalPadding;
            var curLabelText = label.text;
            EditorGUI.DrawRect(new Rect(0, position.y + yOffset, EditorGUIUtility.currentViewWidth, GetPropertyHeight(property, null) - verticalPadding * 2), backgroundColor);
            yOffset += verticalMargin;
            EmbeddedLocalizedAssetValidator.Validate(property);
            var tableCollection = LocalizationEditorSettings.GetAssetTableCollection(property.GetField().GetLocalizationTableName());
            var locales = LocalizationEditorSettings.GetLocales();
            EditorGUI.LabelField(new Rect(position.x, position.y + yOffset, position.width, EditorGUIUtility.singleLineHeight), curLabelText, EditorStyles.boldLabel);
            yOffset += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            if (AssetDatabase.TryGetGUIDAndLocalFileIdentifier(property.serializedObject.targetObject, out string guid, out long _)) {

                EditorGUI.BeginDisabledGroup(locales.Count <= 1);
                localeIndex = EditorGUI.Popup(new Rect(position.x, position.y + yOffset, position.width, EditorGUIUtility.singleLineHeight), "Locale", localeIndex, locales.Select((l) => l.name).ToArray());
                yOffset += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                EditorGUI.EndDisabledGroup();
                AssetTable table = tableCollection.GetTable(locales[localeIndex].Identifier) as AssetTable;
                var localizedTargetGuid = table.GetEntry(guid).LocalizedValue;

                if (!references.TryGetValue(localizedTargetGuid, out SerializedObject target)) {
                    target = new SerializedObject(AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(localizedTargetGuid), property.GetFieldType().GenericTypeArguments[0]));
                    references[localizedTargetGuid] = target;
                }

                foreach (var field in property.GetFieldType().GenericTypeArguments[0].GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)) {
                    if (field.IsSerializableField()) {
                        var prop = target.FindProperty(field.Name);
                        var propHeight = EditorGUI.GetPropertyHeight(prop, true);
                        EditorGUI.PropertyField(new Rect(position.x, position.y + yOffset, position.width, propHeight), prop);
                        yOffset += propHeight + EditorGUIUtility.standardVerticalSpacing;
                    }
                }
                if (target.hasModifiedProperties)
                    target.ApplyModifiedProperties();
            }

            EditorGUI.EndFoldoutHeaderGroup();
            EditorGUI.EndProperty();

        }
        public override VisualElement CreatePropertyGUI(SerializedProperty property) {

            var container = new VisualElement();
            if (AssetDatabase.TryGetGUIDAndLocalFileIdentifier(property.serializedObject.targetObject, out string guid, out long _)) {
                var header = new VisualElement();
                var fields = new BindableElement();
                container.Add(header);
                container.Add(fields);
                EmbeddedLocalizedAssetValidator.Validate(property);
                var tableCollection = LocalizationEditorSettings.GetAssetTableCollection(property.GetField().GetLocalizationTableName());
                var locales = LocalizationEditorSettings.GetLocales().ToList();
                var localeIndex = new PopupField<Locale>("Locale", locales, locales.FirstOrDefault(), (l) => l?.name, l => l?.name);
                header.Add(localeIndex);
                var status = locales.Count > 1;
                header.SetEnabled(status);
                header.style.display = status ? DisplayStyle.Flex : DisplayStyle.None;
                AssetTable table = tableCollection.GetTable(localeIndex.value.Identifier) as AssetTable;
                var localizedTargetGuid = table.GetEntry(guid).LocalizedValue;
                if (!references.TryGetValue(localizedTargetGuid, out SerializedObject target)) {
                    target = new SerializedObject(AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(localizedTargetGuid), property.GetFieldType().GenericTypeArguments[0]));
                    references[localizedTargetGuid] = target;
                }
                foreach (var field in property.GetFieldType().GenericTypeArguments[0].GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).Where((f) => f.IsSerializableField())) {
                    var prop = target.FindProperty(field.Name);
                    if (prop != null)
                        fields.Add(new PropertyField(prop));
                }
                fields.Bind(target);
            }
            return container;
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {


            if (!AssetDatabase.TryGetGUIDAndLocalFileIdentifier(property.serializedObject.targetObject, out string guid, out long _)) {
                return EditorGUIUtility.singleLineHeight + (verticalPadding * 2) + (verticalMargin * 2);
            }
            else {
                float height = (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing) * 2;
                EmbeddedLocalizedAssetValidator.Validate(property);
                var tableCollection = LocalizationEditorSettings.GetAssetTableCollection(property.GetField().GetLocalizationTableName());
                var locales = LocalizationEditorSettings.GetLocales();
                AssetTable table = tableCollection.GetTable(locales[localeIndex].Identifier) as AssetTable;

                var entry = table.GetEntry(guid);
                if (entry != null) {
                    var localizedTargetGuid = entry.LocalizedValue;
                    if (references.TryGetValue(localizedTargetGuid, out SerializedObject target)) {
                        foreach (var field in property.GetFieldType().GenericTypeArguments[0].GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)) {
                            if (field.IsSerializableField()) {
                                height += EditorGUI.GetPropertyHeight(target.FindProperty(field.Name));
                            }
                        }
                    }
                }
                return height + EditorGUIUtility.standardVerticalSpacing + (verticalPadding * 2) + (verticalMargin * 2);
            }

        }

    }
}