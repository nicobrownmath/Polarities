using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using static Terraria.ModLoader.ModContent;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Polarities.NPCs;
using System;
using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.GameContent.Creative;
using Polarities.Buffs;
using Polarities.Items.Placeable.Bars;
using Terraria.GameContent;
using Polarities.Effects;

namespace Polarities.Items.Weapons.Melee.Warhammers
{
	public class SeismicStriker : WarhammerBase, IDrawHeldItem
	{
		private static int hammerLength = 98;
		private static int hammerHeadSize = 30;

        public override int HammerLength => 98;
        public override int HammerHeadSize => 30;
        public override int DefenseLoss => 48;
        public override int DebuffTime => 1800;

        public override void SetDefaults()
		{
			Item.SetWeaponValues(180, 20f, 0);
			Item.DamageType = DamageClass.Melee;

			Item.width = 128;
			Item.height = 128;

			Item.useTime = 40;
			Item.useAnimation = 40;
			Item.useStyle = WarhammerUseStyle;
			Item.autoReuse = true;

			Item.value = 75000 * 5;
			Item.rare = ItemRarityID.Yellow;
		}

		public override bool AltFunctionUse(Player player)
		{
			return true;
		}

		public override bool CanUseItem(Player player)
		{
			return player.ownedProjectileCounts[ProjectileType<SeismicStrikerProjectile>()] < 1;
		}

		public override bool? UseItem(Player player)
		{
			if (player.altFunctionUse == 2)
			{
				Item.useStyle = ItemUseStyleID.Thrust;
				Item.shoot = ProjectileType<SeismicStrikerProjectile>();
				Item.shootSpeed = 2.5f;
				Item.noMelee = true;
				Item.noUseGraphic = true;
				Item.UseSound = SoundID.Item34;
			}
			else
			{
				Item.useStyle = WarhammerUseStyle;
				Item.shoot = 0;
				Item.noMelee = false;
				Item.noUseGraphic = false;
				Item.UseSound = SoundID.Item1;
			}
			return null;
		}

		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
		{
			damage /= 2;
			knockback /= 2;
		}

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
		{
			return player.altFunctionUse == 2;
		}

		public override void OnHitNPC(Player player, NPC target, int damage, float knockBack, bool crit)
		{
			target.AddBuff(BuffType<Incinerating>(), 120);

			base.OnHitNPC(player, target, damage, knockBack, crit);
		}

		public override void AddRecipes()
		{
			CreateRecipe()
				.AddIngredient(ItemType<MantellarBar>(), 12)
				.AddIngredient(ItemType<SkullMelter>())
				.AddTile(TileID.MythrilAnvil)
				.Register();
		}

		public void DrawHeldItem(ref PlayerDrawSet drawInfo)
		{
			Texture2D glowTexture = Textures.Glow256.Value;
			Rectangle glowFrame = glowTexture.Frame();
			Vector2 glowCenter = glowFrame.Center();

			int trailLength = 10;
			float trailStretching = 0.1f;

			float animationProgress = 1 - (drawInfo.drawPlayer.itemAnimation - 1) / (float)drawInfo.drawPlayer.itemAnimationMax;
			if (animationProgress >= SwingWindup(drawInfo.drawPlayer))
			{
				if (drawInfo.drawPlayer.itemAnimation < 2)
				{
					trailStretching *= drawInfo.drawPlayer.itemAnimation / 2f;
				}
				if (drawInfo.drawPlayer.itemAnimation > drawInfo.drawPlayer.itemAnimationMax * (1 - SwingWindup(drawInfo.drawPlayer)) - 2)
				{
					trailStretching *= (drawInfo.drawPlayer.itemAnimationMax * (1 - SwingWindup(drawInfo.drawPlayer)) - drawInfo.drawPlayer.itemAnimation) / 2f;
				}

				for (int i = 1; i < trailLength; i++)
				{
					Vector2 positionI = drawInfo.drawPlayer.itemLocation + new Vector2(drawInfo.drawPlayer.direction, -drawInfo.drawPlayer.gravDir).RotatedBy(drawInfo.drawPlayer.itemRotation - drawInfo.drawPlayer.direction * drawInfo.drawPlayer.gravDir * i * trailStretching) * (hammerLength + 2) * Item.scale;
					Vector2 positionI2 = drawInfo.drawPlayer.itemLocation + new Vector2(drawInfo.drawPlayer.direction, -drawInfo.drawPlayer.gravDir).RotatedBy(drawInfo.drawPlayer.itemRotation - drawInfo.drawPlayer.direction * drawInfo.drawPlayer.gravDir * (i - 1) * trailStretching) * (hammerLength + 2) * Item.scale;

					float progress = 1 - i / (float)trailLength;
					Vector2 scale = new Vector2(progress * 0.1f, (positionI - positionI2).Length() * 0.02f);
					Color color = ModUtils.ConvectiveFlameColor(progress * progress) * progress;

					DrawData drawData = new DrawData(glowTexture, positionI - Main.screenPosition, glowFrame, color, drawInfo.drawPlayer.itemRotation - drawInfo.drawPlayer.direction * drawInfo.drawPlayer.gravDir * MathHelper.PiOver4 - drawInfo.drawPlayer.direction * drawInfo.drawPlayer.gravDir * i * trailStretching, glowCenter, scale, SpriteEffects.None, 0);
					drawInfo.DrawDataCache.Add(drawData);
				}
			}

			int textureFrames = 1;
			int frame = 0;

			Texture2D texture = TextureAssets.Item[Type].Value;

			SpriteEffects spriteEffects = (SpriteEffects)((drawInfo.drawPlayer.gravDir != 1f) ? ((drawInfo.drawPlayer.direction != 1) ? 3 : 2) : ((drawInfo.drawPlayer.direction != 1) ? 1 : 0));

			if (drawInfo.drawPlayer.gravDir == -1)
			{
				DrawData drawData = new DrawData(texture, new Vector2((float)(int)(drawInfo.drawPlayer.itemLocation.X - Main.screenPosition.X), (float)(int)(drawInfo.drawPlayer.itemLocation.Y - Main.screenPosition.Y)), (Rectangle?)new Rectangle(0, frame * texture.Height / textureFrames, texture.Width, texture.Height / textureFrames), Color.White, drawInfo.drawPlayer.itemRotation, new Vector2((float)texture.Width * 0.5f - (float)texture.Width * 0.5f * (float)drawInfo.drawPlayer.direction, 0f), drawInfo.drawPlayer.inventory[drawInfo.drawPlayer.selectedItem].scale, spriteEffects, 0);
				drawInfo.DrawDataCache.Add(drawData);
			}
			else
			{
				Vector2 value21 = Vector2.Zero;
				int type6 = drawInfo.drawPlayer.inventory[drawInfo.drawPlayer.selectedItem].type;
				DrawData drawData = new DrawData(texture, new Vector2((float)(int)(drawInfo.drawPlayer.itemLocation.X - Main.screenPosition.X), (float)(int)(drawInfo.drawPlayer.itemLocation.Y - Main.screenPosition.Y)), (Rectangle?)new Rectangle(0, frame * texture.Height / textureFrames, texture.Width, texture.Height / textureFrames), Color.White, drawInfo.drawPlayer.itemRotation, new Vector2((float)texture.Width * 0.5f - (float)texture.Height / textureFrames * 0.5f * (float)drawInfo.drawPlayer.direction, (float)texture.Height / textureFrames) + value21, drawInfo.drawPlayer.inventory[drawInfo.drawPlayer.selectedItem].scale, spriteEffects, 0);
				drawInfo.DrawDataCache.Add(drawData);
			}
		}

		public bool DoVanillaDraw()
		{
			return false;
		}

        public override void OnHitTiles(Player player)
		{
			Rectangle hitbox = GetHitbox(player);
			Projectile.NewProjectile(player.GetSource_ItemUse(Item), new Vector2(GetHitbox(player).Center.X, GetHitbox(player).Bottom), Vector2.Zero, ProjectileType<SeismicStrikerFlamePillar>(), Item.damage, 0f, player.whoAmI);
		}
    }

	public class SeismicStrikerProjectile : ModProjectile
	{
		public override string Texture => "Polarities/Items/Weapons/Melee/Warhammers/SeismicStriker";

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("{$Mods.Polarities.ItemName.SeismicStriker}");
		}

		public override void SetDefaults()
		{
			Projectile.width = 60;
			Projectile.height = 60;
			DrawOffsetX = 30 * 2 - 64 * 2;
			DrawOriginOffsetY = 0;
			DrawOriginOffsetX = 64 - 30;

			Projectile.aiStyle = -1;
			Projectile.penetrate = -1;
			Projectile.scale = 1f;
			Projectile.alpha = 0;

			//Projectile.hide = true;
			Projectile.ownerHitCheck = true;
			Projectile.DamageType = DamageClass.Melee;
			Projectile.tileCollide = false;
			Projectile.friendly = true;
		}

		public float movementFactor
		{
			get => Projectile.ai[0];
			set => Projectile.ai[0] = value;
		}

		public override void AI()
		{
			Player projOwner = Main.player[Projectile.owner];
			Vector2 ownerMountedCenter = projOwner.RotatedRelativePoint(projOwner.MountedCenter, true);
			Projectile.direction = projOwner.direction;
			projOwner.itemTime = projOwner.itemAnimation;
			Projectile.position.X = ownerMountedCenter.X - (float)(Projectile.width / 2);
			Projectile.position.Y = ownerMountedCenter.Y - (float)(Projectile.height / 2);
			if (!projOwner.frozen)
			{
				if (movementFactor == 0f)
				{
					movementFactor = 3f;
					Projectile.netUpdate = true;
				}
				if (projOwner.itemAnimation < projOwner.itemAnimationMax / 3)
				{
					movementFactor -= 2.4f;
				}
				else
				{
					movementFactor += 2.1f;
				}
			}
			Projectile.position += Projectile.velocity * movementFactor;
			if (projOwner.itemAnimation == 0)
			{
				Projectile.Kill();
			}
			Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.ToRadians(45f);
			if (Projectile.spriteDirection == -1)
			{
				Projectile.rotation -= MathHelper.ToRadians(90f);
			}
		}
		public override void OnHitNPC(NPC target, int damage, float knockBack, bool crit)
		{
			target.AddBuff(BuffType<Incinerating>(), 20);
		}

		public override void OnHitPvp(Player target, int damage, bool crit)
		{
			target.AddBuff(BuffType<Incinerating>(), 20);
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			Player projOwner = Main.player[Projectile.owner];

			float progress = 1 - projOwner.itemAnimation / (float)projOwner.itemAnimationMax;
			if (Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center, Projectile.Center + new Vector2(0, -128f).RotatedBy(Projectile.rotation + MathHelper.PiOver4) * (progress * 16 + 1) * (progress + 1f) * 0.15f)) return true;

			return null;
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Player projOwner = Main.player[Projectile.owner];

			float progress = 1 - projOwner.itemAnimation / (float)projOwner.itemAnimationMax;
			Vector2 flameScale = new Vector2(Math.Min((progress + 1) / 2f, (1 - progress) * 2f) * 1.5f, progress * 16 + 1) * (progress + 1f) * 0.15f;
			Vector2 flamePos = Projectile.Center - Main.screenPosition + new Vector2(0, -20 - 64 * flameScale.X).RotatedBy(Projectile.rotation + MathHelper.PiOver4);

			AsthenosProjectile.DrawFlame(Main.spriteBatch, flamePos, Projectile.rotation + MathHelper.PiOver4, flameScale, 0.5f, new Terraria.Utilities.UnifiedRandom(19011), PolaritiesSystem.timer * 3, 2, alpha: 1f, goalAngle: Projectile.rotation + MathHelper.PiOver4);

			Main.spriteBatch.End();
			Main.spriteBatch.Begin((SpriteSortMode)0, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, (Effect)null, Main.Transform);

			AsthenosProjectile.DrawFlame(Main.spriteBatch, flamePos, Projectile.rotation + MathHelper.PiOver4, flameScale, 0.5f, new Terraria.Utilities.UnifiedRandom(19011), PolaritiesSystem.timer * 3, 2, alpha: 1f, goalAngle: Projectile.rotation + MathHelper.PiOver4);

			Main.spriteBatch.End();
			Main.spriteBatch.Begin((SpriteSortMode)0, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, (Effect)null, Main.Transform);

			Texture2D texture = TextureAssets.Projectile[Type].Value;
			Rectangle frame = texture.Frame();
			Vector2 center = new Vector2(texture.Width - Projectile.width / 2, Projectile.width / 2);
			SpriteEffects spriteEffects = Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

			Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, frame, Color.White, Projectile.rotation, center, Projectile.scale, spriteEffects, 0);

			return false;
		}
	}

	public class SeismicStrikerFlamePillar : ModProjectile
	{
		public override string Texture => "Polarities/Textures/Pixel";

		public override void SetDefaults()
		{
			Projectile.width = 60;
			Projectile.height = 60;
			Projectile.aiStyle = -1;
			Projectile.friendly = true;
			Projectile.penetrate = -1;
			Projectile.DamageType = DamageClass.Melee;
			Projectile.timeLeft = 120;
			Projectile.tileCollide = false;
			Projectile.hide = false;
		}

        public override void AI()
        {
			Projectile.rotation = 0f;
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			float progress = 1 - Projectile.timeLeft / 120f;
			return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center, Projectile.Center + new Vector2(0, -128f).RotatedBy(Projectile.rotation + MathHelper.PiOver4) * (Math.Min(progress * 4, 1) * 16 + 1) * (Math.Min(progress * 4, 1) + 1f) * 0.15f);
		}

		public override bool PreDraw(ref Color lightColor)
		{
			float progress = 1 - Projectile.timeLeft / 120f;
			Vector2 flameScale = new Vector2(Math.Min(0.5f, Math.Min(progress * 4f, (1 - progress) * 4f)) * 1.5f, Math.Min(progress * 4, 1) * 16 + 1) * (Math.Min(progress * 4, 1) + 1f) * 0.15f;
			Vector2 flamePos = Projectile.Center - Main.screenPosition + new Vector2(0, - 64 * flameScale.X).RotatedBy(Projectile.rotation);

			AsthenosProjectile.DrawFlame(Main.spriteBatch, flamePos, Projectile.rotation, flameScale, 0.5f, new Terraria.Utilities.UnifiedRandom(19011), PolaritiesSystem.timer * 3, 2, alpha: 1f, goalAngle: Projectile.rotation);

			return false;
		}

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
		{
			DrawLayer.AddProjectile<DrawLayerAdditiveAfterProjectiles>(index);
		}

		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
		{
			target.AddBuff(BuffType<Incinerating>(), 20);

			int defenseLoss = 12;
			int time = 900;

			WarhammerBase.Hit(Main.player[Projectile.owner], target, defenseLoss, time);
		}
	}
}

