using UnityEngine;
using UnityEditor;
//using Reactics.Battle.Unit;
using System.Collections.Generic;
using System;

namespace Reactics.Editor
{



    public abstract class BaseEffectEditor<T> : UnityEditor.Editor
    {
        protected List<Type> effectTypes;
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            
        }
        private void OnEnable()
        {
            if (effectTypes == null)
            {

                effectTypes = new List<Type>();
                CollectEffectTypes(effectTypes);
            }

        }
        protected void CollectEffectTypes(List<Type> types)
        {
            types.Clear();
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var type in assembly.GetTypes())
                {
                    if (type.IsUnmanaged())
                        types.Add(type);
                }

            }
        }
    }
}