using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using static Terraria.ModLoader.ModContent;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Polarities.Projectiles;
using System;
using Polarities.Buffs;
using Terraria.DataStructures;
using Terraria.Audio;
using System.Collections.Generic;
using Terraria.GameContent;

namespace Polarities.Items.Weapons.Magic
{
    public class Speleothem : ModItem
    {
        public override void SetStaticDefaults()
        {
            SacrificeTotal = (1);

            Item.staff[Item.type] = true;
        }

        public override void SetDefaults()
        {
            Item.SetWeaponValues(25, 8f, 0);
            Item.DamageType = DamageClass.Magic;
            Item.mana = 15;

            Item.width = 48;
            Item.height = 48;

            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.autoReuse = false;
            Item.noMelee = true;
            Item.useStyle = 5;

            Item.UseSound = SoundID.Item8;
            Item.shoot = ProjectileType<SpeleothemStalactite>();
            Item.shootSpeed = 8f;

            Item.value = 5000;
            Item.rare = ItemRarityID.Green;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            //stalactites
            type = ProjectileType<SpeleothemStalactite>();

            for (int j = -3; j < 4; j++)
            {
                int positionX = j + ((int)(Main.MouseWorld.X)) / 16;
                int positionY = ((int)(Main.MouseWorld.Y)) / 16;

                float extraY = 0;

                for (int i = 0; i < Main.rand.Next(50, 100); i++)
                {
                    positionY--;
                    if (Main.tile[positionX, positionY].HasUnactuatedTile && Main.tileSolid[Main.tile[positionX, positionY].TileType] && !Main.tileSolidTop[Main.tile[positionX, positionY].TileType])
                    {
                        if (Main.tile[positionX, positionY].Slope == SlopeType.SlopeUpLeft || Main.tile[positionX, positionY].Slope == SlopeType.SlopeUpRight)
                        {
                            extraY = -8;
                        }
                        break;
                    }
                }
                Projectile.NewProjectile(source, new Vector2(16 * positionX + 8, 16 * positionY + 16 + extraY), Vector2.Zero, type, damage, knockback, Main.myPlayer);
            }

            //stalagmites
            type = ProjectileType<SpeleothemStalagmite>();

            for (int j = -3; j < 4; j++)
            {
                int positionX = j + ((int)(Main.MouseWorld.X)) / 16;
                int positionY = ((int)(Main.MouseWorld.Y)) / 16;

                float extraY = 0;

                for (int i = 0; i < 50; i++)
                {
                    positionY++;
                    if (Main.tile[positionX, positionY].HasUnactuatedTile && Main.tileSolid[Main.tile[positionX, positionY].TileType] && !Main.tileSolidTop[Main.tile[positionX, positionY].TileType])
                    {
                        if (Main.tile[positionX, positionY].IsHalfBlock || Main.tile[positionX, positionY].Slope == SlopeType.SlopeDownLeft || Main.tile[positionX, positionY].Slope == SlopeType.SlopeDownRight)
                        {
                            extraY = 8;
                        }
                        break;
                    }
                }
                Projectile.NewProjectile(source, new Vector2(16 * positionX + 8, 16 * positionY + extraY), Vector2.Zero, type, damage, knockback, Main.myPlayer);
            }
            return false;
        }

        public override Vector2? HoldoutOrigin()
        {
            return new Vector2(10, 10);
        }
    }

    public class SpeleothemStalactite : ModProjectile
    {
        public override string Texture => "Polarities/Textures/Stalactite";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.DontAttachHideToAlpha[Projectile.type] = true;
            Main.projFrames[Projectile.type] = 3;
        }

        public override void SetDefaults()
        {
            Projectile.width = 2;
            Projectile.height = 2;
            DrawOffsetX = -9;
            DrawOriginOffsetY = -38;
            DrawOriginOffsetX = 0;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.spriteDirection = Main.rand.NextBool() ? 1 : -1;
            Projectile.hide = true;
        }

        public override void AI()
        {
            if (Projectile.ai[0] == 0)
            {
                Projectile.frame = Main.rand.Next(3);
            }

            if (Projectile.ai[0] == 4) Projectile.tileCollide = true;

            Projectile.ai[0]++;

            Projectile.velocity.Y += 0.2f;
            Projectile.velocity.X = 0;
        }

        public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac)
        {
            fallThrough = true;
            return true;
        }

        public override void Kill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Tink, Projectile.Center);
            Collision.HitTiles(Projectile.position + Projectile.velocity, Projectile.velocity, Projectile.width, Projectile.height);
        }

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            behindNPCsAndTiles.Add(index);
        }
    }

    public class SpeleothemStalagmite : ModProjectile
    {
        public override string Texture => "Polarities/Textures/Stalactite";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.DontAttachHideToAlpha[Projectile.type] = true;
            Main.projFrames[Projectile.type] = 3;
        }

        public override void SetDefaults()
        {
            Projectile.width = 2;
            Projectile.height = 2;
            DrawOffsetX = -9;
            DrawOriginOffsetY = -38;
            DrawOriginOffsetX = 0;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.spriteDirection = Main.rand.NextBool() ? 1 : -1;
            Projectile.hide = true;
        }

        public override void AI()
        {
            Projectile.rotation = (float)Math.PI;

            if (Projectile.localAI[0] == 0)
            {
                Projectile.frame = Main.rand.Next(3);
                Projectile.localAI[0] = 1;
            }

            Projectile.ai[0]++;

            switch (Projectile.ai[1])
            {
                case 0:
                    Projectile.velocity.Y = -4;
                    if (Projectile.ai[0] == 8)
                    {
                        Projectile.ai[0] = 0;
                        Projectile.ai[1] = 1;
                    }
                    break;
                case 1:
                    Projectile.velocity.Y = 0;
                    if (Projectile.ai[0] == 8)
                    {
                        Projectile.ai[0] = 0;
                        Projectile.ai[1] = 2;
                    }
                    break;
                case 2:
                    Projectile.velocity.Y = 2;
                    if (Projectile.ai[0] == 16)
                    {
                        Projectile.Kill();
                    }
                    break;
            }

            //ai[1] tracks y offset from start
            Projectile.localAI[1] += Projectile.velocity.Y;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Rectangle frame = texture.Frame(1, 3, 0, Projectile.frame);
            Vector2 origin = frame.Size() / 2 - new Vector2(DrawOriginOffsetX, DrawOriginOffsetY) / 2;

            //cut off some of the frame to account for vertical motion
            int frameCutoff = Math.Clamp(frame.Height + (int)Projectile.localAI[1], 0, frame.Height);

            frame.Y += frameCutoff;
            frame.Height -= frameCutoff;

            SpriteEffects effects = Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition + new Vector2(0, - frameCutoff / 2), frame, lightColor, Projectile.rotation, origin, Projectile.scale, effects, 0f);

            return false;
        }

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            behindNPCsAndTiles.Add(index);
        }
    }
}