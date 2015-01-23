using System.Collections.Generic;
using System.Linq;

namespace ggj2015
{
	public class SharedControlsManager
	{
		private Player[] _players;
		private Dictionary<int, Player> _idLookup = new Dictionary<int, Player>();

		private int _personIdCounter = 0;

		public SharedControlsManager(int playerCount)
		{
			var colors = new[]
			{
				"#ff0000",
				"#00ff00",
				"#0000ff",
				"#ffffff"
			};

			_players = new Player[playerCount];
			for (var i = 0; i < playerCount; i++)
				_players[i] = new Player(i, colors[i]);
		}


		public PlayerPerson Join(int? playerNumber = null)
		{
			lock (this)
			{
				Player p;
				if (playerNumber.HasValue)
				{
					p = _players[playerNumber.Value];
				}
				else
				{
					var minCount = _players.Min(x => x.PersonCount);
					p = _players.First(x => x.PersonCount == minCount);
				}

				p.PersonCount++;
				_personIdCounter++;
				_idLookup[_personIdCounter] = p;
				return new PlayerPerson(p, _personIdCounter, p.Color);
			}
		}

		public void Update(ControlPacket packet)
		{
			_idLookup[packet.Id].Update(packet);
		}
	}

	public class PlayerPerson
	{
		public Player Player { get; set; }
		public int Id { get; set; }
		public string Color { get; set; }

		public PlayerPerson(Player player, int id, string color)
		{
			Player = player;
			Id = id;
			Color = color;
		}
	}
}
