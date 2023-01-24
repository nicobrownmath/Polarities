using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria.DataStructures;

namespace Polarities.Items.Placeable.Blocks.Fractal
{
    // This class shows off a number of less common ModTile methods. These methods help our trap tile behave like vanilla traps. 
    // In particular, hammer behavior is particularly tricky. The logic here is setup for multiple styles as well.
    public class FractalTrap : ModTile
    {
        public override void SetStaticDefaults()
        {
            TileID.Sets.DrawsWalls[Type] = true;

            Main.tileSolid[Type] = true;
            Main.tileBlockLight[Type] = true;
            Main.tileFrameImportant[Type] = true;

            // These 2 AddMapEntry and GetMapOption show off multiple Map Entries per Tile. Delete GetMapOption and all but 1 of these for your own ModTile if you don't actually need it.
            AddMapEntry(new Color(21, 179, 192), Language.GetText("MapObject.Trap")); // localized text for "Trap"

            DustType = 116;
            HitSound = SoundID.Tink;
        }

        public override bool IsTileDangerous(int i, int j, Player player)
        {
            return true;
        }

        public override bool Drop(int i, int j)
        {
            Tile t = Main.tile[i, j];
            int style = t.TileFrameY / 18;
            // It can be useful to share a single tile with multiple styles.
            switch (style)
            {
                case 0:
                    Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 16, 16, ItemType<FractalEnergyTrap>());
                    break;
                case 1:
                    Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 16, 16, ItemType<FractalLaserTrap>());
                    break;
            }
            return base.Drop(i, j);
        }
        public override void PlaceInWorld(int i, int j, Item item)
        {
            int style = Main.LocalPlayer.HeldItem.placeStyle;
            Tile tile = Main.tile[i, j];
            tile.TileFrameY = (short)(style * 18);
            if (Main.LocalPlayer.direction == 1)
            {
                tile.TileFrameX += 18;
            }
            if (Main.netMode == 1)
            {
                NetMessage.SendTileSquare(-1, Player.tileTargetX, Player.tileTargetY, 1, TileChangeType.None);
            }
        }

        // This progression matches vanilla tiles, you don't have to follow it if you don't want. Some vanilla traps don't have 6 states, only 4. This can be implemented with different logic in Slope. Making 8 directions is also easily done in a similar manner.
        static int[] frameXCycle = { 1, 2, 3, 0 };
        // We can use the Slope method to override what happens when this tile is hammered.
        public override bool Slope(int i, int j)
        {
            Tile tile = Main.tile[i, j];
            int style = tile.TileFrameY / 18;
            int nextFrameX = frameXCycle[tile.TileFrameX / 18];
            tile.TileFrameX = (short)(nextFrameX * 18);
            if (Main.netMode == 1)
            {
                NetMessage.SendTileSquare(-1, Player.tileTargetX, Player.tileTargetY, 1, TileChangeType.None);
            }
            return false;
        }

        public override void HitWire(int i, int j)
        {
            Tile tile = Main.tile[i, j];
            int style = tile.TileFrameY / 18;
            Vector2 spawnPosition;
            // This logic here corresponds to the orientation of the sprites in the spritesheet, change it if your tile is different in design.
            int horizontalDirection = tile.TileFrameX == 0 ? -1 : tile.TileFrameX == 18 ? 1 : 0;
            int verticalDirection = tile.TileFrameX == 36 ? -1 : tile.TileFrameX == 54 ? 1 : 0;

            // Wiring.CheckMech checks if the wiring cooldown has been reached. Put a longer number here for less frequent projectile spawns. 200 is the dart/flame cooldown. Spear is 90, spiky ball is 300
            switch (style)
            {
                case 0:
                    if (Wiring.CheckMech(i, j, 240))
                    {
                        spawnPosition = new Vector2(i * 16 + 8 + 0 * horizontalDirection, j * 16 + 8 + 0 * verticalDirection); // The extra numbers here help center the projectile spawn position if you need to.

                        // In reality you should be spawning projectiles that are both hostile and friendly to do damage to both players and NPC.
                        // Make sure to change velocity, projectile, damage, and knockback.

                        for (int k = 0; k < Main.rand.Next(3, 6); k++)
                        {
                            Projectile.NewProjectile(new EntitySource_TileUpdate(i, j), spawnPosition, new Vector2(horizontalDirection, verticalDirection).RotatedByRandom(Math.PI / 4) * Main.rand.NextFloat(3f, 6f), ProjectileType<FractalTrapEnergy>(), 30, 4f, Main.myPlayer);
                        }
                        SoundEngine.PlaySound(SoundID.Item61, spawnPosition);
                    }
                    break;
                case 1:
                    if (Wiring.CheckMech(i, j, 240))
                    {
                        spawnPosition = new Vector2(i * 16 + 8 + 16 * horizontalDirection, j * 16 + 8 + 16 * verticalDirection);
                        Projectile.NewProjectile(new EntitySource_TileUpdate(i, j), spawnPosition, new Vector2(horizontalDirection, verticalDirection), ProjectileType<FractalTrapLaser>(), 55, 2f, Main.myPlayer);
                    }
                    break;
            }
        }
    }
    public class FractalTrapEnergy : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 12;
            Projectile.height = 12;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.hostile = true;
            Projectile.penetrate = -1;
            Projectile.tileCollide = true;
            Projectile.light = 0.75f;
            Projectile.timeLeft = 1200;
        }

        public override void AI()
        {
            Projectile.rotation += 0.1f;
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (Projectile.velocity.X != oldVelocity.X)
            {
                Projectile.velocity.X = -oldVelocity.X;
            }
            if (Projectile.velocity.Y != oldVelocity.Y)
            {
                Projectile.velocity.Y = -oldVelocity.Y;
            }
            return false;
        }
    }

    public class FractalTrapLaser : ModProjectile
    {
        // Use a different style for constant so it is very clear in code when a constant is used

        private const float MOVE_DISTANCE = 0f;

        // The actual distance is stored in the ai0 field
        // By making a property to handle this it makes our life easier, and the accessibility more readable
        public float Distance
        {
            get => Projectile.ai[0];
            set => Projectile.ai[0] = value;
        }

        public override void SetStaticDefaults()
        {
        }

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.hostile = true;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.hide = false;
            Projectile.timeLeft = 180;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            // We start drawing the laser
            DrawLaser(Main.spriteBatch, TextureAssets.Projectile[Projectile.type].Value, Projectile.Center, Projectile.velocity, 16, Projectile.damage, -1.57f, 1f, 1000f, Color.White, (int)MOVE_DISTANCE);
            return false;
        }

        // The core function of drawing a laser
        public void DrawLaser(SpriteBatch spriteBatch, Texture2D texture, Vector2 start, Vector2 unit, float step, int damage, float rotation = 0f, float scale = 1f, float maxDist = 2000f, Color color = default(Color), int transDist = 50)
        {
            float r = unit.ToRotation() + rotation;

            Color c = Color.White;
            if (Projectile.timeLeft < 120)
            {
                spriteBatch.Draw(texture, start - Main.screenPosition,
                    new Rectangle(0, Projectile.frame * 16 + 64, 16, 16), c, r,
                    new Vector2(16 * .5f, 16 * .5f), scale, 0, 0);
            }
            else
            {
                spriteBatch.Draw(texture, start - Main.screenPosition,
                    new Rectangle(6, Projectile.frame * 16 + 64, 4, 16), c, r,
                    new Vector2(4 * .5f, 16 * .5f), scale, 0, 0);
            }

            // Draws the laser 'body'
            for (float i = transDist + 16f; i <= Distance; i += step)
            {
                var origin = start + i * unit;

                if (Projectile.timeLeft < 120)
                {
                    spriteBatch.Draw(texture, origin - Main.screenPosition,
                        new Rectangle(0, Projectile.frame * 16, 16, 16), i < transDist ? Color.Transparent : c, r,
                        new Vector2(16 * .5f, 16 * .5f), scale, 0, 0);
                }
                else
                {
                    spriteBatch.Draw(texture, origin - Main.screenPosition,
                        new Rectangle(6, Projectile.frame * 16, 4, 16), i < transDist ? Color.Transparent : c, r,
                        new Vector2(4 * .5f, 16 * .5f), scale, 0, 0);
                }
            }
            // Draws the laser 'head'
            //spriteBatch.Draw(texture, start + (Distance + step) * unit - Main.screenPosition,
            //new Rectangle(0, projectile.frame*16, 16, 16), Color.White, r, new Vector2(16 * .5f, 16 * .5f), scale, 0, 0);
        }

        // Change the way of collision check of the projectile
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            if (Projectile.timeLeft < 120)
            {
                Vector2 unit = Projectile.velocity;
                float point = 0f;
                // Run an AABB versus Line check to look for collisions, look up AABB collision first to see how it works
                // It will look for collisions on the given line using AABB
                return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center,
                    Projectile.Center + unit * Distance, 16, ref point);
            }
            else
            {
                return false;
            }
        }

        // The AI of the projectile
        public override void AI()
        {
            SetLaserPosition();
            SpawnDusts();
            CastLights();
            Projectile.frameCounter++;
            if (Projectile.frameCounter == 2)
            {
                Projectile.frameCounter = 0;
                Projectile.frame = ++Projectile.frame % 4;
            }

            if (Projectile.timeLeft == 180 || Projectile.timeLeft == 160 || Projectile.timeLeft == 140)
            {
                SoundEngine.PlaySound(SoundID.Item93, Projectile.Center);
            }
            else if (Projectile.timeLeft == 120)
            {
                SoundEngine.PlaySound(SoundID.Item122, Projectile.Center);
            }
        }

        private void SpawnDusts()
        {
            if (Projectile.timeLeft < 120)
            {
                Vector2 unit = Projectile.velocity * -1;
                Vector2 dustPos = Projectile.Center + Projectile.velocity * (Distance + 8);

                if (Main.rand.NextBool(5))
                {
                    Vector2 offset = Projectile.velocity.RotatedBy(1.57f) * ((float)Main.rand.NextDouble() - 0.5f) * Projectile.width;
                    Dust dust = Main.dust[Dust.NewDust(dustPos + offset - Vector2.One * 4f, 8, 8, DustID.Electric, 0.0f, 0.0f, 100, new Color(), 1.5f)];
                    dust.noGravity = true;
                }
            }
        }


        /*
		 * Sets the end of the laser position based on where it collides with something
		 */
        private void SetLaserPosition()
        {
            for (Distance = MOVE_DISTANCE; Distance <= 2200f; Distance += 16f)
            {
                var start = Projectile.Center + Projectile.velocity * Distance;
                if (Main.tile[(int)(start.X / 16), (int)(start.Y / 16)].HasTile && Main.tileSolid[Main.tile[(int)(start.X / 16), (int)(start.Y / 16)].TileType])
                {
                    Distance -= 16f;
                    break;
                }
            }
        }

        private void CastLights()
        {
            // Cast a light along the line of the laser
            DelegateMethods.v3_1 = new Vector3(0.8f, 0.8f, 1f);
            Utils.PlotTileLine(Projectile.Center, Projectile.Center + Projectile.velocity * (Distance - MOVE_DISTANCE), 26, DelegateMethods.CastLight);
        }

        public override bool ShouldUpdatePosition() => false;
    }
}