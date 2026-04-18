using System;
using XRL.Rules;
using XRL.World;

namespace XRL.World.Effects
{

	[Serializable]
	public class BRD_DodgeBoost : Effect, ITierInitialized
	{
		public int Bonus;

		public string Source;

		public bool isonCoolDown = false;

		public int Cooldown;

		public BRD_DodgeBoost()
		{
			DisplayName = "{{g|hyper-responsive}}";
		}

		public BRD_DodgeBoost(int Duration, int Bonus, int Cooldown, GameObject Source)
			: this()
		{
			base.Duration = Duration;
			this.Bonus = Bonus;
			this.Cooldown = Cooldown;
			this.Source = Source.ID;
			this.isonCoolDown = false;
		}

		public void Initialize(int Tier)
		{
			if (Duration <= 0)
			{
				Duration = 11;
			}
			if (Bonus == 0)
			{
				Bonus = 100;
			}
			if (Cooldown <= 0)
			{
				Cooldown = 10;
			}
			isonCoolDown = false;
		}

		public override bool SameAs(Effect e)
		{
			return false;
		}

		public override string GetDetails()
		{
			if (isonCoolDown)
			{
				return "Cooling Down: (" + Cooldown + ")";
			}
			return Bonus.Signed() + " Quickness";
		}

		public override bool Apply(GameObject Object)
		{
			if (!Object.FireEvent(Event.New("ApplyBRD_DodgeBoost", "Effect", this)))
			{
				return false;
			}
			if (Object.IsPlayer())
			{
				IComponent<GameObject>.AddPlayerMessage("A moment is fractured.", 'g');
			}
			base.StatShifter.SetStatShift("Speed", Bonus);
			Object?.PlayWorldSound("Sounds/StatusEffects/sfx_statusEffect_positiveVitality");
			return true;
		}

		public override void Remove(GameObject Object)
		{
			ClearSpeedStatShifts();
			base.Remove(Object);
		}

		public void Refresh(int Duration, int Bonus, int Cooldown, GameObject Source)
		{
			this.Duration = Duration;
			this.Bonus = Bonus;
			this.Cooldown = Cooldown;
			this.Source = Source?.ID;
			isonCoolDown = false;
			base.StatShifter.SetStatShift("Speed", this.Bonus);
		}

		void ClearSpeedStatShifts()
		{
			base.StatShifter.RemoveStatShifts();
		}

		public override bool WantEvent(int ID, int cascade)
		{
			if (!base.WantEvent(ID, cascade))
			{
				return ID == SingletonEvent<BeginTakeActionEvent>.ID;
			}
			return true;
		}

		public override bool HandleEvent(BeginTakeActionEvent E)
		{
			Cell cell = base.Object?.CurrentCell;
			if (cell == null || cell.OnWorldMap())
			{
				Duration = 0;
			}
			else
			{
				if (!string.IsNullOrEmpty(Source))
				{
					GameObject sourceImplant = GameObject.FindByID(Source);
					if (!GameObject.Validate(ref sourceImplant) || sourceImplant.Implantee != base.Object)
					{
						Duration = 0;
					}
				}
				if (Duration > 0 && Duration != 9999)
				{
					if (!isonCoolDown && Duration <= Cooldown)
					{
						isonCoolDown = true;
						ClearSpeedStatShifts();
					}
					Duration--;
				}
			}
			return base.HandleEvent(E);
		}
	}
}
