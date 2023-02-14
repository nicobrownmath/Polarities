using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Polarities.Items.Weapons.Melee.Warhammers
{
    public class LeadWarhammer : WarhammerBase
    {
        public override int HammerLength => 53;
        public override int HammerHeadSize => 13;
        public override int DefenseLoss => 4;
        public override int DebuffTime => 480;

        public override void SetDefaults()
        {
            Item.SetWeaponValues(17, 10, 0);
            Item.DamageType = DamageClass.Melee;

            Item.width = 66;
            Item.height = 66;

            Item.useTime = 40;
            Item.useAnimation = 40;
            Item.useStyle = WarhammerUseStyle;

            Item.value = Item.sellPrice(silver: 10);
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.LeadBar, 12)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }
}