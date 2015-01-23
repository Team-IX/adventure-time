﻿using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using FarseerPhysics.Collision;
using FarseerPhysics.Controllers;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Nuclex.Input.Devices;

namespace ggj2015
{
	class Simulation
	{
		public Player[] Players;

		public void InitialPopulate()
		{
			var playerCellStarts = new[]
			{
				new Vector2(0, 0),
				new Vector2(GameWorld.Width - 1, GameWorld.Height - 1),

				new Vector2(GameWorld.Width - 1, 0),
				new Vector2(0, GameWorld.Height - 1),
			};
			var colors = new[]
			{
				"#ff0000",
				"#00ff00",
				"#0000ff",
				"#ffffff"
			};

			Players = new Player[Player.Count];
			for (var i = 0; i < Player.Count; i++)
			{
				var pos = playerCellStarts[i];
				var playerBody = BodyFactory.CreateRoundedRectangle(Globals.World, 
					GameWorld.CellSize * GameWorld.PlayerScale, GameWorld.CellSize * GameWorld.PlayerScale, 
					GameWorld.PlayerRadius, GameWorld.PlayerRadius, 
					4, 0.01f, 
					new Vector2(pos.X * GameWorld.CellSize, pos.Y * GameWorld.CellSize), bodyType: BodyType.Dynamic);
				playerBody.FixedRotation = true;
				playerBody.LinearDamping = 20;
				playerBody.Friction = 0;

				//var limit = new VelocityLimitController(PlayerMaximumVelocity, 0);
				//Globals.World.AddController(limit);
				//limit.AddBody(playerBody);

				Players[i] = new Player(i, colors[i], playerBody);
			}
		}

		public void Update(GameTime gameTime)
		{
		}

		public void UpdateControls(GameTime gameTime)
		{
			var gps1 = Globals.Input.GamePads[0].GetState();

			List<Control> controls = new List<Control>();
			if (gps1.DPad.Down == ButtonState.Pressed)
				controls.Add(Control.Down);
			if (gps1.DPad.Left == ButtonState.Pressed)
				controls.Add(Control.Left);
			if (gps1.DPad.Up == ButtonState.Pressed)
				controls.Add(Control.Up);
			if (gps1.DPad.Right == ButtonState.Pressed)
				controls.Add(Control.Right);
			if (gps1.IsButtonDown(Buttons.A))
				controls.Add(Control.Bomb);

			Players[0].ConsumePacket(new ControlPacket(0, controls.ToArray()));


			Players[0].ApplyMovementForce();
		}
	}
}
