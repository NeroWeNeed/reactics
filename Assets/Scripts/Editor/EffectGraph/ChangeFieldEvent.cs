using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.UIElements;

namespace Reactics.Editor
{
    public class ChangeFieldEvent : EventBase<ChangeFieldEvent>
    {
        public Type type { get; protected set; }

        public string name { get; protected set; }

        public object value { get; protected set; }

        public ChangeFieldEvent()
        {
            base.Init();
            Initialize();
        }
        private void Initialize()
        {
            var prop = typeof(ChangeFieldEvent).GetProperty("propagation",BindingFlags.Instance | 
                            BindingFlags.NonPublic |
                            BindingFlags.Public);
            prop?.SetValue(this, 3);
        }
        public static ChangeFieldEvent GetPooled(Type type, string name, object value)
        {
            ChangeFieldEvent e = GetPooled();
            e.type = type;
            e.name = name;
            e.value = value;
            e.Initialize();
            return e;
        }
    }
}