using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Polarities.Items.Pets
{
	public class GoldfishExplorerPetItem : ModItem
	{
		public override void SetStaticDefaults()
		{
			this.SetResearch(1);
		}

		public override void SetDefaults()
		{
			Item.damage = 0;
			Item.useStyle = 1;
			Item.shoot = ProjectileType<GoldfishExplorerPet>();
			Item.width = 32;
			Item.height = 32;
			Item.UseSound = SoundID.Item2;
			Item.useAnimation = 20;
			Item.useTime = 20;
			Item.rare = 3;
			Item.noMelee = true;
			Item.value = Item.sellPrice(0, 1, 0, 0);
			Item.buffType = BuffType<GoldfishExplorerPetBuff>();
		}

		public override void UseStyle(Player player, Rectangle heldItemFrame)
		{
			if (player.whoAmI == Main.myPlayer && player.itemTime == 0)
			{
				player.AddBuff(Item.buffType, 3600, true);
			}
		}
	}

	public class GoldfishExplorerPetBuff : ModBuff
	{
		public override void SetStaticDefaults()
		{
			Main.buffNoTimeDisplay[Type] = true;
			Main.vanityPet[Type] = true;
		}

		public override void Update(Player player, ref int buffIndex)
		{
			player.buffTime[buffIndex] = 18000;
			bool petProjectileNotSpawned = player.ownedProjectileCounts[ProjectileType<GoldfishExplorerPet>()] <= 0;
			if (petProjectileNotSpawned && player.whoAmI == Main.myPlayer)
			{
				Projectile.NewProjectile(player.GetSource_Buff(buffIndex), player.position.X + (float)(player.width / 2), player.position.Y + (float)(player.height / 2), 0f, 0f, ProjectileType<GoldfishExplorerPet>(), 0, 0f, player.whoAmI, 0f, 0f);
			}
		}
	}

	public class GoldfishExplorerPet : ModProjectile
	{
		private int teleportCooldown = -1;

		public override void SetStaticDefaults()
		{
			// DisplayName.SetDefault("Goldfish Explorer");
			Main.projFrames[Projectile.type] = 12;
			Main.projPet[Projectile.type] = true;
		}

		public override void SetDefaults()
		{
			DrawOriginOffsetY = 4;
			Projectile.width = 28;
			Projectile.height = 34;
			Projectile.penetrate = -1;
			Projectile.friendly = true;
			Projectile.ignoreWater = true;
			Projectile.tileCollide = true;
		}

		public override void AI()
		{
			Player player = Main.player[Projectile.owner];
			if (!player.active)
			{
				Projectile.active = false;
				return;
			}
			if (player.dead)
			{
				player.ClearBuff(BuffType<GoldfishExplorerPetBuff>());
			}
			if (player.HasBuff(BuffType<GoldfishExplorerPetBuff>()))
			{
				Projectile.timeLeft = 2;
			}

			teleportCooldown++;

			Tile tile = Main.tile[(int)(player.Center.X / 16), (int)(0.5 + player.Center.Y / 16) + 1];

			if ((Projectile.Center - Main.player[Projectile.owner].Center).Length() > 750 && tile != null && tile.HasUnactuatedTile && Main.tileSolid[tile.TileType])
			{
				Projectile.position.X = player.position.X;
				Projectile.position.Y = player.position.Y;
				Projectile.velocity.X = 0;
				Projectile.velocity.Y = 0;
				teleportCooldown = 0;
			}
			else if (Math.Abs(player.Center.X - Projectile.Center.X) < 50 || teleportCooldown < 64)
			{
				Projectile.velocity.X = 0;
			}
			else if (player.Center.X - Projectile.Center.X < 0 && Projectile.velocity.X > -3)
			{
				if (Projectile.velocity.Y == 0 && Projectile.velocity.X == 0 && (Projectile.Center - player.Center).Length() > 64 && teleportCooldown != 64)
				{
					Projectile.velocity.Y = -5.1f;
				}
				Projectile.velocity.X--;
			}
			else if (Projectile.velocity.X < 3)
			{
				if (Projectile.velocity.Y == 0 && Projectile.velocity.X == 0 && (Projectile.Center - player.Center).Length() > 64 && teleportCooldown != 64)
				{
					Projectile.velocity.Y = -5.1f;
				}
				Projectile.velocity.X++;
			}

			if (Projectile.velocity.Y < 15) { Projectile.velocity.Y += 0.5f; }

			Projectile.frameCounter++;
			if (teleportCooldown == 0)
			{
				Projectile.frameCounter = 0;
				Projectile.frame = 0;
			}
			else if (teleportCooldown < 48 && Projectile.frameCounter >= 4)
			{
				Projectile.frameCounter = 0;
				Projectile.frame = Math.Min(Projectile.frame + 1, 6);
			}
			else if (teleportCooldown >= 48 && Projectile.frameCounter >= 8)
			{
				Projectile.frameCounter = 0;
				if (Projectile.frame != 6 || Projectile.velocity.X != 0)
				{
					Projectile.frame = 6 + (Projectile.frame + 1) % 6;
				}
				if (Projectile.velocity.X < 0)
				{
					Projectile.spriteDirection = 1;
				}
				else if (Projectile.velocity.X > 0)
				{
					Projectile.spriteDirection = -1;
				}
			}
		}

		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			if (Projectile.velocity.X != oldVelocity.X)
			{
				Projectile.velocity.X = 0;
			}
			if (Projectile.velocity.Y != oldVelocity.Y)
			{
				if (Projectile.velocity.X != oldVelocity.X && (Projectile.Center - Main.player[Projectile.owner].Center).Length() > 64)
				{
					Projectile.velocity.Y = -5.1f;
				}
				else
				{
					Projectile.velocity.Y = 0;
				}
			}
			return false;
		}
	}
}