using Terraria;
using static Terraria.ModLoader.ModContent;

namespace Polarities.Items.Books
{
    public class EsophageBook : BookBase
    {
        public override int BuffType => BuffType<EsophageBookBuff>();
        public override int BookIndex => 19;
    }

    public class EsophageBookBuff : BookBuffBase
    {
        public override int ItemType => ItemType<EsophageBook>();

        public override void Update(Player player, ref int buffIndex)
        {
            //buff player speed and damage if on the ground
            if (player.velocity.Y == 0)
            {
                player.GetModPlayer<PolaritiesPlayer>().runSpeedBoost += 0.16f;
                player.statDefense += 16;
            }

            base.Update(player, ref buffIndex);
        }
    }
}