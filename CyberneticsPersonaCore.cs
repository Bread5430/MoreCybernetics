using System;
using System.Collections.Generic;
using XRL.Rules;
using XRL.World.Effects;

namespace XRL.World.Parts{

	[Serializable]
	public class CyberneticsPersonaCore : IPart
	{
		public override bool SameAs(IPart p)
		{
			return false;
		}

		public override bool WantEvent(int ID, int cascade)
        {
            if (base.WantEvent(ID, cascade)
                || ID == SingletonEvent<GetAvailableComputePowerEvent>.ID)
            {
                return true;
            }
            return false;
            
        }


		public override bool HandleEvent(GetAvailableComputePowerEvent E)
		{
			if (E.Actor == ParentObject.Implantee)
			{
				E.Amount += Math.Max(ParentObject.Implantee.StatMod("Ego") * 10, 0);
			}
			return base.HandleEvent(E);
		}

		public override bool AllowStaticRegistration()
		{
			return true;
		}
	}
}