using System;
using XRL.World.Effects;

namespace XRL.World.Parts
{
    [Serializable]
	public class CyberneticsBurstJets : IPart
	{
		public Guid ActivatedAbilityID = Guid.Empty;

		public override bool SameAs(IPart p)
		{
			return false;
		}

		public override bool AllowStaticRegistration()
		{
			return false;
		}

		public int GetCooldown()
		{
			return 150;
		}

		public int GetDuration()
		{
			return 1;
		}

		public void CollectStats(Templates.StatCollector stats)
		{
			stats.Set("Duration", GetDuration());
			stats.CollectCooldownTurns("Cooldown", GetCooldown());
		}

		public override bool WantEvent(int ID, int cascade)
		{
			if (base.WantEvent(ID, cascade)
				|| ID == AIGetMovementAbilityListEvent.ID
				|| ID == AIGetOffensiveAbilityListEvent.ID 
				|| ID == ImplantedEvent.ID 
				|| ID == CommandEvent.ID
				|| ID == UnImplantedEvent.ID
				|| ID == BeforeAbilityManagerOpenEvent.ID)
			{
				return true;
			}
			return false;
		}

		public override bool HandleEvent(BeforeAbilityManagerOpenEvent E)
		{
            DescribeMyActivatedAbility(ActivatedAbilityID, CollectStats, ParentObject?.Implantee);
			return base.HandleEvent(E);
		}

		public override bool HandleEvent(AIGetMovementAbilityListEvent E)
		{
			if (E.Actor == ParentObject.Equipped && E.Distance - E.StandoffDistance * 2 >= 10 && E.Actor.IsActivatedAbilityAIUsable(ActivatedAbilityID))
			{
				E.Add("ActivateBurstJet", 1, ParentObject, Inv: true);
			}
			return base.HandleEvent(E);
		}

		public override bool HandleEvent(AIGetOffensiveAbilityListEvent E)
		{
			if (E.Actor == ParentObject.Implantee && E.Actor.IsActivatedAbilityAIUsable(ActivatedAbilityID))
			{
				E.Add("ActivateBurstJet", 1, ParentObject, Inv: true);
			}
			return base.HandleEvent(E);
		}

        public override bool HandleEvent(ImplantedEvent E)
        {
            ActivatedAbilityID = E.Implantee.AddActivatedAbility("Activate Afterburners", "ActivateBurstJet", "Cybernetics", "You may perform a dash attack during your next action.");
            return base.HandleEvent(E);
        }

        public override bool HandleEvent(UnimplantedEvent E)
        {
            E.Implantee.RemoveActivatedAbility(ref ActivatedAbilityID);
            return base.HandleEvent(E);
        }

		public override bool HandleEvent(CommandEvent E)
        {
            if (E.Command == commandId && E.Actor == ParentObject.Implantee)
            {
                if (base.OnWorldMap)
                {
                    return ParentObject.Fail("You cannot do that on the world map.");
                }
				if (!ActivateBurstJet())
				{
					return false;
				}
			}
			return base.FireEvent(E);
		}

		private bool ActivateBurstJet()
		{
			GameObject Implantee = ParentObject.Implantee;
			if (Implantee == null)
			{
				return false;
			}
			if (!Implantee.IsActivatedAbilityUsable(ActivatedAbilityID))
			{
				return false;
			}
			if (!Implantee.ApplyEffect(new Dashing(GetDuration())))
			{
				return false;
			}
			IComponent<GameObject>.XDidY(Implantee, "start", "dashing in a plume of flame and smoke", "!", null, null, Implantee);
			Implantee.CooldownActivatedAbility(ActivatedAbilityID, GetCooldown());
			Implantee.PlayWorldSound("Sounds/Interact/sfx_interact_jetpack_activate");
			return true;
		}
	}
}