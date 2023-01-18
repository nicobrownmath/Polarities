using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Polarities.Items.Weapons.Ranged.Ammo
{
	public class BoneDart : ModItem
	{
		public override void SetStaticDefaults()
		{
			SacrificeTotal = (99);
		}

		public override void SetDefaults()
		{
			Item.SetWeaponValues(5, 4f, 0);
			Item.DamageType = DamageClass.Ranged;

			Item.width = 14;
			Item.height = 24;

			Item.maxStack = 9999;
			Item.consumable = true;

			Item.shoot = ProjectileType<BoneDartProjectile>();
			Item.shootSpeed = 3f;
			Item.ammo = AmmoID.Dart;

			Item.value = Item.sellPrice(copper: 1);
			Item.rare = ItemRarityID.Green;
		}

		public override void AddRecipes()
		{
			CreateRecipe(20)
				.AddIngredient(ItemID.Bone)
				.AddTile(TileID.WorkBenches)
				.Register();
		}
	}

	public class BoneDartProjectile : ModProjectile
	{
		public override string Texture => "Polarities/Items/Weapons/Ranged/Ammo/BoneDart";

		public override void SetDefaults()
		{
			Projectile.aiStyle = -1;
			Projectile.friendly = true;
			Projectile.DamageType = DamageClass.Ranged;
			Projectile.width = 14;
			Projectile.height = 14;
			Projectile.alpha = 0;
			Projectile.timeLeft = 3600;
			Projectile.penetrate = -1;
			Projectile.tileCollide = true;
			Projectile.ignoreWater = false;

			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 60;
		}

		public override void OnSpawn(IEntitySource source)
		{
			while (Projectile.velocity.X >= 16f || Projectile.velocity.X <= -16f || Projectile.velocity.Y >= 16f || Projectile.velocity.Y < -16f)
			{
				Projectile.velocity.X *= 0.97f;
				Projectile.velocity.Y *= 0.97f;
			}
		}

		public override void AI()
		{
			if (Projectile.ai[0] == 0)
			{
				Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
			}
			else
			{
				Projectile.rotation += Projectile.velocity.X * 0.1f;
				Projectile.velocity.X *= 0.98f;
				Projectile.velocity.Y += 0.6f;
			}
		}

		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
		{
			Projectile.ai[0] = 1;
		}

		public override void OnHitPvp(Player target, int damage, bool crit)
		{
			Projectile.ai[0] = 1;
		}

		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			SoundEngine.PlaySound(SoundID.Dig, Projectile.position);
			Collision.HitTiles(Projectile.position + Projectile.velocity, Projectile.velocity, Projectile.width, Projectile.height);
			return true;
		}
	}
}

