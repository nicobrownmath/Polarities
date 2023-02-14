using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Polarities.NPCs.Eclipxie
{
    public class EclipseFlare : ModProjectile
    {
        private float Distance;
        private int frame;
        private int timer;
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Eclipse Flare");
        }

        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.hostile = true;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.hide = false;
            Projectile.timeLeft = 60;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            DrawLaser(Main.spriteBatch, TextureAssets.Projectile[Projectile.type].Value, Projectile.Center, new Vector2(0, -1), 32, Projectile.damage, (float)Math.PI / 2);
            return false;
        }

        // The core function of drawing a laser
        public void DrawLaser(SpriteBatch spriteBatch, Texture2D texture, Vector2 start, Vector2 unit, float step, int damage, float rotation = 0f, float scale = 1f, float maxDist = 2000f, Color color = default(Color), int transDist = 0)
        {
            float r = unit.ToRotation() + rotation;

            // Draws the laser 'body'
            for (float i = transDist + step; i <= Distance; i += step)
            {
                Color c = Color.White;
                var origin = start + i * unit;
                spriteBatch.Draw(texture, origin - Main.screenPosition + new Vector2(0, Projectile.ai[1]),
                    new Rectangle(0, 32 * frame, 32, 32), i < transDist ? Color.Transparent : c, r,
                    new Vector2(32 * .5f, 32), scale, 0, 0);
            }
        }

        // Change the way of collision check of the projectile
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            Vector2 unit = new Vector2(0, -1);
            float point = 0f;
            // Run an AABB versus Line check to look for collisions, look up AABB collision first to see how it works
            // It will look for collisions on the given line using AABB
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center,
                Projectile.Center + unit * Distance, 22, ref point);
        }

        // The AI of the projectile
        public override void AI()
        {
            Distance = Projectile.position.Y;
            CastLights();
            timer = (timer + 1) % 5;
            if (timer == 0)
            {
                frame++;
            }
            if (frame == 9)
            {
                Projectile.Kill();
            }

            Projectile.ai[1] += 1f;
        }

        public override void OnHitPlayer(Player target, int damage, bool crit)
        {
            target.AddBuff(BuffID.OnFire, 600, true);
        }

        private void CastLights()
        {
            // Cast a light along the line of the laser
            DelegateMethods.v3_1 = new Vector3(0.8f, 0.8f, 1f);
            Utils.PlotTileLine(Projectile.Center, Projectile.Center + Projectile.velocity * (Distance - 0), 26, DelegateMethods.CastLight);
        }

        public override bool ShouldUpdatePosition() => false;
    }
}
