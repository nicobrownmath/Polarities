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
using Polarities.Items.Placeable.Bars;
using Terraria.Audio;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.GameContent;

namespace Polarities.Items.Weapons.Summon.Minions
{
	public class WingedStarStaff : ModItem
	{
		public override void SetStaticDefaults()
		{
			this.SetResearch(1);
		}

		public override void SetDefaults()
		{
			Item.SetWeaponValues(20, 1f, 0);
			Item.DamageType = DamageClass.Summon;
			Item.mana = 10;

			Item.width = 42;
			Item.height = 42;

			Item.useTime = 30;
			Item.useAnimation = 30;
			Item.useStyle = 1;
			Item.noMelee = true;
			Item.UseSound = SoundID.Item9;
			Item.autoReuse = true;

			Item.buffType = BuffType<WingedStarMinionBuff>();
			Item.shoot = ProjectileType<WingedStarMinion>();

			Item.value = 5000;
			Item.rare = ItemRarityID.Blue;
		}

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
		{
			player.AddBuff(Item.buffType, 18000, true);
			player.SpawnMinionOnCursor(source, player.whoAmI, type, damage, knockback);
			return false;
		}

		public override void AddRecipes()
		{
			CreateRecipe()
				.AddIngredient(ItemType<SunplateBar>(), 8)
				.AddIngredient(ItemID.FallenStar, 10)
				.AddTile(TileID.SkyMill)
				.Register();
		}
	}

	public class WingedStarMinionBuff : ModBuff
	{
		public override void SetStaticDefaults()
		{
			Main.buffNoTimeDisplay[Type] = true;
			Main.buffNoSave[Type] = true;
		}

		public override void Update(Player player, ref int buffIndex)
		{
			if (player.ownedProjectileCounts[ProjectileType<WingedStarMinion>()] > 0)
			{
				player.buffTime[buffIndex] = 18000;
			}
			else
			{
				player.DelBuff(buffIndex);
				buffIndex--;
			}
		}
	}

	public class WingedStarMinion : ModProjectile
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

		private int target;
		private Vector2 playerDisplacement;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Winged Star");
			Main.projPet[Projectile.type] = true;
			ProjectileID.Sets.MinionSacrificable[Projectile.type] = true;
			ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true;
			ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true;
		}

		public override void SetDefaults()
		{
			Projectile.width = 22;
			Projectile.height = 22;
			Projectile.penetrate = -1;
			Projectile.minion = true;
			Projectile.minionSlots = 1f;
			Projectile.friendly = true;
			Projectile.tileCollide = true;
			Projectile.DamageType = DamageClass.Summon;

			Projectile.localNPCHitCooldown = 10;
			Projectile.usesLocalNPCImmunity = true;
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
				player.ClearBuff(BuffType<WingedStarMinionBuff>());
			}
			if (player.HasBuff(BuffType<WingedStarMinionBuff>()))
			{
				Projectile.timeLeft = 2;
			}

			bool justImpacted = false;
			if (Projectile.velocity.Y <= 0 && target != -1)
			{
				//burst on impact
				justImpacted = true;

				if (Projectile.localAI[1] == 0)
				{
					Projectile.localAI[1]++;

					Color newColor7 = Color.CornflowerBlue;
					if (Main.tenthAnniversaryWorld)
					{
						newColor7 = Color.HotPink;
						newColor7.A = (byte)(newColor7.A / 2);
					}
					for (int num573 = 0; num573 < 7; num573++)
					{
						Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, 58, Projectile.velocity.X * 0.1f, Projectile.velocity.Y * 0.1f, 150, default(Color), 0.8f);
					}
					for (float num574 = 0f; num574 < 1f; num574 += 0.125f)
					{
						Vector2 center25 = Projectile.Center;
						Vector2 unitY11 = Vector2.UnitY;
						double radians36 = num574 * ((float)Math.PI * 2f) + Main.rand.NextFloat() * 0.5f;
						Vector2 center2 = default(Vector2);
						Dust.NewDustPerfect(center25, 278, unitY11.RotatedBy(radians36, center2) * (4f + Main.rand.NextFloat() * 4f), 150, newColor7).noGravity = true;
					}
					for (float num575 = 0f; num575 < 1f; num575 += 0.25f)
					{
						Vector2 center26 = Projectile.Center;
						Vector2 unitY12 = Vector2.UnitY;
						double radians37 = num575 * ((float)Math.PI * 2f) + Main.rand.NextFloat() * 0.5f;
						Vector2 center2 = default(Vector2);
						Dust.NewDustPerfect(center26, 278, unitY12.RotatedBy(radians37, center2) * (2f + Main.rand.NextFloat() * 3f), 150, Color.Gold).noGravity = true;
					}
					Vector2 value15 = new Vector2((float)Main.screenWidth, (float)Main.screenHeight);
					if (Projectile.Hitbox.Intersects(Utils.CenteredRectangle(Main.screenPosition + value15 / 2f, value15 + new Vector2(400f))))
					{
						for (int num576 = 0; num576 < 7; num576++)
						{
							Vector2 val29 = Projectile.position;
							Vector2 val30 = Main.rand.NextVector2CircularEdge(0.5f, 0.5f) * 16;
							int[] array18 = new int[8] { 16, 17, 17, 17, 17, 17, 17, 17 };
							Gore.NewGore(Projectile.GetSource_FromAI(), val29, val30, Utils.SelectRandom(Main.rand, array18));
						}
					}
				}

				target = -1;
				Projectile.ai[0] = 0;
			}

			if (player.HasMinionAttackTargetNPC)
			{
				target = player.MinionAttackTargetNPC;
			}

			if (target == -1 || !Main.npc[target].active || !Main.npc[target].chaseable || Main.npc[target].dontTakeDamage || (Main.npc[target].Center - Main.player[Projectile.owner].Center).Length() > 1000)
			{
				Projectile.Minion_FindTargetInRange(750, ref target, false);
			}

			if (Main.rand.NextBool(60) && Main.myPlayer == Projectile.owner)
			{
				playerDisplacement = new Vector2(Main.rand.Next(-100, 101), Main.rand.Next(-100, 0));
			}

			Vector2 targetPos = player.Center + playerDisplacement;
			Vector2 targetVel = player.velocity;
			if (target != -1)
			{
				Projectile.tileCollide = Projectile.position.Y + Projectile.height > Main.npc[target].Center.Y;

				int distanceToTarget = Main.rand.Next(400, 600);

				targetPos = Main.npc[target].Center + Main.npc[target].velocity * distanceToTarget / 16;

				if (Projectile.ai[0] == 0)
				{
					Projectile.ai[1] = 0;
					Projectile.ai[0] = 1;

					//teleport to above target and move at them
					Vector2 goalPosition = targetPos + new Vector2(0, -distanceToTarget).RotatedByRandom(1);

					if (!Main.tileSolid[Main.tile[(int)(goalPosition.X / 16), (int)(goalPosition.Y / 16)].TileType] || !Main.tile[(int)(goalPosition.X / 16), (int)(goalPosition.Y / 16)].HasUnactuatedTile)
					{
						if (!justImpacted)
						{
							TeleportBurst();
						}

						Projectile.position = goalPosition - Projectile.Hitbox.Size() / 2;

						Projectile.velocity = (targetPos - Projectile.Center).SafeNormalize(Vector2.Zero);
						Projectile.velocity *= 16;

						TeleportBurst();
					}
					else
					{
						target = -1;
						Projectile.ai[0] = 0;
					}
				}
				else
				{
					Projectile.ai[1]++;
					if (Projectile.ai[1] == 120)
					{
						Projectile.ai[0] = 0;
					}
				}
			}
			else
			{
				Projectile.tileCollide = true;
				Projectile.ai[0] = 0;

				Vector2 acc = (targetPos - Projectile.Center) / 10 + (targetVel - Projectile.velocity);
				acc.X *= 2;
				acc.Y -= 1f;
				if (acc.Length() > 1) { acc.Normalize(); }
				acc *= 0.5f;
				Projectile.velocity += acc;
				if (Projectile.velocity.Length() > 20)
				{
					Projectile.velocity.Normalize();
					Projectile.velocity *= 20;
				}
			}
			if ((Projectile.Center - player.Center).Length() > 1000 && Projectile.ai[0] == 0)
			{
				Projectile.position = player.Center - new Vector2(Projectile.width / 2, Projectile.height / 2);
				target = -1;
				Projectile.ai[0] = 0;
			}

			Lighting.AddLight(Projectile.Center, Color.Yellow.ToVector3());

			if (target == -1)
			{
				//fallen star item dust visuals
				Projectile.localAI[1]++;
				if (Projectile.localAI[1] % 12 == 0)
				{
					Dust dust = Dust.NewDustPerfect(Projectile.Center + new Vector2(0f, (float)Projectile.height * 0.2f) + Main.rand.NextVector2CircularEdge(Projectile.width, (float)Projectile.height * 0.6f) * (0.3f + Main.rand.NextFloat() * 0.5f), 228, (Vector2?)new Vector2(0f, (0f - Main.rand.NextFloat()) * 0.3f - 1.5f), 127, default(Color), 1f);
					dust.scale = 0.5f;
					dust.fadeIn = 1.1f;
					dust.noGravity = true;
					dust.noLight = true;
				}

				Projectile.rotation = 0.1f * Projectile.velocity.X;
				Projectile.localAI[0] = (Projectile.localAI[0] + 0.2f) % MathHelper.TwoPi;
				wingRotationOffset = (float)Math.Sin(Projectile.localAI[0]) * 0.75f - 0.1f;
				wingScale = new Vector2((-(float)Math.Cos(Projectile.localAI[0]) + 4) / 3, ((float)Math.Cos(Projectile.localAI[0]) + 2) / 3);
			}
			else
			{
				Vector2 value34 = new Vector2((float)Main.screenWidth, (float)Main.screenHeight);
				if (Projectile.Hitbox.Intersects(Utils.CenteredRectangle(Main.screenPosition + value34 / 2f, value34 + new Vector2(400f))) && Main.rand.Next(6) == 0)
				{
					int[] array6 = new int[4] { 16, 17, 17, 17 };
					int num855 = Utils.SelectRandom(Main.rand, array6);
					if (Main.tenthAnniversaryWorld)
					{
						int[] array7 = new int[4] { 16, 16, 16, 17 };
						num855 = Utils.SelectRandom(Main.rand, array7);
					}
					Gore.NewGore(Projectile.GetSource_FromAI(), Projectile.position, Projectile.velocity * 0.2f, num855);
				}
				if (Main.rand.Next(20) == 0 || (Main.tenthAnniversaryWorld && Main.rand.Next(15) == 0))
				{
					Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, 58, Projectile.velocity.X * 0.5f, Projectile.velocity.Y * 0.5f, 150, default(Color), 1.2f);
				}

				Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
				wingRotationOffset = 0;
				wingScale = new Vector2(2 / 3f, 1);
			}
		}

		void TeleportBurst()
        {
			Color newColor7 = Color.CornflowerBlue;
			if (Main.tenthAnniversaryWorld)
			{
				newColor7 = Color.HotPink;
				newColor7.A = (byte)(newColor7.A / 2);
			}
			for (int num573 = 0; num573 < 7 / 2; num573++)
			{
				Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, 58, Projectile.velocity.X * 0.05f, Projectile.velocity.Y * 0.05f, 150, default(Color), 0.8f);
			}
			for (float num574 = 0f; num574 < 1f / 2; num574 += 0.125f)
			{
				Vector2 center25 = Projectile.Center;
				Vector2 unitY11 = Vector2.UnitY;
				double radians36 = num574 * ((float)Math.PI * 2f) + Main.rand.NextFloat() * 0.5f;
				Vector2 center2 = default(Vector2);
				Dust.NewDustPerfect(center25, 278, unitY11.RotatedBy(radians36, center2) * (4f + Main.rand.NextFloat() * 4f), 150, newColor7).noGravity = true;
			}
			for (float num575 = 0f; num575 < 1f / 2; num575 += 0.25f)
			{
				Vector2 center26 = Projectile.Center;
				Vector2 unitY12 = Vector2.UnitY;
				double radians37 = num575 * ((float)Math.PI * 2f) + Main.rand.NextFloat() * 0.5f;
				Vector2 center2 = default(Vector2);
				Dust.NewDustPerfect(center26, 278, unitY12.RotatedBy(radians37, center2) * (2f + Main.rand.NextFloat() * 3f), 150, Color.Gold).noGravity = true;
			}
			Vector2 value15 = new Vector2((float)Main.screenWidth, (float)Main.screenHeight);
			if (Projectile.Hitbox.Intersects(Utils.CenteredRectangle(Main.screenPosition + value15 / 2f, value15 + new Vector2(400f))))
			{
				for (int num576 = 0; num576 < 7 / 2; num576++)
				{
					Vector2 val29 = Projectile.position;
					Vector2 val30 = Main.rand.NextVector2CircularEdge(0.5f, 0.5f) * Projectile.velocity.Length() / 2f;
					int[] array18 = new int[8] { 16, 17, 17, 17, 17, 17, 17, 17 };
					Gore.NewGore(Projectile.GetSource_FromAI(), val29, val30, Utils.SelectRandom(Main.rand, array18));
				}
			}
		}

		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			if (target == -1)
			{
				if (Projectile.velocity.X != oldVelocity.X)
				{
					Projectile.velocity.X = -oldVelocity.X;
				}
				if (Projectile.velocity.Y != oldVelocity.Y)
				{
					Projectile.velocity.Y = -oldVelocity.Y;
				}
			}
			else
            {
				Projectile.velocity.Y = 0;

				SoundEngine.PlaySound(SoundID.Item10, Projectile.position);

				Color newColor7 = Color.CornflowerBlue;
				if (Main.tenthAnniversaryWorld)
				{
					newColor7 = Color.HotPink;
					newColor7.A = (byte)(newColor7.A / 2);
				}
				for (int num573 = 0; num573 < 7; num573++)
				{
					Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, 58, oldVelocity.X * 0.1f, oldVelocity.Y * 0.1f, 150, default(Color), 0.8f);
				}
				for (float num574 = 0f; num574 < 1f; num574 += 0.125f)
				{
					Vector2 center25 = Projectile.Center;
					Vector2 unitY11 = Vector2.UnitY;
					double radians36 = num574 * ((float)Math.PI * 2f) + Main.rand.NextFloat() * 0.5f;
					Vector2 center2 = default(Vector2);
					Dust.NewDustPerfect(center25, 278, unitY11.RotatedBy(radians36, center2) * (4f + Main.rand.NextFloat() * 4f), 150, newColor7).noGravity = true;
				}
				for (float num575 = 0f; num575 < 1f; num575 += 0.25f)
				{
					Vector2 center26 = Projectile.Center;
					Vector2 unitY12 = Vector2.UnitY;
					double radians37 = num575 * ((float)Math.PI * 2f) + Main.rand.NextFloat() * 0.5f;
					Vector2 center2 = default(Vector2);
					Dust.NewDustPerfect(center26, 278, unitY12.RotatedBy(radians37, center2) * (2f + Main.rand.NextFloat() * 3f), 150, Color.Gold).noGravity = true;
				}
				Vector2 value15 = new Vector2((float)Main.screenWidth, (float)Main.screenHeight);
				if (Projectile.Hitbox.Intersects(Utils.CenteredRectangle(Main.screenPosition + value15 / 2f, value15 + new Vector2(400f))))
				{
					for (int num576 = 0; num576 < 7; num576++)
					{
						Vector2 val29 = Projectile.position;
						Vector2 val30 = Main.rand.NextVector2CircularEdge(0.5f, 0.5f) * oldVelocity.Length();
						int[] array18 = new int[8] { 16, 17, 17, 17, 17, 17, 17, 17 };
						Gore.NewGore(Projectile.GetSource_FromAI(), val29, val30, Utils.SelectRandom(Main.rand, array18));
					}
				}
			}
			return false;
		}

        public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac)
		{
			if (target != -1)
			{
				fallThrough = Main.npc[target].Center.Y > Projectile.Center.Y;
			}
			else
			{
				fallThrough = true;
			}
			return true;
		}

		public override bool? CanCutTiles()
		{
			return false;
		}

		public override bool MinionContactDamage()
		{
			return target != -1;
		}

		float wingRotationOffset;
		Vector2 wingScale;

        public override bool PreDraw(ref Color lightColor)
        {
			//fallen star trail
			if (target != -1)
			{
				Texture2D value175 = TextureAssets.Extra[91].Value;
				Rectangle value176 = value175.Frame();
				Vector2 origin10 = new Vector2((float)value176.Width / 2f, 10f);
				Vector2 value177 = new Vector2(0f, Projectile.gfxOffY);
				Vector2 spinningpoint = new Vector2(0f, -10f);
				float num184 = (float)Main.timeForVisualEffects / 60f;
				Vector2 value178 = Projectile.Center + Projectile.velocity;
				Color color42 = Color.Blue * 0.2f;
				Color value179 = Color.White * 0.5f;
				value179.A = 0;
				float num185 = 0f;
				if (Main.tenthAnniversaryWorld)
				{
					color42 = Color.HotPink * 0.3f;
					value179 = Color.White * 0.75f;
					value179.A = 0;
					num185 = -0.1f;
				}
				Color color43 = color42;
				color43.A = 0;
				Color color44 = color42;
				color44.A = 0;
				Color color45 = color42;
				color45.A = 0;
				Vector2 val8 = value178 - Main.screenPosition + value177;
				Vector2 spinningpoint17 = spinningpoint;
				double radians6 = (float)Math.PI * 2f * num184;
				Vector2 val2 = default(Vector2);
				Main.EntitySpriteDraw(value175, val8 + spinningpoint17.RotatedBy(radians6, val2), value176, color43, Projectile.velocity.ToRotation() + (float)Math.PI / 2f, origin10, 1.5f + num185, (SpriteEffects)0, 0);
				Vector2 val9 = value178 - Main.screenPosition + value177;
				Vector2 spinningpoint18 = spinningpoint;
				double radians7 = (float)Math.PI * 2f * num184 + (float)Math.PI * 2f / 3f;
				val2 = default(Vector2);
				Main.EntitySpriteDraw(value175, val9 + spinningpoint18.RotatedBy(radians7, val2), value176, color44, Projectile.velocity.ToRotation() + (float)Math.PI / 2f, origin10, 1.1f + num185, (SpriteEffects)0, 0);
				Vector2 val10 = value178 - Main.screenPosition + value177;
				Vector2 spinningpoint19 = spinningpoint;
				double radians8 = (float)Math.PI * 2f * num184 + 4.1887903f;
				val2 = default(Vector2);
				Main.EntitySpriteDraw(value175, val10 + spinningpoint19.RotatedBy(radians8, val2), value176, color45, Projectile.velocity.ToRotation() + (float)Math.PI / 2f, origin10, 1.3f + num185, (SpriteEffects)0, 0);
				Vector2 value180 = Projectile.Center - Projectile.velocity * 0.5f;
				for (float num186 = 0f; num186 < 1f; num186 += 0.5f)
				{
					float num187 = num184 % 0.5f / 0.5f;
					num187 = (num187 + num186) % 1f;
					float num188 = num187 * 2f;
					if (num188 > 1f)
					{
						num188 = 2f - num188;
					}
					Main.EntitySpriteDraw(value175, value180 - Main.screenPosition + value177, value176, value179 * num188, Projectile.velocity.ToRotation() + (float)Math.PI / 2f, origin10, 0.3f + num187 * 0.5f, (SpriteEffects)0, 0);
				}
			}

			for (int i = 0; i < 2; i++)
            {
				Main.EntitySpriteDraw(WingTexture.Value, Projectile.Center - Main.screenPosition, WingTexture.Value.Frame(), Color.White, Projectile.rotation - (i * 2 - 1) * wingRotationOffset, WingTexture.Value.Frame().Size() / 2, Projectile.scale * wingScale, (SpriteEffects)i, 0);
            }

			lightColor = Color.White;
			return true;
        }
    }
}