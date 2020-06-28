using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Reactics.Commons.Reflection
{
    public static class EmbeddedLocalizedReferenceExtensions
    {
        public static void ValidateEmbeddedLocalizedReferences(this UnityEngine.Object self, string identifier)
        {
            var subAssets = AssetDatabase.LoadAllAssetRepresentationsAtPath(AssetDatabase.GetAssetPath(self));

            foreach (var field in self.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance))
            {
                if (typeof(EmbeddedLocalizedAsset<>).IsAssignableFrom(field.FieldType) && SerializationUtility.IsSerializableField(field))
                {
                    var embeddedLocalizedReference = field.GetValue(self);


                }
            }
        }
        public static string GetLocalizationTableName(this FieldInfo fieldInfo) => fieldInfo.GetCustomAttribute<LocalizationTableNameAttribute>()?.name ?? fieldInfo.Name;

        private static async System.Threading.Tasks.Task EnsureValuesExistAsync<TObject>(EmbeddedLocalizedAsset<TObject> reference, string identifier, UnityEngine.Object rootAsset, UnityEngine.Object[] subAssets) where TObject : UnityEngine.Object
        {

            AssetTable assetTable = null;

            AsyncOperationHandle<AssetTable> tableHandle;
            foreach (var locale in LocalizationSettings.AvailableLocales.Locales)
            {
                if (!Array.Find(subAssets, (subAsset) => subAsset.name == $"{identifier}_{locale.name}"))
                {
                    if (assetTable == null)
                    {
                        tableHandle = LocalizationSettings.AssetDatabase.GetTableAsync(reference.TableReference);
                        await tableHandle.Task;
                        if (tableHandle.IsDone)
                        {
                            Debug.Log("Found " + tableHandle.Result);
                        }
                    }






                }

            }
        }
    }
}