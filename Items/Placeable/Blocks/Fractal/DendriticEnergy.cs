using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Polarities.Items.Placeable.Blocks.Fractal
{
    public class DendriticEnergy : ModItem
    {
        public override void SetStaticDefaults()
        {
            SacrificeTotal = 100;
            ItemID.Sets.SortingPriorityMaterials[Type] = 58;
            ItemID.Sets.ItemNoGravity[Type] = true;
            ItemID.Sets.ItemIconPulse[Type] = true;
        }

        public override void SetDefaults()
        {
            Item.useStyle = ItemUseStyleID.Swing;
            Item.useTurn = true;
            Item.useAnimation = 15;
            Item.useTime = 10;
            Item.autoReuse = true;
            Item.maxStack = 9999;
            Item.consumable = true;
            Item.createTile = ModContent.TileType<DendriticEnergyTile>();
            Item.rare = ItemRarityID.Pink;
            Item.width = 50;
            Item.height = 42;
            Item.value = 3000;
        }

        public override void PostUpdate()
        {
            Lighting.AddLight(Item.Center, Color.WhiteSmoke.ToVector3() * 0.75f * Main.essScale);
        }
    }

    [LegacyName("DendriticEnergy")]
    public class DendriticEnergyTile : ModTile
    {
        public override void SetStaticDefaults()
        {
            TileID.Sets.Ore[Type] = true;
            Main.tileMergeDirt[Type] = true;
            Main.tileSpelunker[Type] = true;
            Main.tileLighted[Type] = true;
            Main.tileOreFinderPriority[Type] = 705;
            Main.tileShine2[Type] = true;
            Main.tileShine[Type] = 975;
            Main.tileSolid[Type] = true;
            Main.tileBlockLight[Type] = true;

            AddMapEntry(new Color(176, 239, 232), CreateMapEntryName());

            DustType = 84;
            ItemDrop = ModContent.ItemType<DendriticEnergy>();
            HitSound = SoundID.Tink;
            MineResist = 4f;
            MinPick = 180;
        }

        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
        {
            r = 1f;
            g = 1f;
            b = 1f;
        }

        public override bool CanExplode(int i, int j)
        {
            return false;
        }

        public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem)
        {
            Player player = Main.player?[Player.FindClosest(new Vector2(i * 16, j * 16), 16, 16)];

            if (player == null)
            {
                noItem = true;
            }
            //else if (Subworld.IsActive<FractalSubworld>() && player.buffTime[player.FindBuffIndex(BuffType<Buffs.FractalSubworldDebuff>())] < FractalSubworld.HARDMODE_DANGER_TIME)
            //{
            //    fail = true;
            //}
        }
    }
}