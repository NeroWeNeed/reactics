using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Reactics.Core.Editor.Graph {

    public interface IMasterNodeConfigurator : IObjectGraphModule {
        void ConfigureMaster(Node master);
    }
}