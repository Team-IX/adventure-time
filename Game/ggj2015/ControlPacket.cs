using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ggj2015
{
	public enum Control
	{
		NoMovement,

		Down,
		Up,
		Left,
		Right,

		Bomb
	}

	public class BasePacket
	{
		public string Id;
	}

	public class ControlPacket : BasePacket
	{
		public Control[] Controls;

		public ControlPacket()
		{
		}

		public ControlPacket(string id, params Control[] controls)
		{
			Id = id;
			Controls = controls;
		}
	}
}
