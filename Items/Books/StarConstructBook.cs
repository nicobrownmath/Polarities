using Terraria;
using static Terraria.ModLoader.ModContent;

namespace Polarities.Items.Books
{
    public class StarConstructBook : BookBase
    {
        public override int BuffType => BuffType<StarConstructBookBuff>();
        public override int BookIndex => 7;
    }

    public class StarConstructBookBuff : BookBuffBase
    {
        public override int ItemType => ItemType<StarConstructBook>();

        public override void Update(Player player, ref int buffIndex)
        {
            if (player.grapCount > 0)
            {
                player.statDefense += 16;
            }

            base.Update(player, ref buffIndex);
        }
    }
}