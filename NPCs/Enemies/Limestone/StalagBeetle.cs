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
using Terraria.Audio;
using Polarities.Biomes;
using Polarities.Items.Placeable.Blocks;
using Terraria.GameContent.ItemDropRules;
using Polarities.Items.Consumables;
using Terraria.GameContent.Bestiary;

namespace Polarities.NPCs.Enemies.Limestone
{
    public class StalagBeetle : ModNPC
    {
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 7;

            Polarities.customNPCGlowMasks[Type] = Request<Texture2D>(Texture + "_Mask");
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
            NPC.aiStyle = 3;
            NPC.stairFall = false;
            NPC.width = 58;
            NPC.height = 32;
            NPC.damage = 40;
            NPC.lifeMax = 120;
            NPC.knockBackResist = 0.1f;
            NPC.npcSlots = 1f;
            AIType = 508;
            NPC.value = 200;
            NPC.HitSound = SoundID.NPCHit2;
            NPC.DeathSound = SoundID.NPCDeath1;

            Banner = NPC.type;
            BannerItem = ItemType<StalagBeetleBanner>();

            SpawnModBiomes = new int[1] { GetInstance<LimestoneCave>().Type };
        }

        public override bool PreAI()
        {
            if (NPC.velocity.X > 0)
            {
                NPC.spriteDirection = 1;
            }
            else if (NPC.velocity.X < 0)
            {
                NPC.spriteDirection = -1;
            }

            if (Main.expertMode)
            {
                if (NPC.ai[1] > 0 && (NPC.collideY || NPC.ai[1] < 540 + 119)) NPC.ai[1]--;

                Player player = Main.player[NPC.target];
                if (NPC.ai[1] == 0 && Collision.CanHit(NPC.position, NPC.width, NPC.height, player.position, player.width, player.height) && (player.Center - NPC.Center).Length() > 240f)
                {
                    NPC.ai[1] = 540 + 89;
                }
                if (NPC.ai[1] > 540)
                {
                    if (player.Center.X > NPC.Center.X)
                    {
                        NPC.spriteDirection = 1;
                    }
                    else if (player.Center.X < NPC.Center.X)
                    {
                        NPC.spriteDirection = -1;
                    }

                    if (NPC.ai[1] % 30 == 0)
                    {
                        Vector2 shotPos = NPC.Center + new Vector2(NPC.spriteDirection * NPC.width / 2, 0);
                        SoundEngine.PlaySound(SoundID.Item, NPC.Center, 17);
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), shotPos, (player.Center - shotPos).SafeNormalize(Vector2.Zero) * 6, ProjectileType<StalagBeetleShot>(), 2, 0f, Main.myPlayer);
                    }
                    if (NPC.collideY)
                    {
                        NPC.velocity.X *= 0.98f;
                    }
                    return false;
                }
            }

            return true;
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            Tile playerTile = Main.tile[(int)(spawnInfo.Player.Center.X / 16), (int)((spawnInfo.Player.Center.Y + 1 + spawnInfo.Player.height / 2) / 16)];
            if (spawnInfo.Player.InModBiome(GetInstance<LimestoneCave>()) && (spawnInfo.SpawnTileType == TileType<LimestoneTile>() || playerTile.TileType == TileType<LimestoneTile>()))
            {
                return 2f;
            }
            return 0f;
        }

        public override void FindFrame(int frameHeight)
        {
            if (NPC.ai[1] <= 540)
            {
                NPC.frameCounter++;
                if (NPC.frameCounter == 2)
                {
                    NPC.frameCounter = 0;
                    NPC.frame.Y = (NPC.frame.Y + frameHeight) % (7 * frameHeight);
                }
            }
            else
            {
                NPC.frame.Y = 0;
            }
        }

        public override bool CheckDead()
        {
            for (int i = 1; i <= 2; i++)
                Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, Mod.Find<ModGore>("StalagBeetleGore" + i).Type);
            return true;
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            npcLoot.Add(ItemDropRule.Common(ItemType<StalagBeetleHead>(), 20));
            npcLoot.Add(ItemDropRule.Common(ItemType<Items.Placeable.Blocks.Limestone>(), 1, 2, 4));
            npcLoot.Add(ItemDropRule.Common(ItemType<KeyLimePie>(), 50));
        }
    }

    public class StalagBeetleShot : ModProjectile
    {
        public override string Texture => "Polarities/Textures/Pixel";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Boiling Fluid");
        }

        public override void SetDefaults()
        {
            Projectile.aiStyle = -1;
            Projectile.hostile = true;
            Projectile.width = 2;
            Projectile.height = 2;
            Projectile.alpha = 0;
            Projectile.timeLeft = 240;
            Projectile.penetrate = 1;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.hide = true;
        }

        public override void AI()
        {
            Main.dust[Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, 74, Scale: 1f)].velocity = new Vector2(0.5f, 0).RotatedByRandom(MathHelper.TwoPi);
        }

        public override void OnHitPlayer(Player target, int damage, bool crit)
        {
            target.AddBuff(BuffType<Buffs.Corroding>(), 15 * 60);
        }
    }
}