using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria.Audio;
using Polarities.NPCs;
using Terraria.DataStructures;
using MultiHitboxNPCLibrary;

namespace Polarities.Items.Weapons.Ranged.Ammo
{
	public class ChlorophyteDart : ModItem
	{
		public override void SetStaticDefaults()
		{
			Tooltip.SetDefault("Sticks to enemies");

			SacrificeTotal = (99);
		}

		public override void SetDefaults()
		{
			Item.SetWeaponValues(12, 0, 0);
			Item.DamageType = DamageClass.Ranged;

			Item.width = 14;
			Item.height = 26;

			Item.maxStack = 9999;
			Item.consumable = true;

			Item.value = 20;
			Item.rare = ItemRarityID.Lime;

			Item.shoot = ProjectileType<ChlorophyteDartProjectile>();
			Item.shootSpeed = 3.5f;
			Item.ammo = AmmoID.Dart;
		}

		public override void AddRecipes()
		{
			CreateRecipe(100)
				.AddIngredient(ItemID.ChlorophyteBar)
				.AddTile(TileID.MythrilAnvil)
				.Register();
		}
	}

	public class ChlorophyteDartProjectile : ModProjectile
	{
		public override string Texture => "Polarities/Items/Weapons/Ranged/Ammo/ChlorophyteDart";

        public override void SetStaticDefaults()
        {
			DisplayName.SetDefault("{$Mods.Polarities.ItemName.ChlorophyteDart}");
        }

        public override void SetDefaults()
		{
			Projectile.aiStyle = -1;
			Projectile.friendly = true;
			Projectile.DamageType = DamageClass.Ranged;
			Projectile.width = 14;
			Projectile.height = 14;
			Projectile.alpha = 0;
			Projectile.timeLeft = 3600;
			Projectile.penetrate = 3;
			Projectile.tileCollide = true;
			Projectile.ignoreWater = false;

            Projectile.GetGlobalProjectile<MultiHitboxNPCLibraryProjectile>().badCollision = true;
            Projectile.GetGlobalProjectile<MultiHitboxNPCLibraryProjectile>().javelinSticking = true;
        }

		public override void OnSpawn(IEntitySource source)
		{
			while (Projectile.velocity.X >= 16f || Projectile.velocity.X <= -16f || Projectile.velocity.Y >= 16f || Projectile.velocity.Y < -16f)
			{
				Projectile.velocity.X *= 0.97f;
				Projectile.velocity.Y *= 0.97f;
			}
		}

		public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac)
        {
            width = height = 10;
			return true;
        }

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			if (targetHitbox.Width > 8 && targetHitbox.Height > 8)
			{
				targetHitbox.Inflate(-targetHitbox.Width / 8, -targetHitbox.Height / 8);
			}
			return projHitbox.Intersects(targetHitbox);
		}

		public bool IsStickingToTarget
		{
			get => Projectile.ai[0] == 1f;
			set => Projectile.ai[0] = value ? 1f : 0f;
		}

		public int TargetWhoAmI
		{
			get => (int)Projectile.ai[1];
			set => Projectile.ai[1] = value;
		}

		private const int MAX_STICKY_JAVELINS = 32;
		private readonly Point[] _stickingJavelins = new Point[MAX_STICKY_JAVELINS];

		public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
		{
			IsStickingToTarget = true;
			TargetWhoAmI = target.whoAmI;
			Projectile.velocity =
				(target.Center - Projectile.Center) *
				0.75f;
			Projectile.netUpdate = true;

			Projectile.damage = 0;
			UpdateStickyJavelins(target);
		}

		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
		{
			target.immune[Projectile.owner] = 0;
		}

		private void UpdateStickyJavelins(NPC target)
		{
			int currentJavelinIndex = 0;

			for (int i = 0; i < Main.maxProjectiles; i++)
			{
				Projectile currentProjectile = Main.projectile[i];
				if (i != Projectile.whoAmI
					&& currentProjectile.active
					&& currentProjectile.owner == Main.myPlayer
					&& currentProjectile.type == Projectile.type
					&& currentProjectile.ModProjectile is ChlorophyteDartProjectile javelinProjectile
					&& javelinProjectile.IsStickingToTarget
					&& javelinProjectile.TargetWhoAmI == target.whoAmI)
				{

					_stickingJavelins[currentJavelinIndex++] = new Point(i, currentProjectile.timeLeft);
					if (currentJavelinIndex >= _stickingJavelins.Length)
						break;
				}
			}

			if (currentJavelinIndex >= MAX_STICKY_JAVELINS)
			{
				int oldJavelinIndex = 0;
				for (int i = 1; i < MAX_STICKY_JAVELINS; i++)
				{
					if (_stickingJavelins[i].Y < _stickingJavelins[oldJavelinIndex].Y)
					{
						oldJavelinIndex = i;
					}
				}
				Main.projectile[_stickingJavelins[oldJavelinIndex].X].Kill();
			}
		}
		public override void AI()
		{
			if (IsStickingToTarget) StickyAI();
			else NormalAI();
		}

		private void NormalAI()
		{
			Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
			Projectile.velocity.Y += 0.075f;

			if (Projectile.velocity.Y > 16) Projectile.velocity.Y = 16;
		}

		private void StickyAI()
		{
			Projectile.ignoreWater = true;
			Projectile.tileCollide = false;
			const int aiFactor = 15;
			Projectile.localAI[0] += 1f;

			bool hitEffect = Projectile.localAI[0] % 30f == 0f;
			int projTargetIndex = (int)TargetWhoAmI;
			if (Projectile.localAI[0] >= 60 * aiFactor || projTargetIndex < 0 || projTargetIndex >= 200)
			{
				Projectile.Kill();
			}
			else if (Main.npc[projTargetIndex].active && !Main.npc[projTargetIndex].dontTakeDamage)
			{
				Main.npc[projTargetIndex].GetGlobalNPC<PolaritiesNPC>().chlorophyteDarts++;
				Projectile.Center = Main.npc[projTargetIndex].Center - Projectile.velocity * 2f;
				Projectile.gfxOffY = Main.npc[projTargetIndex].gfxOffY;
				if (hitEffect)
				{
					Main.npc[projTargetIndex].HitEffect(0, 1.0);
				}
			}
			else
			{
				Projectile.Kill();
			}
		}

		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			SoundEngine.PlaySound(SoundID.Dig, Projectile.position);
			Collision.HitTiles(Projectile.position + Projectile.velocity, Projectile.velocity, Projectile.width, Projectile.height);
			return true;
		}
	}
}