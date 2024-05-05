using Microsoft.Xna.Framework;
using ShatteredApostasy.Systems;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ShatteredApostasy.NPCs
{
    [AutoloadBossHead]
    public class Asteria : ModNPC
    {
        private enum NPCState
        {
            Idle,
            GoingUp,
            GoingDown
        }

        // Movement
        private NPCState currentState = NPCState.Idle;
        private float acceleration = 0.1f; // Base acceleration

        // Flight
        private int flightTimer = 0;
        private float offsetX = 0f;
        private float amplitude = 10f;
        private float frequency = 0.4f;

        public override void SetDefaults()
        {
            NPC.lifeMax = int.MaxValue;
            NPC.defense = 10000000;
            NPC.knockBackResist = 0f;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.width = 100;
            NPC.height = 100;
            NPC.buffImmune = NPCID.Sets.ImmuneToAllBuffs;
            NPC.boss = true;
        }

        public override void AI()
        {
            Player player = Main.LocalPlayer;
            AdjustDamageBasedOnPlayer();
            HandleNPCOrientation(player);
            UpdateMovements();
            CheckForCommands();
            HandleStates();
            ConsiderProjectileAvoidance();
        }

        private void AdjustDamageBasedOnPlayer()
        {
            NPC.damage = (int)(Main.LocalPlayer.statLifeMax2 * 0.2f);
        }

        private void HandleNPCOrientation(Player player)
        {
            NPC.spriteDirection = NPC.Center.X < player.Center.X ? -1 : 1;
        }

        private void UpdateMovements()
        {
            flightTimer++;
            if (flightTimer > 30)
            {
                frequency = Main.rand.NextFloat(0.3f, 0.4f);
                amplitude = Main.rand.NextFloat(1f, 7f);
                flightTimer = 0;

                // Add vertical oscillation for flying
                float flyingWobble = (float)(amplitude * Math.Sin(frequency * offsetX));
                NPC.position.Y += flyingWobble;
                offsetX += 1;
            }
        }

        private void CheckForCommands()
        {
            if (ModContent.GetInstance<KeybindSystem>().DebugResetVelocity.JustPressed)
            {
                NPC.velocity = Vector2.Zero;
                acceleration = 0f;
            }

            // Movement commands
            if (ModContent.GetInstance<KeybindSystem>().DebugGoUP.JustPressed)
            {
                currentState = NPCState.GoingUp;
            }
            else if (ModContent.GetInstance<KeybindSystem>().DebugGoDOWN.JustPressed)
            {
                currentState = NPCState.GoingDown;
            }
            else if (ModContent.GetInstance<KeybindSystem>().DebugStayStill.JustPressed)
            {
                currentState = NPCState.Idle;
            }
        }

        private void HandleStates()
        {
            switch (currentState)
            {
                case NPCState.GoingUp:
                    NPC.velocity.Y -= acceleration;
                    break;
                case NPCState.GoingDown:
                    NPC.velocity.Y += acceleration;
                    break;
            }
        }

        private void ConsiderProjectileAvoidance()
        {
            // Dodge friendly projectiles
            foreach (Projectile projectile in Main.projectile.Where(p => p.active && p.friendly && p.owner == Main.myPlayer))
            {
                float detectionRadius = 300f;
                Vector2 projectilePosition = projectile.Center;
                Vector2 directionFromProjectile = NPC.Center - projectilePosition;
                float distance = directionFromProjectile.Length();

                if (distance < detectionRadius)
                {
                    Vector2 dodgeDirection = directionFromProjectile.SafeNormalize(Vector2.Zero);
                    NPC.velocity += dodgeDirection * (1 - (distance / detectionRadius)) * 10f;
                }
            }
        }
    }
}
