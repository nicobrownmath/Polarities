using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using System;
using Terraria.DataStructures;

namespace Polarities.Items.Placeable.Blocks
{
	public class Salt : ModItem
	{
		public override void SetStaticDefaults()
		{
			ItemID.Sets.SortingPriorityMaterials[Item.type] = ItemID.Sets.SortingPriorityMaterials[ItemID.SandBlock];

			this.SetResearch(100);
		}

		public override void SetDefaults()
		{
			Item.DefaultToPlaceableTile(TileType<SaltTile>());

			//for use with salt shaker
			Item.ammo = Item.type;
		}

		public override void AddRecipes()
		{
			CreateRecipe()
				.AddIngredient(ItemType<Items.Placeable.Walls.SaltWall>(), 4)
				.AddTile(TileID.WorkBenches)
				.Register();
		}
	}

	public class SaltTile : ModTile
	{
		public override void SetStaticDefaults()
		{
			Main.tileSolid[Type] = true;
			Main.tileMergeDirt[Type] = true;
			Main.tileBlockLight[Type] = true;
			Main.tileBlendAll[Type] = true;
			TileID.Sets.BlockMergesWithMergeAllBlock[Type] = true;
			TileID.Sets.Falling[Type] = true;
			TileID.Sets.CanBeClearedDuringGeneration[Type] = false;

			AddMapEntry(new Color(255, 220, 220));

			DustType = DustType<Dusts.SaltDust>();

			HitSound = SoundID.Dig;
		}

		public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak)
		{
			if (WorldGen.noTileActions)
				return true;

			Tile above = Main.tile[i, j - 1];
			Tile below = Main.tile[i, j + 1];
			bool canFall = true;

			if (below.HasUnactuatedTile)
				canFall = false;

			if (above.HasTile && (TileID.Sets.BasicChest[above.TileType] || TileID.Sets.BasicChestFake[above.TileType] || above.TileType == TileID.PalmTree || TileID.Sets.BasicDresser[above.TileType]))
				canFall = false;

			if (canFall)
			{
				int ProjectileType = ModContent.ProjectileType<SaltProjectile>();
				float positionX = i * 16 + 8;
				float positionY = j * 16 + 8;

				if (Main.netMode == NetmodeID.SinglePlayer)
				{
					Main.tile[i, j].ClearTile();
					int proj = Projectile.NewProjectile(new EntitySource_TileBreak(i, j), positionX, positionY, 0f, 0.41f, ProjectileType, 10, 0f, Main.myPlayer);
					Main.projectile[proj].ai[0] = 1f;
					WorldGen.SquareTileFrame(i, j);
				}
				else if (Main.netMode == NetmodeID.Server)
				{
					WorldGen.TileEmpty(i, j);
					bool spawnProj = true;

					for (int k = 0; k < 1000; k++)
					{
						Projectile otherProj = Main.projectile[k];

						if (otherProj.active && otherProj.owner == Main.myPlayer && otherProj.type == ProjectileType && Math.Abs(otherProj.timeLeft - 3600) < 60 && otherProj.Distance(new Vector2(positionX, positionY)) < 4f)
						{
							spawnProj = false;
							break;
						}
					}

					if (spawnProj)
					{
						int proj = Projectile.NewProjectile(new EntitySource_TileBreak(i, j), positionX, positionY, 0f, 2.5f, ProjectileType, 10, 0f, Main.myPlayer);
						Main.projectile[proj].velocity.Y = 0.5f;
						Main.projectile[proj].position.Y += 2f;
						Main.projectile[proj].netUpdate = true;
					}

					NetMessage.SendTileSquare(-1, i, j, 1);
					WorldGen.SquareTileFrame(i, j);
				}
				return false;
			}
			return true;
		}

		public override void RandomUpdate(int i, int j)
		{
			if (j > Main.rockLayer && !Main.tile[i, j - 1].HasTile && Main.tile[i, j - 1].LiquidType == 0 && Main.tile[i, j - 1].LiquidAmount == 255 && Main.tile[i, j].Slope == 0 && !Main.tile[i, j].IsHalfBlock)
			{
				//count nearby salt crystals
				int crystalCount = 0;
				for (int k = -5; k < 6; k++)
				{
					for (int l = -5; l < 6; l++)
					{
						if (Main.tile[i + k, j + l].HasTile && Main.tile[i + k, j + l].TileType == TileType<SaltCrystalsTile>()) { crystalCount++; }
					}
				}
				//grow salt crystal
				if (crystalCount < 3)
				{
					WorldGen.PlaceTile(i, j - 1, TileType<SaltCrystalsTile>(), mute: true, style: Main.rand.Next(7));
				}
			}
		}
	}

	public class SaltProjectile : ModProjectile
	{
		protected bool falling = true;
		protected int tileType;
		protected int dustType;

		public override void SetStaticDefaults()
		{
			ProjectileID.Sets.ForcePlateDetection[Projectile.type] = true;
		}

		public override void SetDefaults()
		{
			Projectile.knockBack = 6f;
			Projectile.width = 14;
			Projectile.height = 14;
			Projectile.friendly = true;
			Projectile.hostile = true;
			Projectile.penetrate = -1;

			tileType = TileType<SaltTile>();

			dustType = DustType<Dusts.SaltDust>();
		}

		public override void AI()
		{
			if (Main.rand.Next(5) == 0)
			{
				int dust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, dustType);
				Main.dust[dust].velocity.X *= 0.4f;
			}

			Projectile.tileCollide = true;
			Projectile.localAI[1] = 0f;

			if (Projectile.ai[0] == 1f)
			{
				if (!falling)
				{
					Projectile.ai[1] += 1f;

					if (Projectile.ai[1] >= 60f)
					{
						Projectile.ai[1] = 60f;
						Projectile.velocity.Y += 0.2f;
					}
				}
				else
					Projectile.velocity.Y += 0.41f;
			}
			else if (Projectile.ai[0] == 2f)
			{
				Projectile.velocity.Y += 0.2f;

				if (Projectile.velocity.X < -0.04f)
					Projectile.velocity.X += 0.04f;
				else if (Projectile.velocity.X > 0.04f)
					Projectile.velocity.X -= 0.04f;
				else
					Projectile.velocity.X = 0f;
			}

			Projectile.rotation += 0.1f;

			if (Projectile.velocity.Y > 10f)
				Projectile.velocity.Y = 10f;
		}

        public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac)
		{
			Projectile.velocity = Collision.TileCollision(Projectile.position, Projectile.velocity, Projectile.width, Projectile.height, fallThrough, fallThrough, 1);

			return false;
		}

		public override void OnKill(int timeLeft)
		{
			if (Projectile.owner == Main.myPlayer && !Projectile.noDropItem)
			{
				bool success = false;

				int tileX = (int)(Projectile.position.X + Projectile.width / 2) / 16;
				int tileY = (int)(Projectile.position.Y + Projectile.width / 2) / 16;

				Tile tile = Main.tile[tileX, tileY];
				Tile tileBelow = Main.tile[tileX, tileY + 1];

				if (tile.IsHalfBlock && Projectile.velocity.Y > 0f && System.Math.Abs(Projectile.velocity.Y) > System.Math.Abs(Projectile.velocity.X))
					tileY--;

				if (!tile.HasTile)
				{
					bool onMinecartTrack = tileY < Main.maxTilesY - 2 && tileBelow != null && tileBelow.HasTile && tileBelow.TileType == TileID.MinecartTrack;

					if (!onMinecartTrack)
					{
						success = WorldGen.PlaceTile(tileX, tileY, tileType, false, true);
					}

					if (!onMinecartTrack && tile.HasTile && tile.TileType == tileType)
					{
						if (tileBelow.IsHalfBlock || tileBelow.Slope != SlopeType.Solid)
						{
							WorldGen.SlopeTile(tileX, tileY + 1, 0);

							if (Main.netMode == NetmodeID.Server)
								NetMessage.SendData(17, -1, -1, null, 14, tileX, tileY + 1);
						}

						if (Main.netMode != NetmodeID.SinglePlayer)
							NetMessage.SendData(17, -1, -1, null, 1, tileX, tileY, tileType);
					}
				}

				if (!success)
				{
					Item.NewItem(Projectile.GetSource_DropAsItem(), Projectile.Hitbox, ItemType<Salt>());
				}
			}
		}

		public override bool? CanDamage() => Projectile.localAI[1] != -1f;
	}
}
