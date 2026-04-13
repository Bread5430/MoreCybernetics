using System;
using XRL.Rules;
using XRL.World.Effects;

namespace XRL.World.Parts
{
	[Serializable]
	public class CyberneticsBRDDodgeAction : IPart
	{
		public override bool AllowStaticRegistration()
		{
			return true;
		}

		public override void Register(GameObject Object, IEventRegistrar Registrar)
		{
			Registrar.Register("DefenderAfterAttackMissed");
			base.Register(Object, Registrar);
		}

		public override bool FireEvent(Event E)
		{
			if (E.ID == "DefenderAfterAttackMissed"
				&& ParentObject.Implantee != null
				&& !ParentObject.Implantee.TryGetEffect<BRD_DodgeBoost>(out _))
			{
				GameObject defender = E.GetGameObjectParameter("Defender");
				if (defender == ParentObject.Implantee)
				{
					ParentObject.Implantee.ApplyEffect(new BRD_DodgeBoost(11, 100, 10, ParentObject));
				}
			}
			return base.FireEvent(E);
		}
	}
}
