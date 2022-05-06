using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using Polarities.Buffs;

namespace Polarities.Items.Books
{
    public class StrangeObituary : ModItem
    {
        public override void SetStaticDefaults()
        {
            this.SetResearch(1);
        }

        public override void SetDefaults()
        {
            Item.width = 26;
            Item.height = 32;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.useAnimation = 15;
            Item.useTime = 15;
            Item.useTurn = true;
            Item.UseSound = SoundID.Item8;
            Item.rare = ItemRarityID.Blue;
            Item.value = Item.buyPrice(gold: 5);
        }

        public override bool? UseItem(Player player)
        {
            player.AddBuff(BuffType<StrangeObituaryBuff>(), 2);
            return true;
        }
    }

    public class StrangeObituaryBuff : ModBuff
    {
        public override string Texture => "Polarities/Items/Books/BookBuff";

        public override void SetStaticDefaults()
        {
            Main.buffNoTimeDisplay[Type] = true;
            Main.persistentBuff[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            player.GetModPlayer<PolaritiesPlayer>().strangeObituary = true;
            player.buffTime[buffIndex] = 2;
        }
    }
}