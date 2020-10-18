using System;
using System.Collections.Generic;
using System.Linq;
using Alosha;
using MedicalSystemUI.Models;
using Rocket.API;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;

namespace MedicalSystemUI.Commands
{
	public class CommandRevive : IRocketCommand
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
				return "revive";
			}
		}

		public string Help
		{
			get
			{
				return "Birisini canlandırmanı sağlar.";
			}
		}

		public string Syntax
		{
			get
			{
				return "revive";
			}
		}

		public List<string> Aliases
		{
			get
			{
				return new List<string>
				{
					"canlandır",
					"canlandir"
				};
			}
		}

		public List<string> Permissions
		{
			get
			{
				return new List<string>
				{
					"alosha.revive",
					"alosha.canlandir"
				};
			}
		}

		public void Execute(IRocketPlayer caller, string[] command)
		{
			UnturnedPlayer unturnedPlayer = caller as UnturnedPlayer;
			if (command.Length == 0)
			{
				UnturnedChat.Say(unturnedPlayer, "/revive <playerName>");
				return;
			}
			UnturnedPlayer victim = UnturnedPlayer.FromName(string.Join(" ", command));
			if (victim == null)
			{
				UnturnedChat.Say(unturnedPlayer, "Böyle bir oyuncu yok!");
				return;
			}
			if (!Main.instance.woundeds.Any((Wounded e) => e.Id == victim.CSteamID))
			{
				UnturnedChat.Say(unturnedPlayer, "Bu oyuncu yaralı olmadığı için canlandırılamıyor.");
				return;
			}
			Main.instance.Kaldır(unturnedPlayer, victim);
			UnturnedChat.Say(unturnedPlayer, victim.DisplayName + " kişisini hızlı canlandırdın.");
			UnturnedChat.Say(unturnedPlayer, unturnedPlayer.DisplayName + " seni canlandırdı.");
		}
	}
}
