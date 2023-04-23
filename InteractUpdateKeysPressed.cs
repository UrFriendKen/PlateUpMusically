using Kitchen;
using KitchenMods;
using Unity.Entities;

namespace KitchenMusically
{
    public struct CGrabPressed : IComponentData, IModComponent { }
    public struct CActPressed : IComponentData, IModComponent { }
    public struct CNotifyPressed : IComponentData, IModComponent { }

    public abstract class InteractUpdateKeyState : ItemInteractionSystem
    {
        protected override bool RequirePress => false;
        protected override bool RequireHold => false;

        protected override bool IsPossible(ref InteractionData data)
        {
            if (!Has<CMusicalInstrument>(data.Target))
                return false;
            return true;
        }

        protected override void Perform(ref InteractionData data)
        {
            switch (data.Attempt.Type)
            {
                case InteractionType.Grab:
                    Set<CGrabPressed>(data.Target);
                    break;
                case InteractionType.Act:
                    Set<CActPressed>(data.Target);
                    break;
                case InteractionType.Notify:
                    Set<CNotifyPressed>(data.Target);
                    break;
                default:
                    break;
            }
        }
    }

    public class InteractInstrumentGrab : InteractUpdateKeyState
    {
        protected override InteractionType RequiredType => InteractionType.Grab;
    }

    public class InteractInstrumentAct : InteractUpdateKeyState
    {
        protected override InteractionType RequiredType => InteractionType.Act;
    }

    public class InteractInstrumentNotify : InteractUpdateKeyState
    {
        protected override InteractionType RequiredType => InteractionType.Notify;
    }
}
