using System;
using XRL.World.Effects;
using System.Collections.Generic;
using XRL.UI;
using XRL.World.AI;
using XRL.Core;
using XRL.UI;
using XRL.World.AI.GoalHandlers;
using XRL.World.Capabilities;

namespace XRL.World.Parts
{
    [Serializable]
	public class CyberneticsBRDSatUplink : IPart
	{
		public Guid ActivatedAbilityID = Guid.Empty;
		public string commandId = "ActivateComLink";


		List<string> OptionStrings = new List<string>
		{
			"Mech Drop", // Steel's Perfection
			"Supply Drop", // Boundless Riches
			"Orbital Strike" // Sultan's Gaze
		};

		List<char> keymap = new List<char>
		{
			'1',
			'2',
			'3'
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
			stats.Set("Cooldown", GetCooldown());
		}

		public override bool WantEvent(int ID, int cascade)
		{
			if (base.WantEvent(ID, cascade)
				|| ID == AIGetOffensiveAbilityListEvent.ID 
				|| ID == ImplantedEvent.ID 
				|| ID == CommandEvent.ID
				|| ID == UnimplantedEvent.ID
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
            ActivatedAbilityID = E.Implantee.AddActivatedAbility("Open Com-link", commandId, "Cybernetics", "Hack into a local kill-sat, and call down orbital support.");
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
						delayedActionIndicator("mech");
						break;
					case 2:
						delayedActionIndicator("strike");
						break;
					case 3:
						delayedActionIndicator("supply");
						break;

				}

				if (choice_num > 0)
				{
					E.Actor.UseEnergy(1000, "Orbital Package Request");
				}
			}
			return base.HandleEvent(E);
		}

		public void delayedActionIndicator(string action_type)
		{

			Cell cell = PickDestinationCell(12, RequireCombat: true, Label: "Choose Calldown Destination", Snap: true);
			if (cell == null)
			{
				return false;
			}

			GameObject widget = GameObjectFactory.Factory.CreateObject("Widget");
			widget.AddPart(new BRDDelayedActivation(ParentObject, 4, action_type));

			cell.AddObject(widget);
			PlayWorldSound("Sounds/Missile/Fires/Heavy Weapons/sfx_missile_missileLauncher_fire");

			MissileWeaponVFXConfiguration vfx = MissileWeaponVFXConfiguration.next();
			CombatJuiceManager.startDelay();
			vfx.addStep(0, ParentObject.CurrentCell.Location);
			vfx.addStep(0, cell.Location);
			vfx.setPathProjectileVFX(0, "MissileWeaponsEffects/vls_laser", "duration::1;;beamColor0::#FFFFFF;;beamColor1::#FFFFFF");
			CombatJuiceManager.endDelay();
			CombatJuice.missileWeaponVFX(vfx);

			if (!ParentObject.IsPlayer())
			{
				ParentObject.Brain.RemoveGoalsDescendedFrom<IMovementGoal>();
				ParentObject.Brain.PushGoal(new FleeLocation(cell, (200 - ParentObject.Stat("MoveSpeed", 100)) * 3 / 100));
			}

		}



		public class BRDDelayedActivation : IPart
		{
			public GameObject owner;
			public int turns;

			public String activation_type;

			public BRDDelayedActivation(GameObject owner, int turns, String activation_type)
			{
				this.owner = owner;
				this.turns = turns;
				this.activation_type = activation_type;
			}

			public override bool AllowStaticRegistration()
			{
				return true;
			}

			public override bool WantEvent(int ID, int Cascade)
			{
				return ID == EndTurnEvent.ID;
			}

			public override bool FinalRender(RenderEvent E, bool bAlt)
			{
				E.WantsToPaint = true;
				return base.FinalRender(E, bAlt);
			}

			public override void OnPaint(ScreenBuffer buffer)
			{
				int num = XRLCore.CurrentFrame % 60;
				Location2D cell = ParentObject.CurrentCell.Location;
				ConsoleChar consoleChar;
				if (num < 30)
				{
					int radius = num / 10;
					if (cell != null)
					{
						for (int i = -radius; i <= radius; i++)
						{
							for (int j = -radius; j <= radius; j++)
							{
								Location2D loc = Location2D.Get(cell.X + i, cell.Y + j);
								if (loc != null && loc.Distance(cell) == radius)
								{
									consoleChar = buffer.get(loc.X, loc.Y);
									if (consoleChar != null)
									{
										consoleChar.Tile = null;
										consoleChar.Char = '!';
										consoleChar.Foreground = The.Color.R;
									}
								}
							}
						}
					}
				}
				consoleChar = buffer.get(cell.X, cell.Y);
				if (consoleChar != null)
				{
					consoleChar.Tile = null;
					consoleChar.Char = 'X';
					consoleChar.Foreground = The.Color.R;
				}
			}

			public override bool HandleEvent(EndTurnEvent E)
			{
				if (--turns <= 0)
				{
					if (Options.UseParticleVFX && ParentObject.CurrentZone != null & ParentObject.CurrentZone.IsActive())
					{
						CombatJuice.playPrefabAnimation(ParentObject.CurrentCell.Location, "MissileWeaponsEffects/vls_impact");
						CombatJuiceWait(0.5f);
					}
					
					// Activate the effect of the action based on the saved action string
					if (activation_type == "mech")
					{
						
					} else if (activation_type == "strike")
					{
						
					} else if (activation_type == "supply")
					{
						
					}
					
					ParentObject.Obliterate();
				}
				else
				{
					foreach (Cell cell in ParentObject.CurrentCell.GetAdjacentCells(4))
					{
						foreach (GameObject obj in cell.Objects)
						{
							if (!obj.IsCombatObject())
							{
								continue;
							}
							if (obj.IsPlayer())
							{
								AutoAct.Interrupt("you are in the area of " + owner.poss("orbital calldown."));
								continue;
							}
							if (!obj.IsPotentiallyMobile() || obj.Brain.Goals.Peek() is FleeLocation)
							{
								continue;
							}
							obj.Brain.RemoveGoalsDescendedFrom<IMovementGoal>();
							obj.Brain.PushGoal(new FleeLocation(ParentObject.CurrentCell, (200 - obj.Stat("MoveSpeed", 100)) * turns / 100));
						}
					}
				}
				return base.HandleEvent(E);
			}

			public void mech_drop()
			{
				
			}

			public void orbital_strike()
			{
				List<GameObject> hit = Event.NewGameObjectList();
				hit.Add(ParentObject);
				Physics.ApplyExplosion(ParentObject.CurrentCell, 30000, Hit: hit, Local: false, Owner: owner); //, Phase: phase);
				
			}

			public void supply_drop()
			{
				
			}

			private void Deploy(Cell Cell, GameObject Object, GameObject Actor)
			{

				Cell.AddObject(Object);
				Object.MakeActive();
				if (Object.HasStat("XPValue"))
				{
					Object.GetStat("XPValue").BaseValue = 0;
				}
				if (Actor != null)
				{
					Object.SetAlliedLeader<AllyConstructed>(Actor);
					Object.IsTrifling = true;
				}
				Object.DustPuff();
			}
		}

	}
}