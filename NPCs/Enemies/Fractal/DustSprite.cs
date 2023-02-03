using Microsoft.Xna.Framework;
using Polarities.Biomes.Fractal;
using Polarities.Items.Materials;
using Polarities.Items.Placeable.Banners;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Polarities.NPCs.Enemies.Fractal
{
    public class DustSprite : ModNPC
    {
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 3;
        }

        public override void SetDefaults()
        {
            NPC.aiStyle = 14;
            NPC.stairFall = true;
            NPC.width = 10;
            NPC.height = 10;
            NPC.defense = 0;
            NPC.damage = 20;
            NPC.lifeMax = 40;
            NPC.knockBackResist = 0f;
            NPC.npcSlots = 0.067f;
            NPC.noGravity = true;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;

            Banner = NPC.type;
            BannerItem = ItemType<DustSpriteBanner>();

            this.SetModBiome<FractalBiome, FractalWastesBiome>();

            NPC.GetGlobalNPC<PolaritiesNPC>().usesProjectileHitCooldowns = true;
            NPC.GetGlobalNPC<PolaritiesNPC>().projectileHitCooldownTime = 10;
        }

        public override void HitEffect(int hitDirection, double damage)
        {
            if (NPC.life <= 0 && Main.netMode != NetmodeID.Server)
            {
                GoreHelper.DeathGore(NPC, $"DustSpriteGore{1 + NPC.frame.Y / (TextureAssets.Npc[NPC.type].Value.Height / Main.npcFrameCount[NPC.type])}", velocity: NPC.velocity);
            }
        }

        public override int SpawnNPC(int tileX, int tileY)
        {
            for (int i = 1; i <= 15; i++)
            {
                NPC.NewNPC(NPC.GetSource_NaturalSpawn(), tileX * 16 + 8 - i, tileY * 16, NPC.type);
            }

            return NPC.NewNPC(NPC.GetSource_NaturalSpawn(), tileX * 16 + 8, tileY * 16, NPC.type);
        }

        public override bool PreAI()
        {
            Player player = Main.player[NPC.target];
            if (!player.active || player.dead)
            {
                NPC.netUpdate = true;
                NPC.TargetClosest(false);
                player = Main.player[NPC.target];
            }

            if (NPC.collideX)
            {
                NPC.velocity.X = -NPC.velocity.X;
            }
            if (NPC.collideY)
            {
                NPC.velocity.Y = -NPC.velocity.Y;
            }

            Boids(player.Center);

            if (NPC.wet)
            {
                NPC.velocity.Y -= 0.15f;
                if (NPC.velocity.Y > 0)
                {
                    NPC.velocity.Y *= 0.9f;
                }
            }

            NPC.rotation += NPC.velocity.X / 10f;

            return false;
        }

        public override void FindFrame(int frameHeight)
        {
            if (NPC.frameCounter == 0)
            {
                NPC.frameCounter++;
                NPC.frame.Y = Main.rand.Next(3) * frameHeight;
            }
        }

        private void Boids(Vector2 goalPosition)
        {
            //boids
            Vector2 separation = Vector2.Zero;
            Vector2 alignment = Vector2.Zero;
            Vector2 cohesion = Vector2.Zero;
            int count = 0;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC otherNPC = Main.npc[i];

                if (i != NPC.whoAmI && otherNPC.type == NPC.type && otherNPC.active && (NPC.Center - otherNPC.Center).Length() < 128)
                {
                    count++;

                    //separation component
                    separation += 32f * (NPC.Center - otherNPC.Center).SafeNormalize(Vector2.Zero) / (NPC.Center - otherNPC.Center).Length();

                    //alignment component
                    alignment += 1 / 6f * (otherNPC.velocity - NPC.velocity);

                    //cohesion component
                    cohesion += 1 / 24f * (otherNPC.Center - NPC.Center);
                }

            }

            if (count > 0)
            {
                alignment /= count;
                cohesion /= count;
            }

            Vector2 goalVelocity = NPC.velocity + separation + alignment + cohesion + (goalPosition - NPC.Center).SafeNormalize(Vector2.Zero) * 1.5f;
            if (goalVelocity.Length() > 5)
            {
                goalVelocity.Normalize();
                goalVelocity *= 5;
            }
            NPC.velocity += (goalVelocity - NPC.velocity) / 20;
        }

        public override void OnHitPlayer(Player target, int damage, bool crit)
        {
            target.AddBuff(BuffID.Confused, 10, true);
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            if (FractalSubworld.Active)
            {
                return 0.5f * FractalSubworld.SpawnConditionFractalWastes(spawnInfo) * (1 - FractalSubworld.SpawnConditionFractalSky(spawnInfo));
            }
            return 0f;
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
				//flavor text
				this.TranslatedBestiaryEntry()
            });
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<FractalResidue>(), chanceDenominator: 30));
        }
    }
}