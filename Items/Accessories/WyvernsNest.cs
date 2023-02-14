using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Polarities.Items.Accessories
{
    public class WyvernsNest : ModItem
    {
        public override void SetStaticDefaults()
        {
            SacrificeTotal = (1);
            PolaritiesItem.IsFlawless.Add(Type);
        }

        public override void SetDefaults()
        {
            Item.width = 50;
            Item.height = 40;
            Item.accessory = true;
            Item.damage = 40;
            Item.DamageType = DamageClass.Summon;

            Item.value = Item.sellPrice(gold: 20);
            Item.rare = RarityType<BetsyFlawlessRarity>();
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<PolaritiesPlayer>().wyvernsNestDamage = Item.damage;
        }
    }

    public class WyvernsNestMinion : ModProjectile
    {
        public override string Texture => "Terraria/Images/NPC_" + NPCID.DD2WyvernT1;

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 5;
            Main.projPet[Projectile.type] = true;
            ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true;
            ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.width = 26;
            Projectile.height = 26;
            Projectile.penetrate = -1;
            Projectile.minion = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.friendly = true;
            Projectile.tileCollide = false;
        }

        public override void OnSpawn(IEntitySource source)
        {
            //which type of wyvern are we
            Projectile.localAI[0] = Main.rand.Next(3);
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];

            if (player.GetModPlayer<PolaritiesPlayer>().wyvernsNestDamage <= 0)
            {
                Projectile.Kill();
                return;
            }

            Projectile.timeLeft = 2;

            if (Projectile.ai[0] > 0 && (!Main.projectile[(int)Projectile.ai[0] - 1].active || !Main.projectile[(int)Projectile.ai[0] - 1].sentry || Main.projectile[(int)Projectile.ai[0] - 1].owner != Projectile.owner))
            {
                //our focused sentry is no longer valid
                Projectile.ai[0] = 0;
            }

            int indexAsMinionFollowingPlayer = 0;
            int indexAsMinion = 0;
            for (int i = 0; i < Projectile.whoAmI; i++)
            {
                if (Main.projectile[i].active && Main.projectile[i].type == Type && Main.projectile[i].owner == Projectile.owner)
                {
                    indexAsMinion++;

                    if (indexAsMinion >= player.maxTurrets)
                    {
                        Projectile.Kill();
                        return;
                    }

                    if (Main.projectile[i].ai[0] == 0)
                        indexAsMinionFollowingPlayer++;
                }
            }

            if (Projectile.ai[0] <= 0)
            {
                HashSet<int> reservedSentries = new HashSet<int>();

                for (int i = 0; i < Main.maxProjectiles; i++)
                {
                    if (Main.projectile[i].active && Main.projectile[i].type == Type && Main.projectile[i].owner == Projectile.owner && Main.projectile[i].ai[0] > 0)
                    {
                        reservedSentries.Add((int)Main.projectile[i].ai[0] - 1);
                    }
                }

                for (int i = 0; i < Main.maxProjectiles; i++)
                {
                    if (Main.projectile[i].active && Main.projectile[i].sentry && Main.projectile[i].owner == Projectile.owner && !reservedSentries.Contains(i) && (Main.projectile[i].Center - player.Center).Length() > 600)
                    {
                        Projectile.ai[0] = i + 1;
                    }
                }
            }

            if (Projectile.ai[0] <= 0)
            {
                //no valid sentry, so shoot some weak fireballs at stuff or follow the player if no target is available
                int targetID = -1;
                Projectile.Minion_FindTargetInRange(600, ref targetID, true);

                if (targetID == -1)
                {
                    //player chasing
                    Vector2 goalPosition = player.Center + new Vector2(-player.direction * (indexAsMinionFollowingPlayer + 1) * 64, -32);
                    Vector2 goalVelocity = (goalPosition - Projectile.Center) / 15f / (float)Math.Sqrt(indexAsMinionFollowingPlayer + 1);
                    Projectile.velocity += (goalVelocity - Projectile.velocity) / 15f / (float)Math.Sqrt(indexAsMinionFollowingPlayer + 1);

                    Projectile.spriteDirection = (Projectile.velocity.X + player.direction) > 0 ? 1 : -1;

                    //we're by the player so set this ai flag
                    Projectile.ai[0] = 0;
                }
                else
                {
                    NPC target = Main.npc[targetID];
                    Projectile.localAI[1] = targetID;

                    int indexForTargetingNPCOnSide = 0;
                    for (int i = 0; i < Projectile.whoAmI; i++)
                    {
                        if (Main.projectile[i].active && Main.projectile[i].type == Type && Main.projectile[i].owner == Projectile.owner && Main.projectile[i].ai[0] < 0 && Main.projectile[i].localAI[1] == targetID)
                        {
                            if ((Projectile.Center.X > target.Center.X) == (Main.projectile[i].Center.X > target.Center.X))
                                indexForTargetingNPCOnSide++;
                        }
                    }

                    const float hoverDistWhileFireballing = 128;

                    int targetingDirection = (Projectile.Center.X > target.Center.X) ? 1 : -1;

                    Vector2 goalPosition = target.Center + new Vector2(targetingDirection * (indexForTargetingNPCOnSide + 0.5f) * 64, -hoverDistWhileFireballing);
                    Vector2 goalVelocity = (goalPosition - Projectile.Center) / 15f / (float)Math.Sqrt(indexForTargetingNPCOnSide + 1);
                    Projectile.velocity += (goalVelocity - Projectile.velocity) / 15f / (float)Math.Sqrt(indexForTargetingNPCOnSide + 1);

                    Projectile.spriteDirection = (Projectile.velocity.X + targetingDirection) > 0 ? 1 : -1;

                    //we're not by the player so this is used as an ai timer
                    Projectile.ai[0]--;
                    if (Projectile.ai[0] == -61)
                    {
                        Projectile.ai[0] = -1;
                        Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero) * 12f, ProjectileType<WyvernsNestMinionFireball>(), Projectile.damage, Projectile.knockBack, Projectile.owner);
                    }
                }

                Projectile.ai[1] = 0;
            }
            else
            {
                Projectile targetSentry = Main.projectile[(int)Projectile.ai[0] - 1];

                const float heightAboveSentry = 48;

                switch (Projectile.ai[1])
                {
                    case 0:
                        //go to sentry
                        Vector2 goalPosition = targetSentry.Center + new Vector2(0, -heightAboveSentry);
                        Vector2 goalVelocity = (goalPosition - Projectile.Center) / 15f;
                        Projectile.velocity += (goalVelocity - Projectile.velocity) / 15f;

                        if ((goalPosition - Projectile.Center).Length() < 8)
                        {
                            Projectile.ai[1] = 1f;
                        }
                        break;
                    case 1:
                        //return sentry to player
                        //accelerate slower because we're carrying a sentry
                        goalPosition = player.Center + new Vector2(0, -heightAboveSentry);
                        goalVelocity = (goalPosition - Projectile.Center) / 15f + player.velocity;
                        Projectile.velocity += (goalVelocity - Projectile.velocity) / 60f;

                        targetSentry.Center = Projectile.Center + new Vector2(0, heightAboveSentry) + Projectile.velocity;
                        targetSentry.velocity = Vector2.Zero;

                        if (targetSentry.Distance(player.Center) < 100)
                        {
                            //drop sentry
                            Projectile.ai[0] = 0f;
                        }
                        break;
                }

                Projectile.spriteDirection = Projectile.velocity.X > 0 ? 1 : -1;
            }

            if (Projectile.velocity.Length() > 32)
            {
                Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.Zero) * 32;
            }

            Projectile.rotation = (float)Math.Atan(Projectile.velocity.X * 0.5f) * 0.5f;

            Projectile.frameCounter++;
            if (Projectile.frameCounter >= 5)
            {
                Projectile.frameCounter = 0;
                Projectile.frame++;
                if (Projectile.frame >= 4)
                {
                    Projectile.frame = 0;
                }
            }
        }

        public override bool? CanCutTiles()
        {
            return false;
        }

        public override bool? CanDamage()
        {
            return false;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Asset<Texture2D> TextureAsset = TextureAssets.Npc[NPCID.DD2WyvernT1 + (int)Projectile.localAI[0]];

            if (!TextureAsset.IsLoaded)
            {
                Request<Texture2D>("Terraria/Images/NPC_" + (NPCID.DD2WyvernT1 + (int)Projectile.localAI[0]), AssetRequestMode.ImmediateLoad);
            }

            Rectangle frame = TextureAsset.Frame(1, Main.projFrames[Type], 0, Projectile.frame);
            Main.EntitySpriteDraw(TextureAsset.Value, Projectile.Center - Main.screenPosition, frame, Lighting.GetColor(Projectile.Center.ToTileCoordinates()), Projectile.rotation, frame.Size() / 2, Projectile.scale, Projectile.spriteDirection == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0);

            return false;
        }
    }

    public class WyvernsNestMinionFireball : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.ImpFireball;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.SentryShot[Type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.aiStyle = -1;
            Projectile.width = 16;
            Projectile.height = 16;

            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.penetrate = 1;
            Projectile.light = 0.9f;
            Projectile.tileCollide = true;
            Projectile.hide = false;
            Projectile.timeLeft = 600;
        }

        public override void AI()
        {
            //vanilla AI for hostile cursed flames
            Vector2 position58 = new Vector2(Projectile.position.X + Projectile.velocity.X, Projectile.position.Y + Projectile.velocity.Y);
            int width31 = Projectile.width;
            int height31 = Projectile.height;
            float x13 = Projectile.velocity.X;
            float y9 = Projectile.velocity.Y;
            Color newColor4 = default(Color);
            int num2467 = Dust.NewDust(position58, width31, height31, 6, x13, y9, 100, newColor4, 3f * Projectile.scale);
            Main.dust[num2467].noGravity = true;

            Projectile.rotation += 0.3f * Projectile.direction;
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            target.AddBuff(BuffID.OnFire, 60);
            Projectile.Kill();
        }

        public override void Kill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Dig, Projectile.Center);
            Collision.HitTiles(Projectile.position + Projectile.velocity, Projectile.velocity, Projectile.width, Projectile.height);
        }
    }
}

