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
    public class InfernalArrow : ModNPC
    {
        private int dashCooldown;
        private float targetRotation;

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
                    BuffType<Incinerating>()
                }
            };
            NPCID.Sets.DebuffImmunitySets.Add(Type, debuffData);

            Main.npcFrameCount[Type] = 5;
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
            NPC.width = 42;
            NPC.height = 42;
            NPC.defense = 18;
            NPC.damage = 40;
            NPC.lifeMax = 200;
            NPC.knockBackResist = 0.4f;
            NPC.value = Item.buyPrice(silver: 10);
            NPC.lavaImmune = true;
            NPC.noGravity = true;
            NPC.noTileCollide = true;

            //TODO: Adjust
            NPC.HitSound = SoundID.NPCHit36;
            NPC.DeathSound = SoundID.NPCDeath39;

            Banner = NPC.type;
            BannerItem = ItemType<InfernalArrowBanner>();

            //NPC.hide = true;
        }

        //TODO: Adjust
        public override void AI()
        {
            if (NPC.localAI[0] == 0)
            {
                NPC.localAI[0] = 1;
                NPC.netUpdate = true;
            }
            Lighting.AddLight(NPC.Center, 1f, 0.5f, 0f);

            Player player = Main.player[NPC.target];
            if (!player.active || player.dead)
            {
                NPC.netUpdate = true;
                NPC.TargetClosest(false);
                player = Main.player[NPC.target];
            }

            NPC.noGravity = true;

            if (true)
            {
                if (NPC.velocity.Length() >= 16)
                {
                    dashCooldown = Main.rand.Next(180, 240);
                    NPC.netUpdate = true;
                }

                if (dashCooldown <= 0 && NPC.noGravity && NPC.velocity.Length() < 16 && Math.Abs(Math.Sin(-Math.PI / 4 + ((NPC.Center - player.Center).ToRotation() - NPC.rotation) / 2)) < Math.Sin(0.1))
                {
                    NPC.velocity += 0.5f * (player.Center - NPC.Center) / (NPC.Center - player.Center).Length();
                    targetRotation = NPC.velocity.ToRotation() + (float)Math.PI / 2;
                }
                else if (NPC.noGravity)
                {
                    NPC.velocity *= 0.99f;
                    NPC.velocity += 0.05f * (player.Center - NPC.Center) / (NPC.Center - player.Center).Length();
                    targetRotation = NPC.velocity.ToRotation() + (float)Math.PI / 2;
                }
                else
                {
                    if (NPC.velocity.X > 0)
                    {
                        targetRotation = -NPC.velocity.ToRotation();
                    }
                    else
                    {
                        targetRotation = (float)Math.PI - NPC.velocity.ToRotation();
                    }
                }

                if (Math.Abs(Math.Sin((targetRotation - NPC.rotation) / 2)) < Math.Sin(0.1))
                {
                    NPC.rotation = targetRotation;
                }
                else if (Math.Sin(targetRotation - NPC.rotation) > 0)
                {
                    NPC.rotation += 0.1f;
                }
                else
                {
                    NPC.rotation -= 0.1f;
                }
            }

            dashCooldown -= 1;
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            //requires no lava to spawn
            if (Main.hardMode && Biomes.LavaOcean.SpawnValid(spawnInfo, requireLava: false))
            {
                return 0.5f;
            }
            return 0f;
        }

        public override void FindFrame(int frameHeight)
        {
            NPC.frameCounter++;
            if (NPC.frameCounter % 4 == 0)
            {
                NPC.frameCounter = 0;
                NPC.frame.Y = (NPC.frame.Y + frameHeight) % (5 * frameHeight);
            }
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            npcLoot.Add(ItemDropRule.Common(ItemID.Hellstone, 1, 2, 4));
        }

        public override void OnHitPlayer(Player target, int damage, bool crit)
        {
            target.AddBuff(BuffID.OnFire, 60);
        }
    }
}