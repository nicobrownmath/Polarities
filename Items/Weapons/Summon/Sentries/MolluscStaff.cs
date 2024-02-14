using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using static Terraria.ModLoader.ModContent;
using Microsoft.Xna.Framework;
using System;
using System.IO;
using Polarities.Buffs;
using Terraria.DataStructures;
using Terraria.Audio;

namespace Polarities.Items.Weapons.Summon.Sentries
{
	public class MolluscStaff : ModItem
	{
		public override void SetStaticDefaults()
		{
			this.SetResearch(1);
		}

		public override void SetDefaults()
		{
			Item.SetWeaponValues(25, 5f, 0);
			Item.DamageType = DamageClass.Summon;
			Item.sentry = true;
			Item.mana = 5;

			Item.width = 68;
			Item.height = 60;

			Item.useTime = 30;
			Item.useAnimation = 30;
			Item.useStyle = 1;
			Item.noMelee = true;
			Item.UseSound = SoundID.Item44;
			Item.autoReuse = true;

			Item.shoot = ProjectileType<MolluscStaffSentry>();
			Item.shootSpeed = 0f;

			Item.value = 5000;
			Item.rare = 2;
		}

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
		{
			player.SpawnSentry(source, player.whoAmI, type, damage, knockback);
			return false;
		}
	}

	public class MolluscStaffSentry : ModProjectile
	{
		private NPC target;

		public override void SetStaticDefaults()
		{
			Main.projFrames[Projectile.type] = 2;
			ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true;
			ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true;
		}

		public override void SetDefaults()
		{
			Projectile.width = 40;
			Projectile.height = 40;
			DrawOriginOffsetY = 4;
			Projectile.penetrate = -1;
			Projectile.sentry = true;
			Projectile.DamageType = DamageClass.Summon;
			Projectile.friendly = true;
			Projectile.ignoreWater = true;
			Projectile.tileCollide = true;
			Projectile.timeLeft = Projectile.SentryLifeTime;
		}

		public override void AI()
		{
			Player player = Main.player[Projectile.owner];

			double minVal;
			Vector2 newMove;
			Projectile.velocity.Y++;

			float v = 8;
			float a = 0.1f;

			bool check = false;
			if (target != null)
			{
				float x = target.Center.X - Projectile.Center.X;
				float y = target.Center.Y - Projectile.Center.Y;
				double val = -4 * Math.Pow(a, 2) * Math.Pow(x, 2) + 4 * a * Math.Pow(v, 2) * y + Math.Pow(v, 4);
				check = val < 0;
			}

			if (check || target == null || !target.active || !target.chaseable || target.dontTakeDamage)
			{
				minVal = 0;

				bool isTarget = false;
				int targetID = -1;
				for (int k = 0; k < 200; k++)
				{
					if (Main.npc[k].active && !Main.npc[k].dontTakeDamage && !Main.npc[k].friendly && Main.npc[k].lifeMax > 5 && !Main.npc[k].immortal && Main.npc[k].chaseable)
					{
						newMove = Main.npc[k].Center - Projectile.Center;
						double val = -4 * Math.Pow(a, 2) * Math.Pow(newMove.X, 2) + 4 * a * Math.Pow(v, 2) * newMove.Y + Math.Pow(v, 4);
						if (val > minVal)
						{
							targetID = k;
							minVal = val;
							isTarget = true;
						}
					}
				}

				if (isTarget)
				{
					target = Main.npc[targetID];
				}
				else if (!check)
				{
					target = null;
					Projectile.ai[0] = 0;
					Projectile.frame = 1;
					return;
				}
			}

			if (player.HasMinionAttackTargetNPC)
			{
				target = Main.npc[player.MinionAttackTargetNPC];
			}

			Projectile.direction = target.Center.X > Projectile.Center.X ? 1 : -1;
			Projectile.spriteDirection = -Projectile.direction;

			Projectile.ai[0]++;
			if (Projectile.ai[0] >= 240)
			{
				Projectile.ai[0] = 0;
			}

			Projectile.frame = ((30 + Projectile.ai[0]) % 240) < 60 ? 1 : 0;

			if (Projectile.ai[0] % 60 == 0 && Projectile.ai[0] != 0)
			{
				//shoot pearl
				//a = 0.2 units per tick per tick
				//v = 8 ticks per second
				//we want the path to intersect playerX, playerY
				//x = 8cos(theta)*t
				//y = 8sin(theta)*t+0.1*t^2
				float x = target.Center.X - Projectile.Center.X;
				float y = target.Center.Y - Projectile.Center.Y;

				double theta = (new Vector2(x, y)).ToRotation();
				theta += Math.PI / 2;
				if (theta > Math.PI) { theta -= Math.PI * 2; }
				theta *= 0.5;
				theta -= Math.PI / 2;

				double num0 = -4 * Math.Pow(a, 2) * Math.Pow(x, 4) + 4 * a * Math.Pow(v, 2) * Math.Pow(x, 2) * y + Math.Pow(v, 4) * Math.Pow(x, 2);
				if (num0 > 0)
				{
					num0 = Projectile.direction * Math.Sqrt(num0);
					double num1 = a * x * x - v * v * y;

					theta = -2 * Math.Atan(
						num0 / (2 * num1) +
						0.5 * Math.Sqrt(
							-(
								(num1 * (-16 * Math.Pow(v, 2) * x * (a * x * x + v * v * y) / Math.Pow(num1, 2) - 16 * Math.Pow(v, 2) * x / num1 + 8 * Math.Pow(v, 6) * Math.Pow(x, 3) / Math.Pow(num1, 3))) /
								(4 * num0)
							)
							- 2 * (a * x * x + v * v * y) / num1 + 2 + 2 * Math.Pow(v, 4) * Math.Pow(x, 2) / (num1 * num1)
						) -
						Math.Pow(v, 2) * x / (2 * num1)
					); //some magic thingy idk

					int thetaDir = Math.Cos(theta) < 0 ? -1 : 1;
					if (thetaDir != Projectile.direction) { theta -= Math.PI; }
				}
				Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, 8 * (new Vector2((float)Math.Cos(theta), (float)Math.Sin(theta))), ProjectileType<MolluscStaffSentryProjectile>(), Projectile.damage, 5f, Projectile.owner);
				SoundEngine.PlaySound(SoundID.Item61, Projectile.Center);
			}
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
			return false;
		}
	}

	public class MolluscStaffSentryProjectile : ModProjectile
	{
		public override string Texture => "Polarities/NPCs/Enemies/Salt/MusselProjectile";

        public override void SetStaticDefaults()
		{
			// DisplayName.SetDefault("{$Mods.Polarities.ProjectileName.MusselProjectile}");
			ProjectileID.Sets.SentryShot[Projectile.type] = true;
		}

		public override void SetDefaults()
		{
			Projectile.DamageType = DamageClass.Summon;
			Projectile.aiStyle = -1;
			Projectile.width = 24;
			Projectile.height = 24;
			Projectile.timeLeft = 1200;
			Projectile.penetrate = -1;
			Projectile.friendly = true;
			Projectile.tileCollide = true;
			Projectile.ignoreWater = true;
			Projectile.light = 0.3f;
		}

		public override void AI()
		{
			Projectile.rotation += Projectile.velocity.X / Projectile.width * 2;
			Projectile.velocity.Y += 0.2f;
		}

		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			if (Projectile.velocity.X != oldVelocity.X)
			{
				Projectile.velocity.X = -oldVelocity.X;
				Projectile.velocity *= 0.9f;
			}
			if (Projectile.velocity.Y != oldVelocity.Y)
			{
				Projectile.velocity.Y = -oldVelocity.Y;
				Projectile.velocity *= 0.9f;
			}
			if (Projectile.timeLeft < 600)
			{
				Projectile.Kill();
			}
			return false;
		}

		public override bool? CanCutTiles()
		{
			return false;
		}

		public override void OnKill(int timeLeft)
		{
			SoundEngine.PlaySound(SoundID.Shatter, Projectile.Center);
			for (int i = 0; i < 3; i++)
			{
				Gore.NewGore(Projectile.GetSource_Death(), Projectile.Center, Projectile.velocity, Mod.Find<ModGore>("PearlShard").Type);
			}
		}
	}
}