using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FarseerPhysics.Dynamics;

namespace ggj2015.GameObjects
{
	class UnbreakableWall : GameObject
	{
		public UnbreakableWall(Body body)
		{
			Body = body;
		}
	}
}
