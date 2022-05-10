using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using static Terraria.ModLoader.ModContent;
using Microsoft.Xna.Framework;
using Polarities.Projectiles;
using System;
using Microsoft.Xna.Framework.Graphics;
using Terraria.DataStructures;
using Terraria.Audio;
using Polarities.Items.Weapons.Ranged.Ammo;
using Polarities.Items.Materials;

namespace Polarities.Items.Weapons.Ranged
{
	public class Hemiola : ModItem
	{
		public override void SetStaticDefaults()
		{
			this.SetResearch(1);
		}

		public override void SetDefaults()
		{
			Item.SetWeaponValues(24, 5, 0);
			Item.DamageType = DamageClass.Ranged;
			Item.useAmmo = AmmoID.Dart;

			Item.width = 74;
			Item.height = 32;
			Item.useTime = 20;
			Item.useAnimation = 20;
			Item.useStyle = 5;
			Item.noMelee = true;
			Item.value = Item.sellPrice(gold: 10);
			Item.rare = ItemRarityID.LightPurple;
			Item.autoReuse = true;
			Item.shoot = 10;
			Item.shootSpeed = 10f;
		}

		public override Vector2? HoldoutOffset()
		{
			return new Vector2(-20, 7);
		}

		private int phase = 0;

        public override bool CanConsumeAmmo(Player player)
		{
			return phase != 1;
		}

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
		{
			if (phase % 2 == 0)
			{
				Projectile.NewProjectile(source, position, velocity * 1.5f, type == ProjectileType<WoodenDartProjectile>() ? ProjectileID.CursedDart : type, damage, knockback, player.whoAmI);
				SoundEngine.PlaySound(SoundID.Item, player.position, 98);
			}
			if (phase % 3 == 0)
			{
				Projectile.NewProjectile(source, position, velocity, type == ProjectileType<WoodenDartProjectile>() ? ProjectileID.IchorDart : type, (damage * 3) / 2, (knockback * 3) / 2, player.whoAmI);
				SoundEngine.PlaySound(SoundID.Item, player.position, 99);
			}

			phase = (phase + 1) % 6;
			return false;
		}

		public override void AddRecipes()
		{
			CreateRecipe()
				.AddIngredient(ItemID.DartRifle)
				.AddIngredient(ItemID.DartPistol)
				.AddIngredient(ItemType<EvilDNA>())
				.AddTile(TileID.DemonAltar)
				.Register();
		}
	}
}