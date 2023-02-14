using Terraria;
using static Terraria.ModLoader.ModContent;

namespace Polarities.Items.Books
{
    public class QueenSlimeBook : BookBase
    {
        public override int BuffType => BuffType<QueenSlimeBookBuff>();
        public override int BookIndex => 14;
    }

    public class QueenSlimeBookBuff : BookBuffBase
    {
        public override int ItemType => ItemType<QueenSlimeBook>();

        public override void Update(Player player, ref int buffIndex)
        {
            player.GetModPlayer<PolaritiesPlayer>().wingTimeBoost += 30;

            base.Update(player, ref buffIndex);
        }
    }
}