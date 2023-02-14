using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Polarities.Items.Weapons.Melee.Warhammers
{
    public class EbonwoodWarhammer : WarhammerBase
    {
        public override int HammerLength => 45;
        public override int HammerHeadSize => 13;
        public override int DefenseLoss => 2;
        public override int DebuffTime => 300;

        public override void SetDefaults()
        {
            Item.SetWeaponValues(16, 9, 0);
            Item.DamageType = DamageClass.Melee;

            Item.width = 58;
            Item.height = 58;

            Item.useTime = 40;
            Item.useAnimation = 40;
            Item.useStyle = WarhammerUseStyle;

            Item.value = Item.sellPrice(copper: 25);
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.Ebonwood, 10)
                .AddTile(TileID.WorkBenches)
                .Register();
        }
    }
}

