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
    public class DukeFishronBook : BookBase
    {
        public override int BuffType => BuffType<DukeFishronBookBuff>();
    }

    public class DukeFishronBookBuff : BookBuffBase
    {
        public override int ItemType => ItemType<DukeFishronBook>();

        public override void Update(Player player, ref int buffIndex)
        {
            if (player.ZoneBeach)
            {
                player.GetDamage(DamageClass.Generic) += 0.20f;
            }

            base.Update(player, ref buffIndex);
        }
    }
}