using System.Reflection;
using UnityEngine;
using UnityEngine.UIElements;

namespace Reactics.Core.Editor {
    public class ChangeEvent : EventBase<ChangeEvent> {
        public object newValue;

        public object previousValue;
        public ChangeEvent() {
            base.Init();
            Initialize();
        }
        private void Initialize() {
            //For some reason propagation is internal?
            var prop = typeof(EventBase).GetProperty("propagation", BindingFlags.Instance |
                            BindingFlags.NonPublic |
                            BindingFlags.Public);
            prop?.SetValue(this, 3);
        }
        public static ChangeEvent GetPooled(object previousValue, object newValue) {
            ChangeEvent e = GetPooled();
            e.newValue = newValue;
            e.previousValue = previousValue;
            e.Initialize();
            return e;
        }
    }
}