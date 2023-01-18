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
using Polarities.NPCs;
using Terraria.GameContent;

namespace Polarities.Items.Weapons.Summon.Orbs
{
	public class SpiritBottle : ModItem
	{
		public override void SetStaticDefaults()
		{
			SacrificeTotal = (1);
		}

		public override void SetDefaults()
		{
			Item.SetWeaponValues(45, 0, 0);
			Item.DamageType = DamageClass.Summon;

			Item.width = 22;
			Item.height = 26;

			Item.useTime = 30;
			Item.useAnimation = 30;
			Item.channel = true;
			Item.useStyle = ItemUseStyleID.HoldUp;
			Item.UseSound = SoundID.NPCHit54;
			Item.noMelee = true;

			Item.value = Item.sellPrice(gold: 6);
			Item.rare = ItemRarityID.Yellow;

			Item.shoot = ProjectileType<SpiritBottleMinion>();
		}

		public override void HoldItem(Player player)
		{
			if (player.channel)
			{
				player.direction = (Main.MouseWorld.X - player.Center.X > 0) ? 1 : -1;
				if (!player.ItemTimeIsZero) player.itemTime = player.itemTimeMax;
				player.itemAnimation = player.itemAnimationMax;
			}

			player.itemLocation += new Vector2(0, 8 - Item.height / 2);
		}

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
		{
			for (int i = 0; i < player.GetModPlayer<PolaritiesPlayer>().orbMinionSlots * 2; i++)
			{
				Main.projectile[Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI)].originalDamage = damage;
			}
			return false;
		}

		public override void AddRecipes()
		{
			CreateRecipe()
				.AddIngredient(ItemID.Bottle)
				.AddIngredient(ItemID.Ectoplasm, 16)
				.AddTile(TileID.MythrilAnvil)
				.Register();
		}
	}

	public class SpiritBottleMinion : ModProjectile
	{
		public override void SetStaticDefaults()
		{
			Main.projFrames[Projectile.type] = 4;
		}

		public override void SetDefaults()
		{
			Projectile.width = 30;
			Projectile.height = 36;
			Projectile.penetrate = -1;
			Projectile.minion = true;
			Projectile.DamageType = DamageClass.Summon;
			Projectile.friendly = true;
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

			Vector2 playerCenter = player.Center + new Vector2(player.direction * 8, -34);

			for (int dist = Main.rand.Next(1, 32); dist < (Projectile.Center - playerCenter).Length() - 16; dist += Main.rand.Next(1, 32))
			{
				Dust.NewDustPerfect(playerCenter + (Projectile.Center - playerCenter).SafeNormalize(Vector2.Zero).RotatedByRandom(8 / (Projectile.Center - playerCenter).Length()) * dist, 88, (Projectile.Center - playerCenter).SafeNormalize(Vector2.Zero) * 4, 0, Color.Transparent, 0.5f).noGravity = true;
			}


			int targetID = -1;
			Projectile.Minion_FindTargetInRange(750, ref targetID, false);
			NPC target = null;
			if (targetID != -1)
			{
				target = Main.npc[targetID];
			}

			int index = 0;
			for (int i = 0; i < Projectile.whoAmI; i++)
			{
				if (Main.projectile[i].active && Main.projectile[i].type == Projectile.type && Main.projectile[i].owner == Projectile.owner)
					index++;
			}

			if (target == null)
			{
				Projectile.direction = player.Center.X > Projectile.Center.X ? 1 : -1;

				Projectile.ai[0] = 0;
				Projectile.ai[1] = 0;
				Vector2 goalPosition = player.Center + new Vector2(-player.direction * (index + 1) * 64, -64 + (float)Math.Sin(PolaritiesSystem.timer * 0.1f + index) * 16);
				Vector2 goalVelocity = (goalPosition - Projectile.Center) / 10;
				if (goalVelocity.Length() > 14)
				{
					goalVelocity.Normalize();
					goalVelocity *= 14;
				}
				Projectile.velocity += (goalVelocity - Projectile.velocity) / 6;
			}
			else
			{
				if (Projectile.ai[0] < 60)
				{
					Projectile.ai[1] = 0;
					Projectile.direction = target.Center.X > Projectile.Center.X ? 1 : -1;

					Vector2 goalPosition = target.Center + new Vector2(-Projectile.direction * 120, (float)Math.Sin(PolaritiesSystem.timer * 0.1f + index) * 16);
					Vector2 goalVelocity = (goalPosition - Projectile.Center) / 10;
					if (goalVelocity.Length() > 14)
					{
						goalVelocity.Normalize();
						goalVelocity *= 14;
					}
					Projectile.velocity += (goalVelocity - Projectile.velocity) / 6;

					if (Projectile.ai[0] == 59 && Projectile.velocity.Length() > 8)
					{
						Projectile.ai[0]--;
					}
				}
				else
				{
					if (Projectile.ai[0] == 60)
					{
						Projectile.direction = target.Center.X > Projectile.Center.X ? 1 : -1;
						Projectile.ai[1] = 1;
						Projectile.velocity = new Vector2(Projectile.direction * 16, 0);

						SoundEngine.PlaySound(SoundID.NPCHit54, Projectile.Center);
					}
					else if (Projectile.ai[0] == 70)
						Projectile.direction *= -1;
				}
				Projectile.ai[0]++;
				if (Projectile.ai[0] == 80 && Main.myPlayer == Projectile.owner)
				{
					Projectile.ai[0] = Main.rand.Next(0, 15);
					Projectile.ai[1] = 0;
				}
				Projectile.netUpdate = true;
			}

			Projectile.spriteDirection = Projectile.direction;

			Projectile.frameCounter++;
			if (Projectile.frameCounter == 8)
			{
				Projectile.frameCounter = 0;
				Projectile.frame = (Projectile.frame + 1) % 4;
			}
		}

		public override bool? CanCutTiles()
		{
			return false;
		}

		public override bool MinionContactDamage()
		{
			return true;
		}

		public override bool? CanHitNPC(NPC target)
		{
			if (Projectile.ai[1] == 1)
			{
				return null;
			}
			return false;
		}

		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
		{
			Projectile.ai[1] = 0;
			target.immune[Projectile.owner] = 0;
			target.AddBuff(BuffType<SpiritBottleDebuff>(), 60 * 10);

			if (target.GetGlobalNPC<PolaritiesNPC>().spiritBiteLevel < 20)
			{
				target.GetGlobalNPC<PolaritiesNPC>().spiritBiteLevel++;
			}
		}

		public override bool PreDraw(ref Color lightColor)
		{
			float trailLength = 4f;
			float trailSize = Projectile.ai[0] < 60 ? 4 : 8;
			for (int i = (int)trailLength - 1; i >= 0; i--)
			{
                Main.EntitySpriteDraw(TextureAssets.Projectile[Type].Value, Projectile.Center - Projectile.velocity * (i / trailLength) * trailSize - Main.screenPosition, new Rectangle(0, (TextureAssets.Projectile[Type].Height() * Projectile.frame) / Main.projFrames[Projectile.type], TextureAssets.Projectile[Type].Width(), TextureAssets.Projectile[Type].Height() / Main.projFrames[Projectile.type]), Color.White * (1 - i / trailLength), Projectile.rotation, new Vector2(TextureAssets.Projectile[Type].Width() / 2 + DrawOriginOffsetX, Projectile.height / 2), Projectile.scale, Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0);
			}
			return false;
		}
	}

	public class SpiritBottleDebuff : ModBuff
	{
        public override string Texture => "Terraria/Images/Buff_" + BuffID.Venom;

        public override void SetStaticDefaults()
		{
			Main.debuff[Type] = true;
		}

		public override void Update(NPC npc, ref int buffIndex)
		{
			npc.GetGlobalNPC<PolaritiesNPC>().spiritBite = true;
			for (int i = 0; i < npc.GetGlobalNPC<PolaritiesNPC>().spiritBiteLevel; i++)
			{
				if (Main.rand.NextBool(5))
				{
					Main.dust[Dust.NewDust(npc.position, npc.width, npc.height, 88, newColor: Color.Transparent, Scale: 1f)].noGravity = true;
				}
			}
		}
	}
}