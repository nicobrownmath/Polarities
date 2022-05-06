using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using static Terraria.ModLoader.ModContent;
using Microsoft.Xna.Framework;
using System;
using Polarities.Projectiles;
using Terraria.DataStructures;

namespace Polarities.Items.Weapons.Ranged
{
	public class ToxicShot : ModItem
	{
		public override void SetStaticDefaults()
		{
			this.SetResearch(1);
		}

		public override void SetDefaults()
		{
			Item.SetWeaponValues(40, 1f, 0);
			Item.DamageType = DamageClass.Ranged;
			Item.useAmmo = AmmoID.Arrow;

			Item.width = 28;
			Item.height = 72;

			Item.useAnimation = 30;
			Item.useTime = 6;
			Item.reuseDelay = 45;
			Item.useStyle = 5;
			Item.noMelee = true;
			Item.UseSound = SoundID.Item13;
			Item.autoReuse = true;

			Item.shoot = 10;
			Item.shootSpeed = 12f;

			Item.value = Item.sellPrice(gold: 2);
			Item.rare = ItemRarityID.LightPurple;
		}

		public override bool CanConsumeAmmo(Player player)
		{
			return !(player.itemAnimation < Item.useAnimation - 2);
		}

        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
		{
			type = ProjectileType<ToxicShotProjectile>();

			float a = 0.1f;
			float v = velocity.Length();
			float x = Main.MouseWorld.X - player.Center.X;
			float y = Main.MouseWorld.Y - player.Center.Y;

			double theta = (new Vector2(x, y)).ToRotation();
			theta += Math.PI / 2;
			if (theta > Math.PI) { theta -= Math.PI * 2; }
			theta *= 0.5;
			theta -= Math.PI / 2;

			double num0 = -4 * Math.Pow(a, 2) * Math.Pow(x, 4) + 4 * a * Math.Pow(v, 2) * Math.Pow(x, 2) * y + Math.Pow(v, 4) * Math.Pow(x, 2);
			if (num0 > 0)
			{
				num0 = -player.direction * Math.Sqrt(num0);
				double num1 = a * x * x - v * v * y;

				theta = -2 * Math.Atan(
					num0 / (2 * num1) +
					0.5 * Math.Sqrt(Math.Max(0,
						-(
							(num1 * (-16 * Math.Pow(v, 2) * x * (a * x * x + v * v * y) / Math.Pow(num1, 2) - 16 * Math.Pow(v, 2) * x / num1 + 8 * Math.Pow(v, 6) * Math.Pow(x, 3) / Math.Pow(num1, 3))) /
							(4 * num0)
						)
						- 2 * (a * x * x + v * v * y) / num1 + 2 + 2 * Math.Pow(v, 4) * Math.Pow(x, 2) / (num1 * num1)
					)) -
					Math.Pow(v, 2) * x / (2 * num1)
				); //some magic thingy idk

				int thetaDir = Math.Cos(theta) < 0 ? -1 : 1;
				if (thetaDir != player.direction) { theta -= Math.PI; }
			}

			velocity = new Vector2(v, 0).RotatedBy(theta);
		}

		public override void AddRecipes()
		{
			CreateRecipe()
				.AddIngredient(ItemType<Materials.VenomGland>(), 4)
				.AddRecipeGroup("Polarities:AdamantiteBar", 12)
				.AddTile(TileID.MythrilAnvil)
				.Register();
		}
	}

	public class ToxicShotProjectile : ModProjectile
	{
		public override string Texture => "Polarities/Textures/Pixel";

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("{$Mods.Polarities.ProjectileName.SpitterVenom}");
		}

		public override void SetDefaults()
		{
			Projectile.aiStyle = -1;
			Projectile.friendly = true;
			Projectile.DamageType = DamageClass.Ranged;
			Projectile.arrow = true;
			Projectile.width = 2;
			Projectile.height = 2;
			Projectile.alpha = 0;
			Projectile.timeLeft = 3600;
			Projectile.penetrate = 1;
			Projectile.tileCollide = true;
			Projectile.ignoreWater = true;
			Projectile.hide = true;
		}

		public override void AI()
		{
			Main.dust[Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.ToxicBubble, Scale: 1.5f)].noGravity = true;
			Main.dust[Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.ToxicBubble, Scale: 1.5f)].noGravity = true;
			Projectile.velocity.Y += 0.2f;
		}

		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
		{
			target.AddBuff(BuffID.Venom, 300);
		}
	}
}