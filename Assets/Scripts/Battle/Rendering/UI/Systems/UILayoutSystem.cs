using System.Collections.Generic;
using Reactics.Commons;
using Unity.Entities;

namespace Reactics.UI
{

    public class UILayoutSystem : SystemBase
    {
        private Dictionary<BlittableGuid,IUILayout> layoutHandlers;

        protected override void OnCreate() {
            layoutHandlers = new Dictionary<BlittableGuid, IUILayout>();
        }
        protected override void OnUpdate()
        {
            throw new System.NotImplementedException();
        }
        private void LoadLayouts(Dictionary<BlittableGuid,IUILayout> layouts) {
            //Temporary, replace with asset loading

            
        }
        
    }
}