using UnityEditor.Experimental.GraphView;
using Reactics.Battle.Unit;
using System;
using UnityEngine.UIElements;
using UnityEngine;
using Reactics.Battle;

namespace Reactics.Editor
{
    public class EffectMasterNode : Node
    {

        public Port EffectPort { get; private set; }
        public Port IdentifierPort { get; private set; }
        public EffectMasterNode()
        {
            title = "Effect Master";
            EffectPort = this.InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(IEffect));
            EffectPort.portName = "Effect";
            inputContainer.Add(EffectPort);
            IdentifierPort = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, typeof(string));
            IdentifierPort.portName = "Identifier";
            inputContainer.Add(IdentifierPort);

            capabilities ^= Capabilities.Deletable;
            //capabilities = Capabilities.Ascendable | Capabilities.Collapsible | Capabilities.Droppable | Capabilities.Movable | Capabilities.Resizable | Capabilities.Selectable;
            RefreshPorts();
        }

    }
}