using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Polarities.Items.Weapons.Ranged.Atlatls
{
    public class ShadewoodAtlatl : AtlatlBase
    {
        public override Vector2[] ShotDistances => new Vector2[] { new Vector2(30) };

        public override void SetDefaults()
        {
            Item.SetWeaponValues(11, 3, 0);
            Item.DamageType = DamageClass.Ranged;

            Item.width = 38;
            Item.height = 32;

            Item.useTime = 26;
            Item.useAnimation = 26;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.noMelee = true;
            Item.UseSound = SoundID.Item1;

            Item.shoot = 10;
            Item.shootSpeed = 16f;
            Item.useAmmo = AmmoID.Dart;

            Item.value = Item.sellPrice(copper: 20);
            Item.rare = ItemRarityID.White;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.Shadewood, 12)
                .AddTile(TileID.WorkBenches)
                .Register();
        }
    }
}