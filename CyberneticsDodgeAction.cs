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

        public override void Register(GameObject Object, IEventRegistrar Registrar)
        {
            Registrar.Register("DefenderAfterAttackMissed");
            base.Register(Object, Registrar);
        }



    // Create a new status effect to show the free action, refernce inflated axons
	public override bool FireEvent(Event E)

    
    
	{
		if (E.ID == "DefenderAfterAttackMissed")
		{
            GameObject gameObjectParameter = E.GetGameObjectParameter("Defender");
			if (ParentObject.Implantee != null && gameObjectParameter == ParentObject.Implantee)
            {
                ParentObject.ApplyEffect(new AxonsInflated(1, 50, ParentObject));
            }
		}
		return base.FireEvent(E);
	}

    }
}

