using Microsoft.Xna.Framework;
using MonoMod.Cil;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using Polarities.Buffs;
using Microsoft.Xna.Framework.Graphics;
using Terraria.ModLoader.Utilities;
using Terraria.GameContent.Bestiary;
using Polarities.Biomes;
using Polarities.Items;
using Terraria.GameContent;
using Polarities.Effects;

namespace Polarities.NPCs.Critters
{
    public class BabyWanderer : ModNPC
    {
        public override void SetStaticDefaults()
        {
            NPCID.Sets.CountsAsCritter[Type] = true;
            Main.npcCatchable[NPC.type] = true; //TODO: Require fireproof bug net

            Polarities.customNPCGlowMasks[Type] = TextureAssets.Npc[Type];
            PolaritiesNPC.canSpawnInLava.Add(Type);
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

        public override int SpawnNPC(int tileX, int tileY)
        {
            int groupSize = Main.rand.Next(5, 11);
            for (int i = 1; i < groupSize; i++)
            {
                int npc = NPC.NewNPC(NPC.GetSource_NaturalSpawn(), tileX * 16 + 8 - i, tileY * 16, NPC.type);
                Main.npc[npc].velocity = (Main.LocalPlayer.Center - Main.npc[npc].Center).SafeNormalize(Vector2.Zero) * 4;
            }

            int output = NPC.NewNPC(NPC.GetSource_NaturalSpawn(), tileX * 16 + 8, tileY * 16, NPC.type);
            Main.npc[output].velocity = (Main.LocalPlayer.Center - Main.npc[output].Center).SafeNormalize(Vector2.Zero) * 4;
            return output;
        }

        public override void SetDefaults()
        {
            NPC.aiStyle = -1;
            NPC.lifeMax = 5;
            NPC.width = 10;
            NPC.height = 10;
            NPC.noTileCollide = true;
            NPC.noGravity = true;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = Sounds.ConvectiveBabyDeath;
            NPC.npcSlots = 0.1f;

            NPC.rarity = 4;

            NPC.catchItem = (short)ItemType<BabyWandererItem>();

            SpawnModBiomes = new int[1] { GetInstance<LavaOcean>().Type };
        }

        public override void AI()
        {
            Tile tile = Framing.GetTileSafely((NPC.Center / 16).ToPoint());
            if (tile.HasUnactuatedTile || tile.LiquidAmount >= 64)
            {
                //supported
                //meander, favoring lava if it's nearby
                Vector2 favoredDirection = Vector2.Zero;
                const int range = 10;
                for (int i = -range; i <= range; i++)
                {
                    for (int j = -range; j <= range; j++)
                    {
                        if (i != 0 || j != 0)
                        {
                            Tile testTile = Framing.GetTileSafely((NPC.Center / 16).ToPoint() + new Point(i, j));
                            if (testTile.LiquidType == LiquidID.Lava || (testTile.LiquidAmount == 0 && !testTile.HasUnactuatedTile))
                            {
                                Vector2 displacement = ((NPC.Center / 16).ToPoint() + new Point(i, j)).ToVector2() * 16 + new Vector2(8, 8) - NPC.Center;
                                favoredDirection += displacement / displacement.LengthSquared();
                            }
                        }
                    }
                }
                NPC.velocity += (Vector2.UnitX.RotateRandom(MathHelper.TwoPi) + favoredDirection) * 0.2f;

                Boids();

                NPC.velocity *= (float)Math.Pow(0.75f + 1 / NPC.velocity.Length(), 0.1f);

                NPC.rotation = NPC.velocity.ToRotation();
            }
            else
            {
                //unsupported
                NPC.velocity.Y += 0.3f;
                NPC.velocity *= 0.975f;
                NPC.rotation = NPC.velocity.ToRotation();
            }
        }

        private void Boids()
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
                    separation += 8f * (NPC.Center - otherNPC.Center).SafeNormalize(Vector2.Zero) / (NPC.Center - otherNPC.Center).Length();

                    //alignment component
                    alignment += 1 / 128f * (otherNPC.velocity - NPC.velocity);

                    //cohesion component
                    cohesion += 1 / 128f * (otherNPC.Center - NPC.Center);
                }
                else if (i != NPC.whoAmI && otherNPC.active && (NPC.Center - otherNPC.Center).Length() < 128)
                {
                    //avoidance component
                    separation += 16f * (NPC.Center - otherNPC.Center).SafeNormalize(Vector2.Zero) / (NPC.Center - otherNPC.Center).Length();
                }
            }

            if (!Main.LocalPlayer.dead && (NPC.Center - Main.LocalPlayer.Center).Length() < 128)
            {
                //avoidance component for player
                separation += 16f * (NPC.Center - Main.LocalPlayer.Center).SafeNormalize(Vector2.Zero) / (NPC.Center - Main.LocalPlayer.Center).Length();
            }

            if (count > 0)
            {
                alignment /= count;
                cohesion /= count;
            }

            NPC.velocity += separation + alignment + cohesion;
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            //requires no lava to spawn
            if (Main.hardMode && LavaOcean.SpawnValid(spawnInfo, requireLava: true))
            {
                return 0.25f;
            }
            return 0f;
        }

        public override bool CheckDead()
        {
            for (int i = 1; i <= 2; i++)
                Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, Mod.Find<ModGore>("BabyWandererGore" + i).Type);
            if (!NPC.AnyNPCs(NPCType<ConvectiveWanderer.ConvectiveWanderer>()) && Main.LocalPlayer.InModBiome(GetInstance<LavaOcean>()) && PolaritiesSystem.convectiveWandererSpawnTimer == 0) PolaritiesSystem.convectiveWandererSpawnTimer = 1;
            return true;
        }
        public override void DrawBehind(int index)
        {
            RenderTargetLayer.AddNPC<ConvectiveEnemyTarget>(index);
        }
    }

    public class BabyWandererItem : ModItem
    {
        public override string Texture => "Polarities/NPCs/Critters/BabyWanderer";

        public override void SetStaticDefaults()
        {
            SacrificeTotal = (5);

            DisplayName.SetDefault("{$Mods.Polarities.NPCName.BabyWanderer}");

            ItemID.Sets.IsLavaBait[Type] = true;
        }

        public override void SetDefaults()
        {
            Item.CloneDefaults(ItemID.Firefly);
            Item.bait = 0;
            Item.rare = ItemRarityID.Blue;
            Item.makeNPC = (short)NPCType<BabyWanderer>();
        }

        public override void AddRecipes()
        {
            Recipe.Create(ItemType<ConvectiveWandererSummonItem>())
                .AddIngredient(Type, 5)
                .Register();
        }
    }
}
