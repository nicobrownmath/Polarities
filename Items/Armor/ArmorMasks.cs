using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace Polarities.Items.Armor
{
    public interface IDrawArmor
    {
        void DrawArmor(ref PlayerDrawSet drawInfo);

        virtual bool DoVanillaDraw() => true;
    }

    public interface IGetBodyMaskColor
    {
        Color BodyColor(ref PlayerDrawSet drawInfo);
    }

    public class HeadMaskDrawLayer : PlayerDrawLayer
    {
        public override Position GetDefaultPosition() => new Between(PlayerDrawLayers.Head, PlayerDrawLayers.FinchNest);

        public override bool GetDefaultVisibility(PlayerDrawSet drawInfo)
        {
            return ArmorMasks.headIndexToArmorDraw.ContainsKey(drawInfo.drawPlayer.head);
        }

        protected override void Draw(ref PlayerDrawSet drawInfo)
        {
            ArmorMasks.headIndexToArmorDraw[drawInfo.drawPlayer.head].DrawArmor(ref drawInfo);
        }
    }

    public class LegMaskDrawLayer : PlayerDrawLayer
    {
        public override Position GetDefaultPosition() => new Between(PlayerDrawLayers.Leggings, PlayerDrawLayers.Shoes);

        public override bool GetDefaultVisibility(PlayerDrawSet drawInfo)
        {
            return ArmorMasks.legIndexToArmorDraw.ContainsKey(drawInfo.drawPlayer.legs);
        }

        protected override void Draw(ref PlayerDrawSet drawInfo)
        {
            ArmorMasks.legIndexToArmorDraw[drawInfo.drawPlayer.legs].DrawArmor(ref drawInfo);
        }
    }

    public class WingMaskDrawLayer : PlayerDrawLayer
    {
        public override Position GetDefaultPosition() => new Between(PlayerDrawLayers.Wings, PlayerDrawLayers.BackAcc);

        public override bool GetDefaultVisibility(PlayerDrawSet drawInfo)
        {
            return ArmorMasks.wingIndexToArmorDraw.ContainsKey(drawInfo.drawPlayer.wings);
        }

        protected override void Draw(ref PlayerDrawSet drawInfo)
        {
            ArmorMasks.wingIndexToArmorDraw[drawInfo.drawPlayer.wings].DrawArmor(ref drawInfo);
        }
    }

    public class ArmorMasks : ILoadable
    {
        public static Dictionary<int, IDrawArmor> headIndexToArmorDraw;
        public static Dictionary<int, IDrawArmor> legIndexToArmorDraw;
        public static Dictionary<int, IDrawArmor> wingIndexToArmorDraw;

        public static Dictionary<int, IGetBodyMaskColor> bodyIndexToBodyMaskColor;

        public void Load(Mod mod)
        {
            headIndexToArmorDraw = new Dictionary<int, IDrawArmor>();
            legIndexToArmorDraw = new Dictionary<int, IDrawArmor>();
            wingIndexToArmorDraw = new Dictionary<int, IDrawArmor>();
            bodyIndexToBodyMaskColor = new Dictionary<int, IGetBodyMaskColor>();
        }

        public void Unload()
        {
            headIndexToArmorDraw = null;
            legIndexToArmorDraw = null;
            wingIndexToArmorDraw = null;
            bodyIndexToBodyMaskColor = null;
        }

        public static void DrawHeadBasic(PlayerDrawSet drawInfo, Asset<Texture2D> glowTexture)
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
                drawInfo.DrawDataCache.Add(new DrawData(glowTexture.Value, drawInfo.helmetOffset + new Vector2((int)(drawInfo.Position.X - Main.screenPosition.X - drawInfo.drawPlayer.bodyFrame.Width / 2 + drawInfo.drawPlayer.width / 2), (int)(drawInfo.Position.Y - Main.screenPosition.Y + drawInfo.drawPlayer.height - drawInfo.drawPlayer.bodyFrame.Height + 4f)) + drawInfo.drawPlayer.headPosition + drawInfo.headVect, bodyFrame3, Color.White, drawInfo.drawPlayer.headRotation, headVect2, 1f, drawInfo.playerEffect, 0) { shader = drawInfo.cHead, });
            }
        }
    }
}

