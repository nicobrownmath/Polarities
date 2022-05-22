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
using Terraria.GameContent.Creative;

namespace Polarities.Items.Weapons.Melee
{
	public class SaltKnife : ModItem
	{
		public override void SetStaticDefaults()
		{
			this.SetResearch(1);
		}

        public override void SetDefaults()
		{
			Item.SetWeaponValues(10, 1, 0);
			Item.DamageType = DamageClass.Melee;
			Item.maxStack = 5;

			Item.noMelee = true;
			Item.noUseGraphic = true;
			Item.autoReuse = true;

			Item.shoot = ProjectileType<SaltKnifeProjectile>();
			Item.shootSpeed = 1f;

			Item.width = 28;
			Item.height = 26;

			Item.useTime = 40;
			Item.useAnimation = 40;

			//TODO: See if I can make this use the shortsword-esque arm system
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.UseSound = SoundID.Item1;

			Item.value = Item.sellPrice(silver: 4);
			Item.rare = ItemRarityID.Blue;
		}

        public override void UpdateInventory(Player player)
        {
			Item.maxStack = 5;
			Item.useTime = (int)Math.Ceiling(40f / Item.stack);
			Item.useAnimation = (int)Math.Ceiling(40f / Item.stack);
		}

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
			if (player.ownedProjectileCounts[Item.shoot] < Item.stack)
			{
				Projectile.NewProjectile(source, position, Vector2.Zero, type, damage, knockback, player.whoAmI, 0, 40);
			}
			return false;
		}

		public override void AddRecipes()
		{
			CreateRecipe()
				.AddIngredient(ItemType<Items.Placeable.SaltCrystals>(), 6)
				.AddTile(TileID.Anvils)
				.Register();
		}
	}

	public class SaltKnifeProjectile : ModProjectile
	{
        public override string Texture => "Polarities/Items/Weapons/Melee/SaltKnife";

        private float rotationOffset;
		private int timer;
		private int atkCooldown;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("{$Mods.Polarities.ItemName.SaltKnife}");
        }

        public override void SetDefaults()
		{
			Projectile.width = 28;
			Projectile.height = 28;
			Projectile.aiStyle = -1;
			Projectile.friendly = true;
			Projectile.penetrate = -1;
			Projectile.DamageType = DamageClass.Melee;
			Projectile.timeLeft = 40;
			Projectile.tileCollide = false;
		}

		public override void AI()
		{
			if (Main.player[Projectile.owner].dead || !Main.player[Projectile.owner].active)
			{
				Projectile.active = false;
			}

			timer++;
			atkCooldown--;
			Projectile.friendly = atkCooldown < 0;

			rotationOffset = -Main.player[Projectile.owner].direction * 0.5f * (float)Math.Cos(Projectile.ai[0] + timer * 2 * Math.PI / Projectile.ai[1]);
			Projectile.rotation = rotationOffset + (Main.MouseWorld - Main.player[Projectile.owner].Center).ToRotation() + MathHelper.Pi / 4;
			Projectile.position = -(new Vector2(Projectile.width / 2, Projectile.height / 2)) + Main.player[Projectile.owner].Center + (new Vector2(1, -1)).RotatedBy(Projectile.rotation) * Projectile.width * 0.5f * (1 + 0.2f * (1 + (float)Math.Sin(Projectile.ai[0] + timer * 2 * Math.PI / Projectile.ai[1])));

			Projectile.velocity = Vector2.Zero;
		}

		public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
		{
			hitDirection = Main.player[Projectile.owner].direction;
		}

		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
		{
			target.immune[Projectile.owner] = 0;
			atkCooldown = (int)Projectile.ai[1] / 2;
		}
	}
}