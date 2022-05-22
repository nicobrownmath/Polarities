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
    public class GolemBook : BookBase
    {
        public override int BuffType => BuffType<GolemBookBuff>();
        public override int BookIndex => 23;
    }

    public class GolemBookBuff : BookBuffBase
    {
        public override int ItemType => ItemType<GolemBook>();

        public override void Update(Player player, ref int buffIndex)
        {
            base.Update(player, ref buffIndex);
        }
    }
}