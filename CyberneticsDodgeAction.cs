using System;
using XRL.Rules;
using XRL.World.Effects;


namespace XRL.World.Parts
{
    [Serializable]
    public class CyberneticsFastBrain : IPart
	{

        public override bool AllowStaticRegistration()
        {
            return true;
        }

        public override bool WantEvent(int ID, int cascade)
        {
            if (base.WantEvent(ID, cascade)
                || ID == BeginTakeAction.ID
                || ID == DefenderAfterAttackMissed.ID)
            {
                return true;
            }
            return false;

        }


        // Create a new status effect to show the free action, refernce inflated axons
        public override bool HandleEvent(DefenderAfterAttackMissed E)
        {
            if (E.Actor != null && E.Actor == ParentObject.Implantee)
            {
                E.Actor.ApplyEffect(new AxonsInflated(1, 50, ParentObject));
            }
            return base.HandleEvent(E);
        }
    }
}

