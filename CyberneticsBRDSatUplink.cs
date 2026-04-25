using System;
using System.Collections.Generic;
using ConsoleLib.Console;
using Genkit;
using XRL;
using XRL.Core;
using XRL.UI;
using XRL.World;
using XRL.World.AI;
using XRL.World.AI.GoalHandlers;
using XRL.Rules;
using XRL.World.Capabilities;
using XRL.World.Effects;
using static XRL.Liquids.LiquidWarmStatic;

namespace XRL.World.Parts
{
    [Serializable]
	public class CyberneticsBRDSatUplink : IPart
	{
		public Guid ActivatedAbilityID = Guid.Empty;
		public string commandId = "BRD_ActivateComLink";


		List<string> OptionStrings = new List<string>
		{
			"Mechsuit Drop",
			"Swarmer Support",
			"Ontokinetic Strike"
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

		public void CollectStats(Templates.StatCollector stats)
		{
			stats.Set("Range", 12);
			stats.Set("Cooldown", GetAvailableComputePowerEvent.AdjustDown(ParentObject, 3000));
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
				int choice_num = Popup.PickOption("Select your package:", null, "", "Sounds/UI/ui_notification", OptionStrings.ToArray(), keymap.ToArray(), null, null, null, null, null, 0, 60, 0, -1, AllowEscape: true);

				switch (choice_num){
					case 0:
						delayedActionIndicator("mech");
						break;
					case 1:
						delayedActionIndicator("turret");
						break;
					case 2:
						delayedActionIndicator("strike");
						break;

				}

				if (choice_num > 0)
				{
					E.Actor.UseEnergy(1000, "Orbital Package Request");
					//set cooldown after use
					E.Actor.CooldownActivatedAbility(ActivatedAbilityID, GetAvailableComputePowerEvent.AdjustDown(E.Actor, 3000));
				}
			}
			return base.HandleEvent(E);
		}
		public override bool HandleEvent(GetShortDescriptionEvent E)
		{
			E.Postfix.AppendRules("Compute power on the local lattice reduces this item's cooldown.");
			return base.HandleEvent(E);
		}

		public void delayedActionIndicator(string action_type)
		{
			//IComponent<GameObject>.AddPlayerMessage("debug: delayedActionIndicator event fired", 'g');

			GameObject actor = ParentObject.Implantee;
			if (actor == null)
			{
				return;
			}
			//IComponent<GameObject>.AddPlayerMessage("debug: implantee found", 'g');
			// Same as other cyberware: PickDestinationCell on IPart uses the implant item as basis, which is not
			// IsSelfControlledPlayer, so the picker never runs and returns null. Target from the implantee's Physics.
			Cell cell = actor.Physics.PickDestinationCell(12, AllowVis.OnlyVisible, Locked: true, IgnoreSolid: false, IgnoreLOS: true, RequireCombat: true, PickTarget.PickStyle.EmptyCell, "Choose Calldown Destination", Snap: true);
			if (cell == null)
			{
				return;
			}
			//IComponent<GameObject>.AddPlayerMessage("debug: cell found", 'g');

			GameObject widget = GameObjectFactory.Factory.CreateObject("Widget");
			widget.AddPart(new BRDDelayedActivation(ParentObject, 4, action_type));
			//IComponent<GameObject>.AddPlayerMessage("debug: widget created", 'g');

			cell.AddObject(widget);
			PlayWorldSound("Sounds/Missile/Fires/Heavy Weapons/sfx_missile_missileLauncher_fire");

			if (!actor.IsPlayer() && actor.Brain != null)
			{
				actor.Brain.RemoveGoalsDescendedFrom<IMovementGoal>();
				actor.Brain.PushGoal(new FleeLocation(cell, (200 - actor.Stat("MoveSpeed", 100)) * 3 / 100));
			}

		}



		public class BRDDelayedActivation : IPart
		{
			public GameObject owner;
			public int turns;

			public String activation_type;

			public BRDDelayedActivation(GameObject owner, int turns, String activation_type)
			{
				//IComponent<GameObject>.AddPlayerMessage("debug: BRDDelayedActivation constructor fired", 'g');
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
				Location2D root = ParentObject?.CurrentCell?.Location;
				if (root == null)
				{
					return;
				}
				ConsoleChar consoleChar;
				if (num < 30)
				{
					int radius = num / 10;
					for (int i = -radius; i <= radius; i++)
					{
						for (int j = -radius; j <= radius; j++)
						{
							Location2D loc = Location2D.Get(root.X + i, root.Y + j);
							if (loc != null && loc.Distance(root) == radius)
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
				consoleChar = buffer.get(root.X, root.Y);
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
						mech_drop();
					} else if (activation_type == "strike")
					{
						orbital_strike();
					} else if (activation_type == "turret")
					{
						turret_drop();
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
				Cell calldownCell = ParentObject.CurrentCell;
				if (calldownCell == null)
				{
					return;
				}
				GameObject deployer = owner?.Implantee;
				if (deployer == null)
				{
					return;
				}
				string blueprint = CyberneticsBRDSatUplink.ResolveTemplarMechaBlueprintName();
				GameObject mech = GameObject.Create(blueprint);
				if (mech == null)
				{
					return;
				}
				Cell placeCell = CyberneticsBRDSatUplink.FindEmptyCellForMech(mech, calldownCell);
				if (placeCell == null)
				{
					mech.Obliterate();
					return;
				}
				placeCell.AddObject(mech);
				mech.MakeActive();
				mech.PlayWorldSound("Sounds/Robot/sfx_turret_deploy");
				CyberneticsBRDSatUplink.ApplyMechDropVehicleState(mech, deployer);
				mech.AddPart(new BRDMechCalldownCleanup
				{
					Deployer = deployer
				});
			}

			public void orbital_strike()
			{
				//  trigger the pour effect for warm static
				Zone Z = ParentObject.CurrentCell.ParentZone;
				// LiquidWarmStatic.GlitchZone(Z);
				// Directly copy the code instead of calling the function so that I can alter the chances of movement and glitching
				GameManager.Instance.StaticEffecting = 4;
				SoundManager.PlayUISound("sfx_warmStaticSizzle");
				List<GameObject> objects = Z.GetObjects(o => o != null && o.IsReal && o.Render != null && o.Render.Visible);
				Stat.PushState("WarmStaticGlitchZone" + Z.ZoneID);
				try
				{
					foreach (GameObject item in objects)
					{

						MoveToRandomIn(Z, item);
						if (50.in100())
						{
							GlitchObject(item);
						}
					}
				}
				finally
				{
					Stat.PopState();
				}


				// Then trigger explosion
				List<GameObject> hit = Event.NewGameObjectList();
				hit.Add(ParentObject);
				Physics.ApplyExplosion(ParentObject.CurrentCell, 500000, Hit: hit, Local: false, Owner: owner); //, Phase: phase);
			}

			public void turret_drop()
			{
				Cell baseCell = ParentObject.CurrentCell;
				if (baseCell == null)
				{
					return;
				}
				GameObject deployer = owner?.Implantee;
				if (deployer == null)
				{
					return;
				}
				string swarmWeaponBlueprint = ResolveSwarmTurretWeaponBlueprint();
				if (swarmWeaponBlueprint.IsNullOrEmpty())
				{
					return;
				}
				int radius = 2;
				int count = 3;
				List<Cell> candidateCells = baseCell.GetLocalAdjacentCells(radius);
				Dictionary<int, bool> usedCells = new Dictionary<int, bool>();
				for (int i = 0; i < count; i++)
				{
					int tries = 0;
					Cell cell = null;
					do
					{
						cell = candidateCells.GetRandomElement();
						if (cell == null)
						{
							break;
						}
						if (CanDeployTurretAt(cell, usedCells))
						{
							break;
						}
						cell = null;
					}
					while (++tries < 10);
					if (cell == null)
					{
						continue;
					}
					usedCells[cell.LocalCoordKey] = true;
					GameObject turret = IntegratedWeaponHosts.GenerateTurret(GameObject.Create(swarmWeaponBlueprint), deployer, overrideSupply: true);
					cell.AddObject(turret);
					turret.MakeActive();
					// Keep supply local and deterministic: no player ammo transfer prompt, 100 HE missiles loaded.
					turret.SetIntProperty("IntegratedWeaponHostShots", 100);
					turret.FireEventOnBodyparts(Event.New("GenerateIntegratedHostInitialAmmo", "Host", turret));
					turret.ReceiveObject("HE Missile", 100);
					CommandReloadEvent.Execute(turret, FreeAction: true);
					turret.PlayWorldSound("Sounds/Robot/sfx_turret_deploy");
				}
			}

			private static bool CanDeployTurretAt(Cell cell, Dictionary<int, bool> usedCells)
			{
				if (cell == null)
				{
					return false;
				}
				if (usedCells != null && usedCells.ContainsKey(cell.LocalCoordKey))
				{
					return false;
				}
				if (!cell.IsPassable())
				{
					return false;
				}
				if (cell.HasObjectWithTag("ExcavatoryTerrainFeature"))
				{
					return false;
				}
				if (!cell.IsEmpty())
				{
					return false;
				}
				return true;
			}

			private static string ResolveSwarmTurretWeaponBlueprint()
			{
				string[] preferred = new string[4] { "Swarm Rack", "Swarm Missile Rack", "Swarm Launcher", "Swarmer Missile Rack" };
				foreach (string blueprintName in preferred)
				{
					GameObjectBlueprint preferredBlueprint = GameObjectFactory.Factory.GetBlueprint(blueprintName);
					if (preferredBlueprint != null)
					{
						return preferredBlueprint.Name;
					}
				}
				foreach (GameObjectBlueprint bp in GameObjectFactory.Factory.BlueprintList)
				{
					if (!bp.DescendsFrom("MissileWeapon"))
					{
						continue;
					}
					string name = bp.Name ?? "";
					string displayName = bp.CachedDisplayNameStripped ?? "";
					if (name.IndexOf("Swarm", StringComparison.OrdinalIgnoreCase) >= 0 || displayName.IndexOf("Swarm", StringComparison.OrdinalIgnoreCase) >= 0)
					{
						return bp.Name;
					}
				}
				return null;
			}
		}

		private static string ResolveTemplarMechaBlueprintName()
		{
			List<string> candidates = new List<string>();
			foreach (GameObjectBlueprint bp in GameObjectFactory.Factory.BlueprintList)
			{
				if (bp.DescendsFrom("VehicleTemplarMech"))
				{
					candidates.Add(bp.Name);
				}
			}
			if (candidates.Count == 0)
			{
				return "Templar Mecha";
			}
			return candidates.GetRandomElement();
		}

		private static Cell FindEmptyCellForMech(GameObject mech, Cell calldownCell)
		{
			if (calldownCell == null || mech == null)
			{
				return null;
			}
			Cell.SpiralEnumerator enumerator = calldownCell.IterateAdjacent(10, IncludeSelf: false, LocalOnly: true);
			while (enumerator.MoveNext())
			{
				Cell cell = enumerator.Current;
				if (cell.IsReachable() && cell.IsEmptyFor(mech) && !cell.HasObjectWithTag("ExcavatoryTerrainFeature"))
				{
					return cell;
				}
			}
			return null;
		}

		// Clears BindBlueprint so VehicleSeat does not require a military security card; OwnerID grants access (Vehicle.cs).
		private static void ApplyMechDropVehicleState(GameObject mech, GameObject deployer)
		{
			if (!GameObject.Validate(ref mech) || deployer == null)
			{
				return;
			}
			NormalizeMechSpawnState(mech);
			if (!mech.TryGetPart<Vehicle>(out Vehicle veh))
			{
				return;
			}
			GameObject pilot = veh.Pilot;
			if (pilot != null && GameObject.Validate(ref pilot) && pilot != deployer && !pilot.IsPlayer())
			{
				pilot.Die(mech);
			}
			veh.BindBlueprint = null;
			if (deployer.IsPlayer())
			{
				veh.OwnerID = "Player";
			}
			else
			{
				veh.OwnerID = deployer.ID;
			}
			mech.SetAlliedLeader<AllyConstructed>(deployer);
		}

		private static void NormalizeMechSpawnState(GameObject mech)
		{
			if (mech == null)
			{
				return;
			}
			if (mech.HasStat("Hitpoints"))
			{
				int maxHp = mech.GetStat("Hitpoints").BaseValue;
				if (maxHp > 0)
				{
					mech.Heal(maxHp * 10, Message: false, FloatText: false);
				}
			}
			if (mech.TryGetPart<EnergyCellSocket>(out EnergyCellSocket socket) && socket.Cell != null)
			{
				IEnergyCell cell = socket.Cell.GetPartDescendedFrom<IEnergyCell>();
				cell?.SetChargePercentage(50);
			}
		}

		[Serializable]
		public class BRDMechCalldownCleanup : IPart
		{
			public GameObject Deployer;

			[NonSerialized]
			private bool completed;

			[NonSerialized]
			private int turnsUntilFallback = 2;

			public override bool AllowStaticRegistration()
			{
				return true;
			}

			public override bool WantEvent(int ID, int cascade)
			{
				return ID == PooledEvent<InteriorZoneBuiltEvent>.ID || ID == EndTurnEvent.ID;
			}

			public override bool HandleEvent(InteriorZoneBuiltEvent E)
			{
				TryComplete();
				return base.HandleEvent(E);
			}

			public override bool HandleEvent(EndTurnEvent E)
			{
				if (!completed && --turnsUntilFallback <= 0)
				{
					TryComplete();
				}
				return base.HandleEvent(E);
			}

			private void TryComplete()
			{
				if (completed)
				{
					return;
				}
				CyberneticsBRDSatUplink.ApplyMechDropVehicleState(ParentObject, Deployer);
				completed = true;
				if (ParentObject != null)
				{
					ParentObject.RemovePart(this);
				}
			}
		}

	}
}
