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
using System.Collections.Generic;
using Terraria.GameContent;
using ReLogic.Content;

namespace Polarities.Items.Weapons.Summon.Orbs
{
	public class WeaverOrb : ModItem
	{
		public override void SetStaticDefaults()
		{
			this.SetResearch(1);
		}

		public override void SetDefaults()
		{
			Item.SetWeaponValues(31, 0, 0);
			Item.DamageType = DamageClass.Summon;

			Item.width = 22;
			Item.height = 38;

			Item.useTime = 30;
			Item.useAnimation = 30;
			Item.channel = true;
			Item.useStyle = ItemUseStyleID.HoldUp;
			Item.UseSound = SoundID.Item17;
			Item.noMelee = true;

			Item.shoot = ProjectileType<WeaverOrbMinion>();
			Item.shootSpeed = 2f;

			Item.value = Item.sellPrice(gold: 4);
			Item.rare = ItemRarityID.LightRed;
		}

		public override void HoldItem(Player player)
		{
			if (player.channel)
			{
				player.direction = (Main.MouseWorld.X - player.Center.X > 0) ? 1 : -1;
				player.itemTime = player.itemTimeMax;
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
				Main.projectile[Projectile.NewProjectile(source, position, velocity.RotatedBy(i * MathHelper.TwoPi / player.GetModPlayer<PolaritiesPlayer>().orbMinionSlots), type, damage, knockback, player.whoAmI)].originalDamage = damage;
			}
			return false;
		}

		public override void AddRecipes()
		{
			CreateRecipe()
				.AddIngredient(ItemID.SpiderFang, 24)
				.AddTile(TileID.Anvils)
				.Register();
		}
	}

	public class WeaverOrbMinion : ModProjectile
	{
		static Asset<Texture2D> ClawTexture;
		static Asset<Texture2D> LegTexture;

        public override void Load()
        {
			ClawTexture = Request<Texture2D>(Texture + "_Claw");
			LegTexture = Request<Texture2D>(Texture + "_Leg");
		}

        public override void Unload()
        {
			ClawTexture = null;
			LegTexture = null;
        }

        public override void SetStaticDefaults()
		{
			Main.projFrames[Projectile.type] = 1;
		}

		public override void SetDefaults()
		{
			Projectile.width = 36;
			Projectile.height = 36;
			Projectile.penetrate = -1;
			Projectile.minion = true;
			Projectile.DamageType = DamageClass.Summon;
			Projectile.friendly = true;
			Projectile.tileCollide = false;
			Projectile.timeLeft = 3600;
		}

		private float goalRotation;

		public override void AI()
		{
			if (Projectile.localAI[0] == 0)
			{
				Projectile.localAI[0] = 1;

				for (int i = 0; i < 8; i++)
				{
					legPositions[i] = Projectile.Center;
				}
				goalRotation = Projectile.rotation;
			}

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

			int targetID = -1;
			Projectile.Minion_FindTargetInRange(750, ref targetID, false);
			NPC target = null;
			if (targetID != -1)
			{
				target = Main.npc[targetID];
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

			bool notTargetReliant = Projectile.ai[0] != 0 || Projectile.ai[1] != 0;

			if (target != null || notTargetReliant)
			{
				switch (Projectile.ai[0])
				{
					case 0:
						//orbit the enemy along web lines
						if (Projectile.ai[1] == 540)
						{
							Projectile.ai[0] = 1;
							Projectile.ai[1] = 0;
							Projectile.velocity = Vector2.Zero;
							break;
						}
						if (Projectile.ai[1] % 90 == 0)
						{
							Projectile.velocity = Vector2.Zero;
							if (Projectile.ai[1] == 0)
								goalRotation = (target.Center - Projectile.Center).ToRotation() + MathHelper.Pi / 6 + 0.5f * ((2 * index - ownedProjectiles + 1) / (float)ownedProjectiles);
							else
								goalRotation += MathHelper.Pi / 3;

							SoundEngine.PlaySound(SoundID.Item17, Projectile.Center);
							Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, new Vector2(16, 0).RotatedBy(goalRotation - MathHelper.PiOver2), ProjectileType<WeaverOrbMinionWeb>(), Projectile.damage / 3, Projectile.knockBack, Projectile.owner, ai0: Projectile.ai[1]);
							Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, new Vector2(16, 0).RotatedBy(goalRotation - MathHelper.Pi / 6), ProjectileType<WeaverOrbMinionWeb>(), Projectile.damage / 3, Projectile.knockBack, Projectile.owner, ai0: Projectile.ai[1]);
						}
						else if (Projectile.ai[1] % 90 == 30)
						{
							if (Projectile.ai[1] == 480)
							{
								goalRotation += MathHelper.Pi / 3;
							}
							Projectile.velocity = new Vector2(8, 0).RotatedBy(goalRotation - MathHelper.PiOver2);
						}
						Projectile.ai[1]++;
						break;
					case 1:
						//spit webs the cursor
						float rotOffset = 0.2f * (float)Math.Sin(Projectile.ai[1] / 10);

						goalRotation = (Main.MouseWorld - Projectile.Center).ToRotation() + MathHelper.Pi / 2 + rotOffset;

						if (Projectile.ai[1] % 10 == 0 && Projectile.ai[1] >= 30 && Projectile.ai[1] <= 180)
						{
							SoundEngine.PlaySound(SoundID.Item17, Projectile.Center);
							Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, new Vector2(16, 0).RotatedBy(goalRotation - MathHelper.PiOver2), ProjectileType<WeaverOrbMinionWebBall>(), Projectile.damage, Projectile.knockBack, Projectile.owner);
						}
						if (Projectile.ai[1] == 209)
						{
							Projectile.ai[0] = 0;
							Projectile.ai[1] = 0;
							break;
						}
						Projectile.ai[1]++;
						break;
				}
			}
			else
			{
				Vector2 goalPosition = player.Center + new Vector2(0, -120);
				Vector2 goalVelocty = (goalPosition - Projectile.Center).RotatedBy(0.5f * ((2 * index - ownedProjectiles + 1) / (float)ownedProjectiles)).SafeNormalize(Vector2.Zero) * 12;
				if ((Projectile.Center - goalPosition).Length() > 64)
				{
					Projectile.velocity += (goalVelocty - Projectile.velocity) / 60;
				}
				goalRotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
			}

			UpdateLegData();

			if (goalRotation > MathHelper.TwoPi) goalRotation -= MathHelper.TwoPi;
			else if (goalRotation < 0) goalRotation += MathHelper.TwoPi;
			if (Projectile.rotation > MathHelper.TwoPi) Projectile.rotation -= MathHelper.TwoPi;
			else if (Projectile.rotation < 0) Projectile.rotation += MathHelper.TwoPi;

			float angleOffset = goalRotation - Projectile.rotation;
			if (angleOffset > MathHelper.Pi) angleOffset -= MathHelper.TwoPi;
			else if (angleOffset < -MathHelper.Pi) angleOffset += MathHelper.TwoPi;

			float maxTurn = 0.2f;

			if (Math.Abs(angleOffset) < maxTurn) { Projectile.rotation = goalRotation; }
			else if (angleOffset > 0)
			{
				Projectile.rotation += maxTurn;
			}
			else
			{
				Projectile.rotation -= maxTurn;
			}
		}

		private Vector2[] legPositions = new Vector2[8];
		private bool[] legStates = new bool[8];

		private void UpdateLegData()
		{
			for (int i = 0; i < 8; i++)
			{
				Vector2 center;
				if (i % 2 == 0)
				{
					center = Projectile.Center + new Vector2(8, 16).RotatedBy(Projectile.rotation) + Projectile.velocity;
				}
				else
				{
					center = Projectile.Center + new Vector2(-8, 16).RotatedBy(Projectile.rotation) + Projectile.velocity;
				}

				int xOffset = i % 2 == 0 ? 36 : -36;
				int yOffset = (i / 2) * 14 - 18;

				Vector2 legBasePosition = Projectile.Center + new Vector2(xOffset, yOffset).RotatedBy(Projectile.rotation);

				if (legStates[i]) //this is if the leg is in its stationary state
				{
					if ((legBasePosition - legPositions[i]).Length() > 32)
					{
						legStates[i] = false;
					}
				}
				else //this is if the leg is in its moving state
				{
					if ((legBasePosition - legPositions[i]).Length() < 4)
					{
						legPositions[i] = legBasePosition;
						legStates[i] = true;
					}
					else
					{

						float dRadius = (legBasePosition - center).Length() - (legPositions[i] - center).Length();
						float dAngle = (legBasePosition - center).ToRotation() - (legPositions[i] - center).ToRotation();

						while (dAngle > MathHelper.Pi)
						{
							dAngle -= MathHelper.TwoPi;
						}
						while (dAngle < -MathHelper.Pi)
						{
							dAngle += MathHelper.TwoPi;
						}

						legPositions[i] += new Vector2(dRadius, dAngle * (legPositions[i] - center).Length()).RotatedBy((legPositions[i] - center).ToRotation()).SafeNormalize(Vector2.Zero) * 4;

						//legPositions[i] += (legBasePosition - legPositions[i]).SafeNormalize(Vector2.Zero) * 4;
					}
					legPositions[i] += Projectile.velocity;
				}

				if ((legPositions[i] - center).Length() >= 46)
				{
					legPositions[i] = center + (legPositions[i] - center).SafeNormalize(Vector2.Zero) * 46;
					legStates[i] = false;
				}
			}
		}


		public override bool? CanCutTiles()
		{
			return false;
		}

		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
		{
			target.AddBuff(BuffID.Venom, 60 * 12);
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Player player = Main.player[Projectile.owner];

			Vector2 playerCenter = player.Center + new Vector2(player.direction * 8, -24);
			Vector2 center = Projectile.Center + new Vector2(0, 44).RotatedBy(Projectile.rotation);
			Vector2 distToProj = playerCenter - center;
			float projRotation = distToProj.ToRotation() - 1.57f;
			float distance = distToProj.Length();

			int frame = 0;

			while (distance > 4f && !float.IsNaN(distance))
			{
				distToProj.Normalize();                 //get unit vector
				distToProj *= 4f;
				center += distToProj;                   //update draw position
				distToProj = playerCenter - center;    //update distance
				distance = distToProj.Length();
				Color drawColor = Lighting.GetColor((int)(center.X / 16), (int)(center.Y / 16));

				frame = (frame + 4) % 16;

				//Draw chain
				Main.spriteBatch.Draw(TextureAssets.Projectile[ProjectileType<WeaverOrbMinionWeb>()].Value, new Vector2(center.X - Main.screenPosition.X, center.Y - Main.screenPosition.Y),
					new Rectangle(0, frame, 4, 4), drawColor, projRotation,
					new Vector2(4 * 0.5f, 4 * 0.5f), 1f, SpriteEffects.None, 0);
			}

			for (int i = 0; i < 8; i++)
			{
				if (i % 2 == 0)
				{
					Vector2 drawPosition = legPositions[i] - Main.screenPosition;

					center = Projectile.Center - Main.screenPosition + new Vector2(8, 16).RotatedBy(Projectile.rotation);

					int segmentLength = 24;

					float rotation = (center - drawPosition).ToRotation() + MathHelper.PiOver2 + (float)Math.Acos(((drawPosition - center) / 2).Length() / segmentLength);
					float rotation2 = (center - drawPosition).ToRotation() + MathHelper.PiOver2 - (float)Math.Acos(((drawPosition - center) / 2).Length() / segmentLength);

					Main.EntitySpriteDraw(ClawTexture.Value, drawPosition,
						new Rectangle(0, 0, 8, 32), lightColor, rotation,
						new Vector2(4, 28), 1f, SpriteEffects.FlipHorizontally, 0);

					Main.EntitySpriteDraw(LegTexture.Value, center,
						new Rectangle(0, 0, 8, 32), lightColor, rotation2,
						new Vector2(4, 4), 1f, SpriteEffects.FlipHorizontally, 0);
				}
				else
				{
					Vector2 drawPosition = legPositions[i] - Main.screenPosition;

					center = Projectile.Center - Main.screenPosition + new Vector2(-8, 16).RotatedBy(Projectile.rotation);

					int segmentLength = 24;

					float rotation = (drawPosition - center).ToRotation() - MathHelper.PiOver2 - (float)Math.Acos(((drawPosition - center) / 2).Length() / segmentLength);
					float rotation2 = (drawPosition - center).ToRotation() - MathHelper.PiOver2 + (float)Math.Acos(((drawPosition - center) / 2).Length() / segmentLength);

					Main.EntitySpriteDraw(ClawTexture.Value, drawPosition,
						new Rectangle(0, 0, 8, 32), lightColor, rotation,
						new Vector2(4, 28), 1f, SpriteEffects.None, 0);

					Main.EntitySpriteDraw(LegTexture.Value, center,
						new Rectangle(0, 0, 8, 32), lightColor, rotation2,
						new Vector2(4, 4), 1f, SpriteEffects.None, 0);
				}
			}

			return true;
		}
	}

	public class WeaverOrbMinionWeb : ModProjectile
	{
		public override void SetStaticDefaults()
		{
			ProjectileID.Sets.MinionShot[Projectile.type] = true;
		}

		public override void SetDefaults()
		{
			Projectile.width = 4;
			Projectile.height = 4;
			Projectile.penetrate = -1;
			Projectile.friendly = true;
			Projectile.tileCollide = false;
			Projectile.timeLeft = 600;
			Projectile.DamageType = DamageClass.Summon;

			Projectile.hide = true;
		}

		public override void AI()
		{
			Player player = Main.player[Projectile.owner];
			if (!player.channel || !player.active || player.dead)
			{
				Projectile.Kill();
				return;
			}
			if (Projectile.localAI[0] == 0)
			{
				Projectile.localAI[0] = 1;
				Projectile.localAI[1] = 810 - Projectile.ai[0];
				Projectile.timeLeft = (int)Projectile.localAI[1];

				Projectile.ai[0] = Projectile.Center.X;
				Projectile.ai[1] = Projectile.Center.Y;
			}

			if (Projectile.timeLeft == Projectile.localAI[1] - 30)
			{
				Projectile.velocity = Vector2.Zero;
			}
		}

		public override bool? CanCutTiles()
		{
			return false;
		}

		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
		{
			target.AddBuff(BuffID.Slow, 10);
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			Vector2 unit = Projectile.velocity;
			float point = 0f;
			// Run an AABB versus Line check to look for collisions, look up AABB collision first to see how it works
			// It will look for collisions on the given line using AABB
			return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center,
				new Vector2(Projectile.ai[0], Projectile.ai[1]), 4, ref point);
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Vector2 playerCenter = new Vector2(Projectile.ai[0], Projectile.ai[1]);
			Vector2 center = Projectile.Center;
			Vector2 distToProj = playerCenter - center;
			float projRotation = distToProj.ToRotation() - 1.57f;
			float distance = distToProj.Length();

			int frame = 0;

			while (distance > 4f && !float.IsNaN(distance))
			{
				distToProj.Normalize();                 //get unit vector
				distToProj *= 4f;
				center += distToProj;                   //update draw position
				distToProj = playerCenter - center;    //update distance
				distance = distToProj.Length();
				Color drawColor = Lighting.GetColor((int)(center.X / 16), (int)(center.Y / 16));

				frame = (frame + 4) % 16;

				//Draw chain
				Main.spriteBatch.Draw(TextureAssets.Projectile[Type].Value, new Vector2(center.X - Main.screenPosition.X, center.Y - Main.screenPosition.Y),
					new Rectangle(0, frame, 4, 4), drawColor, projRotation,
					new Vector2(4 * 0.5f, 4 * 0.5f), 1f, SpriteEffects.None, 0f);
			}
			return false;
		}

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
			behindProjectiles.Add(index);
		}
	}

	public class WeaverOrbMinionWebBall : ModProjectile
	{
		public override void SetStaticDefaults()
		{
			ProjectileID.Sets.MinionShot[Projectile.type] = true;
		}

		public override void SetDefaults()
		{
			Projectile.width = 22;
			Projectile.height = 22;
			Projectile.penetrate = 1;
			Projectile.friendly = true;
			Projectile.tileCollide = false;
			Projectile.timeLeft = 600;
			Projectile.DamageType = DamageClass.Summon;
		}

		public override void AI()
		{
			Projectile.rotation += 0.1f;
		}

		public override bool? CanCutTiles()
		{
			return false;
		}

		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
		{
			target.AddBuff(BuffID.Slow, 60);
		}
	}
}