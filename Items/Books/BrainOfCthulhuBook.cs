using Terraria;
using static Terraria.ModLoader.ModContent;

namespace Polarities.Items.Books
{
    public class BrainOfCthulhuBook : BookBase
    {
        public override int BuffType => BuffType<BrainOfCthulhuBookBuff>();
        public override int BookIndex => 6;
    }

    public class BrainOfCthulhuBookBuff : BookBuffBase
    {
        public override int ItemType => ItemType<BrainOfCthulhuBook>();

        public override void Update(Player player, ref int buffIndex)
        {
            player.tileSpeed += 0.5f;
            player.wallSpeed += 0.5f;

            base.Update(player, ref buffIndex);
        }
    }
}