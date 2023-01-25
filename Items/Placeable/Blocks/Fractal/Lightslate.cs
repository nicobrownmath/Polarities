using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Polarities.Items.Placeable.Blocks.Fractal
{
    public class Lightslate : ModItem
    {
        public override void SetStaticDefaults()
        {
            SacrificeTotal = 100;
            ItemID.Sets.ExtractinatorMode[Type] = Type;
        }

        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<LightslateTile>());
        }

        public override void ExtractinatorUse(ref int resultType, ref int resultStack)
        {
            resultStack = 1;

            /*
            if (Main.rand.NextBool(2000))
            {
				//set to fractus pet item
				return;
            }
			*/

            switch (Main.rand.Next(3))
            {
                case 0:
                    //more lightslate
                    resultType = ItemType<Lightslate>();
                    resultStack = 2;
                    break;
                case 1:
                    //money
                    if (Main.rand.NextBool(5000))
                    {
                        resultType = 74;
                        if (Main.rand.NextBool(10))
                        {
                            resultStack += Main.rand.Next(0, 3);
                        }
                        if (Main.rand.NextBool(10))
                        {
                            resultStack += Main.rand.Next(0, 3);
                        }
                        if (Main.rand.NextBool(10))
                        {
                            resultStack += Main.rand.Next(0, 3);
                        }
                        if (Main.rand.NextBool(10))
                        {
                            resultStack += Main.rand.Next(0, 3);
                        }
                        if (Main.rand.NextBool(10))
                        {
                            resultStack += Main.rand.Next(0, 3);
                        }
                    }
                    else if (Main.rand.NextBool(400))
                    {
                        resultType = 73;
                        if (Main.rand.NextBool(5))
                        {
                            resultStack += Main.rand.Next(1, 21);
                        }
                        if (Main.rand.NextBool(5))
                        {
                            resultStack += Main.rand.Next(1, 21);
                        }
                        if (Main.rand.NextBool(5))
                        {
                            resultStack += Main.rand.Next(1, 21);
                        }
                        if (Main.rand.NextBool(5))
                        {
                            resultStack += Main.rand.Next(1, 21);
                        }
                        if (Main.rand.NextBool(5))
                        {
                            resultStack += Main.rand.Next(1, 20);
                        }
                    }
                    else if (Main.rand.NextBool(30))
                    {
                        resultType = 72;
                        if (Main.rand.NextBool(3))
                        {
                            resultStack += Main.rand.Next(5, 26);
                        }
                        if (Main.rand.NextBool(3))
                        {
                            resultStack += Main.rand.Next(5, 26);
                        }
                        if (Main.rand.NextBool(3))
                        {
                            resultStack += Main.rand.Next(5, 26);
                        }
                        if (Main.rand.NextBool(3))
                        {
                            resultStack += Main.rand.Next(5, 25);
                        }
                    }
                    else
                    {
                        resultType = 71;
                        if (Main.rand.NextBool(2))
                        {
                            resultStack += Main.rand.Next(10, 26);
                        }
                        if (Main.rand.NextBool(2))
                        {
                            resultStack += Main.rand.Next(10, 26);
                        }
                        if (Main.rand.NextBool(2))
                        {
                            resultStack += Main.rand.Next(10, 26);
                        }
                        if (Main.rand.NextBool(2))
                        {
                            resultStack += Main.rand.Next(10, 25);
                        }
                    }
                    break;
                case 2:
                    //fractal materials
                    switch (Main.rand.Next(5))
                    {
                        case 0:
                            resultType = ItemType<FractalMatter>();
                            break;
                        case 1:
                            resultType = ItemType<FractalStrands>();
                            break;
                        case 2:
                            resultType = ItemType<FractalDust>();
                            break;
                        case 3:
                            resultType = ItemType<FractalDuststone>();
                            break;
                        case 4:
                            resultType = ItemType<FractalOre>();
                            break;
                    }
                    if (Main.rand.NextBool(20))
                    {
                        resultStack += Main.rand.Next(0, 2);
                    }
                    if (Main.rand.NextBool(30))
                    {
                        resultStack += Main.rand.Next(0, 3);
                    }
                    if (Main.rand.NextBool(40))
                    {
                        resultStack += Main.rand.Next(0, 4);
                    }
                    if (Main.rand.NextBool(50))
                    {
                        resultStack += Main.rand.Next(0, 5);
                    }
                    if (Main.rand.NextBool(60))
                    {
                        resultStack += Main.rand.Next(0, 6);
                    }
                    break;
            }
        }
    }

    [LegacyName("Lightslate")]
    public class LightslateTile : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileLighted[Type] = true;
            Main.tileSolid[Type] = true;
            Main.tileMergeDirt[Type] = true;
            Main.tileBlockLight[Type] = true;
            Main.tileBlendAll[Type] = true;
            TileID.Sets.Falling[Type] = true;

            AddMapEntry(new Color(117, 171, 235));

            DustType = 116;
            ItemDrop = ItemType<Lightslate>();
            HitSound = SoundID.Dig;

            MineResist = 2f;
            MinPick = 100;
        }

        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
        {
            r = 0.5f;
            g = 0.5f;
            b = 0.5f;
        }

        public override bool CanExplode(int i, int j)
        {
            return true;
        }

        public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak)
        {
            if (WorldGen.noTileActions)
                return true;

            Tile above = Main.tile[i, j - 1];
            Tile below = Main.tile[i, j + 1];
            bool canFall = true;

            if (below == null || below.HasTile)
                canFall = false;

            if (above.HasTile && (TileID.Sets.BasicChest[above.TileType] || TileID.Sets.BasicChestFake[above.TileType] || TileID.Sets.BasicDresser[above.TileType] || above.TileType == TileID.PalmTree))
                canFall = false;

            if (canFall)
            {
                int projectileType = ModContent.ProjectileType<LightslateProjectile>();
                float positionX = i * 16 + 8;
                float positionY = j * 16 + 8;
                var tile = Main.tile[i, j];
                if (Main.netMode == NetmodeID.SinglePlayer)
                {
                    Main.tile[i, j].ClearTile();
                    int proj = Projectile.NewProjectile(new EntitySource_TileBreak(i, j), positionX, positionY, 0f, 0.41f, projectileType, 20, 0f, Main.myPlayer);
                    Main.projectile[proj].ai[0] = 1f;
                    WorldGen.SquareTileFrame(i, j);
                }
                else if (Main.netMode == NetmodeID.Server)
                {
                    tile.HasTile = (false);
                    bool spawnProj = true;

                    for (int k = 0; k < 1000; k++)
                    {
                        Projectile otherProj = Main.projectile[k];

                        if (otherProj.active && otherProj.owner == Main.myPlayer && otherProj.type == projectileType && Math.Abs(otherProj.timeLeft - 3600) < 60 && otherProj.Distance(new Vector2(positionX, positionY)) < 4f)
                        {
                            spawnProj = false;
                            break;
                        }
                    }

                    if (spawnProj)
                    {
                        int proj = Projectile.NewProjectile(new EntitySource_TileBreak(i, j), positionX, positionY, 0f, 2.5f, projectileType, 10, 0f, Main.myPlayer);
                        Main.projectile[proj].velocity.Y = 0.5f;
                        Main.projectile[proj].position.Y += 2f;
                        Main.projectile[proj].netUpdate = true;
                    }

                    NetMessage.SendTileSquare(-1, i, j, 1);
                    WorldGen.SquareTileFrame(i, j);
                }
                return false;
            }
            return true;
        }
    }

    public class LightslateProjectile : ModProjectile
    {
        protected bool falling = true;
        protected int tileType;
        protected int dustType;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Lightslate");
            ProjectileID.Sets.ForcePlateDetection[Projectile.type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.knockBack = 6f;
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.hostile = true;
            Projectile.penetrate = -1;
            tileType = ModContent.TileType<LightslateTile>();
            dustType = 116;

            Projectile.light = 0.5f;
        }

        public override void AI()
        {
            if (Main.rand.NextBool(5))
            {
                int dust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, dustType);
                Main.dust[dust].velocity.X *= 0.4f;
            }

            Projectile.tileCollide = true;
            Projectile.localAI[1] = 0f;

            if (Projectile.ai[0] == 1f)
            {
                if (!falling)
                {
                    Projectile.ai[1] += 1f;

                    if (Projectile.ai[1] >= 60f)
                    {
                        Projectile.ai[1] = 60f;
                        Projectile.velocity.Y += 0.2f;
                    }
                }
                else
                {
                    Projectile.velocity.Y += 0.41f;

                    //wobble wobble
                    Projectile.velocity.X = Projectile.ai[1];

                    if (Main.myPlayer == Projectile.owner)
                    {
                        Projectile.ai[1] = Main.rand.Next(2) * Main.rand.NextFloat(-Projectile.velocity.Y, Projectile.velocity.Y);
                    }
                    Projectile.netUpdate = true;
                }
            }
            else if (Projectile.ai[0] == 2f)
            {
                Projectile.velocity.Y += 0.2f;

                if (Projectile.velocity.X < -0.04f)
                    Projectile.velocity.X += 0.04f;
                else if (Projectile.velocity.X > 0.04f)
                    Projectile.velocity.X -= 0.04f;
                else
                    Projectile.velocity.X = 0f;
            }

            Projectile.rotation += 0.1f;

            if (Projectile.velocity.Y > 10f)
                Projectile.velocity.Y = 10f;
        }

        public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac)
        {
            if (falling)
            {
                Projectile.velocity = Collision.AnyCollision(Projectile.position, Projectile.velocity, Projectile.width, Projectile.height, true);
            }
            else
            {
                Projectile.velocity = Collision.TileCollision(Projectile.position, Projectile.velocity, Projectile.width, Projectile.height, fallThrough, fallThrough, 1);
            }
            return false;
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (Projectile.velocity.X != oldVelocity.X)
            {
                Projectile.velocity.X = 0;
            }
            if (Projectile.velocity.Y != oldVelocity.Y)
            {
                Projectile.velocity.Y = 0;
                Projectile.Kill();
            }
            return false;
        }

        public override void Kill(int timeLeft)
        {
            if (Projectile.owner == Main.myPlayer && !Projectile.noDropItem)
            {
                int tileX = (int)(Projectile.position.X + Projectile.width / 2) / 16;
                int tileY = (int)(Projectile.position.Y + Projectile.width / 2) / 16;

                Tile tile = Main.tile[tileX, tileY];
                Tile tileBelow = Main.tile[tileX, tileY + 1];

                if (tile.IsHalfBlock && Projectile.velocity.Y > 0f && System.Math.Abs(Projectile.velocity.Y) > System.Math.Abs(Projectile.velocity.X))
                    tileY--;

                if (!tile.HasTile)
                {
                    bool onMinecartTrack = tileY < Main.maxTilesY - 2 && tileBelow != null && tileBelow.HasTile && tileBelow.TileType == TileID.MinecartTrack;

                    if (!onMinecartTrack)
                        WorldGen.PlaceTile(tileX, tileY, tileType, false, true);

                    if (!onMinecartTrack && tile.HasTile && tile.TileType == tileType)
                    {
                        if (tileBelow.IsHalfBlock || tileBelow.Slope != 0)
                        {
                            WorldGen.SlopeTile(tileX, tileY + 1, 0);

                            if (Main.netMode == NetmodeID.Server)
                                NetMessage.SendData(17, -1, -1, null, 14, tileX, tileY + 1);
                        }

                        if (Main.netMode != NetmodeID.SinglePlayer)
                            NetMessage.SendData(17, -1, -1, null, 1, tileX, tileY, tileType);
                    }
                }
            }
        }

        public override bool? CanDamage() => Projectile.localAI[1] != -1f ? null : false;
    }
}