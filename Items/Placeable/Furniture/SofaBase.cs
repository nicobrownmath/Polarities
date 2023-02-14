using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.ObjectInteractions;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace Polarities.Items.Placeable.Furniture
{
    public abstract class SofaBase : ModItem
    {
        public abstract int PlaceTile { get; }

        public override void SetStaticDefaults()
        {
            SacrificeTotal = (1);
        }

        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(PlaceTile);
            Item.value = Item.sellPrice(copper: 60);
        }
    }

    public abstract class SofaTileBase : ModTile
    {
        public abstract int MyDustType { get; }
        public abstract int DropItem { get; }
        public virtual bool DieInLava => true;
        public virtual Color MapColor => new Color(191, 142, 111);

        public static bool[] IsSofaTileBase;

        public override void Unload()
        {
            IsSofaTileBase = null;
        }

        public override void SetStaticDefaults()
        {
            if (IsSofaTileBase == null)
            {
                IsSofaTileBase = new bool[Main.tileFrameImportant.Length];
            }
            IsSofaTileBase[Type] = true;

            Main.tileFrameImportant[Type] = true;
            Main.tileNoAttach[Type] = true;
            Main.tileLavaDeath[Type] = DieInLava;
            TileID.Sets.CanBeSatOnForPlayers[Type] = true;
            TileID.Sets.CanBeSatOnForNPCs[Type] = true;
            TileID.Sets.DisableSmartCursor[Type] = true;

            TileObjectData.newTile.CopyFrom(TileObjectData.Style3x2);
            TileObjectData.newTile.Origin = new Point16(1, 1);
            TileObjectData.newTile.CoordinateHeights = new[] { 16, 16 };
            TileObjectData.addTile(Type);
            AddToArray(ref TileID.Sets.RoomNeeds.CountsAsChair);

            AddMapEntry(MapColor, Lang.GetItemName(ItemID.Sofa));

            DustType = MyDustType;
            TileID.Sets.DisableSmartCursor[Type] = true;
            AdjTiles = new int[] { 89 };
        }

        public override void NumDust(int i, int j, bool fail, ref int num)
        {
            num = fail ? 1 : 3;
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 48, 32, DropItem);
        }

        public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings)
        {
            return true;
        }

        public override void MouseOver(int i, int j)
        {
            Player player = Main.LocalPlayer;
            if (player.IsWithinSnappngRangeToTile(i, j, 40))
            {
                player.noThrow = 2;
                player.cursorItemIconEnabled = true;
                player.cursorItemIconID = DropItem;
                player.cursorItemIconReversed = Main.tile[i, j].TileFrameX == 0;
            }
        }

        public override bool RightClick(int i, int j)
        {
            Player player = Main.LocalPlayer;
            if (player.IsWithinSnappngRangeToTile(i, j, 40))
            {
                player.GamepadEnableGrappleCooldown();
                player.sitting.SitDown(player, i, j);
                return true;
            }
            return false;
        }

        public virtual void ModifySittingPosition(ref Vector2 vector, ref Vector2 vector2, ref Vector2 vector3)
        {
            vector3.Y = (vector.Y = (vector2.Y = 0f));
        }
    }
}