#region using

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Timers;
using System.Xml.Serialization;
using LobbyClient;
using PlasmaShared;
using Springie.AutoHostNamespace;
using Springie.PlanetWars;
using Springie.SpringNamespace;
using Timer = System.Timers.Timer;

#endregion

namespace Springie.autohost
{
	public partial class AutoHost
	{
		public const string BoxesName = "boxes.bin";
		public const string ConfigName = "autohost.xml";

		public const int PollTimeout = 60;
		public const string PresetsName = "presets.xml";
		static readonly object savLock = new object();

		IVotable activePoll;
		int autoLock;
		BanList banList;
		string bossName = "";
		string delayedModChange;


		bool kickMinRank;
		bool kickSpectators;
		ResourceLinkProvider linkProvider;
		bool lockedByKickSpec;

		AutoManager manager;
		int mapCycleIndex;



		double minCpuSpeed;
		Timer pollTimer;
		readonly Spring spring;
		readonly Stats stats;

		public string BossName { get { return bossName; } set { bossName = value; } }
		public int CloneNumber { get; private set; }

		public bool KickSpectators { get { return kickSpectators; } }

		public Dictionary<string, Dictionary<int, BattleRect>> MapBoxes = new Dictionary<string, Dictionary<int, BattleRect>>();

		public double MinCpuSpeed { get { return minCpuSpeed; } }
		public PlanetWarsHandler PlanetWars;

		public SpawnConfig SpawnConfig { get; private set; }
		public AutoHostConfig config = new AutoHostConfig();
		public string configPath;
		public int hostingPort { get; private set; }
		public Ladder ladder;
		public List<Preset> presets = new List<Preset>();

		public SpringPaths springPaths;
		public TasClient tas;
		public UnitSyncWrapper wrapper { get; private set; }

		QuickMatchTracking quickMatchTracker;

		public AutoHost(SpringPaths paths, UnitSyncWrapper wrapper, string configPath, int hostingPort, SpawnConfig spawn)
		{
			SpawnConfig = spawn;
			this.configPath = configPath;
			springPaths = paths;
			this.hostingPort = hostingPort;
			spring = new Spring(paths) { UseDedicatedServer = true };
			tas = new TasClient(null, "Springie " + MainConfig.SpringieVersion);
			quickMatchTracker = new QuickMatchTracking(tas, null);

			banList = new BanList(this, tas);

			LoadConfig();
			SaveConfig();

			this.wrapper = wrapper;

			pollTimer = new Timer(PollTimeout*1000);
			pollTimer.Enabled = false;
			pollTimer.AutoReset = false;
			pollTimer.Elapsed += pollTimer_Elapsed;

			spring.SpringExited += spring_SpringExited;
			spring.GameOver += spring_GameOver;

			spring.SpringExited += spring_SpringExited;
			spring.SpringStarted += spring_SpringStarted;
			spring.PlayerSaid += spring_PlayerSaid;

			wrapper.NotifyModsChanged += spring_NotifyModsChanged;

			tas.BattleUserLeft += tas_BattleUserLeft;
			tas.UserStatusChanged += tas_UserStatusChanged;
			tas.BattleUserJoined += tas_BattleUserJoined;
			tas.MyBattleMapChanged += tas_MyBattleMapChanged;
			tas.BattleUserStatusChanged += tas_BattleUserStatusChanged;
			tas.BattleLockChanged += tas_BattleLockChanged;
			tas.BattleOpened += tas_BattleOpened;

			tas.RegistrationDenied += (s, e) =>
				{
					ErrorHandling.HandleException(null, "Registration denied: " + e.ServerParams[0]);
					CloneNumber++;
					tas.Login(GetAccountName(), config.AccountPassword);
				};

			tas.RegistrationAccepted += (s, e) => tas.Login(GetAccountName(), config.AccountPassword);

			tas.AgreementRecieved += (s, e) =>
				{
					tas.AcceptAgreement();

					PlasmaShared.Utils.SafeThread(() =>
						{
							Thread.Sleep(7000);
							tas.Login(GetAccountName(), config.AccountPassword);
						}).Start();
				};

			tas.ConnectionLost += tas_ConnectionLost;
			tas.Connected += tas_Connected;
			tas.LoginDenied += tas_LoginDenied;
			tas.LoginAccepted += tas_LoginAccepted;
			tas.Said += tas_Said;
			tas.MyBattleStarted += tas_MyStatusChangedToInGame;
			tas.ChannelUserAdded += tas_ChannelUserAdded;

			linkProvider = new ResourceLinkProvider(this);

			if (config.StatsEnabled) stats = new Stats(tas, spring, this);

			InitializePlanetWarsServer();

			tas.Connect(Program.main.Config.ServerHost, Program.main.Config.ServerPort);
		}

		public void Dispose()
		{
			Stop();
			tas.UnsubscribeEvents(this);
			tas.UnsubscribeEvents(manager);
			tas.UnsubscribeEvents(banList);
			tas.UnsubscribeEvents(stats);
			tas.UnsubscribeEvents(PlanetWars);
			spring.UnsubscribeEvents(this);
			spring.UnsubscribeEvents(PlanetWars);
			spring.UnsubscribeEvents(stats);
			wrapper.UnsubscribeEvents(this);
			tas.Disconnect();
			if (PlanetWars != null) PlanetWars.Dispose();
			pollTimer.Dispose();
			if (manager != null) manager.Stop();
			banList.Close();
			ladder = null;
			MapBoxes = null;
			banList = null;
			manager = null;
			pollTimer = null;
			PlanetWars = null;
			linkProvider = null;
			wrapper = null;
		}

		public string GetAccountName()
		{
			if (CloneNumber > 0) return config.AccountName + CloneNumber;
			else return config.AccountName;
		}

		/*void fileDownloader_DownloadProgressChanged(object sender, TasEventArgs e)
    {
      if (tas.IsConnected) {
        SayBattle(e.ServerParams[0] + " " + e.ServerParams[1] + "% done");
      }
    }*/


		public int GetUserLevel(TasSayEventArgs e)
		{
			return GetUserLevel(e.UserName);
		}

		public int GetUserLevel(string name)
		{
			foreach (var pu in config.PrivilegedUsers) if (pu.Name == name) return pu.Level;
			User u;
			if (tas.GetExistingUser(name, out u)) if (u.IsAdmin) return config.DefaulRightsLevelForLobbyAdmins;
			return name == bossName ? config.BossRightsLevel : config.DefaulRightsLevel;
		}


		public bool HasRights(string command, TasSayEventArgs e)
		{
			if (banList.IsBanned(e.UserName))
			{
				Respond(e, "tough luck, you are banned");
				return false;
			}
			foreach (var c in config.Commands)
			{
				if (c.Name == command)
				{
					if (c.Throttling > 0)
					{
						var diff = (int)DateTime.Now.Subtract(c.lastCall).TotalSeconds;
						if (diff < c.Throttling)
						{
							Respond(e, "AntiSpam - please wait " + (c.Throttling - diff) + " more seconds");
							return false;
						}
					}

					for (var i = 0; i < c.ListenTo.Length; i++)
					{
						if (c.ListenTo[i] == e.Place)
						{
							var reqLevel = c.Level;
							var ulevel = GetUserLevel(e);

							if (ulevel >= reqLevel)
							{
								// boss stuff
								if (bossName != "" && ulevel <= config.DefaulRightsLevel && e.UserName != bossName && config.DefaultRightsLevelWithBoss < reqLevel)
								{
									Respond(e, "Sorry, you cannot do this right now, ask boss admin " + bossName);
									return false;
								}
								else
								{
									c.lastCall = DateTime.Now;
									return true; // ALL OK
								}
							}
							else
							{
								if (e.Place == TasSayEventArgs.Places.Battle && tas.MyBattle != null && tas.MyBattle.NonSpectatorCount == 1 && (!command.StartsWith("vote") && HasRights("vote" + command, e)))
								{ // server only has 1 player and we have rights for vote variant - we might as well just do it
									return true;
								}
								else
								{
									Respond(e, "Sorry, you do not have rights to execute " + command);
									return false;
								}
							}
						}
					}
					return false; // place not allowed for this command = ignore command
				}
			}
			if (e.Place != TasSayEventArgs.Places.Channel) Respond(e, "Sorry, I don't know command '" + command + "'");
			return false;
		}

		public void InitializePlanetWarsServer()
		{
			if (PlanetWars != null) PlanetWars.Dispose();

			if (config.PlanetWarsEnabled) PlanetWars = new PlanetWarsHandler(Program.main.Config.PlanetWarsServer, Program.main.Config.PlanetWarsPort, this, tas, config);
			else PlanetWars = null;
		}

		public void LoadConfig()
		{
			if (File.Exists(configPath + '/' + ConfigName))
			{
				var s = new XmlSerializer(config.GetType());
				var r = File.OpenText(configPath + '/' + ConfigName);
				config = (AutoHostConfig)s.Deserialize(r);
				r.Close();
				config.AddMissingCommands();
			}
			else config.Defaults();

			if (File.Exists(configPath + '/' + PresetsName))
			{
				var s = new XmlSerializer(presets.GetType());
				using (var r = File.OpenText(configPath + '/' + PresetsName))
				{
					presets = (List<Preset>)s.Deserialize(r);
					r.Close();
				}
			}

			if (File.Exists(springPaths.Cache + '/' + BoxesName))
			{
				try
				{
					var frm = new BinaryFormatter();
					using (var r = new FileStream(springPaths.Cache + '/' + BoxesName, FileMode.Open))
					{
						MapBoxes = (Dictionary<string, Dictionary<int, BattleRect>>)frm.Deserialize(r);
						r.Close();
					}
				}
				catch (Exception ex3)
				{
					ErrorHandling.HandleException(ex3, "Unable to load boxes");
				}
			}

			banList.Load();
		}

		public void RegisterVote(TasSayEventArgs e, string[] words)
		{
			if (activePoll != null)
			{
				if (activePoll.Vote(e, words)) StopVote();
			}
			else Respond(e, "There is no poll going on, start some first");
		}

		public void Respond(TasSayEventArgs e, string text)
		{
			Respond(tas, spring, e, text);
		}

		public static void Respond(TasClient tas, Spring spring, TasSayEventArgs e, string text)
		{
			var p = TasClient.SayPlace.User;
			var emote = false;
			if (e.Place == TasSayEventArgs.Places.Battle)
			{
				p = TasClient.SayPlace.Battle;
				emote = true;
			}
			if (e.Place == TasSayEventArgs.Places.Game && spring.IsRunning) spring.SayGame(text);
			else tas.Say(p, e.UserName, text, emote);
		}

		public void SaveConfig()
		{
			lock (savLock)
			{
				config.Commands.Sort(AutoHostConfig.CommandComparer);

				// remove duplicated admins
				var l = new List<PrivilegedUser>();
				foreach (var p in config.PrivilegedUsers) if (l.Find(delegate(PrivilegedUser u) { return u.Name == p.Name; }) == null) l.Add(p);
				;
				config.PrivilegedUsers = l;
				config.PrivilegedUsers.Sort(AutoHostConfig.UserComparer);

				presets.Sort(delegate(Preset a, Preset b) { return a.Name.CompareTo(b.Name); });

				var s = new XmlSerializer(config.GetType());
				var f = File.OpenWrite(configPath + '/' + ConfigName);
				f.SetLength(0);
				s.Serialize(f, config);
				f.Close();

				s = new XmlSerializer(presets.GetType());
				f = File.OpenWrite(configPath + '/' + PresetsName);
				f.SetLength(0);
				s.Serialize(f, presets);
				f.Close();

				banList.Save();

				var fm = new BinaryFormatter();
				using (var fs = new FileStream(springPaths.Cache + '/' + BoxesName, FileMode.Create))
				{
					fm.Serialize(fs, MapBoxes);
					fs.Close();
				}
			}
		}


		public void SayBattle(string text)
		{
			SayBattle(text, true);
		}

		public void SayBattle(string text, bool ingame)
		{
			SayBattle(tas, spring, text, ingame);
		}


		public static void SayBattle(TasClient tas, Spring spring, string text, bool ingame)
		{
			tas.Say(TasClient.SayPlace.Battle, "", text, true);
			if (spring.IsRunning && ingame) spring.SayGame(text);
		}

		public void Start(string modname, string mapname)
		{
			Stop();

			manager = new AutoManager(this, tas, spring);
			lockedByKickSpec = false;
			autoLock = 0;
			kickSpectators = config.KickSpectators;
			minCpuSpeed = config.MinCpuSpeed;
			kickMinRank = config.KickMinRank;

			if (config.LadderId > 0) ladder = new Ladder(config.LadderId);
			else ladder = null;

			if (String.IsNullOrEmpty(modname)) modname = config.DefaultMod;
			if (String.IsNullOrEmpty(mapname)) mapname = config.DefaultMap;

			if (!string.IsNullOrEmpty(config.AutoUpdateRapidTag)) modname = config.AutoUpdateRapidTag;

			var title = (ladder != null ? "(ladder " + ladder.Id + ") " : "") + config.GameTitle.Replace("%1", MainConfig.SpringieVersion);
			var password = (ladder == null ? config.Password : "ladderlock2");

			if (SpawnConfig != null)
			{
				modname = SpawnConfig.Mod;
				title = SpawnConfig.Title;
				if (string.IsNullOrEmpty(SpawnConfig.Password)) password = "*";
				else password = SpawnConfig.Password;
			}

			var version = Program.main.Downloader.PackageDownloader.GetByTag(modname);
			if (version != null) modname = version.InternalName;

			if (!wrapper.HasMap(mapname)) mapname = modname = wrapper.GetFirstMap();
			if (!wrapper.HasMod(modname)) modname = wrapper.GetFirstMod();

			int mint, maxt;
			var mapi = wrapper.GetMapInfo(mapname);
			var modi = wrapper.GetModInfo(modname);
			var b = new Battle(password,
			                   hostingPort,
			                   config.MaxPlayers,
			                   config.MinRank,
			                   mapi,
			                   title,
			                   modi,
			                   (ladder != null ? ladder.CheckBattleDetails(config.BattleDetails, out mint, out maxt) : config.BattleDetails));
			// if hole punching enabled then we use it
			if (config.UseHolePunching) b.Nat = Battle.NatMode.HolePunching;
			else if (Program.main.Config.GargamelMode && stats != null) b.Nat = Battle.NatMode.FixedPorts;
			else b.Nat = Battle.NatMode.None; // else either no nat or fixed ports (for gargamel fake - to get client IPs)

			for (var i = 0; i < config.DefaultRectangles.Count; ++i) b.Rectangles.Add(i, config.DefaultRectangles[i]);
			tas.OpenBattle(b);

			if (SpawnConfig != null) tas.Say(TasClient.SayPlace.User, SpawnConfig.Owner, "I'm here! Ready to server you! Join me!", false);
		}


		public void StartVote(IVotable vote, TasSayEventArgs e, string[] words)
		{
			if (vote != null)
			{
				if (activePoll != null)
				{
					Respond(e, "Another poll already in progress, please wait");
					return;
				}
				if (vote.Init(e, words))
				{
					activePoll = vote;
					pollTimer.Interval = PollTimeout*1000;
					pollTimer.Enabled = true;
				}
			}
		}


		public void Stop()
		{
			if (manager != null) manager.Stop();
			StopVote();
			spring.ExitGame();
			tas.ChangeMyStatus(false, false);
			tas.LeaveBattle();
		}

		public void StopVote()
		{
			pollTimer.Enabled = false;
			activePoll = null;
		}

		void CheckForBattleExit()
		{
			if ((DateTime.Now - spring.GameStarted) > TimeSpan.FromSeconds(20))
			{
				if (spring.IsRunning)
				{
					var b = tas.MyBattle;
					var count = 0;
					foreach (var p in b.Users)
					{
						if (p.IsSpectator) continue;

						User u;
						if (!tas.GetExistingUser(p.Name, out u)) continue;
						if (u.IsInGame) count++;
					}
					if (count < 1)
					{
						SayBattle("closing game, " + count + " active player left in game");
						spring.ExitGame();
					}
				}
				// kontrola pro pripad ze by se nevypl spring
				User us;
				if (!spring.IsRunning && tas.GetExistingUser(tas.UserName, out us) && us.IsInGame) tas.ChangeMyStatus(false, false);
			}
		}

		void HandleAutoLocking()
		{
			if (!manager.Enabled && autoLock > 0 && (!spring.IsRunning))
			{
				var b = tas.MyBattle;
				var cnt = b.NonSpectatorCount;
				if (cnt >= autoLock) tas.ChangeLock(true);

				if (cnt < autoLock) tas.ChangeLock(false);
			}
		}

		void HandleKickSpecServerLocking()
		{
			if (!spring.IsRunning && (KickSpectators || lockedByKickSpec))
			{
				var b = tas.MyBattle;
				var cnt = b.NonSpectatorCount;
				if (KickSpectators && cnt >= b.MaxPlayers)
				{
					lockedByKickSpec = true;
					tas.ChangeLock(true);
				}

				if (lockedByKickSpec && cnt < b.MaxPlayers)
				{
					lockedByKickSpec = false;
					tas.ChangeLock(false);
				}
			}
		}

		void HandleMinRankKicking()
		{
			if (kickMinRank && config.MinRank > 0)
			{
				var b = tas.MyBattle;
				if (b != null)
				{
					foreach (var u in b.Users)
					{
						User x;
						tas.GetExistingUser(u.Name, out x);
						if (u.Name != tas.UserName && x.Rank < config.MinRank)
						{
							SayBattle(x.Name + ", your rank is too low, rank kicking is enabled here");
							ComKick(TasSayEventArgs.Default, new[] { u.Name });
						}
					}
				}
			}
		}

		void pollTimer_Elapsed(object sender, ElapsedEventArgs e)
		{
			if (activePoll != null) activePoll.TimeEnd();
			StopVote();
		}

		void spring_GameOver(object sender, SpringLogEventArgs e)
		{
			SayBattle("Game over, exiting");
			PlasmaShared.Utils.SafeThread(()=>
				{
					Thread.Sleep(3000); // wait for stats
					spring.ExitGame();
				}).Start();

			if (config.MapCycle.Length > 0)
			{
				mapCycleIndex = mapCycleIndex%config.MapCycle.Length;
				SayBattle("changing to another map in mapcycle");
				ComMap(TasSayEventArgs.Default, config.MapCycle[mapCycleIndex].Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
				mapCycleIndex++;
			}
		}

		void spring_NotifyModsChanged(object sender, EventArgs e)
		{
			if (!string.IsNullOrEmpty(config.AutoUpdateRapidTag) && SpawnConfig == null)
			{
				var version = Program.main.Downloader.PackageDownloader.GetByTag(config.AutoUpdateRapidTag);
				if (version != null)
				{
					var latest = version.InternalName;
					if (wrapper.HasMod(latest))
					{
						var b = tas.MyBattle;
						if (!string.IsNullOrEmpty(latest) && b != null && b.ModName != latest)
						{
							config.DefaultMod = latest;
							if (!spring.IsRunning && b.NonSpectatorCount == 0)
							{
								SayBattle("Updating to latest mod version: " + latest);
								ComRehost(TasSayEventArgs.Default, new[] { latest });
							}
							else delayedModChange = latest;
						}
					}
				}
			}
		}


		void spring_PlayerSaid(object sender, SpringLogEventArgs e)
		{
			tas.GameSaid(e.Username, e.Line);
			if (Program.main.Config.RedirectGameChat && e.Username != tas.UserName && !e.Line.StartsWith("Allies:") && !e.Line.StartsWith("Spectators:")) tas.Say(TasClient.SayPlace.Battle, "", "[" + e.Username + "]" + e.Line, false);
		}


		void spring_SpringExited(object sender, EventArgs e)
		{
			tas.ChangeLock(false);
			tas.ChangeMyStatus(false, false);
			if (PlanetWars != null) PlanetWars.SpringExited();
			var b = tas.MyBattle;
			foreach (var s in toNotify)
			{
				if (b != null && b.Users.Any(x => x.Name == s)) tas.Ring(s);
				tas.Say(TasClient.SayPlace.User, s, "** Game just ended, join me! **", false);
			}
			toNotify.Clear();
		}

		void spring_SpringStarted(object sender, EventArgs e)
		{
			tas.ChangeLock(false);
		}


		void tas_BattleLockChanged(object sender, BattleInfoEventArgs e1)
		{
			if (e1.BattleID == tas.MyBattleID) SayBattle("game " + (tas.MyBattle.IsLocked ? "locked" : "unlocked"), false);
		}

		void tas_BattleOpened(object sender, TasEventArgs e)
		{
			tas.DisableUnits(config.DisabledUnits.Select(x => x.Name).ToArray());
			tas.ChangeMyStatus(true, false, SyncStatuses.Synced);
		}


		void tas_BattleUserJoined(object sender, BattleUserEventArgs e1)
		{
			if (e1.BattleID != tas.MyBattleID) return;
			var name = e1.UserName;

			double elo;
			double w;
			Program.main.SpringieServer.GetElo(name, out elo, out w);

			var welc = config.Welcome;
			if (welc != "")
			{
				welc = welc.Replace("%1", name);
				welc = welc.Replace("%2", GetUserLevel(name).ToString());
				welc = welc.Replace("%3", MainConfig.SpringieVersion);
				SayBattle(welc, false);
			}
			if (spring.IsRunning)
			{
				spring.AddUser(e1.UserName, e1.ScriptPassword);
				var started = DateTime.Now.Subtract(spring.GameStarted);
				started = new TimeSpan((int)started.TotalHours, started.Minutes, started.Seconds);
				SayBattle(string.Format("GAME IS CURRENTLY IN PROGRESS, PLEASE WAIT TILL IT ENDS! Running for {0}", started), false);
				SayBattle("If you say !notify, I will PM you when game ends.", false);
			}

			HandleKickSpecServerLocking();
			HandleAutoLocking();
			HandleMinRankKicking();

			if (minCpuSpeed > 0)
			{
				User u;
				if (tas.GetExistingUser(name, out u))
				{
					if (u.Cpu != 0 && u.Cpu < minCpuSpeed*1000)
					{
						Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
						SayBattle(name + ", your CPU speed is below minimum set for this server:" + minCpuSpeed + "GHz - sorry");
						ComKick(TasSayEventArgs.Default, new[] { u.Name });
					}
				}
			}
			if (PlanetWars != null) PlanetWars.UserJoined(name);

			if (SpawnConfig != null && SpawnConfig.Owner == name) // owner joins, set him boss 
				ComBoss(TasSayEventArgs.Default, new[] { name });
		}

		void tas_BattleUserLeft(object sender, BattleUserEventArgs e1)
		{
			if (e1.BattleID != tas.MyBattleID) return;
			CheckForBattleExit();
			HandleKickSpecServerLocking();
			HandleAutoLocking();

			if (spring.IsRunning) spring.SayGame(e1.UserName + " has left lobby");

			if (e1.UserName == bossName)
			{
				SayBattle("boss has left the battle");
				bossName = "";
			}

			var battle = tas.MyBattle;
			if (battle.IsLocked && battle.Users.Count < 2)
			{
				// player left and only 2 remaining (springie itself + some noob) -> unlock
				tas.ChangeLock(false);
			}

			if (!spring.IsRunning && delayedModChange != null && battle.NonSpectatorCount == 0)
			{
				var mod = delayedModChange;
				delayedModChange = null;
				SayBattle("Updating to latest mod version: " + mod);
				ComRehost(TasSayEventArgs.Default, new[] { mod });
			}
		}

		void tas_BattleUserStatusChanged(object sender, TasEventArgs e)
		{
			UserBattleStatus u;
			var b = tas.MyBattle;

			if (b != null && b.ContainsUser(e.ServerParams[0], out u))
			{
				if (KickSpectators && u.IsSpectator && u.Name != tas.UserName)
				{
					SayBattle(config.KickSpectatorText);
					ComKick(TasSayEventArgs.Default, new[] { u.Name });
				}
				HandleAutoLocking();

				var cnt = 0;
				foreach (var ubs in b.Users) if (!ubs.IsSpectator && ubs.IsReady) cnt++;
				List<string> usname;
				int allyno;
				if (!manager.Enabled)
				{
					int alliances;
					if ((cnt == config.MaxPlayers || (autoLock > 0 && autoLock == cnt)) && !config.PlanetWarsEnabled && AllReadyAndSynced(out usname) &&
					    AllUniqueTeams(out usname) && BalancedTeams(out allyno, out alliances))
					{
						SayBattle("server is full, starting");
						Thread.Sleep(1000); // just to make sure that other clients update their game info and balance ends
						ComStart(TasSayEventArgs.Default, new string[] { });
					}
				}
			}
		}

		void tas_ChannelUserAdded(object sender, TasEventArgs e)
		{
			if (PlanetWars != null) PlanetWars.UserJoinedChannel(e.ServerParams[0], e.ServerParams[1]);
		}


		// login accepted - join channels

		// im connected, let's login
		void tas_Connected(object sender, TasEventArgs e)
		{
			tas.Login(GetAccountName(), config.AccountPassword);
		}


		void tas_ConnectionLost(object sender, TasEventArgs e)
		{
			Stop();
		}


		
		void tas_LoginAccepted(object sender, TasEventArgs e)
		{
			for (var i = 0; i < config.JoinChannels.Count; ++i) tas.JoinChannel(config.JoinChannels[i]);
			Start(null, null);
		}

		void tas_LoginDenied(object sender, TasEventArgs e)
		{
			if (e.ServerParams[0] == "invalid username") tas.Register(GetAccountName(), config.AccountPassword);
			else
			{
				CloneNumber++;
				tas.Login(GetAccountName(), config.AccountPassword);
			}
		}

		void tas_MyBattleMapChanged(object sender, BattleInfoEventArgs e1)
		{
			var b = tas.MyBattle;
			var mapName = b.MapName.ToLower();
			if (MapBoxes.ContainsKey(mapName))
			{
				for (var i = 0; i < b.Rectangles.Count; ++i) tas.RemoveBattleRectangle(i);
				var dict = MapBoxes[mapName];
				foreach (var v in dict) tas.AddBattleRectangle(v.Key, v.Value);
			}

			if (PlanetWars != null) PlanetWars.MapChanged();
		}

		void tas_MyStatusChangedToInGame(object sender, TasEventArgs e)
		{
			var b = tas.MyBattle;
			if (b != null)
			{
				var curMap = b.MapName.ToLower();

				var nd = new Dictionary<int, BattleRect>();
				foreach (var v in b.Rectangles) nd.Add(v.Key, v.Value);

				if (MapBoxes.ContainsKey(curMap)) MapBoxes[curMap] = nd;
				else MapBoxes.Add(curMap, nd);
				SaveConfig();
			}

			spring.StartGame(tas, Program.main.Config.HostingProcessPriority, Program.main.Config.SpringCoreAffinity, null);
		}

		void tas_Said(object sender, TasSayEventArgs e)
		{
			if (PlanetWars != null) PlanetWars.UserSaid(e);

			if (config.RedirectGameChat && e.Place == TasSayEventArgs.Places.Battle && e.Origin == TasSayEventArgs.Origins.Player && e.UserName != tas.UserName &&
			    e.IsEmote == false) spring.SayGame("[" + e.UserName + "]" + e.Text);

			// check if it's command
			if (e.Origin == TasSayEventArgs.Origins.Player && !e.IsEmote && e.Text.StartsWith("!"))
			{
				if (e.Text.Length < 2) return;
				var allwords = e.Text.Substring(1).Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
				if (allwords.Length < 1) return;
				var com = allwords[0];

				// remove first word (command)
				var words = Utils.ShiftArray(allwords, -1);

				if (!HasRights(com, e))
				{
					if (!com.StartsWith("vote"))
					{
						com = "vote" + com;

						if (!config.Commands.Any(x=>x.Name == com) || !HasRights(com, e)) return;
					}
					else return;
				}

				if (e.Place == TasSayEventArgs.Places.Normal)
				{
					if (com != "say" && com != "admins" && com != "help" && com != "helpall" && com != "springie" && com != "listpresets" && com != "listoptions" && com != "presetdetails" && com != "spawn" && com != "listbans" && com != "smurfs" && com != "stats" && com != "predict" && com != "notify") SayBattle(string.Format("{0} executed by {1}", com, e.UserName));
				}

				switch (com)
				{
					case "listmaps":
						ComListMaps(e, words);
						break;

					case "listmods":
						ComListMods(e, words);
						break;

					case "help":
						ComHelp(e, words);
						break;

					case "map":
						ComMap(e, words);
						break;

					case "admins":
						ComAdmins(e, words);
						break;

					case "start":
						ComStart(e, words);
						break;

					case "forcestart":
						ComForceStart(e, words);
						break;

					case "force":
						ComForce(e, words);
						break;

					case "split":
						ComSplit(e, words);
						break;

					case "corners":
						ComCorners(e, words);
						break;

					case "maplink":
						linkProvider.FindLinks(words, ResourceLinkProvider.FileType.Map, tas, e);
						break;

					case "modlink":
						linkProvider.FindLinks(words, ResourceLinkProvider.FileType.Mod, tas, e);
						break;

					case "ring":
						ComRing(e, words);
						break;

					case "kick":
						ComKick(e, words);
						break;

					case "exit":
						ComExit(e, words);
						break;

					case "lock":
						if (!manager.Enabled) tas.ChangeLock(true);
						break;

					case "unlock":
						if (!manager.Enabled) tas.ChangeLock(false);
						break;

					case "vote":
						RegisterVote(e, words);
						break;

					case "votemap":
						StartVote(new VoteMap(tas, spring, this), e, words);
						break;

					case "votekick":
						StartVote(new VoteKick(tas, spring, this), e, words);
						break;

					case "voteforcestart":
						StartVote(new VoteForceStart(tas, spring, this), e, words);
						break;

					case "voteforce":
						StartVote(new VoteForce(tas, spring, this), e, words);
						break;

					case "voteexit":
						StartVote(new VoteExit(tas, spring, this), e, words);
						break;

					case "predict":
						ComPredict(e, words);
						break;

					case "votepreset":
						StartVote(new VotePreset(tas, spring, this), e, words);
						break;

					case "fix":
						ComFix(e, words);
						break;

					case "rehost":
						ComRehost(e, words);
						break;

					case "voterehost":
						StartVote(new VoteRehost(tas, spring, this), e, words);
						break;

					case "random":
						ComRandom(e, words);
						break;

					case "balance":
						ComBalance(e, words);
						break;

					case "setlevel":
						ComSetLevel(e, words);
						break;

					case "setcommandlevel":
						ComSetCommandLevel(e, words);
						break;

					case "say":
						ComSay(e, words);
						break;

					case "id":
						ComTeam(e, words);
						break;

					case "team":
						ComAlly(e, words);
						break;

					case "helpall":
						ComHelpAll(e, words);
						break;

					case "fixcolors":
						ComFixColors(e, words);
						break;

					case "teamcolors":
						ComTeamColors(e, words);
						break;

					case "springie":
						ComSpringie(e, words);
						break;

					case "endvote":
						StopVote();
						SayBattle("poll cancelled");
						break;

					case "addbox":
						ComAddBox(e, words);
						break;

					case "clearbox":
						ComClearBox(e, words);
						break;

					case "listpresets":
						ComListPresets(e, words);
						break;

					case "presetdetails":
						ComPresetDetails(e, words);
						break;

					case "preset":
						ComPreset(e, words);
						break;

					case "cbalance":
						ComCBalance(e, words);
						break;

					case "listbans":
						banList.ComListBans(e, words);
						break;

					case "ban":
						banList.ComBan(e, words);
						break;

					case "unban":
						banList.ComUnban(e, words);
						break;

					case "smurfs":
						RemoteCommand(Stats.smurfScript, e, words);
						break;

					case "stats":
						RemoteCommand(Stats.statsScript, e, words);
						break;

					case "kickspec":
						ComKickSpec(e, words);
						break;

					case "manage":
						ComManage(e, words, false);
						break;

					case "cmanage":
						ComManage(e, words, true);
						break;

					case "notify":
						ComNotify(e, words);
						break;

					case "votekickspec":
						StartVote(new VoteKickSpec(tas, spring, this), e, words);
						break;

					case "boss":
						ComBoss(e, words);
						break;

					case "voteboss":
						StartVote(new VoteBoss(tas, spring, this), e, words);
						break;

					case "setpassword":
						ComSetPassword(e, words);
						break;

					case "setgametitle":
						ComSetGameTitle(e, words);
						break;

					case "setminrank":
						ComSetMinRank(e, words);
						break;

					case "setmaxplayers":
						ComSetMaxPlayers(e, words);
						break;

					case "mincpuspeed":
						ComSetMinCpuSpeed(e, words);
						break;

					case "autolock":
						ComAutoLock(e, words);
						break;

					case "spec":
						ComForceSpectator(e, words);
						break;

					case "specafk":
						ComForceSpectatorAfk(e, words);
						break;

					case "kickminrank":
						ComKickMinRank(e, words);
						break;

					case "cheats":
						if (spring.IsRunning)
						{
							spring.SayGame("/cheat");
							SayBattle("Cheats!");
						}
						else Respond(e, "Cannot set cheats, game not running");
						break;

					case "listoptions":
						ComListOptions(e, words);
						break;

					case "setoptions":
						ComSetOption(e, words);
						break;

					case "votesetoptions":
						StartVote(new VoteSetOptions(tas, spring, this), e, words);
						break;

					case "listplanets":
						if (PlanetWars != null) PlanetWars.ComListPlanets(e, words);
						else Respond(e, "Not a PlanetWars server");
						break;

					case "register":
						if (PlanetWars != null) PlanetWars.ComRegister(e, words);
						else Respond(e, "Not a PlanetWars autohost");
						break;

					case "merc":
						if (PlanetWars != null) PlanetWars.ComMerc(e, words);
						else Respond(e, "Not a PlanetWars autohost");
						break;

					case "planet":
					case "attack":
						if (PlanetWars != null) PlanetWars.ComPlanet(e, words);
						else Respond(e, "Not a PlanetWars server");
						break;

					case "setpwserver":
						if (words.Length < 1) Respond(e, "Specify address");
						else
						{
							// hack this is just debug, remove this later
							Program.main.Config.PlanetWarsServer = words[0];
							InitializePlanetWarsServer();
							Respond(e, "Planetwars server changed to " + words[0]);
						}
						break;

					case "spawn":
					{
						var args = Utils.Glue(words);
						if (string.IsNullOrEmpty(args))
						{
							Respond(e, "Please specify parameters");
							return;
						}
						var configKeys = new Dictionary<string, string>();
						foreach (var f in args.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
						{
							var parts = f.Split('=');
							if (parts.Length == 2) configKeys[parts[0].Trim()] = parts[1].Trim();
						}
						var sc = new SpawnConfig(e.UserName, configKeys);
						if (string.IsNullOrEmpty(sc.Mod))
						{
							Respond(e, "Please specify at least mod name: !spawn mod=ca:stable");
							return;
						}
						Program.main.SpawnAutoHost(configPath, sc);
					}
						break;

					case "voteplanet":
					case "voteattack":
						if (config.PlanetWarsEnabled) StartVote(new VotePlanet(tas, spring, this), e, words);
						else Respond(e, "PlanetWars not enabled on this host");
						break;
					case "resetpassword":
						if (PlanetWars != null) PlanetWars.ComResetPassword(e);
						else Respond(e, "This is not PlanetWars autohost");

						break;
				}
			}
		}


		void tas_UserStatusChanged(object sender, TasEventArgs e)
		{
			if (spring.IsRunning)
			{
				var b = tas.MyBattle;
				if (e.ServerParams[0] != tas.UserName && b.Users.Any(x => x.Name == e.ServerParams[0])) CheckForBattleExit();
			}
		}

	}
}