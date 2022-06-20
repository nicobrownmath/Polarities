using Polarities.NPCs;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;

namespace Polarities.Items.Materials
{
    //TODO: Make this useful for literally anything
    public class BlueQuartz : ModItem
    {
        public override void SetStaticDefaults()
        {
            this.SetResearch(25);
        }

        public override void SetDefaults()
        {
            Item.width = 12;
            Item.height = 20;
            Item.maxStack = 999;
            Item.value = 10;
            Item.rare = ItemRarityID.White;
        }
    }
}