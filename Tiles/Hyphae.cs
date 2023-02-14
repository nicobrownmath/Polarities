using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Polarities.Items.Placeable.Blocks.Fractal;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using static Terraria.ModLoader.ModContent;

namespace Polarities.Tiles
{
    public class HyphaeFilaments : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileCut[Type] = true;
            Main.tileNoFail[Type] = true;
            Main.tileLavaDeath[Type] = true;
            Main.tileLighted[Type] = true;
            DustType = -1;
            TileID.Sets.DisableSmartCursor[Type] = true;

            HitSound = SoundID.Grass;

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
            TileObjectData.newTile.WaterPlacement = LiquidPlacement.Allowed;
            TileObjectData.newTile.LavaDeath = true;
            TileObjectData.newTile.LavaPlacement = LiquidPlacement.NotAllowed;
            TileObjectData.newTile.UsesCustomCanPlace = true;
            TileObjectData.newTile.AnchorTop = new AnchorData(AnchorType.SolidTile | AnchorType.SolidSide, 1, 0);

            TileObjectData.newAlternate.Width = 1;
            TileObjectData.newAlternate.Height = 1;
            TileObjectData.newAlternate.Origin = Point16.Zero;
            TileObjectData.newAlternate.UsesCustomCanPlace = true;
            TileObjectData.newAlternate.CoordinateHeights = new int[]
            {
                16
            };
            TileObjectData.newAlternate.CoordinateWidth = 16;
            TileObjectData.newAlternate.CoordinatePadding = 2;
            TileObjectData.newAlternate.StyleHorizontal = true;
            TileObjectData.newAlternate.WaterPlacement = LiquidPlacement.Allowed;
            TileObjectData.newAlternate.LavaDeath = true;
            TileObjectData.newAlternate.LavaPlacement = LiquidPlacement.NotAllowed;
            TileObjectData.newAlternate.UsesCustomCanPlace = true;
            TileObjectData.newAlternate.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidSide, 1, 0);
            TileObjectData.addAlternate(0);

            TileObjectData.newAlternate.Width = 1;
            TileObjectData.newAlternate.Height = 1;
            TileObjectData.newAlternate.Origin = Point16.Zero;
            TileObjectData.newAlternate.UsesCustomCanPlace = true;
            TileObjectData.newAlternate.CoordinateHeights = new int[]
            {
                16
            };
            TileObjectData.newAlternate.CoordinateWidth = 16;
            TileObjectData.newAlternate.CoordinatePadding = 2;
            TileObjectData.newAlternate.StyleHorizontal = true;
            TileObjectData.newAlternate.WaterPlacement = LiquidPlacement.Allowed;
            TileObjectData.newAlternate.LavaDeath = true;
            TileObjectData.newAlternate.LavaPlacement = LiquidPlacement.NotAllowed;
            TileObjectData.newAlternate.UsesCustomCanPlace = true;
            TileObjectData.newAlternate.AnchorLeft = new AnchorData(AnchorType.SolidTile | AnchorType.SolidSide, 1, 0);
            TileObjectData.addAlternate(0);

            TileObjectData.newAlternate.Width = 1;
            TileObjectData.newAlternate.Height = 1;
            TileObjectData.newAlternate.Origin = Point16.Zero;
            TileObjectData.newAlternate.UsesCustomCanPlace = true;
            TileObjectData.newAlternate.CoordinateHeights = new int[]
            {
                16
            };
            TileObjectData.newAlternate.CoordinateWidth = 16;
            TileObjectData.newAlternate.CoordinatePadding = 2;
            TileObjectData.newAlternate.StyleHorizontal = true;
            TileObjectData.newAlternate.WaterPlacement = LiquidPlacement.Allowed;
            TileObjectData.newAlternate.LavaDeath = true;
            TileObjectData.newAlternate.LavaPlacement = LiquidPlacement.NotAllowed;
            TileObjectData.newAlternate.UsesCustomCanPlace = true;
            TileObjectData.newAlternate.AnchorRight = new AnchorData(AnchorType.SolidTile | AnchorType.SolidSide, 1, 0);
            TileObjectData.addAlternate(0);

            TileObjectData.newTile.AnchorValidTiles = new[]
            {
                TileType<HyphaeFractalDuststone>(),
                TileType<HyphaeFractalMatter>(),
                TileType<HyphaeFractalStrands>()
            };

            AddMapEntry(new Color(200, 62, 254));
            AddMapEntry(new Color(105, 207, 198));
            AddMapEntry(new Color(1, 239, 136));

            TileObjectData.addTile(Type);
        }

        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
        {
            int[] anchorTiles = new int[]
            {
                TileType<HyphaeFractalDuststone>(),
                TileType<HyphaeFractalMatter>(),
                TileType<HyphaeFractalStrands>()
            };

            int anchorType = 0;

            for (int x = 0; x < 3; x++)
            {
                Tile tile = Framing.GetTileSafely(i, j - 1);
                if (tile.HasTile && tile.TileType == anchorTiles[x])
                {
                    anchorType = x;
                    break;
                }
                tile = Framing.GetTileSafely(i, j + 1);
                if (tile.HasTile && tile.TileType == anchorTiles[x])
                {
                    anchorType = x;
                    break;
                }
                tile = Framing.GetTileSafely(i - 1, j);
                if (tile.HasTile && tile.TileType == anchorTiles[x])
                {
                    anchorType = x;
                    break;
                }
                tile = Framing.GetTileSafely(i + 1, j);
                if (tile.HasTile && tile.TileType == anchorTiles[x])
                {
                    anchorType = x;
                    break;
                }
            }

            switch (anchorType)
            {
                case 0:
                    r = 200 / 256f;
                    g = 62 / 256f;
                    b = 254 / 256f;
                    break;
                case 1:
                    r = 105 / 256f;
                    g = 207 / 256f;
                    b = 198 / 256f;
                    break;
                case 2:
                    r = 1 / 256f;
                    g = 239 / 256f;
                    b = 136 / 256f;
                    break;
            }
        }

        public override void AnimateIndividualTile(int type, int i, int j, ref int frameXOffset, ref int frameYOffset)
        {
            int[] anchorTiles = new int[]
            {
                TileType<HyphaeFractalDuststone>(),
                TileType<HyphaeFractalMatter>(),
                TileType<HyphaeFractalStrands>()
            };

            for (int x = 0; x < 3; x++)
            {
                Tile tile = Framing.GetTileSafely(i, j - 1);
                if (tile.HasTile && tile.TileType == anchorTiles[x])
                {
                    frameXOffset = x;
                    frameYOffset = 0 + (i + j) % 4;
                    frameXOffset *= 18;
                    frameYOffset *= 18;
                    return;
                }
                tile = Framing.GetTileSafely(i, j + 1);
                if (tile.HasTile && tile.TileType == anchorTiles[x])
                {
                    frameXOffset = x;
                    frameYOffset = 4 + (i + j) % 4;
                    frameXOffset *= 18;
                    frameYOffset *= 18;
                    return;
                }
                tile = Framing.GetTileSafely(i - 1, j);
                if (tile.HasTile && tile.TileType == anchorTiles[x])
                {
                    frameXOffset = x;
                    frameYOffset = 8 + (i + j) % 4;
                    frameXOffset *= 18;
                    frameYOffset *= 18;
                    return;
                }
                tile = Framing.GetTileSafely(i + 1, j);
                if (tile.HasTile && tile.TileType == anchorTiles[x])
                {
                    frameXOffset = x;
                    frameYOffset = 12 + (i + j) % 4;
                    frameXOffset *= 18;
                    frameYOffset *= 18;
                    return;
                }
            }
        }

        public override void SetSpriteEffects(int i, int j, ref SpriteEffects spriteEffects)
        {
            int[] anchorTiles = new int[]
            {
                TileType<HyphaeFractalDuststone>(),
                TileType<HyphaeFractalMatter>(),
                TileType<HyphaeFractalStrands>()
            };

            if (i + 5 * j % 6 < 3)
            {
                for (ushort x = 0; x < 3; x++)
                {
                    Tile tile = Framing.GetTileSafely(i, j - 1);
                    if (tile.HasTile && tile.TileType == anchorTiles[x])
                    {
                        spriteEffects = SpriteEffects.FlipHorizontally;
                        return;
                    }
                    tile = Framing.GetTileSafely(i, j + 1);
                    if (tile.HasTile && tile.TileType == anchorTiles[x])
                    {
                        spriteEffects = SpriteEffects.FlipHorizontally;
                        return;
                    }
                    tile = Framing.GetTileSafely(i - 1, j);
                    if (tile.HasTile && tile.TileType == anchorTiles[x])
                    {
                        spriteEffects = SpriteEffects.FlipVertically;
                        return;
                    }
                    tile = Framing.GetTileSafely(i + 1, j);
                    if (tile.HasTile && tile.TileType == anchorTiles[x])
                    {
                        spriteEffects = SpriteEffects.FlipVertically;
                        return;
                    }
                }
            }
        }

        public override ushort GetMapOption(int i, int j)
        {
            int[] anchorTiles = new int[]
            {
                TileType<HyphaeFractalDuststone>(),
                TileType<HyphaeFractalMatter>(),
                TileType<HyphaeFractalStrands>()
            };

            for (ushort x = 0; x < 3; x++)
            {
                Tile tile = Framing.GetTileSafely(i, j - 1);
                if (tile.HasTile && tile.TileType == anchorTiles[x])
                {
                    return x;
                }
                tile = Framing.GetTileSafely(i, j + 1);
                if (tile.HasTile && tile.TileType == anchorTiles[x])
                {
                    return x;
                }
                tile = Framing.GetTileSafely(i - 1, j);
                if (tile.HasTile && tile.TileType == anchorTiles[x])
                {
                    return x;
                }
                tile = Framing.GetTileSafely(i + 1, j);
                if (tile.HasTile && tile.TileType == anchorTiles[x])
                {
                    return x;
                }
            }
            return 0;
        }
    }

    public class HyphaeFractalMatter : Hyphae
    {
        public override void SetStaticDefaults()
        {
            substrateType = (ushort)ModContent.TileType<FractalMatterTile>();
            base.SetStaticDefaults();
            Main.tileLighted[Type] = true;
            Main.tileSolid[Type] = true;
            Main.tileBlockLight[Type] = true;
            Main.tileMergeDirt[Type] = true;
            Main.tileBlendAll[Type] = true;
            TileID.Sets.Grass[Type] = true;

            AddMapEntry(new Color(189, 237, 232));

            DustType = 116;
            ItemDrop = ModContent.ItemType<FractalMatter>();
            HitSound = SoundID.Dig;

            MineResist = 3f;
            MinPick = 100;
        }

        public override bool CanExplode(int i, int j)
        {
            return true;
        }

        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
        {
            r = 105 / 384f;
            g = 207 / 384f;
            b = 198 / 384f;
        }
    }

    public class HyphaeFractalStrands : Hyphae
    {
        public override void SetStaticDefaults()
        {
            substrateType = (ushort)ModContent.TileType<FractalStrandsTile>();
            base.SetStaticDefaults();

            Main.tileLighted[Type] = true;
            Main.tileSolid[Type] = true;
            Main.tileBlockLight[Type] = true;
            Main.tileMergeDirt[Type] = true;
            Main.tileBlendAll[Type] = true;
            TileID.Sets.Grass[Type] = true;

            AddMapEntry(new Color(150, 247, 207));

            DustType = 116;
            ItemDrop = ModContent.ItemType<FractalStrands>();
            HitSound = SoundID.Dig;

            MineResist = 3f;
            MinPick = 100;
            //SetModTree(new FractalTree());
        }

        public override bool CanExplode(int i, int j)
        {
            return true;
        }

        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
        {
            r = 1 / 384f;
            g = 239 / 384f;
            b = 136 / 384f;
        }

        //public override int SaplingGrowthType(ref int style)
        //{
        //	style = 0;
        //	return TileType<FractalSapling>();
        //}
    }

    public class HyphaeFractalDuststone : Hyphae
    {
        public override void SetStaticDefaults()
        {
            substrateType = (ushort)ModContent.TileType<FractalDuststoneTile>();
            base.SetStaticDefaults();

            Main.tileLighted[Type] = true;
            Main.tileSolid[Type] = true;
            Main.tileBlockLight[Type] = true;
            Main.tileMergeDirt[Type] = true;
            Main.tileBlendAll[Type] = true;
            TileID.Sets.Grass[Type] = true;

            AddMapEntry(new Color(227, 122, 248));

            DustType = 37;
            ItemDrop = ModContent.ItemType<FractalDuststone>();
            HitSound = SoundID.Dig;

            MineResist = 3f;
            MinPick = 100;
        }

        public override bool CanExplode(int i, int j)
        {
            return true;
        }

        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
        {
            r = 200 / 384f;
            g = 62 / 384f;
            b = 254 / 384f;
        }
    }

    public abstract class Hyphae : ModTile
    {
        public ushort substrateType;

        public override void SetStaticDefaults()
        {
            Main.tileBrick[base.Type] = true;
            TileID.Sets.ChecksForMerge[Type] = true;
            TileID.Sets.NeedsGrassFraming[base.Type] = true;
            TileID.Sets.NeedsGrassFramingDirt[base.Type] = substrateType;
            this.SetMerge(substrateType);
            this.SetMerge<FractalMatterTile>();
            this.SetMerge<FractalDuststoneTile>();
            this.SetMerge<FractalStrandsTile>();
        }

        public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem)
        {
            if (!effectOnly && fail)
            {
                Main.tile[i, j].TileType = substrateType;
            }
        }

        public override void RandomUpdate(int i, int j)
        {
            int num171 = i - 1;
            int num170 = i + 2;
            int num169 = j - 1;
            int num168 = j + 2;
            if (num171 < 10)
            {
                num171 = 10;
            }
            if (num170 > Main.maxTilesX - 10)
            {
                num170 = Main.maxTilesX - 10;
            }
            if (num169 < 10)
            {
                num169 = 10;
            }
            if (num168 > Main.maxTilesY - 10)
            {
                num168 = Main.maxTilesY - 10;
            }

            int type12 = Main.tile[i, j].TileType;
            bool flag33 = false;
            for (int num126 = num171; num126 < num170; num126++)
            {
                for (int num123 = num169; num123 < num168; num123++)
                {
                    if ((i != num126 || j != num123) && Main.tile[num126, num123].HasTile && Main.tile[num126, num123].TileType == substrateType)
                    {
                        SpreadCustomGrass(num126, num123, substrateType, type12);
                        if (Main.tile[num126, num123].TileType == type12)
                        {
                            WorldGen.SquareTileFrame(num126, num123);
                            flag33 = true;
                        }
                    }
                }
            }
            if (Main.netMode == NetmodeID.Server && flag33)
            {
                NetMessage.SendTileSquare(-1, i, j, 3);
            }
            if (WorldGen.genRand.NextBool(6))
            {
                int num125 = i;
                int num124 = j;
                switch (WorldGen.genRand.Next(4))
                {
                    case 0:
                        num125--;
                        break;
                    case 1:
                        num125++;
                        break;
                    case 2:
                        num124--;
                        break;
                    default:
                        num124++;
                        break;
                }
                if (!Main.tile[num125, num124].HasTile)
                {
                    WorldGen.PlaceTile(num125, num124, TileType<HyphaeFilaments>(), mute: true);
                    if (Main.netMode == 2 && Main.tile[num125, num124].HasTile)
                    {
                        NetMessage.SendTileSquare(-1, num125, num124, 1);
                    }
                }
            }
        }

        public static void SpreadCustomGrass(int i, int j, int dirt, int grass, int spread = 0, byte color = 0, bool spreadFilaments = false)
        {
            try
            {
                if (WorldGen.InWorld(i, j, 1) && Main.tile[i, j].TileType == dirt && Main.tile[i, j].HasTile)
                {
                    int num7 = i - 1;
                    int num6 = i + 2;
                    int num5 = j - 1;
                    int num4 = j + 2;
                    if (num7 < 0)
                    {
                        num7 = 0;
                    }
                    if (num6 > Main.maxTilesX)
                    {
                        num6 = Main.maxTilesX;
                    }
                    if (num5 < 0)
                    {
                        num5 = 0;
                    }
                    if (num4 > Main.maxTilesY)
                    {
                        num4 = Main.maxTilesY;
                    }
                    bool flag = true;
                    for (int n = num7; n < num6; n++)
                    {
                        for (int k = num5; k < num4; k++)
                        {
                            if (!Main.tile[n, k].HasTile || !Main.tileSolid[Main.tile[n, k].TileType])
                            {
                                flag = false;
                            }
                            if ((Main.tile[n, k].LiquidType == LiquidID.Lava) && Main.tile[n, k].LiquidAmount > 0)
                            {
                                flag = true;
                                break;
                            }
                        }
                    }
                    if (!flag)
                    {
                        var tile = Main.tile[i, j];
                        Main.tile[i, j].TileType = (ushort)grass;
                        tile.TileColor = color;

                        if (spreadFilaments)
                        {
                            int num125 = i;
                            int num124 = j;
                            switch (WorldGen.genRand.Next(4))
                            {
                                case 0:
                                    num125--;
                                    break;
                                case 1:
                                    num125++;
                                    break;
                                case 2:
                                    num124--;
                                    break;
                                default:
                                    num124++;
                                    break;
                            }
                            if (!Main.tile[num125, num124].HasTile)
                            {
                                WorldGen.PlaceTile(num125, num124, TileType<HyphaeFilaments>(), mute: true);
                                if (Main.netMode == 2 && Main.tile[num125, num124].HasTile)
                                {
                                    NetMessage.SendTileSquare(-1, num125, num124, 1);
                                }
                            }
                        }

                        for (int m = num7; m < num6; m++)
                        {
                            for (int l = num5; l < num4; l++)
                            {
                                if (Main.tile[m, l].HasTile && Main.tile[m, l].TileType == dirt)
                                {
                                    try
                                    {
                                        if (spread > 0)
                                        {
                                            SpreadCustomGrass(m, l, dirt, grass, spread - 1, color, spreadFilaments);
                                        }
                                    }
                                    catch
                                    {
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch
            {
            }
        }
    }
}