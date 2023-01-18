using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using Terraria.DataStructures;
using static Terraria.ModLoader.ModContent;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Polarities.Projectiles;
using System;
using Terraria.GameContent;
using System.Collections.Generic;
using Polarities.Effects;
using Terraria.Graphics.Shaders;
using Terraria.Graphics;

namespace Polarities.Items.Weapons.Melee
{
	public class LaserCutter : ModItem
	{
		public override void SetStaticDefaults()
		{
			SacrificeTotal = (1);
		}

		public override void SetDefaults()
		{
			Item.damage = 78;
			Item.knockBack = 3;
			Item.DamageType = DamageClass.Melee;
			Item.width = 24;
			Item.height = 24;
			Item.useTime = 30;
			Item.useAnimation = 30;
			Item.useStyle = 5;
			Item.value = Item.sellPrice(gold: 5);
			Item.rare = 6;
			Item.UseSound = SoundID.Item15;
			Item.autoReuse = true;
			Item.noUseGraphic = true;
			Item.shoot = ProjectileType<LaserCutterProjectile>();
			Item.shootSpeed = 1f;
		}

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
			Projectile.NewProjectile(source, player.Center, velocity.RotatedBy(0.5f * (velocity.X < 0 ? 1 : -1)), type, damage, knockback, player.whoAmI, ai1: -0.5f * (velocity.X < 0 ? 1 : -1));
			return false;
		}
	}

	public class LaserCutterProjectile : ModProjectile
	{
		public override string Texture => "Polarities/Items/Weapons/Melee/LaserCutter";

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("{$Mods.Polarities.ItemName.LaserCutter}");

			ProjectileID.Sets.TrailCacheLength[Type] = 16;
			ProjectileID.Sets.TrailingMode[Type] = 2;
		}

		public override void SetDefaults()
		{
			Projectile.aiStyle = -1;
			Projectile.width = 12;
			Projectile.height = 12;

			Projectile.alpha = 0;
			Projectile.timeLeft = 120;
			Projectile.penetrate = -1;
			Projectile.friendly = true;
			Projectile.DamageType = DamageClass.Melee;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.light = 1f;

			Projectile.scale = 0.1f;
		}

		public override void AI()
		{
			Player projOwner = Main.player[Projectile.owner];

			if (Projectile.localAI[0] == 0)
			{
				Projectile.localAI[0] = 1;
				Projectile.ai[0] = projOwner.itemAnimation;
				Projectile.timeLeft = projOwner.itemAnimation;

				Projectile.rotation = Projectile.velocity.ToRotation();

				for (int i = 0; i < Projectile.oldPos.Length; i++)
				{
					Projectile.oldPos[i] = Projectile.position;
					Projectile.oldRot[i] = Projectile.rotation;
				}

				Projectile.localAI[1] = 1; //damage multiplier
			}

			float sinScale = (float)Math.Sin(MathHelper.Pi * (Projectile.ai[0] - Projectile.timeLeft) / Projectile.ai[0]);

			Projectile.scale = Math.Min(0.75f, Math.Abs(sinScale));

			Projectile.rotation += Projectile.ai[1] * sinScale / Projectile.ai[0] * MathHelper.Pi;

			//projOwner.heldProj = Projectile.whoAmI;
			projOwner.itemTime = projOwner.itemAnimation;
			Projectile.Center = projOwner.MountedCenter + new Vector2(20, 0).RotatedBy(Projectile.rotation);

			if (projOwner.itemAnimation == 1)
			{
				Projectile.Kill();
			}
		}

        public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
        {
            damage = (int)(damage * Projectile.localAI[1]);
			Projectile.localAI[1] *= 0.9f;
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
		{
			target.AddBuff(BuffID.OnFire, 300);
		}

		const float radius = 2000;

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			return CustomCollision.CheckAABBvTriangle(targetHitbox, Projectile.Center, Projectile.Center + new Vector2(radius, 0).RotatedBy(Projectile.rotation), Projectile.Center + new Vector2(radius, 0).RotatedBy(Projectile.oldRot[1]));
		}

		public override bool PreDraw(ref Color lightColor)
		{
			if (Projectile.scale <= 0)
			{
				return false;
			}

			Vector2 startPos = Projectile.Center;
			Vector2 drawCurrentSegmentPos = startPos;
			Vector2 goalPos = Projectile.Center + new Vector2(radius, 0).RotatedBy(Projectile.rotation);
			float segmentLength = 12 * Projectile.scale;

			int steps = 0;

			while ((drawCurrentSegmentPos - goalPos).Length() > segmentLength && steps < 800)
			{
				drawCurrentSegmentPos = startPos + steps * segmentLength * (goalPos - startPos).SafeNormalize(Vector2.Zero);

				float numLayers = 8;

				for (int i = 0; i < numLayers; i++)
				{
					float stepValue = 1 - 32f / (steps + 32f);
					float colorAmount = (numLayers - i + numLayers * stepValue) / (2 * numLayers);

					Color color = new Color((int)(255 * colorAmount + 255 * (1 - colorAmount)), (int)(220 * colorAmount + 255 * (1 - colorAmount)), (int)(64 * colorAmount + 255 * (1 - colorAmount)));
					float alpha = 0.3f * (1 - 6f / (steps + 6f));
					float scale = 1f;

					Vector2 positionOffset = new Vector2(Projectile.scale * 4, 0).RotatedBy(i * MathHelper.TwoPi / numLayers + Projectile.timeLeft * 0.1f);

					Main.EntitySpriteDraw(TextureAssets.Projectile[ProjectileType<NPCs.SunPixie.SunPixieDeathray>()].Value, positionOffset + drawCurrentSegmentPos - Main.screenPosition, new Rectangle(0, 0, 12, 12), color * alpha, Projectile.rotation, new Vector2(6, 6), Projectile.scale * scale, SpriteEffects.None, 0);
				}

				steps++;
			}

			if (!DrawLayer.IsActive<DrawLayerAdditiveAfterProjectiles>())
				Main.EntitySpriteDraw(TextureAssets.Projectile[Type].Value, Projectile.Center - Main.screenPosition, TextureAssets.Projectile[Type].Frame(), Color.White, Projectile.rotation + MathHelper.PiOver4, TextureAssets.Projectile[Type].Size() / 2, 1f, SpriteEffects.None, 0);

			return false;
		}

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
			DrawLayer.AddProjectile<DrawLayerAdditiveAfterProjectiles>(index);
        }

        public override bool ShouldUpdatePosition() => false;
	}
}