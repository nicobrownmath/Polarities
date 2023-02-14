using Terraria;
using static Terraria.ModLoader.ModContent;

namespace Polarities.Items.Books
{
    public class WallOfFleshBook : BookBase
    {
        public override int BuffType => BuffType<WallOfFleshBookBuff>();
        public override int BookIndex => 13;
    }

    public class WallOfFleshBookBuff : BookBuffBase
    {
        public override int ItemType => ItemType<WallOfFleshBook>();

        public override void Update(Player player, ref int buffIndex)
        {
            player.statLifeMax2 = (int)(1.10f * player.statLifeMax2);

            base.Update(player, ref buffIndex);
        }
    }
}