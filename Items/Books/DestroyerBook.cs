using Terraria;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Polarities.Items.Books
{
    public class DestroyerBook : BookBase
    {
        public override int BuffType => BuffType<DestroyerBookBuff>();
        public override int BookIndex => 15;
    }

    public class DestroyerBookBuff : BookBuffBase
    {
        public override int ItemType => ItemType<DestroyerBook>();

        public override void Update(Player player, ref int buffIndex)
        {
            if (player.HeldItem.axe > 0 || player.HeldItem.hammer > 0 || player.HeldItem.pick > 0)
            {
                player.GetDamage(DamageClass.Melee) += 0.5f;
            }
            player.pickSpeed *= 0.833f;
            player.tileSpeed += 1;
            player.wallSpeed += 1;
            Player.tileRangeX += 5;
            Player.tileRangeY += 5;

            base.Update(player, ref buffIndex);
        }
    }
}