using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using static Terraria.ModLoader.ModContent;
using Microsoft.Xna.Framework;
using Polarities.Projectiles;
using Terraria.DataStructures;
using System;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;

namespace Polarities.Items.Weapons.Melee
{
	public class Starduster : ModItem
	{
		public override void SetStaticDefaults()
		{
            this.SetResearch(1);
		}

		public override void SetDefaults()
		{
			Item.SetWeaponValues(20, 1f, 0);
			Item.DamageType = DamageClass.Melee;

			Item.width = 28;
			Item.height = 28;

			Item.useTime = 20;
			Item.useAnimation = 20;
			Item.useStyle = ItemUseStyleID.Rapier;
			Item.noMelee = true;
			Item.noUseGraphic = true;
			Item.UseSound = SoundID.Item9;
			Item.autoReuse = true;

			Item.shoot = ProjectileType<StardusterProjectile>();
            Item.shootSpeed = 2f;

			Item.value = 10000;
			Item.rare = ItemRarityID.Blue;
		}

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
		{
			Projectile.NewProjectile(source, Main.MouseWorld + new Vector2(0, -1000).RotatedByRandom(1), Vector2.Zero, ProjectileType<StardusterSkyProjectile>(), damage, knockback, player.whoAmI, Main.MouseWorld.X, Main.MouseWorld.Y);
			return true;
		}

		public override void AddRecipes()
		{
			CreateRecipe()
				.AddIngredient(ItemType<Items.Placeable.Bars.SunplateBar>(), 10)
				.AddIngredient(ItemID.FallenStar, 5)
				.AddTile(TileID.SkyMill)
				.Register();
		}
	}

	//copied from exampleshortsword
	public class StardusterProjectile : ModProjectile
	{
		public override string Texture => "Polarities/Items/Weapons/Melee/Starduster";

		public const int FadeInDuration = 4;
		public const int FadeOutDuration = 4;

		public const int TotalDuration = 16;

		public float CollisionWidth => 10f * Projectile.scale;

		public int Timer
		{
			get => (int)Projectile.ai[0];
			set => Projectile.ai[0] = value;
		}

		public override void SetStaticDefaults()
		{
			// DisplayName.SetDefault("{$Mods.Polarities.ItemName.Starduster}");
		}

		public override void SetDefaults()
		{
			Projectile.Size = new Vector2(18);
			Projectile.aiStyle = -1;
			Projectile.friendly = true;
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
			Projectile.scale = 1f;
			Projectile.DamageType = DamageClass.Melee;
			Projectile.ownerHitCheck = true;
			Projectile.extraUpdates = 1;
			Projectile.timeLeft = 360;
			Projectile.hide = true;
		}

		public override void AI()
		{
			Player player = Main.player[Projectile.owner];

			Timer += 1;
			if (Timer >= TotalDuration)
			{
				Projectile.Kill();
				return;
			}
			else
			{
				player.heldProj = Projectile.whoAmI;
			}

			// Fade in and out
			Projectile.Opacity = Utils.GetLerpValue(0f, FadeInDuration, Timer, clamped: true) * Utils.GetLerpValue(TotalDuration, TotalDuration - FadeOutDuration, Timer, clamped: true);

			Vector2 playerCenter = player.RotatedRelativePoint(player.MountedCenter, reverseRotation: false, addGfxOffY: false);
			Projectile.Center = playerCenter + Projectile.velocity * (Timer - 1f);

			Projectile.spriteDirection = (Vector2.Dot(Projectile.velocity, Vector2.UnitX) >= 0f).ToDirectionInt();
			Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2 - MathHelper.PiOver4 * Projectile.spriteDirection;

			SetVisualOffsets();
		}

		private void SetVisualOffsets()
		{
			const int HalfSpriteWidth = 28 / 2;
			const int HalfSpriteHeight = 28 / 2;

			int HalfProjWidth = Projectile.width / 2;
			int HalfProjHeight = Projectile.height / 2;

			DrawOriginOffsetX = 0;
			DrawOffsetX = -(HalfSpriteWidth - HalfProjWidth);
			DrawOriginOffsetY = -(HalfSpriteHeight - HalfProjHeight);
		}

		public override bool ShouldUpdatePosition()
		{
			return false;
		}

		public override void CutTiles()
		{
			DelegateMethods.tilecut_0 = Terraria.Enums.TileCuttingContext.AttackProjectile;
			Vector2 start = Projectile.Center;
			Vector2 end = start + Projectile.velocity.SafeNormalize(-Vector2.UnitY) * 10f;
			Utils.PlotTileLine(start, end, CollisionWidth, DelegateMethods.CutTiles);
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			Vector2 start = Projectile.Center;
			Vector2 end = start + Projectile.velocity * 6f;
			float collisionPoint = 0f;
			return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), start, end, CollisionWidth, ref collisionPoint);
		}
	}

	public class StardusterSkyProjectile : ModProjectile
    {
        public override string Texture => "Polarities/Items/Weapons/Melee/Starduster";

        private Vector2 direction;

        public override void SetStaticDefaults()
		{
			// DisplayName.SetDefault("{$Mods.Polarities.ItemName.Starduster}");
		}

        public override void SetDefaults()
        {
            Projectile.width = 28;
            Projectile.height = 28;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.timeLeft = 80;
            Projectile.tileCollide = false;
            Projectile.light = 0.5f;
        }

        public override void AI()
        {
            if (Projectile.localAI[0] == 0)
            {
                direction = Projectile.Center - new Vector2(Projectile.ai[0], Projectile.ai[1]);
                direction.Normalize();
                Projectile.rotation = direction.ToRotation() + 3 * (float)Math.PI / 4;
            }
            Projectile.localAI[0]++;
            float dist = (float)Math.Pow((Projectile.localAI[0] - 40), 2) * 0.625f;
            Projectile.velocity = direction * dist + new Vector2(Projectile.ai[0], Projectile.ai[1]) - Projectile.Center;
            Projectile.rotation += 16 / (float)(dist + 16);
        }

        public override bool? CanCutTiles()
        {
            return false;
        }

        public override bool PreDraw(ref Color lightColor)
        {
			float trailAlphaMult = Math.Clamp((40 - Projectile.localAI[0]) / 10f, 0f, 1f);

			if (trailAlphaMult > 0f)
			{
				Texture2D value175 = TextureAssets.Extra[91].Value;
				Rectangle value176 = value175.Frame();
				Vector2 origin10 = new Vector2((float)value176.Width / 2f, 10f);
				Vector2 value177 = new Vector2(0f, Projectile.gfxOffY);
				Vector2 spinningpoint = new Vector2(0f, -10f);
				float num184 = (float)Main.timeForVisualEffects / 60f;
				Vector2 value178 = Projectile.Center + Projectile.velocity;
				Color color42 = Color.Blue * 0.2f * trailAlphaMult;
				Color value179 = Color.White * 0.5f;
				value179.A = 0;
				float num185 = 0f;
				if (Main.tenthAnniversaryWorld)
				{
					color42 = Color.HotPink * 0.3f;
					value179 = Color.White * 0.75f;
					value179.A = 0;
					num185 = -0.1f;
				}
				Color color43 = color42;
				color43.A = 0;
				Color color44 = color42;
				color44.A = 0;
				Color color45 = color42;
				color45.A = 0;
				Vector2 val8 = value178 - Main.screenPosition + value177;
				Vector2 spinningpoint17 = spinningpoint;
				double radians6 = (float)Math.PI * 2f * num184;
				Vector2 val2 = default(Vector2);
				Main.EntitySpriteDraw(value175, val8 + spinningpoint17.RotatedBy(radians6, val2), value176, color43, Projectile.velocity.ToRotation() + (float)Math.PI / 2f, origin10, 1.5f + num185, (SpriteEffects)0, 0);
				Vector2 val9 = value178 - Main.screenPosition + value177;
				Vector2 spinningpoint18 = spinningpoint;
				double radians7 = (float)Math.PI * 2f * num184 + (float)Math.PI * 2f / 3f;
				val2 = default(Vector2);
				Main.EntitySpriteDraw(value175, val9 + spinningpoint18.RotatedBy(radians7, val2), value176, color44, Projectile.velocity.ToRotation() + (float)Math.PI / 2f, origin10, 1.1f + num185, (SpriteEffects)0, 0);
				Vector2 val10 = value178 - Main.screenPosition + value177;
				Vector2 spinningpoint19 = spinningpoint;
				double radians8 = (float)Math.PI * 2f * num184 + 4.1887903f;
				val2 = default(Vector2);
				Main.EntitySpriteDraw(value175, val10 + spinningpoint19.RotatedBy(radians8, val2), value176, color45, Projectile.velocity.ToRotation() + (float)Math.PI / 2f, origin10, 1.3f + num185, (SpriteEffects)0, 0);
				Vector2 value180 = Projectile.Center - Projectile.velocity * 0.5f;
				for (float num186 = 0f; num186 < 1f; num186 += 0.5f)
				{
					float num187 = num184 % 0.5f / 0.5f;
					num187 = (num187 + num186) % 1f;
					float num188 = num187 * 2f;
					if (num188 > 1f)
					{
						num188 = 2f - num188;
					}
					num188 *= trailAlphaMult;
					Main.EntitySpriteDraw(value175, value180 - Main.screenPosition + value177, value176, value179 * num188, Projectile.velocity.ToRotation() + (float)Math.PI / 2f, origin10, 0.3f + num187 * 0.5f, (SpriteEffects)0, 0);
				}
			}

			lightColor = Color.White;

			return true;
		}
    }
}