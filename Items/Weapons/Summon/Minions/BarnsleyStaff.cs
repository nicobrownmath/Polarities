using Microsoft.Xna.Framework;
using Polarities.NPCs.Enemies.Fractal;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Polarities.Items.Weapons.Summon.Minions
{
    public class BarnsleyStaff : ModItem
    {
        public override void SetStaticDefaults()
        {
            SacrificeTotal = 1;
            PolaritiesItem.IsFractalWeapon.Add(Type);
        }

        public override void SetDefaults()
        {
            Item.damage = 40;
            Item.DamageType = DamageClass.Summon;
            Item.width = 48;
            Item.height = 48;
            Item.mana = 5;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.useStyle = 1;
            Item.noMelee = true;
            Item.knockBack = 2f;
            Item.value = 10000;
            Item.rare = ItemRarityID.Pink;
            Item.UseSound = SoundID.Item44;
            Item.autoReuse = true;
            Item.buffType = ModContent.BuffType<BarnsleyFernBuff>();
            Item.shoot = ModContent.ProjectileType<BarnsleyFernMinion>();
            Item.shootSpeed = 0f;
        }

        public override void ModifyManaCost(Player player, ref float reduce, ref float mult)
        {
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            position = Main.MouseWorld;
            player.AddBuff(Item.buffType, 18000, true);
            return true;
        }
    }

    public class BarnsleyFernBuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.buffNoTimeDisplay[Type] = true;
            Main.buffNoSave[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            if (player.ownedProjectileCounts[ModContent.ProjectileType<BarnsleyFernMinion>()] > 0)
            {
                player.buffTime[buffIndex] = 18000;
            }
            else
            {
                player.DelBuff(buffIndex);
                buffIndex--;
            }
        }
    }

    public class BarnsleyFernMinion : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Barnsley Fern");
            Main.projFrames[Type] = 6;
            Main.projPet[Type] = true;
            ProjectileID.Sets.MinionSacrificable[Type] = true;
            ProjectileID.Sets.CultistIsResistantTo[Type] = true;
            ProjectileID.Sets.MinionTargettingFeature[Type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 50;
            Projectile.penetrate = -1;
            Projectile.minion = true;
            Projectile.minionSlots = 1f;
            Projectile.friendly = true;
            Projectile.tileCollide = true;

            //projectile.ai[0] = 240;
        }

        public override void AI()
        {
            Projectile.frameCounter++;
            if (Projectile.frameCounter == 4)
            {
                Projectile.frameCounter = 0;
                Projectile.frame = (Projectile.frame + 1) % Main.projFrames[Projectile.type];
            }

            Player player = Main.player[Projectile.owner];
            if (!player.active)
            {
                Projectile.active = false;
                return;
            }
            if (player.dead)
            {
                player.ClearBuff(ModContent.BuffType<BarnsleyFernBuff>());
            }
            if (player.HasBuff(ModContent.BuffType<BarnsleyFernBuff>()))
            {
                Projectile.timeLeft = 2;
            }

            int targetID = -1;
            Projectile.Minion_FindTargetInRange(750, ref targetID, skipIfCannotHitWithOwnBody: true);
            NPC target = null;
            if (targetID != -1)
            {
                target = Main.npc[targetID];
            }

            Vector2 targetCenter = player.Center;

            if (target != null)
            {
                targetCenter = target.Center;
            }

            if (Projectile.ai[0] == 0 || (Projectile.Center - player.Center).Length() > 1000)
            {
                Projectile.ai[0] = 240;
                //teleport nearby but not on top of npc
                bool attemptSuccessful = false;

                Teleport(targetCenter, ref attemptSuccessful);

                if (!attemptSuccessful)
                {
                    return;
                }
            }
            else if (Projectile.ai[0] == 180 || Projectile.ai[0] == 150 || Projectile.ai[0] == 120)
            {
                //shoot projectile at player
                if (target != null)
                {
                    float v = 24;
                    /*v*t*Direction+npc.Center = player.velocity*t+player.Center
                    v*t*cos(theta)+npc.Center.X = player.velocity.X*t+player.Center.X
                    v*t*sin(theta)+npc.Center.Y = player.velocity.Y*t+player.Center.Y

                    v*t*cos(theta)-player.velocity.X*t = player.Center.X-npc.Center.X
                    t = (player.Center.X-npc.Center.X)/(v*cos(theta)-player.velocity.X)

                    (v*sin(theta)-player.velocity.Y)*(player.Center.X-npc.Center.X) = (v*cos(theta)-player.velocity.X)*(player.Center.Y-npc.Center.Y)*/

                    float a = target.velocity.Y;
                    float b = target.velocity.X;
                    float c = (target.Center.X - Projectile.Center.X);
                    float d = (target.Center.Y - Projectile.Center.Y + Projectile.height / 2);

                    float theta = 2 * (float)Math.Atan2(c * v - Math.Sqrt(
                        -a * a * c * c + 2 * a * b * c * d - b * b * d * d + v * v * (c * c + d * d)
                    ),
                    a * c - d * (b + v));

                    Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center - new Vector2(0, Projectile.height / 2), new Vector2(v / 2, 0).RotatedBy(theta), ModContent.ProjectileType<FractalFrondFriendly>(), Projectile.damage, Projectile.knockBack, Projectile.owner);
                }
            }
            Projectile.ai[0]--;
        }

        private void Teleport(Vector2 targetCenter, ref bool attemptSuccessful)
        {
            //try up to 20 times
            for (int i = 0; i < 40; i++)
            {
                Vector2 tryGoalPoint = targetCenter + new Vector2(-Projectile.width / 2 + Main.rand.NextFloat(100f, 500f) * (Main.rand.Next(2) * 2 - 1), Main.rand.NextFloat(-500f, 500f));
                tryGoalPoint.Y = 16 * (int)(tryGoalPoint.Y / 16);
                tryGoalPoint -= new Vector2(0, Projectile.height);

                bool viable = true;

                for (int x = (int)((tryGoalPoint.X) / 16); x <= (int)((tryGoalPoint.X + Projectile.width) / 16); x++)
                {
                    for (int y = (int)((tryGoalPoint.Y) / 16); y <= (int)((tryGoalPoint.Y + Projectile.height) / 16); y++)
                    {
                        if (Main.tile[x, y].HasTile)
                        {
                            viable = false;
                            break;
                        }
                    }
                    if (!viable)
                    {
                        break;
                    }
                }

                if (viable)
                {
                    for (int y = (int)((tryGoalPoint.Y + Projectile.height) / 16); y < (int)((tryGoalPoint.Y + Projectile.height) / 16) + 100; y++)
                    {
                        int x = (int)((tryGoalPoint.X + Projectile.width / 2) / 16);
                        if (Main.tile[x, y].HasTile && (Main.tileSolid[Main.tile[x, y].TileType] || Main.tileSolid[Main.tile[x, y].TileType]))
                        {
                            for (int a = 0; a < 12; a++)
                            {
                                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Electric, newColor: Color.LightBlue, Scale: 1.4f);
                            }

                            Projectile.position = new Vector2(tryGoalPoint.X, y * 16 - Projectile.height);

                            for (int a = 0; a < 12; a++)
                            {
                                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Electric, newColor: Color.LightBlue, Scale: 1.4f);
                            }
                            break;
                        }
                    }
                    attemptSuccessful = true;
                    break;
                }
            }
            Projectile.netUpdate = true;
        }

        public override bool? CanCutTiles()
        {
            return false;
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            return false;
        }

        public override bool MinionContactDamage()
        {
            return false;
        }
    }

    public class FractalFrondFriendly : ModProjectile
    {
        public override string Texture => $"{ModContent.GetInstance<FractalFrond>().Texture}";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Fractal Frond");
            ProjectileID.Sets.MinionShot[Type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 600;
            Projectile.tileCollide = true;
            Projectile.extraUpdates = 1;
        }

        public override void AI()
        {
            if (Main.rand.NextBool(3))
            {
                Main.dust[Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Electric, newColor: Color.LightBlue, Scale: 1f)].noGravity = true;
            }

            Projectile.rotation = Projectile.velocity.ToRotation() + (float)Math.PI / 2;
        }

        public override bool? CanCutTiles()
        {
            return false;
        }

        public override void Kill(int timeLeft)
        {
            for (int i = 0; i < 4; i++)
            {
                Main.dust[Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Electric, newColor: Color.LightBlue, Scale: 1f)].noGravity = true;
            }
        }
    }
}