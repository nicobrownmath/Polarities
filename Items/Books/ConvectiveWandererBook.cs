using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using Polarities.Buffs;

namespace Polarities.Items.Books
{
    public class ConvectiveWandererBook : BookBase
    {
        public override int BuffType => BuffType<ConvectiveWandererBookBuff>();
        public override int BookIndex => 21;
    }

    public class ConvectiveWandererBookBuff : BookBuffBase
    {
        public override int ItemType => ItemType<ConvectiveWandererBook>();

        public override void Update(Player player, ref int buffIndex)
        {
            player.lavaImmune = true;

            base.Update(player, ref buffIndex);
        }
    }
}