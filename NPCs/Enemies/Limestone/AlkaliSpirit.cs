using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Polarities.Biomes;
using Polarities.Items.Consumables;
using Polarities.Items.Materials;
using Polarities.Items.Placeable.Banners;
using Polarities.Items.Placeable.Blocks;
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
    public class AlkaliSpirit : ModNPC
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
            NPC.width = 30;
            NPC.height = 56;
            NPC.damage = 20;
            NPC.lifeMax = 50;
            NPC.knockBackResist = 1f;
            NPC.npcSlots = 1f;
            NPC.value = 80;
            NPC.HitSound = SoundID.NPCHit54;
            NPC.DeathSound = SoundID.NPCDeath52;
            NPC.lavaImmune = true;
            NPC.noGravity = true;
            NPC.noTileCollide = true;

            Banner = NPC.type;
            BannerItem = ItemType<AlkaliSpiritBanner>();

            SpawnModBiomes = new int[1] { GetInstance<LimestoneCave>().Type };
        }

        public override void AI()
        {
            Lighting.AddLight(NPC.Center, 173f / 255, 217f / 255, 160f / 255);

            //float, relying on blocks/walls for movement
            Tile tile = Main.tile[(int)(NPC.Center.X / 16), (int)((NPC.Center.Y + NPC.height / 2 + 32) / 16)];
            if (tile.HasUnactuatedTile && (Main.tileSolid[tile.TileType] || Main.tileSolidTop[tile.TileType]))
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
                        if (NPC.ai[1] >= 120)
                        {
                            NPC.ai[1] = 0;
                            NPC.ai[0] = 0;
                        }
                        else if (NPC.ai[1] % 30 == 0)
                        {
                            NPC.TargetClosest(false);
                            NPC.velocity = 8 * (Main.player[NPC.target].Center - NPC.Center).SafeNormalize(Vector2.Zero);

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
            for (int i = 0; i < 10; i++)
            {
                Main.dust[Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.GreenFairy, Scale: 1.75f)].noGravity = true;
            }
            return true;
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            npcLoot.Add(ItemDropRule.Common(ItemType<AlkalineFluid>(), 2, 1, 3));
            npcLoot.Add(ItemDropRule.Common(ItemType<KeyLimePie>(), 50));
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            return false;
        }
    }
}