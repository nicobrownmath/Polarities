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
using Terraria.Audio;
using Terraria.GameContent;
using System.Collections.Generic;
using Polarities.Effects;
using ReLogic.Content;

namespace Polarities.Items.Weapons.Magic
{
	public class RayOfSunshine : ModItem
	{
		public override void SetStaticDefaults()
		{
			this.SetResearch(1);
		}

		public override void SetDefaults()
		{
			Item.SetWeaponValues(130, 3, 0);
			Item.DamageType = DamageClass.Magic;
			Item.mana = 10;

			Item.width = 50;
			Item.height = 122;

			Item.useTime = 30;
			Item.useAnimation = 30;
			Item.useStyle = 5;
			Item.UseSound = SoundID.Item15;
			Item.autoReuse = false;
			Item.channel = true;

			Item.shoot = ProjectileType<RayOfSunshineProjectile>();
			Item.shootSpeed = 1f;

			Item.value = Item.sellPrice(gold: 6, silver: 50);
			Item.rare = RarityType<SunPixieFlawlessRarity>();
			Item.GetGlobalItem<PolaritiesItem>().flawless = true;
		}

		private int time;

		public override void HoldItem(Player player)
		{
			if (player.channel)
			{
				Lighting.AddLight(player.Center, new Vector3(255 / 128f, 220 / 128f, 64 / 128f));

				player.direction = (Main.MouseWorld.X - player.Center.X > 0) ? 1 : -1;
				time++;
				if (!player.ItemTimeIsZero) player.itemTime = player.itemTimeMax;
				player.itemAnimation = player.itemAnimationMax;
				player.manaRegen = Math.Min(player.manaRegen, 0);

				if (time % 10 == 0)
				{
					if (!player.CheckMana(player.inventory[player.selectedItem].mana, true))
					{
						player.channel = false;
					}
				}
				if (time % 30 == 0 && Item.UseSound != null)
				{
					SoundEngine.PlaySound((SoundStyle)Item.UseSound, player.position);
				}
			}
			else
			{
				time = 0;
			}
		}

        public override void UseStyle(Player player, Rectangle heldItemFrame)
		{
			player.itemRotation = (new Vector2(Main.MouseWorld.X, player.MountedCenter.Y - 1000) - player.MountedCenter).ToRotation() + MathHelper.PiOver2;
			player.itemLocation += new Vector2(-player.direction * 22, -44).RotatedBy(player.itemRotation);
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
		{
			SoundEngine.PlaySound(SoundID.Zombie104, Main.MouseWorld);
			player.GetModPlayer<PolaritiesPlayer>().AddScreenShake(60, 60);
			Projectile.NewProjectile(source, Main.MouseWorld, Vector2.Zero, type, damage, knockback, player.whoAmI);
			return false;
		}
	}

	public class RayOfSunshineProjectile : ModProjectile
	{
		public override string Texture => "Polarities/NPCs/SunPixie/SunPixieDeathray";

		public override void SetDefaults()
		{
			Projectile.width = 12;
			Projectile.height = 12;
			Projectile.friendly = true;
			Projectile.DamageType = DamageClass.Magic;
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
			Projectile.scale = 0;

			Projectile.GetGlobalProjectile<PolaritiesProjectile>().ForceDraw = true;
		}

		public override void AI()
		{
			CastLights();

			if (Projectile.localAI[0] == 0)
			{
				Projectile.localAI[0] = 1;
			}

			Player projOwner = Main.player[Projectile.owner];

			Projectile.rotation = -MathHelper.PiOver2;

			if (!projOwner.channel && Projectile.timeLeft > 20 && Projectile.timeLeft < 3600 - 20)
			{
				Projectile.timeLeft = 20;
			}

			if (Projectile.scale < 4f && Projectile.timeLeft > 20)
			{
				Projectile.scale += 0.2f;
			}
			else if (Projectile.timeLeft <= 20)
			{
				Projectile.scale -= 0.2f;
				Projectile.velocity.X *= 0.95f;
				return;
			}

			if (Main.myPlayer == Projectile.owner)
			{
				Projectile.velocity.X = (Main.MouseWorld.X - Projectile.Center.X) / 16;
				if (Projectile.velocity.X > 6)
				{
					Projectile.velocity.X = 6;
				}
				if (Projectile.velocity.X < -6)
				{
					Projectile.velocity.X = -6;
				}
			}
			Projectile.netUpdate = true;
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			float point = 0f;
			return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), new Vector2(Projectile.Center.X, 0),
				new Vector2(Projectile.Center.X, Main.maxTilesY * 16), 16 * Projectile.scale, ref point);
		}

		public override bool PreDraw(ref Color lightColor)
		{
			if (Projectile.scale <= 0.1f)
			{
				return false;
			}

			Asset<Texture2D> texture = TextureAssets.Projectile[Type];

			float radius = Projectile.position.Y;

			float numLayers = 12;

			float alphaMult = 0.6f;

			for (int i = 0; i < numLayers; i++)
			{
				Vector2 startPos = Projectile.Center;
				Vector2 drawCurrentSegmentPos = startPos;
				Vector2 goalPos = Projectile.Center + new Vector2(radius, 0).RotatedBy(Projectile.rotation);
				float segmentLength = 12 * Projectile.scale;

				float colorAmount = (numLayers - i) / numLayers;

				Color color = new Color((int)(255 * colorAmount + 255 * (1 - colorAmount)), (int)(195 * colorAmount + 255 * (1 - colorAmount)), (int)(32 * colorAmount + 255 * (1 - colorAmount)));
				float alpha = 0.4f * alphaMult;
				float scale = 1f;

				Vector2 positionOffset = new Vector2(Projectile.scale * 4, 0).RotatedBy(5 * i * MathHelper.TwoPi / numLayers + Projectile.timeLeft * 0.1f);

				Main.EntitySpriteDraw(texture.Value, new Vector2(Projectile.Center.X + positionOffset.X, 0) - Main.screenPosition, texture.Frame(), color * alpha, 0f, new Vector2(texture.Width() / 2, 0), new Vector2(Projectile.scale * scale, Main.maxTilesY * 16 / 6f), SpriteEffects.None, 0);
			}

			return false;
		}

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
			DrawLayer.AddProjectile<DrawLayerAdditiveAfterProjectiles>(index);
        }

        public override void OnHitNPC(NPC target, int damage, float knockBack, bool crit)
		{
			target.AddBuff(BuffID.OnFire, 600, true);
		}

		private void CastLights()
		{
			DelegateMethods.v3_1 = new Vector3(255 / 128f, 220 / 128f, 64 / 128f);
			Utils.PlotTileLine(new Vector2(Projectile.Center.X, 0), new Vector2(Projectile.Center.X, Main.maxTilesY * 16), 26, DelegateMethods.CastLight);
		}
	}
}