using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
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

namespace ShatteredApostasy.Content.NPCs
{
    [AutoloadBossHead]
    public class Asteria : ModNPC
    {
        // Dodge
        private Vector2 totalDistance = Vector2.Zero;
        private int projectileCount = 0;

        // Movement
        private int movementTimer;

        private float lockRadius = 300;
        private int currentDelay = 0;
        private Vector2 oldPosition = Vector2.Zero;
        private bool hasRun = false;
        private int currentDuration;

        private int previousMouseX = 0;
        private int previousMouseY = 0;

        float verticalPosition = 0;
        float initialVelocity = 2; // Initial velocity of the NPC
        float acceleration = 0.1f; // Acceleration for upward movement

        // Flight
        private int flightTimer = 0;
        private float offsetX = 0f;
        private float amplitude = 10f; // Amplitude of the sine wave
        private float frequency = 0.4f; // Frequency of the sine wave
        private float verticalMovement = 0f;
        

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
            NPC.width = 175;
            NPC.height = 200;

            NPC.buffImmune = NPCID.Sets.ImmuneToAllBuffs;
        }

        public override void AI()
        {
            movementTimer++;
            flightTimer++;
            
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

            // Calculate perpendicular line passing through player's center
            Vector2 perpendicularLine = new Vector2(-playerToCursor.Y, playerToCursor.X);

            // Move Asteria to a position on the other side
            Vector2 newPosition = player.Center + perpendicularLine * 500;

            if (flightTimer > 30)
            {
                frequency = Main.rand.NextFloat(0.3f, 0.4f);
                amplitude = Main.rand.NextFloat(1f, 7f);
                flightTimer = 0;
            }

            offsetX += 1;
            float flyingWobble = (float)(amplitude * Math.Sin(frequency * offsetX));

            ProcessMovement(newPosition, flyingWobble, movementTimer);

            hasRun = true;
            oldPosition = newPosition;
        }
        public void ProcessMovement(Vector2 newPositionIn, float wobbleIn, int timerIn)
        {
            // My idea is to get various preset states and then continuesly apply them depending on a set of conditions, here just
            // we check for mouse direction so thats that
            int currentMouseX = Main.mouseX;
            int currentMouseY = Main.mouseY;
            NPC.velocity.X *= (0.88f);
            NPC.Center = newPositionIn;

            // Update the previous mouse position for the next comparison
            previousMouseX = currentMouseX;
            previousMouseY = currentMouseY;
        }
    }
}
