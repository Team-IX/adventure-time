using System.Collections.Generic;
using System.Linq;

namespace ggj2015
{
	public class SharedControlsManager
	{
		private readonly Dictionary<int, Player> _idLookup = new Dictionary<int, Player>();

		private int _personIdCounter = 0;

		public SharedControlsManager()
		{
		}


		public PlayerPerson Join(int? playerNumber = null)
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
				return new PlayerPerson(p, _personIdCounter, p.Color);
			}
		}

		public void Update(ControlPacket packet)
		{
			_idLookup[packet.Id].ConsumePacket(packet);
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
