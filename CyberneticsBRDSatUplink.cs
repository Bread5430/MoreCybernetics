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
		public string commandId = "ActivateComLink";


		List<string> OptionStrings = new List<string>
		{
			"Mech Drop", // Steel's Perfection
			"Turret Drop", // Boundless Riches
			"Anti-Reality Strike" // Sultan's Gaze
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
						delayedActionIndicator("turret");
						break;
					case 3:
						delayedActionIndicator("strike");
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
				return;
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
						if (60.in100())
						{
							MoveToRandomIn(Z, item);
						}
						if (10.in100())
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
				Physics.ApplyExplosion(ParentObject.CurrentCell, 30000, Hit: hit, Local: false, Owner: owner); //, Phase: phase);
			}

			public void turret_drop()
			{
				// Scatter pattern: DeploymentGrenade.DoDetonate (e.g. Spring Grenade) rolls Count and
				// deploys to random cells from GetLocalAdjacentCells(Radius).
				// Turrets: TurretTinker.CommandTinkerTurret / IntegratedWeaponHosts.GenerateTurret with a
				// DynamicInheritsTable missile weapon (GetRandomTurretWeaponBlueprint).
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
				int tier = deployer.GetTier();
				int radius = 2;
				int count = "4-6".RollCached();
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
					string weaponBlueprint = GetRandomTurretMissileWeaponBlueprint(tier);
					GameObject turret = IntegratedWeaponHosts.GenerateTurret(GameObject.Create(weaponBlueprint), deployer, overrideSupply: true);
					cell.AddObject(turret);
					turret.MakeActive();
					turret.FireEventOnBodyparts(Event.New("GenerateIntegratedHostInitialAmmo", "Host", turret));
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

			private static string GetRandomTurretMissileWeaponBlueprint(int tier)
			{
				string populationName = "DynamicInheritsTable:MissileWeapon:Tier" + tier;
				int attempts = 0;
				do
				{
					PopulationResult populationResult = PopulationManager.RollOneFrom(populationName);
					if (populationResult == null)
					{
						return "Pump Shotgun";
					}
					if (++attempts > 10)
					{
						return "Pump Shotgun";
					}
					GameObjectBlueprint blueprint = GameObjectFactory.Factory.GetBlueprint(populationResult.Blueprint);
					if (blueprint != null && blueprint.GetPartParameter("MissileWeapon", "FiresManually", Default: true))
					{
						return populationResult.Blueprint;
					}
				}
				while (true);
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
			if (!mech.TryGetPart<Vehicle>(out Vehicle veh))
			{
				return;
			}
			GameObject pilot = veh.Pilot;
			if (pilot != null && GameObject.Validate(ref pilot) && pilot != deployer && !pilot.IsPlayer())
			{
				pilot.Die(deployer);
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