using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using Polarities.Buffs;

namespace Polarities.Items.Books
{
    public class BrainOfCthulhuBook : BookBase
    {
        public override int BuffType => BuffType<BrainOfCthulhuBookBuff>();
    }

    public class BrainOfCthulhuBookBuff : BookBuffBase
    {
        public override int ItemType => ItemType<BrainOfCthulhuBook>();

        public override void Update(Player player, ref int buffIndex)
        {
            player.tileSpeed += 0.5f;
            player.wallSpeed += 0.5f;

            base.Update(player, ref buffIndex);
        }
    }
}