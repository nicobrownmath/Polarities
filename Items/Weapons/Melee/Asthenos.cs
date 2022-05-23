using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using static Terraria.ModLoader.ModContent;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Polarities.Projectiles;
using System;
using Polarities.Buffs;
using Terraria.DataStructures;
using Terraria.Utilities;
using System.Collections.Generic;
using Polarities.Items.Placeable.Bars;
using Terraria.Audio;
using Terraria.GameContent;

namespace Polarities.Items.Weapons.Melee
{
	public class Asthenos : ModItem
	{
		public override void SetStaticDefaults()
		{
			this.SetResearch(1);
		}

		public override void SetDefaults()
		{
			Item.SetWeaponValues(40, 3, 0);
			Item.DamageType = DamageClass.Melee;

			Item.width = 32;
			Item.height = 64;

			Item.useTime = 20;
			Item.useAnimation = 20;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.autoReuse = false;
			Item.noUseGraphic = true;
			Item.channel = true;

			Item.shoot = ProjectileType<AsthenosProjectile>();

			Item.value = 80000 * 5;
			Item.rare = ItemRarityID.Yellow;
		}

		public override void AddRecipes()
		{
			CreateRecipe()
				.AddIngredient(ItemType<MantellarBar>(), 12)
				.AddIngredient(ItemID.FieryGreatsword)
				.AddTile(TileID.MythrilAnvil)
				.Register();
		}
	}

	public class AsthenosProjectile : ModProjectile
	{
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("{$Mods.Polarities.ItemName.Asthenos}");

			ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10;
			ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
		}

		public override void SetDefaults()
		{
			Projectile.aiStyle = -1;
			Projectile.friendly = true;
			Projectile.DamageType = DamageClass.Melee;
			Projectile.width = 18;
			Projectile.height = 18;
			Projectile.alpha = 0;
			Projectile.timeLeft = 3600;
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;

			Projectile.hide = true;

			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 10;
		}

		const int MAX_CHARGE = 600;
		const int SWING_TIME = 10;
		const int FADE_TIME = 10;

		public override void AI()
		{
			Player player = Main.player[Projectile.owner];
			//player.heldProj = Projectile.whoAmI;

			if (player.channel)
			{
				Projectile.Center = player.Center.Floor() + new Vector2(5 * player.direction, -20);
				Projectile.rotation = 0f;

				//charge
				if (Projectile.ai[0] < MAX_CHARGE)
				{
					Projectile.ai[0] += player.GetTotalAttackSpeed(DamageClass.Melee);

					if (Projectile.ai[0] >= MAX_CHARGE)
					{
						Projectile.ai[0] = MAX_CHARGE;
						SoundEngine.PlaySound(SoundID.Item15, Projectile.Center);
					}
				}

				Projectile.ai[1] = (int)Projectile.ai[0];

				//set time for release anim
				Projectile.timeLeft = SWING_TIME + FADE_TIME;

				player.direction = Main.MouseWorld.X > player.Center.X ? 1 : -1;

				if (Projectile.soundDelay <= 0)
				{
					SoundEngine.PlaySound(SoundID.Item34, Projectile.Center);
					Projectile.soundDelay = 30;
				}
			}
			else
			{
				if (Projectile.timeLeft == SWING_TIME + FADE_TIME - 1)
				{
					for (int i = 0; i < Main.maxNPCs; i++)
					{
						Projectile.localNPCImmunity[i] = 0;
					}
					Projectile.localNPCHitCooldown = SWING_TIME + FADE_TIME;

					if (Projectile.ai[1] == MAX_CHARGE)
					{
						player.GetModPlayer<PolaritiesPlayer>().AddScreenShake(60, SWING_TIME + FADE_TIME - 1);

						SoundEngine.PlaySound(SoundID.Item45, Projectile.Center);
					}
					else
					{
						SoundEngine.PlaySound(SoundID.Item20, Projectile.Center);
					}
				}

				if (Projectile.timeLeft >= FADE_TIME)
				{
					Projectile.rotation = (Projectile.rotation + player.direction * (MathHelper.Pi / SWING_TIME) + MathHelper.TwoPi) % MathHelper.TwoPi;
				}
				else
				{
					Projectile.ai[0] = Projectile.ai[1] * (float)Math.Sqrt(Projectile.timeLeft / (float)FADE_TIME);

					float speedMult = Projectile.timeLeft / (float)FADE_TIME;
					Projectile.rotation = (Projectile.rotation + player.direction * (MathHelper.Pi / SWING_TIME) * speedMult + MathHelper.TwoPi) % MathHelper.TwoPi;
				}
				Projectile.Center = player.Center.Floor() + new Vector2(0, -20).RotatedBy(Projectile.rotation);
			}

			player.itemLocation = Projectile.Center;
			player.itemRotation = player.direction * Projectile.rotation - player.direction * MathHelper.PiOver2;
			player.itemAnimation = 10;
			player.itemTime = 10;
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			float progress = Projectile.ai[0] / MAX_CHARGE;
			float lengthScale = (progress * 16 + 1) * (progress + 1f);
			return CustomCollision.CheckAABBvTriangle(targetHitbox, Projectile.Center, Projectile.Center + new Vector2(0, -12.8f).RotatedBy(Projectile.rotation) * lengthScale, Projectile.Center + new Vector2(0, -12.8f).RotatedBy(Projectile.oldRot[1]) * lengthScale);
		}

		public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
		{
			damage = (int)(damage * Projectile.ai[0] / MAX_CHARGE);
			hitDirection = Main.player[Projectile.owner].direction;

			Player player = Main.player[Projectile.owner];
			if (!player.channel)
			{
				damage = (int)(damage * Projectile.ai[0] / 10);
			}
		}

		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
		{
			Player player = Main.player[Projectile.owner];
			if (!player.channel)
				target.AddBuff(BuffType<Incinerating>(), (int)Projectile.ai[0]);
		}

		public override bool PreDraw(ref Color lightColor)
		{
			const float glowScaleMult = 0.1f;

			float progress = Projectile.ai[0] / MAX_CHARGE;

			Vector2 flameScale = new Vector2((progress + 1) / 2f, progress * 16 + 1) * (progress + 1f) * glowScaleMult;
			Vector2 flamePos = Projectile.Center + new Vector2(0, -10 - 64 * (progress + 1f) * ((progress + 1) / 2f) * glowScaleMult).RotatedBy(Projectile.rotation) - Main.screenPosition;
			Vector2 secondaryFlamePos = Projectile.Center + new Vector2(0, -10 - 64 * (progress + 1f) * ((progress + 1) / 2f) * glowScaleMult).RotatedBy(Projectile.rotation) - Main.screenPosition;

			if (progress > 0.9f)
			{
				Vector2 multiplier = new Vector2(0.4f, 0.125f) * (progress - 0.9f) * 10;

				for (int i = 1; i < 8; i++)
				{
					if (i != 4)
						DrawFlame(Main.spriteBatch, secondaryFlamePos, Projectile.rotation + i * MathHelper.PiOver4, flameScale * multiplier, progress, new UnifiedRandom(48391 + 4829 * i), PolaritiesSystem.timer, 1, alpha: 1f, goalAngle: Projectile.oldRot[5] + i * MathHelper.PiOver4);
				}
			}
			DrawFlame(Main.spriteBatch, flamePos, Projectile.rotation, flameScale, progress, new UnifiedRandom(53290), PolaritiesSystem.timer, 2, alpha: 1f, goalAngle: Projectile.oldRot[5]);

			Main.spriteBatch.End();
			Main.spriteBatch.Begin((SpriteSortMode)0, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, (Effect)null, Main.Transform);

			if (progress > 0.9f)
			{
				Vector2 multiplier = new Vector2(0.4f, 0.125f) * (progress - 0.9f) * 10;

				for (int i = 1; i < 8; i++)
				{
					if (i != 4)
						DrawFlame(Main.spriteBatch, secondaryFlamePos, Projectile.rotation + i * MathHelper.PiOver4, flameScale * multiplier, progress, new UnifiedRandom(48391 + 4829 * i), PolaritiesSystem.timer, 1, alpha: 1f, goalAngle: Projectile.oldRot[5] + i * MathHelper.PiOver4);
				}
			}
			DrawFlame(Main.spriteBatch, flamePos, Projectile.rotation, flameScale, progress, new UnifiedRandom(53290), PolaritiesSystem.timer, 2, alpha: 1f, goalAngle: Projectile.oldRot[5]);

			Main.spriteBatch.End();
			Main.spriteBatch.Begin((SpriteSortMode)0, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, (Effect)null, Main.Transform);

			Texture2D texture = TextureAssets.Projectile[Type].Value;
			Rectangle frame = texture.Frame();
			Vector2 center = frame.Center();

			Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, frame, Color.White, Projectile.rotation, center, Projectile.scale, SpriteEffects.None, 0);

			return false;
		}

		public static void DrawFlame(SpriteBatch spriteBatch, Vector2 position, float rotation, Vector2 scale, float progress, UnifiedRandom random, float timer, int index, float alpha = 1f, float goalAngle = 0f)
		{
			Texture2D glowTexture = Textures.Glow256.Value;
			Rectangle glowFrame = glowTexture.Frame();
			Rectangle halfGlowFrame = glowTexture.Frame(1, 2, 0, 0);
			Vector2 glowCenter = glowFrame.Center();

			if (index > 0)
			{
				for (int i = 0; i < 8 * index; i++)
				{
					float mySpeed = random.NextFloat(60f, 120f);
					float progression = (random.NextFloat(0f, 1f) + timer / mySpeed) % 1f;
					Vector2 nextPosition = position + (new Vector2(random.NextFloat(-1f, 1f) * 2 * progression * (1 - progression) * (1 - progression), -progression) * scale * new Vector2(128f)).RotatedBy(rotation);
					float nextRotation = (rotation + random.NextFloat(-0.5f, 0.5f) * progression * (1 - progression)).AngleLerp(goalAngle, (progression + 1) / 2);

					Vector2 nextScale = new Vector2(scale.X, Math.Max(scale.X, scale.Y / 2)) * 4 * progression * (1 - progression) * (1 - progression);

					DrawFlame(spriteBatch, nextPosition, nextRotation, nextScale, progress, random, timer, index - 1, alpha * (1 - progression), goalAngle);
				}
			}

			Color colorA = ModUtils.ConvectiveFlameColor(progress * progress - 0.25f);
			float alphaA = (((float)Math.Sqrt(progress) - 0.5f) * (index + 1) * 0.25f * alpha);
			spriteBatch.Draw(glowTexture, position, halfGlowFrame, colorA * alphaA, rotation, glowCenter, scale * 2f, SpriteEffects.None, 0f);
			spriteBatch.Draw(glowTexture, position, halfGlowFrame, colorA * alphaA, rotation + MathHelper.Pi, glowCenter, new Vector2(scale.X * 2f), SpriteEffects.None, 0f);

			colorA = ModUtils.ConvectiveFlameColor(progress * progress);
			alphaA = ((float)Math.Sqrt(progress) * (index + 1) * 0.5f * alpha);
			spriteBatch.Draw(glowTexture, position, halfGlowFrame, colorA * alphaA, rotation, glowCenter, scale, SpriteEffects.None, 0f);
			spriteBatch.Draw(glowTexture, position, halfGlowFrame, colorA * alphaA, rotation + MathHelper.Pi, glowCenter, new Vector2(scale.X), SpriteEffects.None, 0f);

			colorA = ModUtils.ConvectiveFlameColor(progress * progress + 0.5f);
			alphaA = ((float)Math.Sqrt(progress) * (index + 1) * 0.25f * alpha);
			spriteBatch.Draw(glowTexture, position, halfGlowFrame, colorA * alphaA, rotation, glowCenter, scale * 0.5f, SpriteEffects.None, 0f);
			spriteBatch.Draw(glowTexture, position, halfGlowFrame, colorA * alphaA, rotation + MathHelper.Pi, glowCenter, new Vector2(scale.X * 0.5f), SpriteEffects.None, 0f);
		}

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
			overWiresUI.Add(index);
		}
	}
}