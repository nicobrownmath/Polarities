using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Polarities.Items.Armor.Vanity
{
    [AutoloadEquip(EquipType.Head)]
    public class StormCloudfishMask : ModItem
    {
        public override void SetStaticDefaults()
        {
            SacrificeTotal = (1);

            int equipSlotHead = EquipLoader.GetEquipSlot(Mod, Name, EquipType.Head);
            ArmorIDs.Head.Sets.DrawHead[equipSlotHead] = false;
        }

        public override void SetDefaults()
        {
            Item.width = 22;
            Item.height = 22;
            Item.rare = ItemRarityID.Blue;
            Item.vanity = true;
        }
    }
}