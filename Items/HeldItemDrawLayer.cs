using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Graphics.Shaders;
using static Terraria.ModLoader.ModContent;
using Terraria.GameInput;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using System.Collections.Generic;
using Polarities.Items;
using Polarities.NPCs;
using MonoMod.Cil;
using Terraria.ModLoader.IO;
using Terraria.Enums;

namespace Polarities.Items
{
    public interface IDrawHeldItem
    {
        void DrawHeldItem(ref PlayerDrawSet drawInfo);

        bool DoVanillaDraw();
    }

    public class HeldItemDrawLayer : PlayerDrawLayer
    {
        public override Position GetDefaultPosition() => new Between(PlayerDrawLayers.ArmOverItem, PlayerDrawLayers.HeldItem);

        public override bool GetDefaultVisibility(PlayerDrawSet drawInfo)
        {
            return drawInfo.drawPlayer.itemAnimation != 0 && !drawInfo.drawPlayer.HeldItem.noUseGraphic && drawInfo.drawPlayer.HeldItem.ModItem != null && drawInfo.drawPlayer.HeldItem.ModItem is IDrawHeldItem;
        }

        protected override void Draw(ref PlayerDrawSet drawInfo)
        {
            (drawInfo.drawPlayer.HeldItem.ModItem as IDrawHeldItem)?.DrawHeldItem(ref drawInfo);
        }
    }
}

