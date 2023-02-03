using Terraria;
using Terraria.ID;
using Terraria.Localization;

namespace Polarities.Items.Placeable.Furniture
{
    public abstract class ToiletTileBase : ChairTileBase
    {
        protected override void AddMapEntry()
        {
            AddMapEntry(MapColor, Language.GetText("MapObject.Toilet"));
        }

        public override void HitWire(int i, int j)
        {
            Tile tile = Main.tile[i, j];

            int spawnX = i;
            int spawnY = j - (tile.TileFrameY % 40) / 18;

            Wiring.SkipWire(spawnX, spawnY);
            Wiring.SkipWire(spawnX, spawnY + 1);

            if (Wiring.CheckMech(spawnX, spawnY, 60))
            {
                Projectile.NewProjectile(Wiring.GetProjectileSource(spawnX, spawnY), spawnX * 16 + 8, spawnY * 16 + 12, 0f, 0f, ProjectileID.ToiletEffect, 0, 0f, Main.myPlayer);
            }
        }
    }
}