using Microsoft.Xna.Framework;
using Polarities.Items.Consumables;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace Polarities.Items.Placeable.Blocks.Fractal
{
    //note to self: bottom-rooted tiles can attach to all, right-rooted tiles can only attach to bottom-rooted or right-rooted, and left-rooted can only attach to bottom or left-rooted

    public class Fractus : ModItem
    {
        public override void SetStaticDefaults()
        {
            SacrificeTotal = 100;
        }

        public override void SetDefaults()
        {
            Item.useStyle = ItemUseStyleID.Swing;
            Item.useTurn = true;
            Item.useAnimation = 15;
            Item.useTime = 10;
            Item.autoReuse = true;
            Item.maxStack = 999;
            Item.consumable = true;
            Item.createTile = ModContent.TileType<FractusBase>();
            Item.rare = ItemRarityID.White;
            Item.width = 16;
            Item.height = 16;
            Item.value = Item.sellPrice(copper: 3);
        }
    }

    public class FractusBase : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileAxe[Type] = true;

            DustType = 37;
            ItemDrop = ModContent.ItemType<Fractus>();
            //TileID.Sets.DisableSmartCursor[Type] = true;

            HitSound = SoundID.Dig;


            TileObjectData.newTile.Width = 1;
            TileObjectData.newTile.Height = 1;
            TileObjectData.newTile.Origin = Point16.Zero;
            TileObjectData.newTile.UsesCustomCanPlace = true;
            TileObjectData.newTile.CoordinateHeights = new int[]
            {
                18
            };
            TileObjectData.newTile.CoordinateWidth = 16;
            TileObjectData.newTile.CoordinatePadding = 2;
            TileObjectData.newTile.StyleHorizontal = true;
            TileObjectData.newTile.WaterPlacement = LiquidPlacement.NotAllowed;
            TileObjectData.newTile.LavaDeath = true;
            TileObjectData.newTile.LavaPlacement = LiquidPlacement.NotAllowed;
            TileObjectData.newTile.UsesCustomCanPlace = true;

            //this may need to be modified if fracti don't count as solid
            TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile, 1, 0);

            TileObjectData.newTile.AnchorValidTiles = new[]
            {
                ModContent.TileType<FractalDustTile>(),
            };

            AddMapEntry(new Color(160, 88, 173), CreateMapEntryName("Fractus"));

            TileObjectData.addTile(Type);
        }

        public override void RandomUpdate(int i, int j)
        {
            try
            {
                if (WorldGen.InWorld(i, j, 2))
                {
                    //depth check: cannot grow beyond depth 20
                    if (FractusHelper.GetFractusDepth(i, j) >= 20)
                    {
                        return;
                    }

                    //check if there's space to grow upwards without bumping into any tiles
                    if (Main.tile[i, j - 1].HasTile || Main.tile[i - 1, j - 1].HasTile || Main.tile[i, j - 2].HasTile || Main.tile[i + 1, j - 1].HasTile)
                    {
                        return;
                    }

                    WorldGen.PlaceTile(i, j - 1, ModContent.TileType<FractusRootedBottom>(), mute: true);
                }
            }
            catch
            {

            }
        }

        public override void AnimateIndividualTile(int type, int i, int j, ref int frameXOffset, ref int frameYOffset)
        {
            //base of the fractus
            if (FractusHelper.GetFractusDepth(i, j) == 0 && (Framing.GetTileSafely(i, j - 1).TileType == ModContent.TileType<FractusRootedBottom>() || Framing.GetTileSafely(i, j - 1).TileType == ModContent.TileType<FractusFruitBottom>()))
            {
                frameYOffset = 20 * 2;
                return;
            }

            int x = 0;
            int y = 0;
            for (int a = 0; a < 32; a++)
            {
                x = x ^ i >> a & 1;
                y = y ^ j >> a & 1;
            }

            frameYOffset = (x ^ y) * 20;
        }
    }

    public class FractusRootedBottom : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileAxe[Type] = true;

            DustType = 37;
            ItemDrop = ModContent.ItemType<Fractus>();
            TileID.Sets.DisableSmartCursor[Type] = true;

            HitSound = SoundID.Dig;


            TileObjectData.newTile.Width = 1;
            TileObjectData.newTile.Height = 1;
            TileObjectData.newTile.Origin = Point16.Zero;
            TileObjectData.newTile.UsesCustomCanPlace = true;
            TileObjectData.newTile.CoordinateHeights = new int[]
            {
                16
            };
            TileObjectData.newTile.CoordinateWidth = 16;
            TileObjectData.newTile.CoordinatePadding = 2;
            TileObjectData.newTile.StyleHorizontal = true;
            TileObjectData.newTile.WaterPlacement = LiquidPlacement.NotAllowed;
            TileObjectData.newTile.LavaDeath = true;
            TileObjectData.newTile.LavaPlacement = LiquidPlacement.NotAllowed;
            TileObjectData.newTile.UsesCustomCanPlace = true;

            //this may need to be modified if fracti don't count as solid
            TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.AlternateTile, 1, 0);

            TileObjectData.newTile.AnchorValidTiles = new int[0];
            TileObjectData.newTile.AnchorAlternateTiles = new[]
            {
                ModContent.TileType<FractusRootedBottom>(),
                ModContent.TileType<FractusRootedLeft>(),
                ModContent.TileType<FractusRootedRight>(),
                ModContent.TileType<FractusBase>()
            };

            AddMapEntry(new Color(160, 88, 173), CreateMapEntryName("Fractus"));

            TileObjectData.addTile(Type);
        }

        public override void RandomUpdate(int i, int j)
        {
            try
            {
                if (WorldGen.InWorld(i, j, 2))
                {
                    //depth check: cannot grow beyond depth 20
                    if (FractusHelper.GetFractusDepth(i, j) >= 20)
                    {
                        Main.tile[i, j].TileType = (ushort)ModContent.TileType<FractusFruitBottom>();
                        return;
                    }

                    //can't grow if already grown
                    if (Main.tile[i, j - 1].HasTile)
                    {
                        return;
                    }

                    //check if there's space to grow upwards without bumping into any tiles
                    if (Main.tile[i - 1, j - 1].HasTile || Main.tile[i, j - 2].HasTile || Main.tile[i + 1, j - 1].HasTile)
                    {
                        Main.tile[i, j].TileType = (ushort)ModContent.TileType<FractusFruitBottom>();
                        return;
                    }

                    WorldGen.PlaceTile(i, j - 1, ModContent.TileType<FractusRootedBottom>(), mute: true);

                    //try placing to the left
                    if (WorldGen.genRand.NextBool() && !Main.tile[i - 1, j].HasTile && !Main.tile[i - 1, j - 1].HasTile && !Main.tile[i - 2, j].HasTile && !Main.tile[i - 1, j + 1].HasTile)
                    {
                        WorldGen.PlaceTile(i - 1, j, ModContent.TileType<FractusRootedRight>(), mute: true);
                    }
                    //try placing to the right
                    if (WorldGen.genRand.NextBool() && !Main.tile[i + 1, j].HasTile && !Main.tile[i + 1, j - 1].HasTile && !Main.tile[i + 2, j].HasTile && !Main.tile[i + 1, j + 1].HasTile)
                    {
                        WorldGen.PlaceTile(i + 1, j, ModContent.TileType<FractusRootedLeft>(), mute: true);
                    }
                }
            }
            catch
            {

            }
        }

        public override void AnimateIndividualTile(int type, int i, int j, ref int frameXOffset, ref int frameYOffset)
        {
            int state = 0;
            if (Framing.GetTileSafely(i - 1, j).TileType == ModContent.TileType<FractusRootedRight>() || Framing.GetTileSafely(i - 1, j).TileType == ModContent.TileType<FractusFruitRight>())
            {
                state += 1;
            }
            if (Framing.GetTileSafely(i, j - 1).TileType == ModContent.TileType<FractusRootedBottom>() || Framing.GetTileSafely(i, j - 1).TileType == ModContent.TileType<FractusFruitBottom>())
            {
                state += 2;
            }
            if (Framing.GetTileSafely(i + 1, j).TileType == ModContent.TileType<FractusRootedLeft>() || Framing.GetTileSafely(i + 1, j).TileType == ModContent.TileType<FractusFruitLeft>())
            {
                state += 4;
            }
            switch (state)
            {
                case 0:
                    int x = 0;
                    int y = 0;
                    for (int a = 0; a < 32; a++)
                    {
                        x = x ^ i >> a & 1;
                        y = y ^ j >> a & 1;
                    }

                    frameYOffset = (x ^ y) * 18;
                    return;
                case 2:
                    frameYOffset = 2 * 18;
                    return;
                case 1:
                case 3:
                    frameYOffset = 3 * 18;
                    return;
                case 4:
                case 6:
                    frameYOffset = 4 * 18;
                    return;
                case 5:
                case 7:
                    frameYOffset = 5 * 18;
                    return;
            }
        }
    }

    public class FractusRootedLeft : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileAxe[Type] = true;

            DustType = 37;
            ItemDrop = ModContent.ItemType<Fractus>();
            TileID.Sets.DisableSmartCursor[Type] = true;

            HitSound = SoundID.Dig;


            TileObjectData.newTile.Width = 1;
            TileObjectData.newTile.Height = 1;
            TileObjectData.newTile.Origin = Point16.Zero;
            TileObjectData.newTile.UsesCustomCanPlace = true;
            TileObjectData.newTile.CoordinateHeights = new int[]
            {
                16
            };
            TileObjectData.newTile.CoordinateWidth = 16;
            TileObjectData.newTile.CoordinatePadding = 2;
            TileObjectData.newTile.StyleHorizontal = true;
            TileObjectData.newTile.WaterPlacement = LiquidPlacement.NotAllowed;
            TileObjectData.newTile.LavaDeath = true;
            TileObjectData.newTile.LavaPlacement = LiquidPlacement.NotAllowed;
            TileObjectData.newTile.UsesCustomCanPlace = true;

            //this may need to be modified if fracti don't count as solid
            TileObjectData.newTile.AnchorLeft = new AnchorData(AnchorType.AlternateTile, 1, 0);

            TileObjectData.newTile.AnchorValidTiles = new int[0];
            TileObjectData.newTile.AnchorAlternateTiles = new[]
            {
                ModContent.TileType<FractusRootedBottom>(),
                ModContent.TileType<FractusRootedLeft>()
            };

            AddMapEntry(new Color(160, 88, 173), CreateMapEntryName("Fractus"));

            TileObjectData.addTile(Type);
        }

        public override void RandomUpdate(int i, int j)
        {
            try
            {
                if (WorldGen.InWorld(i, j, 2))
                {
                    //depth check: cannot grow beyond depth 20
                    if (FractusHelper.GetFractusDepth(i, j) >= 20)
                    {
                        Main.tile[i, j].TileType = (ushort)ModContent.TileType<FractusFruitLeft>();
                        return;
                    }

                    //can't grow if already grown
                    if (Main.tile[i, j - 1].HasTile || Main.tile[i + 1, j].HasTile)
                    {
                        return;
                    }

                    bool availableUp = false;
                    bool availableSide = false;

                    //check if there's space to grow upwards
                    if (!Main.tile[i, j - 1].HasTile && !Main.tile[i - 1, j - 1].HasTile && !Main.tile[i, j - 2].HasTile && !Main.tile[i + 1, j - 1].HasTile)
                    {
                        availableUp = true;
                    }
                    //check sideways space
                    if (!Main.tile[i + 1, j].HasTile && !Main.tile[i + 1, j - 1].HasTile && !Main.tile[i + 2, j].HasTile && !Main.tile[i + 1, j + 1].HasTile)
                    {
                        availableSide = true;
                    }

                    //can't grow both ways
                    if (availableUp && availableSide)
                    {
                        availableUp = WorldGen.genRand.NextBool();
                        availableSide = !availableUp;
                    }

                    //grow
                    if (availableUp)
                    {
                        WorldGen.PlaceTile(i, j - 1, ModContent.TileType<FractusRootedBottom>(), mute: true);
                    }
                    else if (availableSide)
                    {
                        WorldGen.PlaceTile(i + 1, j, ModContent.TileType<FractusRootedLeft>(), mute: true);
                    }
                    else
                    {
                        Main.tile[i, j].TileType = (ushort)ModContent.TileType<FractusFruitLeft>();
                    }
                }
            }
            catch
            {

            }
        }

        public override void AnimateIndividualTile(int type, int i, int j, ref int frameXOffset, ref int frameYOffset)
        {
            int state = 0;
            if (Framing.GetTileSafely(i, j - 1).TileType == ModContent.TileType<FractusRootedBottom>() || Framing.GetTileSafely(i, j - 1).TileType == ModContent.TileType<FractusFruitBottom>())
            {
                state += 2;
            }
            if (Framing.GetTileSafely(i + 1, j).TileType == ModContent.TileType<FractusRootedLeft>() || Framing.GetTileSafely(i + 1, j).TileType == ModContent.TileType<FractusFruitLeft>())
            {
                state += 4;
            }
            switch (state)
            {
                case 0:
                    int x = 0;
                    int y = 0;
                    for (int a = 0; a < 32; a++)
                    {
                        x = x ^ i >> a & 1;
                        y = y ^ j >> a & 1;
                    }

                    frameYOffset = (x ^ y) * 18;
                    return;
                case 2:
                    frameYOffset = 2 * 18;
                    return;
                case 4:
                    frameYOffset = 3 * 18;
                    return;
            }
        }
    }

    public class FractusRootedRight : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileAxe[Type] = true;

            DustType = 37;
            ItemDrop = ModContent.ItemType<Fractus>();
            TileID.Sets.DisableSmartCursor[Type] = true;

            HitSound = SoundID.Dig;


            TileObjectData.newTile.Width = 1;
            TileObjectData.newTile.Height = 1;
            TileObjectData.newTile.Origin = Point16.Zero;
            TileObjectData.newTile.UsesCustomCanPlace = true;
            TileObjectData.newTile.CoordinateHeights = new int[]
            {
                16
            };
            TileObjectData.newTile.CoordinateWidth = 16;
            TileObjectData.newTile.CoordinatePadding = 2;
            TileObjectData.newTile.StyleHorizontal = true;
            TileObjectData.newTile.WaterPlacement = LiquidPlacement.NotAllowed;
            TileObjectData.newTile.LavaDeath = true;
            TileObjectData.newTile.LavaPlacement = LiquidPlacement.NotAllowed;
            TileObjectData.newTile.UsesCustomCanPlace = true;

            //this may need to be modified if fracti don't count as solid
            TileObjectData.newTile.AnchorRight = new AnchorData(AnchorType.AlternateTile, 1, 0);

            TileObjectData.newTile.AnchorValidTiles = new int[0];
            TileObjectData.newTile.AnchorAlternateTiles = new[]
            {
                ModContent.TileType<FractusRootedBottom>(),
                ModContent.TileType<FractusRootedRight>()
            };

            AddMapEntry(new Color(160, 88, 173), CreateMapEntryName("Fractus"));

            TileObjectData.addTile(Type);
        }

        public override void RandomUpdate(int i, int j)
        {
            try
            {
                if (WorldGen.InWorld(i, j, 2))
                {
                    //depth check: cannot grow beyond depth 20
                    if (FractusHelper.GetFractusDepth(i, j) >= 20)
                    {
                        Main.tile[i, j].TileType = (ushort)ModContent.TileType<FractusFruitRight>();
                        return;
                    }

                    //can't grow if already grown
                    if (Main.tile[i, j - 1].HasTile || Main.tile[i - 1, j].HasTile)
                    {
                        return;
                    }

                    bool availableUp = false;
                    bool availableSide = false;

                    //check if there's space to grow upwards
                    if (!Main.tile[i, j - 1].HasTile && !Main.tile[i - 1, j - 1].HasTile && !Main.tile[i, j - 2].HasTile && !Main.tile[i + 1, j - 1].HasTile)
                    {
                        availableUp = true;
                    }
                    //check sideways space
                    if (!Main.tile[i - 1, j].HasTile && !Main.tile[i - 1, j - 1].HasTile && !Main.tile[i - 2, j].HasTile && !Main.tile[i - 1, j + 1].HasTile)
                    {
                        availableSide = true;
                    }

                    //can't grow both ways
                    if (availableUp && availableSide)
                    {
                        availableUp = WorldGen.genRand.NextBool();
                        availableSide = !availableUp;
                    }

                    //grow
                    if (availableUp)
                    {
                        WorldGen.PlaceTile(i, j - 1, ModContent.TileType<FractusRootedBottom>(), mute: true);
                    }
                    else if (availableSide)
                    {
                        WorldGen.PlaceTile(i - 1, j, ModContent.TileType<FractusRootedRight>(), mute: true);
                    }
                    else
                    {
                        Main.tile[i, j].TileType = (ushort)ModContent.TileType<FractusFruitRight>();
                    }
                }
            }
            catch
            {

            }
        }

        public override void AnimateIndividualTile(int type, int i, int j, ref int frameXOffset, ref int frameYOffset)
        {
            int state = 0;
            if (Framing.GetTileSafely(i - 1, j).TileType == ModContent.TileType<FractusRootedRight>() || Framing.GetTileSafely(i - 1, j).TileType == ModContent.TileType<FractusFruitRight>())
            {
                state += 1;
            }
            if (Framing.GetTileSafely(i, j - 1).TileType == ModContent.TileType<FractusRootedBottom>() || Framing.GetTileSafely(i, j - 1).TileType == ModContent.TileType<FractusFruitBottom>())
            {
                state += 2;
            }
            switch (state)
            {
                case 0:
                    int x = 0;
                    int y = 0;
                    for (int a = 0; a < 32; a++)
                    {
                        x = x ^ i >> a & 1;
                        y = y ^ j >> a & 1;
                    }

                    frameYOffset = (x ^ y) * 18;
                    return;
                case 1:
                    frameYOffset = 3 * 18;
                    return;
                case 2:
                    frameYOffset = 2 * 18;
                    return;
            }
        }
    }

    public class FractusFruitBottom : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileAxe[Type] = true;
            Main.tileLighted[Type] = true;

            DustType = 37;
            ItemDrop = ModContent.ItemType<FractusFruit>();
            TileID.Sets.DisableSmartCursor[Type] = true;

            HitSound = SoundID.Dig;


            TileObjectData.newTile.Width = 1;
            TileObjectData.newTile.Height = 1;
            TileObjectData.newTile.Origin = Point16.Zero;
            TileObjectData.newTile.UsesCustomCanPlace = true;
            TileObjectData.newTile.CoordinateHeights = new int[]
            {
                16
            };
            TileObjectData.newTile.CoordinateWidth = 16;
            TileObjectData.newTile.CoordinatePadding = 2;
            TileObjectData.newTile.StyleHorizontal = true;
            TileObjectData.newTile.WaterPlacement = LiquidPlacement.NotAllowed;
            TileObjectData.newTile.LavaDeath = true;
            TileObjectData.newTile.LavaPlacement = LiquidPlacement.NotAllowed;
            TileObjectData.newTile.UsesCustomCanPlace = true;

            //this may need to be modified if fracti don't count as solid
            TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.AlternateTile, 1, 0);

            TileObjectData.newTile.AnchorValidTiles = new int[0];
            TileObjectData.newTile.AnchorAlternateTiles = new[]
            {
                ModContent.TileType<FractusRootedBottom>(),
                ModContent.TileType<FractusRootedLeft>(),
                ModContent.TileType<FractusRootedRight>(),
                ModContent.TileType<FractusBase>()
            };

            AddMapEntry(new Color(243, 112, 255), CreateMapEntryName("FractusFruit"));

            TileObjectData.addTile(Type);
        }

        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
        {
            r = 0.05f;
            g = 0.05f;
            b = 0.05f;
        }
    }

    public class FractusFruitLeft : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileAxe[Type] = true;
            Main.tileLighted[Type] = true;

            DustType = 37;
            ItemDrop = ModContent.ItemType<FractusFruit>();
            TileID.Sets.DisableSmartCursor[Type] = true;

            HitSound = SoundID.Dig;


            TileObjectData.newTile.Width = 1;
            TileObjectData.newTile.Height = 1;
            TileObjectData.newTile.Origin = Point16.Zero;
            TileObjectData.newTile.UsesCustomCanPlace = true;
            TileObjectData.newTile.CoordinateHeights = new int[]
            {
                16
            };
            TileObjectData.newTile.CoordinateWidth = 16;
            TileObjectData.newTile.CoordinatePadding = 2;
            TileObjectData.newTile.StyleHorizontal = true;
            TileObjectData.newTile.WaterPlacement = LiquidPlacement.NotAllowed;
            TileObjectData.newTile.LavaDeath = true;
            TileObjectData.newTile.LavaPlacement = LiquidPlacement.NotAllowed;
            TileObjectData.newTile.UsesCustomCanPlace = true;

            //this may need to be modified if fracti don't count as solid
            TileObjectData.newTile.AnchorLeft = new AnchorData(AnchorType.AlternateTile, 1, 0);

            TileObjectData.newTile.AnchorValidTiles = new int[0];
            TileObjectData.newTile.AnchorAlternateTiles = new[]
            {
                ModContent.TileType<FractusRootedBottom>(),
                ModContent.TileType<FractusRootedLeft>()
            };

            AddMapEntry(new Color(243, 112, 255), CreateMapEntryName("FractusFruit"));

            TileObjectData.addTile(Type);
        }

        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
        {
            r = 0.05f;
            g = 0.05f;
            b = 0.05f;
        }
    }

    public class FractusFruitRight : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileAxe[Type] = true;
            Main.tileLighted[Type] = true;

            DustType = 37;
            ItemDrop = ModContent.ItemType<Fractus>();
            TileID.Sets.DisableSmartCursor[Type] = true;

            HitSound = SoundID.Dig;


            TileObjectData.newTile.Width = 1;
            TileObjectData.newTile.Height = 1;
            TileObjectData.newTile.Origin = Point16.Zero;
            TileObjectData.newTile.UsesCustomCanPlace = true;
            TileObjectData.newTile.CoordinateHeights = new int[]
            {
                16
            };
            TileObjectData.newTile.CoordinateWidth = 16;
            TileObjectData.newTile.CoordinatePadding = 2;
            TileObjectData.newTile.StyleHorizontal = true;
            TileObjectData.newTile.WaterPlacement = LiquidPlacement.NotAllowed;
            TileObjectData.newTile.LavaDeath = true;
            TileObjectData.newTile.LavaPlacement = LiquidPlacement.NotAllowed;
            TileObjectData.newTile.UsesCustomCanPlace = true;

            //this may need to be modified if fracti don't count as solid
            TileObjectData.newTile.AnchorRight = new AnchorData(AnchorType.AlternateTile, 1, 0);

            TileObjectData.newTile.AnchorValidTiles = new int[0];
            TileObjectData.newTile.AnchorAlternateTiles = new[]
            {
                ModContent.TileType<FractusRootedBottom>(),
                ModContent.TileType<FractusRootedRight>()
            };

            AddMapEntry(new Color(243, 112, 255), CreateMapEntryName("FractusFruit"));

            TileObjectData.addTile(Type);
        }

        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
        {
            r = 0.05f;
            g = 0.05f;
            b = 0.05f;
        }
    }

    public static class FractusHelper
    {
        public static int GetFractusDepth(int i, int j)
        {
            Tile tile = Framing.GetTileSafely(i, j);

            if (tile == null)
            {
                return 0;
            }

            if (tile.TileType == (ushort)ModContent.TileType<FractusBase>())
            {
                return 0;
            }
            else if (tile.TileType == (ushort)ModContent.TileType<FractusRootedBottom>())
            {
                return GetFractusDepth(i, j + 1) + 1;
            }
            else if (tile.TileType == (ushort)ModContent.TileType<FractusRootedLeft>())
            {
                return GetFractusDepth(i - 1, j) + 1;
            }
            else if (tile.TileType == (ushort)ModContent.TileType<FractusRootedRight>())
            {
                return GetFractusDepth(i + 1, j) + 1;
            }

            return 0;
        }

        public static void QuickGrowFractus(List<Point> fractusHeads)
        {
            while (fractusHeads.Count > 0)
            {
                int index = WorldGen.genRand.Next(fractusHeads.Count);
                Point fractusHead = fractusHeads[index];
                fractusHeads.RemoveAt(index);

                FractusGrowthStep(fractusHead.X, fractusHead.Y, fractusHeads);
            }
        }

        private static void FractusGrowthStep(int i, int j, List<Point> fractusHeads)
        {
            Tile tile = Framing.GetTileSafely(i, j);

            if (tile == null)
            {
                return;
            }

            if (tile.TileType == (ushort)ModContent.TileType<FractusBase>())
            {
                if (WorldGen.InWorld(i, j, 2))
                {
                    //depth check: cannot grow beyond depth 20
                    if (GetFractusDepth(i, j) >= 20)
                    {
                        return;
                    }

                    //check if there's space to grow upwards without bumping into any tiles
                    if (Main.tile[i, j - 1].HasTile || Main.tile[i - 1, j - 1].HasTile || Main.tile[i, j - 2].HasTile || Main.tile[i + 1, j - 1].HasTile)
                    {
                        return;
                    }

                    WorldGen.PlaceTile(i, j - 1, ModContent.TileType<FractusRootedBottom>(), mute: true);
                    fractusHeads.Add(new Point(i, j - 1));
                }
            }
            else if (tile.TileType == (ushort)ModContent.TileType<FractusRootedBottom>())
            {
                if (WorldGen.InWorld(i, j, 2))
                {
                    //depth check: cannot grow beyond depth 20
                    if (GetFractusDepth(i, j) >= 20)
                    {
                        tile.TileType = (ushort)ModContent.TileType<FractusFruitBottom>();
                        return;
                    }

                    //can't grow if already grown
                    if (Main.tile[i, j - 1].HasTile)
                    {
                        return;
                    }

                    //check if there's space to grow upwards without bumping into any tiles
                    if (Main.tile[i - 1, j - 1].HasTile || Main.tile[i, j - 2].HasTile || Main.tile[i + 1, j - 1].HasTile)
                    {
                        tile.TileType = (ushort)ModContent.TileType<FractusFruitBottom>();
                        return;
                    }

                    WorldGen.PlaceTile(i, j - 1, ModContent.TileType<FractusRootedBottom>(), mute: true);
                    fractusHeads.Add(new Point(i, j - 1));

                    //try placing to the left
                    if (WorldGen.genRand.NextBool() && !Main.tile[i - 1, j].HasTile && !Main.tile[i - 1, j - 1].HasTile && !Main.tile[i - 2, j].HasTile && !Main.tile[i - 1, j + 1].HasTile)
                    {
                        WorldGen.PlaceTile(i - 1, j, ModContent.TileType<FractusRootedRight>(), mute: true);
                        fractusHeads.Add(new Point(i - 1, j));
                    }
                    //try placing to the right
                    if (WorldGen.genRand.NextBool() && !Main.tile[i + 1, j].HasTile && !Main.tile[i + 1, j - 1].HasTile && !Main.tile[i + 2, j].HasTile && !Main.tile[i + 1, j + 1].HasTile)
                    {
                        WorldGen.PlaceTile(i + 1, j, ModContent.TileType<FractusRootedLeft>(), mute: true);
                        fractusHeads.Add(new Point(i + 1, j));
                    }
                }
            }
            else if (tile.TileType == (ushort)ModContent.TileType<FractusRootedLeft>())
            {
                if (WorldGen.InWorld(i, j, 2))
                {
                    //depth check: cannot grow beyond depth 20
                    if (GetFractusDepth(i, j) >= 20)
                    {
                        tile.TileType = (ushort)ModContent.TileType<FractusFruitLeft>();
                        return;
                    }

                    //can't grow if already grown
                    if (Main.tile[i, j - 1].HasTile || Main.tile[i + 1, j].HasTile)
                    {
                        return;
                    }

                    bool availableUp = false;
                    bool availableSide = false;

                    //check if there's space to grow upwards
                    if (!Main.tile[i, j - 1].HasTile && !Main.tile[i - 1, j - 1].HasTile && !Main.tile[i, j - 2].HasTile && !Main.tile[i + 1, j - 1].HasTile)
                    {
                        availableUp = true;
                    }
                    //check sideways space
                    if (!Main.tile[i + 1, j].HasTile && !Main.tile[i + 1, j - 1].HasTile && !Main.tile[i + 2, j].HasTile && !Main.tile[i + 1, j + 1].HasTile)
                    {
                        availableSide = true;
                    }

                    //can't grow both ways
                    if (availableUp && availableSide)
                    {
                        availableUp = WorldGen.genRand.NextBool();
                        availableSide = !availableUp;
                    }

                    //grow
                    if (availableUp)
                    {
                        WorldGen.PlaceTile(i, j - 1, ModContent.TileType<FractusRootedBottom>(), mute: true);
                        fractusHeads.Add(new Point(i, j - 1));
                    }
                    else if (availableSide)
                    {
                        WorldGen.PlaceTile(i + 1, j, ModContent.TileType<FractusRootedLeft>(), mute: true);
                        fractusHeads.Add(new Point(i + 1, j));
                    }
                    else
                    {
                        tile.TileType = (ushort)ModContent.TileType<FractusFruitLeft>();
                    }
                }
            }
            else if (tile.TileType == (ushort)ModContent.TileType<FractusRootedRight>())
            {
                if (WorldGen.InWorld(i, j, 2))
                {
                    //depth check: cannot grow beyond depth 20
                    if (GetFractusDepth(i, j) >= 20)
                    {
                        tile.TileType = (ushort)ModContent.TileType<FractusFruitRight>();
                        return;
                    }

                    //can't grow if already grown
                    if (Main.tile[i, j - 1].HasTile || Main.tile[i - 1, j].HasTile)
                    {
                        return;
                    }

                    bool availableUp = false;
                    bool availableSide = false;

                    //check if there's space to grow upwards
                    if (!Main.tile[i, j - 1].HasTile && !Main.tile[i - 1, j - 1].HasTile && !Main.tile[i, j - 2].HasTile && !Main.tile[i + 1, j - 1].HasTile)
                    {
                        availableUp = true;
                    }
                    //check sideways space
                    if (!Main.tile[i - 1, j].HasTile && !Main.tile[i - 1, j - 1].HasTile && !Main.tile[i - 2, j].HasTile && !Main.tile[i - 1, j + 1].HasTile)
                    {
                        availableSide = true;
                    }

                    //can't grow both ways
                    if (availableUp && availableSide)
                    {
                        availableUp = WorldGen.genRand.NextBool();
                        availableSide = !availableUp;
                    }

                    //grow
                    if (availableUp)
                    {
                        WorldGen.PlaceTile(i, j - 1, ModContent.TileType<FractusRootedBottom>(), mute: true);
                        fractusHeads.Add(new Point(i, j - 1));
                    }
                    else if (availableSide)
                    {
                        WorldGen.PlaceTile(i - 1, j, ModContent.TileType<FractusRootedRight>(), mute: true);
                        fractusHeads.Add(new Point(i - 1, j));
                    }
                    else
                    {
                        tile.TileType = (ushort)ModContent.TileType<FractusFruitRight>();
                    }
                }
            }
        }
    }
}
