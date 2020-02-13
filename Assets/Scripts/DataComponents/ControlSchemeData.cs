using Unity.Collections;
using Unity.Entities;
using Reactics.Battle;

[GenerateAuthoringComponent]
public struct ControlSchemeData : IComponentData
{
    public ControlSchemes currentControlScheme;
}