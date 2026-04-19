using System;
using System.Collections.Generic;
using XRL;
using XRL.World;
using XRL.World.Parts;

namespace XRL.World.Effects
{

    [Serializable]
    public class BRD_StickyCookingEffect : ProceduralCookingEffect
    {
        public const string RemoveStickyEventId = "RemoveStickyProceduralCookingEffects";

        private static readonly HashSet<string> VanillaProceduralCookingHungerOrMassClearEventIds = new HashSet<string>(StringComparer.Ordinal)
        {
            "BecameHungry",
            "BecameFamished",
            "ApplyWellFed",
            "ClearFoodEffects",
            "RemoveProceduralCookingEffects"
        };

        internal static bool IsVanillaProceduralCookingHungerOrMassClearEvent(string eventId)
        {
            return VanillaProceduralCookingHungerOrMassClearEventIds.Contains(eventId);
        }

        private sealed class RegisterStringCollector : IEventRegistrar
        {
            public readonly List<string> StringIds = new List<string>();

            public bool IsUnregister => false;

            public void Register(IEventSource Source, IEventHandler Handler, int EventID, int Order = 0, bool Serialize = false)
            {
            }

            public void Register(IEventSource Source, int EventID, int Order = 0, bool Serialize = false)
            {
            }

            public void Register(int EventID, int Order = 0, bool Serialize = false)
            {
            }

            public void Register(string EventID)
            {
                StringIds.Add(EventID);
            }
        }

        /// <summary>
        /// String events a vanilla <see cref="ProceduralCookingEffectWithTrigger"/> of <paramref name="concreteTriggerEffectType"/>
        /// would register, minus hunger / mass-clear events the metabolic anchor must ignore.
        /// </summary>
        internal static List<string> CollectRetainedRegisterStrings(Type concreteTriggerEffectType, GameObject basis)
        {
            List<string> result = new List<string>();
            if (concreteTriggerEffectType == null || !typeof(ProceduralCookingEffectWithTrigger).IsAssignableFrom(concreteTriggerEffectType))
            {
                return result;
            }
            RegisterStringCollector collector = new RegisterStringCollector();
            Effect dummy = (Effect)Activator.CreateInstance(concreteTriggerEffectType);
            dummy.Register(basis, collector);
            foreach (string id in collector.StringIds)
            {
                if (!VanillaProceduralCookingHungerOrMassClearEventIds.Contains(id))
                {
                    result.Add(id);
                }
            }
            return result;
        }

        public BRD_StickyCookingEffect()
        {
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
            ProceduralCookingEffect src = PickBestProceduralCookingEffectToAnchor(go);
            if (src == null)
            {
                return false;
            }
            ProceduralCookingEffect sticky = CreateStickyCloneFrom(src, go);
            go.RemoveEffect(src);
            sticky.Duration = 1;
            sticky.DisplayName = "{{W|integrated}}";
            sticky.Init(go);
            go.ApplyEffect(sticky);
            return true;
        }

        /// <summary>
        /// Prefer a metabolizing effect that carries trigger logic. Otherwise the first non-sticky
        /// <see cref="ProceduralCookingEffect"/> (list order can put plain effects before trigger meals).
        /// </summary>
        private static ProceduralCookingEffect PickBestProceduralCookingEffectToAnchor(GameObject go)
        {
            ProceduralCookingEffect firstPlain = null;
            foreach (Effect e in go.Effects)
            {
                ProceduralCookingEffect pce = e as ProceduralCookingEffect;
                if (pce == null)
                {
                    continue;
                }
                if (e is BRD_StickyCookingEffect || e is BRD_StickyCookingWithTriggerEffect)
                {
                    continue;
                }
                ProceduralCookingEffectWithTrigger withTrigger = e as ProceduralCookingEffectWithTrigger;
                if (withTrigger != null)
                {
                    return withTrigger;
                }
                if (firstPlain == null)
                {
                    firstPlain = pce;
                }
            }
            return firstPlain;
        }

        public static ProceduralCookingEffect CreateStickyCloneFrom(ProceduralCookingEffect src, GameObject basis)
        {
            if (src is ProceduralCookingEffectWithTrigger)
            {
                return new BRD_StickyCookingWithTriggerEffect
                {
                    Shadow = (ProceduralCookingEffectWithTrigger)src.DeepCopy(basis)
                };
            }
            BRD_StickyCookingEffect sticky = new BRD_StickyCookingEffect();
            foreach (ProceduralCookingEffectUnit unit in src.units)
            {
                sticky.AddUnit(unit.DeepCopy(sticky));
            }
            return sticky;
        }
    }
}