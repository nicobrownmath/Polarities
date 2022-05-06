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
    public class MoonLordBook : BookBase
    {
        public override int BuffType => BuffType<MoonLordBookBuff>();
    }

    public class MoonLordBookBuff : BookBuffBase
    {
        public override int ItemType => ItemType<MoonLordBook>();

        public override void Update(Player player, ref int buffIndex)
        {
            base.Update(player, ref buffIndex);
        }
    }
}