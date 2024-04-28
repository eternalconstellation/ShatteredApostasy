using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ShatteredApostasy.Content.NPCs
{
    [AutoloadBossHead]
    public class Asteria : ModNPC
    {
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
            NPC.width = 256;
            NPC.height = 256;

            NPC.buffImmune = NPCID.Sets.ImmuneToAllBuffs;
        }
        public override void AI() //Remember, this method updates every tick
        {
            // We continuesly set the damage and life (not the max health)
            NPC.damage = (int)(Main.LocalPlayer.statLifeMax2 * 0.2f); //This will account for the player somehow gaining more max health while asteria is alive
            NPC.life = 214748364;
        }
    }
}
