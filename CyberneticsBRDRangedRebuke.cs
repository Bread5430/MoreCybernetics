using System;
using System.Collections.Generic;
using System.Linq;
using XRL.World;
using XRL.Rules;
using XRL.UI;
using XRL.World.AI;
using XRL.World.Effects;
using XRL.World.Parts.Skill;

namespace XRL.World.Parts
{
    [Serializable]
    public class CyberneticsBRDRangedRebuke: IPart
	{
		public string commandId = "CommandBRDRebukeRobot";
		public Guid ActivatedAbilityID = Guid.Empty;
		public int Bonus = 5;

		public void CollectStats(Templates.StatCollector stats)
		{
			stats.Set("Cooldown", GetAvailableComputePowerEvent.AdjustDown(ParentObject, 100));
		}

		public override bool WantEvent(int ID, int cascade)
		{
			// CommandEvent is pooled; without this, the Body→cybernetics cascade never asks this part,
			// so HandleEvent(CommandEvent) and FireEvent for the command string never run (see CommandEvent.Send).
			if (!base.WantEvent(ID, cascade)
				&& ID != SingletonEvent<GetRebukeLevelEvent>.ID
				&& ID != PooledEvent<GetCompanionLimitEvent>.ID
				&& ID != PooledEvent<GetItemElementsEvent>.ID
				&& ID != GetShortDescriptionEvent.ID
				&& ID != ImplantedEvent.ID
				&& ID != UnimplantedEvent.ID
				&& ID != PooledEvent<CommandEvent>.ID)
			{
				return false;
			}
			return true;
		}

		public override bool HandleEvent(GetItemElementsEvent E)
		{
			if (E.IsRelevantCreature(ParentObject))
			{
				E.Add("jewels", 3);
			}
			return base.HandleEvent(E);
		}

		public override bool HandleEvent(GetCompanionLimitEvent E)
		{
			if (E.Means == "Rebuke" && E.Actor == ParentObject && ActivatedAbilityID != Guid.Empty)
			{
				E.Limit++;
			}
			return base.HandleEvent(E);
		}

		public override bool AllowStaticRegistration()
		{
			return true;
		}

		public override void Register(GameObject Object, IEventRegistrar Registrar)
		{
			Registrar.Register("CanCompanionRestorePartyLeader");
			base.Register(Object, Registrar);
		}


        public override bool HandleEvent(CommandEvent E)
        {
            if (E.Command == commandId && E.Actor == ParentObject.Implantee)
            {
                //IComponent<GameObject>.AddPlayerMessage("debug: CommandBRDRebukeRobot event fired", 'g');
                if (!AttemptRebuke())
                {
                    //IComponent<GameObject>.AddPlayerMessage("debug: AttemptRebuke failed", 'r');
                    return false;
                }
            }
            return base.HandleEvent(E);
        }

		public override bool HandleEvent(UnimplantedEvent E)
		{
			E.Implantee.RemoveActivatedAbility(ref ActivatedAbilityID);
			return base.HandleEvent(E);
		}


		public override bool HandleEvent(ImplantedEvent E)
		{
			ActivatedAbilityID = E.Implantee.AddActivatedAbility("Tele-rebuke", commandId, "Cybernetics", "Rebuke a robot from range. Level + Ego-based difficulty check.", "\u0003");
			return base.HandleEvent(E);
		}

		public bool AttemptRebuke()
		{
			GameObject actor = ParentObject.Implantee;
			if (actor == null)
			{
				return false;
			}
			// PickDestinationCell on IPart uses ParentObject (the implant) as basis; implants are not
			// IsSelfControlledPlayer, so the picker never opens and returns null. Use the implantee's Physics.
			Cell cell = actor.Physics.PickDestinationCell(5, AllowVis.OnlyVisible, Locked: true, IgnoreSolid: false, IgnoreLOS: true, RequireCombat: true, PickTarget.PickStyle.EmptyCell, "Rebuke what robot?", Snap: true);
			if (cell == null)
			{
				return false;
			}
			//IComponent<GameObject>.AddPlayerMessage("debug: cell found", 'g');

			bool flag = false;
			foreach (GameObject item in cell.GetObjectsWithPart("Brain"))
			{
				if (item != actor && item.Statistics.ContainsKey("Level") && item.HasPart<Robot>())
				{
					if (!item.CheckInfluence(By: actor, Type: base.Name))
					{
						return false;
					}
					flag = true;
					int num = item.Stat("Level") * 4 / 5;
					if (item.HasEffect<Proselytized>())
					{
						num++;
					}
					if (item.HasEffect<Rebuked>())
					{
						num++;
					}
					if (item.TryGetEffect<Beguiled>(out var Effect))
					{
						num += Effect.LevelApplied;
					}
					int num2 = GetRebukeLevelEvent.GetFor(actor, item);
					num2 = actor.StatMod("Ego") + num2 * 4 / 5;
					if (Options.SifrahRecruitment)
					{
						new RebukingSifrah(item, num2, num).Play(item);
					}
					else
					{
						PerformMentalAttack(Rebuke, actor, item, null, "Rebuke Robot", null, 2, int.MinValue, int.MinValue, num2, num);
					}
					actor.UseEnergy(1000, "Skill Tele-rebuke");
					CooldownMyActivatedAbility(ActivatedAbilityID, GetAvailableComputePowerEvent.AdjustDown(actor, 100), actor);
				}
			}
			if (!flag && actor.IsPlayer())
			{
				Popup.Show("There is nothing there to rebuke.");
			}
			return true;
		}

		public bool Rebuke(MentalAttackEvent E)
		{
			return E?.Attacker?.GetPart<Persuasion_RebukeRobot>()?.Rebuke(E) ?? false;
		}

		public bool FinalizeRebuke(GameObject Actor, GameObject Robot)
		{
			return Actor?.GetPart<Persuasion_RebukeRobot>()?.FinalizeRebuke(Actor, Robot) ?? false;
		}

		public static bool Rebuke(GameObject Actor, GameObject Robot)
		{
			return Actor.GetPart<Persuasion_RebukeRobot>()?.FinalizeRebuke(Actor, Robot) ?? false;
		}

		public static void SyncTarget(GameObject Rebuker, GameObject Target = null, bool Independent = false)
		{
			Persuasion_RebukeRobot.SyncTarget(Rebuker, Target, Independent);
		}

		public static void Neutralize(GameObject Actor, GameObject Object)
		{
			Persuasion_RebukeRobot.Neutralize(Actor, Object);
		}

		public void Neutralize(GameObject Object)
		{
			GameObject implantee = ParentObject.Implantee;
			if (implantee != null)
			{
				Neutralize(implantee, Object);
			}
		}

		public override bool HandleEvent(GetRebukeLevelEvent E)
		{
			E.Level += GetAvailableComputePowerEvent.AdjustUp(E.Actor, Bonus);
			return base.HandleEvent(E);
		}

		public override bool HandleEvent(GetShortDescriptionEvent E)
		{
			E.Postfix.AppendRules("Compute power on the local lattice increases this item's effectiveness.");
			return base.HandleEvent(E);
		}
	}
}

