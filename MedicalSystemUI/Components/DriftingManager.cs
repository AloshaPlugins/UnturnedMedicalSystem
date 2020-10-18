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
	public class DriftingManager : MonoBehaviour
	{
		public void Connect(UnturnedPlayer player, UnturnedPlayer drifter, Wounded wounded, bool active)
		{
			this.player = player;
			this.drifter = drifter;
			this.wounded = wounded;
			this.active = active;
		}

		private void FixedUpdate()
		{
			if (!this.active)
			{
				return;
			}
			if (this.player == null || this.drifter == null || !this.wounded.drifting || !Main.instance.woundeds.Any((Wounded e) => e.Id == this.player.CSteamID))
			{
				UnityEngine.Object.Destroy(this);
			}
			if (this.drifter.Player.animator.gesture == EPlayerGesture.SURRENDER_START && this.drifter.Stance == EPlayerStance.CROUCH)
			{
				if (Vector3.Distance(this.player.Position, this.drifter.Position) >= 1.5f)
				{
					this.player.Teleport(this.drifter.Position, this.player.Rotation);
				}
				EffectManager.sendUIEffect(51001, 995, this.player.CSteamID, true, "SÜRÜKLENİYORSUN", this.drifter.DisplayName ?? "");
				EffectManager.sendUIEffect(51001, 995, this.drifter.CSteamID, true, "SÜRÜKLÜYORSUN", this.player.DisplayName ?? "");
				return;
			}
			UnityEngine.Object.Destroy(this);
		}

		private void OnDestroy()
		{
			if (Main.instance.woundeds.Any((Wounded e) => e.Id == this.wounded.Id))
			{
				this.wounded.drifting = false;
				this.wounded.driftingMember = CSteamID.Nil;
			}
			if (this.player != null)
			{
				Main.instance.Temizle(this.player);
			}
			if (this.drifter != null)
			{
				Main.instance.Temizle(this.drifter);
			}
		}

		private UnturnedPlayer player;

		private UnturnedPlayer drifter;

		private Wounded wounded;

		private bool active;
	}
}
