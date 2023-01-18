using Polarities.NPCs;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using Polarities.NPCs.SunPixie;
using Terraria.GameContent;
using Terraria.Localization;
using System.Collections.Generic;
using Terraria.DataStructures;
using Microsoft.CodeAnalysis;
using static Humanizer.In;
using static Terraria.ModLoader.PlayerDrawLayer;
using Polarities.NPCs.StormCloudfish;
using Polarities.NPCs.ConvectiveWanderer;
using Terraria.Audio;
using ReLogic.Content;

namespace Polarities.Items
{
    public class StormCloudfishSummonItem : ModItem
    {
        public override void SetStaticDefaults()
        {
            SacrificeTotal = (1);

            ItemID.Sets.SortingPriorityBossSpawns[Type] = 12;

            ItemID.Sets.IsAKite[Type] = true;
        }

        public override void SetDefaults()
        {
            Item.DefaultTokite(ProjectileType<StormCloudfishSummonProjectile>());
            Item.value = 0;
        }

        public override bool CanShoot(Player player)
        {
            return player.ownedProjectileCounts[Item.shoot] == 0;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.Silk, 20)
                .AddIngredient(ItemID.Worm)
                .AddTile(TileID.WorkBenches)
                .Register();
        }
    }

    public class StormCloudfishSummonProjectile : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = ProjectileID.Sets.TrailCacheLength[ProjectileID.KiteRed];
            ProjectileID.Sets.TrailingMode[Type] = ProjectileID.Sets.TrailingMode[ProjectileID.KiteRed];
            Main.projFrames[Type] = 4;
        }

        public override void SetDefaults()
        {
            Projectile.CloneDefaults(ProjectileID.KiteRed);
        }

        public override bool PreAI()
        {
            Player player = Main.player[Projectile.owner];
            Vector2 vector = player.RotatedRelativePoint(player.MountedCenter);
            Projectile.timeLeft = 60;
            bool flag = false;
            if (player.CCed || player.noItems)
            {
                flag = true;
            }
            else if (player.inventory[player.selectedItem].shoot != Projectile.type)
            {
                flag = true;
            }
            else if (player.pulley)
            {
                flag = true;
            }
            else if (player.dead)
            {
                flag = true;
            }
            if (!flag)
            {
                Vector2 val = player.Center - Projectile.Center;
                flag = val.Length() > 2000f;
            }
            if (flag)
            {
                Projectile.Kill();
                return false;
            }
            float num = 4f;
            float num6 = 800f; //can extend much further than other kites
            float num7 = num6 / 2f;
            if (Projectile.owner == Main.myPlayer && Projectile.extraUpdates == 0)
            {
                float num13 = Projectile.ai[0];
                if (Projectile.ai[0] == 0f)
                {
                    Projectile.ai[0] = num7;
                }
                float num8 = Projectile.ai[0];
                if (Main.mouseRight)
                {
                    num8 -= 5f;
                }
                if (Main.mouseLeft)
                {
                    num8 += 5f;
                }
                Projectile.ai[0] = MathHelper.Clamp(num8, num, num6);
                if (num13 != num8)
                {
                    Projectile.netUpdate = true;
                }
            }
            if (Projectile.numUpdates == 1)
            {
                Projectile.extraUpdates = 0;
            }
            int num9 = 0;
            float cloudAlpha = Main.cloudAlpha;
            float num10 = 0f;
            if (WorldGen.InAPlaceWithWind(Projectile.position, Projectile.width, Projectile.height))
            {
                num10 = Main.WindForVisuals;
            }
            float num11 = Utils.GetLerpValue(0.2f, 0.5f, Math.Abs(num10), clamped: true) * 0.5f;
            switch (num9)
            {
                case 0:
                    {
                        Vector2 mouseWorld = Main.MouseWorld;
                        mouseWorld = Projectile.Center;
                        mouseWorld += new Vector2(num10, (float)Math.Sin((double)Main.GlobalTimeWrappedHourly) + cloudAlpha * 5f) * 25f;
                        Vector2 v = mouseWorld - Projectile.Center;
                        v = v.SafeNormalize(Vector2.Zero) * (3f + cloudAlpha * 7f);
                        if (num11 == 0f)
                        {
                            v = Projectile.velocity;
                        }
                        float num12 = Projectile.Distance(mouseWorld);
                        float lerpValue = Utils.GetLerpValue(5f, 10f, num12, clamped: true);
                        float y = Projectile.velocity.Y;
                        if (num12 > 10f)
                        {
                            Projectile.velocity = Vector2.Lerp(Projectile.velocity, v, 0.075f * lerpValue);
                        }
                        Projectile.velocity.Y = y;
                        Projectile.velocity.Y -= num11;
                        Projectile.velocity.Y += 0.02f + num11 * 0.25f;
                        Projectile.velocity.Y = MathHelper.Clamp(Projectile.velocity.Y, -2f, 2f);
                        if (Projectile.Center.Y + Projectile.velocity.Y < mouseWorld.Y)
                        {
                            Projectile.velocity.Y = MathHelper.Lerp(Projectile.velocity.Y, Projectile.velocity.Y + num11 + 0.01f, 0.75f);
                        }
                        Projectile.velocity.X *= 0.98f;
                        float num2 = Projectile.Distance(vector);
                        float num3 = Projectile.ai[0];
                        if (num2 > num3)
                        {
                            Vector2 vector2 = Projectile.DirectionTo(vector);
                            float scaleFactor = num2 - num3;
                            Projectile.Center += vector2 * scaleFactor;
                            bool num14 = Vector2.Dot(vector2, Vector2.UnitY) < 0.8f || num11 > 0f;
                            Projectile.velocity.Y += vector2.Y * 0.05f;
                            if (num14)
                            {
                                Projectile.velocity.Y -= 0.15f;
                            }
                            Projectile.velocity.X += vector2.X * 0.2f;
                            if (num3 == num && Projectile.owner == Main.myPlayer)
                            {
                                Projectile.Kill();
                                return false;
                            }
                        }
                        break;
                    }
                case 1:
                    {
                        Vector2 value = Projectile.DirectionTo(vector);
                        Projectile.velocity = Vector2.Lerp(Projectile.velocity, value * 16f, 1f);
                        if (Projectile.Distance(vector) < 10f && Projectile.owner == Main.myPlayer)
                        {
                            Projectile.Kill();
                            return false;
                        }
                        break;
                    }
            }
            Projectile.timeLeft = 2;
            Vector2 vector3 = Projectile.Center - vector;
            int dir = ((vector3.X > 0f) ? 1 : (-1));
            if (Math.Abs(vector3.X) > Math.Abs(vector3.Y) / 2f)
            {
                player.ChangeDir(dir);
            }
            Vector2 value2 = Projectile.DirectionTo(vector).SafeNormalize(Vector2.Zero);
            if (num11 == 0f && Projectile.velocity.Y > -0.02f)
            {
                Projectile.rotation *= 0.95f;
            }
            else
            {
                float num4 = (-value2).ToRotation() + (float)Math.PI / 4f;
                if (Projectile.spriteDirection == -1)
                {
                    num4 -= (float)Math.PI / 2f * (float)player.direction;
                }
                Projectile.rotation = num4 + Projectile.velocity.X * 0.05f;
            }
            float num5 = Projectile.velocity.Length();

            if (num5 < 3f)
            {
                Projectile.frame = 0;
            }
            else if (num5 < 5f)
            {
                Projectile.frame = 1;
            }
            else if (num5 < 7f)
            {
                Projectile.frame = 2;
            }
            else
            {
                Projectile.frame = 3;
            }
            Projectile.spriteDirection = player.direction;

            if (Projectile.Center.Y < player.Center.Y - 650 && (player.ZoneOverworldHeight || player.ZoneSkyHeight))
            {
                Projectile.ai[1]++;
                if (Projectile.ai[1] > 60 && !NPC.AnyNPCs(NPCType<StormCloudfish>()))
                {
                    NPC cloudfish = Main.npc[NPC.NewNPC(NPC.GetBossSpawnSource(player.whoAmI), (int)Projectile.Center.X, (int)Projectile.Center.Y, NPCType<StormCloudfish>())];
                    Main.NewText(Language.GetTextValue("Announcement.HasAwoken", cloudfish.TypeName), 171, 64, 255);
                    SoundEngine.PlaySound(SoundID.Roar, player.position);

                    player.velocity += (Projectile.Center - player.Center).SafeNormalize(Vector2.Zero) * 4f; //tug the player

                    SoundEngine.PlaySound(SoundID.Item17, player.position);

                    Projectile.Kill();
                    return false;
                }
            }
            else if (Projectile.ai[1] > 0)
            {
                Projectile.ai[1]--;
            }

            return false;
        }

        public static Asset<Texture2D> TailTexture;

        public override void Load()
        {
            TailTexture = Request<Texture2D>(Texture + "_Tail");
        }

        public override void Unload()
        {
            TailTexture = null;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D value = TextureAssets.Projectile[Type].Value;
            Texture2D value5 = TailTexture.Value;
            int num = 15;
            float num12 = 0f;
            int num23 = 10;
            int num32 = 5;
            float num33 = 10f;
            float num34 = 0f;
            int num35 = -14;
            int num36 = -2;
            int num37 = -1;
            int num2 = -1;
            int num3 = 8;
            int num4 = 0;
            int num5 = 1;
            int num6 = 0;
            int num7 = 0;
            bool flag = true;

            num4 = (Type - 766) * 3 + 3;

            SpriteEffects effects = (SpriteEffects)((Projectile.spriteDirection != 1) ? 1 : 0);
            Rectangle rectangle = value.Frame(Main.projFrames[Type], 1, Projectile.frame);
            Vector2 origin = rectangle.Size() / 2f;
            Vector2 position = Projectile.Center - Main.screenPosition;
            Color color = Lighting.GetColor(Projectile.Center.ToTileCoordinates());
            Color alpha = Projectile.GetAlpha(color);
            Texture2D value6 = TextureAssets.FishingLine.Value;
            Rectangle value7 = value6.Frame();
            Vector2 origin2 = new Vector2((float)(value7.Width / 2), 2f);
            Rectangle rectangle2 = value5.Frame(num);
            int width = rectangle2.Width;
            rectangle2.Width -= 2;
            Vector2 origin3 = rectangle2.Size() / 2f;
            rectangle2.X = width * (num - 1);
            Vector2 playerArmPosition = Main.GetPlayerArmPosition(Projectile);
            Vector2 center = Projectile.Center;
            Vector2.Distance(center, playerArmPosition);
            float scaleFactor = 12f;
            _ = (playerArmPosition - center).SafeNormalize(Vector2.Zero) * scaleFactor;
            Vector2 vector = playerArmPosition;
            Vector2 vector2 = center - vector;
            Vector2 velocity = Projectile.velocity;
            if (Math.Abs(velocity.X) > Math.Abs(velocity.Y))
            {
                Utils.Swap(ref velocity.X, ref velocity.Y);
            }
            float num8 = vector2.Length();
            float num9 = 16f;
            float num10 = 80f;
            bool flag3 = true;
            if (num8 == 0f)
            {
                flag3 = false;
            }
            else
            {
                vector2 *= 12f / num8;
                vector -= vector2;
                vector2 = center - vector;
            }
            while (flag3)
            {
                float num11 = 12f;
                float num13 = vector2.Length();
                float num14 = num13;
                if (float.IsNaN(num13) || num13 == 0f)
                {
                    flag3 = false;
                    continue;
                }
                if (num13 < 20f)
                {
                    num11 = num13 - 8f;
                    flag3 = false;
                }
                num13 = 12f / num13;
                vector2 *= num13;
                vector += vector2;
                vector2 = center - vector;
                if (num14 > 12f)
                {
                    float num15 = 0.3f;
                    float num16 = Math.Abs(velocity.X) + Math.Abs(velocity.Y);
                    if (num16 > num9)
                    {
                        num16 = num9;
                    }
                    num16 = 1f - num16 / num9;
                    num15 *= num16;
                    num16 = num14 / num10;
                    if (num16 > 1f)
                    {
                        num16 = 1f;
                    }
                    num15 *= num16;
                    if (num15 < 0f)
                    {
                        num15 = 0f;
                    }
                    num16 = 1f;
                    num15 *= num16;
                    if (vector2.Y > 0f)
                    {
                        vector2.Y *= 1f + num15;
                        vector2.X *= 1f - num15;
                    }
                    else
                    {
                        num16 = Math.Abs(velocity.X) / 3f;
                        if (num16 > 1f)
                        {
                            num16 = 1f;
                        }
                        num16 -= 0.5f;
                        num15 *= num16;
                        if (num15 > 0f)
                        {
                            num15 *= 2f;
                        }
                        vector2.Y *= 1f + num15;
                        vector2.X *= 1f - num15;
                    }
                }
                float rotation = vector2.ToRotation() - (float)Math.PI / 2f;
                if (!flag3)
                {
                    value7.Height = (int)num11;
                }
                Color color2 = Lighting.GetColor(center.ToTileCoordinates());
                Main.EntitySpriteDraw(value6, vector - Main.screenPosition, value7, color2, rotation, origin2, 1f, (SpriteEffects)0, 0);
            }
            Vector2 value8 = Projectile.Size / 2f;
            float num17 = Math.Abs(Main.WindForVisuals);
            float num18 = MathHelper.Lerp(0.5f, 1f, num17);
            float num19 = num17;
            if (vector2.Y >= -0.02f && vector2.Y < 1f)
            {
                num19 = Utils.GetLerpValue(0.2f, 0.5f, num17, clamped: true);
            }
            int num20 = num32;
            int num21 = num23 + 1;
            for (int i = 0; i < num5; i++)
            {
                rectangle2.X = width * (num - 1);
                List<Vector2> list = new List<Vector2>();
                Vector2 value9 = new Vector2(num18 * (float)num3 * (float)Projectile.spriteDirection, (float)Math.Sin(Main.timeForVisualEffects / 300.0 * 6.2831854820251465) * num19) * 2f;
                float num22 = num35 + num6;
                float num24 = num36 + num7;
                switch (i)
                {
                    case 1:
                        value9 = new Vector2(num18 * (float)num3 * (float)Projectile.spriteDirection, (float)Math.Sin(Main.timeForVisualEffects / 300.0 * 6.2831854820251465) * num19 + 0.5f) * 2f;
                        num22 -= 8f;
                        num24 -= 8f;
                        break;
                    case 2:
                        value9 = new Vector2(num18 * (float)num3 * (float)Projectile.spriteDirection, (float)Math.Sin(Main.timeForVisualEffects / 300.0 * 6.2831854820251465) * num19 + 1f) * 2f;
                        num22 -= 4f;
                        num24 -= 4f;
                        break;
                    case 3:
                        value9 = new Vector2(num18 * (float)num3 * (float)Projectile.spriteDirection, (float)Math.Sin(Main.timeForVisualEffects / 300.0 * 6.2831854820251465) * num19 + 1.5f) * 2f;
                        num22 -= 12f;
                        num24 -= 12f;
                        break;
                }
                Vector2 value10 = Projectile.Center + Utils.RotatedBy(new Vector2(((float)rectangle.Width * 0.5f + num22) * (float)Projectile.spriteDirection, num24), Projectile.rotation + num34);
                list.Add(value10);
                int num25 = num20;
                int num26 = 1;
                while (num25 < num21 * num20)
                {
                    if (num37 != -1 && num37 == num26)
                    {
                        num33 = num2;
                    }
                    Vector2 value11 = Projectile.oldPos[num25];
                    if (value11.X == 0f && value11.Y == 0f)
                    {
                        list.Add(value10);
                    }
                    else
                    {
                        value11 += value8 + Utils.RotatedBy(new Vector2(((float)rectangle.Width * 0.5f + num22) * (float)Projectile.oldSpriteDirection[num25], num24), Projectile.oldRot[num25] + num34);
                        value11 += value9 * (float)(num26 + 1);
                        Vector2 value12 = value10 - value11;
                        float num27 = value12.Length();
                        if (num27 > num33)
                        {
                            value12 *= num33 / num27;
                        }
                        value11 = value10 - value12;
                        list.Add(value11);
                        value10 = value11;
                    }
                    num25 += num20;
                    num26++;
                }
                if (flag)
                {
                    Rectangle value2 = value6.Frame();
                    for (int num28 = list.Count - 2; num28 >= 0; num28--)
                    {
                        Vector2 vector3 = list[num28];
                        Vector2 v = list[num28 + 1] - vector3;
                        float num29 = v.Length();
                        if (!(num29 < 2f))
                        {
                            float rotation2 = v.ToRotation() - (float)Math.PI / 2f;
                            Main.EntitySpriteDraw(value6, vector3 - Main.screenPosition, value2, alpha, rotation2, origin2, new Vector2(1f, num29 / (float)value2.Height), (SpriteEffects)0, 0);
                        }
                    }
                }
                for (int num30 = list.Count - 2; num30 >= 0; num30--)
                {
                    Vector2 value3 = list[num30];
                    Vector2 value4 = list[num30 + 1];
                    Vector2 v2 = value4 - value3;
                    v2.Length();
                    float rotation3 = v2.ToRotation() - (float)Math.PI / 2f + num12;

                    int frame = num30 == 0 ? 0 : num30 == list.Count - 2 ? 2 : 1;
                    rectangle2 = value5.Frame(3, 1, frame, 0);
                    origin3 = rectangle2.Size() / 2;

                    Main.EntitySpriteDraw(value5, value4 - Main.screenPosition, rectangle2, alpha, rotation3, origin3, Projectile.scale, effects, 0);
                }
            }
            Main.EntitySpriteDraw(value, position, rectangle, alpha, Projectile.rotation + num34, origin, Projectile.scale, effects, 0);

            return false;
        }
    }
}

