using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace ggj2015
{
	public class SharedControlsManager
	{
		private readonly Dictionary<string, PlayerPerson> _idLookup = new Dictionary<string, PlayerPerson>();

		private readonly ManualResetEventSlim _longPollReset = new ManualResetEventSlim(false);

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
					var minCount = Globals.Simulation.Players.Min(x => x.PersonCount);
					p = Globals.Simulation.Players.First(x => x.PersonCount == minCount);
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
				_idLookup[packet.Id].Player.ConsumePacket(packet);
			}
		}

		public void EverybodySwap()
		{
			if (Globals.Simulation.Players.Count(x => x.IsAlive) < 2)
				return;

			lock (this)
			{
				foreach (var p in Globals.Simulation.Players)
				{
					p.ResetVotes();
				}

				foreach (var pp in _idLookup.Values)
				{
					if (!pp.Player.IsAlive)
						continue;

					//Get a random player that is alive
					var startingPlayer = pp.Player;
					do
					{
						pp.Player = Globals.Simulation.Players[Globals.Random.Next(0, Player.Count)];
					} while (!pp.Player.IsAlive || pp.Player == startingPlayer);

					pp.RaiseColorChanged();
				}
			}

			_longPollReset.Set();
		}

		public object StatusPoll(BasePacket packet)
		{
			var pp = _idLookup[packet.Id];
			pp.ResetEvent.Wait(TimeSpan.FromSeconds(10));
			pp.ResetEvent.Reset();

			return new { color = pp.Color };
		}

		public void Reset()
		{
			_idLookup.Clear();
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

		public PlayerPerson(Player player, string id)
		{
			Player = player;
			Id = id;
		}

		public void RaiseColorChanged()
		{
			ResetEvent.Set();
		}
	}
}
