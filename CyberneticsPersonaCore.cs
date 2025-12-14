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
			if (!base.WantEvent(ID, cascade) && ID != SingletonEvent<GetAvailableComputePowerEvent>.ID && ID != ImplantedEvent.ID)
			{
				return ID == UnimplantedEvent.ID;
			}
			return true;
		}


		public override bool HandleEvent(GetAvailableComputePowerEvent E)
		{
			if (E.Actor == ParentObject.Implantee)
			{
				Dictionary<string, Statistic> statistics = ParentObject.Implantee.Statistics;
				Statistic value = null;
				if (statistics != null && statistics.TryGetValue("Ego", out value))
				{
					E.Amount += value.Modifier * 10;
				}
			}
			return base.HandleEvent(E);
		}

		public override bool AllowStaticRegistration()
		{
			return true;
		}
	}
}