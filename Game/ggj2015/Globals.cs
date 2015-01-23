﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FarseerPhysics.Dynamics;
using Nuclex.Input;

namespace ggj2015
{
	static class Globals
	{
		public const int RenderWidth = 1280;
		public const int RenderHeight = 720;

		public static World World;
		public static InputManager Input;

		public static SharedControlsManager Controls { get; set; }
		public static WebServer WebServer { get; set; }
		public static GameWorld GameWorld { get; set; }
		public static Simulation Simulation { get; set; }
	}
}
