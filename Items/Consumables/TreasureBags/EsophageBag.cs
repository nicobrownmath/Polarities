using Polarities.Items.Weapons;
using Polarities.Items.Armor;
using Polarities.Items.Placeable;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using Terraria.DataStructures;
using Polarities.Items.Accessories;
using Polarities.Items.Hooks;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Terraria.GameContent;
using System;
using Polarities.Items.Armor.Vanity;
using Polarities.Items.Weapons.Ranged;
using Polarities.Items.Weapons.Melee;
using Polarities.Items.Weapons.Magic;
using Polarities.Items.Weapons.Summon.Minions;
using Polarities.Items.Materials;
using Terraria.GameContent.ItemDropRules;
using Polarities.NPCs.Esophage;
using Polarities.NPCs;

namespace Polarities.Items.Consumables.TreasureBags
{
	public class EsophageBag : ModItem
	{
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("{$Mods.Polarities.ItemName.TreasureBag} ({$Mods.Polarities.NPCName.Esophage})");
			Tooltip.SetDefault("{$CommonItemTooltip.RightClickToOpen}");

			ItemID.Sets.BossBag[Type] = true;

            this.SetResearch(3);
		}

		public override void SetDefaults()
		{
			Item.maxStack = 999;
			Item.consumable = true;
			Item.width = 24;
			Item.height = 24;
			Item.rare = ItemRarityID.Lime;
			Item.expert = true;
		}

		public override bool CanRightClick()
		{
			return true;
		}

        public override void ModifyItemLoot(ItemLoot itemLoot)
        {
            itemLoot.Add(ItemDropRule.NotScalingWithLuck(ItemType<AdaptiveGenes>(), 1));
            itemLoot.Add(ItemDropRule.NotScalingWithLuck(ItemType<EsophageMask>(), 7));
            itemLoot.Add(ItemDropRule.NotScalingWithLuck(ItemID.SoulofNight, 1, 14, 21));
            itemLoot.Add(ItemDropRule.NotScalingWithLuck(ItemType<EvilDNA>(), 1, 3, 5));
            itemLoot.Add(ItemDropRule.NotScalingWithLuck(ItemType<StrangeSamples>(), 2));
			itemLoot.Add(new OneFromOptionsWithCountsNotScaledWithLuckDropRule(1, 1,
				(ItemID.Ichor, 20, 40),
				(ItemID.CursedFlame, 20, 40)));
			itemLoot.Add(ItemDropRule.OneFromOptionsNotScalingWithLuck(1, ItemType<EsophageousStaff>(), ItemType<PhagefootHook>()));
            itemLoot.Add(ItemDropRule.CoinsBasedOnNPCValue(NPCType<Esophage>()));
        }

		public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
		{
			Texture2D texture = TextureAssets.Item[Type].Value;
			Rectangle frame = texture.Frame();

			Vector2 vector = frame.Size() / 2f;
			Vector2 value = new Vector2((float)(Item.width / 2) - vector.X, (float)(Item.height - frame.Height));
			Vector2 vector2 = Item.position - Main.screenPosition + vector + value;

			float num = Item.velocity.X * 0.2f;

			float num7 = (float)Item.timeSinceItemSpawned / 240f + Main.GlobalTimeWrappedHourly * 0.04f;
			float globalTimeWrappedHourly = Main.GlobalTimeWrappedHourly;
			globalTimeWrappedHourly %= 4f;
			globalTimeWrappedHourly /= 2f;
			if (globalTimeWrappedHourly >= 1f)
			{
				globalTimeWrappedHourly = 2f - globalTimeWrappedHourly;
			}
			globalTimeWrappedHourly = globalTimeWrappedHourly * 0.5f + 0.5f;
			for (float num8 = 0f; num8 < 1f; num8 += 0.25f)
			{
				spriteBatch.Draw(texture, vector2 + Utils.RotatedBy(new Vector2(0f, 8f), (num8 + num7) * ((float)Math.PI * 2f)) * globalTimeWrappedHourly, frame, new Color(90, 70, 255, 50), num, vector, scale, (SpriteEffects)0, 0f);
			}
			for (float num9 = 0f; num9 < 1f; num9 += 0.34f)
			{
				spriteBatch.Draw(texture, vector2 + Utils.RotatedBy(new Vector2(0f, 4f), (num9 + num7) * ((float)Math.PI * 2f)) * globalTimeWrappedHourly, frame, new Color(140, 120, 255, 77), num, vector, scale, (SpriteEffects)0, 0f);
			}
			return true;
		}

		public override void Update(ref float gravity, ref float maxFallSpeed)
		{
			Lighting.AddLight((int)((Item.position.X + (float)Item.width) / 16f), (int)((Item.position.Y + (float)(Item.height / 2)) / 16f), 0.4f, 0.4f, 0.4f);
			if (Item.timeSinceItemSpawned % 12 == 0)
			{
				Dust dust2 = Dust.NewDustPerfect(Item.Center + new Vector2(0f, (float)Item.height * -0.1f) + Main.rand.NextVector2CircularEdge((float)Item.width * 0.6f, (float)Item.height * 0.6f) * (0.3f + Main.rand.NextFloat() * 0.5f), 279, (Vector2?)new Vector2(0f, (0f - Main.rand.NextFloat()) * 0.3f - 1.5f), 127, default(Color), 1f);
				dust2.scale = 0.5f;
				dust2.fadeIn = 1.1f;
				dust2.noGravity = true;
				dust2.noLight = true;
				dust2.alpha = 0;
			}
		}

		public override Color? GetAlpha(Color lightColor)
		{
			return Color.Lerp(lightColor, Color.White, 0.4f);
		}
	}
}

