using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Polarities.Buffs
{
    public class Turbulence : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.buffNoSave[Type] = true;
            BuffID.Sets.LongerExpertDebuff[Type] = true;
        }

        public static void Update(Player player)
        {
            if (player.HasBuff<Turbulence>())
            {
                float turbulenceAmount = 0.5f;
                if (player.wet || player.honeyWet || player.lavaWet)
                {
                    turbulenceAmount = 1.5f;
                }
                player.velocity += new Vector2(turbulenceAmount, 0).RotatedByRandom(MathHelper.TwoPi);
            }
        }
    }
}