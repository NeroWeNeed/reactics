using Reactics.Core.Battle;
using Reactics.Core.Commons;
using Unity.Entities;
using Unity.Mathematics;
namespace Reactics.Core.Input {


    [GenerateAuthoringComponent]
    public struct InputData : IComponentData {
        public ActionMaps currentActionMap;
        public ActionMaps previousActionMap;

        //public ControlSchemes currentControlScheme;
        public float2 pan;
        public float2 tileMovement;
        public bool menuMovement;
        public float2 menuMovementDirection;
        public float zoom;
        public float2 rotation;
        public bool select;
        //public bool selectHeld;
        public bool cancel;
        //public bool cancelHeld;
        public int menuOption;
        //incredibly temporary
        public int CurrentMenuOption() => menuOption % 2;
    }
}