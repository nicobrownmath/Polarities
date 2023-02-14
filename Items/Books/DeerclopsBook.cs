using Terraria;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace Polarities.Items.Books
{
    public class DeerclopsBook : BookBase
    {
        public override int BuffType => BuffType<DeerclopsBookBuff>();
        public override int BookIndex => 11;
    }

    public class DeerclopsBookBuff : BookBuffBase
    {
        public override int ItemType => ItemType<DeerclopsBook>();

        public override void Update(Player player, ref int buffIndex)
        {
            if (player.HasBuff(BuffID.WellFed))
            {
                player.buffTime[player.FindBuffIndex(BuffID.WellFed)]++;
            }
            if (player.HasBuff(BuffID.WellFed2))
            {
                player.buffTime[player.FindBuffIndex(BuffID.WellFed2)]++;
            }
            if (player.HasBuff(BuffID.WellFed3))
            {
                player.buffTime[player.FindBuffIndex(BuffID.WellFed3)]++;
            }

            base.Update(player, ref buffIndex);
        }
    }
}