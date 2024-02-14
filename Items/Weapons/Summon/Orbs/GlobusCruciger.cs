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
using Terraria.Audio;

namespace Polarities.Items.Weapons.Summon.Orbs
{
	public class GlobusCruciger : ModItem
	{
		public override void SetStaticDefaults()
		{
			this.SetResearch(1);
		}

		public override void SetDefaults()
		{
			Item.SetWeaponValues(28, 0, 0);
			Item.DamageType = DamageClass.Summon;

			Item.width = 26;
			Item.height = 36;

			Item.useTime = 30;
			Item.useAnimation = 30;
			Item.channel = true;
			Item.useStyle = ItemUseStyleID.HoldUp;
			Item.UseSound = SoundID.Item44;
			Item.noMelee = true;
			Item.knockBack = 0;

			Item.shoot = ProjectileType<GlobusCrucigerMinion>();

			Item.value = Item.sellPrice(gold: 4, silver: 30);
			Item.rare = ItemRarityID.Pink;
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

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
		{
			position = new Vector2(Main.MouseWorld.X, Main.MouseWorld.Y - 32);
			velocity.Y = 0.3f;

			for (int i = 0; i < player.GetModPlayer<PolaritiesPlayer>().orbMinionSlots; i++)
			{
				Main.projectile[Projectile.NewProjectile(source, position.X + 64 * (2 * i - player.GetModPlayer<PolaritiesPlayer>().orbMinionSlots + 1), position.Y, velocity.X, velocity.Y, type, damage, knockback, player.whoAmI)].originalDamage = damage;
			}
			return false;
		}

		public override void AddRecipes()
		{
			CreateRecipe()
				.AddIngredient(ItemID.HallowedBar, 12)
				.AddTile(TileID.MythrilAnvil)
				.Register();
		}
	}

	public class GlobusCrucigerMinionBuff : ModBuff
	{
		public override void SetStaticDefaults()
		{
			Main.buffNoTimeDisplay[Type] = true;
		}

		public override void Update(Player player, ref int buffIndex)
		{
			player.lifeRegen += 2;
		}
	}

	public class GlobusCrucigerMinion : ModProjectile
	{
		public override void SetDefaults()
		{
			Projectile.width = 32;
			Projectile.height = 64;
			DrawOriginOffsetY = 2;

			Projectile.penetrate = -1;
			Projectile.minion = true;
			Projectile.DamageType = DamageClass.Summon;
			Projectile.friendly = true;
			Projectile.tileCollide = true;
			Projectile.timeLeft = 3600;
		}

		public override void AI()
		{
			Player player = Main.player[Projectile.owner];

			if (Projectile.localAI[0] == 0)
			{
				Projectile.localAI[0] = 1;
				Projectile.direction = player.direction;
				Projectile.spriteDirection = Projectile.direction;
			}

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

			Vector2 playerCenter = player.Center + new Vector2(player.direction * 8, -24);

			for (int dist = Main.rand.Next(1, 32); dist < (Projectile.Center - playerCenter).Length(); dist += Main.rand.Next(1, 32))
			{
				Dust.NewDustPerfect(playerCenter + (Projectile.Center - playerCenter).SafeNormalize(Vector2.Zero).RotatedByRandom(8 / (Projectile.Center - playerCenter).Length()) * dist, DustID.SparksMech, (Projectile.Center - playerCenter).SafeNormalize(Vector2.Zero) * 4, 0, Color.Transparent, 2f).noGravity = true;
			}

			if (Projectile.velocity.Y == 0)
			{
				if ((player.Center - Projectile.Center).Length() < 784 && Projectile.ai[0] > 300)
				{
					for (int dist = Main.rand.Next(1, 32); dist < (Projectile.Center - playerCenter).Length(); dist += Main.rand.Next(1, 32))
					{
						Dust.NewDustPerfect(playerCenter + (Projectile.Center - playerCenter).SafeNormalize(Vector2.Zero).RotatedByRandom(8 / (Projectile.Center - playerCenter).Length()) * dist, DustID.SparksMech, -(Projectile.Center - playerCenter).SafeNormalize(Vector2.Zero) * 4, 0, Color.Transparent, 2f).noGravity = true;
					}

					player.AddBuff(BuffType<GlobusCrucigerMinionBuff>(), 2);
				}
				else if ((player.Center - Projectile.Center).Length() > 1176)
				{
					Projectile.ai[0] = 0;

					Projectile.position = (Projectile.position - Projectile.Center) + player.Center + new Vector2(-player.direction * 64 + 64 * (2 * index - ownedProjectiles + 1), -32);
					Projectile.direction = player.direction;
					Projectile.spriteDirection = Projectile.direction;
					return;
				}

				Projectile.ai[0]++;

				if (Projectile.ai[0] % 60 == 0)
				{
					float randFloat = Main.rand.NextFloat(MathHelper.TwoPi);
					for (int i = 0; i < player.maxMinions; i++)
					{
						Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, new Vector2(1, 0).RotatedBy(randFloat + i * MathHelper.TwoPi / player.maxMinions), ProjectileType<GlobusCrucigerMinionBlade>(), Projectile.damage, Projectile.knockBack, Projectile.owner, Projectile.whoAmI);
					}
					randFloat = Main.rand.NextFloat(MathHelper.TwoPi);
					for (int i = 0; i < player.maxMinions; i++)
					{
						Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, new Vector2(1, 0).RotatedBy(randFloat + i * MathHelper.TwoPi / player.maxMinions), ProjectileType<GlobusCrucigerMinionBlade2>(), Projectile.damage, Projectile.knockBack, Projectile.owner, Projectile.whoAmI);
					}

					SoundEngine.PlaySound(SoundID.Item39, Projectile.Center);

				}

				if (Projectile.ai[0] % 5 == 0 && Projectile.ai[0] > 300)
				{
					Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, new Vector2(24, 0).RotatedBy((Main.MouseWorld - Projectile.Center).ToRotation()), ProjectileType<GlobusCrucigerMinionBlade3>(), Projectile.damage, Projectile.knockBack, Projectile.owner);

					SoundEngine.PlaySound(SoundID.Item39, Projectile.Center);
				}
			}

			Projectile.velocity.Y += 0.3f;
		}

		public override bool? CanCutTiles()
		{
			return false;
		}

		public override bool MinionContactDamage()
		{
			return false;
		}

        public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac)
		{
			fallThrough = false;
			return true;
		}

		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			if (oldVelocity.Y > 0.6f)
			{
				SoundEngine.PlaySound(SoundID.NPCDeath14, Projectile.Center);
				Projectile.position.Y += Projectile.velocity.Y;
				Projectile.velocity.Y = 0;

				for (int num231 = 0; num231 < 20; num231++)
				{
					int num217 = Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y + Projectile.height * 3 / 4), Projectile.width, Projectile.height / 4, DustID.Smoke, 0f, 0f, 100, default(Color), 1.5f);
					Dust dust71 = Main.dust[num217];
					Dust dust362 = dust71;
					dust362.velocity *= 1.4f;
				}
				Vector2 position67 = new Vector2(Projectile.position.X, Projectile.position.Y + Projectile.height * 3 / 4);
				Vector2 val = default(Vector2);
				int num229 = Gore.NewGore(Projectile.GetSource_FromAI(), position67, val, Main.rand.Next(61, 64));
				Gore gore20 = Main.gore[num229];
				Gore gore76 = gore20;
				gore76.velocity *= 0.4f;
				Main.gore[num229].velocity.X += 1f;
				Main.gore[num229].velocity.Y += 1f;
				Vector2 position68 = new Vector2(Projectile.position.X, Projectile.position.Y + Projectile.height * 3 / 4);
				val = default(Vector2);
				num229 = Gore.NewGore(Projectile.GetSource_FromAI(), position68, val, Main.rand.Next(61, 64));
				gore20 = Main.gore[num229];
				gore76 = gore20;
				gore76.velocity *= 0.4f;
				Main.gore[num229].velocity.X -= 1f;
				Main.gore[num229].velocity.Y += 1f;
				Vector2 position69 = new Vector2(Projectile.position.X, Projectile.position.Y + Projectile.height * 3 / 4);
				val = default(Vector2);
				num229 = Gore.NewGore(Projectile.GetSource_FromAI(), position69, val, Main.rand.Next(61, 64));
				gore20 = Main.gore[num229];
				gore76 = gore20;
				gore76.velocity *= 0.4f;
				Main.gore[num229].velocity.X += 1f;
				Main.gore[num229].velocity.Y -= 1f;
				Vector2 position70 = new Vector2(Projectile.position.X, Projectile.position.Y + Projectile.height * 3 / 4);
				val = default(Vector2);
				num229 = Gore.NewGore(Projectile.GetSource_FromAI(), position70, val, Main.rand.Next(61, 64));
				gore20 = Main.gore[num229];
				gore76 = gore20;
				gore76.velocity *= 0.4f;
				Main.gore[num229].velocity.X -= 1f;
				Main.gore[num229].velocity.Y -= 1f;
			}

			return false;
		}
	}

	public class GlobusCrucigerMinionBlade : ModProjectile
	{
		public override void SetStaticDefaults()
		{
			ProjectileID.Sets.MinionShot[Projectile.type] = true;
		}

		public override void SetDefaults()
		{
			Projectile.width = 14;
			Projectile.height = 14;
			DrawOffsetX = -34;
			DrawOriginOffsetX = 17;
			Projectile.DamageType = DamageClass.Summon;
			Projectile.penetrate = -1;
			Projectile.friendly = true;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.timeLeft = 600;
		}

		public override void AI()
		{
			if (Projectile.localAI[0] == 0)
			{
				Projectile.localAI[0] = Main.projectile[(int)Projectile.ai[0]].Center.X;
				Projectile.localAI[1] = Main.projectile[(int)Projectile.ai[0]].Center.Y;
				Projectile.Center += Projectile.velocity;
			}

			Player player = Main.player[Projectile.owner];

			if (!player.channel || !player.active || player.dead || Projectile.localAI[0] != Main.projectile[(int)Projectile.ai[0]].Center.X || Projectile.localAI[1] != Main.projectile[(int)Projectile.ai[0]].Center.Y)
			{
				Projectile.Kill();
				return;
			}

			float oldAI1 = Projectile.ai[1];
			Projectile.ai[1] += 16;
			Projectile.ai[1] *= 0.98f;

			Vector2 mainCenter = new Vector2(Projectile.localAI[0], Projectile.localAI[1]);
			Vector2 goalPosition = mainCenter + new Vector2(Projectile.ai[1], 0).RotatedBy((Projectile.Center - mainCenter).ToRotation() + Math.Sqrt(256 - (Projectile.ai[1] - oldAI1) * (Projectile.ai[1] - oldAI1)) / Projectile.ai[1]);
			Projectile.velocity = goalPosition - Projectile.Center;
			Projectile.rotation = Projectile.velocity.ToRotation();
		}

		public override bool? CanCutTiles()
		{
			return false;
		}
	}

	public class GlobusCrucigerMinionBlade2 : ModProjectile
	{
		public override string Texture => "Polarities/Items/Weapons/Summon/Orbs/GlobusCrucigerMinionBlade";

		public override void SetStaticDefaults()
		{
			// DisplayName.SetDefault("{$Mods.Polarities.ProjectileName.GlobusCrucigerMinionBlade}");
			ProjectileID.Sets.MinionShot[Projectile.type] = true;
		}

		public override void SetDefaults()
		{
			Projectile.width = 14;
			Projectile.height = 14;
			DrawOffsetX = -34;
			DrawOriginOffsetX = 17;
			Projectile.DamageType = DamageClass.Summon;
			Projectile.penetrate = -1;
			Projectile.friendly = true;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.timeLeft = 600;
		}

		public override void AI()
		{
			if (Projectile.localAI[0] == 0)
			{
				Projectile.localAI[0] = Main.projectile[(int)Projectile.ai[0]].Center.X;
				Projectile.localAI[1] = Main.projectile[(int)Projectile.ai[0]].Center.Y;
				Projectile.Center += Projectile.velocity;
			}

			Player player = Main.player[Projectile.owner];

			if (!player.channel || !player.active || player.dead || Projectile.localAI[0] != Main.projectile[(int)Projectile.ai[0]].Center.X || Projectile.localAI[1] != Main.projectile[(int)Projectile.ai[0]].Center.Y)
			{
				Projectile.Kill();
				return;
			}

			float oldAI1 = Projectile.ai[1];
			Projectile.ai[1] += 16;
			Projectile.ai[1] *= 0.98f;

			Vector2 mainCenter = new Vector2(Projectile.localAI[0], Projectile.localAI[1]);
			Vector2 goalPosition = mainCenter + new Vector2(Projectile.ai[1], 0).RotatedBy((Projectile.Center - mainCenter).ToRotation() - Math.Sqrt(256 - (Projectile.ai[1] - oldAI1) * (Projectile.ai[1] - oldAI1)) / Projectile.ai[1]);
			Projectile.velocity = goalPosition - Projectile.Center;
			Projectile.rotation = Projectile.velocity.ToRotation();
		}

		public override bool? CanCutTiles()
		{
			return false;
		}
	}

	public class GlobusCrucigerMinionBlade3 : ModProjectile
	{
		public override string Texture => "Polarities/Items/Weapons/Summon/Orbs/GlobusCrucigerMinionBlade";

		public override void SetStaticDefaults()
		{
			// DisplayName.SetDefault("{$Mods.Polarities.ProjectileName.GlobusCrucigerMinionBlade}");
			ProjectileID.Sets.MinionShot[Projectile.type] = true;
		}

		public override void SetDefaults()
		{
			Projectile.width = 14;
			Projectile.height = 14;
			DrawOffsetX = -34;
			DrawOriginOffsetX = 17;
			Projectile.DamageType = DamageClass.Summon;
			Projectile.penetrate = -1;
			Projectile.friendly = true;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.timeLeft = 33;
		}

		public override void AI()
		{
			Projectile.rotation = Projectile.velocity.ToRotation();
		}

		public override bool? CanCutTiles()
		{
			return false;
		}
	}
}