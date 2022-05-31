using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using static Terraria.ModLoader.ModContent;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using Polarities.Projectiles;
using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.GameContent;
using ReLogic.Content;

namespace Polarities.Items.Weapons.Summon.Orbs
{
	public class Discorb : ModItem
	{
		public override void SetStaticDefaults()
		{
			this.SetResearch(1);
		}

		public override void SetDefaults()
		{
			Item.SetWeaponValues(0, 0, 0);
			Item.DamageType = DamageClass.Summon;

			Item.width = 22;
			Item.height = 22;

			Item.useTime = 10;
			Item.useAnimation = 10;
			Item.channel = true;
			Item.useStyle = ItemUseStyleID.HoldUp;
			Item.UseSound = SoundID.Item4;
			Item.noMelee = true;

			Item.shoot = ProjectileType<DiscorbMinion>();
			Item.shootSpeed = 0f;

			Item.value = Item.buyPrice(gold: 2);
			Item.rare = ItemRarityID.Blue;
		}

		public override void HoldItem(Player player)
		{
			if (player.channel)
			{
				player.direction = (Main.MouseWorld.X - player.Center.X > 0) ? 1 : -1;
				if (!player.ItemTimeIsZero) player.itemTime = player.itemTimeMax;
				player.itemAnimation = player.itemAnimationMax;
			}
		}

        public override void UseStyle(Player player, Rectangle heldItemFrame)
		{
			player.itemLocation += new Vector2(0, 8 - Item.height / 2);
		}

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
		{
			for (int i = 0; i < player.GetModPlayer<PolaritiesPlayer>().orbMinionSlots; i++)
			{
				int proj = Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
				Main.projectile[proj].originalDamage = damage;
			}
			return false;
		}
	}

	public class DiscorbMinion : ModProjectile
	{
		//textures for cacheing
		public static Asset<Texture2D> WingTexture;

		public override void Load()
		{
			WingTexture = Request<Texture2D>(Texture + "_Wing");
		}

		public override void Unload()
		{
			WingTexture = null;
		}

		public override void SetDefaults()
		{
			Projectile.width = 46;
			Projectile.height = 46;

			Projectile.penetrate = -1;
			Projectile.DamageType = DamageClass.Summon; //irrelevant
			Projectile.minion = true;
			Projectile.tileCollide = true;
			Projectile.ignoreWater = false;
			Projectile.timeLeft = 3600;
		}

		public override void AI()
		{
			Projectile.damage = 1;

			Player player = Main.player[Projectile.owner];

			if (!player.channel || !player.active || player.dead)
			{
				Projectile.Kill();
				return;
			}
			else
			{
				Projectile.timeLeft = 2;
			}

			int index = 0;
			int ownedProjectiles = 0;
			for (int i = 0; i < Main.maxProjectiles; i++)
			{
				if (Main.projectile[i].active && Main.projectile[i].type == Projectile.type && Main.projectile[i].owner == Projectile.owner)
				{
					ownedProjectiles++;
					if (i < Projectile.whoAmI)
					{
						index++;
					}
				}
			}

			Projectile.ai[0]++;
			Projectile.ai[1] = (Projectile.ai[0] + 180 - (180 * index) / ownedProjectiles) % 180;
			if (Projectile.ai[1] < 30)
			{
				Projectile.velocity = Vector2.Zero;

				if (Projectile.ai[1] == 0)
				{
					//flashbang all nearby enemies to confuse them
					for (int i = 0; i < Main.maxNPCs; i++)
					{
						if (Main.npc[i].active && Main.npc[i].Distance(Projectile.Center) < 240)
						{
							Main.npc[i].AddBuff(BuffID.Confused, 360);
						}
					}
				}
			}
			else
			{
				Vector2 goalPosition = Main.MouseWorld;
				Vector2 goalVelocity = (goalPosition - Projectile.Center) / 20f;
				Projectile.velocity += (goalVelocity - Projectile.velocity) / 30f;
			}

			if (player.Distance(Projectile.Center) > 1400)
			{
				Projectile.Center = player.Center;
			}
		}

		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			if (Projectile.velocity.X != oldVelocity.X)
			{
				Projectile.velocity.X = -oldVelocity.X;
			}
			if (Projectile.velocity.Y != oldVelocity.Y)
			{
				Projectile.velocity.Y = -oldVelocity.Y;
			}
			return false;
		}

		public override bool? CanCutTiles()
		{
			return false;
		}

		public override bool MinionContactDamage()
		{
			return false;
		}

        public override bool PreDraw(ref Color lightColor)
		{
			Texture2D texture = TextureAssets.Projectile[Type].Value;
			Color tintColor = Main.hslToRgb((Projectile.ai[0] / 120f) % 1f, 1f, 0.95f);
			Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, texture.Frame(), tintColor, 0f, texture.Frame().Size() / 2, Projectile.scale, SpriteEffects.None, 0);

			Texture2D texture2 = WingTexture.Value;
			float totalRotation = (float)Math.Atan(Projectile.velocity.X * 0.3f) * 0.5f;
			for (int i = 2; i >= 0; i--)
			{
				Color color = Main.hslToRgb((Projectile.ai[0] / 180f + i / 3f) % 1f, 1f, 0.95f);

				float rotation = ((float)Math.Cos(Projectile.ai[0] * MathHelper.TwoPi / 60f) + 1f) * (i - 4) * 0.3f + 1.25f + totalRotation;
				Main.EntitySpriteDraw(texture2, Projectile.Center - Main.screenPosition + new Vector2(30, 0).RotatedBy(rotation), texture2.Frame(), color, rotation, new Vector2(0, 7), Projectile.scale * (1 - i * 0.2f), SpriteEffects.None, 0);

				float rotation2 = MathHelper.TwoPi - rotation + 2 * totalRotation;
				Main.EntitySpriteDraw(texture2, Projectile.Center - Main.screenPosition - new Vector2(30, 0).RotatedBy(rotation2), texture2.Frame(), color, rotation2, new Vector2(74, 7), Projectile.scale * (1 - i * 0.2f), SpriteEffects.FlipHorizontally, 0);
			}

			if (Projectile.ai[1] < 20)
			{
				Texture2D auraTexture = Textures.Glow256.Value;
				float scale = 2f * (20 - Projectile.ai[1]) / 20f;

				Main.EntitySpriteDraw(auraTexture, Projectile.Center - Main.screenPosition, auraTexture.Frame(), tintColor * scale, 0f, auraTexture.Frame().Size() / 2, scale, SpriteEffects.None, 0);
			}

			return false;
		}
	}
}