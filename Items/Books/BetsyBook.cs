using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using Polarities.Buffs;
using Microsoft.Xna.Framework;
using Terraria.GameContent;
using Microsoft.Xna.Framework.Graphics;

namespace Polarities.Items.Books
{
    public class BetsyBook : BookBase
    {
        public override int BuffType => BuffType<BetsyBookBuff>();
    }

    public class BetsyBookBuff : BookBuffBase
    {
        public override int ItemType => ItemType<BetsyBook>();

        public override void Update(Player player, ref int buffIndex)
        {
            player.maxTurrets += 2;

            base.Update(player, ref buffIndex);
        }
    }
}