using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Polarities.Items.Placeable.Blocks.Fractal
{
    public class FractalOre : ModItem
    {
        public override void SetStaticDefaults()
        {
            SacrificeTotal = 100;
            ItemID.Sets.SortingPriorityMaterials[Type] = 58;
        }

        public override void SetDefaults()
        {
            Item.useStyle = 1;
            Item.useTurn = true;
            Item.useAnimation = 15;
            Item.useTime = 10;
            Item.autoReuse = true;
            Item.maxStack = 999;
            Item.value = Item.sellPrice(silver: 12);
            Item.consumable = true;
            Item.createTile = ModContent.TileType<FractalOreTile>();
            Item.rare = ItemRarityID.Pink;
            Item.width = 16;
            Item.height = 16;
            Item.value = 2500;
        }
    }

    [LegacyName("FractalOre")]
    public class FractalOreTile : ModTile
    {
        public override void SetStaticDefaults()
        {
            TileID.Sets.Ore[Type] = true;
            Main.tileMergeDirt[Type] = false;
            Main.tileSpelunker[Type] = true;
            Main.tileLighted[Type] = true;
            Main.tileOreFinderPriority[Type] = 635;
            Main.tileShine2[Type] = true;
            Main.tileShine[Type] = 975;
            Main.tileSolid[Type] = true;
            Main.tileBlockLight[Type] = true;

            AddMapEntry(new Color(76, 173, 164), CreateMapEntryName());

            DustType = 116;
            ItemDrop = ModContent.ItemType<FractalOre>();
            HitSound = SoundID.Tink;
            MineResist = 3f;
            MinPick = 100;
        }

        public override bool CanExplode(int i, int j)
        {
            return false;
        }
    }
}
