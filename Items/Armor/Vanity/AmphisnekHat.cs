using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Polarities.Items.Armor.Vanity
{
    [AutoloadEquip(EquipType.Head)]
    public class AmphisnekHat : ModItem, IDrawArmor
    {
        public override void SetStaticDefaults()
        {
            SacrificeTotal = 1;
            ArmorIDs.Head.Sets.DrawHead[Item.headSlot] = true;
            ArmorIDs.Head.Sets.DrawBackHair[Item.headSlot] = true;
            ArmorIDs.Head.Sets.DrawHatHair[Item.headSlot] = true;
            ArmorIDs.Head.Sets.DrawFullHair[Item.headSlot] = false;

            ArmorMasks.headIndexToArmorDraw.TryAdd(Item.headSlot, this);
        }

        public override void SetDefaults()
        {
            Item.width = 22;
            Item.height = 18;
            Item.rare = ItemRarityID.Blue;
            Item.vanity = true;
        }

        public void DrawArmor(ref PlayerDrawSet drawInfo)
        {
            ArmorMasks.DrawHeadBasic(drawInfo, ModContent.Request<Texture2D>($"{Texture}_Head_Mask"));
        }
    }
}