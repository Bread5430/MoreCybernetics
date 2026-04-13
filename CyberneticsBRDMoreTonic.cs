using System;
using System.Collections.Generic;
using XRL.Rules;
using XRL.World.Effects;

namespace XRL.World.Parts{

	[Serializable]
	public class CyberneticsBRDMoreTonic : IPart
	{

		public override bool WantEvent(int ID, int cascade)
        {
            if (base.WantEvent(ID, cascade)
                || ID == SingletonEvent<GetTonicCapacityEvent>.ID
                || ID == ImplantedEvent.ID
                || ID == UnimplantedEvent.ID)
            {
                return true;
            }
            return false;
            
        }

		
        public override bool HandleEvent(ImplantedEvent E)
        {
            return base.HandleEvent(E);
        }

        public override bool HandleEvent(UnimplantedEvent E)
        {
            return base.HandleEvent(E);
        }


		public override bool HandleEvent(GetTonicCapacityEvent E)
		{
			E.Capacity++;
			return base.HandleEvent(E);
		}
		public override bool SameAs(IPart p)
		{
			return false;
		}
	}
}