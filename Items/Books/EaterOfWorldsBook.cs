using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using Polarities.Buffs;

namespace Polarities.Items.Books
{
    public class EaterOfWorldsBook : BookBase
    {
        public override int BuffType => BuffType<EaterOfWorldsBookBuff>();
    }

    public class EaterOfWorldsBookBuff : BookBuffBase
    {
        public override int ItemType => ItemType<EaterOfWorldsBook>();

        public override void Update(Player player, ref int buffIndex)
        {
            player.pickSpeed *= 0.75f;
            if (player.HeldItem.axe > 0 || player.HeldItem.hammer > 0 || player.HeldItem.pick > 0)
            {
                player.GetDamage(DamageClass.Melee) += 0.5f;
            }

            base.Update(player, ref buffIndex);
        }
    }
}