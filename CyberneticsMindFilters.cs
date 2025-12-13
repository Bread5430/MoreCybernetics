using System;

namespace XRL.World.Parts
{
    [Serializable]
    public class CyberneticsMindFilter : IPart
    {
        public override bool SameAs(IPart p)
        {
            return false;
        }

        public override bool AllowStaticRegistration()
        {
            return false;
        }

        public override bool WantEvent(int ID, int cascade)
        {
            if (!base.WantEvent(ID, cascade) && ID != ImplantedEvent.ID)
            {
                return ID == UnimplantedEvent.ID;
            }
            return true;
        }

        public override bool HandleEvent(ImplantedEvent E)
        {
            E.Implantee.RegisterPartEvent(this, "CanApplyConfused");
            E.Implantee.RegisterPartEvent(this, "ApplyConfused");
            return base.HandleEvent(E);
        }

        public override bool HandleEvent(UnimplantedEvent E)
        {
            E.Implantee.UnregisterPartEvent(this, "CanApplyConfused");
            E.Implantee.UnregisterPartEvent(this, "ApplyConfused");
            return base.HandleEvent(E);
        }

        public override bool FireEvent(Event E)
        {
            if (E.ID == "CanApplyConfused" || E.ID == "ApplyConfused")
            {
                return false;
            }
            return base.FireEvent(E);
        }
    }
}
