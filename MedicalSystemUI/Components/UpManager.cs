using System;
using System.Linq;
using Alosha;
using MedicalSystemUI.Models;
using Rocket.Unturned.Player;
using SDG.Unturned;
using Steamworks;
using UnityEngine;

namespace MedicalSystemUI.Components
{
	public class UpManager : MonoBehaviour
	{
		public void Connect(UnturnedPlayer player, UnturnedPlayer doctor, Wounded wounded, bool active, float canlandirmaSuresi)
		{
			this.player = player;
			this.doctor = doctor;
			this.wounded = wounded;
			this.active = active;
			this.startTime = DateTime.Now;
			this.canlandirilacakSure = canlandirmaSuresi;
		}

		private void FixedUpdate()
		{
			if (!this.active)
			{
				return;
			}
			if (this.player == null || this.doctor == null || !this.wounded.improve || !Main.instance.woundeds.Any((Wounded e) => e.Id == this.player.CSteamID))
			{
				UnityEngine.Object.Destroy(this);
			}
			if (Vector3.Distance(this.player.Position, this.doctor.Position) >= Main.instance.Configuration.Instance.neKadarMesafesiOlsun)
			{
				UnityEngine.Object.Destroy(this);
			}
			if (this.doctor.Player.animator.gesture != EPlayerGesture.SURRENDER_START)
			{
				UnityEngine.Object.Destroy(this);
			}
			if ((DateTime.Now - this.startTime).TotalSeconds >= (double)this.canlandirilacakSure)
			{
				Main.instance.woundeds.RemoveAll((Wounded e) => e.Id == this.wounded.Id);
				Main.instance.Kaldır(this.doctor, this.player);
				UnityEngine.Object.Destroy(this);
			}
			double num = Math.Floor((double)this.canlandirilacakSure - (DateTime.Now - this.startTime).TotalSeconds);
			EffectManager.sendUIEffect(51001, 995, this.player.CSteamID, true, " CANLANDIRILIYORSUN ", string.Format("Canlanmana: {0} saniye kaldı", num));
			EffectManager.sendUIEffect(51001, 995, this.doctor.CSteamID, true, " CANLANDIRIYORSUN ", string.Format("Canlanmasına: {0} saniye kaldı", num));
		}

		private void OnDestroy()
		{
			if (Main.instance.woundeds.Any((Wounded e) => e.Id == this.wounded.Id))
			{
				this.wounded.improve = false;
				this.wounded.improveDoctor = CSteamID.Nil;
			}
			if (this.player != null)
			{
				Main.instance.Temizle(this.player);
			}
			if (this.doctor != null)
			{
				Main.instance.Temizle(this.doctor);
			}
		}

		private UnturnedPlayer player;

		private UnturnedPlayer doctor;

		private Wounded wounded;

		private DateTime startTime;

		private float canlandirilacakSure;
		private bool active;
	}
}
