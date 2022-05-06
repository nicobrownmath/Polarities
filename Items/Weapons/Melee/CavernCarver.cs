using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using static Terraria.ModLoader.ModContent;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Polarities.Buffs;
using Terraria.DataStructures;
using Terraria.Audio;

namespace Polarities.Items.Weapons.Melee
{
	public class CavernCarver : ModItem
	{
		public override void SetStaticDefaults()
		{
			this.SetResearch(1);
		}

		public override void SetDefaults()
		{
			Item.damage = 13;
			Item.DamageType = DamageClass.Melee;
			Item.width = 30;
			Item.height = 32;
			Item.useTime = 15;
			Item.useAnimation = 15;
			Item.noUseGraphic = true;
			Item.noMelee = true;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.knockBack = 3.5f;
			Item.value = Item.sellPrice(silver: 2);
			Item.rare = ItemRarityID.Blue;
			Item.UseSound = SoundID.Item1;
			Item.autoReuse = true;
			Item.shoot = ProjectileType<CavernCarverProjectile>();
			Item.shootSpeed = 16f;
		}

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
		{
			int pattern = Main.rand.Next(3);
			float offset = Main.rand.NextFloat(MathHelper.Pi / 8);
			if (pattern != 0)
			{
				Projectile.NewProjectile(source, position, velocity.RotatedBy(-offset), type, damage, knockback, player.whoAmI);
			}
			if (pattern != 1)
			{
				Projectile.NewProjectile(source, position, velocity.RotatedBy(offset), type, damage, knockback, player.whoAmI);
			}
			return false;
		}

		public override void AddRecipes()
		{
			CreateRecipe()
				.AddIngredient(ItemType<Materials.AlkalineFluid>(), 4)
				.AddIngredient(ItemType<Placeable.Blocks.Limestone>(), 20)
				.AddTile(TileID.Anvils)
				.Register();
		}
	}

	public class CavernCarverProjectile : ModProjectile
	{
		public override string Texture => "Polarities/Items/Weapons/Melee/CavernCarver";

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("{$Mods.Polarities.ItemName.CavernCarver}");
		}

		public override void SetDefaults()
		{
			Projectile.width = 8;
			Projectile.height = 8;
			DrawOffsetX = -22;
			DrawOriginOffsetX = 11;

			Projectile.aiStyle = -1;
			Projectile.friendly = true;
			Projectile.penetrate = 1;
			Projectile.DamageType = DamageClass.Melee;
			Projectile.tileCollide = true;
			Projectile.ignoreWater = false;
			Projectile.timeLeft = 1200;
		}

		public override void AI()
		{
			if (Projectile.timeLeft == 1200 - 20)
			{
				Projectile.velocity = (Main.MouseWorld - Projectile.Center).SafeNormalize(Vector2.Zero) * Projectile.velocity.Length();
				Projectile.netUpdate = true;
			}
			else if (Projectile.timeLeft < 1200 - 20)
			{
				Projectile.velocity.Y += 0.3f;
			}
			Projectile.spriteDirection = Projectile.velocity.X > 0 ? 1 : -1;
			DrawOffsetX = Projectile.velocity.X > 0 ? -22 : 0;
			DrawOriginOffsetX = Projectile.velocity.X > 0 ? 11 : -11;
			Projectile.rotation = Projectile.velocity.ToRotation() - MathHelper.PiOver2 + 3 * Projectile.spriteDirection * MathHelper.PiOver4;
		}

		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			SoundEngine.PlaySound(0, Projectile.position);
			Collision.HitTiles(Projectile.position + Projectile.velocity, Projectile.velocity, Projectile.width, Projectile.height);
			return true;
		}

        public override void Kill(int timeLeft)
        {
            for (int i = 0; i < 4; i++)
            {
				Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustType<Dusts.LimestoneDust>(), Projectile.velocity.X, Projectile.velocity.Y);
            }
        }
    }
}