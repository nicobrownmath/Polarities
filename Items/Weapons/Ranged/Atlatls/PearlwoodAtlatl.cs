using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Polarities.Items.Weapons.Ranged.Atlatls
{
    public class PearlwoodAtlatl : AtlatlBase
    {
        public override Vector2[] ShotDistances => new Vector2[] { new Vector2(30) };

        public override void SetDefaults()
        {
            Item.SetWeaponValues(12, 3, 0);
            Item.DamageType = DamageClass.Ranged;

            Item.width = 34;
            Item.height = 38;

            Item.useTime = 24;
            Item.useAnimation = 24;
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
                .AddIngredient(ItemID.Pearlwood, 12)
                .AddTile(TileID.WorkBenches)
                .Register();
        }
    }
}