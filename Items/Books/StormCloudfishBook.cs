using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Polarities.Items.Books
{
    public class StormCloudfishBook : BookBase
    {
        public override int BuffType => BuffType<StormCloudfishBookBuff>();
        public override int BookIndex => 3;
    }

    public class StormCloudfishBookBuff : BookBuffBase
    {
        public override int ItemType => ItemType<StormCloudfishBook>();

        public override void Update(Player player, ref int buffIndex)
        {
            if (Main.rand.NextBool(5))
            {
                float r = (float)Main.rand.NextDouble();
                float theta = (float)(2 * Math.PI * Main.rand.NextDouble());

                Projectile raindrop = Main.projectile[Projectile.NewProjectile(player.GetSource_Buff(buffIndex), player.Center.X + 50 * r * (float)Math.Cos(theta), player.Center.Y + 50 * r * (float)Math.Sin(theta) - 800, 0, (float)Main.rand.NextDouble() * 7 + 2, ProjectileID.RainFriendly, 14, 1f, Main.myPlayer)];
                raindrop.DamageType = DamageClass.Generic;
            }

            base.Update(player, ref buffIndex);
        }
    }
}