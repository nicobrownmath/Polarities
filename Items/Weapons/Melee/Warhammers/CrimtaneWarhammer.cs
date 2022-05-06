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

namespace Polarities.Items.Weapons.Melee.Warhammers
{
	public class CrimtaneWarhammer : WarhammerBase
	{
		public override int HammerLength => 52;
		public override int HammerHeadSize => 14;
		public override int DefenseLoss => 12;
		public override int DebuffTime => 720;

		public override void SetDefaults()
		{
			Item.SetWeaponValues(28, 13, 0);
			Item.DamageType = DamageClass.Melee;

			Item.width = 66;
			Item.height = 66;

			Item.useTime = 40;
			Item.useAnimation = 40;
			Item.useStyle = WarhammerUseStyle;

			Item.value = Item.sellPrice(silver: 30);
			Item.rare = ItemRarityID.Blue;
		}

		public override void AddRecipes()
		{
			CreateRecipe()
				.AddIngredient(ItemID.CrimtaneBar, 15)
				.AddTile(TileID.Anvils)
				.Register();
		}
	}
}