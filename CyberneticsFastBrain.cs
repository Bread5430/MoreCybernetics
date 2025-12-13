using System;
using XRL.Rules;
using XRL.World.Effects;


namespace XRL.World.Parts
{
    [Serializable]
    public class CyberneticsFastBrain : IPart
	{
        public override bool WantEvent(int ID, int cascade)
        {
            if (!base.WantEvent(ID, cascade) && ID != ApplyEffectEvent.ID)
            {
                return ID == SingletonEvent<GetEnergyCostEvent>.ID;
            }
            return true;
        }

        public override bool HandleEvent(GetEnergyCostEvent E)
        {
            if (E.Type != null)
            {
                E.PercentageReduction += 15;
            }
            return base.HandleEvent(E);
        }
    }
}

