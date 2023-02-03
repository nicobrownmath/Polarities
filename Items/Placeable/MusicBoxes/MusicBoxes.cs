using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using static Terraria.ModLoader.ModContent;

namespace Polarities.Items.Placeable.MusicBoxes
{
    public abstract class MusicBoxBase : ModItem
    {
        public abstract int Index { get; }
        public abstract string MusicName { get; }
        public virtual Vector3 ActiveLight => Vector3.Zero;

        public override void Load()
        {
            MusicLoader.AddMusic(Mod, "Sounds/Music/" + MusicName);
        }

        public override void SetStaticDefaults()
        {
            SacrificeTotal = 1;

            MusicLoader.AddMusicBox(Mod, MusicLoader.GetMusicSlot(Mod, "Sounds/Music/" + MusicName), Type, TileType<MusicBoxTile>(), Index * 36);
            MusicBoxTile.musicBoxIndexToItemType.Add(Index, Type);
        }

        public override void SetDefaults()
        {
            Item.useStyle = ItemUseStyleID.Swing;
            Item.useTurn = true;
            Item.useAnimation = 15;
            Item.useTime = 10;
            Item.autoReuse = true;
            Item.consumable = true;
            Item.createTile = TileType<MusicBoxTile>();
            Item.placeStyle = Index;
            Item.width = 24;
            Item.height = 24;
            Item.rare = ItemRarityID.LightRed;
            Item.value = Item.sellPrice(gold: 1);
            Item.accessory = true;
        }
    }

    public class MusicBoxTile : ModTile
    {
        public static Dictionary<int, int> musicBoxIndexToItemType = new Dictionary<int, int>();

        public override void Unload()
        {
            musicBoxIndexToItemType = null;
        }

        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileObsidianKill[Type] = true;
            Main.tileLighted[Type] = true;
            TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
            TileObjectData.newTile.Origin = new Point16(0, 1);
            TileObjectData.newTile.LavaDeath = false;
            TileObjectData.newTile.DrawYOffset = 2;
            TileObjectData.addTile(Type);

            AddMapEntry(new Color(115, 65, 68), Lang.GetItemName(ItemID.MusicBox));
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            int itemType = musicBoxIndexToItemType[frameY / 36];
            if (itemType != 0)
            {
                Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 32, 32, itemType);
            }
        }

        public override void MouseOver(int i, int j)
        {
            Player player = Main.LocalPlayer;
            player.noThrow = 2;
            player.cursorItemIconEnabled = true;
            player.cursorItemIconID = musicBoxIndexToItemType[Main.tile[i, j].TileFrameY / 36];
        }

        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
        {
            if (Main.tile[i, j].TileFrameX > 18)
            {
                Vector3 ActiveLight = (GetModItem(musicBoxIndexToItemType[Main.tile[i, j].TileFrameY / 36]) as MusicBoxBase).ActiveLight;
                r = ActiveLight.X;
                g = ActiveLight.Y;
                b = ActiveLight.Z;
            }
        }
    }

    public class StormCloudfishMusicBox : MusicBoxBase { public override int Index => 0; public override string MusicName => "StormCloudfish"; }
    public class StarConstructMusicBox : MusicBoxBase { public override int Index => 1; public override string MusicName => "StarConstruct"; public override Vector3 ActiveLight => Vector3.One; }
    public class GigabatMusicBox : MusicBoxBase { public override int Index => 2; public override string MusicName => "Gigabat"; public override Vector3 ActiveLight => new Vector3(0.5f); }
    public class SunPixieMusicBox : MusicBoxBase { public override int Index => 3; public override string MusicName => "SunPixie"; public override Vector3 ActiveLight => Vector3.One; }
    public class PestilenceMusicBox : MusicBoxBase { public override int Index => 4; public override string MusicName => "Pestilence"; }
    public class EsophageMusicBox : MusicBoxBase { public override int Index => 5; public override string MusicName => "Esophage"; }
    public class FractalDimensionMusicBox : MusicBoxBase { public override int Index => 6; public override string MusicName => "FractalPalace"; }
    public class FractalDimensionLoadingMusicBox : MusicBoxBase { public override int Index => 7; public override string MusicName => "FractalLoading"; }
    public class RiftDenizenMusicBox : MusicBoxBase { public override int Index => 8; public override string MusicName => "RiftDenizen"; }
    public class SentinelP1MusicBox : MusicBoxBase { public override int Index => 9; public override string MusicName => "SentinelP1"; }
    public class SentinelP2MusicBox : MusicBoxBase { public override int Index => 10; public override string MusicName => "SentinelP2"; }
    public class EclipxieMusicBox : MusicBoxBase { public override int Index => 11; public override string MusicName => "Eclipxie"; }
    public class HemorrphageMusicBox : MusicBoxBase { public override int Index => 12; public override string MusicName => "Hemorrphage"; }
    public class PolaritiesMusicBox : MusicBoxBase { public override int Index => 13; public override string MusicName => "Polarities"; }
    public class ConvectiveWandererMusicBox : MusicBoxBase { public override int Index => 14; public override string MusicName => "EvenDeeperBurns"; }
}