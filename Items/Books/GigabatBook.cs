using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using Polarities.Buffs;

namespace Polarities.Items.Books
{
    public class GigabatBook : BookBase
    {
        public override int BuffType => BuffType<GigabatBookBuff>();
    }

    public class GigabatBookBuff : BookBuffBase
    {
        public override int ItemType => ItemType<GigabatBook>();

        public override void Update(Player player, ref int buffIndex)
        {
            player.noFallDmg = true;
            player.GetModPlayer<PolaritiesPlayer>().hasGlide = true;

            base.Update(player, ref buffIndex);
        }
    }
}