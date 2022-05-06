using Terraria.ModLoader;
using Terraria.ID;
using Polarities.NPCs;
using static Terraria.ModLoader.ModContent;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using Terraria;
using Polarities.Items.Materials;

namespace Polarities.Items.Armor.Vanity
{
	[AutoloadEquip(EquipType.Head)]
	public class SeaSnekHat : ModItem
	{
		public override void SetStaticDefaults()
		{
			this.SetResearch(1);

			int equipSlotHead = Mod.GetEquipSlot(Name, EquipType.Head);
			ArmorIDs.Head.Sets.DrawHead[equipSlotHead] = true;
			ArmorIDs.Head.Sets.DrawBackHair[equipSlotHead] = true;
			ArmorIDs.Head.Sets.DrawHatHair[equipSlotHead] = true;
			ArmorIDs.Head.Sets.DrawFullHair[equipSlotHead] = false;
		}

		public override void SetDefaults()
		{
			Item.width = 22;
			Item.height = 18;
			Item.rare = ItemRarityID.Pink;
			Item.vanity = true;
		}

        public override void AddRecipes()
        {
			CreateRecipe()
				.AddIngredient(ItemType<SnekHat>())
				.AddIngredient(ItemType<SerpentScale>(), 20)
				.Register();
		}
    }
}