using Microsoft.Xna.Framework;
using MonoMod.Cil;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent.Bestiary;

namespace Polarities.NPCs.Critters
{
    public class CloudCichlid : ModNPC
    {
        private float goalHeight
        {
            get => goalHeight = NPC.ai[0];
            set => NPC.ai[0] = value;
        }

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 5;
            Main.npcCatchable[NPC.type] = true;
            NPCID.Sets.CountsAsCritter[Type] = true;
        }

        public override void SetDefaults()
        {
            NPC.aiStyle = -1;
            NPC.lifeMax = 5;
            NPC.width = 30;
            NPC.height = 22;
            NPC.noTileCollide = false;
            NPC.noGravity = true;
            NPC.HitSound = SoundID.NPCHit30;
            NPC.DeathSound = SoundID.NPCDeath33;
            NPC.catchItem = (short)ItemType<CloudCichlidItem>();
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
                //spawn conditions
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Events.Rain,
				//flavor text
				this.TranslatedBestiaryEntry()
            });
        }

        public override void AI()
        {
            if (NPC.ai[1] == 0)
            {
                NPC.ai[1] = 1;
                goalHeight = NPC.position.Y;
                NPC.velocity.X = Main.rand.NextBool() ? 1 : -1;
                NPC.netUpdate = true;
            }
            if (NPC.collideX)
            {
                NPC.velocity.X = -NPC.oldVelocity.X;
            }
            if (NPC.collideY)
            {
                goalHeight += Main.rand.NextBool() ? -128 : 128;
            }
            if (Main.rand.NextBool(300))
            {
                NPC.netUpdate = true;
                goalHeight += Main.rand.NextFloat(-64, 64);
            }
            if (goalHeight > 16 * Main.worldSurface)
            {
                goalHeight = (float)(16 * Main.worldSurface);
            }
            if (goalHeight > 16 * Main.worldSurface / 2 && !Main.raining)
            {
                goalHeight = (float)(16 * Main.worldSurface / 2);
            }

            NPC.spriteDirection = (NPC.velocity.X > 0) ? 1 : -1;
            NPC.velocity *= 0.99f;
            NPC.velocity.X += (1.5f * NPC.spriteDirection + Main.windSpeedCurrent - NPC.velocity.X) / 60;
            NPC.velocity.Y += (goalHeight - NPC.position.Y > 50 * NPC.velocity.Y) ? 0.01f : -0.01f;
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            if (spawnInfo.Water || !spawnInfo.Player.ZoneOverworldHeight)
            {
                return 0f;
            }
            if (Main.raining)
            {
                return 0.3f;
            }
            return 0f;
        }

        public override void FindFrame(int frameHeight)
        {
            NPC.frameCounter++;
            if (NPC.frameCounter == 6)
            {
                NPC.frameCounter = 0;
                NPC.frame.Y = (NPC.frame.Y + frameHeight) % (5 * frameHeight);
            }
        }

        public override bool CheckDead()
        {
            for (int i = 0; i < 16; i++)
            {
                Main.dust[Dust.NewDust(NPC.position, NPC.width, NPC.height, 51, Scale: 1.5f)].noGravity = true;
            }

            return true;
        }
    }

    public class CloudCichlidItem : ModItem
    {
        public override void SetStaticDefaults()
        {
            this.SetResearch(5);

            DisplayName.SetDefault("{$Mods.Polarities.NPCName.CloudCichlid}");
        }

        public override void SetDefaults()
        {
            Item.CloneDefaults(ItemID.Firefly);
            Item.bait = 0;
            Item.rare = 1;
            Item.makeNPC = (short)NPCType<CloudCichlid>();
        }

        public override void AddRecipes()
        {
            Recipe.Create(ItemID.Cloud, 5)
                .AddIngredient(Type)
                .Register();
        }
    }

    public class StormcloudCichlid : ModNPC
    {
        private float goalHeight
        {
            get => goalHeight = NPC.ai[0];
            set => NPC.ai[0] = value;
        }

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 5;
            Main.npcCatchable[NPC.type] = true;
            NPCID.Sets.CountsAsCritter[Type] = true;
        }

        public override void SetDefaults()
        {
            NPC.aiStyle = -1;
            NPC.lifeMax = 5;
            NPC.width = 30;
            NPC.height = 22;
            NPC.noTileCollide = false;
            NPC.noGravity = true;
            NPC.HitSound = SoundID.NPCHit30;
            NPC.DeathSound = SoundID.NPCDeath33;
            NPC.catchItem = (short)ItemType<StormcloudCichlidItem>();
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
                //spawn conditions
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Events.WindyDay,
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Events.Rain,
				//flavor text
				this.TranslatedBestiaryEntry()
            });
        }

        public override void AI()
        {
            if (NPC.ai[1] == 0)
            {
                NPC.ai[1] = 1;
                goalHeight = NPC.position.Y;
                NPC.velocity.X = Main.rand.NextBool() ? 1 : -1;
                NPC.netUpdate = true;
            }
            if (NPC.collideX)
            {
                NPC.velocity.X = -NPC.oldVelocity.X;
            }
            if (NPC.collideY)
            {
                goalHeight += Main.rand.NextBool() ? -128 : 128;
            }
            if (Main.rand.NextBool(300))
            {
                NPC.netUpdate = true;
                goalHeight += Main.rand.NextFloat(-64, 64);
            }
            if (goalHeight > 16 * Main.worldSurface)
            {
                goalHeight = (float)(16 * Main.worldSurface);
            }
            if (goalHeight > 16 * Main.worldSurface / 2 && !Main.raining)
            {
                goalHeight = (float)(16 * Main.worldSurface / 2);
            }

            NPC.spriteDirection = (NPC.velocity.X > 0) ? 1 : -1;
            NPC.velocity *= 0.99f;
            NPC.velocity.X += (1.5f * NPC.spriteDirection + Main.windSpeedCurrent - NPC.velocity.X) / 60;
            NPC.velocity.Y += (goalHeight - NPC.position.Y > 50 * NPC.velocity.Y) ? 0.01f : -0.01f;
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            if (spawnInfo.Water || !spawnInfo.Player.ZoneOverworldHeight || !PolaritiesSystem.downedStormCloudfish)
            {
                return 0f;
            }
            if (Main.raining && Main.WindyEnoughForKiteDrops)
            {
                return 0.3f;
            }
            return 0f;
        }

        public override void FindFrame(int frameHeight)
        {
            NPC.frameCounter++;
            if (NPC.frameCounter == 6)
            {
                NPC.frameCounter = 0;
                NPC.frame.Y = (NPC.frame.Y + frameHeight) % (5 * frameHeight);
            }
        }

        public override bool CheckDead()
        {
            for (int i = 0; i < 16; i++)
            {
                Main.dust[Dust.NewDust(NPC.position, NPC.width, NPC.height, 36, Scale: 1.5f)].noGravity = true;
            }

            return true;
        }
    }

    public class StormcloudCichlidItem : ModItem
    {
        public override void SetStaticDefaults()
        {
            this.SetResearch(5);

            DisplayName.SetDefault("{$Mods.Polarities.NPCName.StormcloudCichlid}");
        }

        public override void SetDefaults()
        {
            Item.CloneDefaults(ItemID.Firefly);
            Item.bait = 0;
            Item.rare = 1;
            Item.makeNPC = (short)NPCType<StormcloudCichlid>();
        }
    }
}