using System;
using XRL.Rules;
using XRL.World;
using XRL.World.Effects;

namespace XRL.World.Parts
{
	[Serializable]
	public class CyberneticsBRDDodgeAction : IPart
	{
		const string DefenderAfterAttackMissedId = "DefenderAfterAttackMissed";
		public override bool SameAs(IPart p)
		{
			return false;
		}

		public override bool AllowStaticRegistration()
		{
			return true;
		}

		public override bool WantEvent(int ID, int cascade)
		{
			if (!base.WantEvent(ID, cascade) && ID != ImplantedEvent.ID && ID != UnimplantedEvent.ID)
			{
				return false;
			}
			return true;
		}


		public override bool HandleEvent(ImplantedEvent E)
		{
			if (GameObject.Validate(ref E.Implantee))
			{
				E.Implantee.RegisterPartEvent(this, DefenderAfterAttackMissedId);
			}
			return base.HandleEvent(E);
		}

		public override bool HandleEvent(UnimplantedEvent E)
		{
			GameObject implantee = E.Implantee;
			if (GameObject.Validate(ref implantee))
			{
				implantee.UnregisterPartEvent(this, DefenderAfterAttackMissedId);
			}
			return base.HandleEvent(E);
		}



		public override bool FireEvent(Event E)
		{
			if (E.ID != DefenderAfterAttackMissedId)
			{
				return base.FireEvent(E);
			}
			GameObject host = ParentObject.Implantee;
			if (!GameObject.Validate(ref host))
			{
				return base.FireEvent(E);
			}
			GameObject defender = E.GetGameObjectParameter("Defender");
			if (defender != host)
			{
				return base.FireEvent(E);
			}
			if (host.TryGetEffect<BRD_DodgeBoost>(out BRD_DodgeBoost effect))
			{
				//effect.Refresh(10, 100, 9, ParentObject);
				if (host.IsPlayer())
				{
					IComponent<GameObject>.AddPlayerMessage("You evade the strike; hyper-response surges anew.", 'g');
				}
			}
			else
			{
				host.ApplyEffect(new BRD_DodgeBoost(10, 100, 9, ParentObject));
				if (host.IsPlayer())
				{
					IComponent<GameObject>.AddPlayerMessage("You evade the strike—borrowed speed floods your limbs.", 'g');
				}
			}
			return base.FireEvent(E);
		}
	}
}
