using Polarities.Dusts;
using Polarities.Items.Placeable.Blocks.Fractal;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Polarities.Items.Placeable.Furniture.Fractal
{
    public class FractalDoor : ModItem
    {
        public override void SetStaticDefaults()
        {
            SacrificeTotal = 1;
        }

        public override void SetDefaults()
        {
            Item.CloneDefaults(ItemID.GrandfatherClock);
            Item.createTile = ModContent.TileType<FractalDoorClosed>();
            Item.placeStyle = 0;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<FractalBrick>(), 6)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }

    public class FractalDoorClosed : DoorClosedBase
    {
        public override int MyDustType => ModContent.DustType<FractalMatterDust>();
        public override int DropItem => ModContent.ItemType<FractalClock>();
        public override int OpenVersion => ModContent.TileType<FractalDoorOpen>();
    }

    public class FractalDoorOpen : DoorOpenBase
    {
        public override int MyDustType => ModContent.DustType<FractalMatterDust>();
        public override int DropItem => ModContent.ItemType<FractalClock>();
        public override int ClosedVersion => ModContent.TileType<FractalDoorClosed>();
    }
}