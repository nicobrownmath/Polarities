using Terraria;
using static Terraria.ModLoader.ModContent;

namespace Polarities.Items.Books
{
    public class MoonLordBook : BookBase
    {
        public override int BuffType => BuffType<MoonLordBookBuff>();
        public override int BookIndex => 31;
    }

    public class MoonLordBookBuff : BookBuffBase
    {
        public override int ItemType => ItemType<MoonLordBook>();

        public override void Update(Player player, ref int buffIndex)
        {
            base.Update(player, ref buffIndex);
        }
    }
}