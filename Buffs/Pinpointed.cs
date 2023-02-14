using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace Polarities.Buffs
{
    public class Pinpointed : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.pvpBuff[Type] = true;
            Main.buffNoSave[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            if (player.buffTime[buffIndex] % 10 == 0)
            {
                int numDusts = 20;
                for (int i = 0; i < numDusts; i++)
                {
                    int dust = Dust.NewDust(player.Center, 0, 0, 92, Scale: 1.5f);
                    Main.dust[dust].noGravity = true;
                    Main.dust[dust].noLight = true;
                    Main.dust[dust].velocity = new Vector2(4, 0).RotatedBy(i * MathHelper.TwoPi / numDusts);
                }
            }
        }

        public override void Update(NPC npc, ref int buffIndex)
        {
            if (npc.buffTime[buffIndex] % 10 == 0)
            {
                int numDusts = 20;
                for (int i = 0; i < numDusts; i++)
                {
                    int dust = Dust.NewDust(npc.Center, 0, 0, 92, Scale: 1.5f);
                    Main.dust[dust].noGravity = true;
                    Main.dust[dust].noLight = true;
                    Main.dust[dust].velocity = new Vector2(4, 0).RotatedBy(i * MathHelper.TwoPi / numDusts);
                }
            }
        }
    }
}