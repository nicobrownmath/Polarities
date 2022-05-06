using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Polarities.Items
{
	public class PickledHerring : ModItem
	{
		public override void SetStaticDefaults()
		{
			this.SetResearch(2);
		}

		public override void SetDefaults()
		{
			Item.questItem = true;
			Item.maxStack = 1;
			Item.width = 34;
			Item.height = 34;
			Item.uniqueStack = true;
			Item.rare = ItemRarityID.Quest;
		}

		public override bool IsQuestFish()
		{
			return true;
		}

		public override bool IsAnglerQuestAvailable()
		{
			return true;
		}

		public override void AnglerQuestChat(ref string description, ref string catchLocation)
		{
			description = Language.GetTextValue("Mods.Polarities.AnglerQuest." + Name + "_Description");
			catchLocation = Language.GetTextValue("Mods.Polarities.AnglerQuest." + Name + "_Location");
		}
	}
}
