using System;
using XRL.Rules;
using XRL.World.Effects;


namespace XRL.World.Parts
{
    [Serializable]
    public class CyberneticsBRDLoadBalancer : IPart
	{

        public override bool WantEvent(int ID, int cascade)
		{
			if (base.WantEvent(ID, cascade)
				|| ID == GetMaxCarriedWeightEvent.ID)
                //|| ID == GetItemElementsEvent.ID)
			{
				return true;
			}
			return false;
		}

        /*public override bool HandleEvent(GetItemElementsEvent E)
        {
            if (E.IsRelevantCreature(ParentObject))
            {
                E.Add("travel", BaseElementWeight);
            }
            return base.HandleEvent(E);
        }*/

        public override bool HandleEvent(GetMaxCarriedWeightEvent E)
        {
            E.AdjustWeight(1.25);
            return base.HandleEvent(E);
        }

    }
}

