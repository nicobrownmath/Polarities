using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Polarities.Items.Weapons.Melee.Warhammers
{
    public class Warhammerhead : WarhammerBase
    {
        public override int HammerLength => 61;
        public override int HammerHeadSize => 21;
        public override int DefenseLoss => 16;
        public override int DebuffTime => 1200;

        public override void SetDefaults()
        {
            Item.SetWeaponValues(48, 12, 0);
            Item.DamageType = DamageClass.Melee;

            Item.width = 82;
            Item.height = 82;

            Item.useTime = 40;
            Item.useAnimation = 40;
            Item.useStyle = WarhammerUseStyle;

            Item.value = Item.sellPrice(gold: 1, silver: 50);
            Item.rare = ItemRarityID.Orange;
        }

        public override void ModifyItemPosition(Player player)
        {
            player.itemLocation -= new Vector2(player.direction * 4, -player.gravDir * 4).RotatedBy(player.itemRotation);
        }
    }
}

