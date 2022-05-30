using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Polarities.Items.Weapons.Ranged.Ammo
{
	public class BouncyDart : ModItem
	{
		public override void SetStaticDefaults()
		{
			this.SetResearch(99);
		}

		public override void SetDefaults()
		{
			Item.SetWeaponValues(5, 1, 0);
			Item.DamageType = DamageClass.Ranged;

			Item.width = 10;
			Item.height = 22;

			Item.maxStack = 999;
			Item.consumable = true;

			Item.shoot = ProjectileType<BouncyDartProjectile>();
			Item.shootSpeed = 1f;
			Item.ammo = AmmoID.Dart;

			Item.value = Item.sellPrice(copper: 1);
			Item.rare = ItemRarityID.White;
		}

		public override void AddRecipes()
		{
			CreateRecipe(10)
				.AddIngredient(ItemType<WoodenDart>(), 10)
				.AddIngredient(ItemID.PinkGel)
				.AddTile(TileID.WorkBenches)
				.Register();
		}
	}

	public class BouncyDartProjectile : ModProjectile
	{
		public override string Texture => "Polarities/Items/Weapons/Ranged/Ammo/BouncyDart";

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("{$Mods.Polarities.ItemName.BouncyDart}");
		}

		public override void SetDefaults()
		{
			Projectile.aiStyle = -1;
			Projectile.friendly = true;
			Projectile.DamageType = DamageClass.Ranged;
			Projectile.width = 10;
			Projectile.height = 10;
			Projectile.alpha = 0;
			Projectile.timeLeft = 3600;
			Projectile.penetrate = 1;
			Projectile.tileCollide = true;
			Projectile.ignoreWater = false;
		}

		public override void AI()
		{
			Projectile.rotation = Projectile.velocity.ToRotation() + (float)Math.PI / 2;
			Projectile.velocity.Y += 0.3f;
		}

		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			SoundEngine.PlaySound(SoundID.Dig, Projectile.position);
			Collision.HitTiles(Projectile.position + Projectile.velocity, Projectile.velocity, Projectile.width, Projectile.height);

			if (Projectile.velocity.X != oldVelocity.X)
			{
				Projectile.velocity.X = -oldVelocity.X;
			}
			if (Projectile.velocity.Y != oldVelocity.Y)
			{
				Projectile.velocity.Y = -oldVelocity.Y;
			}

			Projectile.ai[0]++;
			return Projectile.ai[0] > 3;
		}
	}
}

