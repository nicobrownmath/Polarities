using Microsoft.Xna.Framework;
using Polarities.Biomes;
using Polarities.Dusts;
using Polarities.Items.Placeable.Blocks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Polarities.Items.Placeable.Furniture.Limestone
{
    public class LimestoneToilet : ModItem
    {
        public override void SetStaticDefaults()
        {
            SacrificeTotal = 1;
        }

        public override void SetDefaults()
        {
            Item.CloneDefaults(ItemID.WoodenChair);
            Item.createTile = ModContent.TileType<LimestoneToiletTile>();
            Item.placeStyle = 0;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<LimestoneBrick>(), 6)
                .AddTile(TileID.Sawmill)
                .Register();
        }
    }
    public class LimestoneToiletTile : ToiletTileBase
    {
        public override int MyDustType => DustType<LimestoneDust>();
        public override int DropItem => ItemType<LimestoneToilet>();
    }

    public class LimestonePlatform : PlatformBase
    {
        public override int PlaceTile => TileType<LimestonePlatformTile>();

        public override void AddRecipes()
        {
            CreateRecipe(2)
                .AddIngredient(ItemType<Blocks.LimestoneBrick>())
                .Register();
        }
    }
    public class LimestonePlatformTile : PlatformTileBase
    {
        public override Color MapColor => new Color(200, 200, 200);
        public override int MyDustType => DustType<LimestoneDust>();
        public override int DropItem => ItemType<LimestonePlatform>();
    }

    public class LimestoneBathtub : BathtubBase
    {
        public override int PlaceTile => TileType<LimestoneBathtubTile>();

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemType<Blocks.LimestoneBrick>(), 14)
                .AddTile(TileID.Sawmill)
                .Register();
        }
    }
    public class LimestoneBathtubTile : BathtubTileBase
    {
        public override int MyDustType => DustType<LimestoneDust>();
        public override int DropItem => ItemType<LimestoneBathtub>();
    }

    public class LimestoneBed : BedBase
    {
        public override int PlaceTile => TileType<LimestoneBedTile>();

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemType<Blocks.LimestoneBrick>(), 15)
                .AddIngredient(ItemID.Silk, 5)
                .AddTile(TileID.Sawmill)
                .Register();
        }
    }
    public class LimestoneBedTile : BedTileBase
    {
        public override int MyDustType => DustType<LimestoneDust>();
        public override int DropItem => ItemType<LimestoneBed>();
    }

    public class LimestoneBookcase : BookcaseBase
    {
        public override int PlaceTile => TileType<LimestoneBookcaseTile>();

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemType<Blocks.LimestoneBrick>(), 20)
                .AddIngredient(ItemID.Book, 10)
                .AddTile(TileID.Sawmill)
                .Register();
        }
    }
    public class LimestoneBookcaseTile : BookcaseTileBase
    {
        public override int MyDustType => DustType<LimestoneDust>();
        public override int DropItem => ItemType<LimestoneBookcase>();
    }

    public class LimestoneCandelabra : CandelabraBase
    {
        public override int PlaceTile => TileType<LimestoneCandelabraTile>();

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemType<Blocks.LimestoneBrick>(), 5)
                .AddIngredient(ItemID.Torch, 3)
                .AddTile(TileID.WorkBenches)
                .Register();
        }
    }
    public class LimestoneCandelabraTile : CandelabraTileBase
    {
        public override int MyDustType => DustType<LimestoneDust>();
        public override int DropItem => ItemType<LimestoneCandelabra>();
        public override bool DieInWater => false;
    }

    public class LimestoneCandle : CandleBase
    {
        public override int PlaceTile => TileType<LimestoneCandleTile>();

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemType<Blocks.LimestoneBrick>(), 4)
                .AddIngredient(ItemID.Torch)
                .AddTile(TileID.WorkBenches)
                .Register();
        }
    }
    public class LimestoneCandleTile : CandleTileBase
    {
        public override int MyDustType => DustType<LimestoneDust>();
        public override int DropItem => ItemType<LimestoneCandle>();
        public override bool DieInWater => false;
    }

    public class LimestoneChair : ChairBase
    {
        public override int PlaceTile => TileType<LimestoneChairTile>();

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemType<Blocks.LimestoneBrick>(), 4)
                .AddTile(TileID.WorkBenches)
                .Register();
        }
    }
    public class LimestoneChairTile : ChairTileBase
    {
        public override int MyDustType => DustType<LimestoneDust>();
        public override int DropItem => ItemType<LimestoneChair>();
    }

    public class LimestoneChandelier : ChandelierBase
    {
        public override int PlaceTile => TileType<LimestoneChandelierTile>();

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemType<Blocks.LimestoneBrick>(), 4)
                .AddIngredient(ItemID.Torch, 4)
                .AddIngredient(ItemID.Chain, 1)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }
    public class LimestoneChandelierTile : ChandelierTileBase
    {
        public override int MyDustType => DustType<LimestoneDust>();
        public override int DropItem => ItemType<LimestoneChandelier>();
        public override bool DieInWater => false;
    }

    public class LimestoneChest : ChestBase
    {
        public override int PlaceTile => TileType<LimestoneChestTile>();

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemType<Blocks.LimestoneBrick>(), 8)
                .AddRecipeGroup(RecipeGroupID.IronBar, 2)
                .AddTile(TileID.WorkBenches)
                .Register();
        }
    }
    public class LimestoneChestTile : ChestTileBase
    {
        public override int MyDustType => DustType<LimestoneDust>();
        public override int DropItem => ItemType<LimestoneChest>();
    }

    public class LimestoneClock : ClockBase
    {
        public override int PlaceTile => TileType<LimestoneClockTile>();

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemType<Blocks.LimestoneBrick>(), 10)
                .AddIngredient(ItemID.Glass, 6)
                .AddRecipeGroup(RecipeGroupID.IronBar, 3)
                .AddTile(TileID.Sawmill)
                .Register();
        }
    }
    public class LimestoneClockTile : ClockTileBase
    {
        public override int MyDustType => DustType<LimestoneDust>();
        public override int DropItem => ItemType<LimestoneClock>();
    }

    public class LimestoneDoor : DoorBase
    {
        public override int PlaceTile => TileType<LimestoneDoorClosed>();

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemType<Blocks.LimestoneBrick>(), 6)
                .AddTile(TileID.WorkBenches)
                .Register();
        }
    }
    public class LimestoneDoorClosed : DoorClosedBase
    {
        public override int MyDustType => DustType<LimestoneDust>();
        public override int DropItem => ItemType<LimestoneDoor>();

        public override int OpenVersion => TileType<LimestoneDoorOpen>();
    }
    public class LimestoneDoorOpen : DoorOpenBase
    {
        public override int ClosedVersion => TileType<LimestoneDoorClosed>();
    }

    public class LimestoneDresser : DresserBase
    {
        public override int PlaceTile => TileType<LimestoneDresserTile>();

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemType<Blocks.LimestoneBrick>(), 16)
                .AddTile(TileID.Sawmill)
                .Register();
        }
    }
    public class LimestoneDresserTile : DresserTileBase
    {
        public override int MyDustType => DustType<LimestoneDust>();
        public override int DropItem => ItemType<LimestoneDresser>();
    }

    public class LimestoneLamp : LampBase
    {
        public override int PlaceTile => TileType<LimestoneLampTile>();

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemType<Blocks.LimestoneBrick>(), 3)
                .AddIngredient(ItemID.Torch)
                .AddTile(TileID.WorkBenches)
                .Register();
        }
    }
    public class LimestoneLampTile : LampTileBase
    {
        public override int MyDustType => DustType<LimestoneDust>();
        public override int DropItem => ItemType<LimestoneLamp>();
    }

    public class LimestoneLantern : LanternBase
    {
        public override int PlaceTile => TileType<LimestoneLanternTile>();

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemType<Blocks.LimestoneBrick>(), 6)
                .AddIngredient(ItemID.Torch)
                .AddTile(TileID.WorkBenches)
                .Register();
        }
    }
    public class LimestoneLanternTile : LanternTileBase
    {
        public override int MyDustType => DustType<LimestoneDust>();
        public override int DropItem => ItemType<LimestoneLantern>();
    }

    public class LimestonePiano : PianoBase
    {
        public override int PlaceTile => TileType<LimestonePianoTile>();

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemType<Blocks.LimestoneBrick>(), 15)
                .AddIngredient(ItemID.Bone, 4)
                .AddIngredient(ItemID.Book)
                .AddTile(TileID.Sawmill)
                .Register();
        }
    }
    public class LimestonePianoTile : PianoTileBase
    {
        public override int MyDustType => DustType<LimestoneDust>();
        public override int DropItem => ItemType<LimestonePiano>();
    }

    public class LimestoneSink : SinkBase
    {
        public override int PlaceTile => TileType<LimestoneSinkTile>();

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemType<Blocks.LimestoneBrick>(), 6)
                .AddIngredient(ItemID.WaterBucket)
                .AddTile(TileID.WorkBenches)
                .Register();
        }
    }
    public class LimestoneSinkTile : SinkTileBase
    {
        public override int MyDustType => DustType<LimestoneDust>();
        public override int DropItem => ItemType<LimestoneSink>();
    }

    public class LimestoneSofa : SofaBase
    {
        public override int PlaceTile => TileType<LimestoneSofaTile>();

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemType<Blocks.LimestoneBrick>(), 5)
                .AddIngredient(ItemID.Silk, 2)
                .AddTile(TileID.Sawmill)
                .Register();
        }
    }
    public class LimestoneSofaTile : SofaTileBase
    {
        public override int MyDustType => DustType<LimestoneDust>();
        public override int DropItem => ItemType<LimestoneSofa>();

        public override void ModifySittingPosition(ref Vector2 vector, ref Vector2 vector2, ref Vector2 vector3)
        {
            vector3.Y = (vector.Y = (vector2.Y = 2f));
        }
    }

    public class LimestoneTable : SofaBase
    {
        public override int PlaceTile => TileType<LimestoneTableTile>();

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemType<Blocks.LimestoneBrick>(), 8)
                .AddTile(TileID.WorkBenches)
                .Register();
        }
    }
    public class LimestoneTableTile : TableTileBase
    {
        public override int MyDustType => DustType<LimestoneDust>();
        public override int DropItem => ItemType<LimestoneTable>();
    }

    public class LimestoneTorch : TorchBase
    {
        public override int PlaceTile => TileType<LimestoneTorchTile>();

        public override void AddRecipes()
        {
            CreateRecipe(3)
                .AddIngredient(ItemType<Blocks.Limestone>())
                .AddIngredient(ItemType<Materials.AlkalineFluid>())
                .Register();
        }
    }
    public class LimestoneTorchTile : TorchTileBase
    {
        public override int MyDustType => DustType<LimestoneDust>();
        public override int DropItem => ItemType<LimestoneTorch>();

        public override Color LightColor => new Color(198, 239, 159);
        public override bool Flame => false;

        public override float GetTorchLuck(Player player)
        {
            return player.InModBiome(GetInstance<LimestoneCave>()) ? 1f : -1f;
        }
    }

    public class LimestoneWorkBench : WorkBenchBase
    {
        public override int PlaceTile => TileType<LimestoneWorkBenchTile>();

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemType<Blocks.LimestoneBrick>(), 10)
                .Register();
        }
    }
    public class LimestoneWorkBenchTile : WorkBenchTileBase
    {
        public override int MyDustType => DustType<LimestoneDust>();
        public override int DropItem => ItemType<LimestoneWorkBench>();
    }
}

