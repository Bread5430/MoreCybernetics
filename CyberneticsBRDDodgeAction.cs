using System;
using XRL.Rules;
using XRL.World;
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
			if (E.ID == "DefenderAfterAttackMissed" && ParentObject.Implantee != null)
			{
				IComponent<GameObject>.AddPlayerMessage("debug: DefenderAfterAttackMissed event fired", 'g');

				GameObject defender = E.GetGameObjectParameter("Defender");
				if (defender == ParentObject.Implantee)
				{
					IComponent<GameObject>.AddPlayerMessage("debug: Defender is the implantee", 'g');
					GameObject host = ParentObject.Implantee;
					if (host.IsPlayer())
					{
						IComponent<GameObject>.AddPlayerMessage("You slip the blow; your dodge implant sparks to life.", 'g');
					}
					if (host.TryGetEffect<BRD_DodgeBoost>(out BRD_DodgeBoost effect))
					{
						effect.Refresh(10, 100, 9, ParentObject);
						if (host.IsPlayer())
						{
							IComponent<GameObject>.AddPlayerMessage("Hyper-response surges through you again.", 'g');
						}
					}
					else
					{
						host.ApplyEffect(new BRD_DodgeBoost(10, 100, 9, ParentObject));
						if (host.IsPlayer())
						{
							IComponent<GameObject>.AddPlayerMessage("Neural timing threads into your muscles.", 'g');
						}
					}
				}
			}
			return base.FireEvent(E);
		}
	}
}
