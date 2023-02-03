using System;
using Microsoft.Xna.Framework;
using Polarities.Dusts;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using static Terraria.ModLoader.ModContent;
using Polarities.Biomes;
using Polarities.Items.Placeable.Blocks;

namespace Polarities.Items.Placeable.Furniture.Salt
{
    public class SaltToilet : ModItem 
    {
        public override void SetStaticDefaults()
        {
            SacrificeTotal = 1;
        }

        public override void SetDefaults()
        {
            Item.CloneDefaults(ItemID.WoodenChair);
            Item.createTile = ModContent.TileType<SaltToiletTile>();
            Item.placeStyle = 0;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<SaltBrick>(), 6)
                .AddTile(TileID.Sawmill)
                .Register();
        }
    }
    public class SaltToiletTile : ToiletTileBase 
    {
        public override int MyDustType => DustType<SaltDust>();
        public override int DropItem => ItemType<SaltToilet>();
    }

    public class SaltPlatform : PlatformBase
    {
        public override int PlaceTile => TileType<SaltPlatformTile>();

        public override void AddRecipes()
        {
            CreateRecipe(2)
                .AddIngredient(ItemType<Blocks.SaltBrick>())
                .Register();
        }
    }
    public class SaltPlatformTile : PlatformTileBase
    {
        public override int MyDustType => DustType<SaltDust>();
        public override int DropItem => ItemType<SaltPlatform>();
    }

    public class SaltBathtub : BathtubBase
    {
        public override int PlaceTile => TileType<SaltBathtubTile>();

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemType<Blocks.SaltBrick>(), 14)
                .AddTile(TileID.Sawmill)
                .Register();
        }
    }
    public class SaltBathtubTile : BathtubTileBase
    {
        public override int MyDustType => DustType<SaltDust>();
        public override int DropItem => ItemType<SaltBathtub>();
    }

    public class SaltBed : BedBase
    {
        public override int PlaceTile => TileType<SaltBedTile>();

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemType<Blocks.SaltBrick>(), 15)
                .AddIngredient(ItemID.Silk, 5)
                .AddTile(TileID.Sawmill)
                .Register();
        }
    }
    public class SaltBedTile : BedTileBase
    {
        public override int MyDustType => DustType<SaltDust>();
        public override int DropItem => ItemType<SaltBed>();
    }

    public class SaltBookcase : BookcaseBase
    {
        public override int PlaceTile => TileType<SaltBookcaseTile>();

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemType<Blocks.SaltBrick>(), 20)
                .AddIngredient(ItemID.Book, 10)
                .AddTile(TileID.Sawmill)
                .Register();
        }
    }
    public class SaltBookcaseTile : BookcaseTileBase
    {
        public override int MyDustType => DustType<SaltDust>();
        public override int DropItem => ItemType<SaltBookcase>();
    }

    public class SaltCandelabra : CandelabraBase
    {
        public override int PlaceTile => TileType<SaltCandelabraTile>();

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemType<Blocks.SaltBrick>(), 5)
                .AddIngredient(ItemID.Torch, 3)
                .AddTile(TileID.WorkBenches)
                .Register();
        }
    }
    public class SaltCandelabraTile : CandelabraTileBase
    {
        public override int MyDustType => DustType<SaltDust>();
        public override int DropItem => ItemType<SaltCandelabra>();
        public override bool DieInWater => false;
    }

    public class SaltCandle : CandleBase
    {
        public override int PlaceTile => TileType<SaltCandleTile>();

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemType<Blocks.SaltBrick>(), 4)
                .AddIngredient(ItemID.Torch)
                .AddTile(TileID.WorkBenches)
                .Register();
        }
    }
    public class SaltCandleTile : CandleTileBase
    {
        public override int MyDustType => DustType<SaltDust>();
        public override int DropItem => ItemType<SaltCandle>();
        public override bool DieInWater => false;
    }

    public class SaltChair : ChairBase
    {
        public override int PlaceTile => TileType<SaltChairTile>();

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemType<Blocks.SaltBrick>(), 4)
                .AddTile(TileID.WorkBenches)
                .Register();
        }
    }
    public class SaltChairTile : ChairTileBase
    {
        public override int MyDustType => DustType<SaltDust>();
        public override int DropItem => ItemType<SaltChair>();
    }

    public class SaltChandelier : ChandelierBase
    {
        public override int PlaceTile => TileType<SaltChandelierTile>();

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemType<Blocks.SaltBrick>(), 4)
                .AddIngredient(ItemID.Torch, 4)
                .AddIngredient(ItemID.Chain, 1)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }
    public class SaltChandelierTile : ChandelierTileBase
    {
        public override int MyDustType => DustType<SaltDust>();
        public override int DropItem => ItemType<SaltChandelier>();
        public override bool DieInWater => false;
    }

    public class SaltChest : ChestBase
    {
        public override int PlaceTile => TileType<SaltChestTile>();

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemType<Blocks.SaltBrick>(), 8)
                .AddRecipeGroup(RecipeGroupID.IronBar, 2)
                .AddTile(TileID.WorkBenches)
                .Register();
        }
    }
    public class SaltChestTile : ChestTileBase
    {
        public override int MyDustType => DustType<SaltDust>();
        public override int DropItem => ItemType<SaltChest>();
    }

    public class SaltClock : ClockBase
    {
        public override int PlaceTile => TileType<SaltClockTile>();

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemType<Blocks.SaltBrick>(), 10)
                .AddIngredient(ItemID.Glass, 6)
                .AddRecipeGroup(RecipeGroupID.IronBar, 3)
                .AddTile(TileID.Sawmill)
                .Register();
        }
    }
    public class SaltClockTile : ClockTileBase
    {
        public override int MyDustType => DustType<SaltDust>();
        public override int DropItem => ItemType<SaltClock>();
    }

    public class SaltDoor : DoorBase
    {
        public override int PlaceTile => TileType<SaltDoorClosed>();

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemType<Blocks.SaltBrick>(), 6)
                .AddTile(TileID.WorkBenches)
                .Register();
        }
    }
    public class SaltDoorClosed : DoorClosedBase
    {
        public override int MyDustType => DustType<SaltDust>();
        public override int DropItem => ItemType<SaltDoor>();

        public override int OpenVersion => TileType<SaltDoorOpen>();
    }
    public class SaltDoorOpen : DoorOpenBase
    {
        public override int ClosedVersion => TileType<SaltDoorClosed>();
    }

    public class SaltDresser : DresserBase
    {
        public override int PlaceTile => TileType<SaltDresserTile>();

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemType<Blocks.SaltBrick>(), 16)
                .AddTile(TileID.Sawmill)
                .Register();
        }
    }
    public class SaltDresserTile : DresserTileBase
    {
        public override int MyDustType => DustType<SaltDust>();
        public override int DropItem => ItemType<SaltDresser>();
    }

    public class SaltLamp : LampBase
    {
        public override int PlaceTile => TileType<SaltLampTile>();

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemType<Blocks.SaltBrick>(), 3)
                .AddIngredient(ItemID.Torch)
                .AddTile(TileID.WorkBenches)
                .Register();
        }
    }
    public class SaltLampTile : LampTileBase
    {
        public override int MyDustType => DustType<SaltDust>();
        public override int DropItem => ItemType<SaltLamp>();
    }

    public class SaltLantern : LanternBase
    {
        public override int PlaceTile => TileType<SaltLanternTile>();

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemType<Blocks.SaltBrick>(), 6)
                .AddIngredient(ItemID.Torch)
                .AddTile(TileID.WorkBenches)
                .Register();
        }
    }
    public class SaltLanternTile : LanternTileBase
    {
        public override int MyDustType => DustType<SaltDust>();
        public override int DropItem => ItemType<SaltLantern>();
    }

    public class SaltPiano : PianoBase
    {
        public override int PlaceTile => TileType<SaltPianoTile>();

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemType<Blocks.SaltBrick>(), 15)
                .AddIngredient(ItemID.Bone, 4)
                .AddIngredient(ItemID.Book)
                .AddTile(TileID.Sawmill)
                .Register();
        }
    }
    public class SaltPianoTile : PianoTileBase
    {
        public override int MyDustType => DustType<SaltDust>();
        public override int DropItem => ItemType<SaltPiano>();
    }

    public class SaltPotTile : PotBase
    {
        public override int MyDustType => DustType<SaltDust>();

        public override bool DoSpecialBiomeTorch(ref int itemID)
        {
            itemID = ItemType<SaltTorch>();
            return true;
        }
    }

    public class SaltSink : SinkBase
    {
        public override int PlaceTile => TileType<SaltSinkTile>();

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemType<Blocks.SaltBrick>(), 6)
                .AddIngredient(ItemID.WaterBucket)
                .AddTile(TileID.WorkBenches)
                .Register();
        }
    }
    public class SaltSinkTile : SinkTileBase
    {
        public override int MyDustType => DustType<SaltDust>();
        public override int DropItem => ItemType<SaltSink>();
    }

    public class SaltSofa : SofaBase
    {
        public override int PlaceTile => TileType<SaltSofaTile>();

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemType<Blocks.SaltBrick>(), 5)
                .AddIngredient(ItemID.Silk, 2)
                .AddTile(TileID.Sawmill)
                .Register();
        }
    }
    public class SaltSofaTile : SofaTileBase
    {
        public override int MyDustType => DustType<SaltDust>();
        public override int DropItem => ItemType<SaltSofa>();

        public override void ModifySittingPosition(ref Vector2 vector, ref Vector2 vector2, ref Vector2 vector3)
        {
            vector3.Y = (vector.Y = (vector2.Y = 2f));
        }
    }

    public class SaltTable : SofaBase
    {
        public override int PlaceTile => TileType<SaltTableTile>();

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemType<Blocks.SaltBrick>(), 8)
                .AddTile(TileID.WorkBenches)
                .Register();
        }
    }
    public class SaltTableTile : TableTileBase
    {
        public override int MyDustType => DustType<SaltDust>();
        public override int DropItem => ItemType<SaltTable>();
    }

    public class SaltTorch : TorchBase
    {
        public override int PlaceTile => TileType<SaltTorchTile>();

        public override void AddRecipes()
        {
            CreateRecipe(3)
                .AddIngredient(ItemID.Torch, 3)
                .AddIngredient(ItemType<SaltCrystals>())
                .Register();
        }
    }
    public class SaltTorchTile : TorchTileBase
    {
        public override int MyDustType => DustType<SaltDust>();
        public override int DropItem => ItemType<SaltTorch>();

        public override Color LightColor => new Color(244, 245, 199);

        public override float GetTorchLuck(Player player)
        {
            return player.InModBiome(GetInstance<SaltCave>()) ? 1f : -1f;
        }
    }

    public class SaltWorkBench : WorkBenchBase
    {
        public override int PlaceTile => TileType<SaltWorkBenchTile>();

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemType<Blocks.SaltBrick>(), 10)
                .Register();
        }
    }
    public class SaltWorkBenchTile : WorkBenchTileBase
    {
        public override int MyDustType => DustType<SaltDust>();
        public override int DropItem => ItemType<SaltWorkBench>();
    }
}

