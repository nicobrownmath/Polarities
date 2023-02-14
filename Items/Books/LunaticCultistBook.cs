using Terraria;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Polarities.Items.Books
{
    public class LunaticCultistBook : BookBase
    {
        public override int BuffType => BuffType<LunaticCultistBookBuff>();
        public override int BookIndex => 29;
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