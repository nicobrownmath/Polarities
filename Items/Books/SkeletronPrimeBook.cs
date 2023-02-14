using Terraria;
using static Terraria.ModLoader.ModContent;

namespace Polarities.Items.Books
{
    public class SkeletronPrimeBook : BookBase
    {
        public override int BuffType => BuffType<SkeletronPrimeBookBuff>();
        public override int BookIndex => 17;
    }

    public class SkeletronPrimeBookBuff : BookBuffBase
    {
        public override int ItemType => ItemType<SkeletronPrimeBook>();

        public override void Update(Player player, ref int buffIndex)
        {
            player.GetModPlayer<PolaritiesPlayer>().skeletronBook = true;
            if (player.GetModPlayer<PolaritiesPlayer>().skeletronBookCooldown == 0)
            {
                player.statDefense += 9999;
            }

            base.Update(player, ref buffIndex);
        }
    }
}