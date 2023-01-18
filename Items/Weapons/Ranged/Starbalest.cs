using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using static Terraria.ModLoader.ModContent;
using Microsoft.Xna.Framework;
using System;
using Polarities.Projectiles;
using Terraria.Audio;
using Terraria.DataStructures;

namespace Polarities.Items.Weapons.Ranged
{
	public class Starbalest : ModItem
	{
		private int charges;
		public override void SetStaticDefaults()
		{
			SacrificeTotal = (1);
		}

		public override void SetDefaults()
		{
			Item.SetWeaponValues(60, 0f, 0);
			Item.DamageType = DamageClass.Ranged;

			Item.width = 60;
			Item.height = 62;

			Item.useAnimation = 14;
			Item.useTime = 14;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.noMelee = true;
			Item.channel = true;
			Item.autoReuse = true;

			Item.shoot = ProjectileID.FallingStar;
			Item.shootSpeed = 16f;
			Item.useAmmo = AmmoID.FallenStar;

			Item.value = 100000;
			Item.rare = ItemRarityID.Green;
		}

		public override void HoldItem(Player player)
		{
			if (!player.channel && charges > 0)
			{
				for (int i = 0; i < charges; i++)
				{
					Projectile shot = Main.projectile[Projectile.NewProjectile(player.GetSource_ItemUse(Item), player.MountedCenter, new Vector2(Item.shootSpeed * Main.rand.NextFloat(0.75f, 1.333f), 0).RotatedBy((Main.MouseWorld - player.MountedCenter).ToRotation()).RotatedByRandom(0.5f), Item.shoot, Item.damage, Item.knockBack, player.whoAmI)];
					shot.maxPenetrate = 1;
					shot.penetrate = 1;
					shot.timeLeft = 600;
				}
				charges = 0;

				SoundEngine.PlaySound(SoundID.Item9, player.Center);
			}
			else if (player.channel && !player.HasAmmo(Item))
			{
				player.itemTime = 10;
				player.itemAnimation = 10;
			}

			if (player.channel)
			{
				player.itemRotation = (Main.MouseWorld - player.MountedCenter).ToRotation();
				if (player.direction == -1) { player.itemRotation += (float)Math.PI; }
			}
		}

        public override bool CanConsumeAmmo(Item ammo, Player player)
		{
			return charges < 20;
		}

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
		{
			if (charges < 20)
			{
				charges++;
				SoundEngine.PlaySound(SoundID.Item4, player.MountedCenter);

				for (int i = 0; i < 5; i++)
				{
					Vector2 position30 = player.Center;
					int width27 = 0;
					int height27 = 0;
					float speedX13 = player.velocity.X * 0.5f;
					float speedY13 = player.velocity.Y * 0.5f;
					Color newColor = default(Color);
					Dust.NewDust(position30, width27, height27, 58, speedX13, speedY13, 150, newColor, 1.2f);
				}
				for (int i = 0; i < 3; i++)
				{
					Gore.NewGore(player.GetSource_ItemUse(Item), player.MountedCenter - new Vector2(8, 8), new Vector2(player.velocity.X * 0.2f, player.velocity.Y * 0.2f), Main.rand.Next(16, 18));
				}
			}

			return false;
		}

		public override Vector2? HoldoutOffset()
		{
			return new Vector2(-28, 0);
		}

		public override void AddRecipes()
		{
			CreateRecipe()
				.AddIngredient(ItemID.GoldBow)
				.AddIngredient(ItemType<Placeable.Bars.SunplateBar>(), 20)
				.AddIngredient(ItemID.FallenStar, 5)
				.AddTile(TileID.SkyMill)
				.Register();

			CreateRecipe()
				.AddIngredient(ItemID.PlatinumBow)
				.AddIngredient(ItemType<Placeable.Bars.SunplateBar>(), 20)
				.AddIngredient(ItemID.FallenStar, 5)
				.AddTile(TileID.SkyMill)
				.Register();
		}
	}
}