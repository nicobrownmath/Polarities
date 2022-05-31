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
	public class SaltKillifishStaff : ModItem
	{
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Salt Killifish Staff");
			Tooltip.SetDefault("Summons a salt killifish to protect you");

			this.SetResearch(1);
		}

		public override void SetDefaults()
		{
			Item.SetWeaponValues(21, 1, 0);
			Item.DamageType = DamageClass.Summon;
			Item.mana = 10;

			Item.width = 38;
			Item.height = 40;

			Item.useTime = 30;
			Item.useAnimation = 30;
			Item.useStyle = 1;
			Item.noMelee = true;
			Item.UseSound = SoundID.Item44;
			Item.autoReuse = true;

			Item.buffType = BuffType<SaltKillifishMinionBuff>();
			Item.shoot = ProjectileType<SaltKillifishMinion>();

			Item.value = Item.sellPrice(gold: 4);
			Item.rare = ItemRarityID.Pink;
		}

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
		{
			player.AddBuff(Item.buffType, 18000, true);
			player.SpawnMinionOnCursor(source, player.whoAmI, type, damage, knockback);
			return false;
		}
	}

	public class SaltKillifishMinionBuff : ModBuff
	{
		public override void SetStaticDefaults()
		{
			Main.buffNoTimeDisplay[Type] = true;
			Main.buffNoSave[Type] = true;
		}

		public override void Update(Player player, ref int buffIndex)
		{
			if (player.ownedProjectileCounts[ProjectileType<SaltKillifishMinion>()] > 0)
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

	public class SaltKillifishMinion : ModProjectile
    {
		public override void SetStaticDefaults()
		{
			Main.projPet[Projectile.type] = true;
			ProjectileID.Sets.MinionSacrificable[Projectile.type] = true;
			ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true;
			ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true;

			Main.projFrames[Type] = 8;
		}

		public override void SetDefaults()
		{
			Projectile.width = 32;
			Projectile.height = 32;
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
				player.ClearBuff(BuffType<SaltKillifishMinionBuff>());
			}
			if (player.HasBuff(BuffType<SaltKillifishMinionBuff>()))
			{
				Projectile.timeLeft = 2;
			}

			int targetID = -1;
			Projectile.Minion_FindTargetInRange(750, ref targetID, true);
			Projectile.ai[0] = targetID;

			int index = 0;
			int numProjectiles = 0;
			for (int i = 0; i < Main.maxProjectiles; i++)
			{
				if (Main.projectile[i].active && Main.projectile[i].type == Type && Main.projectile[i].owner == Projectile.owner && Main.projectile[i].ai[0] == targetID)
				{
					numProjectiles++;
					if (i < Projectile.whoAmI) index++;
				}
			}

			if (targetID == -1)
            {
				//follow player
				float upAngle = 0.25f + 0.25f * (float)Math.Sin(3 * Math.Sin(index * MathHelper.TwoPi / numProjectiles + PolaritiesSystem.timer * 0.01f));
				float phase = index * MathHelper.TwoPi / numProjectiles + PolaritiesSystem.timer * 0.05f + (float)Math.Sin(2 * Math.Sin(index * MathHelper.TwoPi / numProjectiles + PolaritiesSystem.timer * 0.03f));
				Vector2 goalPosition = player.Center + new Vector2(-60 * (float)Math.Sqrt(numProjectiles), 0).RotatedBy(upAngle) * new Vector2((float)Math.Sin(phase), 1);
				Vector2 goalVelocity = (goalPosition - Projectile.Center) / 15f;
				Projectile.velocity += (goalVelocity - Projectile.velocity) / 15f;
			}
			else
            {
				//attack enemy
				NPC target = Main.npc[targetID];

				float upAngle = 0.5f * (float)Math.Sin(3 * Math.Sin(index * MathHelper.TwoPi / numProjectiles + PolaritiesSystem.timer * 0.01f));
				float phase = index * MathHelper.TwoPi / numProjectiles + PolaritiesSystem.timer * 0.05f + (float)Math.Sin(2 * Math.Sin(index * MathHelper.TwoPi / numProjectiles + PolaritiesSystem.timer * 0.03f));
				Vector2 goalPosition = target.Center + new Vector2(-60 * (float)Math.Sqrt(numProjectiles), 0).RotatedBy(upAngle) * new Vector2((float)Math.Sin(phase), 1);
				Vector2 goalVelocity = (goalPosition - Projectile.Center) / 15f;
				Projectile.velocity += (goalVelocity - Projectile.velocity) / 15f;
			}

			if (Projectile.Distance(player.Center) > 1600)
            {
				Projectile.Center = player.Center;
            }

			Projectile.spriteDirection = Projectile.velocity.X > 0 ? -1 : 1;
        }

        public override bool MinionContactDamage()
        {
			return Projectile.ai[0] != -1;
		}

		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
		{
			target.AddBuff(BuffType<Desiccating>(), 30);
		}
	}
}

