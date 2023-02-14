using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using static Terraria.ModLoader.ModContent;

namespace Polarities.Items.Placeable.Furniture
{
    public abstract class TorchBase : ModItem
    {
        public abstract int PlaceTile { get; }

        public bool WaterDeath => (TileLoader.GetTile(PlaceTile) as TorchTileBase).DieInWater;
        public Color LightColor => (TileLoader.GetTile(PlaceTile) as TorchTileBase).LightColor;
        public bool Flame => (TileLoader.GetTile(PlaceTile) as TorchTileBase).Flame;

        public override void SetStaticDefaults()
        {
            SacrificeTotal = (100);
        }

        public override void SetDefaults()
        {
            Item.DefaultToTorch(0, !WaterDeath);
            Item.createTile = PlaceTile;
            Item.value = Item.sellPrice(copper: 12);

            Item.flame = Flame;
        }

        public override void HoldItem(Player player)
        {
            /*TODO: Dust support for both held and placed torches
			if (Main.rand.Next(player.itemAnimation > 0 ? 40 : 80) == 0)
			{
				Dust.NewDust(new Vector2(player.itemLocation.X + 16f * player.direction, player.itemLocation.Y - 14f * player.gravDir), 4, 4, DustType<SaltTorchDust>());
			}
			*/

            Vector2 position = player.RotatedRelativePoint(new Vector2(player.itemLocation.X + 12f * player.direction + player.velocity.X, player.itemLocation.Y - 14f + player.velocity.Y), true);
            Lighting.AddLight(position, LightColor.R / 255f, LightColor.G / 255f, LightColor.B / 255f);
        }

        public override void PostUpdate()
        {
            Lighting.AddLight((int)((Item.position.X + Item.width / 2) / 16f), (int)((Item.position.Y + Item.height / 2) / 16f), LightColor.R / 255f, LightColor.G / 255f, LightColor.B / 255f);
        }

        public override void AutoLightSelect(ref bool dryTorch, ref bool wetTorch, ref bool glowstick)
        {
            if (WaterDeath)
            {
                dryTorch = true;
            }
            else
            {
                wetTorch = true;
            }
        }
    }

    public abstract class TorchTileBase : ModTile
    {
        public abstract int MyDustType { get; }
        public abstract int DropItem { get; }
        public virtual bool DieInWater => true;
        public virtual bool DieInLava => true;
        public virtual bool Flame => true;
        public virtual Color MapColor => new Color(253, 221, 3);
        public virtual Color LightColor => Color.White;

        public Asset<Texture2D> FlameTexture;

        public override void Load()
        {
            if (Flame)
                FlameTexture = Request<Texture2D>(Texture + "_Flame");
        }

        public override void Unload()
        {
            FlameTexture = null;
        }

        public override void SetStaticDefaults()
        {
            TileID.Sets.FramesOnKillWall[Type] = true;
            TileID.Sets.DisableSmartCursor[Type] = true;
            TileID.Sets.Torch[Type] = true;
            Main.tileLighted[Type] = true;
            Main.tileFrameImportant[Type] = true;
            Main.tileSolid[Type] = false;
            Main.tileNoAttach[Type] = true;
            Main.tileNoFail[Type] = true;

            Main.tileWaterDeath[Type] = DieInWater;
            Main.tileLavaDeath[Type] = DieInLava;

            TileObjectData.newTile.CopyFrom(TileObjectData.StyleTorch);
            if (!DieInWater)
            {
                TileObjectData.newTile.WaterPlacement = LiquidPlacement.Allowed;
                TileObjectData.newTile.WaterDeath = false;
            }
            if (!DieInLava)
            {
                TileObjectData.newTile.LavaPlacement = LiquidPlacement.Allowed;
                TileObjectData.newTile.LavaDeath = false;
            }
            TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidSide, TileObjectData.newTile.Width, 0);
            TileObjectData.newAlternate.CopyFrom(TileObjectData.StyleTorch);
            if (!DieInWater)
            {
                TileObjectData.newAlternate.WaterPlacement = LiquidPlacement.Allowed;
                TileObjectData.newAlternate.WaterDeath = false;
            }
            if (!DieInLava)
            {
                TileObjectData.newAlternate.LavaPlacement = LiquidPlacement.Allowed;
                TileObjectData.newAlternate.LavaDeath = false;
            }
            TileObjectData.newAlternate.AnchorLeft = new AnchorData(AnchorType.SolidTile | AnchorType.SolidSide | AnchorType.Tree | AnchorType.AlternateTile, TileObjectData.newTile.Height, 0);
            TileObjectData.newAlternate.AnchorAlternateTiles = new[] { 124 };
            TileObjectData.addAlternate(1);
            TileObjectData.newAlternate.CopyFrom(TileObjectData.StyleTorch);
            if (!DieInWater)
            {
                TileObjectData.newAlternate.WaterPlacement = LiquidPlacement.Allowed;
                TileObjectData.newAlternate.WaterDeath = false;
            }
            if (!DieInLava)
            {
                TileObjectData.newAlternate.LavaPlacement = LiquidPlacement.Allowed;
                TileObjectData.newAlternate.LavaDeath = false;
            }
            TileObjectData.newAlternate.AnchorRight = new AnchorData(AnchorType.SolidTile | AnchorType.SolidSide | AnchorType.Tree | AnchorType.AlternateTile, TileObjectData.newTile.Height, 0);
            TileObjectData.newAlternate.AnchorAlternateTiles = new[] { 124 };
            TileObjectData.addAlternate(2);
            TileObjectData.newAlternate.CopyFrom(TileObjectData.StyleTorch);
            if (!DieInWater)
            {
                TileObjectData.newAlternate.WaterPlacement = LiquidPlacement.Allowed;
                TileObjectData.newAlternate.WaterDeath = false;
            }
            if (!DieInLava)
            {
                TileObjectData.newAlternate.LavaPlacement = LiquidPlacement.Allowed;
                TileObjectData.newAlternate.LavaDeath = false;
            }
            TileObjectData.newAlternate.AnchorWall = true;
            TileObjectData.addAlternate(0);
            TileObjectData.addTile(Type);
            AddToArray(ref TileID.Sets.RoomNeeds.CountsAsTorch);

            AddMapEntry(MapColor, Lang.GetItemName(ItemID.Torch));
            DustType = MyDustType;
            ItemDrop = DropItem;
            AdjTiles = new int[] { TileID.Torches };
        }

        public override void NumDust(int i, int j, bool fail, ref int num)
        {
            num = fail ? 1 : 3;
        }

        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
        {
            Tile tile = Main.tile[i, j];
            if (tile.TileFrameX < 66)
            {
                r = LightColor.R / 255f;
                g = LightColor.G / 255f;
                b = LightColor.B / 255f;
            }
        }

        public override void SetDrawPositions(int i, int j, ref int width, ref int offsetY, ref int height, ref short tileFrameX, ref short tileFrameY)
        {
            offsetY = 0;

            if (WorldGen.SolidTile(i, j - 1))
            {
                offsetY = 2;

                if (WorldGen.SolidTile(i - 1, j + 1) || WorldGen.SolidTile(i + 1, j + 1))
                {
                    offsetY = 4;
                }
            }
        }

        public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
        {
            if (!Flame) return;

            ulong randSeed = Main.TileFrameSeed ^ (ulong)((long)j << 32 | (uint)i);
            Color color = new Color(100, 100, 100, 0);
            int frameX = Main.tile[i, j].TileFrameX;
            int frameY = Main.tile[i, j].TileFrameY;
            int width = 20;
            int offsetY = 0;
            int height = 20;
            if (WorldGen.SolidTile(i, j - 1))
            {
                offsetY = 2;
                if (WorldGen.SolidTile(i - 1, j + 1) || WorldGen.SolidTile(i + 1, j + 1))
                {
                    offsetY = 4;
                }
            }
            Vector2 zero = new Vector2(Main.offScreenRange, Main.offScreenRange);
            if (Main.drawToScreen)
            {
                zero = Vector2.Zero;
            }
            for (int k = 0; k < 7; k++)
            {
                float x = Utils.RandomInt(ref randSeed, -10, 11) * 0.15f;
                float y = Utils.RandomInt(ref randSeed, -10, 1) * 0.35f;
                Main.spriteBatch.Draw(FlameTexture.Value, new Vector2(i * 16 - (int)Main.screenPosition.X - (width - 16f) / 2f + x, j * 16 - (int)Main.screenPosition.Y + offsetY + y) + zero, new Rectangle(frameX, frameY, width, height), color, 0f, default(Vector2), 1f, SpriteEffects.None, 0f);
            }
        }
    }
}