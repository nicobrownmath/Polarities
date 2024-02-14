using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using static Terraria.ModLoader.ModContent;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Polarities.NPCs;
using System;
using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.Audio;
using Terraria.GameContent;

namespace Polarities.Items.Weapons.Ranged
{
	public class AlkalineRain : ModItem
	{
        public override void SetStaticDefaults()
        {
			this.SetResearch(1);
        }

        public override void SetDefaults()
		{
			Item.SetWeaponValues(8, 4, 0);
			Item.DamageType = DamageClass.Ranged;

			Item.width = 20;
			Item.height = 44;

			Item.useTime = 25;
			Item.useAnimation = 25;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.noMelee = true;
			Item.UseSound = SoundID.Item5;

			Item.shoot = 10;
			Item.shootSpeed = 12f;
			Item.useAmmo = AmmoID.Arrow;

			Item.value = Item.sellPrice(silver: 20);
			Item.rare = ItemRarityID.Blue;
		}

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
		{
			type = ProjectileType<AlkalineRainProjectile>();
			for (int i = -3; i < 4; i++)
			{
				position = player.Center + new Vector2(i * 160, -800);
				Vector2 shotVel = (Main.MouseWorld - position).SafeNormalize(Vector2.Zero).RotatedByRandom(0.25f) * velocity.Length();
				Projectile.NewProjectile(source, position, shotVel, type, damage, knockback, player.whoAmI);
			}
			return false;
		}

        public override void AddRecipes()
        {
			CreateRecipe()
				.AddIngredient(ItemType<Materials.AlkalineFluid>(), 10)
				.AddIngredient(ItemType<Placeable.Blocks.Limestone>(), 6)
				.AddTile(TileID.Anvils)
				.Register();
        }
    }

	public class AlkalineRainProjectile : ModProjectile
	{
		public override void SetDefaults()
		{
			Projectile.width = 2;
			Projectile.height = 2;
			DrawOffsetX = -46;
			DrawOriginOffsetY = 0;
			DrawOriginOffsetX = 23;
			Projectile.aiStyle = -1;
			Projectile.friendly = true;
			Projectile.penetrate = 1;
			Projectile.DamageType = DamageClass.Ranged;
			Projectile.arrow = true;
			Projectile.tileCollide = true;
		}

		public override void AI()
		{
			Projectile.rotation = Projectile.velocity.ToRotation();
		}

		public override void OnKill(int timeLeft)
		{
			Main.dust[Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, 74, Scale: 1f)].noGravity = true;
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Main.spriteBatch.Draw(TextureAssets.Projectile[Projectile.type].Value, Projectile.Center - Main.screenPosition, new Rectangle(0, 0, 48, 2), Color.White, Projectile.rotation, new Vector2(47, 1), Projectile.scale, SpriteEffects.None, 0f);
			return false;
		}
	}
}

