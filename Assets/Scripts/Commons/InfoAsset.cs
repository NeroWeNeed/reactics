using UnityEngine;
using UnityEngine.Localization.Tables;

namespace Reactics.Commons
{
    public abstract class InfoAsset : ScriptableObject
    {
        [SerializeField]
        public abstract TableReference Table { get; }

        [SerializeField]
        public string Identifier { get; }


private void OnValidate() {
    
}




    }
}