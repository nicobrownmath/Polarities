using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using static Terraria.ModLoader.ModContent;
using Microsoft.Xna.Framework;
using System;
using System.IO;
using Polarities.Buffs;
using Microsoft.Xna.Framework.Graphics;
using Terraria.DataStructures;
using Terraria.Audio;
using Polarities.NPCs.Gigabat;

namespace Polarities.Items.Weapons.Summon.Minions
{
	public class Batastrophe : ModItem
	{
		public override void SetStaticDefaults()
		{
			this.SetResearch(1);

			Item.staff[Type] = true;
		}

		public override void SetDefaults()
		{
			Item.SetWeaponValues(3, 1f, 0);
			Item.DamageType = DamageClass.Summon;
			Item.mana = 0;

			Item.width = 48;
			Item.height = 48;

			Item.useTime = 17;
			Item.useAnimation = 17;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.noMelee = true;
			Item.UseSound = SoundID.Item44;
			Item.autoReuse = true;

			Item.shoot = ProjectileType<BatastropheMinion>();
			Item.shootSpeed = 10f;

			Item.value = Item.sellPrice(gold: 3);
			Item.rare = RarityType<GigabatFlawlessRarity>();
			Item.GetGlobalItem<PolaritiesItem>().flawless = true;
		}

		public override bool CanUseItem(Player player)
		{
			return player.slotsMinions <= player.maxMinions - 1 / 12f;
		}

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
		{
			for (int i = 0; i < Math.Min(1 + (player.maxMinions - player.slotsMinions) * 12, 12); i++)
			{
				Vector2 speed = velocity.RotatedByRandom(MathHelper.Pi);
				int proj = Projectile.NewProjectile(source, position, speed, type, damage, knockback, player.whoAmI, Main.rand.NextFloat(64, 1024), Main.rand.NextFloat(3, 6));
				Main.projectile[proj].originalDamage = damage;
			}
			return false;
		}
	}

	public class BatastropheMinion : ModProjectile
	{
		public override string Texture => "Polarities/NPCs/Gigabat/GigabatMinion";

		private int rotDir;
		private float maxTargetDistance
		{
			get => Projectile.ai[0];
			set => value = Projectile.ai[0];
		}

		public override void SetStaticDefaults()
		{
			Main.projFrames[Projectile.type] = 4;

			ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true;
			ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true;
			ProjectileID.Sets.MinionSacrificable[Projectile.type] = true;
		}

		public override void SetDefaults()
		{
			Projectile.width = 16;
			Projectile.height = 12;
			DrawOffsetX = -6;
			DrawOriginOffsetY = -6;
			DrawOriginOffsetX = 0;
			Projectile.penetrate = 1;

			Projectile.minion = true;
			Projectile.minionSlots = 1 / 12f;
			Projectile.DamageType = DamageClass.Summon;

			Projectile.friendly = true;
			Projectile.tileCollide = true;
			Projectile.timeLeft = 3600;
		}

		public override void AI()
		{
			Player player = Main.player[Projectile.owner];

			if (Projectile.timeLeft == 3600)
			{
				rotDir = -player.direction;
			}

			if ((player.Center - Projectile.Center).Length() > 2000)
			{
				Projectile.Kill();
			}

			int targetID = -1;
			Projectile.Minion_FindTargetInRange((int)maxTargetDistance, ref targetID, true);
			NPC target = null;
			if (targetID != -1)
			{
				target = Main.npc[targetID];
			}

			if (Main.rand.NextBool(60))
			{
				Projectile.ai[0] = Main.rand.NextFloat(96, 1024);
				Projectile.ai[1] = Main.rand.NextFloat(3, 6);

			}
			Projectile.netUpdate = true;

			if (target != null)
			{
				if ((target.Center - player.Center).Length() > maxTargetDistance)
				{
					target = null;
				}
			}

			rotDir = player.velocity.X < 0 ? 1 : (player.velocity.X > 0 ? -1 : rotDir);
			Vector2 dir;

			if (target == null)
			{
				dir = (player.Center - Projectile.Center).RotatedBy(rotDir * MathHelper.Pi / Projectile.ai[1]);
			}
			else
			{
				dir = target.Center - Projectile.Center;
			}
			if (dir.Length() > 16)
			{
				dir.Normalize();
				dir.RotateRandom(Math.PI / 4);
				Projectile.velocity += dir;
				if (Projectile.velocity.Length() > 10)
				{
					Projectile.velocity.Normalize();
					Projectile.velocity *= 10;
				}
			}
			Projectile.netUpdate = true;

			Projectile.rotation = (float)(0.5 * Math.Atan(Projectile.velocity.X));
			Projectile.spriteDirection = Projectile.velocity.X > 0 ? -1 : 1;

			Projectile.frameCounter++;
			if (Projectile.frameCounter == 5)
			{
				Projectile.frameCounter = 0;
				Projectile.frame = (Projectile.frame + 1) % 4;
			}
		}

		public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
		{
			modifiers.FlatBonusDamage += Math.Min(Math.Max(0, target.defense - (int)Main.player[Projectile.owner].GetArmorPenetration(DamageClass.Generic)), 15) / 2;
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			SoundEngine.PlaySound(new SoundStyle("Terraria/Sounds/NPC_Killed_4")
			{
				Volume = 0.5f,
			}, Projectile.Center);
		}

		public override bool? CanCutTiles()
		{
			return false;
		}

		public override bool MinionContactDamage()
		{
			return true;
		}

        public override bool OnTileCollide(Vector2 oldVelocity)
		{
			if (Projectile.velocity.X != oldVelocity.X)
			{
				Projectile.velocity.X = -oldVelocity.X;
			}
			if (Projectile.velocity.Y != oldVelocity.Y)
			{
				Projectile.velocity.Y = -oldVelocity.Y;
			}
			return false;
		}

        public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac)
		{
			fallThrough = true;
			return true;
		}

        public override void PostDraw(Color lightColor)
		{
			Texture2D mask = Polarities.customNPCGlowMasks[NPCType<GigabatMinion>()].Value;
			Vector2 drawOrigin = new Vector2(mask.Width * 0.5f, mask.Height / Main.projFrames[Type] * 0.5f);
			Vector2 drawPos = Projectile.Center - Main.screenPosition;
			Main.EntitySpriteDraw(mask, drawPos, mask.Frame(1, Main.projFrames[Type], 0, Projectile.frame), Projectile.GetAlpha(Color.White), Projectile.rotation, drawOrigin, Projectile.scale, Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0);
		}
    }
}
