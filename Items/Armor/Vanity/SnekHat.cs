using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Polarities.Items.Armor.Vanity
{
    [AutoloadEquip(EquipType.Head)]
    public class SnekHat : ModItem
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
            Item.rare = ItemRarityID.Blue;
            Item.vanity = true;
        }
    }
}