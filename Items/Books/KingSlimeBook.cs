using static Terraria.ModLoader.ModContent;

namespace Polarities.Items.Books
{
    public class KingSlimeBook : BookBase
    {
        public override int BuffType => BuffType<KingSlimeBookBuff>();
        public override int BookIndex => 2;
    }

    public class KingSlimeBookBuff : BookBuffBase
    {
        public override int ItemType => ItemType<KingSlimeBook>();
    }
}