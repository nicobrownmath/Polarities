using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace Polarities.NPCs.Eclipxie
{
    public class AncientStarlight : ModProjectile
    {
        private Vector2 gravityPoint
        {
            get => new Vector2(Projectile.ai[0], Projectile.ai[1]);
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Ancient Starlight");
        }

        public override void SetDefaults()
        {
            Projectile.aiStyle = -1;
            Projectile.width = 2;
            Projectile.height = 2;
            DrawOffsetX = -28;
            DrawOriginOffsetX = 15;
            DrawOriginOffsetY = -2;
            Projectile.alpha = 0;
            Projectile.timeLeft = 600;
            Projectile.penetrate = 1;
            Projectile.hostile = true;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.light = 0.8f;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();

            Vector2 acceleration = gravityPoint - Projectile.Center;
            float distance = acceleration.Length();
            if (distance < 32)
            {
                Projectile.Kill();
            }
            acceleration /= distance * distance * distance / 7500;
            Projectile.velocity += acceleration;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Vector2 drawPosition1 = Projectile.Center;
            Vector2 drawVelocity1 = Projectile.velocity;
            Vector2 drawPosition2 = Projectile.Center;
            Vector2 drawVelocity2 = Projectile.velocity;
            for (int i = 0; i < 10; i++)
            {
                Vector2 acceleration1 = gravityPoint - drawPosition1;
                float distance1 = acceleration1.Length();
                acceleration1 /= distance1 * distance1 * distance1 / 7500;
                drawVelocity1 += acceleration1 / 4;
                drawPosition1 += drawVelocity1 / 4;

                Vector2 acceleration2 = gravityPoint - drawPosition2;
                float distance2 = acceleration2.Length();
                acceleration2 /= distance2 * distance2 * distance2 / 7500;
                drawVelocity2 -= acceleration2 / 4;
                drawPosition2 -= drawVelocity2 / 4;

                Main.spriteBatch.Draw(TextureAssets.Projectile[Projectile.type].Value, drawPosition1 - Main.screenPosition, new Rectangle(0, 0, TextureAssets.Projectile[Projectile.type].Value.Width, TextureAssets.Projectile[Projectile.type].Value.Height), Color.White * (1 - i / 10f), Projectile.rotation, new Vector2(TextureAssets.Projectile[Projectile.type].Value.Width / 2, TextureAssets.Projectile[Projectile.type].Value.Height / 2), Projectile.scale, SpriteEffects.None, 0f);
                Main.spriteBatch.Draw(TextureAssets.Projectile[Projectile.type].Value, drawPosition2 - Main.screenPosition, new Rectangle(0, 0, TextureAssets.Projectile[Projectile.type].Value.Width, TextureAssets.Projectile[Projectile.type].Value.Height), Color.White * (1 - i / 10f), Projectile.rotation, new Vector2(TextureAssets.Projectile[Projectile.type].Value.Width / 2, TextureAssets.Projectile[Projectile.type].Value.Height / 2), Projectile.scale, SpriteEffects.None, 0f);
            }
            return false;
        }
    }
}