using System;

namespace XRL.World.Parts
{
	/// <summary>
	/// While implanted, the host gains +20 MoveSpeed per active tonic effect on the implantee.
	/// </summary>
	[Serializable]
	public class CyberneticsBRDTonicBuff : IPart
	{
		public override bool SameAs(IPart p)
		{
			return false;
		}

		public override bool WantTurnTick()
		{
			return true;
		}

		public override bool WantEvent(int ID, int cascade)
		{
			if (!base.WantEvent(ID, cascade)
				&& ID != ImplantedEvent.ID
				&& ID != UnimplantedEvent.ID)
			{
				return false;
			}
			return true;
		}

		public override bool HandleEvent(ImplantedEvent E)
		{
			StatShifter.DefaultDisplayName = "tonic motility";
			SyncTonicMoveSpeed();
			return base.HandleEvent(E);
		}

		public override bool HandleEvent(UnimplantedEvent E)
		{
			if (GameObject.Validate(ref E.Implantee))
			{
				StatShifter.RemoveStatShifts(E.Implantee);
			}
			return base.HandleEvent(E);
		}

		public override void TurnTick(long TimeTick, int Amount)
		{
			SyncTonicMoveSpeed();
		}

		void SyncTonicMoveSpeed()
		{
			GameObject host = ParentObject?.Implantee;
			if (!GameObject.Validate(ref host) || !host.HasStat("MoveSpeed"))
			{
				return;
			}
			int bonus = host.GetTonicEffectCount() * 20;
			StatShifter.SetStatShift(host, "MoveSpeed", -bonus);
		}
	}
}
