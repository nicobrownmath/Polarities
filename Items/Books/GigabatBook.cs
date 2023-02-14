using Terraria;
using static Terraria.ModLoader.ModContent;

namespace Polarities.Items.Books
{
    public class GigabatBook : BookBase
    {
        public override int BuffType => BuffType<GigabatBookBuff>();
        public override int BookIndex => 8;
    }

    public class GigabatBookBuff : BookBuffBase
    {
        public override int ItemType => ItemType<GigabatBook>();

        public override void Update(Player player, ref int buffIndex)
        {
            player.noFallDmg = true;
            player.GetModPlayer<PolaritiesPlayer>().hasGlide = true;

            base.Update(player, ref buffIndex);
        }
    }
}