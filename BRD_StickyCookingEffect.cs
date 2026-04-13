using System;
using System.Collections.Generic;
using XRL.World.Parts;

namespace XRL.World.Effects
{

    [Serializable]
    public class BRD_StickyCookingEffect : ProceduralCookingEffect
    {
        public const string RemoveStickyEventId = "RemoveStickyProceduralCookingEffects";

        public BRD_StickyCookingEffect()
        {
            DisplayName = "{{W|integrated}}";
            Duration = 1;
        }

        public override void Register(GameObject Object, IEventRegistrar Registrar)
        {
            Registrar.Register("JoinedPartyLeader");
            Registrar.Register(RemoveStickyEventId);
        }

        public override bool FireEvent(Event E)
        {
            if (E.ID == RemoveStickyEventId)
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

        public static bool TryStickifyFirstVanilla(GameObject go)
        {
            ProceduralCookingEffect src = null;
            foreach (Effect e in go.Effects)
            {
                if (e is ProceduralCookingEffect pce && e is not BRD_StickyCookingEffect && e is not BRD_StickyCookingWithTriggerEffect)
                {
                    src = pce;
                    break;
                }
            }
            if (src == null)
            {
                return false;
            }
            ProceduralCookingEffect sticky = CreateStickyCloneFrom(src);
            int duration = src.Duration;
            string displayName = src.DisplayName;
            go.RemoveEffect(src);
            sticky.Duration = duration;
            sticky.DisplayName = displayName;
            sticky.Init(go);
            go.ApplyEffect(sticky);
            return true;
        }

        public static ProceduralCookingEffect CreateStickyCloneFrom(ProceduralCookingEffect src)
        {
            if (src is ProceduralCookingEffectWithTrigger wts)
            {
                BRD_StickyCookingWithTriggerEffect sticky = new BRD_StickyCookingWithTriggerEffect();
                foreach (ProceduralCookingEffectUnit unit in wts.units)
                {
                    sticky.AddUnit(unit.DeepCopy(sticky));
                }
                foreach (ProceduralCookingTriggeredAction action in wts.triggeredActions)
                {
                    sticky.triggeredActions.Add(action.DeepCopy());
                }
                return sticky;
            }
            else
            {
                BRD_StickyCookingEffect sticky = new BRD_StickyCookingEffect();
                foreach (ProceduralCookingEffectUnit unit in src.units)
                {
                    sticky.AddUnit(unit.DeepCopy(sticky));
                }
                return sticky;
            }
        }
    }
}