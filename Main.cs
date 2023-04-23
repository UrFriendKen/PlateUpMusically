using KitchenData;
using KitchenLib;
using KitchenLib.Event;
using KitchenLib.References;
using KitchenMods;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

// Namespace should have "Kitchen" in the beginning
namespace KitchenMusically
{
    public class Main : BaseMod, IModSystem
    {
        public static readonly Dictionary<InstrumentType, List<string>> INSTRUMENT_CLIP_NAMES = new Dictionary<InstrumentType, List<string>>()
        {
            { InstrumentType.Piano,
                new List<string>()
                {
                    "Piano_Chord_C.wav",
                    "Piano_Chord_F.wav",
                    "Piano_Chord_G.wav",
                    "Piano_Chord_Am.wav"
                }
            }
        };

        // GUID must be unique and is recommended to be in reverse domain name notation
        // Mod Name is displayed to the player and listed in the mods menu
        // Mod Version must follow semver notation e.g. "1.2.3"
        public const string MOD_GUID = "IcedMilo.PlateUp.Musically";
        public const string MOD_NAME = "Musically";
        public const string MOD_VERSION = "0.1.0";
        public const string MOD_AUTHOR = "IcedMilo";
        public const string MOD_GAMEVERSION = ">=1.1.5";
        // Game version this mod is designed for in semver
        // e.g. ">=1.1.3" current and all future
        // e.g. ">=1.1.3 <=1.2.3" for all from/until

        // Boolean constant whose value depends on whether you built with DEBUG or RELEASE mode, useful for testing
#if DEBUG
        public const bool DEBUG_MODE = true;
#else
        public const bool DEBUG_MODE = false;
#endif

        public static AssetBundle Bundle;

        public Main() : base(MOD_GUID, MOD_NAME, MOD_AUTHOR, MOD_VERSION, MOD_GAMEVERSION, Assembly.GetExecutingAssembly()) { }

        protected override void OnInitialise()
        {
            LogWarning($"{MOD_GUID} v{MOD_VERSION} in use!");
        }

        private void AddGameData()
        {
            LogInfo("Attempting to register game data...");

            // AddGameDataObject<MyCustomGDO>();

            LogInfo("Done loading game data.");
        }

        protected override void OnUpdate()
        {
        }

        protected override void OnPostActivate(KitchenMods.Mod mod)
        {
            // TODO: Uncomment the following if you have an asset bundle.
            // TODO: Also, make sure to set EnableAssetBundleDeploy to 'true' in your ModName.csproj

            // LogInfo("Attempting to load asset bundle...");
            Bundle = mod.GetPacks<AssetBundleModPack>().SelectMany(e => e.AssetBundles).First();
            // LogInfo("Done loading asset bundle.");

            // Register custom GDOs
            // AddGameData();

            // Perform actions when game data is built
            Events.BuildGameDataEvent += delegate (object s, BuildGameDataEventArgs args)
            {
                if (args.gamedata.TryGet(ApplianceReferences.Piano, out Appliance piano, warn_if_fail: true) && !HasProperty<CMusicalInstrument>(piano))
                {
                    piano.Properties.Add(new CMusicalInstrument()
                    {
                        Type = InstrumentType.Piano,
                        VolumeMultiplier = 1f,
                        ClipCopiesCount = 5
                    });
                }
            };
        }

        public static bool LoadAudioClipFromAssetBundle(string assetPath, out AudioClip clip)
        {
            clip = Bundle?.LoadAsset<AudioClip>(assetPath);

            if (clip == null)
            {
                Debug.LogError($"Failed to load asset {assetPath} from AssetBundle");
                return false;
            }

            return true;
        }

        private bool HasProperty<T>(Appliance appliance) where T : IApplianceProperty
        {
            return appliance.Properties.Select(x => x.GetType() == typeof(T)).Count() > 0;
        }

        private bool HasProperty<T>(Item item) where T : IItemProperty
        {
            return item.Properties.Select(x => x.GetType() == typeof(T)).Count() > 0;
        }

        #region Logging
        public static void LogInfo(string _log) { Debug.Log($"[{MOD_NAME}] " + _log); }
        public static void LogWarning(string _log) { Debug.LogWarning($"[{MOD_NAME}] " + _log); }
        public static void LogError(string _log) { Debug.LogError($"[{MOD_NAME}] " + _log); }
        public static void LogInfo(object _log) { LogInfo(_log.ToString()); }
        public static void LogWarning(object _log) { LogWarning(_log.ToString()); }
        public static void LogError(object _log) { LogError(_log.ToString()); }
        #endregion
    }
}
