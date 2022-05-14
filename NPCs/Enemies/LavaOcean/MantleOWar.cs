using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using Polarities.Projectiles;
using Polarities.Buffs;
using Polarities.Items;
using Polarities.Items.Placeable;
using Polarities.Tiles;
using Polarities.Items.Weapons;
using Polarities.Items.Armor;
using Polarities.Items.Placeable.Banners;
using Terraria.GameContent.Bestiary;
using Polarities.Biomes;
using Polarities.Items.Placeable.Blocks;
using Terraria.Audio;
using Polarities.Items.Placeable.Walls;
using Terraria.GameContent.ItemDropRules;
using Polarities.Items.Consumables;
using Terraria.GameContent;
using ReLogic.Content;
using Polarities.Items.Accessories;
using Terraria.DataStructures;
using Polarities.Items.Materials;
using Terraria.ModLoader.Utilities;
using System.Collections.Generic;
using Polarities.Effects;

namespace Polarities.NPCs.Enemies.LavaOcean
{
	public class MantleOWar : ModNPC
	{
        public override void SetStaticDefaults()
        {
            Polarities.customNPCGlowMasks[Type] = TextureAssets.Npc[Type];
            PolaritiesNPC.canSpawnInLava.Add(Type);

            NPCDebuffImmunityData debuffData = new NPCDebuffImmunityData
            {
                SpecificallyImmuneTo = new int[] {
                BuffID.Confused,
                BuffID.OnFire,
                BuffID.Frostburn,
                BuffID.OnFire3,
                BuffID.ShadowFlame,
                BuffID.CursedInferno,
                //BuffType<Incinerating>(),
            }
            };
            NPCID.Sets.DebuffImmunitySets.Add(Type, debuffData);
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
            //spawn conditions
			BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.TheUnderworld,
			//flavor text
			this.TranslatedBestiaryEntry()
        });
        }

        public override void SetDefaults()
        {
            NPC.aiStyle = -1;
            NPC.width = 48;
            NPC.height = 32;
            DrawOffsetY = 0;

            NPC.defense = 30;
            NPC.damage = 60;
            NPC.lifeMax = 1200;
            NPC.knockBackResist = 0f;

            NPC.npcSlots = 1f;

            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.value = Item.buyPrice(gold: 1);

            NPC.lavaImmune = true;
            NPC.noGravity = true;
            NPC.noTileCollide = true;

            NPC.hide = true;

            Banner = Type;
            //TODO: BannerItem = ItemType<MantleOWarBanner>();

            SpawnModBiomes = new int[1] { GetInstance<Biomes.LavaOcean>().Type };

            tentacles = new MantleOWarTentacle[NUM_TENTACLES];
        }

        MantleOWarTentacle[] tentacles;
        const int NUM_TENTACLES = 8;
        const int TENTACLE_LENGTH = 16;

        public override void OnSpawn(IEntitySource source)
        {
            NPC.direction = Main.rand.NextBool() ? 1 : -1;

            for (int i = 0; i < NUM_TENTACLES; i++)
            {
                tentacles[i].Initialize(NPC.Center, TENTACLE_LENGTH, MathHelper.TwoPi * i / (float)NUM_TENTACLES);
            }
        }

        public override void AI()
        {
            Vector2 CenterWithOffsetForFloatBobbing = NPC.Center + new Vector2(0, (float)Math.Cos(NPC.ai[0] * 0.1f) * 4);

            Tile centerTile = Framing.GetTileSafely(CenterWithOffsetForFloatBobbing.ToTileCoordinates());

            int currentRequiredLiquidAmountToFloat = (int)((16 - (CenterWithOffsetForFloatBobbing.Y % 16)) * 16);

            if (centerTile.LiquidType == LiquidID.Lava && centerTile.LiquidAmount > currentRequiredLiquidAmountToFloat)
            {
                //occasionally switch directions
                if (Main.rand.NextBool(600))
                {
                    NPC.direction *= -1;
                }

                //test adjacent tiles in our direction, if it's invalid, switch directions
                Tile adjacentTile = Framing.GetTileSafely(CenterWithOffsetForFloatBobbing.ToTileCoordinates() + new Point(NPC.direction, 0));
                if (adjacentTile.HasUnactuatedTile && Main.tileSolid[adjacentTile.TileType])
                {
                    NPC.direction *= -1;
                }

                NPC.noGravity = true;

                NPC.velocity.X += 0.1f * NPC.direction;
                NPC.velocity.Y -= 0.1f;
                NPC.velocity *= 0.9f;
            }
            else if (centerTile.HasUnactuatedTile && Main.tileSolid[centerTile.TileType])
            {
                NPC.noGravity = true;

                //drift to side to find lava
                for (int i = 1; i < 20; i++)
                {
                    for (int j = -1; j <= 1; j += 2)
                    {
                        Tile testTileStart = Framing.GetTileSafely(CenterWithOffsetForFloatBobbing.ToTileCoordinates() + new Point(i * j, 0));

                        int verticalDirection = 1;
                        if (testTileStart.HasUnactuatedTile && Main.tileSolid[centerTile.TileType])
                        {
                            //search up
                            verticalDirection = -1;
                        }

                        for (int k = 0; k < 10; k++)
                        {
                            Tile testTile = Framing.GetTileSafely(CenterWithOffsetForFloatBobbing.ToTileCoordinates() + new Point(i * j, k * verticalDirection));

                            if (testTile.LiquidType == LiquidID.Lava && testTile.LiquidAmount > currentRequiredLiquidAmountToFloat)
                            {
                                NPC.direction = j;
                                goto exitLoop;
                            }
                            else if (testTile.HasUnactuatedTile && Main.tileSolid[centerTile.TileType] && verticalDirection == 1)
                            {
                                break;
                            }
                            else if (!testTile.HasUnactuatedTile && Main.tileSolid[centerTile.TileType] && verticalDirection != 1)
                            {
                                break;
                            }
                        }
                    }
                }

                exitLoop:

                NPC.velocity.X += 0.1f * NPC.direction;
                NPC.velocity.Y -= 0.1f;
                NPC.velocity *= 0.9f;
            }
            else
            {
                NPC.noGravity = false;

                NPC.velocity *= 0.9f;
            }

            NPC.spriteDirection = -NPC.direction;
            //float bobbing timer
            NPC.ai[0]++;

            NPC.TargetClosest(false);
            Player player = Main.player[NPC.target];

            //prevent despawning
            if (NPC.Distance(player.Center) < 4000)
            {
                NPC.timeLeft = NPC.activeTime;
            }

            //fire lava blobs at the player if they're above the NPC
            //we don't stop the barrage once it's started even if the player moves behind tiles or goes underneath
            bool canShoot = Framing.GetTileSafely(CenterWithOffsetForFloatBobbing.ToTileCoordinates() + new Point(0, -1)).LiquidAmount == 0 && !Framing.GetTileSafely(CenterWithOffsetForFloatBobbing.ToTileCoordinates() + new Point(0, -1)).HasUnactuatedTile;

            if (player.Center.Y < NPC.Center.Y && (NPC.ai[1] >= 120 || (canShoot && Collision.CanHitLine(NPC.position, NPC.width, NPC.height, player.position, player.width, player.height))))
            {
                if (NPC.ai[1] == 120 && Main.rand.NextBool()) NPC.ai[1] = 240;

                if (NPC.ai[1] >= 120 && NPC.ai[1] < 180 && NPC.ai[1] % 10 == 0)
                {
                    Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, (player.Center - NPC.Center).SafeNormalize(Vector2.Zero) * 4f, ProjectileType<MantleOWarLavaProjectile>(), 30, 4f, Main.myPlayer);
                }
                if (NPC.ai[1] == 240)
                {
                    for (int i = 0; i < 6; i++)
                    {
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, new Vector2(3, 0).RotatedBy((-i - 0.5f) * MathHelper.TwoPi / 12), ProjectileType<MantleOWarLavaProjectile>(), 30, 4f, Main.myPlayer);
                    }
                }

                NPC.ai[1]++;
                if (NPC.ai[1] == 180 || NPC.ai[1] == 300)
                {
                    NPC.ai[1] = 0;
                }
            }
            else
            {
                NPC.ai[1] = 0;
            }

            for (int i = 0; i < NUM_TENTACLES; i++)
            {
                tentacles[i].Update(NPC);
            }
        }

        private struct MantleOWarTentacle
        {
            Vector2[] positions;
            float progressAlongTentacle;
            float angle;
            int direction;

            public void Initialize(Vector2 startingPosition, int length, float angle)
            {
                positions = new Vector2[length + 1];
                for (int i = 0; i < positions.Length; i++)
                {
                    positions[i] = startingPosition;
                }
                this.angle = angle;
                direction = Main.rand.NextBool() ? 1 : -1;
                progressAlongTentacle = Main.rand.NextFloat();
            }

            public Vector2 GetNematocystPosition()
            {
                if (progressAlongTentacle == 1) return positions[positions.Length - 1];

                int index = (int)(progressAlongTentacle * (positions.Length - 1));
                float remainingProgress = (progressAlongTentacle * (positions.Length - 1)) % 1;
                return Vector2.Lerp(positions[index], positions[index + 1], remainingProgress);
            }

            public void Update(NPC npc)
            {
                angle += 0.001f;

                float horizontalForce = (float)Math.Sin(angle) * 0.5f;

                //update tentacle
                positions[0] = npc.Center + new Vector2(horizontalForce * 8, 8);
                for (int i = 1; i < positions.Length - 1; i++)
                {
                    Vector2 force = new Vector2(horizontalForce, (1 - i / (float)(positions.Length - 1))).SafeNormalize(Vector2.Zero) * 0.75f;
                    positions[i] = Vector2.Lerp(positions[i], (positions[i - 1] + positions[i + 1]) / 2, 0.1f) + force;
                }
                positions[positions.Length - 1] = Vector2.Lerp(positions[positions.Length - 1], positions[positions.Length - 2], 0.1f) + new Vector2(horizontalForce, 1).SafeNormalize(Vector2.Zero) * 0.75f;

                //update nematocyst
                Vector2 oldNematocystPosition = GetNematocystPosition();

                float distanceToMove = 4f;
                while (distanceToMove > 0)
                {
                    int goalIndex = (direction == 1) ?
                        (int)Math.Ceiling(progressAlongTentacle * (positions.Length - 1)) - 1 :
                        (int)Math.Floor(progressAlongTentacle * (positions.Length - 1)) + 1;
                    float distanceTo = (positions[goalIndex] - GetNematocystPosition()).Length();
                    float progressTo = (goalIndex - progressAlongTentacle * (positions.Length - 1)) / (positions.Length - 1);

                    if (distanceTo < distanceToMove)
                    {
                        //move along the entire segment and continue
                        progressAlongTentacle += progressTo;
                        distanceToMove -= distanceTo;

                        if (progressAlongTentacle <= 0)
                        {
                            progressAlongTentacle *= -1;
                            direction *= -1;
                        }
                        if (progressAlongTentacle >= 1)
                        {
                            progressAlongTentacle = 2 - progressAlongTentacle;
                            direction *= -1;
                        }
                        progressAlongTentacle = Math.Clamp(progressAlongTentacle, 0, 1);
                    }
                    else
                    {
                        //move as far as we can and break
                        progressAlongTentacle += progressTo * distanceToMove / distanceTo;
                        break;
                    }
                }

                Projectile.NewProjectile(npc.GetSource_FromAI(), GetNematocystPosition(), GetNematocystPosition() - oldNematocystPosition, ProjectileType<MantleOWarNematocyst>(), 30, 4f, Main.myPlayer);
            }

            public void Draw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor, NPC npc)
            {
                Color npcColor = npc.GetNPCColorTintedByBuffs(Color.White);
                for (int i = 0; i < positions.Length - 2; i++)
                {
                    if (positions[i + 1] != positions[i])
                    {
                        Color flameColor = ModUtils.ConvectiveFlameColor((float)Math.Pow((1 - i / (float)(positions.Length - 1)), 4) * 0.25f);
                        float tentacleWidth = 4f * (1 - i / (float)(positions.Length - 1));
                        spriteBatch.Draw(Textures.PixelTexture.Value, positions[i] - screenPos, Textures.PixelTexture.Frame(), npcColor.MultiplyRGB(flameColor), (positions[i + 1] - positions[i]).ToRotation(), new Vector2(0, 0.5f), new Vector2((positions[i + 1] - positions[i]).Length(), tentacleWidth), SpriteEffects.None, 0);
                    }
                }

                Color nematocystColor = ModUtils.ConvectiveFlameColor((float)Math.Pow(1 - progressAlongTentacle, 4) * 0.25f + 0.25f);
                spriteBatch.Draw(Textures.Glow58.Value, GetNematocystPosition() - screenPos, Textures.Glow58.Frame(), npcColor.MultiplyRGB(nematocystColor), 0f, Textures.Glow58.Size() / 2, 0.75f, SpriteEffects.None, 0);
            }
        }

        public override void OnHitPlayer(Player target, int damage, bool crit)
        {
            //TODO: Make this incineration
            target.AddBuff(BuffID.OnFire, 180, true);
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            for (int i = 0; i < NUM_TENTACLES; i++)
            {
                tentacles[i].Draw(spriteBatch, screenPos, drawColor, NPC);
            }

            return true;
        }

        public override bool CheckDead()
        {
            //TODO: Gores

            return true;
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            //TODO: Loot
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            //requires lava to spawn
            if (PolaritiesSystem.downedConvectiveWanderer && Biomes.LavaOcean.SpawnValid(spawnInfo, requireLava: true))
            {
                return 0.5f;
            }
            return 0f;
        }

        public override void DrawBehind(int index)
        {
            RenderTargetLayer.AddNPC<ConvectiveEnemyTarget>(index);
        }
    }

    public class MantleOWarLavaProjectile : ModProjectile
    {
        public override string Texture => "Polarities/Textures/Glow58";

        public override void SetDefaults()
        {
            Projectile.aiStyle = -1;
            Projectile.width = 36;
            Projectile.height = 36;
            Projectile.alpha = 0;
            Projectile.timeLeft = 600;
            Projectile.penetrate = 1;
            Projectile.hostile = true;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.light = 1f;
            Projectile.hide = false;
        }

        public override void OnSpawn(IEntitySource source)
        {
            SoundEngine.PlaySound(SoundID.NPCHit, Projectile.Center, 8);
        }

        public override void AI()
        {
            if (Projectile.ai[0] < 30)
                Projectile.velocity *= (float)Math.Pow(4, 1/30f);
            Projectile.ai[0]++;

            Projectile.rotation = Projectile.velocity.ToRotation();

            Main.dust[Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Lava, Scale: 1.5f)].noGravity = true;
        }

        public override void OnHitPlayer(Player target, int damage, bool crit)
        {
            //TODO: Make this incineration
            target.AddBuff(BuffID.OnFire, 180, true);
        }

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            DrawLayer.AddProjectile<DrawLayerAdditiveAfterProjectiles>(index);
        }

        public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac)
        {
            width = 2;
            height = 2;
            return true;
        }

        public override void Kill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.NPCHit, Projectile.Center, 9);
            for (int i = 0; i < 16; i++)
            {
                Main.dust[Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Lava, Scale: 1.5f)].noGravity = true;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            float scaleFactor = Projectile.velocity.Length() / 32f;
            Main.EntitySpriteDraw(TextureAssets.Projectile[Type].Value, Projectile.Center - Main.screenPosition, TextureAssets.Projectile[Type].Frame(), Color.OrangeRed, Projectile.rotation, TextureAssets.Projectile[Type].Size() / 2, new Vector2(1 + scaleFactor, 1) * Projectile.scale, SpriteEffects.None, 0);
            return false;
        }
    }

    public class MantleOWarNematocyst : ModProjectile
    {
        public override string Texture => "Polarities/Textures/Glow58";

        public override void SetDefaults()
        {
            Projectile.aiStyle = -1;
            Projectile.width = 27;
            Projectile.height = 27;
            Projectile.alpha = 0;
            Projectile.timeLeft = 1;
            Projectile.penetrate = 1;
            Projectile.hostile = true;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.light = 0.5f;
            Projectile.hide = true;
        }

        public override void OnHitPlayer(Player target, int damage, bool crit)
        {
            //TODO: Make this incineration
            target.AddBuff(BuffID.OnFire, 180, true);
        }

        public override bool ShouldUpdatePosition()
        {
            return false;
        }
    }
}
