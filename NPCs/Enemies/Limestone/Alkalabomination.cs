using Microsoft.Xna.Framework;
using Polarities.Biomes;
using Polarities.Items.Consumables;
using Polarities.Items.Materials;
using Polarities.Items.Placeable.Banners;
using Polarities.Items.Placeable.Blocks;
using Polarities.Items.Weapons.Summon.Orbs;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Polarities.NPCs.Enemies.Limestone
{
    public class Alkalabomination : ModNPC
    {
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 4;

            Polarities.customNPCGlowMasks[Type] = TextureAssets.Npc[Type];

            NPCID.Sets.NPCBestiaryDrawModifiers drawModifiers = new NPCID.Sets.NPCBestiaryDrawModifiers(0)
            {
                SpriteDirection = 1
            };
            NPCID.Sets.NPCBestiaryDrawOffset.Add(NPC.type, drawModifiers);
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
				//flavor text
				this.TranslatedBestiaryEntry()
            });
        }

        public override void SetDefaults()
        {
            NPC.aiStyle = -1;
            NPC.width = 60;
            NPC.height = 86;
            NPC.damage = 50;
            NPC.lifeMax = 300;
            NPC.knockBackResist = 0.5f;
            NPC.npcSlots = 1f;
            NPC.value = 500;
            NPC.HitSound = SoundID.NPCHit54;
            NPC.DeathSound = SoundID.NPCDeath52;
            NPC.lavaImmune = true;
            NPC.noGravity = true;
            NPC.noTileCollide = true;

            Banner = NPC.type;
            BannerItem = ItemType<AlkalabominationBanner>();

            SpawnModBiomes = new int[1] { GetInstance<LimestoneCave>().Type };
        }

        public override void AI()
        {
            Lighting.AddLight(NPC.Center, 173f / 255, 217f / 255, 160f / 255);

            if (NPC.life * 3 < NPC.lifeMax && NPC.ai[2] != 1 && Main.expertMode)
            {
                NPC.ai[2] = 1;
                Main.npc[NPC.NewNPC(NPC.GetSource_FromAI(), (int)NPC.Center.X, (int)NPC.Center.Y, NPCType<AlkaliSpirit>())].velocity.X = -4;
                Main.npc[NPC.NewNPC(NPC.GetSource_FromAI(), (int)NPC.Center.X, (int)NPC.Center.Y, NPCType<AlkaliSpirit>())].velocity.X = 4;
            }

            //float, relying on blocks/walls for movement
            bool supported = false;
            for (int i = (int)(NPC.Center.X / 16) - 1; i <= (int)(NPC.Center.X / 16) + 1; i++)
            {
                for (int j = (int)(NPC.position.Y / 16); j <= (int)((NPC.Center.Y + NPC.height / 2 + 32) / 16); j++)
                {
                    Tile tile = Main.tile[i, j];
                    if (tile.HasUnactuatedTile && (Main.tileSolid[tile.TileType] || Main.tileSolidTop[tile.TileType]))
                    {
                        supported = true;
                        break;
                    }
                }
                if (supported) { break; }
            }

            if (supported)
            {
                switch (NPC.ai[0])
                {
                    case 0:
                        NPC.TargetClosest(false);
                        Vector2 velMod = (Main.player[NPC.target].Center - NPC.Center).SafeNormalize(Vector2.Zero);
                        velMod.Y *= 2;
                        NPC.velocity += 0.05f * velMod;

                        NPC.ai[1]++;
                        if (NPC.ai[1] >= 240)
                        {
                            NPC.ai[1] = 0;
                            NPC.ai[0] = 1;
                        }
                        break;
                    case 1:
                        NPC.ai[1]++;
                        if (NPC.ai[1] >= 150)
                        {
                            NPC.ai[1] = 0;
                            NPC.ai[0] = 0;
                        }
                        else if (NPC.ai[1] % 30 == 0)
                        {
                            NPC.TargetClosest(false);
                            NPC.velocity = 12 * (Main.player[NPC.target].Center - NPC.Center).SafeNormalize(Vector2.Zero);

                            for (int i = 0; i < 10; i++)
                            {
                                Main.dust[Dust.NewDust(NPC.position, NPC.width, NPC.height, 74, Scale: 1.75f)].noGravity = true;
                            }
                            SoundEngine.PlaySound(SoundID.NPCHit54, NPC.position);
                        }
                        break;
                }
            }
            else
            {
                NPC.velocity.Y += 0.1f;
            }
            NPC.velocity *= 0.99f;

            NPC.rotation = NPC.velocity.X * 0.1f;

            NPC.spriteDirection = NPC.velocity.X > 0 ? -1 : 1;
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            if (spawnInfo.PlayerSafe) return 0f;

            if (!Main.hardMode)
            {
                return 0f;
            }

            Tile playerTile = Main.tile[(int)(spawnInfo.Player.Center.X / 16), (int)((spawnInfo.Player.Center.Y + 1 + spawnInfo.Player.height / 2) / 16)];
            if (spawnInfo.Player.InModBiome(GetInstance<LimestoneCave>()) && (spawnInfo.SpawnTileType == TileType<LimestoneTile>() || playerTile.TileType == TileType<LimestoneTile>()))
            {
                return 0.5f;
            }
            return 0f;
        }

        public override void FindFrame(int frameHeight)
        {
            NPC.frameCounter++;
            if (NPC.frameCounter == 5)
            {
                NPC.frameCounter = 0;
                NPC.frame.Y = (NPC.frame.Y + frameHeight) % (4 * frameHeight);
            }
        }

        public override bool CheckDead()
        {
            for (int i = 0; i < 16; i++)
            {
                Main.dust[Dust.NewDust(NPC.position, NPC.width, NPC.height, 74, Scale: 1.75f)].noGravity = true;
            }
            return true;
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            npcLoot.Add(ItemDropRule.Common(ItemType<AlkalineFluid>(), 1, 2, 5));
            npcLoot.Add(ItemDropRule.Common(ItemType<AlkalineOrb>(), 10));
            npcLoot.Add(ItemDropRule.Common(ItemType<KeyLimePie>(), 50));
        }
    }
}

