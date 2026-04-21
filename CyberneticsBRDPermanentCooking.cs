using System;
using XRL.World.Effects;
using XRL.World.Parts;

namespace XRL.World.Parts
{
	[Serializable]
	public class CyberneticsBRDPermanentCooking : IPart
	{
		public static readonly string COMMAND_NAME = "CommandBRDMetabolicAnchor";

		public Guid ActivatedAbilityID = Guid.Empty;

		public override bool SameAs(IPart p)
		{
			return false;
		}

		public override bool WantEvent(int ID, int cascade)
		{
			if (!base.WantEvent(ID, cascade) && ID != PooledEvent<CommandEvent>.ID && ID != ImplantedEvent.ID)
			{
				return ID == UnimplantedEvent.ID;
			}
			return true;
		}

		public override bool HandleEvent(ImplantedEvent E)
		{
			ActivatedAbilityID = E.Implantee.AddActivatedAbility(
				"Metabolic Anchor",
				COMMAND_NAME,
				"Cybernetics",
				"While active, your current meal effect is integrated and no longer expires from hunger or eating. Activating makes you hungry. Toggle off to release integrated effects.",
				"\a",
				null,
				Toggleable: true,
				DefaultToggleState: false,
				ActiveToggle: false,
				IsAttack: false,
				IsRealityDistortionBased: false,
				IsWorldMapUsable: true);
			return base.HandleEvent(E);
		}

		public override bool HandleEvent(UnimplantedEvent E)
		{
			E.Implantee.FireEvent(BRD_StickyCookingEffect.RemoveStickyEventId);
			E.Implantee.RemoveActivatedAbility(ref ActivatedAbilityID);
			return base.HandleEvent(E);
		}

		public override bool HandleEvent(CommandEvent E)
		{
			if (E.Command == COMMAND_NAME && E.Actor != null && E.Actor == ParentObject.Implantee)
			{
				GameObject implantee = ParentObject.Implantee;
				implantee.ToggleActivatedAbility(ActivatedAbilityID);
				ActivatedAbilityEntry ability = implantee.GetActivatedAbility(ActivatedAbilityID);
				if (ability != null && ability.ToggleState)
				{
					if (!BRD_StickyCookingEffect.TryStickifyFirstVanilla(implantee))
					{
						IComponent<GameObject>.AddPlayerMessage("You have no metabolizing effect to anchor.");
					}
					SetHungry(implantee);
				}
				else
				{
					implantee.FireEvent(BRD_StickyCookingEffect.RemoveStickyEventId);
				}
				ParentObject.ModIntProperty("ActivatedAbilityCommandsProcessed", 1);
			}
			return base.HandleEvent(E);
		}

		private static void SetHungry(GameObject go)
		{
			Stomach stomach = go.GetPart<Stomach>();
			if (stomach == null)
			{
				return;
			}
			stomach.CookingCounter = stomach.CalculateCookingIncrement();
			stomach.HungerLevel = 1;
			go.RemoveEffect<Famished>();
			go.FireEvent("BecameHungry");
		}

		public override bool AllowStaticRegistration()
		{
			return true;
		}
	}
}
