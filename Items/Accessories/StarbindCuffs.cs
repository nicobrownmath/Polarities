using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using Polarities.Items.Placeable;
using System.Collections.Generic;
using System;
using Terraria.GameContent;
using Microsoft.Xna.Framework.Graphics;

namespace Polarities.Items.Accessories
{
	[AutoloadEquip(EquipType.HandsOn)]
	public class StarbindCuffs : ModItem
	{
        public override void SetStaticDefaults()
		{
			this.SetResearch(1);

			ArmorIDs.HandOn.Sets.UsesOldFramingTexturesForWalking[Mod.GetAccessorySlot(Name, EquipType.HandsOn)] = true;
			TextureAssets.AccHandsOn[Mod.GetAccessorySlot(Name, EquipType.HandsOn)] = Request<Texture2D>(Texture + "_HandsOn_Old");
		}

		public override void SetDefaults()
		{
			Item.width = 50;
			Item.height = 50;
			Item.accessory = true;
			Item.value = Item.sellPrice(gold: 4);
			Item.rare = ItemRarityID.Lime;
		}

		public override void UpdateAccessory(Player player, bool hideVisual)
		{
			player.GetModPlayer<PolaritiesPlayer>().manaStarMultiplier *= 2;
			player.manaMagnet = true;
			player.magicCuffs = true;
			player.statManaMax2 += 20;
		}

		public override void AddRecipes()
		{
			CreateRecipe()
				.AddIngredient(ItemID.CelestialCuffs)
				.AddIngredient(ItemType<StarBadge>())
				.AddTile(TileID.TinkerersWorkbench)
				.Register();
		}
	}
}
