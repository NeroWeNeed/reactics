using Unity.Entities;
using Unity.Jobs;

namespace Reactics.Core.Battle {

    /*     public class ActionMeterSystem : ComponentSystem
        {

            private EntityQuery query;
            protected override void OnCreate()
            {
                query = GetEntityQuery(typeof(ActionMeter));
            }

            protected override void OnUpdate()
            {
                Entities.With(query).ForEach((ref ActionMeter actionMeter) =>
                {
                    if (actionMeter.chargeable && !actionMeter.Active())
                    {
                        var next = actionMeter.charge + actionMeter.rechargeRate * Time.DeltaTime;
                        if (next > ActionMeter.MAX_ACTION_POINTS)
                            actionMeter.charge = ActionMeter.MAX_ACTION_POINTS;
                        else
                            actionMeter.charge = next;
                    }
                });
            }

        } */
}