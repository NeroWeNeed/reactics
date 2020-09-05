using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;

namespace Reactics.Editor.Graph {
    public interface IVariableProvider : IObjectGraphModule {
        IObjectGraphVariableProvider[] VariableTypes { get; }
    }


}