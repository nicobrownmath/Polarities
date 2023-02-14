using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Polarities.NPCs.Eclipxie
{
    public class EclipseFlareBolt : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Eclipse Flare");
        }

        public override void SetDefaults()
        {
            Projectile.width = 24;
            Projectile.height = 24;
            Projectile.aiStyle = -1;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 3600;
            Projectile.tileCollide = true;
            Projectile.hostile = true;
            Projectile.light = 1f;
        }

        public override void AI()
        {
            Projectile.rotation += Projectile.velocity.Length() / 6;
            Projectile.velocity *= 1.04f;
            if (Projectile.velocity.Length() > 24)
            {
                Projectile.velocity.Normalize();
                Projectile.velocity *= 24;
            }

            Main.dust[Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Torch, 0, 0, 0, Color.Orange, 2)].noGravity = true;
        }

        public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac)
        {
            fallThrough = false;

            //only collide with tiles if old enough
            return Projectile.timeLeft < 3570;
        }

        public override bool CanHitPlayer(Player target)
        {
            if (Collision.CheckAABBvAABBCollision(target.position, target.Size, Projectile.position, Projectile.Size))
            {
                Projectile.Kill();
            }
            return false;
        }

        public override void Kill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item122, Projectile.position);
            Main.projectile[Projectile.NewProjectile(Projectile.GetSource_Death(), new Vector2(Projectile.Center.X, (Main.maxTilesY - 1) * 16), Vector2.Zero, ProjectileType<EclipseFlare>(), 40, 3, Projectile.owner)].netUpdate = true;
        }
    }
}