using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.UnityConverters.Helpers;
using UnityEditor;
using UnityEngine;

namespace Reactics.Editor {
    public static class UnityConverterInitializer {

        [InitializeOnLoadMethod]
        public static void InitializeUnityConvertersForEditor() {
            var type = Type.GetType("Newtonsoft.Json.UnityConverters.UnityConverterInitializer, Newtonsoft.Json.UnityConverters, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
            type.GetMethod("Init", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, null);


        }
    }


}