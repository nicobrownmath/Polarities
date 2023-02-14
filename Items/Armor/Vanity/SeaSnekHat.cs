using Polarities.Items.Materials;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Polarities.Items.Armor.Vanity
{
    [AutoloadEquip(EquipType.Head)]
    public class SeaSnekHat : ModItem
    {
        public override void SetStaticDefaults()
        {
            SacrificeTotal = (1);

            int equipSlotHead = EquipLoader.GetEquipSlot(Mod, Name, EquipType.Head);
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