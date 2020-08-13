using System;

namespace Reactics.Core.Editor.Graph {
    public interface IVariableProvider : IObjectGraphModule {
        Type[] VariableTypes { get; }
    }

}