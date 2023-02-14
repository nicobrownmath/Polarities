using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Polarities.Items.Armor.Vanity
{
    [AutoloadEquip(EquipType.Head)]
    public class SunPixieMask : ModItem, IDrawArmor
    {
        public override void SetStaticDefaults()
        {
            SacrificeTotal = (1);

            int equipSlotHead = EquipLoader.GetEquipSlot(Mod, Name, EquipType.Head);
            ArmorIDs.Head.Sets.DrawHead[equipSlotHead] = false;

            //registers a head glowmask
            ArmorMasks.headIndexToArmorDraw.TryAdd(equipSlotHead, this);
        }

        public override void SetDefaults()
        {
            Item.width = 34;
            Item.height = 26;
            Item.rare = ItemRarityID.Blue;
            Item.vanity = true;
        }

        public void DrawArmor(ref PlayerDrawSet drawInfo)
        {
            if (!drawInfo.drawPlayer.invis)
            {
                Rectangle bodyFrame3 = drawInfo.drawPlayer.bodyFrame;
                Vector2 headVect2 = drawInfo.headVect;
                if (drawInfo.drawPlayer.gravDir == 1f)
                {
                    bodyFrame3.Height -= 4;
                }
                else
                {
                    headVect2.Y -= 4f;
                    bodyFrame3.Height -= 4;
                }
                Vector2 helmetOffset = drawInfo.helmetOffset;
                DrawData data = new DrawData(TextureAssets.ArmorHead[EquipLoader.GetEquipSlot(Mod, Name, EquipType.Head)].Value, helmetOffset + new Vector2((int)(drawInfo.Position.X - Main.screenPosition.X - drawInfo.drawPlayer.bodyFrame.Width / 2 + drawInfo.drawPlayer.width / 2), (int)(drawInfo.Position.Y - Main.screenPosition.Y + drawInfo.drawPlayer.height - drawInfo.drawPlayer.bodyFrame.Height + 4f)) + drawInfo.drawPlayer.headPosition + drawInfo.headVect, bodyFrame3, Color.White, drawInfo.drawPlayer.headRotation, headVect2, 1f, drawInfo.playerEffect, 0)
                {
                    shader = drawInfo.cHead
                };
                drawInfo.DrawDataCache.Add(data);
            }
        }
    }
}