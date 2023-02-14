using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Polarities.Items.Weapons.Ranged.Atlatls
{
    public class Ehecatl : AtlatlBase
    {
        public override Vector2[] ShotDistances => new Vector2[] { new Vector2(36), new Vector2(42), new Vector2(48) };

        public override void SetDefaults()
        {
            Item.SetWeaponValues(20, 3, 4);
            Item.DamageType = DamageClass.Ranged;

            Item.width = 56;
            Item.height = 56;

            Item.useTime = 24;
            Item.useAnimation = 24;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.noMelee = true;
            Item.autoReuse = true;

            Item.shoot = 10;
            Item.shootSpeed = 17f;
            Item.useAmmo = AmmoID.Dart;

            Item.value = Item.sellPrice(gold: 4);
            Item.rare = ItemRarityID.Pink;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.HallowedBar, 12)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
}
