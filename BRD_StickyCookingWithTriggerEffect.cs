using System;
using System.Collections.Generic;
using XRL;
using XRL.World;
namespace XRL.World.Effects
{
    [Serializable]
    public class BRD_StickyCookingWithTriggerEffect : ProceduralCookingEffectWithTrigger
    {
        /// <summary>
        /// Deep clone of the original procedural cooking effect (same concrete subtype as the meal).
        /// Holds units, triggered actions, tier fields, and domain-specific <see cref="FireEvent"/> logic.
        /// </summary>
        public ProceduralCookingEffectWithTrigger Shadow;

        public BRD_StickyCookingWithTriggerEffect()
        {
            DisplayName = "{{W|integrated}}";
            Duration = 1;
        }

        public override Effect DeepCopy(GameObject Parent)
        {
            ProceduralCookingEffectWithTrigger shadowCopy = Shadow != null
                ? (ProceduralCookingEffectWithTrigger)Shadow.DeepCopy(Parent)
                : null;
            BRD_StickyCookingWithTriggerEffect copy = (BRD_StickyCookingWithTriggerEffect)base.DeepCopy(Parent);
            copy.Shadow = shadowCopy;
            return copy;
        }

        public override void Init(GameObject target)
        {
            if (Shadow != null)
            {
                Shadow.Object = target;
                Shadow.Init(target);
                Shadow.DisplayName = DisplayName;
            }
            init = true;
            bApplied = false;
        }

        public override bool Apply(GameObject Object)
        {
            if (Shadow == null)
            {
                return false;
            }
            bool ok = Shadow.Apply(Object);
            bApplied = Shadow.bApplied;
            StartTick = Shadow.StartTick;
            Duration = Shadow.Duration;
            return ok;
        }

        public override void Register(GameObject Object, IEventRegistrar Registrar)
        {
            if (Shadow != null)
            {
                foreach (string eventId in BRD_StickyCookingEffect.CollectRetainedRegisterStrings(Shadow.GetType(), Object))
                {
                    Registrar.Register(eventId);
                }
            }
            Registrar.Register(BRD_StickyCookingEffect.RemoveStickyEventId);
        }

        public override bool WantEvent(int ID, int cascade)
        {
            return base.WantEvent(ID, cascade) || (Shadow != null && Shadow.WantEvent(ID, cascade));
        }

        public override bool HandleEvent(MinEvent E)
        {
            if (Shadow == null)
            {
                return base.HandleEvent(E);
            }
            return Shadow.HandleEvent(E);
        }

        public override bool FireEvent(Event E)
        {
            if (Shadow == null)
            {
                return base.FireEvent(E);
            }
            if (E.ID == BRD_StickyCookingEffect.RemoveStickyEventId)
            {
                Duration = 0;
                Shadow.Duration = 0;
                return true;
            }
            if (E.ID == "JoinedPartyLeader")
            {
                StartTick = Shadow.StartTick;
                Duration = Shadow.Duration;
                CheckNonPlayerExpiry();
                Shadow.Duration = Duration;
                Shadow.StartTick = StartTick;
            }
            if (BRD_StickyCookingEffect.IsVanillaProceduralCookingHungerOrMassClearEvent(E.ID))
            {
                foreach (ProceduralCookingEffectUnit unit in Shadow.units)
                {
                    unit.FireEvent(E);
                }
                return true;
            }
            bool result = Shadow.FireEvent(E);
            Duration = Shadow.Duration;
            StartTick = Shadow.StartTick;
            return result;
        }

        public override void Remove(GameObject Object)
        {
            if (Shadow != null)
            {
                if (Shadow.bApplied)
                {
                    Shadow.bApplied = false;
                    foreach (ProceduralCookingEffectUnit unit in Shadow.units)
                    {
                        unit.Remove(Object, Shadow);
                    }
                }
                foreach (ProceduralCookingTriggeredAction triggeredAction in Shadow.triggeredActions)
                {
                    triggeredAction.Remove(Object);
                }
            }
        }

        public override string GetDescription()
        {
            return Shadow != null ? Shadow.GetDescription() : base.GetDescription();
        }

        public override string GetDetails()
        {
            return Shadow != null ? Shadow.GetDetails() : base.GetDetails();
        }

        public override string GetTemplatedProceduralEffectDescription()
        {
            return Shadow != null ? Shadow.GetTemplatedProceduralEffectDescription() : base.GetTemplatedProceduralEffectDescription();
        }

        public override string GetProceduralEffectDescription()
        {
            return Shadow != null ? Shadow.GetProceduralEffectDescription() : base.GetProceduralEffectDescription();
        }
    }
}
