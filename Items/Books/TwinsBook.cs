using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using Polarities.Buffs;

namespace Polarities.Items.Books
{
    public class TwinsBook : BookBase
    {
        public override int BuffType => BuffType<TwinsBookBuff>();
        public override int BookIndex => 16;
    }

    public class TwinsBookBuff : BookBuffBase
    {
        public override int ItemType => ItemType<TwinsBook>();

        public override void Update(Player player, ref int buffIndex)
        {
            player.scope = true;

            base.Update(player, ref buffIndex);
        }
    }
}