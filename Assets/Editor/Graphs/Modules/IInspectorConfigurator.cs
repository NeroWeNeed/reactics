using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Reactics.Editor.Graph {
    public interface IInspectorConfigurator : IObjectGraphModule {
        VisualElement CreateInspectorSection(ObjectGraphView graphView);
    }
}