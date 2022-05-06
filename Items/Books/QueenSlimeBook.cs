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
    public class QueenSlimeBook : BookBase
    {
        public override int BuffType => BuffType<QueenSlimeBookBuff>();
    }

    public class QueenSlimeBookBuff : BookBuffBase
    {
        public override int ItemType => ItemType<QueenSlimeBook>();

        public override void Update(Player player, ref int buffIndex)
        {
            player.GetModPlayer<PolaritiesPlayer>().wingTimeBoost += 30;

            base.Update(player, ref buffIndex);
        }
    }
}