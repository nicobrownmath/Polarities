using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Polarities.Items.Weapons.Ranged.Atlatls
{
    public class ThistleThrower : AtlatlBase
    {
        public override Vector2[] ShotDistances => new Vector2[] { new Vector2(30) };
        public override float SpriteRotationOffset => -0.1f;

        public override void SetDefaults()
        {
            Item.SetWeaponValues(21, 3, 0);
            Item.DamageType = DamageClass.Ranged;

            Item.width = 32;
            Item.height = 42;

            Item.useTime = 24;
            Item.useAnimation = 24;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.noMelee = true;
            Item.UseSound = SoundID.Item1;

            Item.shoot = 10;
            Item.shootSpeed = 16f;
            Item.useAmmo = AmmoID.Dart;

            Item.value = Item.sellPrice(gold: 1);
            Item.rare = ItemRarityID.Orange;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.RichMahogany, 8)
                .AddIngredient(ItemID.JungleSpores, 12)
                .AddIngredient(ItemID.Vine)
                .AddTile(TileID.WorkBenches)
                .Register();
        }
    }
}
