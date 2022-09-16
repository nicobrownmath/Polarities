using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using static Terraria.ModLoader.ModContent;
using Microsoft.Xna.Framework;
using Polarities.Projectiles;
using System;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria.DataStructures;
using Polarities.Items.Placeable.Bars;
using Terraria.Audio;
using Polarities.Effects;
using Terraria.GameContent;

namespace Polarities.Items.Weapons.Magic
{
	public class HadeanUpwelling : ModItem
	{
		public override void SetStaticDefaults()
		{
			this.SetResearch(1);
		}

		public override void SetDefaults()
		{
			Item.SetWeaponValues(63, 0, 0);
			Item.DamageType = DamageClass.Magic;
			Item.mana = 18;

			Item.width = 34;
			Item.height = 42;

			Item.useTime = 30;
			Item.useAnimation = 30;
			Item.useStyle = 5;
			Item.noMelee = true;
			Item.UseSound = SoundID.Item34;
			Item.autoReuse = true;

			Item.shoot = ProjectileType<HadeanUpwellingProjectile>();
			Item.shootSpeed = 8f;

			Item.value = 60000 * 5;
			Item.rare = ItemRarityID.Yellow;
		}

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
		{
			int numShots = 8;
			float rotationOffset = Main.rand.NextFloat(MathHelper.TwoPi);
			for (int i = 0; i < numShots; i++)
			{
				Projectile.NewProjectile(source, player.Center.X, player.Center.Y, Item.shootSpeed * (float)Math.Cos(rotationOffset + 2 * Math.PI * i / numShots), velocity.Length() * (float)Math.Sin(rotationOffset + 2 * Math.PI * i / numShots), type, damage, knockback, player.whoAmI, 1, i);
			}
			rotationOffset = Main.rand.NextFloat(MathHelper.TwoPi);
			for (int i = 0; i < numShots; i++)
			{
				Projectile.NewProjectile(source, player.Center.X, player.Center.Y, Item.shootSpeed * (float)Math.Cos(rotationOffset + 2 * Math.PI * i / numShots), velocity.Length() * (float)Math.Sin(rotationOffset + 2 * Math.PI * i / numShots), type, damage, knockback, player.whoAmI, -1, i);
			}
			return false;
		}

		public override void AddRecipes()
		{
			CreateRecipe()
				.AddIngredient(ItemType<MantellarBar>(), 12)
				.AddIngredient(ItemID.SpellTome)
				.AddTile(TileID.Bookcases)
				.Register();
		}
	}

	public class HadeanUpwellingProjectile : ModProjectile
	{
		public override void SetStaticDefaults()
		{
			ProjectileID.Sets.TrailCacheLength[Projectile.type] = 20;
			ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
		}

		const int FADE_TIME = 10;

		public override void SetDefaults()
		{
			Projectile.width = 40;
			Projectile.height = 40;
			Projectile.DamageType = DamageClass.Magic;
			Projectile.aiStyle = -1;
			Projectile.penetrate = -1;
			Projectile.timeLeft = 180 + FADE_TIME;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.friendly = true;
			Projectile.light = 1f;

			Projectile.usesIDStaticNPCImmunity = true;
			Projectile.idStaticNPCHitCooldown = 10;

			Projectile.hide = true;
		}

		public override void AI()
		{
			Projectile.velocity = Projectile.velocity.RotatedBy(Projectile.ai[0] * 2 * Math.PI / 180);

			Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

			if (Projectile.timeLeft == FADE_TIME && Projectile.ai[0] == 1 && Projectile.ai[1] == 0)
			{
				//produce cool blast thing
				SoundEngine.PlaySound(SoundID.Item20, Projectile.Center);
				Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, (Main.MouseWorld - Projectile.Center).SafeNormalize(Vector2.Zero) * 32, ProjectileType<HadeanUpwellingMegablast>(), Projectile.damage * 2, Projectile.knockBack, Projectile.owner);
			}
		}

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
			behindNPCs.Add(index);
			DrawLayer.AddProjectile<DrawLayerAdditiveAfterNPCs>(index);
        }

		public override bool? CanHitNPC(NPC target)
		{
			return Projectile.timeLeft >= FADE_TIME ? null : false;
		}

		public override bool PreDraw(ref Color lightColor)
		{
			float drawAlpha = 0.5f;
			if (Projectile.timeLeft < FADE_TIME) drawAlpha = 0.5f * Projectile.timeLeft / (float)FADE_TIME;

			Texture2D texture = Textures.Glow256.Value;
			Rectangle frame = texture.Frame();
			Vector2 center = frame.Center();

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

			Texture2D mainTexture = TextureAssets.Projectile[Type].Value;
			Rectangle mainFrame = mainTexture.Frame();
			Vector2 mainCenter = new Vector2(20, 20);

			Main.EntitySpriteDraw(mainTexture, Projectile.Center - Main.screenPosition, mainFrame, Color.White * drawAlpha, Projectile.rotation, mainCenter, Projectile.scale, SpriteEffects.None, 0);

			return false;
		}

		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
		{
			target.AddBuff(BuffID.OnFire, 300, true);
		}

		public override bool? CanCutTiles()
		{
			return false;
		}
	}

	public class HadeanUpwellingMegablast : ModProjectile
	{
		public override string Texture => "Polarities/Items/Weapons/Magic/HadeanUpwellingProjectile";

		public override void SetStaticDefaults()
		{
			ProjectileID.Sets.TrailCacheLength[Projectile.type] = 20;
			ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
		}

		const int FADE_TIME = 10;

		public override void SetDefaults()
		{
			Projectile.width = 40;
			Projectile.height = 40;
			Projectile.scale = 2f;
			Projectile.DamageType = DamageClass.Magic;
			Projectile.aiStyle = -1;
			Projectile.penetrate = -1;
			Projectile.timeLeft = 180 + FADE_TIME;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.friendly = true;
			Projectile.light = 1f;

			Projectile.usesIDStaticNPCImmunity = true;
			Projectile.idStaticNPCHitCooldown = 10;

			Projectile.hide = true;
		}

		public override void AI()
		{
			Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
		}

		public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
		{
			behindNPCs.Add(index);
			DrawLayer.AddProjectile<DrawLayerAdditiveAfterNPCs>(index);
		}

		public override bool? CanHitNPC(NPC target)
		{
			return Projectile.timeLeft >= FADE_TIME;
		}

		public override bool PreDraw(ref Color lightColor)
		{
			float drawAlpha = 0.5f;
			if (Projectile.timeLeft < FADE_TIME) drawAlpha = 0.5f * Projectile.timeLeft / (float)FADE_TIME;

			Texture2D texture = Textures.Glow256.Value;
			Rectangle frame = texture.Frame();
			Vector2 center = frame.Center();

			for (int i = 1; i < Projectile.oldPos.Length; i++)
			{
				if (Projectile.oldPos[i] != Vector2.Zero)
				{
					float progress = 1 - i / (float)Projectile.oldPos.Length;
					Vector2 scale = new Vector2(progress * 0.3f, (Projectile.oldPos[i] - Projectile.oldPos[i - 1]).Length() * 0.02f);
					Color color = ModUtils.ConvectiveFlameColor(progress * progress / 2f) * drawAlpha;
					Main.EntitySpriteDraw(texture, Projectile.oldPos[i] + Projectile.Center - Projectile.position - Main.screenPosition, frame, color, Projectile.oldRot[i], center, scale, SpriteEffects.None, 0);
				}
			}

			Texture2D mainTexture = TextureAssets.Projectile[Type].Value;
			Rectangle mainFrame = mainTexture.Frame();
			Vector2 mainCenter = new Vector2(20, 20);

			Main.EntitySpriteDraw(mainTexture, Projectile.Center - Main.screenPosition, mainFrame, Color.White * drawAlpha, Projectile.rotation, mainCenter, Projectile.scale, SpriteEffects.None, 0);

			return false;
		}

		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
		{
			target.AddBuff(BuffID.OnFire, 300, true);
		}

		public override bool? CanCutTiles()
		{
			return false;
		}
	}
}