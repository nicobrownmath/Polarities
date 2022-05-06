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
	public class StarConstructMask : ModItem, IDrawArmor
	{
        Asset<Texture2D> GlowTexture;

        public override void Load()
        {
            GlowTexture = Request<Texture2D>(Texture + "_Head_Mask");
        }

        public override void Unload()
        {
            GlowTexture = null;
        }

        public override void SetStaticDefaults()
		{
			this.SetResearch(1);

			int equipSlotHead = Mod.GetEquipSlot(Name, EquipType.Head);
			ArmorIDs.Head.Sets.DrawHead[equipSlotHead] = false;

            //registers a head glowmask
            ArmorMasks.headIndexToArmorDraw.TryAdd(equipSlotHead, this);
        }

		public override void SetDefaults()
		{
			Item.width = 32;
			Item.height = 30;
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
                DrawData data = new DrawData(GlowTexture.Value, helmetOffset + new Vector2((float)(int)(drawInfo.Position.X - Main.screenPosition.X - (float)(drawInfo.drawPlayer.bodyFrame.Width / 2) + (float)(drawInfo.drawPlayer.width / 2)), (float)(int)(drawInfo.Position.Y - Main.screenPosition.Y + (float)drawInfo.drawPlayer.height - (float)drawInfo.drawPlayer.bodyFrame.Height + 4f)) + drawInfo.drawPlayer.headPosition + drawInfo.headVect, bodyFrame3, Color.White, drawInfo.drawPlayer.headRotation, headVect2, 1f, drawInfo.playerEffect, 0);
                data.shader = drawInfo.cHead;
                drawInfo.DrawDataCache.Add(data);
            }
        }
    }
}