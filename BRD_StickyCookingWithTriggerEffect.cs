using System;
using System.Collections.Generic;
using XRL.World.Parts;

namespace XRL.World.Effects
{
    [Serializable]
    public class BRD_StickyCookingWithTriggerEffect : ProceduralCookingEffectWithTrigger
    {
        public BRD_StickyCookingWithTriggerEffect()
        {
            DisplayName = "{{W|integrated}}";
            Duration = 1;
        }

        public override void Register(GameObject Object, IEventRegistrar Registrar)
        {
            Registrar.Register("JoinedPartyLeader");
            Registrar.Register(BRD_StickyCookingEffect.RemoveStickyEventId);
        }

        public override bool FireEvent(Event E)
        {
            if (E.ID == BRD_StickyCookingEffect.RemoveStickyEventId)
            {
                Duration = 0;
            }
            else if (E.ID == "JoinedPartyLeader")
            {
                CheckNonPlayerExpiry();
            }
            foreach (ProceduralCookingEffectUnit unit in units)
            {
                unit.FireEvent(E);
            }
            return base.FireEvent(E);
        }

        public override void Remove(GameObject Object)
        {
            if (bApplied)
            {
                bApplied = false;
                foreach (ProceduralCookingEffectUnit unit in units)
                {
                    unit.Remove(Object, this);
                }
            }
            foreach (ProceduralCookingTriggeredAction triggeredAction in triggeredActions)
            {
                triggeredAction.Remove(Object);
            }
        }
    }
}