using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Polarities.Items.Weapons.Melee.Warhammers
{
    public class DemoniteWarhammer : WarhammerBase
    {
        public override int HammerLength => 46;
        public override int HammerHeadSize => 12;
        public override int DefenseLoss => 12;
        public override int DebuffTime => 600;

        public override void SetDefaults()
        {
            Item.SetWeaponValues(20, 13, 0);
            Item.DamageType = DamageClass.Melee;

            Item.width = 58;
            Item.height = 58;

            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.useStyle = WarhammerUseStyle;

            Item.value = Item.sellPrice(silver: 30);
            Item.rare = ItemRarityID.Blue;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.DemoniteBar, 15)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }
}