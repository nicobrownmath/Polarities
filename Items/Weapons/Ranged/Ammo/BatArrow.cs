using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Polarities.Items.Weapons.Ranged.Ammo
{
	public class BatArrow : ModItem
	{
		public override void SetStaticDefaults()
		{
			this.SetResearch(99);
		}

		public override void SetDefaults()
		{
			Item.SetWeaponValues(6, 1f, 0);
			Item.DamageType = DamageClass.Ranged;
			Item.consumable = true;
			Item.maxStack = 999;
			Item.ammo = AmmoID.Arrow;

			Item.width = 12 * 2;
			Item.height = 20 * 2;

			Item.shoot = ProjectileType<BatArrowProjectile>();
			Item.shootSpeed = 3f;

			Item.value = 10;
			Item.rare = 2;
		}
	}

    public class BatArrowProjectile : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("{$Mods.Polarities.ItemName.BatArrow}");

            Main.projFrames[Projectile.type] = 4;
        }
        public override void SetDefaults()
        {
            Projectile.width = 13 * 2;
            Projectile.height = 13 * 2;
            DrawOffsetX = 0;
            DrawOriginOffsetY = 0;
            DrawOriginOffsetX = 0;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.penetrate = 1;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.arrow = true;
            Projectile.timeLeft = 600;
            Projectile.tileCollide = true;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + (float)Math.PI / 2;
            Projectile.spriteDirection = (Projectile.velocity.X > 0) ? 1 : -1;

            Projectile.velocity.Y += 0.1f;

            NPC target = Projectile.FindTargetWithinRange(400);

            if (target != null)
            {
                Vector2 a = target.Center - Projectile.Center;
                if (a.Length() > 1)
                {
                    a /= a.LengthSquared();
                }
                a *= 40f;
                if (a.Length() > 0.5f)
                {
                    a.Normalize();
                    a *= 0.5f;
                }
                Projectile.velocity += a;
            }

            Projectile.frameCounter++;
            if (Projectile.frameCounter == 5)
            {
                Projectile.frameCounter = 0;
                Projectile.frame = (Projectile.frame + 1) % 4;
            }
        }
        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            Projectile.Kill();
        }
        public override void OnHitPlayer(Player target, int damage, bool crit)
        {
            Projectile.Kill();
        }
        public override void OnHitPvp(Player target, int damage, bool crit)
        {
            Projectile.Kill();
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Projectile.Kill();
            return false;
        }
        public override void Kill(int timeLeft)
        {
            Collision.HitTiles(Projectile.position + Projectile.velocity, Projectile.velocity, Projectile.width, Projectile.height);
            SoundEngine.PlaySound(SoundID.Dig, Projectile.position);
            if (Main.rand.NextBool(3) && !Projectile.noDropItem)
            {
                Item.NewItem(Projectile.GetSource_DropAsItem(), Projectile.getRect(), ItemType<BatArrow>());
            }
        }
    }
}
