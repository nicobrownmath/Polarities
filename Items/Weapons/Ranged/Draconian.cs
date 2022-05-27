using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using static Terraria.ModLoader.ModContent;
using Microsoft.Xna.Framework;
using Polarities.Projectiles;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria.DataStructures;
using Polarities.Items.Placeable.Bars;
using Terraria.Audio;
using Polarities.Effects;
using Terraria.GameContent;

namespace Polarities.Items.Weapons.Ranged
{
	public class Draconian : ModItem
	{
		public override void SetStaticDefaults()
		{
			this.SetResearch(1);
		}

		public override void SetDefaults()
		{
			Item.SetWeaponValues(80, 1, 0);
			Item.DamageType = DamageClass.Ranged;
			Item.useAmmo = AmmoID.Bullet;

			Item.width = 54;
			Item.height = 36;

			Item.useTime = 15;
			Item.useAnimation = 15;
			Item.useStyle = 5;
			Item.noMelee = true;
			Item.UseSound = SoundID.Item20;
			Item.autoReuse = false;

			Item.shoot = 10;
			Item.shootSpeed = 12f;

			Item.value = 100000;
			Item.rare = ItemRarityID.Yellow;
		}

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
			float timeLeft = (Main.MouseWorld - position).Length() / velocity.Length();
			Projectile.NewProjectile(source, position, velocity, ProjectileType<DraconianFireball>(), damage, knockback, player.whoAmI, type, timeLeft);
			return false;
		}

		public override Vector2? HoldoutOffset()
		{
			return Vector2.Zero;
		}

		public override void AddRecipes()
		{
			CreateRecipe()
				.AddIngredient(ItemType<MantellarBar>(), 12)
				.AddIngredient(ItemID.PhoenixBlaster)
				.AddTile(TileID.MythrilAnvil)
				.Register();
		}
	}

	public class DraconianFireball : ModProjectile
	{
		public override void SetStaticDefaults()
		{
			ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10;
			ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
		}

		public override void SetDefaults()
		{
			Projectile.aiStyle = -1;
			Projectile.width = 32;
			Projectile.height = 32;
			Projectile.timeLeft = 120;
			Projectile.penetrate = -1;
			Projectile.friendly = true;
			Projectile.tileCollide = true;
			Projectile.ignoreWater = true;
			Projectile.light = 1f;
			Projectile.DamageType = DamageClass.Ranged;

			Projectile.extraUpdates = 1;

			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 10;
		}

		const int FADE_TIME = 10;

		public override void AI()
		{
			if (Projectile.velocity != Vector2.Zero)
				Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

			if (Projectile.timeLeft == FADE_TIME)
			{
				TryExplode();
			}
		}

		void TryExplode()
		{
			if (Projectile.localAI[1] == 0)
			{
				Projectile.localAI[1] = 1;

				int numShots = Main.rand.Next(5, 9);
				float angle = Main.rand.NextFloat(MathHelper.TwoPi);
				for (int i = 0; i < numShots; i++)
				{
					Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, Projectile.velocity.RotatedBy(i * MathHelper.TwoPi / numShots + angle), (int)Projectile.ai[0], Projectile.damage / numShots, Projectile.knockBack, Projectile.owner);
				}

				Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, Vector2.Zero, ProjectileType<DraconianExplosion>(), Projectile.damage, Projectile.knockBack, Projectile.owner);

				SoundEngine.PlaySound(SoundID.NPCDeath14, Projectile.Center);
				SoundEngine.PlaySound(SoundID.Item40, Projectile.Center);

				Projectile.timeLeft = FADE_TIME;
				Projectile.velocity = Vector2.Zero;
				Projectile.damage = 0;
				Projectile.penetrate = 1;
			}
		}

		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
		{
			TryExplode();
		}

		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			Projectile.velocity = oldVelocity;
			TryExplode();

			return false;
		}

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
			DrawLayer.AddProjectile<DrawLayerAdditiveAfterProjectiles>(index);
        }

		public override bool PreDraw(ref Color lightColor)
		{
			float drawAlpha = 0.5f;
			if (Projectile.timeLeft < FADE_TIME) drawAlpha = 0.5f * Projectile.timeLeft / (float)FADE_TIME;

			Texture2D texture = Textures.Glow256.Value;
			Rectangle frame = texture.Frame();
			Vector2 center = frame.Center();

			Texture2D mainTexture = TextureAssets.Projectile[Type].Value;
			Rectangle mainFrame = mainTexture.Frame();
			Vector2 mainCenter = new Vector2(18, 18);

			Main.EntitySpriteDraw(mainTexture, Projectile.Center - Main.screenPosition, mainFrame, Color.White * drawAlpha, Projectile.rotation + MathHelper.PiOver2, mainCenter, Projectile.scale, SpriteEffects.None, 0);

			for (int i = 1; i < Projectile.oldPos.Length; i++)
			{
				if (Projectile.oldPos[i] != Vector2.Zero)
				{
					float progress = 1 - i / (float)Projectile.oldPos.Length;
					Vector2 scale = new Vector2(progress * 0.15f, (Projectile.oldPos[i] - Projectile.oldPos[i - 1]).Length() * 0.02f);
					Color color = ModUtils.ConvectiveFlameColor(progress * progress / 2f) * drawAlpha;
					Main.EntitySpriteDraw(texture, Projectile.oldPos[i] + Projectile.Center - Projectile.position - Main.screenPosition, frame, color, Projectile.oldRot[i], center, scale, SpriteEffects.None, 0);
				}
			}

			return false;
		}
	}

	public class DraconianExplosion : ModProjectile
	{
		public override string Texture => "Polarities/Textures/Glow256";

		public override void SetDefaults()
		{
			Projectile.aiStyle = -1;
			Projectile.width = 128;
			Projectile.height = 128;
			Projectile.scale = 0f;
			Projectile.timeLeft = 10;
			Projectile.penetrate = -1;
			Projectile.friendly = true;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.light = 1f;
			Projectile.DamageType = DamageClass.Ranged;

			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 10;
		}

		public override void AI()
		{
			Vector2 oldCenter = Projectile.Center;

			Projectile.scale = 1 - Projectile.timeLeft / 10f;
			Projectile.width = (int)(128 * Projectile.scale);
			Projectile.height = (int)(128 * Projectile.scale);

			Projectile.Center = oldCenter;
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			return CustomCollision.CheckAABBvDisc(targetHitbox, new Circle(Projectile.Center, Projectile.width / 2));
		}

		public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
		{
			DrawLayer.AddProjectile<DrawLayerAdditiveAfterProjectiles>(index);
		}

		public override bool PreDraw(ref Color lightColor)
		{
			float progress = Projectile.timeLeft / 10f;

			Texture2D texture = TextureAssets.Projectile[Type].Value;
			Rectangle frame = texture.Frame();
			Vector2 center = frame.Center();

			for (int i = 1; i <= 4; i++)
			{
				Color color = ModUtils.ConvectiveFlameColor(progress * progress * i / 4f) * (progress * 2f);
				float drawScale = Projectile.width / 128f * i / 4f;
				Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, frame, color, 0f, center, drawScale, SpriteEffects.None, 0);
			}

			return false;
		}
	}
}