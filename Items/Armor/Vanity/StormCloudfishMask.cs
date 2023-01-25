using Terraria.ModLoader;
using Terraria.ID;
using Polarities.NPCs;
using static Terraria.ModLoader.ModContent;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using Terraria;
using Terraria.GameContent;
using ReLogic.Content;

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