using Kitchen;
using KitchenData;
using System.Collections.Generic;
using UnityEngine;

namespace KitchenMusically
{
    public class SpawnInstrumentManager : GameSystemBase
    {
        static Queue<int> requestedAppliances = new Queue<int>();

        protected override void OnUpdate()
        {
            EntityContext ctx = new EntityContext(EntityManager);
            if (requestedAppliances.Count > 0)
            {
                int id = requestedAppliances.Dequeue();
                if (base.Data.TryGet<Appliance>(id, out Appliance appliance))
                {
                    SpawnApplianceBlueprint(ctx, appliance.ID, GetFrontDoor(get_external_tile: true));
                }
            }
        }

        protected void SpawnApplianceBlueprint(EntityContext ctx, int applianceId, Vector3 position)
        {
            PostHelpers.CreateOpenedLetter(ctx, position, applianceId, 0, 0);
        }

        public static void RequestAppliance(int applianceId)
        {
            requestedAppliances.Enqueue(applianceId);
        }
    }
}
