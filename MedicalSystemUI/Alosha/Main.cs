using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MedicalSystemUI.Components;
using MedicalSystemUI.Models;
using Rocket.API;
using Rocket.API.Serialisation;
using Rocket.Core.Plugins;
using Rocket.Unturned;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Events;
using Rocket.Unturned.Player;
using SDG.Unturned;
using Steamworks;
using UnityEngine;

namespace Alosha
{
	public class Main : RocketPlugin<ConfigAlosha>
	{
		protected override void Load()
		{
			Main.instance = this;
			U.Events.OnPlayerConnected += new UnturnedEvents.PlayerConnected(this.Events_OnPlayerConnected);
			U.Events.OnPlayerDisconnected += new UnturnedEvents.PlayerDisconnected(this.Events_OnPlayerDisconnected);
			DamageTool.damagePlayerRequested += new DamageTool.DamagePlayerHandler(this.DamageTool_damagePlayerRequested);
			UnturnedPlayerEvents.OnPlayerUpdateGesture += new UnturnedPlayerEvents.PlayerUpdateGesture(this.DoktorKontorlü);
			VehicleManager.onEnterVehicleRequested += new VehicleManager.EnterVehicleRequestHandler(this.VehicleManager_onEnterVehicleRequested);
			UnturnedPlayerEvents.OnPlayerDeath += new UnturnedPlayerEvents.PlayerDeath(this.UnturnedPlayerEvents_OnPlayerDead);
			if (base.Configuration.Instance.gelişmişSunucu)
			{
				this.coroutine = base.StartCoroutine(this.KontrolEt());
			}
		}
		private IEnumerator KontrolEt()
		{
			for (;;)
			{
				using (List<SteamPlayer>.Enumerator enumerator = (from e in Provider.clients
				where !this.woundeds.Any((Wounded c) => c.Id == e.playerID.steamID)
				select e).ToList<SteamPlayer>().GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						SteamPlayer item = enumerator.Current;
						Wounded wounded = this.woundeds.FirstOrDefault((Wounded e) => e.driftingMember != item.playerID.steamID && e.improveDoctor != item.playerID.steamID && Vector3.Distance(UnturnedPlayer.FromCSteamID(e.Id).Position, item.player.transform.position) <= this.Configuration.Instance.yaralıGörünümüMesafesi);
						if (wounded != null)
						{
							EffectManager.sendUIEffect(51001, 995, item.playerID.steamID, true, "YARALI", "Yerde yaralı birisi var " + UnturnedPlayer.FromCSteamID(wounded.Id).DisplayName);
						}
						else
						{
							EffectManager.askEffectClearByID(51001, item.playerID.steamID);
						}
					}
				}
				yield return new WaitForSeconds(1f);
			}
			yield break;
		}
		private void Events_OnPlayerDisconnected(UnturnedPlayer player)
		{
			if (this.woundeds.Any((Wounded e) => e.Id == player.CSteamID))
			{
				this.willDeads.Add(player.CSteamID);
			}
			PlayerEquipment equipment = player.Player.equipment;
			equipment.onEquipRequested = (PlayerEquipRequestHandler)Delegate.Remove(equipment.onEquipRequested, new PlayerEquipRequestHandler(this.onEquip));
		}

		private void Events_OnPlayerConnected(UnturnedPlayer player)
		{
			if (this.willDeads.Any((CSteamID e) => e == player.CSteamID))
			{
				player.Damage(byte.MaxValue, player.Position, 0, ELimb.SKULL, CSteamID.Nil);
				this.willDeads.Remove(player.CSteamID);
			}
			PlayerEquipment equipment = player.Player.equipment;
			equipment.onEquipRequested = (PlayerEquipRequestHandler)Delegate.Combine(equipment.onEquipRequested, new PlayerEquipRequestHandler(this.onEquip));
		}

		private void onEquip(PlayerEquipment equipment, ItemJar jar, ItemAsset asset, ref bool shouldAllow)
		{
			UnturnedPlayer player = UnturnedPlayer.FromPlayer(equipment.player);
			if (this.woundeds.Any((Wounded e) => e.Id == player.CSteamID))
			{
				shouldAllow = false;
			}
		}

		private void VehicleManager_onEnterVehicleRequested(Player caller, InteractableVehicle vehicle, ref bool shouldAllow)
		{
			UnturnedPlayer player = UnturnedPlayer.FromPlayer(caller);
			if (this.woundeds.Any((Wounded e) => e.Id == player.CSteamID))
			{
				shouldAllow = false;
			}
		}

		private void UnturnedPlayerEvents_OnPlayerDead(UnturnedPlayer player, EDeathCause cause, ELimb limb, CSteamID murderer)
		{
			if (this.woundeds.Any((Wounded e) => e.Id == player.CSteamID))
			{
				this.woundeds.Remove(this.woundeds.First((Wounded e) => e.Id == player.CSteamID));
				player.Player.movement.sendPluginSpeedMultiplier(1f);
				this.Temizle(player);
			}
		}

		private void DoktorKontorlü(UnturnedPlayer player, UnturnedPlayerEvents.PlayerGesture gesture)
		{
			if (!this.woundeds.Any<Wounded>())
			{
				return;
			}
			if (gesture == UnturnedPlayerEvents.PlayerGesture.SurrenderStart && player.Stance == EPlayerStance.CROUCH && player.HasPermission(base.Configuration.Instance.taşımaYetkisi))
			{
				Wounded wounded = this.woundeds.FirstOrDefault((Wounded e) => e.Id != player.CSteamID && !e.drifting && !e.improve && Vector3.Distance(player.Position, UnturnedPlayer.FromCSteamID(e.Id).Position) <= 5f);
				if (wounded != null)
				{
					wounded.drifting = true;
					wounded.driftingMember = player.CSteamID;
					UnturnedPlayer unturnedPlayer = UnturnedPlayer.FromCSteamID(wounded.Id);
					unturnedPlayer.Player.gameObject.AddComponent<DriftingManager>().Connect(unturnedPlayer, player, wounded, true);
					UnturnedChat.Say(player, "Taşımaya başladın.");
					return;
				}
			}
			if (gesture != UnturnedPlayerEvents.PlayerGesture.SurrenderStart && this.woundeds.Any((Wounded e) => e.driftingMember == player.CSteamID) && player.HasPermission(base.Configuration.Instance.taşımaYetkisi))
			{
				Wounded wounded2 = this.woundeds.FirstOrDefault((Wounded e) => e.Id != player.CSteamID && e.driftingMember == player.CSteamID);
				if (wounded2 == null)
				{
					return;
				}
				wounded2.drifting = false;
				wounded2.driftingMember = CSteamID.Nil;
				UnturnedChat.Say(player, "Taşımayı bıraktın.");
				return;
			}
			else if (gesture == UnturnedPlayerEvents.PlayerGesture.SurrenderStart && player.HasPermission(base.Configuration.Instance.canlandirmaYetkisi))
			{
				Wounded wounded3 = this.woundeds.FirstOrDefault((Wounded e) => e.Id != player.CSteamID && !e.improve && Vector3.Distance(player.Position, UnturnedPlayer.FromCSteamID(e.Id).Position) <= 5f);
				if (wounded3 == null)
				{
					return;
				}
				wounded3.improve = true;
				wounded3.improveDoctor = player.CSteamID;
				float canlandirmaSuresi = base.Configuration.Instance.canlandırmaSüresi;
				if (player.HasPermission(base.Configuration.Instance.doktorYetkisi))
				{
					canlandirmaSuresi = base.Configuration.Instance.doktorCanlandirmaSüresi;
				}
				UnturnedPlayer unturnedPlayer2 = UnturnedPlayer.FromCSteamID(wounded3.Id);
				unturnedPlayer2.Player.gameObject.AddComponent<UpManager>().Connect(unturnedPlayer2, player, wounded3, true, canlandirmaSuresi);
				UnturnedChat.Say(player, "İyileştirmeye başladın.");
				return;
			}
			else
			{
				if (gesture == UnturnedPlayerEvents.PlayerGesture.SurrenderStart || !player.HasPermission(base.Configuration.Instance.canlandirmaYetkisi) || !this.woundeds.Any((Wounded e) => e.improveDoctor == player.CSteamID))
				{
					return;
				}
				Wounded wounded4 = this.woundeds.FirstOrDefault((Wounded e) => e.Id != player.CSteamID && e.improveDoctor == player.CSteamID);
				if (wounded4 == null)
				{
					return;
				}
				wounded4.improve = false;
				wounded4.improveDoctor = CSteamID.Nil;
				UnturnedChat.Say(player, "İyileştirmeyi bıraktın.");
				return;
			}
		}

		public void Temizle(UnturnedPlayer player)
		{
			if (player != null)
			{
				EffectManager.askEffectClearByID(51001, player.CSteamID);
			}
		}

		private void DamageTool_damagePlayerRequested(ref DamagePlayerParameters parameters, ref bool shouldAllow)
		{
			UnturnedPlayer player = UnturnedPlayer.FromPlayer(parameters.player);
			if (player.GetPermissions().Any((Permission e) => e.Name == this.Configuration.Instance.bertarafEdilecekYetki))
			{
				return;
			}
			if (parameters.killer != CSteamID.Nil && base.Configuration.Instance.buSilahlarKesinÖldürsün.Contains(UnturnedPlayer.FromCSteamID(parameters.killer).Player.equipment.asset.id))
			{
				return;
			}
			if (base.Configuration.Instance.Yerde_Vur_Ölsün && this.woundeds.Any((Wounded e) => e.Id == player.CSteamID))
			{
				this.Öldür(player.CSteamID);
				return;
			}
			EDeathCause cause = parameters.cause;
			if ((cause == EDeathCause.ROADKILL && !base.Configuration.Instance.araçlaEzdiğindeÖl) || (cause == EDeathCause.MISSILE && !base.Configuration.Instance.füzeAttığındaÖl))
			{
				shouldAllow = false;
				if (!this.woundeds.Any((Wounded e) => e.Id == player.CSteamID))
				{
					this.Yarala(player);
				}
				return;
			}
			if (this.woundeds.Any((Wounded e) => e.Id == player.CSteamID))
			{
				shouldAllow = false;
				return;
			}
			float num = float.Parse(player.Health.ToString()) - parameters.damage;
			if (num <= 0f || num <= base.Configuration.Instance.kaçCandaDüşsün)
			{
				this.Yarala(player);
				shouldAllow = false;
				player.Player.life.channel.send("tellHealth", ESteamCall.OWNER, ESteamPacket.UPDATE_RELIABLE_BUFFER, new object[]
				{
					base.Configuration.Instance.yereDüştüğündeKaçCanıOlsun
				});
				return;
			}
		}

		// Token: 0x0600000B RID: 11 RVA: 0x000027B0 File Offset: 0x000009B0
		public void Yarala(UnturnedPlayer player)
		{
			if (this.woundeds.Any((Wounded e) => e.Id == player.CSteamID))
			{
				return;
			}
			Wounded wounded = new Wounded(player.CSteamID, true, DateTime.Now);
			this.woundeds.Add(wounded);
			player.Player.movement.sendPluginSpeedMultiplier(0f);
			player.Player.equipment.dequip();
			byte b;
			Vector3 vector;
			byte b2;
			if (player.CurrentVehicle)
			{
				player.CurrentVehicle.forceRemovePlayer( out _, player.CSteamID, out _, out _);
			}
			if (player.Bleeding && base.Configuration.Instance.yereDüştüğündeKanamaDurdur)
			{
				player.Bleeding = false;
			}
			if (base.Configuration.Instance.yereDüştüğündeKanamaBaşlat)
			{
				player.Bleeding = true;
			}
			if (base.Configuration.Instance.yereDüştüğündeBacakKırıl)
			{
				player.Broken = true;
			}
			if (player.CurrentVehicle != null)
			{
				player.CurrentVehicle.forceRemovePlayer(out _, player.CSteamID, out _, out _);
			}
			player.Player.gameObject.AddComponent<WoundedManager>().Connect(player, wounded, true);
		}
		public void Kaldır(UnturnedPlayer player, UnturnedPlayer victim)
		{
			if (this.woundeds.Any((Wounded e) => e.Id == victim.CSteamID))
			{
				this.woundeds.Remove(this.woundeds.First((Wounded e) => e.Id == victim.CSteamID));
			}
			if (victim != null)
			{
				victim.Player.movement.sendPluginSpeedMultiplier(1f);
				this.Temizle(victim);
				victim.Player.life.channel.send("tellHealth", ESteamCall.OWNER, ESteamPacket.UPDATE_RELIABLE_BUFFER, new object[]
				{
					base.Configuration.Instance.kalktığındaKaçCanıOlsun
				});
				UnturnedChat.Say(victim, player.DisplayName + " tarafından canlandırıldın.");
			}
			if (player != null)
			{
				this.Temizle(player);
				UnturnedChat.Say(player, victim.DisplayName + " kişisinin canlandırdın.");
			}
		}

		public void Öldür(CSteamID id)
		{
			UnturnedPlayer player = UnturnedPlayer.FromCSteamID(id);
			if (player != null)
			{
				player.Damage(byte.MaxValue, player.Position, 0, ELimb.SKULL, player.CSteamID);
				this.Temizle(player);
				player.Player.movement.sendPluginSpeedMultiplier(1f);
			}
			if (this.woundeds.Any((Wounded e) => e.Id == player.CSteamID))
			{
				this.woundeds.Remove(this.woundeds.First((Wounded e) => e.Id == player.CSteamID));
			}
		}

		protected override void Unload()
		{
			Main.instance = null;
			U.Events.OnPlayerConnected -= new UnturnedEvents.PlayerConnected(this.Events_OnPlayerConnected);
			U.Events.OnPlayerDisconnected -= new UnturnedEvents.PlayerDisconnected(this.Events_OnPlayerDisconnected);
			DamageTool.damagePlayerRequested -= new DamageTool.DamagePlayerHandler(this.DamageTool_damagePlayerRequested);
			UnturnedPlayerEvents.OnPlayerUpdateGesture -= new UnturnedPlayerEvents.PlayerUpdateGesture(this.DoktorKontorlü);
			VehicleManager.onEnterVehicleRequested -= new VehicleManager.EnterVehicleRequestHandler(this.VehicleManager_onEnterVehicleRequested);
			UnturnedPlayerEvents.OnPlayerDeath -= new UnturnedPlayerEvents.PlayerDeath(this.UnturnedPlayerEvents_OnPlayerDead);
			this.woundeds.Clear();
			this.willDeads.Clear();
			base.StopCoroutine(this.coroutine);
		}

		public static Main instance;

		public List<Wounded> woundeds = new List<Wounded>();

		private List<CSteamID> willDeads = new List<CSteamID>();

		private Coroutine coroutine;
	}
}
