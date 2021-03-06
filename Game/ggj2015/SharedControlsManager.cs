﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace ggj2015
{
	public class SharedControlsManager
	{
		private readonly Dictionary<string, PlayerPerson> _idLookup = new Dictionary<string, PlayerPerson>();

		public SharedControlsManager()
		{
		}


		public PlayerPerson Join( int? playerNumber = null)
		{
			lock (this)
			{
				Player p;
				if (playerNumber.HasValue)
				{
					p = Globals.Simulation.Players[playerNumber.Value];
				}
				else
				{
					p = Globals.Simulation.Players.Where(x => x.IsAlive).OrderBy(x => x.PersonCount).FirstOrDefault();
					if (p == null)
						throw new Exception();
				}

				p.PersonCount++;
				var id = Guid.NewGuid().ToString();
				var pp = new PlayerPerson(p, id);
				_idLookup[id] = pp;
				return pp;
			}
		}

		public void Update(ControlPacket packet)
		{
			lock (this)
			{
				var pp = _idLookup[packet.Id];
				pp.LastContact = Globals.GameTime.TotalGameTime;
				pp.Player.ConsumePacket(packet);
			}
		}

		public void CheckForTimeouts()
		{
			lock (this)
			{
				foreach (var kvp in _idLookup.ToArray())
				{
					var pp = kvp.Value;
					if (pp.LastContact < Globals.GameTime.TotalGameTime - TimeSpan.FromSeconds(5) && !pp.LongPollActive)
					{
						_idLookup.Remove(kvp.Key);
						Console.WriteLine("timeout 1");
					}
					else if (pp.LongPollActive && pp.LastContact < Globals.GameTime.TotalGameTime - TimeSpan.FromSeconds(15))
					{
						_idLookup.Remove(kvp.Key);
						Console.WriteLine("timeout 2");
					}
				}
			}
		}

		public void EverybodySwap(bool fullyRandom = false)
		{
			lock (this)
			{
				if (Globals.Simulation.Players.Count(x => x.IsAlive) < 2)
					return;

				foreach (var p in Globals.Simulation.Players)
				{
					p.ResetVotes();
				}

				foreach (var pp in _idLookup.Values)
				{
					//Get a random player that is alive
					var startingPlayer = fullyRandom ? null : pp.Player;

					var possiblePlayers = Globals.Simulation.Players.Where(x => x.IsAlive && x != startingPlayer).ToArray();
					if (possiblePlayers.Length > 0)
					{
						pp.Player = possiblePlayers[Globals.Random.Next(0, possiblePlayers.Length)];

						pp.RaiseColorChanged();
					}
				}
			}
		}

		public object StatusPoll(BasePacket packet)
		{
			var pp = _idLookup[packet.Id];
			pp.LongPollActive = true;
			pp.ResetEvent.Wait(TimeSpan.FromSeconds(10));
			pp.LastContact = Globals.GameTime.TotalGameTime;
			pp.LongPollActive = false;
			pp.ResetEvent.Reset();

			if (pp.ForceDisconnect)
				throw new Exception();

			return new { color = pp.Color };
		}

		public void Stop()
		{
			foreach (var pp in _idLookup.Values)
				pp.ResetEvent.Set();
		}

		public int CurrentCount
		{
			get { return _idLookup.Count; }
		}

		public void Reset()
		{
			lock (this)
			{
				foreach (var pp in _idLookup.Values)
				{
					pp.ForceDisconnect = true;
					pp.ResetEvent.Set();
				}
				_idLookup.Clear();
			}
		}

		public void ReallocatePlayer(Player player)
		{
			if (Globals.Simulation.Players.Count(x => x.IsAlive) < 2)
				return;

			lock (this)
			{
				foreach (var pp in _idLookup.Values)
				{
					if (pp.Player != player)
						continue;

					var possiblePlayers = Globals.Simulation.Players.Where(x => x.IsAlive).ToArray();
					if (possiblePlayers.Length > 0)
					{
						pp.Player = possiblePlayers[Globals.Random.Next(0, possiblePlayers.Length)];

						pp.RaiseColorChanged();
					}
				}
			}
		}

		public int CountPeopleForPlayer(Player player)
		{
			lock (this)
			{
				return _idLookup.Count(x => x.Value.Player == player);
			}
		}
	}

	public class PlayerPerson
	{
		public Player Player { get; set; }
		public string Id { get; set; }

		public readonly ManualResetEventSlim ResetEvent = new ManualResetEventSlim(false);

		public string Color
		{
			get { return Player.ColorStr; }
		}

		public TimeSpan LastContact;

		public bool LongPollActive { get; set; }
		public bool ForceDisconnect { get; set; }

		public PlayerPerson(Player player, string id)
		{
			Player = player;
			Id = id;
			LastContact = Globals.GameTime == null ? TimeSpan.Zero : Globals.GameTime.TotalGameTime;
		}

		public void RaiseColorChanged()
		{
			ResetEvent.Set();
		}
	}
}
