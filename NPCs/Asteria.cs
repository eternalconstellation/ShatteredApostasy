using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using ShatteredApostasy.Systems;
using SteelSeries.GameSense;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ShatteredApostasy.NPCs
{
	[AutoloadBossHead]
	public class Asteria : ModNPC
	{
 
		// Dodge
		private Vector2 totalDistance = Vector2.Zero;
		private int projectileCount = 0;

		int lastFriendlyProjectileTime = 0;

		// Movement
		int movementTimer = 0;
		bool isMovingPlayer = false;
        bool shouldStopMoving = false;
		int checkMovementTimer = 0;

        public NPCState currentState = NPCState.Idle;
		public NPCState previousState = NPCState.Idle;

		float transitionDuration = 1.0f; 
		float elapsedTime = 0f;
		Vector2 initialVelocity;
		Vector2 targetVelocity;
        private float maxThresholdDistance = 500f;

        private float lockRadius = 300;
		private int currentDelay = 0;
		private float oldVelocity = 0;
		private bool hasRun = false;
		private int currentDuration;

		private int previousMouseX = 0;
		private int previousMouseY = 0;

		float acceleration = 0.1f; // Acceleration for movement
		float invertedAcceleration = -0.1f;

		// Flight
		private int flightTimer = 0;
		private float offsetX = 0f;
		private float amplitude = 10f; // Amplitude of the sine wave
		private float frequency = 0.4f; // Frequency of the sine wave
		private float verticalMovement = 0f;


        public enum NPCState
		{
			Idle,
			GoingUp,
			GoingDown
		}

		public override void SetDefaults()
		{
			NPC.friendly = false;
			NPC.lifeMax = 214748364;
			NPC.defense = 10000000;
			NPC.boss = true;

			NPC.damage = (int)(Main.LocalPlayer.statLifeMax2 * 0.2f);

			NPC.knockBackResist = 0f;
			NPC.noGravity = true;
			NPC.noTileCollide = true;
			NPC.width = 100;
			NPC.height = 100;

			NPC.buffImmune = NPCID.Sets.ImmuneToAllBuffs;
		}

		public override void AI()
		{
			if (currentState != NPCState.Idle)
			{
				movementTimer++;
			}
			flightTimer++;
			lastFriendlyProjectileTime++;

			shouldStopMoving = false;

            previousState = currentState;

			// We continuously set the damage and life (not the max health)
			NPC.damage = (int)(Main.LocalPlayer.statLifeMax2 * 0.2f); // This will account for the player somehow gaining more max health while Asteria is alive
			NPC.life = 214748364;

			Player player = Main.LocalPlayer;

			bool isToRightOfPlayer = NPC.Center.X < player.Center.X;

			if (isToRightOfPlayer)
			{
				// Flip the sprite horizontally
				NPC.spriteDirection = -1; // Set spriteDirection to -1 to flip the sprite horizontally
			}
			else
			{
				// Reset the sprite direction to its default value
				NPC.spriteDirection = 1; // Set spriteDirection to 1 to reset the sprite to its default orientation
			}

			Vector2 mousePos = Main.MouseWorld;

			// Calculate intersection point of the ray (player's center to cursor)
			Vector2 playerToCursor = mousePos - player.Center;

			playerToCursor.Normalize();

			/*// Calculate perpendicular line passing through player's center
				Vector2 perpendicularLine = new Vector2(-playerToCursorIn.Y, playerToCursorIn.X);

				// Move Asteria to a position on the other side
				Vector2 newPosition = playerIn.Center + perpendicularLine * 500;*/

			if (movementTimer > 1)
			{
				acceleration += 1f;

				movementTimer = 0;
			}
			if (flightTimer > 30)
			{
				frequency = Main.rand.NextFloat(0.3f, 0.4f);
				amplitude = Main.rand.NextFloat(1f, 7f);
				flightTimer = 0;
			}

			offsetX += 1;
			float flyingWobble = (float)(amplitude * Math.Sin(frequency * offsetX));

			// My idea is to get various preset states and then continuesly apply them depending on a set of conditions, here just
			// we check for mouse direction so thats that
			int currentMouseX = Main.mouseX;
			int currentMouseY = Main.mouseY;

			if (ModContent.GetInstance<KeybindSystem>().DebugResetVelocity.JustPressed)
			{
				NPC.velocity = new Vector2(0, 0);
				acceleration = 0.0f;
			}

			if (ModContent.GetInstance<KeybindSystem>().DebugGoUP.JustPressed)
			{
				currentState = NPCState.GoingUp;
				acceleration = 0;
			}
			else if (ModContent.GetInstance<KeybindSystem>().DebugGoDOWN.JustPressed)
			{
				currentState = NPCState.GoingDown;
				acceleration = 0;
			}
			else if (ModContent.GetInstance<KeybindSystem>().DebugStayStill.JustPressed)
			{
				currentState = NPCState.Idle;
				acceleration = 0;
			}

			switch (currentState)
			{
				case NPCState.Idle:
					//NPC.Center = new Vector2(player.Center.X + 500, player.Center.Y + flyingWobble);
					break;
				case NPCState.GoingUp:
					NPC.velocity += new Vector2(0, -acceleration);
					break;
				case NPCState.GoingDown:
					NPC.velocity += new Vector2(0, acceleration);
					break;
			}

			// Update NPC's position based on its velocity
			NPC.position += NPC.velocity;

			bool isOutOfScreen = 

				NPC.Center.X < Main.screenWidth || 

				NPC.Center.Y < Main.screenHeight;

			Main.NewText("npc's center Y is: " + NPC.Center.Y + ", screen height is: " + Main.screenHeight);

			// Configurable parameters
			float detectionRadius = 300f;
			float minMagnitude = 5f;
			float maxMagnitude = 20f;
			float yProximityThreshold = 200f;

			foreach (Projectile projectile in Main.projectile.Where(p => p.active && p.friendly && p.owner == Main.myPlayer))
			{
				Vector2 relativePosition = new Vector2(projectile.Center.X, projectile.Center.Y) - new Vector2(NPC.Center.X, NPC.Center.Y);
				if (relativePosition.Length() < detectionRadius)
				{
					lastFriendlyProjectileTime = 0;
					// Calculate proportional magnitude
					float proportionalMagnitude = MathHelper.Lerp(minMagnitude, maxMagnitude, 1 - (relativePosition.Length() / detectionRadius));
					
					// Calculate perpendicular direction safely
					Vector2 perpendicularDirection = new Vector2(relativePosition.Y, -relativePosition.X);
					Main.NewText(projectile.Center.Y);
					if (perpendicularDirection != Vector2.Zero)
					{
						perpendicularDirection.Normalize();

						// Determine if bullet is above or below NPC
						if (Math.Abs(projectile.position.Y - NPC.position.Y) <= yProximityThreshold)
						{
							NPC.velocity = perpendicularDirection * proportionalMagnitude;
						}
						else
						{
							NPC.velocity = perpendicularDirection * proportionalMagnitude;
						}
					}
					
					if (isOutOfScreen)
					{
						Main.NewText("Is out of screen?: " + isOutOfScreen);
						break; // we break to go to the other condition for this same thing below
					}

				}
			}

            float thresholdDistance = 300f; // Set the threshold distance
            float speed = 15f; // Adjust the desired speed of the NPC

            float distance = Vector2.Distance(Main.LocalPlayer.Center, NPC.Center);

			if (distance <= thresholdDistance)
			{
				isMovingPlayer = true;
				Vector2 directionToPlayer = Main.LocalPlayer.Center - NPC.Center; // Assuming player index 0
				directionToPlayer.Normalize(); // Normalize the direction vector

				// Apply acceleration to the normalized direction vector
				Vector2 accelerationVector = directionToPlayer * 0.1f;

				// Update the NPC's position based on the accelerated direction vector and speed
				NPC.velocity -= accelerationVector * speed;
			}
			else
				isMovingPlayer = false;

            if (distance >= maxThresholdDistance - 5)
			{
                if (NPC.velocity.Y != 0)
                {
                    NPC.velocity = NPC.velocity / 1.2f;
                }
                else
                    NPC.velocity = new(0, 0);
            }

			//TODO: check if velocity is the same for a bit and then move npc

            // sets position
            if (lastFriendlyProjectileTime > 1 && previousMouseX != currentMouseX && previousMouseY != currentMouseY)
			{
				
                // Assuming playerCenter represents the player's center position vector
                Vector2 playerCenter = new Vector2(player.Center.X, player.Center.Y);

				// Assuming mousePosition represents the mouse position vector in the game world
				Vector2 mousePosition = new Vector2(Main.MouseWorld.X, Main.MouseWorld.Y);

				// Calculate the direction from player's center to mouse position
				Vector2 directionToMouse = mousePosition - playerCenter;
				directionToMouse.Normalize(); // Normalize the direction vector

				// Calculate the perpendicular direction (90-degree clockwise rotation)
				Vector2 perpendicularDirection = new Vector2(-directionToMouse.Y / 2, directionToMouse.X / 2);

				// Define the distance to position the center point from the player
				float distanceFromPlayer = 500f; // Adjust as needed

				// Calculate the center point that is 90 degrees perpendicular to the mouse position
				Vector2 perpendicularCenter = playerCenter + perpendicularDirection * distanceFromPlayer;

				// Calculate the direction towards the target point
				Vector2 directionToTarget = new Vector2(perpendicularCenter.X, perpendicularCenter.Y) - NPC.Center;
				directionToTarget.Normalize(); // Normalize the vector to get the direction

				// Define the speed at which the interpolation occurs
				float interpolationSpeed = 0.5f; // Adjust as needed

				// Interpolate from the current velocity to the required velocity
				Vector2 targetVelocity = directionToTarget * 10f; // Define the maximum speed based on the game's requirements
				Vector2 lerpedVelocity = Vector2.Lerp(NPC.velocity, targetVelocity, interpolationSpeed);
				NPC.velocity.Y = (float)Math.Ceiling(lerpedVelocity.Y);
				NPC.velocity.X = (float)Math.Ceiling(lerpedVelocity.X);// Perform linear interpolation
			}
			/*else if (previousMouseY == currentMouseY && previousMouseX == currentMouseX && isMovingPlayer == false && shouldStopMoving == true)
            {
				if (NPC.velocity.Y != 0)
				{
					NPC.velocity = NPC.velocity / 1.1f;
				}
				else
					NPC.velocity = new(0, 0);
            }*/
		   



			// Update the previous mouse position for the next comparison
			previousMouseX = currentMouseX;
			previousMouseY = currentMouseY;

			hasRun = true;

			oldVelocity = acceleration;
		}
		public static float QuadraticFunction(float xConstant, float quadraticTerm, float linearTerm, float constantTerm)
		{
			return quadraticTerm * xConstant * xConstant + linearTerm * xConstant + constantTerm;
		}
	}
}
