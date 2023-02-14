using Terraria;
using static Terraria.ModLoader.ModContent;

namespace Polarities.Items.Books
{
    public class BetsyBook : BookBase
    {
        public override int BuffType => BuffType<BetsyBookBuff>();
        public override int BookIndex => 25;
    }

    public class BetsyBookBuff : BookBuffBase
    {
        public override int ItemType => ItemType<BetsyBook>();

        public override void Update(Player player, ref int buffIndex)
        {
            player.maxTurrets += 2;

            base.Update(player, ref buffIndex);
        }
    }
}