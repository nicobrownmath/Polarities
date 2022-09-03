using Polarities.NPCs;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using Polarities.NPCs.SunPixie;
using Terraria.GameContent;
using Terraria.Localization;
using Polarities.NPCs.Eclipxie;

namespace Polarities.Items
{
	//TODO: Inventory glow effect thing during the eclipse
	public class SunPixieSummonItem : ModItem
	{
		public override void SetStaticDefaults()
		{
			this.SetResearch(1);

			ItemID.Sets.SortingPriorityBossSpawns[Item.type] = 12;
		}

		public override void SetDefaults()
		{
			Item.width = 30;
			Item.height = 28;
			Item.maxStack = 1;
			Item.rare = 1;
			Item.useAnimation = 30;
			Item.useTime = 30;
			Item.useStyle = 4;
			Item.consumable = false;
		}

		public override bool CanUseItem(Player player)
		{
			return !NPC.AnyNPCs(NPCType<SunPixie>()) && PolaritiesSystem.sunPixieSpawnTimer == 0 && !NPC.AnyNPCs(NPCType<Eclipxie>());
		}

        public override void UseAnimation(Player player)
        {
            base.UseAnimation(player);
        }

        public override bool? UseItem(Player player)
		{
			if (!NPC.AnyNPCs(NPCType<Eclipxie>()) && Main.eclipse)
            {
                Eclipxie.SpawnOn(player);
            }
			else
			{
				SunPixie.SpawnOn(player);
			}
			return true;
		}

		public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
		{
			Texture2D texture = TextureAssets.Item[Type].Value;
			spriteBatch.Draw
			(
				texture,
				new Vector2
				(
					Item.position.X - Main.screenPosition.X + Item.width * 0.5f,
					Item.position.Y - Main.screenPosition.Y + Item.height - texture.Height * 0.5f + 2f
				),
				new Rectangle(0, 0, texture.Width, texture.Height),
				Color.White,
				rotation,
				texture.Size() * 0.5f,
				scale,
				SpriteEffects.None,
				0f
			);
		}

		public override void ModifyTooltips(System.Collections.Generic.List<TooltipLine> tooltips)
		{
			if (Main.eclipse)
			{
				TooltipLine line = new TooltipLine(Mod, "Tooltip1", Language.GetTextValue("Mods.Polarities.ItemTooltip.SunPixieSummonItemExtra"));
				tooltips.Add(line);
			}
		}
	}
}