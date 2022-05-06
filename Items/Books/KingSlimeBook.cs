using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using Polarities.Buffs;

namespace Polarities.Items.Books
{
    public class KingSlimeBook : BookBase
    {
        public override int BuffType => BuffType<KingSlimeBookBuff>();
    }

    public class KingSlimeBookBuff : BookBuffBase
    {
        public override int ItemType => ItemType<KingSlimeBook>();
    }
}