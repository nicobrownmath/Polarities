using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using static Terraria.ModLoader.ModContent;
using Microsoft.Xna.Framework;
using Polarities.Projectiles;
using System;
using System.Collections.Generic;

namespace Polarities.Items.Weapons.Melee
{
	public class Epee : ModItem
	{
		public override void SetStaticDefaults()
		{
			Item.staff[Type] = true;

			SacrificeTotal = (1);
		}

		public override void ModifyTooltips(List<TooltipLine> tooltips)
		{
			foreach (TooltipLine tooltip in tooltips)
			{
				if (tooltip.Name.StartsWith("Tooltip"))
				{
					tooltip.Text = tooltip.Text.Replace("{PlayerName}", Main.LocalPlayer.name);
				}
			}
		}

		public override void SetDefaults()
		{
			Item.SetWeaponValues(12, 8f, 0);
			Item.DamageType = DamageClass.Melee;

			Item.width = 62;
			Item.height = 60;

			Item.useTime = 60;
			Item.useAnimation = 30;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.noMelee = false;
			Item.noUseGraphic = false;
			Item.UseSound = SoundID.Item1;
			Item.autoReuse = false;

			Item.rare = 0;
			Item.value = 1000;
		}

        public override bool CanUseItem(Player player)
        {
			return player.itemTime == 0;
        }

        public override bool? UseItem(Player player)
		{
			player.direction = (Main.MouseWorld.X - player.Center.X > 0) ? 1 : -1;
			if (player.velocity.Y == 0)
			{
				player.velocity.X = player.direction * 8;
			}
			player.itemTime = Item.useTime;
			return null;
		}

        public override bool? CanHitNPC(Player player, NPC target)
        {
			return player.itemTime > 0 ? null : false;
        }

        public override void OnHitNPC(Player player, NPC target, int damage, float knockBack, bool crit)
		{
			player.velocity.X = -player.direction * 5;
			if (!target.immortal)
			{
				player.immune = true;
				player.immuneTime = 60;
			}
			player.itemTime = 60;
		}

		public override void UseItemHitbox(Player player, ref Rectangle hitbox, ref bool noHitbox)
		{
			Vector2 hitboxPosition = player.itemLocation + new Vector2((player.direction == 1 ? 0 : -Item.width * Item.scale * 1.414f), -4);
			hitbox = new Rectangle((int)hitboxPosition.X, (int)hitboxPosition.Y, (int)(Item.width * Item.scale * 1.414f), 8);
		}

		public override void AddRecipes()
		{
			CreateRecipe()
				.AddRecipeGroup("SilverBar", 9)
				.AddTile(TileID.Anvils)
				.Register();
		}
	}
}