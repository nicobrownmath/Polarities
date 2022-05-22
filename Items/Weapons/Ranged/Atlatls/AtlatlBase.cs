using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using static Terraria.ModLoader.ModContent;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Polarities.NPCs;
using System;
using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.Audio;
using Terraria.GameContent;

namespace Polarities.Items.Weapons.Ranged.Atlatls
{
	public abstract class AtlatlBase : ModItem, IDrawHeldItem
    {
        public abstract Vector2[] ShotDistances { get; }
        public abstract float BaseShotDistance { get; }
        public virtual float SpriteRotationOffset => 0;

        public static int mostRecentAmmo;
        public static int[] mostRecentShotTypes;
        public static int mostRecentDamage;
        public static float mostRecentKB;
        public static float mostRecentShotSpeed;
        public static int[] usedShots;

        public override void Unload()
        {
            usedShots = null;
        }

        public override void SetStaticDefaults()
        {
            this.SetResearch(1);
        }

        //TODO: Be rotated 90 degrees in inventory
        public override void UseStyle(Player player, Rectangle heldItemFrame)
        {
            if (player.itemAnimation == player.itemAnimationMax)
            {
                usedShots = new int[ShotDistances.Length];
            }

            for (int i = 0; i < usedShots.Length; i++)
            {
                if (usedShots[i] > 0)
                    usedShots[i]++;

                Vector2 shotDistance = ShotDistances[i];
                Vector2 shotPosition = GetShotPosition(player, shotDistance);

                if (usedShots[i] == 0 && IsShotAvailable(player, shotPosition))
                {
                    Vector2 shotVelocity = (Main.MouseWorld - shotPosition).SafeNormalize(Vector2.Zero) * shotDistance.Length() / BaseShotDistance * mostRecentShotSpeed;

                    EntitySource_ItemUse_WithAmmo source = player.GetSource_ItemUse_WithPotentialAmmo(Item, mostRecentAmmo) as EntitySource_ItemUse_WithAmmo;

                    int shotType = mostRecentShotTypes[i];
                    int damage = mostRecentDamage;
                    float KB = mostRecentKB;

                    if (RealShoot(player, source, i, ref shotPosition, ref shotVelocity, ref shotType, ref damage, ref KB))
                    {
                        Projectile.NewProjectile(source, shotPosition, shotVelocity, shotType, damage, KB, player.whoAmI);
                    }

                    usedShots[i] = 1;
                }
            }

            player.itemRotation += player.direction * player.gravDir * (MathHelper.PiOver2 + SpriteRotationOffset);
            player.itemLocation += new Vector2(-player.direction * TextureAssets.Item[Type].Width(), 0).RotatedBy(player.itemRotation);
        }

        public virtual bool RealShoot(Player player, EntitySource_ItemUse_WithAmmo source, int index, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            return true;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            mostRecentAmmo = source.AmmoItemIdUsed;
            mostRecentShotSpeed = velocity.Length();
            mostRecentDamage = damage;
            mostRecentKB = damage;

            mostRecentShotTypes = new int[ShotDistances.Length];
            for (int i = 0; i < mostRecentShotTypes.Length; i++)
            {
                mostRecentShotTypes[i] = type;
            }
            return false;
        }

        bool IsShotAvailable(Player player, Vector2 shotPosition)
        {
            return player.itemAnimation == 0 || ((player.itemRotation + MathHelper.PiOver4 - MathHelper.PiOver4 * player.direction + MathHelper.PiOver4 * player.direction * player.gravDir) % MathHelper.TwoPi * player.direction * player.gravDir > (3 * MathHelper.PiOver4 - 3 * MathHelper.PiOver4 * player.direction + (Main.MouseWorld - shotPosition).ToRotation() - player.fullRotation) % MathHelper.TwoPi * player.direction * player.gravDir);
        }

        Vector2 GetShotPosition(Player player, Vector2 shotDistance)
        {
            return player.position + player.fullRotationOrigin + (player.itemLocation - (player.position + player.fullRotationOrigin) + new Vector2(player.direction * shotDistance.X, -shotDistance.Y).RotatedBy(player.itemRotation).RotatedBy(player.gravDir == 1 ? 0 : player.direction * MathHelper.PiOver2)).RotatedBy(player.fullRotation);
        }

        public void DrawHeldItem(ref PlayerDrawSet drawInfo)
        {
            Vector2 oldLocation = drawInfo.drawPlayer.itemLocation;
            float oldRotation = drawInfo.drawPlayer.itemRotation;

            //reset draw position data to normal after drawing the actual item
            drawInfo.drawPlayer.itemLocation -= new Vector2(-drawInfo.drawPlayer.direction * TextureAssets.Item[Type].Width(), 0).RotatedBy(drawInfo.drawPlayer.itemRotation);
            drawInfo.drawPlayer.itemRotation -= drawInfo.drawPlayer.direction * drawInfo.drawPlayer.gravDir * (MathHelper.PiOver2 + SpriteRotationOffset);

            //draws the darts that haven't yet been used
            //for some reason vanilla darts don't draw until a few ticks after spawning so this can look strange with them
            for (int i = 0; i < usedShots.Length; i++)
            {
                if (usedShots[i] == 0 && DoDartDraw(i))
                {
                    Texture2D dartTexture = TextureAssets.Projectile[mostRecentShotTypes[i]].Value;

                    Vector2 shotDistance = ShotDistances[i];
                    float playerFullRot = drawInfo.drawPlayer.fullRotation;
                    drawInfo.drawPlayer.fullRotation = 0;
                    Vector2 shotPosition = GetShotPosition(drawInfo.drawPlayer, shotDistance) - Main.screenPosition;
                    drawInfo.drawPlayer.fullRotation = playerFullRot;

                    float rotationOffset = MathHelper.Pi - drawInfo.drawPlayer.direction * (MathHelper.PiOver2 - drawInfo.drawPlayer.gravDir * MathHelper.PiOver4);

                    //modded glowing darts
                    if (ProjectileLoader.GetProjectile(mostRecentShotTypes[i]) is ICustomAtlatlDart atlatlDart)
                    {
                        atlatlDart.DrawDart(drawInfo, shotPosition, drawInfo.drawPlayer.itemRotation + rotationOffset);
                    }
                    else
                    {
                        Color dartColor = drawInfo.itemColor;

                        //vanilla glowing darts
                        if (mostRecentShotTypes[i] == ProjectileID.IchorDart || mostRecentShotTypes[i] == ProjectileID.CursedDart || mostRecentShotTypes[i] == ProjectileID.CrystalDart)
                        {
                            dartColor = Color.White;
                        }

                        drawInfo.DrawDataCache.Add(new DrawData(dartTexture, shotPosition, dartTexture.Frame(), dartColor, drawInfo.drawPlayer.itemRotation + rotationOffset, dartTexture.Frame().Size() / 2, 1f, SpriteEffects.None, 0));
                    }
                }
            }

            //re-reset draw data
            drawInfo.drawPlayer.itemLocation = oldLocation;
            drawInfo.drawPlayer.itemRotation = oldRotation;
        }

        public virtual bool DoDartDraw(int index) => true;

        public bool DoVanillaDraw()
        {
            return true;
        }
    }

    public interface ICustomAtlatlDart
    {
        public void DrawDart(PlayerDrawSet drawInfo, Vector2 position, float rotation);
    }
}

