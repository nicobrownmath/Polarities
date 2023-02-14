using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Polarities.Items.Pets
{
    public class StarConstructPetItem : ModItem
    {
        public override void SetStaticDefaults()
        {
            SacrificeTotal = (1);
        }

        public override void SetDefaults()
        {
            Item.DefaultToVanitypet(ProjectileType<StarConstructPet>(), BuffType<StarConstructPetBuff>());

            Item.width = 26;
            Item.height = 20;

            Item.value = Item.sellPrice(gold: 5);
            Item.rare = ItemRarityID.Master;
            Item.master = true;
        }

        public override void UseStyle(Player player, Rectangle heldItemFrame)
        {
            if (player.whoAmI == Main.myPlayer && player.itemTime == 0)
            {
                player.AddBuff(Item.buffType, 3600, true);
            }
        }
    }

    public class StarConstructPetBuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.buffNoTimeDisplay[Type] = true;
            Main.vanityPet[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            player.buffTime[buffIndex] = 18000;
            int projType = ModContent.ProjectileType<StarConstructPet>();
            if (player.whoAmI == Main.myPlayer && player.ownedProjectileCounts[projType] <= 0)
            {
                Projectile.NewProjectile(player.GetSource_Buff(buffIndex), player.Center, Vector2.Zero, projType, 0, 0f, player.whoAmI);
            }
        }
    }

    public class StarConstructPet : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 19;
            Main.projPet[Projectile.type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.width = 34;
            Projectile.height = 34;

            DrawOffsetX = -8;
            DrawOriginOffsetY = -16;
            DrawOriginOffsetX = 0;

            Projectile.penetrate = -1;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            if (!player.active)
            {
                Projectile.active = false;
                return;
            }
            if (player.dead)
            {
                player.ClearBuff(BuffType<StarConstructPetBuff>());
            }
            if (player.HasBuff(BuffType<StarConstructPetBuff>()))
            {
                Projectile.timeLeft = 2;
            }

            switch (Projectile.ai[0])
            {
                case 0:
                    //standard walking/staying still behavior

                    Projectile.tileCollide = true;
                    Projectile.rotation = 0f;

                    if ((Projectile.Center - player.MountedCenter).Length() > 750)
                    {
                        //switch to case 1 if too far away
                        Projectile.ai[0] = 1;
                    }
                    else if (Math.Abs(player.MountedCenter.X - Projectile.Center.X) < 64)
                    {
                        Projectile.velocity.X *= 0.93f;

                        Projectile.spriteDirection = 1;

                        if (Projectile.frame == 1 || Projectile.frame == 2 || Projectile.frame == 3 || Projectile.frame == 4)
                        {
                            Projectile.frame = 0;
                            Projectile.frameCounter = 0;
                        }
                        else if (Projectile.frame == 0)
                        {
                            Projectile.frameCounter++;
                            if (Projectile.frameCounter >= 600)
                            {
                                Projectile.frameCounter = 0;
                                Projectile.frame = 5;
                            }
                        }
                        else
                        {
                            Projectile.frameCounter++;
                            if (Projectile.frameCounter >= 5)
                            {
                                Projectile.frameCounter = 0;
                                Projectile.frame++;
                                if (Projectile.frame == 19)
                                {
                                    Projectile.frame = 12;
                                }
                            }
                        }
                    }
                    else
                    {
                        if (player.MountedCenter.X - Projectile.Center.X < 0 && Projectile.velocity.X > -6)
                        {
                            if (Projectile.velocity.Y == 0 && Projectile.velocity.X == 0 && (Projectile.Center - player.MountedCenter).Length() > 72)
                            {
                                Projectile.velocity.Y = -12f;
                            }
                            Projectile.velocity.X -= 0.25f;
                            if (Projectile.velocity.X < -6)
                            {
                                Projectile.velocity.X = -6;
                            }
                        }
                        else if (Projectile.velocity.X < 6)
                        {
                            if (Projectile.velocity.Y == 0 && Projectile.velocity.X == 0 && (Projectile.Center - player.MountedCenter).Length() > 72)
                            {
                                Projectile.velocity.Y = -12f;
                            }
                            Projectile.velocity.X += 0.25f;
                            if (Projectile.velocity.X > 6)
                            {
                                Projectile.velocity.X = 6;
                            }
                        }

                        if (Projectile.frame != 1 && Projectile.frame != 2 && Projectile.frame != 3)
                        {
                            Projectile.frameCounter = 0;
                            Projectile.frame = 1;
                        }
                        Projectile.spriteDirection = Projectile.velocity.X > 0 ? 1 : -1;
                        Projectile.frameCounter++;
                        if (Projectile.frameCounter >= 3)
                        {
                            Projectile.frameCounter = 0;
                            Projectile.frame++;
                            if (Projectile.frame == 4)
                            {
                                Projectile.frame = 1;
                            }
                        }
                    }

                    if (Projectile.velocity.Y < 16) { Projectile.velocity.Y += 0.5f; }

                    break;
                case 1:
                    //fly to the player if they're too far
                    Projectile.frameCounter = 0;
                    Projectile.frame = 4;

                    Projectile.tileCollide = false;

                    Vector2 goalPosition = player.MountedCenter + new Vector2(0, -64);
                    Vector2 goalVelocity = player.velocity / 2f + (goalPosition - Projectile.Center) / 30f;
                    Projectile.velocity += (goalVelocity - Projectile.velocity) / 30f;

                    Projectile.rotation += Projectile.velocity.X * 0.1f;

                    if ((Projectile.Center - goalPosition).Length() < 72)// && player.velocity.Y == 0)
                    {
                        Projectile.ai[0] = 0;
                    }

                    break;
            }
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (Projectile.velocity.X != oldVelocity.X)
            {
                Projectile.velocity.X = 0;
            }
            if (Projectile.velocity.Y != oldVelocity.Y)
            {
                if (Projectile.velocity.X != oldVelocity.X && (Projectile.Center - Main.player[Projectile.owner].Center).Length() > 64)
                {
                    Projectile.velocity.Y = -12f;
                }
                else
                {
                    Projectile.velocity.Y = 0;
                }
            }
            return false;
        }
    }
}

