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
    public class LunaticCultistBook : BookBase
    {
        public override int BuffType => BuffType<LunaticCultistBookBuff>();
    }

    public class LunaticCultistBookBuff : BookBuffBase
    {
        public override int ItemType => ItemType<LunaticCultistBook>();

        public override void Update(Player player, ref int buffIndex)
        {
            if (player.ZoneTowerNebula)
            {
                player.GetDamage(DamageClass.Magic) += 0.30f;
            }
            if (player.ZoneTowerSolar)
            {
                player.GetDamage(DamageClass.Melee) += 0.30f;
            }
            if (player.ZoneTowerStardust)
            {
                player.GetDamage(DamageClass.Summon) += 0.30f;
            }
            if (player.ZoneTowerVortex)
            {
                player.GetDamage(DamageClass.Ranged) += 0.30f;
            }

            base.Update(player, ref buffIndex);
        }
    }
}