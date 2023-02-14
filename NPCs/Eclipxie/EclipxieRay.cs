using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Polarities.NPCs.Eclipxie
{
    public class EclipxieRay : ModProjectile
    {
        public override string Texture => "Polarities/NPCs/SunPixie/SunPixieRay";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Solar Ray");
        }

        public override void SetDefaults()
        {
            Projectile.aiStyle = -1;
            Projectile.width = 10;
            Projectile.height = 10;
            DrawOffsetX = -54;
            DrawOriginOffsetY = 0;
            DrawOriginOffsetX = 27;
            Projectile.alpha = 0;
            Projectile.timeLeft = 600;
            Projectile.penetrate = 1;
            Projectile.hostile = true;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.light = 1f;
            Projectile.hide = true;
        }

        public override void AI()
        {
            if (Projectile.timeLeft % 8 == 7)
            {
                Dust.NewDustPerfect(Projectile.Center, 133, Vector2.Zero, Scale: 1).noGravity = true;
            }

            Projectile.rotation = Projectile.velocity.ToRotation();
        }

        public override void OnHitPlayer(Player target, int damage, bool crit)
        {
            target.AddBuff(BuffID.OnFire, 180, true);
        }

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            behindNPCs.Add(index);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            for (int i = 5; i >= 0; i--)
            {
                Main.spriteBatch.Draw(TextureAssets.Projectile[Projectile.type].Value, Projectile.Center - Projectile.velocity * (i / 6f) * 8 - Main.screenPosition, new Rectangle(0, 0, 64, 10), Color.White * (1 - i / 18f), Projectile.rotation, new Vector2(59, 5), Projectile.scale * (1 - i / 6f), SpriteEffects.None, 0f);
            }
            return false;
        }

        public override void Kill(int timeLeft)
        {
            for (int i = 0; i < 8; i++)
            {
                Main.dust[Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, 133, Scale: 1)].noGravity = true;
            }
        }
    }
}
