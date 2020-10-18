using System;
using Steamworks;

namespace MedicalSystemUI.Models
{
	public class Wounded
	{
		public Wounded()
		{
		}

		public Wounded(CSteamID id, bool drifting, DateTime lastTime)
		{
			this.Id = id;
			this.drifting = drifting;
			this.improve = false;
			this.drifting = false;
			this.driftingMember = CSteamID.Nil;
			this.improveDoctor = CSteamID.Nil;
			this.lastTime = lastTime;
		}

		public CSteamID Id;

		public bool drifting;

		public CSteamID driftingMember;

		public bool improve;

		public CSteamID improveDoctor;

		public DateTime lastTime;
	}
}
