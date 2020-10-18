using System;
using System.Collections.Generic;
using System.Linq;
using Alosha;
using MedicalSystemUI.Models;
using Rocket.API;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using SDG.Unturned;
using Steamworks;
using UnityEngine;

namespace MedicalSystemUI.Commands
{
	public class CommandSuicide : IRocketCommand
	{
		public AllowedCaller AllowedCaller
		{
			get
			{
				return AllowedCaller.Player;
			}
		}

		public string Name
		{
			get
			{
				return "intihar";
			}
		}

		public string Help
		{
			get
			{
				return "Eğer yaralıysan intihar etmeni sağlar.";
			}
		}

		public string Syntax
		{
			get
			{
				return "suicide";
			}
		}

		public List<string> Aliases
		{
			get
			{
				return new List<string>
				{
					"suicide",
					"öl"
				};
			}
		}

		public List<string> Permissions
		{
			get
			{
				return new List<string>
				{
					"alosha.intihar",
					"alosha.suicide"
				};
			}
		}

		public void Execute(IRocketPlayer caller, string[] command)
		{
			UnturnedPlayer player = caller as UnturnedPlayer;
			if (Main.instance.woundeds.Any((Wounded e) => e.Id == player.CSteamID))
			{
				player.Damage(byte.MaxValue, player.Position, 0, ELimb.SKULL, CSteamID.Nil);
				UnturnedChat.Say(player, "İntihar ettin.", Color.red);
				return;
			}
			UnturnedChat.Say(player, "Şu an intihar edemezsin.", Color.red);
		}
	}
}
