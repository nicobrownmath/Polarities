using Polarities.NPCs;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Polarities.Buffs
{
    public class Desiccating : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.pvpBuff[Type] = true;
            Main.buffNoSave[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            player.GetModPlayer<PolaritiesPlayer>().desiccation = player.buffTime[buffIndex];
            if (player.buffTime[buffIndex] > 60 * 10)
            {
                player.confused = !player.HasBuff(BuffID.Confused);
            }
        }

        public override void Update(NPC npc, ref int buffIndex)
        {
            npc.GetGlobalNPC<PolaritiesNPC>().desiccation = npc.buffTime[buffIndex];
            if (!npc.buffImmune[BuffID.Confused] && npc.buffTime[buffIndex] > 60 * 10)
            {
                npc.confused = true;
            }
        }

        public override bool ReApply(Player player, int time, int buffIndex)
        {
            player.buffTime[buffIndex] += time;
            if (player.buffTime[buffIndex] > 20 * 60)
            {
                player.buffTime[buffIndex] = 20 * 60;
            }
            return false;
        }

        public override bool ReApply(NPC npc, int time, int buffIndex)
        {
            npc.buffTime[buffIndex] += time;
            if (npc.buffTime[buffIndex] > 20 * 60)
            {
                npc.buffTime[buffIndex] = 20 * 60;
            }
            return false;
        }
    }
}