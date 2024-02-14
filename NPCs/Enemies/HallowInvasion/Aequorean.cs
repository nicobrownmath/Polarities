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
using System.Collections.Generic;
using Polarities.Biomes;
using Terraria.GameContent.Bestiary;
using Terraria.DataStructures;
using Terraria.GameContent.ItemDropRules;
using Polarities.Dusts;
using Terraria.GameContent;
using ReLogic.Content;
using Polarities.Items.Weapons.Summon.Minions;

namespace Polarities.NPCs.Enemies.HallowInvasion
{
    public class Aequorean : ModNPC
    {
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 6;

            NPCID.Sets.TrailCacheLength[NPC.type] = 12;
            NPCID.Sets.TrailingMode[NPC.type] = 3;

            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Confused] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.OnFire] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.OnFire3] = true;

            PolaritiesNPC.customNPCCapSlot[Type] = NPCCapSlotID.HallowInvasion;
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
                //spawn conditions
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.TheHallow,
				//flavor text
				this.TranslatedBestiaryEntry()
            });
        }

        public override void SetDefaults()
        {
            NPC.aiStyle = -1;
            NPC.width = 48;
            NPC.height = 48;

            NPC.defense = 20;
            NPC.damage = 50;
            NPC.lifeMax = 900;
            NPC.knockBackResist = 0f;

            NPC.npcSlots = 1f;

            NPC.HitSound = SoundID.NPCHit5;
            NPC.DeathSound = SoundID.NPCDeath7;
            NPC.value = Item.buyPrice(silver: 25);

            NPC.lavaImmune = true;
            NPC.noGravity = true;
            NPC.noTileCollide = true;

            Music = GetInstance<Biomes.HallowInvasion>().Music;
            SceneEffectPriority = SceneEffectPriority.Event;

            Banner = Type;
            BannerItem = ItemType<AequoreanBanner>();

            SpawnModBiomes = new int[1] { GetInstance<Biomes.HallowInvasion>().Type };
        }

        public override void AI()
        {
            if (!PolaritiesSystem.hallowInvasion)
            {
                NPC.ai[0] = -1;
            }

            //motion code
            NPC.TargetClosest(false);

            if (NPC.ai[0] >= 0)
            {
                Vector2 velMod = (Main.player[NPC.target].Center - NPC.Center).SafeNormalize(Vector2.Zero) * (float)Math.Pow((60 + NPC.ai[0]) / 180f, 2);
                velMod.Y *= 2;
                NPC.velocity += 0.05f * velMod;

                NPC.ai[0]++;
                if (NPC.ai[0] >= 120)
                {
                    NPC.ai[0] = 0;

                    //modify timers for other aequoreans to keep them desynced
                    for (int i = 0; i < Main.maxNPCs; i++)
                    {
                        if (i != NPC.whoAmI && Main.npc[i].active && Main.npc[i].type == NPC.type && Main.npc[i].ai[0] <= 120 && Main.npc[i].ai[0] > 60)
                        {
                            Main.npc[i].ai[0] = 60;
                        }
                    }

                    bool doTeleport = false;
                    if (Main.rand.NextBool(4))
                    {
                        List<int> candidates = new List<int>();

                        for (int i = 0; i < Main.maxProjectiles; i++)
                        {
                            if (Main.projectile[i].active && Main.projectile[i].type == ProjectileType<AequoreanImage>())
                            {
                                if (Main.projectile[i].Distance(Main.player[NPC.target].Center) < NPC.Distance(Main.player[NPC.target].Center))
                                {
                                    candidates.Add(i);
                                }
                            }
                        }

                        if (candidates.Count > 0)
                        {
                            doTeleport = true;

                            int i = candidates[Main.rand.Next(candidates.Count)];

                            //swap positions and velocities
                            Vector2 oldNPCCenter = NPC.Center;
                            Vector2 oldNPCVelocity = NPC.velocity;
                            NPC.Center = Main.projectile[i].Center;
                            NPC.velocity = Main.projectile[i].velocity;
                            Main.projectile[i].Center = oldNPCCenter;
                            Main.projectile[i].velocity = oldNPCVelocity;
                        }
                    }

                    if (!doTeleport)
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, NPC.velocity, ProjectileType<AequoreanImage>(), 25, 2f, Main.myPlayer, Main.player[NPC.target].whoAmI);
                }
            }
            else
            {
                //flee
                Vector2 velMod = (NPC.Center - Main.player[NPC.target].Center).SafeNormalize(Vector2.Zero) * (float)Math.Pow((60 + NPC.ai[0]) / 180f, 2);
                velMod.Y *= 2;
                NPC.velocity += 0.05f * velMod;
            }

            NPC.velocity *= (1 - 0.01f * (float)Math.Pow((60 + NPC.ai[0]) / 180f, 2));

            NPC.rotation = (float)Math.Atan(NPC.velocity.X * 0.1f);
        }

        public override void FindFrame(int frameHeight)
        {
            NPC.frameCounter++;
            if (NPC.frameCounter == 5)
            {
                NPC.frameCounter = 0;
                NPC.frame.Y = (NPC.frame.Y + frameHeight) % (6 * frameHeight);
            }
        }

        public static Asset<Texture2D> TentaclesTexture;

        public override void Load()
        {
            TentaclesTexture = Request<Texture2D>(Texture + "_Tentacles");
        }

        public override void Unload()
        {
            TentaclesTexture = null;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Vector2 origin = new Vector2(54, 27);
            Texture2D texture = TextureAssets.Npc[NPCType<Aequorean>()].Value;
            Texture2D glowTexture = TextureAssets.Projectile[Type].Value;
            Texture2D tentacleTexture = TentaclesTexture.Value;
            const int numTentaclePairs = 3;

            for (int i = 1; i < NPC.oldPos.Length; i++)
            {
                Color color = NPC.GetNPCColorTintedByBuffs(new Color(220, 240, 255) * (1 - i / (float)NPC.oldPos.Length) * 0.25f);

                for (int j = 0; j < numTentaclePairs; j++)
                {
                    spriteBatch.Draw(tentacleTexture, NPC.oldPos[i] - NPC.position + NPC.Center - screenPos, NPC.frame, color * 0.5f, NPC.rotation, origin, new Vector2((float)Math.Abs(Math.Cos(NPC.rotation + NPC.position.X / 64f + j * MathHelper.Pi / numTentaclePairs)), 1) * NPC.scale, SpriteEffects.None, 0f);
                }

                spriteBatch.Draw(glowTexture, NPC.oldPos[i] - NPC.position + NPC.Center - screenPos, NPC.frame, color, NPC.rotation, origin, NPC.scale, SpriteEffects.None, 0f);
            }

            for (int i = 0; i < numTentaclePairs; i++)
            {
                spriteBatch.Draw(tentacleTexture, NPC.Center - screenPos, NPC.frame, NPC.GetNPCColorTintedByBuffs(Color.White), NPC.rotation, origin, new Vector2((float)Math.Abs(Math.Cos(NPC.rotation + NPC.position.X / 64f + i * MathHelper.Pi / numTentaclePairs)), 1) * NPC.scale, SpriteEffects.None, 0f);
            }

            spriteBatch.Draw(texture, NPC.Center - screenPos, NPC.frame, NPC.GetNPCColorTintedByBuffs(Color.White), NPC.rotation, origin, NPC.scale, SpriteEffects.None, 0f);

            return false;
        }

        public override bool CheckDead()
        {
            if (PolaritiesSystem.hallowInvasion)
            {
                //counts for 4 points
                PolaritiesSystem.hallowInvasionSize -= 4;
            }

            for (int a = 0; a < 12; a++)
            {
                Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, DustType<AequoreanBubble>(), newColor: Color.White, Scale: 1f).noGravity = true;
            }

            return true;
        }

		public override void ModifyNPCLoot(NPCLoot npcLoot)
		{
            npcLoot.Add(ItemDropRule.Common(ItemType<LuminousSlimeStaff>(), 8));
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            //only spawns during the hallow event
            if (spawnInfo.Player.InModBiome(GetInstance<Biomes.HallowInvasion>()))
            {
                return Biomes.HallowInvasion.GetSpawnChance(3);
            }
            return 0f;
        }
    }

    public class AequoreanImage : ModProjectile
    {
        public override string Texture => "Polarities/NPCs/Enemies/HallowInvasion/Aequorean_Glow";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 6;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.aiStyle = -1;
            Projectile.width = 48;
            Projectile.height = 48;

            Projectile.timeLeft = 600;
            Projectile.penetrate = -1;
            Projectile.hostile = true;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;

            Projectile.hide = true;
        }

        public override void AI()
        {
            Vector2 velMod = (Main.player[(int)Projectile.ai[0]].Center - Projectile.Center).SafeNormalize(Vector2.Zero);
            velMod.Y *= 2;
            Projectile.velocity.Y += 0.05f * velMod.Y;

            if (Projectile.velocity.X > 0)
            {
                Projectile.velocity.X += 0.075f;
            }
            else
            {
                Projectile.velocity.X -= 0.075f;
            }
            Projectile.velocity *= 0.99f;

            Projectile.rotation = (float)Math.Atan(Projectile.velocity.X * 0.1f);

            Projectile.frameCounter++;
            if (Projectile.frameCounter == 5)
            {
                Projectile.frameCounter = 0;
                Projectile.frame = (Projectile.frame + 1) % 6;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Vector2 origin = new Vector2(54, 27);
            Texture2D texture = TextureAssets.Npc[NPCType<Aequorean>()].Value;
            Texture2D glowTexture = TextureAssets.Projectile[Type].Value;
            Texture2D tentacleTexture = Aequorean.TentaclesTexture.Value;
            Rectangle frame = texture.Frame(1, 6, 0, Projectile.frame);
            const int numTentaclePairs = 3;

            Color mainColor = Main.hslToRgb(((Main.GlobalTimeWrappedHourly * 10f - Projectile.timeLeft) / 70f) % 1, 1, 0.8f); //new Color(220, 240, 255);

            for (int i = 1; i < Projectile.oldPos.Length; i++)
            {
                Color color = mainColor * (1 - i / (float)Projectile.oldPos.Length) * 0.25f;

                for (int j = 0; j < numTentaclePairs; j++)
                {
                    Main.EntitySpriteDraw(tentacleTexture, Projectile.oldPos[i] - Projectile.position + Projectile.Center - Main.screenPosition, frame, color * 0.5f, Projectile.rotation, origin, new Vector2((float)Math.Abs(Math.Cos(Projectile.rotation + Projectile.position.X / 64f + j * MathHelper.Pi / numTentaclePairs)), 1) * Projectile.scale, SpriteEffects.None, 0);
                }

                Main.EntitySpriteDraw(glowTexture, Projectile.oldPos[i] - Projectile.position + Projectile.Center - Main.screenPosition, frame, color, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);
            }

            for (int i = 0; i < numTentaclePairs; i++)
            {
                Main.EntitySpriteDraw(tentacleTexture, Projectile.Center - Main.screenPosition, frame, mainColor * 0.5f, Projectile.rotation, origin, new Vector2((float)Math.Abs(Math.Cos(Projectile.rotation + Projectile.position.X / 64f + i * MathHelper.Pi / numTentaclePairs)), 1) * Projectile.scale, SpriteEffects.None, 0);
            }

            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, frame, mainColor * 0.5f, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);
            Main.EntitySpriteDraw(glowTexture, Projectile.Center - Main.screenPosition, frame, mainColor * 0.33f, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);

            return false;
        }

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            behindNPCs.Add(index);
        }
    }
}
