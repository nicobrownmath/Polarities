using Polarities.NPCs;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Polarities.Buffs
{
    public class Corroding : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.buffNoSave[Type] = true;
            BuffID.Sets.LongerExpertDebuff[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            player.statDefense = player.statDefense * 3 / 5;
            Main.dust[Dust.NewDust(player.position, player.width, player.height, 74, Scale: 1f)].noGravity = true;
        }

        public override void Update(NPC npc, ref int buffIndex)
        {
            npc.GetGlobalNPC<PolaritiesNPC>().defenseMultiplier *= 0.6f;
            Main.dust[Dust.NewDust(npc.position, npc.width, npc.height, 74, Scale: 1f)].noGravity = true;
        }
    }
}