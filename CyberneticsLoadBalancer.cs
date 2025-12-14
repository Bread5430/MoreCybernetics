using System;
using XRL.Rules;
using XRL.World.Effects;


namespace XRL.World.Parts
{
    [Serializable]
    public class CyberneticsLoadBalancer : IPart
	{
        public override bool HandleEvent(GetMaxCarriedWeightEvent E)
        {
            E.AdjustWeight(1.25);
            return base.HandleEvent(E);
        }

    }
}

