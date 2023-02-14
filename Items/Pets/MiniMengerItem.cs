using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Polarities.Items.Pets
{
    public class MiniMengerItem : ModItem
    {
        public override void SetStaticDefaults()
        {
            SacrificeTotal = 1;
        }

        public override void SetDefaults()
        {
            Item.damage = 0;
            Item.useStyle = 1;
            Item.shoot = ProjectileType<MiniMenger>();
            Item.width = 22;
            Item.height = 22;
            Item.UseSound = SoundID.Item2;
            Item.useAnimation = 20;
            Item.useTime = 20;
            Item.rare = ItemRarityID.Yellow;
            Item.noMelee = true;
            Item.value = Item.sellPrice(0, 5, 0, 0);
            Item.buffType = BuffType<MiniMengerBuff>();
        }

        public override void UseStyle(Player player, Rectangle heldItemFrame)
        {
            if (player.whoAmI == Main.myPlayer && player.itemTime == 0)
            {
                player.AddBuff(Item.buffType, 3600, true);
            }
        }
    }

    public class MiniMengerBuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.buffNoTimeDisplay[Type] = true;
            Main.lightPet[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            player.buffTime[buffIndex] = 18000;
            bool petProjectileNotSpawned = player.ownedProjectileCounts[ModContent.ProjectileType<MiniMenger>()] <= 0;
            if (petProjectileNotSpawned && player.whoAmI == Main.myPlayer)
            {
                Projectile.NewProjectile(player.GetSource_Buff(buffIndex), player.position.X + player.width / 2, player.position.Y + player.height / 2, 0f, 0f, ModContent.ProjectileType<MiniMenger>(), 0, 0f, player.whoAmI, 0f, 0f);
            }
        }
    }

    public class MiniMenger : ModProjectile
    {
        private float[] verticalHookPositions = new float[2];
        private float[] horizontalHookPositions = new float[2];

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Mini Menger");
            Main.projFrames[Projectile.type] = 1;
            Main.projPet[Projectile.type] = true;
            ProjectileID.Sets.LightPet[Projectile.type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.light = 2f;
            Projectile.width = 22;
            Projectile.height = 22;
            Projectile.penetrate = -1;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
        }

        public override void AI()
        {
            //cast light rays
            DelegateMethods.v3_1 = new Vector3(1, 1, 1);
            Utils.PlotTileLine(new Vector2(Projectile.Center.X, verticalHookPositions[0]), new Vector2(Projectile.Center.X, verticalHookPositions[1]), 1, DelegateMethods.CastLight);
            DelegateMethods.v3_1 = new Vector3(1, 1, 1);
            Utils.PlotTileLine(new Vector2(horizontalHookPositions[0], Projectile.Center.Y), new Vector2(horizontalHookPositions[1], Projectile.Center.Y), 1, DelegateMethods.CastLight);

            Player player = Main.player[Projectile.owner];
            if (!player.active)
            {
                Projectile.active = false;
                return;
            }
            if (player.dead)
            {
                player.ClearBuff(ModContent.BuffType<MiniMengerBuff>());
            }
            if (player.HasBuff(ModContent.BuffType<MiniMengerBuff>()))
            {
                Projectile.timeLeft = 2;
            }

            Vector2 goalPoint = player.Center - new Vector2(player.direction * 50, 50);

            if ((goalPoint - Projectile.Center).Length() > 1200 || Projectile.localAI[0] == 0)
            {
                Projectile.localAI[0] = 1;
                Projectile.position.X = player.position.X;
                Projectile.position.Y = player.position.Y;
                Projectile.velocity.X = 0;
                Projectile.velocity.Y = 0;
                verticalHookPositions[0] = Projectile.Center.Y;
                verticalHookPositions[1] = Projectile.Center.Y;
                horizontalHookPositions[0] = Projectile.Center.X;
                horizontalHookPositions[1] = Projectile.Center.X;
                Projectile.ai[0] = 0;
            }
            else if ((goalPoint - Projectile.Center).Length() > 16)
            {
                switch (Projectile.ai[0])
                {
                    case 0:
                        //extending horizontal hooks
                        bool attached = true;
                        for (int i = 0; i < 2; i++)
                        {
                            Tile tile = Main.tile[(int)(horizontalHookPositions[i] / 16), (int)(Projectile.Center.Y / 16)];
                            if (!(Main.tileSolid[tile.TileType] && !Main.tileSolidTop[tile.TileType]) || !tile.HasTile)
                            {
                                attached = false;
                                horizontalHookPositions[i] += (i == 0 ? 16 : -16);
                            }
                        }
                        if (attached || Math.Abs(horizontalHookPositions[0] - Projectile.Center.X) > 400 || Math.Abs(horizontalHookPositions[1] - Projectile.Center.X) > 400)
                        {
                            Projectile.ai[0] = 1;
                        }
                        break;
                    case 1:
                        //retracting vertical hooks
                        bool retracted = true;
                        for (int i = 0; i < 2; i++)
                        {
                            if (Math.Abs(verticalHookPositions[i] - Projectile.Center.Y) > 32)
                            {
                                retracted = false;
                                verticalHookPositions[i] -= (verticalHookPositions[i] > Projectile.Center.Y ? 32 : -32);
                            }
                            else
                            {
                                verticalHookPositions[i] = Projectile.Center.Y;
                            }
                        }
                        if (retracted)
                        {
                            Projectile.ai[0] = 2;
                        }
                        break;
                    case 2:
                        //moving horizontally
                        if (horizontalHookPositions[0] < Projectile.Center.X + 32 || horizontalHookPositions[1] > Projectile.Center.X - 32)
                        {
                            if (horizontalHookPositions[0] < Projectile.Center.X + 32 && horizontalHookPositions[1] > Projectile.Center.X - 32)
                            {
                                Projectile.ai[0] = 0;
                                Projectile.velocity.X = 0;
                            }
                            else if (horizontalHookPositions[0] < Projectile.Center.X + 32)
                            {
                                Projectile.velocity.X = -8;
                            }
                            else
                            {
                                Projectile.velocity.X = 8;
                            }
                            if (Main.rand.NextBool())
                            {
                                Projectile.ai[0] = 3;
                                Projectile.velocity.X = 0;
                            }
                            Projectile.netUpdate = true;
                        }
                        if ((Math.Abs(goalPoint.X - Projectile.Center.X) < 8 && Main.rand.NextBool()) || Main.rand.NextBool(240))
                        {
                            Projectile.ai[0] = 3;
                            Projectile.velocity.X = 0;
                        }
                        if (Projectile.ai[0] == 2 && Projectile.velocity.X == 0)
                        {
                            Projectile.velocity.X = (goalPoint.X > Projectile.Center.X) ? 8 : -8;
                        }
                        Projectile.netUpdate = true;
                        break;
                    case 3:
                        //extending vertical hooks
                        attached = true;
                        for (int i = 0; i < 2; i++)
                        {
                            Tile tile = Main.tile[(int)(Projectile.Center.X / 16), (int)(verticalHookPositions[i] / 16)];
                            if (!(Main.tileSolid[tile.TileType] && !Main.tileSolidTop[tile.TileType]) || !tile.HasTile)
                            {
                                attached = false;
                                verticalHookPositions[i] += (i == 0 ? 16 : -16);
                            }
                        }
                        if (attached || Math.Abs(verticalHookPositions[0] - Projectile.Center.Y) > 400 || Math.Abs(verticalHookPositions[1] - Projectile.Center.Y) > 400)
                        {
                            Projectile.ai[0] = 4;
                        }
                        break;
                    case 4:
                        //retracting horizontal hooks
                        retracted = true;
                        for (int i = 0; i < 2; i++)
                        {
                            if (Math.Abs(horizontalHookPositions[i] - Projectile.Center.X) > 32)
                            {
                                retracted = false;
                                horizontalHookPositions[i] -= (horizontalHookPositions[i] > Projectile.Center.X ? 32 : -32);
                            }
                            else
                            {
                                horizontalHookPositions[i] = Projectile.Center.X;
                            }
                        }
                        if (retracted)
                        {
                            Projectile.ai[0] = 5;
                        }
                        break;
                    case 5:
                        //moving vertically
                        if (verticalHookPositions[0] < Projectile.Center.Y + 32 || verticalHookPositions[1] > Projectile.Center.Y - 32)
                        {
                            if (verticalHookPositions[0] < Projectile.Center.Y + 32 && verticalHookPositions[1] > Projectile.Center.Y - 32)
                            {
                                Projectile.ai[0] = 0;
                                Projectile.velocity.Y = 0;
                            }
                            else if (verticalHookPositions[0] < Projectile.Center.Y + 32)
                            {
                                Projectile.velocity.Y = -8;
                            }
                            else
                            {
                                Projectile.velocity.Y = 8;
                            }
                            if (Main.rand.NextBool())
                            {
                                Projectile.ai[0] = 0;
                                Projectile.velocity.Y = 0;
                            }
                            Projectile.netUpdate = true;
                        }
                        if ((Math.Abs(goalPoint.Y - Projectile.Center.Y) < 8 && Main.rand.NextBool()) || Main.rand.NextBool(240))
                        {
                            Projectile.ai[0] = 0;
                            Projectile.velocity.Y = 0;
                        }
                        if (Projectile.ai[0] == 5 && Projectile.velocity.Y == 0)
                        {
                            Projectile.velocity.Y = (goalPoint.Y > Projectile.Center.Y) ? 8 : -8;
                        }
                        Projectile.netUpdate = true;
                        break;
                }
            }
            else
            {
                Projectile.ai[0] = 0;

                Projectile.velocity = Vector2.Zero;

                for (int i = 0; i < 2; i++)
                {
                    if (Math.Abs(verticalHookPositions[i] - Projectile.Center.Y) > 32)
                    {
                        verticalHookPositions[i] -= (verticalHookPositions[i] > Projectile.Center.Y ? 32 : -32);
                    }
                    else
                    {
                        verticalHookPositions[i] = Projectile.Center.Y;
                    }
                }
                for (int i = 0; i < 2; i++)
                {
                    if (Math.Abs(horizontalHookPositions[i] - Projectile.Center.X) > 32)
                    {
                        horizontalHookPositions[i] -= (horizontalHookPositions[i] > Projectile.Center.X ? 32 : -32);
                    }
                    else
                    {
                        horizontalHookPositions[i] = Projectile.Center.X;
                    }
                }
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            var legTexture = ModContent.Request<Texture2D>($"{Texture}Chain").Value;
            var texture = ModContent.Request<Texture2D>($"{Texture}Hook").Value;

            for (int i = 0; i < 2; i++)
            {
                Vector2 constructCenter = Projectile.Center;
                Vector2 center = new Vector2(Projectile.Center.X, verticalHookPositions[i]);
                Vector2 distToprojectile = constructCenter - center;
                float projRotation = distToprojectile.ToRotation() - MathHelper.PiOver2;
                float distance = distToprojectile.Length();
                while (distance > 8f && !float.IsNaN(distance))
                {
                    distToprojectile.Normalize();                 //get unit vector
                    distToprojectile *= 8f;                      //speed = 24
                    center += distToprojectile;                   //update draw position
                    distToprojectile = constructCenter - center;    //update distance
                    distance = distToprojectile.Length();

                    Main.spriteBatch.Draw(legTexture, new Vector2(center.X - Main.screenPosition.X, center.Y - Main.screenPosition.Y),
                        new Rectangle(0, 0, 6, 8), Color.White, projRotation,
                        new Vector2(6 * 0.5f, 8 * 0.5f), 1f, SpriteEffects.None, 0f);
                }

                Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, texture.Height * 0.5f);
                center = new Vector2(Projectile.Center.X, verticalHookPositions[i]);
                Vector2 drawPos = center - Main.screenPosition;
                Main.spriteBatch.Draw(texture, drawPos, new Rectangle(0, 0, texture.Width, texture.Height), Lighting.GetColor((int)(center.X / 16), (int)(center.Y / 16)), projRotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0f);
            }
            for (int i = 0; i < 2; i++)
            {
                Vector2 constructCenter = Projectile.Center;
                Vector2 center = new Vector2(horizontalHookPositions[i], Projectile.Center.Y);
                Vector2 distToprojectile = constructCenter - center;
                float projRotation = distToprojectile.ToRotation() - MathHelper.PiOver2;
                float distance = distToprojectile.Length();
                while (distance > 8f && !float.IsNaN(distance))
                {
                    //Draw chain

                    distToprojectile.Normalize();                 //get unit vector
                    distToprojectile *= 8f;                      //speed = 24
                    center += distToprojectile;                   //update draw position
                    distToprojectile = constructCenter - center;    //update distance
                    distance = distToprojectile.Length();

                    Main.spriteBatch.Draw(legTexture, new Vector2(center.X - Main.screenPosition.X, center.Y - Main.screenPosition.Y),
                        new Rectangle(0, 0, 6, 8), Color.White, projRotation,
                        new Vector2(6 * 0.5f, 8 * 0.5f), 1f, SpriteEffects.None, 0f);
                }

                Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, texture.Height * 0.5f);
                center = new Vector2(horizontalHookPositions[i], Projectile.Center.Y);
                Vector2 drawPos = center - Main.screenPosition;
                Main.spriteBatch.Draw(texture, drawPos, new Rectangle(0, 0, texture.Width, texture.Height), Lighting.GetColor((int)(center.X / 16), (int)(center.Y / 16)), projRotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0f);
            }
            return true;
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            for (int i = 0; i < 2; i++)
            {
                writer.Write(verticalHookPositions[i]);
                writer.Write(horizontalHookPositions[i]);
            }
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            for (int i = 0; i < 4; i++)
            {
                verticalHookPositions[i] = reader.ReadSingle();
                horizontalHookPositions[i] = reader.ReadSingle();
            }
        }
    }
}