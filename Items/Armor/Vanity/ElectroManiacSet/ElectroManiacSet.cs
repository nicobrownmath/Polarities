using Terraria.ID;
using Terraria.ModLoader;

namespace Polarities.Items.Armor.Vanity.ElectroManiacSet
{
    [AutoloadEquip(EquipType.Head)]
    public class ElectroManiacHead : ModItem
    {
        public override void SetStaticDefaults()
        {
            SacrificeTotal = (1);
        }

        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 30;
            Item.value = 0;
            Item.rare = ItemRarityID.Cyan;
            Item.vanity = true;
        }
    }

    [AutoloadEquip(EquipType.Body)]
    public class ElectroManiacBody : ModItem
    {
        public override void SetStaticDefaults()
        {
            SacrificeTotal = (1);

            ArmorIDs.Body.Sets.shouldersAreAlwaysInTheBack[EquipLoader.GetEquipSlot(Mod, Name, EquipType.Body)] = true;
        }

        public override void SetDefaults()
        {
            Item.width = 36;
            Item.height = 26;
            Item.value = 0;
            Item.rare = ItemRarityID.Cyan;
            Item.vanity = true;
        }
    }

    [AutoloadEquip(EquipType.Legs)]
    public class ElectroManiacLegs : ModItem
    {
        public override void SetStaticDefaults()
        {
            SacrificeTotal = (1);
        }

        public override void SetDefaults()
        {
            Item.width = 26;
            Item.height = 18;
            Item.value = 0;
            Item.rare = ItemRarityID.Cyan;
            Item.vanity = true;
        }
    }
}

