﻿using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using Polarities.Buffs;
using Terraria.ObjectData;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Audio;

namespace Polarities.Items.Books
{
    public abstract class BookBase : ModItem
    {
        public abstract int BuffType { get; }
        public abstract int BookIndex { get; }

        public static Dictionary<int, int> bookIndexToItemType = new Dictionary<int, int>();

        public override void SetStaticDefaults()
        {
            this.SetResearch(1);

            bookIndexToItemType.Add(BookIndex, Type);
        }

        public override void Unload()
        {
            bookIndexToItemType = null;
        }

        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(TileType<BookTile>(), BookIndex);
            Item.maxStack = 1;

            Item.width = 26;
            Item.height = 32;

            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.useAnimation = 15;
            Item.useTime = 15;
            Item.useTurn = true;

            Item.rare = ItemRarityID.Blue;
            Item.value = Item.buyPrice(gold: 5);
        }

        public bool allowConsuming = false;

        public override bool? UseItem(Player player)
        {
            return true;
        }

        public override bool ConsumeItem(Player player)
        {
            if (!allowConsuming)
            {
                //vanilla code to check tile range
                if (!(!(player.position.X / 16f - (float)Player.tileRangeX - (float)player.inventory[player.selectedItem].tileBoost - (float)player.blockRange <= (float)Player.tileTargetX) || !((player.position.X + (float)player.width) / 16f + (float)Player.tileRangeX + (float)player.inventory[player.selectedItem].tileBoost - 1f + (float)player.blockRange >= (float)Player.tileTargetX) || !(player.position.Y / 16f - (float)Player.tileRangeY - (float)player.inventory[player.selectedItem].tileBoost - (float)player.blockRange <= (float)Player.tileTargetY) || !((player.position.Y + (float)player.height) / 16f + (float)Player.tileRangeY + (float)player.inventory[player.selectedItem].tileBoost - 2f + (float)player.blockRange >= (float)Player.tileTargetY)))
                {
                    Tile targetTile = Framing.GetTileSafely(Player.tileTargetX, Player.tileTargetY);
                    if (targetTile.HasTile && targetTile.TileType == TileType<BookTile>())
                    {
                        //consume anyways if placing into a second tile
                        if (targetTile.TileFrameX == 0)
                        {
                            targetTile.TileFrameX = (short)(Item.placeStyle * 18);
                            SoundEngine.PlaySound(0, Player.tileTargetX * 16, Player.tileTargetY * 16);

                            return true;
                        }
                        else if (targetTile.TileFrameY == 0)
                        {
                            targetTile.TileFrameY = (short)(Item.placeStyle * 18);
                            SoundEngine.PlaySound(0, Player.tileTargetX * 16, Player.tileTargetY * 16);

                            return true;
                        }
                    }
                }

                SoundEngine.PlaySound(SoundID.Item8, player.position);

                if (player.HasBuff(BuffType))
                {
                    player.ClearBuff(BuffType);
                }
                else
                {
                    if (GetModBuff(BuffType) is BookBuffBase)
                    {
                        if (player.GetModPlayer<PolaritiesPlayer>().usedBookSlots + 1 > player.GetModPlayer<PolaritiesPlayer>().maxBookSlots)
                        {
                            for (int i = 0; i < Player.MaxBuffs; i++)
                            {
                                if (player.buffTime[i] > 0 && GetModBuff(player.buffType[i]) is BookBuffBase bookBuff)
                                {
                                    player.ClearBuff(player.buffType[i]);
                                    player.GetModPlayer<PolaritiesPlayer>().usedBookSlots--;
                                    if (player.GetModPlayer<PolaritiesPlayer>().usedBookSlots + 1 <= player.GetModPlayer<PolaritiesPlayer>().maxBookSlots)
                                        break;
                                }
                            }
                        }
                    }
                    player.AddBuff(BuffType, 2);
                }
                return false;
            }
            allowConsuming = false;
            return true;
        }
    }

    //TODO: Make the pre-placement version a. not show up if invalid and b. show up shifted if placing to the right (I need to find where vanilla does this)
    public class BookTile : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileSolid[Type] = false;
            Main.tileSolidTop[Type] = false;
            Main.tileFrameImportant[Type] = true;

            TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
            TileObjectData.newTile.StyleHorizontal = true;
            TileObjectData.newTile.WaterDeath = false;
            TileObjectData.newTile.LavaDeath = true;
            TileObjectData.addTile(Type);

            AddMapEntry(new Color(128, 128, 128), CreateMapEntryName());
        }

        public override void PlaceInWorld(int i, int j, Item item)
        {
            if (item.ModItem != null && item.ModItem is BookBase bookBase)
                bookBase.allowConsuming = true;

            if (Main.MouseWorld.X % 16 >= 8)
            {
                Framing.GetTileSafely(i, j).TileFrameY = Framing.GetTileSafely(i, j).TileFrameX;
                Framing.GetTileSafely(i, j).TileFrameX = 0;
            }
        }

        public override void MouseOver(int i, int j)
        {
            Player player = Main.LocalPlayer;
            Tile t = Main.tile[i, j];

            int style;
            if (Main.MouseWorld.X % 16 >= 8) style = t.TileFrameY / 18;
            else style = t.TileFrameX / 18;

            if (style > 0)
            {
                player.cursorItemIconID = BookBase.bookIndexToItemType[style];
                player.cursorItemIconEnabled = true;
            }
        }

        public override bool RightClick(int i, int j)
        {
            Tile t = Framing.GetTileSafely(i, j);
            if (Main.MouseWorld.X % 16 < 8)
            {
                if (t.TileFrameX / 18 != 0)
                {
                    int itemType = BookBase.bookIndexToItemType[t.TileFrameX / 18];
                    if (itemType != 0)
                    {
                        Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 8, 16, itemType);
                    }

                    t.TileFrameX = 0;

                    if (t.TileFrameY == 0)
                    {
                        t.HasTile = false;
                    }

                    SoundEngine.PlaySound(SoundID.Dig, i * 16, j * 16);

                    return true;
                }
            }
            else
            {
                if (t.TileFrameY / 18 != 0)
                {
                    int itemType = BookBase.bookIndexToItemType[t.TileFrameY / 18];
                    if (itemType != 0)
                    {
                        Item.NewItem(new EntitySource_TileBreak(i, j), i * 16 + 8, j * 16, 8, 16, itemType);
                    }

                    t.TileFrameY = 0;

                    if (t.TileFrameX == 0)
                    {
                        t.HasTile = false;
                    }

                    SoundEngine.PlaySound(SoundID.Dig, i * 16, j * 16);

                    return true;
                }
            }

            return false;
        }

        public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
        {
            Tile t = Framing.GetTileSafely(i, j);

            bool leanLeft = false;

            for (int a = 0; a < 16; a++)
            {
                leanLeft = (((i >> a) & 1) == 0) == leanLeft;
                leanLeft = (((j >> a) & 1) == 0) == leanLeft;
            }

            Texture2D texture = TextureAssets.Tile[Type].Value;

            int style1 = t.TileFrameX / 18; 
            int style2 = t.TileFrameY / 18;

            if (style1 > 0 && (style2 <= 0 || !leanLeft))
            {
                spriteBatch.Draw(texture, new Vector2(i + 12, j + 12) * 16 - Main.screenPosition, new Rectangle(style1 * 18, 0, 8, 18), Lighting.GetColor(i, j), 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
            }
            if (style2 > 0 && (style1 <= 0 || leanLeft))
            {
                spriteBatch.Draw(texture, new Vector2(i + 12, j + 12) * 16 + new Vector2(8, 0) - Main.screenPosition, new Rectangle(style2 * 18, 0, 8, 18), Lighting.GetColor(i, j), 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
            }
            if (style1 > 0 && style2 > 0)
            {
                if (leanLeft)
                {
                    spriteBatch.Draw(texture, new Vector2(i + 12, j + 12) * 16 + new Vector2(2, 0) - Main.screenPosition, new Rectangle(style1 * 18, 0, 8, 8), Lighting.GetColor(i, j), 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
                    spriteBatch.Draw(texture, new Vector2(i + 12, j + 12) * 16 + new Vector2(0, 8) - Main.screenPosition, new Rectangle(style1 * 18, 8, 8, 10), Lighting.GetColor(i, j), 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
                }
                else
                {
                    spriteBatch.Draw(texture, new Vector2(i + 12, j + 12) * 16 + new Vector2(6, 0) - Main.screenPosition, new Rectangle(style2 * 18, 0, 8, 8), Lighting.GetColor(i, j), 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
                    spriteBatch.Draw(texture, new Vector2(i + 12, j + 12) * 16 + new Vector2(8, 8) - Main.screenPosition, new Rectangle(style2 * 18, 8, 8, 10), Lighting.GetColor(i, j), 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
                }
            }

            return false;
        }

        public override bool Drop(int i, int j)
        {
            Tile t = Main.tile[i, j];
            int style1 = t.TileFrameX / 18;
            if (style1 > 0)
            {
                int itemType1 = BookBase.bookIndexToItemType[style1];
                if (itemType1 != 0)
                {
                    Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 8, 16, itemType1);
                }
            }
            int style2 = t.TileFrameY / 18;
            if (style2 > 0)
            {
                int itemType2 = BookBase.bookIndexToItemType[style2];
                if (itemType2 != 0)
                {
                    Item.NewItem(new EntitySource_TileBreak(i, j), i * 16 + 8, j * 16, 8, 16, itemType2);
                }
            }
            return true;
        }

        public override void NumDust(int i, int j, bool fail, ref int num)
        {
            num = 0;
        }
    }

    public abstract class BookBuffBase : ModBuff
    {
        public override string Texture => "Polarities/Items/Books/BookBuff";

        public abstract int ItemType { get; }

        public override void SetStaticDefaults()
        {
            string itemName = ItemLoader.GetItem(ItemType).Name;
            DisplayName.SetDefault("{$Mods.Polarities.ItemName." + itemName + "}");

            Main.buffNoTimeDisplay[Type] = true;
            Main.persistentBuff[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            if (player.GetModPlayer<PolaritiesPlayer>().usedBookSlots + 1 <= player.GetModPlayer<PolaritiesPlayer>().maxBookSlots)
            {
                player.GetModPlayer<PolaritiesPlayer>().usedBookSlots++;
                player.buffTime[buffIndex] = 2;
            }
        }
    }
}