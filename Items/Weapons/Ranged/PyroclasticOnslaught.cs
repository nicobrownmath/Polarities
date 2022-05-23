using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using static Terraria.ModLoader.ModContent;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Terraria.DataStructures;
using Polarities.Items.Placeable.Bars;
using Polarities.Effects;
using Terraria.GameContent;

namespace Polarities.Items.Weapons.Ranged
{
	public class PyroclasticOnslaught : ModItem
	{
		public override void SetStaticDefaults()
		{
			this.SetResearch(1);
		}

		public override void SetDefaults()
		{
			Item.SetWeaponValues(75, 4, 0);
			Item.DamageType = DamageClass.Ranged;
			Item.useAmmo = AmmoID.Arrow;

			Item.width = 56;
			Item.height = 88;

			Item.useTime = 15;
			Item.useAnimation = 15;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.noMelee = true;
			Item.UseSound = SoundID.Item5;
			Item.autoReuse = true;

			Item.shoot = 10;
			Item.shootSpeed = 12f;

			Item.value = 100000;
			Item.rare = ItemRarityID.Yellow;
		}

        public override void UseStyle(Player player, Rectangle heldItemFrame)
		{
			player.itemRotation = -player.direction * MathHelper.PiOver2;
		}

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
		{
			type = ProjectileType<PyroclasticOnslaughtProjectile>();

			for (int i = -2; i <= 2; i++)
			{
				Projectile.NewProjectile(source, position, new Vector2(0, -velocity.Length()).RotatedBy(i * 0.1f), type, damage, knockback, player.whoAmI);
			}
			return false;
		}

		public override Vector2? HoldoutOffset()
		{
			return new Vector2(-20, 0);
		}

		public override void AddRecipes()
		{
			CreateRecipe()
				.AddIngredient(ItemType<MantellarBar>(), 12)
				.AddIngredient(ItemID.MoltenFury)
				.AddTile(TileID.MythrilAnvil)
				.Register();
		}
	}

	public class PyroclasticOnslaughtProjectile : ModProjectile
	{
		public override void SetStaticDefaults()
		{
			ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10;
			ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
		}

		const int FADE_TIME = 10;

		public override void SetDefaults()
		{
			Projectile.width = 14;
			Projectile.height = 14;
			Projectile.aiStyle = -1;
			Projectile.friendly = true;
			Projectile.penetrate = -1;
			Projectile.DamageType = DamageClass.Ranged;
			Projectile.arrow = true;
			Projectile.timeLeft = 600;
			Projectile.tileCollide = false;
			Projectile.light = 1f;

			Projectile.localNPCHitCooldown = 10;
			Projectile.usesLocalNPCImmunity = true;
		}

		public override void AI()
		{
			if (Projectile.ai[0] == 0)
			{
				Projectile.ai[0]++;

				Projectile.ai[1] = Projectile.velocity.Length();
			}

			if (Projectile.velocity.Y >= 0 && Projectile.ai[0] == 1)
			{
				Projectile.ai[0]++;

				Projectile.velocity = (Main.MouseWorld - Projectile.Center).SafeNormalize(Vector2.Zero) * Projectile.ai[1];
			}

			Projectile.rotation = Projectile.velocity.ToRotation() + (float)Math.PI / 2;

			Projectile.velocity.Y += 0.2f;

			if (Projectile.damage == 0 && Projectile.timeLeft > FADE_TIME)
			{
				Projectile.timeLeft = FADE_TIME;
			}
		}

		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
		{
			Projectile.damage = 0;
			target.AddBuff(BuffID.OnFire, 120);
		}

		public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
		{
			DrawLayer.AddProjectile<DrawLayerAdditiveAfterProjectiles>(index);
		}

		public override bool PreDraw(ref Color lightColor)
		{
			float drawAlpha = 1f;
			if (Projectile.timeLeft < FADE_TIME) drawAlpha = Projectile.timeLeft / (float)FADE_TIME;

			Texture2D texture = Textures.Glow256.Value;
			Rectangle frame = texture.Frame();
			Vector2 center = frame.Center();

			for (int i = 1; i < Projectile.oldPos.Length; i++)
			{
				if (Projectile.oldPos[i] != Vector2.Zero)
				{
					float progress = 1 - i / (float)Projectile.oldPos.Length;
					Vector2 scale = new Vector2(progress * 0.1f, (Projectile.oldPos[i] - Projectile.oldPos[i - 1]).Length() * 0.01f);
					Color color = ModUtils.ConvectiveFlameColor(progress * progress / 4f) * drawAlpha;
					Main.EntitySpriteDraw(texture, Projectile.oldPos[i] + Projectile.Center - Projectile.position - Main.screenPosition, frame, color, Projectile.oldRot[i], center, scale, SpriteEffects.None, 0);
				}
			}

			Texture2D mainTexture = TextureAssets.Projectile[Type].Value;
			Rectangle mainFrame = mainTexture.Frame();
			Vector2 mainCenter = mainFrame.Center();

			Main.EntitySpriteDraw(mainTexture, Projectile.Center - Main.screenPosition, mainFrame, Color.White * drawAlpha, Projectile.rotation, mainCenter, Projectile.scale, SpriteEffects.None, 0);

			return false;
		}

		public override bool? CanCutTiles()
		{
			return false;
		}
	}
}