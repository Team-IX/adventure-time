using System;
using System.Collections.Generic;
using System.Linq;

namespace ggj2015
{
	public class SharedControlsManager
	{
		private readonly Dictionary<int, Player> _idLookup = new Dictionary<int, Player>();
		private readonly List<PlayerPerson> _playerPersons = new List<PlayerPerson>();

		private int _personIdCounter = 0;

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
				_personIdCounter++;
				_idLookup[_personIdCounter] = p;
				var pp = new PlayerPerson(p, _personIdCounter);
				_playerPersons.Add(pp);
				return pp;
			}
		}

		public void Update(ControlPacket packet)
		{
			_idLookup[packet.Id].ConsumePacket(packet);
		}

		public void EverybodySwap()
		{
			if (!Globals.Simulation.Players.All(x => x.IsAlive))
				return;
			foreach (var pp in _playerPersons)
			{
				if (!pp.Player.IsAlive)
					continue;

				//Get a random player that is alive
				do
				{
					pp.Player = Globals.Simulation.Players[Globals.Random.Next(0, Player.Count)];
				} while (!pp.Player.IsAlive);

				_idLookup[pp.Id] = pp.Player;
				pp.RaiseColorChanged();
			}
		}
	}

	public class PlayerPerson
	{
		public Player Player { get; set; }
		public int Id { get; set; }

		public string Color
		{
			get { return Player.Color; }
		}

		public PlayerPerson(Player player, int id)
		{
			Player = player;
			Id = id;
		}

		public void RaiseColorChanged()
		{
			if (ColorChanged != null)
				ColorChanged();
		}

		public event Action ColorChanged;
	}
}
