﻿using Terraria;
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

namespace Polarities.Items.Weapons.Melee.Warhammers
{
	public class CobaltWarhammer : WarhammerBase
	{
		public override int HammerLength => 67;
		public override int HammerHeadSize => 17;
		public override int DefenseLoss => 20;
		public override int DebuffTime => 1200;

		public override void SetDefaults()
		{
			Item.SetWeaponValues(52, 15, 0);
			Item.DamageType = DamageClass.Melee;

			Item.width = 84;
			Item.height = 84;

			Item.useTime = 40;
			Item.useAnimation = 40;
			Item.useStyle = WarhammerUseStyle;

			Item.value = Item.sellPrice(gold: 1);
			Item.rare = ItemRarityID.LightRed;
		}

		public override bool AltFunctionUse(Player player)
		{
			return true;
		}

		public override bool CanUseItem(Player player)
		{
			return player.ownedProjectileCounts[ProjectileType<CobaltWarhammerProjectile>()] < 1;
		}

		public override bool? UseItem(Player player)
		{
			if (player.altFunctionUse == 2)
			{
				Item.useStyle = ItemUseStyleID.Thrust;
				Item.shoot = ProjectileType<CobaltWarhammerProjectile>();
				Item.shootSpeed = 2.5f;
				Item.noMelee = true;
				Item.noUseGraphic = true;
			}
			else
			{
				Item.useStyle = WarhammerUseStyle;
				Item.shoot = 0;
				Item.shootSpeed = 0f;
				Item.noMelee = false;
				Item.noUseGraphic = false;
			}
			return null;
		}

		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
		{
			damage /= 2;
			knockback /= 2;
		}

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
		{
			return player.altFunctionUse == 2;
		}

		public override void AddRecipes()
		{
			CreateRecipe()
				.AddIngredient(ItemID.CobaltBar, 15)
				.AddTile(TileID.Anvils)
				.Register();
		}
	}


	public class CobaltWarhammerProjectile : ModProjectile
	{
		public override string Texture => "Polarities/Items/Weapons/Melee/Warhammers/CobaltWarhammer";

		public override void SetStaticDefaults()
		{
			// DisplayName.SetDefault("{$Mods.Polarities.ItemName.CobaltWarhammer}");
		}

		public override void SetDefaults()
		{
			Projectile.width = 34;
			Projectile.height = 34;
			DrawOffsetX = 17 * 2 - 42 * 2;
			DrawOriginOffsetY = 0;
			DrawOriginOffsetX = 42 - 17;

			Projectile.aiStyle = -1;
			Projectile.penetrate = -1;
			Projectile.scale = 1f;
			Projectile.alpha = 0;

			Projectile.hide = true;
			Projectile.ownerHitCheck = true;
			Projectile.DamageType = DamageClass.Melee;
			Projectile.tileCollide = false;
			Projectile.friendly = true;
		}

		public float movementFactor
		{
			get => Projectile.ai[0];
			set => Projectile.ai[0] = value;
		}

		public override void AI()
		{
			Player projOwner = Main.player[Projectile.owner];
			Vector2 ownerMountedCenter = projOwner.RotatedRelativePoint(projOwner.MountedCenter, true);
			Projectile.direction = projOwner.direction;
			projOwner.heldProj = Projectile.whoAmI;
			projOwner.itemTime = projOwner.itemAnimation;
			Projectile.Center = ownerMountedCenter;
			if (!projOwner.frozen)
			{
				if (movementFactor == 0f)
				{
					movementFactor = 3f;
				}
				if (projOwner.itemAnimation < projOwner.itemAnimationMax / 3)
				{
					movementFactor -= 1.6f;
				}
				else
				{
					movementFactor += 1.4f;
				}
			}

			Projectile.position += Projectile.velocity * movementFactor;
			if (projOwner.itemAnimation == 0)
			{
				Projectile.Kill();
			}
			Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.ToRadians(45f);
			if (Projectile.spriteDirection == -1)
			{
				Projectile.rotation -= MathHelper.ToRadians(90f);
			}
		}
	}
}

