using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace Polarities.Items.Placeable.Furniture
{
    public abstract class LanternBase : ModItem
    {
        public abstract int PlaceTile { get; }

        public override void SetStaticDefaults()
        {
            SacrificeTotal = (1);
        }

        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(PlaceTile, 1);
            Item.value = Item.sellPrice(copper: 30);
        }
    }

    public abstract class LanternTileBase : ModTile
    {
        public abstract int MyDustType { get; }
        public abstract int DropItem { get; }
        public virtual bool DieInWater => true;
        public virtual bool DieInLava => true;
        public virtual Color MapColor => new Color(253, 221, 3);
        public virtual Color LightColor => Color.White;

        public override void SetStaticDefaults()
        {
            Main.tileLighted[Type] = true;
            Main.tileFrameImportant[Type] = true;
            Main.tileNoAttach[Type] = true;
            Main.tileWaterDeath[Type] = DieInWater;
            Main.tileLavaDeath[Type] = DieInLava;

            TileObjectData.newTile.CopyFrom(TileObjectData.Style1x2);
            if (DieInWater)
            {
                TileObjectData.newTile.WaterDeath = true;
                TileObjectData.newTile.WaterPlacement = LiquidPlacement.NotAllowed;
            }
            if (DieInLava)
                TileObjectData.newTile.LavaPlacement = LiquidPlacement.NotAllowed;
            TileObjectData.newTile.StyleHorizontal = true;

            TileObjectData.newTile.AnchorTop = new AnchorData(AnchorType.SolidTile | AnchorType.SolidSide, 1, 0);
            TileObjectData.newTile.AnchorBottom = AnchorData.Empty;
            TileObjectData.newTile.Origin = new Point16(0, 0);

            TileObjectData.addTile(Type);
            AddToArray(ref TileID.Sets.RoomNeeds.CountsAsTorch);

            AddMapEntry(MapColor, Language.GetText("MapObject.Lantern"));

            DustType = MyDustType;
        }

        public override void NumDust(int i, int j, bool fail, ref int num)
        {
            num = fail ? 1 : 3;
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 16, 32, DropItem);
        }

        public override void HitWire(int i, int j)
        {
            Tile tile = Main.tile[i, j];
            int topX = i - tile.TileFrameX / 18 % 1;
            int topY = j - tile.TileFrameY / 18 % 2;
            short frameAdjustment = (short)(tile.TileFrameX >= 18 ? -18 : 18);
            Main.tile[topX, topY].TileFrameX += frameAdjustment;
            Main.tile[topX, topY + 1].TileFrameX += frameAdjustment;
            Wiring.SkipWire(topX, topY);
            Wiring.SkipWire(topX, topY + 1);
            NetMessage.SendTileSquare(-1, i, topY + 1, 3, TileChangeType.None);
        }

        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
        {
            Tile tile = Main.tile[i, j];
            if (tile.TileFrameX >= 18)
            {
                r = LightColor.R / 255f;
                g = LightColor.G / 255f;
                b = LightColor.B / 255f;
            }
        }
    }
}