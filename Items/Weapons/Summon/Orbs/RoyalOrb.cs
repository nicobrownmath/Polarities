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
using Terraria.DataStructures;

namespace Polarities.Items.Weapons.Summon.Orbs
{
	public class RoyalOrb : ModItem
	{
		public override void SetStaticDefaults()
		{
			SacrificeTotal = (1);
		}

		public override void SetDefaults()
		{
			Item.SetWeaponValues(9, 0, 0);
			Item.DamageType = DamageClass.Summon;

			Item.width = 22;
			Item.height = 30;

			Item.useTime = 30;
			Item.useAnimation = 30;
			Item.channel = true;
			Item.useStyle = ItemUseStyleID.HoldUp;
			Item.UseSound = SoundID.Item44;
			Item.noMelee = true;

			Item.shoot = ProjectileType<RoyalOrbMinion>();

			Item.value = Item.sellPrice(gold: 4);
			Item.rare = RarityType<QueenBeeFlawlessRarity>();
			Item.GetGlobalItem<PolaritiesItem>().flawless = true;
		}

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
		{
			player.GetModPlayer<PolaritiesPlayer>().royalOrbHitCount = 0;

			for (int i = 0; i < player.GetModPlayer<PolaritiesPlayer>().orbMinionSlots; i++)
			{
				Main.projectile[Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI)].originalDamage = damage;
			}
			return false;
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
			player.itemLocation += new Vector2(-player.direction * 2, 8 - Item.height / 2);
		}
    }

	public class RoyalOrbMinion : ModProjectile
	{
		public override void SetStaticDefaults()
		{
			Main.projFrames[Projectile.type] = 2;
		}

		public override void SetDefaults()
		{
			Projectile.width = 28;
			Projectile.height = 28;

			DrawOffsetX = -4;
			DrawOriginOffsetY = 0;
			DrawOriginOffsetX = 2;

			Projectile.penetrate = -1;
			Projectile.minion = true;
			Projectile.DamageType = DamageClass.Summon;
			Projectile.tileCollide = true;
			Projectile.ignoreWater = false;
			Projectile.timeLeft = 3600;
		}

		public override void AI()
		{
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

			if (Main.myPlayer == Projectile.owner)
			{
				Vector2 goalVelocity = (Main.MouseWorld - Projectile.Center).SafeNormalize(Vector2.Zero).RotatedBy(0.5f * ((2 * index - ownedProjectiles + 1) / (float)ownedProjectiles)) * 16;
				if ((Main.MouseWorld - Projectile.Center).Length() > 48)
				{
					Projectile.velocity += (goalVelocity - Projectile.velocity) / 24;
				}
			}
			Projectile.netUpdate = true;

			if ((Projectile.Center - player.Center).Length() > 1200)
			{
				Projectile.position = player.Center + (Projectile.position - Projectile.Center);
			}

			Projectile.direction = Projectile.velocity.X > 0 ? 1 : -1;
			Projectile.spriteDirection = Projectile.direction;
			Projectile.rotation = Projectile.velocity.X * 0.05f;

			Projectile.frameCounter++;
			if (Projectile.frameCounter == 5)
			{
				Projectile.frameCounter = 0;
				Projectile.frame = (Projectile.frame + 1) % 2;
			}

			if (player.GetModPlayer<PolaritiesPlayer>().royalOrbHitCount > Projectile.ai[1] && Projectile.ai[0] == 0)
			{
				Projectile.ai[1] = player.GetModPlayer<PolaritiesPlayer>().royalOrbHitCount;

				if (Projectile.ai[0] == 0)
				{
					Projectile shot = Main.projectile[Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, new Vector2(2, 0).RotatedByRandom(MathHelper.TwoPi), player.beeType(), player.beeDamage(Projectile.damage), player.beeKB(Projectile.knockBack), Projectile.owner)];
					shot.penetrate = 1;
					shot.maxPenetrate = 1;

					Projectile.ai[0] = 10;
				}
			}

			if (Projectile.ai[0] > 0)
			{
				Projectile.ai[0]--;
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
	}
}