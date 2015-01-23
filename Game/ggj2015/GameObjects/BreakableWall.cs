using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FarseerPhysics.Dynamics;

namespace ggj2015.GameObjects
{
	class BreakableWall : GameObject
	{
		public Body Body { get; set; }

		public BreakableWall(Body body)
		{
			Body = body;
		}
	}
}
