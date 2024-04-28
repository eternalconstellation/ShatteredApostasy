using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace ShatteredApostasy.NPCs
{
    public class Asteria : ModNPC
    {
        public override void SetDefaults()
        {
            NPC.boss = true;
            NPC.friendly = false;
            NPC.width = 256;
            NPC.height = 256;
            NPC.aiStyle = 7;
            NPC.lifeMax = 214748364;
            NPC.defense = 10000000;
            NPC.damage = 1;
        }

        public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers)
        {
            float asteriaContactDamage = target.statLifeMax2 * 0.2f;
            modifiers.SourceDamage.Base = asteriaContactDamage;
        }
    }
    
}
