using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using static Terraria.ModLoader.ModContent;
using Microsoft.Xna.Framework;
using Polarities.Projectiles;
using Terraria.DataStructures;
using System;
using Terraria.Audio;
using Terraria.GameContent;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;

namespace Polarities.Items.Weapons.Ranged
{
	public class Flarecaller : ModItem
	{
		public override void SetStaticDefaults()
		{
			this.SetResearch(99);
		}

		public override void SetDefaults()
		{
			Item.SetWeaponValues(150, 2f, 0);
			Item.DamageType = DamageClass.Ranged;
			Item.consumable = true;
			Item.maxStack = 999;

			Item.width = 14;
			Item.height = 20;

			Item.useTime = 30;
			Item.useAnimation = 30;
			Item.useStyle = 1;
			Item.noUseGraphic = true;
			Item.noMelee = true;
			Item.knockBack = 2;
			Item.UseSound = SoundID.Item1;

			Item.shoot = ProjectileType<FlarecallerProjectile>();
			Item.shootSpeed = 10f;

			Item.value = 50;
			Item.rare = ItemRarityID.LightPurple;
		}

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
		{
			Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI, Main.MouseWorld.X);
			return false;
		}
	}

    public class FlarecallerProjectile : ModProjectile
    {
        public override string Texture => "Polarities/Items/Weapons/Ranged/Flarecaller";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("{$Mods.Polarities.ItemName.Flarecaller}");
        }

        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
            DrawOriginOffsetY = -4;
            Projectile.aiStyle = -1;
            Projectile.penetrate = -1;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.timeLeft = 3600;
            Projectile.tileCollide = true;
        }

        public override void AI()
        {
            if (Projectile.velocity.Length() > 0.5f)
            {
                Projectile.rotation += (Projectile.velocity.X > 0 ? 1 : -1) * Projectile.velocity.Length() * 0.1f;
            }

            if (Math.Abs(Projectile.Center.X - Projectile.ai[0]) < Math.Abs(Projectile.velocity.X * 10))
            {
                Projectile.ai[1] = 1;
            }
            if (Projectile.ai[1] == 1)
            {
                Projectile.velocity.X *= 0.95f;
                Projectile.velocity.Y += 0.6f;
            }
            else
            {
                Projectile.velocity.Y += 0.15f;
            }

            if (Main.rand.Next(4) < 3)
            {
                int dust = Dust.NewDust(Projectile.Center + 9 * (new Vector2(0, -1)).RotatedBy(Projectile.rotation) - (new Vector2(3, 3)), 6, 6, 6, 0, 0, 100, default(Color), 1f);
                Main.dust[dust].noGravity = true;
                Main.dust[dust].velocity *= 1.8f;
                Main.dust[dust].velocity.Y -= 0.5f;
                if (Main.rand.NextBool(4))
                {
                    Main.dust[dust].noGravity = false;
                    Main.dust[dust].scale *= 0.5f;
                }
            }
        }

        public override bool? CanDamage()
        {
            return false;
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            SoundEngine.PlaySound(SoundID.Item, Projectile.position + Projectile.velocity, 122);
            Projectile.NewProjectile(Projectile.GetSource_Death(), Projectile.Center + Projectile.velocity + new Vector2(0, Projectile.height / 2 + 2), Vector2.Zero, ProjectileType<FlarecallerFlare>(), Projectile.damage, Projectile.knockBack, Projectile.owner);
            Projectile.Kill();
            return false;
        }
    }

	public class FlarecallerFlare : ModProjectile
	{
		private float Distance;
		private int frame;
		private int timer;

		public override void SetDefaults()
		{
			Projectile.width = 10;
			Projectile.height = 10;
			Projectile.friendly = true;
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
			Projectile.hide = false;
			Projectile.timeLeft = 60;
			Projectile.DamageType = DamageClass.Ranged;

			Projectile.GetGlobalProjectile<PolaritiesProjectile>().ForceDraw = true;
		}

		public override bool PreDraw(ref Color lightColor)
		{
			DrawLaser(TextureAssets.Projectile[Type], Projectile.Center, new Vector2(0, -1), 64, Projectile.damage, (float)Math.PI / 2);
			return false;
		}

		// The core function of drawing a laser
		public void DrawLaser(Asset<Texture2D> texture, Vector2 start, Vector2 unit, float step, int damage, float rotation = 0f, float scale = 1f, float maxDist = 2000f, Color color = default(Color), int transDist = 0)
		{
			float r = unit.ToRotation() + rotation;

			// Draws the laser 'body'
			for (float i = transDist + step; i <= Distance; i += step)
			{
				Color c = Color.White;
				var origin = start + i * unit;
				Main.EntitySpriteDraw(texture.Value, origin - Main.screenPosition,
					new Rectangle(0, 256 * frame, 120, 128), i < transDist ? Color.Transparent : c, r,
					new Vector2(120 * .5f, 128), scale, 0, 0);
			}

			Main.EntitySpriteDraw(texture.Value, start - Main.screenPosition,
				new Rectangle(0, 128 + 256 * frame, 120, 128), Color.White, r, new Vector2(120 * .5f, 128), scale, 0, 0);
		}

		// Change the way of collision check of the projectile
		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			Rectangle projHitboxInflated = new Rectangle(projHitbox.X - 40, projHitbox.Y - 46, projHitbox.Width + 80, projHitbox.Height + 46);
			if (projHitboxInflated.Intersects(targetHitbox)) return true;

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
			SpawnDusts();
			CastLights();
			timer = (timer + 1) % 5;
			if (timer == 0)
			{
				frame++;
			}
			if (frame == 8)
			{
				Projectile.Kill();
			}
		}

		public override void OnHitPlayer(Player target, int damage, bool crit)
		{
			target.AddBuff(BuffID.OnFire, 600, true);
		}

		private void SpawnDusts()
		{
			Vector2 dustPos = Projectile.Center;

			for (int i = 0; i < 2; ++i)
			{
				float r = Main.rand.NextFloat() * 6f;
				float theta = (float)Math.PI * Main.rand.NextFloat();
				Vector2 dustVel = new Vector2((float)Math.Cos(theta) * r, -(float)Math.Sin(theta) * r);
				Dust dust = Main.dust[Dust.NewDust(dustPos, 0, 0, DustID.Firework_Yellow, dustVel.X, dustVel.Y)];
				dust.noGravity = true;
				dust.scale = 1.5f;
			}
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