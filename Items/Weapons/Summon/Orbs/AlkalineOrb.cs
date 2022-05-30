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
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;

namespace Polarities.Items.Weapons.Summon.Orbs
{
	public class AlkalineOrb : ModItem
	{
		public override void SetStaticDefaults()
		{
			this.SetResearch(1);
		}

		public override void SetDefaults()
		{
			Item.SetWeaponValues(41, 0, 0);
			Item.DamageType = DamageClass.Summon;

			Item.width = 26;
			Item.height = 44;

			Item.useTime = 30;
			Item.useAnimation = 30;
			Item.channel = true;
			Item.useStyle = ItemUseStyleID.HoldUp;
			Item.UseSound = SoundID.NPCHit54;
			Item.noMelee = true;

			Item.shoot = ProjectileType<AlkalineOrbMinion>();

			Item.value = Item.sellPrice(gold: 2, silver: 50);
			Item.rare = ItemRarityID.Pink;
		}

		private int time;

		public override void HoldItem(Player player)
		{
			if (player.channel)
			{
				player.direction = (Main.MouseWorld.X - player.Center.X > 0) ? 1 : -1;
				player.itemTime = player.itemTimeMax;
				player.itemAnimation = player.itemAnimationMax;

				time++;
				if (time % 20 == 0 && Item.UseSound != null)
				{
					SoundEngine.PlaySound((SoundStyle)Item.UseSound, player.position);
				}
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
				Main.projectile[Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI)].originalDamage = damage;
			}
			return false;
		}
	}

	public class AlkalineOrbMinion : ModProjectile
	{
		public override void SetStaticDefaults()
		{
			Main.projFrames[Type] = 3;
		}

		public override void SetDefaults()
		{
			Projectile.width = 48;
			Projectile.height = 48;
			Projectile.penetrate = -1;
			Projectile.minion = true;
			Projectile.DamageType = DamageClass.Summon;
			Projectile.friendly = false;
			Projectile.tileCollide = false;
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

			Projectile.ai[0]++;


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

			float angleOffset = index * MathHelper.TwoPi / ownedProjectiles;

			Vector2 goalPosition = player.Center + new Vector2(player.direction * 64 + 100 * (float)Math.Sin(angleOffset + Projectile.ai[0] / 20f), -300 + 80 * (float)Math.Sin(angleOffset + Projectile.ai[0] / 40f));
			Projectile.velocity = (goalPosition - Projectile.Center) / 16;

			if (Projectile.ai[0] % 20 == 0)
			{
				Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, new Vector2(0, -16).RotatedByRandom(MathHelper.PiOver4), ProjectileType<AlkalineOrbMinionProjectile>(), Projectile.damage, Projectile.knockBack, Projectile.owner);
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

		public override void Kill(int timeLeft)
		{
			Player player = Main.player[Projectile.owner];
			Vector2 startPos = player.Center + new Vector2(player.direction * 8, -24);
			Vector2 endPos = Projectile.Center;

			Vector2 dustPos = endPos;
			Vector2 offset;

			while ((startPos - dustPos).Length() > 4)
			{
				offset = new Vector2(startPos.X - dustPos.X, 3 * (startPos.Y - dustPos.Y));
				dustPos += offset.SafeNormalize(Vector2.Zero) * 2;

				Dust.NewDustPerfect(dustPos + new Vector2(Main.rand.NextFloat(0, 16), 0).RotatedByRandom(MathHelper.TwoPi), 74, Vector2.Zero, Scale: 1f);
			}

			for (int i = 0; i < 20; i++)
			{
				Main.dust[Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, 74, Scale: 1.5f)].noGravity = true;
			}
			SoundEngine.PlaySound(new SoundStyle("Terraria/Sounds/NPC_Death_52")
			{
				Volume = 0.5f,
			}, Projectile.position);
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Player player = Main.player[Projectile.owner];
			Texture2D texture = TextureAssets.Projectile[Type].Value;

			Vector2 startDrawPos = player.Center + new Vector2(player.direction * 8, -24) - Main.screenPosition;
			Vector2 endDrawPos = Projectile.Center - Main.screenPosition;

			Vector2 drawPos = endDrawPos;
			Vector2 offset;
			int phase = 0;
			while ((startDrawPos - drawPos).Length() > 4)
			{
				offset = new Vector2(startDrawPos.X - drawPos.X, 3 * (startDrawPos.Y - drawPos.Y));
				drawPos += offset.SafeNormalize(Vector2.Zero) * 2;

				phase = (phase + 2) % 48;

				Main.EntitySpriteDraw(texture, drawPos, new Rectangle(0, 48 + phase, 48, 4), Color.White, offset.ToRotation() - 1.57f, new Vector2(24, 2), 1f, SpriteEffects.None, 0);
			}

			offset = new Vector2(startDrawPos.X - endDrawPos.X, 3 * (startDrawPos.Y - endDrawPos.Y));
			Main.EntitySpriteDraw(texture, endDrawPos, new Rectangle(0, 0, 48, 48), Color.White, offset.ToRotation() - 1.57f, new Vector2(24, 24), 1f, SpriteEffects.None, 0);

			return false;
		}
	}

	public class AlkalineOrbMinionProjectile : ModProjectile
	{
		public override void SetStaticDefaults()
		{
			ProjectileID.Sets.MinionShot[Projectile.type] = true;

			ProjectileID.Sets.TrailCacheLength[Projectile.type] = 6;
			ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
		}

		public override void SetDefaults()
		{
			Projectile.width = 16;
			Projectile.height = 16;
			Projectile.penetrate = 1;
			Projectile.friendly = true;
			Projectile.tileCollide = false;
			Projectile.DamageType = DamageClass.Summon;
			Projectile.timeLeft = 3600;
		}

		public override void AI()
		{
			Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

			Vector2 targetVelocity = Main.MouseWorld - Projectile.Center;
			Projectile.velocity.X += Math.Max(-0.25f, Math.Min(0.25f, (targetVelocity.X - Projectile.velocity.X) / 60));
			Projectile.velocity.Y += Math.Min(0, (targetVelocity.SafeNormalize(Vector2.Zero)).Y * 0.1f);
			Projectile.velocity.Y += 0.35f;
			Projectile.velocity *= 0.975f;

			if (Projectile.Bottom.Y > Main.player[Projectile.owner].Bottom.Y)
            {
				Projectile.tileCollide = true;
            }
		}

		public override bool? CanCutTiles()
		{
			return false;
		}

		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
		{
			target.AddBuff(BuffType<Buffs.Corroding>(), 60 * 5);
		}

		public override void Kill(int timeLeft)
		{
			for (int i = 0; i < 10; i++)
			{
				Main.dust[Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, 74, newColor: new Color(0, 180, 255), Scale: 1f)].noGravity = true;
			}
			SoundEngine.PlaySound(new SoundStyle("Terraria/Sounds/NPC_Death_52")
			{
				Volume = 0.5f,
			}, Projectile.position);
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D texture = TextureAssets.Projectile[Type].Value;

			Color mainColor = Color.White;

			for (int k = 0; k < Projectile.oldPos.Length; k++)
			{
				Color color = mainColor * ((float)(Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length);
				float scale = Projectile.scale * ((float)(Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length);

				float rotation;
				if (k + 1 >= Projectile.oldPos.Length)
				{
					rotation = (Projectile.oldPos[k] - Projectile.position).ToRotation() + MathHelper.PiOver2;
				}
				else
				{
					rotation = (Projectile.oldPos[k] - Projectile.oldPos[k + 1]).ToRotation() + MathHelper.PiOver2;
				}

				Main.EntitySpriteDraw(texture, Projectile.Center - Projectile.position + Projectile.oldPos[k] - Main.screenPosition, new Rectangle(0, Projectile.frame * texture.Height / Main.projFrames[Projectile.type], texture.Width, texture.Height / Main.projFrames[Projectile.type]), color, rotation, new Vector2(texture.Width / 2, texture.Height / Main.projFrames[Projectile.type] / 2), scale, SpriteEffects.None, 0);
			}

			Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, new Rectangle(0, Projectile.frame * texture.Height / Main.projFrames[Projectile.type], texture.Width, texture.Height / Main.projFrames[Projectile.type]), mainColor, Projectile.rotation, new Vector2(texture.Width / 2, texture.Height / Main.projFrames[Projectile.type] / 2), Projectile.scale, SpriteEffects.None, 0);
			return false;
		}
	}
}