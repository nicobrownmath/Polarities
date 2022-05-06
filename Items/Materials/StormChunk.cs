using Polarities.NPCs;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using Terraria.DataStructures;

namespace Polarities.Items.Materials
{
	public class StormChunk : ModItem
	{
		public override void SetStaticDefaults()
		{
			this.SetResearch(25);
			Main.RegisterItemAnimation(Item.type, new DrawAnimationVertical(5, 8));
		}

		public override void SetDefaults()
		{
			Item.width = 28;
			Item.height = 18;
			Item.maxStack = 999;
			Item.value = 50;
			Item.rare = 1;
		}

        public override void AddRecipes()
        {
			CreateRecipe(5)
				.AddIngredient(ItemType<NPCs.Critters.StormcloudCichlidItem>())
				.Register();
        }
    }
}