using System;
using XRL.Rules;
using XRL.World.Effects;


namespace XRL.World.Parts
{
    [Serializable]
    public class CyberneticsAutoLoaders : IPart
	{
        public override bool WantEvent(int ID, int cascade)
        {
            if (base.WantEvent(ID, cascade)
                || ID == SingletonEvent<GetEnergyCostEvent>.ID
                || ID == SingletonEvent<CommandReloadEvent>.ID)
            {
                return true;
            }
            return false;
        }

        public override bool HandleEvent(CommandReloadEvent E)
        {
            E.FreeAction = true;
            return base.HandleEvent(E);
        }

        public override bool HandleEvent(GetEnergyCostEvent E)
        {
            // Allow battery reloading to be free as well
            if (E.Type != null && E.Type.Contains("Ammo Magazine Transfer") )
            {
                E.PercentageReduction += 100;
            }
            return base.HandleEvent(E);
        }
    }
}




