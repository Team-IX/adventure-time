using System.Collections.Generic;

namespace ggj2015
{
	public class Player
	{
		public string Color { get; set; }
		public readonly int PlayerNumber;

		public int PersonCount;

		private readonly Dictionary<int, Control[]> _votes = new Dictionary<int, Control[]>(); 

		public Player(int playerNumber, string color)
		{
			Color = color;
			PlayerNumber = playerNumber;
		}

		public void Update(ControlPacket packet)
		{
			_votes[packet.Id] = packet.Controls;
		}
	}
}
