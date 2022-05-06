using Microsoft.Xna.Framework;
using Polarities.Items.Armor.Vanity;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Polarities.Items.Pets
{
	public class RocketSnekPetItem : ModItem
	{
		public override void SetStaticDefaults()
		{
			this.SetResearch(1);
		}

		public override void SetDefaults()
		{
			Item.DefaultToVanitypet(ProjectileType<RocketSnekPet>(), BuffType<RocketSnekPetBuff>());

			Item.width = 24;
			Item.height = 30;

			Item.value = Item.sellPrice(silver: 10);
			Item.rare = ItemRarityID.Orange;
		}

		public override void UseStyle(Player player, Rectangle heldItemFrame)
		{
			if (player.whoAmI == Main.myPlayer && player.itemTime == 0)
			{
				player.AddBuff(Item.buffType, 3600, true);
			}
		}

		public override void AddRecipes()
		{
			CreateRecipe()
				.AddIngredient(ItemType<SnekHat>())
				.AddRecipeGroup(RecipeGroupID.IronBar, 4)
				.AddTile(TileID.Anvils)
				.Register();
		}
	}

	public class RocketSnekPetBuff : ModBuff
	{
		public override void SetStaticDefaults()
		{
			Main.buffNoTimeDisplay[Type] = true;
			Main.vanityPet[Type] = true;
		}


		public override void Update(Player player, ref int buffIndex)
		{
			player.buffTime[buffIndex] = 18000;
			bool petProjectileNotSpawned = player.ownedProjectileCounts[ProjectileType<RocketSnekPet>()] <= 0;
			if (petProjectileNotSpawned && player.whoAmI == Main.myPlayer)
			{
				Projectile.NewProjectile(player.GetSource_Buff(buffIndex), player.position.X + (float)(player.width / 2), player.position.Y + (float)(player.height / 2), 0f, 0f, ProjectileType<RocketSnekPet>(), 0, 0f, player.whoAmI, 0f, 0f);
			}
		}
	}

	public class RocketSnekPet : ModProjectile
	{
		private float phase;

		public override void SetStaticDefaults()
		{
			Main.projFrames[Projectile.type] = 6;
			Main.projPet[Projectile.type] = true;
		}

		public override void SetDefaults()
		{
			Projectile.width = 24;
			Projectile.height = 16;
			Projectile.penetrate = -1;
			Projectile.friendly = true;
			Projectile.ignoreWater = false;
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
				player.ClearBuff(BuffType<RocketSnekPetBuff>());
			}
			if (player.HasBuff(BuffType<RocketSnekPetBuff>()))
			{
				Projectile.timeLeft = 2;
			}

			Vector2 goalPoint = player.Center - new Vector2(player.direction * 50, 50);

			if ((goalPoint - Projectile.Center).Length() > 800)
			{
				Projectile.position.X = player.position.X;
				Projectile.position.Y = player.position.Y;
				Projectile.velocity.X = 0;
				Projectile.velocity.Y = 0;
			}
			else if ((goalPoint - Projectile.Center).Length() > 120)
			{
				Projectile.rotation = Projectile.velocity.X * 0.1f;
				Projectile.spriteDirection = Projectile.velocity.X > 0 ? 1 : -1;

				phase += 0.05f;
				Vector2 targetVelocity = (goalPoint - Projectile.Center).SafeNormalize(Vector2.Zero) * (player.velocity.Length() + 2);
				Projectile.velocity += (targetVelocity - Projectile.velocity) / 60;
				Projectile.velocity.Y += (float)System.Math.Sin(phase) * 0.1f;

				Projectile.frameCounter++;
				if (Projectile.frameCounter == 3)
				{
					Projectile.frameCounter = 0;
					Projectile.frame = (Projectile.frame + 1) % 6;
				}
				if (Projectile.frame < 2) { Projectile.frame = 2; }
			}
			else
			{
				phase = MathHelper.Pi;
				Projectile.rotation = Projectile.velocity.X * 0.1f;
				Projectile.velocity.X *= 0.95f;
				Projectile.frame = Projectile.velocity.Y == 0 ? 0 : 1;
				Projectile.velocity.Y += 0.1f;
			}
		}

		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			if (Projectile.velocity.X != oldVelocity.X)
			{
				Projectile.velocity.X = -oldVelocity.X;
			}
			if (Projectile.velocity.Y != oldVelocity.Y && oldVelocity.Y > 0 && Projectile.frame < 2)
			{
				Projectile.velocity.X = 0;
			}
			return false;
		}
	}
}