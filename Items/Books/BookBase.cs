using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using Polarities.Buffs;

namespace Polarities.Items.Books
{
    public abstract class BookBase : ModItem
    {
        public abstract int BuffType { get; }

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
            if (player.HasBuff(BuffType))
            {
                player.ClearBuff(BuffType);
            }
            else
            {
                if (player.GetModPlayer<PolaritiesPlayer>().usedBookSlots + 1 > player.GetModPlayer<PolaritiesPlayer>().maxBookSlots)
                {
                    for (int i = 0; i < Player.MaxBuffs; i++)
                    {
                        if (player.buffTime[i] > 0 && GetModBuff(player.buffType[i]) is BookBuffBase bookBuff)
                        {
                            player.ClearBuff(player.buffType[i]);
                            player.GetModPlayer<PolaritiesPlayer>().usedBookSlots--;
                            if (player.GetModPlayer<PolaritiesPlayer>().usedBookSlots + 1 <= player.GetModPlayer<PolaritiesPlayer>().maxBookSlots)
                                break;
                        }
                    }
                }
                player.AddBuff(BuffType, 2);
            }
            return true;
        }
    }

    public abstract class BookBuffBase : ModBuff
    {
        public override string Texture => "Polarities/Items/Books/BookBuff";

        public abstract int ItemType { get; }

        public override void SetStaticDefaults()
        {
            string itemName = ItemLoader.GetItem(ItemType).Name;
            DisplayName.SetDefault("{$Mods.Polarities.ItemName." + itemName +"}");

            Main.buffNoTimeDisplay[Type] = true;
            Main.persistentBuff[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            if (player.GetModPlayer<PolaritiesPlayer>().usedBookSlots + 1 <= player.GetModPlayer<PolaritiesPlayer>().maxBookSlots)
            {
                player.GetModPlayer<PolaritiesPlayer>().usedBookSlots++;
                player.buffTime[buffIndex] = 2;
            }
        }
    }
}