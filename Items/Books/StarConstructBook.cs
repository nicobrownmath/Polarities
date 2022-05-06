using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using Polarities.Buffs;

namespace Polarities.Items.Books
{
    public class StarConstructBook : BookBase
    {
        public override int BuffType => BuffType<StarConstructBookBuff>();
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