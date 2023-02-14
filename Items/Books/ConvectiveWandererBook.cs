using Terraria;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

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
            player.GetModPlayer<PolaritiesPlayer>().incinerationResistanceTime = 600;
            player.lavaImmune = true;
            player.fireWalk = true;
            player.buffImmune[BuffID.OnFire] = true;

            base.Update(player, ref buffIndex);
        }
    }
}