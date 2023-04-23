using Kitchen;
using Kitchen.Components;
using KitchenData;
using KitchenMods;
using MessagePack;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace KitchenMusically
{
    public enum InstrumentType
    {
        None,
        Piano
    }

    public struct CMusicalInstrument : IComponentData, IApplianceProperty, IAttachableProperty, IModComponent
    {
        public InstrumentType Type;
        public int ClipCopiesCount;
    }

    public class MusicalInstrumentView : UpdatableObjectView<MusicalInstrumentView.ViewData>
    {

        protected readonly Dictionary<InstrumentType, string> VOLUME_PREFERENCE_MAP = new Dictionary<InstrumentType, string>()
        {
            { InstrumentType.Piano, "pianoVolume"}
        };

        public class UpdateView : IncrementalViewSystemBase<ViewData>, IModSystem
        {
            private EntityQuery Views;

            protected override void Initialise()
            {
                base.Initialise();

                Views = GetEntityQuery(new QueryHelper()
                    .All(typeof(CMusicalInstrument), typeof(CLinkedView)));
            }

            protected override void OnUpdate()
            {
                using var entities = Views.ToEntityArray(Allocator.Temp);
                using var instruments = Views.ToComponentDataArray<CMusicalInstrument>(Allocator.Temp);
                using var views = Views.ToComponentDataArray<CLinkedView>(Allocator.Temp);

                float dt = Time.DeltaTime;

                for (var i = 0; i < views.Length; i++)
                {
                    var entity = entities[i];
                    var instrument = instruments[i];
                    var view = views[i];

                    ViewData data = new ViewData
                    {
                        IsGrabPressed = Has<CGrabPressed>(entity),
                        IsActPressed = Has<CActPressed>(entity),
                        IsNotifyPressed = Has<CNotifyPressed>(entity),
                        Type = instrument.Type,
                        ClipCopiesCount = instrument.ClipCopiesCount
                    };
                    SendUpdate(view, data);
                }
                EntityManager.RemoveComponent<CGrabPressed>(entities);
                EntityManager.RemoveComponent<CActPressed>(entities);
                EntityManager.RemoveComponent<CNotifyPressed>(entities);
            }
        }

        [MessagePackObject(false)]
        public struct ViewData : ISpecificViewData, IViewData.ICheckForChanges<ViewData>
        {
            [Key(1)] public bool IsGrabPressed;
            [Key(2)] public bool IsActPressed;
            [Key(3)] public bool IsNotifyPressed;
            [Key(4)] public InstrumentType Type;
            [Key(5)] public int ClipCopiesCount;

            public IUpdatableObject GetRelevantSubview(IObjectView view)
            {

                GameObject gameObject = view.GameObject;
                if (!gameObject.GetComponent<MusicalInstrumentView>())
                {
                    MusicalInstrumentView instrumentView = gameObject.AddComponent<MusicalInstrumentView>();

                    instrumentView.Clips.Clear();
                    if (Main.INSTRUMENT_CLIP_NAMES.TryGetValue(Type, out List<string> clipNames))
                    {
                        foreach (string clipName in clipNames)
                        {
                            if (Main.LoadAudioClipFromAssetBundle(clipName, out AudioClip clip))
                            {
                                instrumentView.Clips.Add(clip);
                                Main.LogInfo($"Loaded {clipName} for {Type} (Clip Count = {instrumentView.Clips.Count})");
                            }
                        }
                    }
                    Main.LogInfo($"Added MusicalInstrumentView to {gameObject.name}");
                }
                return view.GetSubView<MusicalInstrumentView>();
            }

            public bool IsChangedFrom(ViewData cached)
            {
                return IsGrabPressed != cached.IsGrabPressed ||
                    IsActPressed != cached.IsActPressed ||
                    IsNotifyPressed != cached.IsNotifyPressed;
            }
        }

        private InstrumentType _instrumentType = InstrumentType.None;

        private bool _isPlaying = false;
        private List<SoundSource> _soundSources = new List<SoundSource>();
        private int _clipCopiesCount = 0;
        public List<AudioClip> Clips = new List<AudioClip>();

        public GameObject VfxGameObject;

        // This receives the updated data from the ECS backend whenever an update is sent
        // In general, this should update the state of the view to match the values in view_data
        // ideally ignoring all current state; it's possible that not all updates will be received so
        // you should avoid relying on previous state (Non-public fields above) where possible
        protected override void UpdateData(ViewData view_data)
        {
            _isPlaying = view_data.IsActPressed;
            _clipCopiesCount = view_data.ClipCopiesCount;
            _instrumentType = view_data.Type;
        }

        void Update()
        {
            UpdatePlaying();
            VfxGameObject?.SetActive(_soundSources.Where(source => source.IsPlaying).Count() > 0);
        }

        void UpdatePlaying()
        {
            if (_soundSources.Count == 0)
            {
                if (_clipCopiesCount < 1)
                    _clipCopiesCount = 1;
                for (int i = 0; i < _clipCopiesCount; i++)
                {
                    for (int j = 0; j < Clips.Count; j++)
                    {
                        GameObject gO = new GameObject($"SoundSource{i * Clips.Count + j}");
                        SoundSource soundSource = gO.AddComponent<SoundSource>();
                        gO.transform.ParentTo(transform);
                        soundSource.Configure(SoundCategory.Effects, Clips[j]);
                        soundSource.ShouldLoop = false;
                        soundSource.TransitionTime = 1f;
                        _soundSources.Add(soundSource);
                    }
                }
            }

            if (Clips.Count == 0)
                return;

            if (_isPlaying)
            {
                bool needToPlay = true;
                _soundSources.ShuffleInPlace();
                for (int i = 0; (i < _soundSources.Count) && needToPlay; i++)
                {
                    if (_soundSources[i].IsPlaying)
                        continue;
                    _soundSources[i].VolumeMultiplier = VOLUME_PREFERENCE_MAP.TryGetValue(_instrumentType, out string prefKey)? Main.PrefManager.Get<float>(prefKey) : 0.5f;
                    _soundSources[i].Play();
                    needToPlay = false;
                }
                _isPlaying = false;
            }
        }
    }
}
