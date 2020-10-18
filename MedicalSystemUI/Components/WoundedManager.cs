using System;
using System.Linq;
using Alosha;
using MedicalSystemUI.Models;
using Rocket.Unturned.Player;
using SDG.Unturned;
using UnityEngine;

namespace MedicalSystemUI.Components
{
	public class WoundedManager : MonoBehaviour
	{
		public void Connect(UnturnedPlayer player, Wounded wounded, bool active)
		{
			this.player = player;
			this.wounded = wounded;
			this.active = active;
		}

		private void FixedUpdate()
		{
			if (!this.active)
			{
				return;
			}
			if (this.player == null || !Main.instance.woundeds.Any((Wounded e) => e.Id == this.player.CSteamID))
			{
				UnityEngine.Object.Destroy(this);
			}
			if (this.player.Stance != EPlayerStance.PRONE)
			{
				this.player.Player.stance.channel.send("tellStance", ESteamCall.ALL, ESteamPacket.UPDATE_UNRELIABLE_BUFFER, new object[]
				{
					(byte)5
				});
			}
			if (this.wounded.improve)
			{
				return;
			}
			if ((DateTime.Now - this.wounded.lastTime).TotalSeconds >= (double)Main.instance.Configuration.Instance.ölümSüresi)
			{
				Main.instance.woundeds.RemoveAll((Wounded e) => e.Id == this.wounded.Id);
				Main.instance.Öldür(this.player.CSteamID);
				this.player.Player.movement.sendPluginSpeedMultiplier(1f);
				UnityEngine.Object.Destroy(this);
				return;
			}
			if (!this.wounded.drifting)
			{
				EffectManager.sendUIEffect(51001, 995, this.player.CSteamID, true, "YARALISIN", "Yeniden Doğmana: " + Math.Floor((double)Main.instance.Configuration.Instance.ölümSüresi - (DateTime.Now - this.wounded.lastTime).TotalSeconds).ToString() + " saniye kaldı");
			}
		}

		private void OnDestroy()
		{
			Main.instance.woundeds.RemoveAll((Wounded e) => e.Id == this.wounded.Id);
			if (this.player != null)
			{
				Main.instance.Temizle(this.player);
			}
		}

		private UnturnedPlayer player;

		private Wounded wounded;

		private bool active;
	}
}
