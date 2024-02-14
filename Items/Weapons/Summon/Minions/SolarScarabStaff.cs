using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using static Terraria.ModLoader.ModContent;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Polarities.Projectiles;
using System;
using Polarities.Buffs;
using Terraria.DataStructures;

namespace Polarities.Items.Weapons.Summon.Minions
{
	public class SolarScarabStaff : ModItem
	{
		public override void SetStaticDefaults()
		{
			this.SetResearch(1);
		}

		public override void SetDefaults()
		{
			Item.SetWeaponValues(70, 5, 0);
			Item.DamageType = DamageClass.Summon;
			Item.mana = 10;

			Item.width = 52;
			Item.height = 50;
			Item.useTime = 30;
			Item.useAnimation = 30;
			Item.noMelee = true;
			Item.useStyle = 1;

			Item.UseSound = SoundID.Item44;
			Item.autoReuse = true;
			Item.shoot = ProjectileType<SolarScarabMinion>();
			Item.buffType = BuffType<SolarScarabMinionBuff>();
			Item.shootSpeed = 12f;

			Item.value = Item.sellPrice(gold: 5);
			Item.rare = 6;
		}

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
		{
			player.AddBuff(Item.buffType, 18000, true);
			Main.projectile[Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI)].originalDamage = damage;
			return false;
		}
	}

	public class SolarScarabMinionBuff : ModBuff
	{
		public override void SetStaticDefaults()
		{
			Main.buffNoTimeDisplay[Type] = true;
			Main.buffNoSave[Type] = true;
		}

		public override void Update(Player player, ref int buffIndex)
		{
			if (player.ownedProjectileCounts[ProjectileType<SolarScarabMinion>()] > 0)
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

	public class SolarScarabMinion : ModProjectile
	{
		private Vector2 playerDisplacement;

		public override void SetStaticDefaults()
		{
			Main.projFrames[Projectile.type] = 12;
			Main.projPet[Projectile.type] = true;
			ProjectileID.Sets.MinionSacrificable[Projectile.type] = true;
			ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true;
			ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true;
		}

		public override void SetDefaults()
		{
			Projectile.width = 26;
			Projectile.height = 26;
			DrawOffsetX = -8;
			DrawOriginOffsetX = 0;
			DrawOriginOffsetY = 0;
			Projectile.penetrate = -1;
			Projectile.DamageType = DamageClass.Summon;
			Projectile.minion = true;
			Projectile.minionSlots = 1f;
			Projectile.friendly = true;
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
				player.ClearBuff(BuffType<SolarScarabMinionBuff>());
			}
			if (player.HasBuff(BuffType<SolarScarabMinionBuff>()))
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

			if (Main.rand.NextBool(60))
			{
				playerDisplacement = new Vector2(Main.rand.Next(-100, 101), Main.rand.Next(-100, 0));
				Projectile.netUpdate = true;
			}

			Vector2 targetPos = player.Center + playerDisplacement;
			if (target != null)
			{
				targetPos = target.Center + new Vector2(Main.rand.Next(-target.width, target.width), Main.rand.Next(-target.height, target.height));
			}

			Projectile.frameCounter++;

			if ((Projectile.Center - player.Center).Length() > 2000 && Projectile.ai[0] == 1)
			{
				Projectile.position = player.Center - new Vector2(Projectile.width / 2, Projectile.height / 2);
			}

			if (Projectile.ai[0] == 1)
			{
				Lighting.AddLight(Projectile.Center, Color.Pink.ToVector3());
				Vector2 targetAcceleration = 16 * (targetPos - Projectile.Center) / (targetPos - Projectile.Center).Length() - Projectile.velocity;
				if (targetAcceleration.Length() > 0.01f) { targetAcceleration = Math.Min(targetAcceleration.Length(), 0.5f) * targetAcceleration / targetAcceleration.Length(); } else { targetAcceleration = Vector2.Zero; }
				Projectile.velocity += targetAcceleration;
				Projectile.rotation = Projectile.velocity.ToRotation() + (float)Math.PI / 2;
				Projectile.frame = 4 + Projectile.frameCounter % 8;
			}
			else
			{
				Lighting.AddLight(Projectile.Center, Color.Yellow.ToVector3());
				Projectile.rotation += 0.5f;
				Projectile.velocity.Y += 0.4f;
				Projectile.frame = Projectile.frameCounter % 4;
			}
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			Projectile.ai[0] = 0f;
			target.AddBuff(BuffID.OnFire, 120);
		}

		public override bool? CanCutTiles()
		{
			return false;
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
			Projectile.ai[0] = 1f;
			return false;
		}

		public override bool MinionContactDamage()
		{
			return true;
		}

        public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac)
		{
			fallThrough = Projectile.ai[0] == 1f;
			return true;
		}
	}
}