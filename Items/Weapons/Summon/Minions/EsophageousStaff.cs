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
using Microsoft.Xna.Framework.Graphics;
using Terraria.Audio;
using System.Collections.Generic;
using ReLogic.Content;
using Polarities.Projectiles;

namespace Polarities.Items.Weapons.Summon.Minions
{
	public class EsophageousStaff : ModItem
	{
		public override void SetStaticDefaults()
		{
			SacrificeTotal = (1);
		}

		public override void SetDefaults()
		{
			Item.damage = 24;
			Item.DamageType = DamageClass.Summon;
			Item.width = 32;
			Item.height = 42;
			Item.mana = 10;
			Item.useTime = 30;
			Item.useAnimation = 30;
			Item.useStyle = 1;
			Item.noMelee = true;
			Item.knockBack = 1f;
			Item.value = 10000 * 5;
			Item.rare = ItemRarityID.Lime;
			Item.UseSound = SoundID.Item44;
			Item.autoReuse = true;
			Item.buffType = BuffType<EsophminiBuff>();
			Item.shoot = ProjectileType<EsophminiClaw>();
		}

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
		{
			int baseProjectile = -1;
			for (int i = 0; i < Main.maxProjectiles; i++)
            {
				if (Main.projectile[i].active && Main.projectile[i].owner == player.whoAmI && Main.projectile[i].type == ProjectileType<EsophminiBody>())
                {
					baseProjectile = i;
					break;
                }
            }

			if (baseProjectile == -1)
			{
				baseProjectile = Projectile.NewProjectile(source, Main.MouseWorld, velocity, ProjectileType<EsophminiBody>(), 0, 0, player.whoAmI);
			}

			player.AddBuff(Item.buffType, 18000, true);
			Main.projectile[Projectile.NewProjectile(source, Main.projectile[baseProjectile].Center, velocity, type, damage, knockback, player.whoAmI, ai0: baseProjectile)].originalDamage = damage;
			return false;
		}
	}

	public class EsophminiBuff : ModBuff
	{
		public override void SetStaticDefaults()
		{
			Main.buffNoTimeDisplay[Type] = true;
			Main.buffNoSave[Type] = true;
		}

		public override void Update(Player player, ref int buffIndex)
		{
			if (player.ownedProjectileCounts[ProjectileType<EsophminiClaw>()] > 0)
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

	public class EsophminiBody : ModProjectile
	{
		public override void SetStaticDefaults()
		{
			Main.projFrames[Projectile.type] = 1;
			ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true;
			ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true;
		}

		public override void SetDefaults()
		{
			Projectile.width = 26;
			Projectile.height = 26;
			DrawOffsetX = 0;
			DrawOriginOffsetY = -28;
			DrawOriginOffsetX = 0;

			Projectile.penetrate = -1;
			Projectile.friendly = true;
			Projectile.tileCollide = false;
		}

		public override void AI()
		{
			Projectile.GetGlobalProjectile<PolaritiesProjectile>().customShader = Main.player[Projectile.owner].cMinion;

			Player player = Main.player[Projectile.owner];
			if (!player.active)
			{
				Projectile.active = false;
				return;
			}
			if (player.dead)
			{
				player.ClearBuff(BuffType<EsophminiBuff>());
			}
			if (player.HasBuff(BuffType<EsophminiBuff>()))
			{
				Projectile.timeLeft = 2;
			}

			int targetID = -1;
			Projectile.Minion_FindTargetInRange(750, ref targetID, true);
			NPC target = null;
			if (targetID != -1)
			{
				target = Main.npc[targetID];
			}

			Vector2 targetPosition = player.Center - new Vector2(player.direction * 64, 64);
			Vector2 targetVelocity = player.velocity;

			if (target != null)
			{
				targetPosition = target.Center - new Vector2(0, 64);
				targetVelocity = target.velocity;

				if (player.ownedProjectileCounts[ProjectileType<EsophminiClaw>()] >= 2 && (targetPosition - Projectile.Center).Length() < 128)
				{
					Projectile.ai[0]++;
					if (Main.netMode != 1 && Projectile.ai[0] % 20 == 0)
					{
						Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center + new Vector2(Main.rand.NextFloat(-8f, 8f), 8), new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), 4), ProjectileType<IchorDrop>(), 1, 0, Projectile.owner);
					}
					if (Projectile.ai[0] % 20 == 0)
					{
						SoundEngine.PlaySound(SoundID.Item17, Projectile.Center);
					}
				}
				else
				{
					Projectile.ai[0] = 0;
				}
			}
			else
			{
				Projectile.ai[0] = 0;
			}

			Vector2 goalVelocity = targetVelocity + (targetPosition - Projectile.Center) / 30;
			Projectile.velocity += (goalVelocity - Projectile.velocity) / 30;

			Projectile.rotation = Projectile.velocity.X * 0.08f;
			//Projectile.frame = Projectile.velocity.X > 0 ? 0 : 1;

			if ((Projectile.Center - player.Center).Length() > 1000)
			{
				Projectile.position = player.Center - new Vector2(Projectile.width / 2, Projectile.height / 2);
				Projectile.velocity = Vector2.Zero;
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
	}

	public class EsophminiClaw : ModProjectile
	{
		private int attackCooldown;
		public override void SetStaticDefaults()
		{
			Main.projFrames[Projectile.type] = 1;
			Main.projPet[Projectile.type] = true;
			ProjectileID.Sets.MinionSacrificable[Projectile.type] = true;
			ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true;
		}

		public override void SetDefaults()
		{
			Projectile.width = 10;
			Projectile.height = 10;
			DrawOffsetX = -4;
			DrawOriginOffsetY = 0;
			DrawOriginOffsetX = 0;

			Projectile.penetrate = -1;
			Projectile.minion = true;
			Projectile.minionSlots = 1f;
			Projectile.DamageType = DamageClass.Summon;
			Projectile.friendly = true;
			Projectile.tileCollide = false;
			Projectile.hide = true;
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
				player.ClearBuff(BuffType<EsophminiBuff>());
			}
			if (player.HasBuff(BuffType<EsophminiBuff>()))
			{
				Projectile.timeLeft = 2;
			}

			Projectile body = Main.projectile[(int)Projectile.ai[0]];

			if (!body.active || body.type != ProjectileType<EsophminiBody>())
            {
				Projectile.Kill();
				return;
            }

			Projectile.velocity.Y += 0.3f;

			if ((Projectile.Center - body.Center).Length() > 80)
			{
				Projectile.velocity = body.velocity + (body.Center - Projectile.Center).SafeNormalize(Vector2.Zero) * 4 + new Vector2(2, 0).RotatedByRandom(Math.PI);
			}

			if (attackCooldown > 0)
			{
				attackCooldown--;
			}

			if ((body.Center - player.Center).Length() > 1000 || (body.Center - Projectile.Center).Length() > 500)
			{
				Projectile.position = player.Center - new Vector2(Projectile.width / 2, Projectile.height / 2);
				Projectile.velocity = Vector2.Zero;
			}
		}

		public override bool? CanCutTiles()
		{
			return false;
		}

		public override bool MinionContactDamage()
		{
			return attackCooldown == 0;
		}

		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
		{
			target.immune[Projectile.owner] = 0;
			attackCooldown = 10;
		}

		public static Asset<Texture2D> ChainTexture;

        public override void Load()
        {
			ChainTexture = Request<Texture2D>(Texture + "_Chain");
        }

        public override void Unload()
        {
			ChainTexture = null;
		}

        public override bool PreDraw(ref Color drawColor)
		{
			Projectile body = Main.projectile[(int)Projectile.ai[0]];

			Vector2 constructCenter = body.Center;
			Vector2 center = Projectile.Center + new Vector2(0, -36);
			Vector2 distToNPC = constructCenter - center;
			float projRotation = distToNPC.ToRotation() + MathHelper.PiOver2;
			float distance = distToNPC.Length();

			int tries = 100;
			while (distance > 6f && !float.IsNaN(distance) && tries > 0)
			{
				tries--;
				//Draw chain
				Main.EntitySpriteDraw(ChainTexture.Value, new Vector2(center.X - Main.screenPosition.X, center.Y - Main.screenPosition.Y),
					new Rectangle(0, 0, 10, 6), Lighting.GetColor((int)(center.X / 16), (int)(center.Y / 16)), projRotation,
					new Vector2(10 * 0.5f, 6 * 0.5f), 1f, SpriteEffects.None, 0);

				distToNPC.Normalize();                 //get unit vector
				distToNPC *= 6f;                      //speed = 24
				center += distToNPC;                   //update draw position
				distToNPC = constructCenter - center;    //update distance
				distance = distToNPC.Length();
			}
			for (int i = 1; i <= 6; i++)
			{
				center = Projectile.Center + new Vector2(0, -i * 6);
				Main.EntitySpriteDraw(ChainTexture.Value, new Vector2(center.X - Main.screenPosition.X, center.Y - Main.screenPosition.Y),
					new Rectangle(0, 0, 10, 6), Lighting.GetColor((int)(center.X / 16), (int)(center.Y / 16)), 0,
					new Vector2(10 * 0.5f, 6 * 0.5f), 1f, SpriteEffects.None, 0);
			}
			return true;
		}

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
			behindProjectiles.Add(index);
        }
	}

	public class IchorDrop : ModProjectile
	{
		public override string Texture => "Polarities/Textures/Pixel";

		public override void SetStaticDefaults()
		{
			ProjectileID.Sets.MinionShot[Type] = true;
		}

		public override void SetDefaults()
		{
			Projectile.aiStyle = -1;
			Projectile.DamageType = DamageClass.Summon;
			Projectile.friendly = true;
			Projectile.width = 16;
			Projectile.height = 16;
			Projectile.alpha = 0;
			Projectile.timeLeft = 20;
			Projectile.penetrate = -1;
			Projectile.hide = true;

			Projectile.GetGlobalProjectile<PolaritiesProjectile>().doNotStrikeNPC = true;
		}

		public override void AI()
		{
			int dust = Dust.NewDust(Projectile.Center - new Vector2(3, 3), 0, 0, 64, Scale: 1.4f);
			Main.dust[dust].velocity = Main.rand.NextFloat(0.5f, 1f) * Projectile.velocity + new Vector2(Main.rand.NextFloat(0f, 1f), 0).RotatedByRandom(MathHelper.Pi);
			Main.dust[dust].noGravity = true;
			dust = Dust.NewDust(Projectile.Center - new Vector2(3, 3), 0, 0, 64, Scale: 1.4f);
			Main.dust[dust].velocity = Main.rand.NextFloat(0.5f, 1f) * Projectile.velocity + new Vector2(Main.rand.NextFloat(0f, 1f), 0).RotatedByRandom(MathHelper.Pi);
			Main.dust[dust].noGravity = true;
		}

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
		{
			target.AddBuff(BuffID.Ichor, 60, true);
		}
	}
}