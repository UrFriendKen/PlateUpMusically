using Kitchen;
using KitchenMods;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;

namespace KitchenMusically
{
    public class VolumeManager : GameSystemBase, IModSystem
    {
        EntityQuery MusicalInstruments;

        protected readonly Dictionary<InstrumentType, string> volumePreferenceMap = new Dictionary<InstrumentType, string>()
        {
            { InstrumentType.Piano, "pianoVolume"}
        };

        protected override void Initialise()
        {
            MusicalInstruments = GetEntityQuery(typeof(CMusicalInstrument));
        }

        protected override void OnUpdate()
        {
            using NativeArray<Entity> entities = MusicalInstruments.ToEntityArray(Allocator.Temp);
            using NativeArray<CMusicalInstrument> instruments = MusicalInstruments.ToComponentDataArray<CMusicalInstrument>(Allocator.Temp);

            for (int i = 0; i < entities.Length; i++)
            {
                Entity entity = entities[i];
                CMusicalInstrument instrument = instruments[i];

                if (TryUpdateVolume(ref instrument))
                    Set(entity, instrument);
            }
        }

        private bool TryUpdateVolume(ref CMusicalInstrument instrument)
        {
            if (!volumePreferenceMap.TryGetValue(instrument.Type, out string prefKey))
                return false;
            instrument.VolumeMultiplier = Main.PrefManager.Get<float>(prefKey);
            return true;
        }
    }
}
