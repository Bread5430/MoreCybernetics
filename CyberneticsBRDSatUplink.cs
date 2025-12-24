using System;
using XRL.World.Effects;

namespace XRL.World.Parts
{
    [Serializable]
	public class CyberneticsBRDSatUplink : IPart
	{
		public Guid ActivatedAbilityID = Guid.Empty;

		List<string> OptionStrings = new List<string>
		{
			"Mech Drop", // Steel's Perfection
			"Supply Drop", // Boundless Riches
			"Orbital Strike" // Heaven's Fire
		};

		List<string> keymap = new List<string>
		{
			"1",
			"2",
			"3"
		};

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

		public override bool HandleEvent(AIGetOffensiveAbilityListEvent E)
		{
			if (E.Actor == ParentObject.Implantee && E.Actor.IsActivatedAbilityAIUsable(ActivatedAbilityID))
			{
				E.Add("ActivateUplink", 1, ParentObject, Inv: true);
			}
			return base.HandleEvent(E);
		}

        public override bool HandleEvent(ImplantedEvent E)
        {
            ActivatedAbilityID = E.Implantee.AddActivatedAbility("Open Com-link", "ActivateUplink", "Cybernetics", "Hack into a local kill-sat, and call down orbital support.");
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
				// The Archon accepts your prayers Aristocrat.\n Choose your blessing.
				int choice_num = Popup.PickOption("Orbital Support Requested:\n Select your package:", null, "", "Sounds/UI/ui_notification", OptionStrings.ToArray(), keymap.ToArray(), null, null, null, null, null, 0, 60, 0, -1, AllowEscape: true);
				
				switch (choice_num){
					case 1:
						mech_drop();
						break;
					case 2:
						orbital_strike();
						break;
					case 3:
						supply_drop();
						break;

				}

				if (choice_num > 0)
				{
					gameObject.UseEnergy(1000, "Orbital Package Request");
				}
			}
			return base.FireEvent(E);
		}

		public void mech_drop()
		{
			
		}

		public void orbital_strike()
		{
			
		}

		public void supply_drop()
		{
			
		}

		private void Deploy(Cell Cell, GameObject Object, GameObject Actor)
		{
			if (!Duration.IsNullOrEmpty())
			{
				Object.AddPart(new Temporary(Duration.RollCached()));
			}

			Cell.AddObject(Object);
			Object.MakeActive();
			if (Object.HasStat("XPValue"))
			{
				Object.GetStat("XPValue").BaseValue = 0;
			}
			if (Actor != null)
			{
				Object.SetAlliedLeader<AllyConstructed>(Actor);
				Object.IsTrifling = TriflingCompanion;
			}
			if (DustPuffEach)
			{
				Object.DustPuff();
			}
		}

	}
}