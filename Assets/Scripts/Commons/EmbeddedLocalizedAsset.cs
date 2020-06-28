using System;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Tables;

namespace Reactics.Commons
{
    [Serializable]
    public class EmbeddedLocalizedAsset<TObject> : LocalizedAsset<TObject> where TObject : UnityEngine.Object { }


    public sealed class EmbeddedLocalizedAssetIdentifier : Attribute
    {

    }
}