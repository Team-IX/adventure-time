using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ggj2015
{
	public enum Control
	{
		Down,
		Up,
		Left,
		Right,

		Bomb
	}

	public class ControlPacket
	{
		public int Id;
		public Control[] Controls;
	}
}
