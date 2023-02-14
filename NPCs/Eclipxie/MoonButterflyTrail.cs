using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using Polarities.Buffs;

namespace Polarities.NPCs.Eclipxie
{
    public class MoonButterflyTrail : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Lunar Trail");
        }

        public override void SetDefaults()
        {
            Projectile.aiStyle = -1;
            Projectile.width = 64;
            Projectile.height = 64;
            Projectile.alpha = 0;
            Projectile.timeLeft = 480;
            Projectile.penetrate = 1;
            Projectile.hostile = true;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
        }

        public override void AI()
        {
            Projectile.damage = (int)(Projectile.ai[0] * (255 - Projectile.alpha) / 256f);
            Projectile.alpha = 255 * (480 - Projectile.timeLeft) / 480;

            Projectile.scale = (Projectile.alpha + 255) / 255f;
            Projectile.width = (int)(64 * Projectile.scale);
            Projectile.height = (int)(64 * Projectile.scale);
            DrawOffsetX = (int)(0.5f * (Projectile.height - 64));
            DrawOriginOffsetY = (int)(0.5f * (Projectile.height - 64));
            DrawOriginOffsetX = 0;

            Player player = Main.player[(int)Projectile.ai[1]];

            Projectile.velocity.X = (player.Center.X - Projectile.Center.X) / (float)Math.Sqrt((player.Center.X - Projectile.Center.X) * (player.Center.X - Projectile.Center.X) + (player.Center.Y - Projectile.Center.Y) * (player.Center.Y - Projectile.Center.Y));
            Projectile.velocity.Y = (player.Center.Y - Projectile.Center.Y) / (float)Math.Sqrt((player.Center.X - Projectile.Center.X) * (player.Center.X - Projectile.Center.X) + (player.Center.Y - Projectile.Center.Y) * (player.Center.Y - Projectile.Center.Y));
        }

        public override void OnHitPlayer(Player target, int damage, bool crit)
        {
            target.AddBuff(BuffID.Frostburn, 300, true);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, new Rectangle(0, Projectile.frame * texture.Height, texture.Width, texture.Height), Color.White * ((255 - Projectile.alpha) / 256f), Projectile.rotation, texture.Size() / 2, Projectile.scale, SpriteEffects.None, 0f);
            return false;
        }
    }
}