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
using Polarities.Items.Weapons;
using Polarities.Items.Armor;
using Polarities.Items.Placeable.Banners;
using Terraria.DataStructures;
using Terraria.GameContent.Bestiary;
using System.Collections.Generic;
using MultiHitboxNPCLibrary;
using Terraria.GameContent;
using Terraria.GameContent.ItemDropRules;
using ReLogic.Content;
using Polarities.Items.Materials;

namespace Polarities.NPCs.Enemies.Granite
{
    public class GraniteCrawler : ModNPC
    {
        public override void SetStaticDefaults()
        {
            NPCDebuffImmunityData debuffData = new NPCDebuffImmunityData
            {
                SpecificallyImmuneTo = new int[] {
                    BuffID.Confused
                }
            };
            NPCID.Sets.DebuffImmunitySets.Add(Type, debuffData);

            MultiHitboxNPC.MultiHitboxNPCTypes.Add(Type);
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Granite,
				//flavor text
				this.TranslatedBestiaryEntry()
            });
        }

        //TODO: Gores

        public override void SetDefaults()
        {
            NPC.aiStyle = -1;
            NPC.width = 18;
            NPC.height = 18;

            NPC.defense = 8;
            NPC.damage = 36;
            NPC.lifeMax = 100;

            NPC.knockBackResist = 0f;
            NPC.value = Item.sellPrice(silver: 10);
            NPC.npcSlots = 1f;
            NPC.behindTiles = true;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.HitSound = SoundID.NPCHit7;
            NPC.DeathSound = SoundID.NPCDeath6;

            Banner = Type;
            BannerItem = ItemType<GraniteCrawlerBanner>();
        }

        const int numSegments = 30;
        private Vector2[] segmentPositions = new Vector2[numSegments * 9 - 4];
        private int[] hitBoxSegmentIds = new int[numSegments];

        public override void AI()
        {
            Player player = Main.player[NPC.target];
            if (!player.active || player.dead)
            {
                NPC.netUpdate = true;
                NPC.TargetClosest(false);
                player = Main.player[NPC.target];
            }

            if (NPC.localAI[0] == 0)
            {
                NPC.rotation = (player.Center - NPC.Center).ToRotation();
                NPC.localAI[0] = 1;

                //initialize segment ids
                for (int i = 0; i < numSegments; i++)
                {
                    hitBoxSegmentIds[i] = -1;
                }

                segmentPositions[0] = NPC.Center - new Vector2(NPC.height / 2 - 2, 0).RotatedBy(NPC.rotation);
                for (int i = 1; i < segmentPositions.Length; i++)
                {
                    segmentPositions[i] = segmentPositions[i - 1] - new Vector2(NPC.height, 0).RotatedBy(NPC.rotation);
                }
            }

            //changeable ai values
            float rotationFade = 9f;
            float rotationAmount = 0.1f;
            NPC.dontTakeDamage = false;
            NPC.ai[3] = 1f;

            //overall AI
            switch (NPC.ai[0])
            {
                case 0:
                    //move towards player
                    NPC.velocity += ((player.Center - NPC.Center).SafeNormalize(Vector2.Zero) * 12f - NPC.velocity) / 60f;
                    NPC.rotation = NPC.velocity.ToRotation();

                    NPC.ai[1]++;
                    if (NPC.ai[1] >= 120 && (player.Center - NPC.Center).Length() < 360)
                    {
                        NPC.ai[1] = 0;
                        NPC.ai[0] = 1;
                    }
                    break;
                case 1:
                    //encircle player
                    if (NPC.ai[1] == 0)
                    {
                        NPC.direction = Main.rand.NextBool() ? 1 : -1;
                    }

                    Vector2 goalPosition = player.Center + (NPC.Center - player.Center).SafeNormalize(Vector2.Zero).RotatedBy(0.2f * NPC.direction) * 120;
                    Vector2 goalVelocity = (goalPosition - NPC.Center).SafeNormalize(Vector2.Zero) * 16f;
                    NPC.velocity += (goalVelocity - NPC.velocity) / 10f;
                    NPC.rotation = NPC.velocity.ToRotation();

                    NPC.ai[1]++;
                    if (NPC.ai[1] >= 120)
                    {
                        NPC.ai[1] = 0;
                        NPC.ai[0] = 2;
                    }
                    break;
                case 2:
                    //freeze in place and shield
                    rotationAmount = 0f;
                    NPC.ai[3] = Math.Max(0f, (20 - NPC.ai[1]) / 20f);

                    if (NPC.ai[3] == 0f)
                    {
                        NPC.dontTakeDamage = true;
                        NPC.velocity = Vector2.Zero;
                    }
                    else
                    {
                        goalPosition = player.Center + (NPC.Center - player.Center).SafeNormalize(Vector2.Zero).RotatedBy(0.2f * NPC.direction) * 120;
                        goalVelocity = (goalPosition - NPC.Center).SafeNormalize(Vector2.Zero) * 2f;
                        NPC.velocity += (goalVelocity - NPC.velocity) / 10f;
                        NPC.rotation = NPC.velocity.ToRotation();
                    }

                    NPC.ai[1]++;
                    if (NPC.ai[1] >= 120)
                    {
                        NPC.ai[1] = 0;
                        NPC.ai[0] = 3;
                    }
                    break;
                case 3:
                    //move in a straight line and curve away from the player a bit
                    goalVelocity = (NPC.Center - player.Center + new Vector2(960, 0).RotatedBy(NPC.rotation)).SafeNormalize(Vector2.Zero) * 6f;
                    NPC.velocity += (goalVelocity - NPC.velocity) / 60f;
                    NPC.rotation = NPC.velocity.ToRotation();

                    NPC.ai[3] = Math.Min(1f, NPC.ai[1] / 20f);

                    NPC.ai[1]++;
                    if (NPC.ai[1] >= 60)
                    {
                        NPC.ai[1] = 0;
                        NPC.ai[0] = 0;
                    }
                    break;
            }

            //ai[2] counts total distance traveled
            NPC.ai[2] += NPC.velocity.Length();

            //update segment positions
            segmentPositions[0] = NPC.Center + NPC.velocity - new Vector2(NPC.height / 2 - 2, 0).RotatedBy(NPC.rotation);
            Vector2 rotationGoal = Vector2.Zero;

            for (int i = 1; i < segmentPositions.Length; i++)
            {
                if (i > 1)
                {
                    rotationGoal = ((rotationFade - 1) * rotationGoal + (segmentPositions[i - 1] - segmentPositions[i - 2])) / rotationFade;
                }

                segmentPositions[i] = segmentPositions[i - 1] + (rotationAmount * rotationGoal + (segmentPositions[i] - segmentPositions[i - 1]).SafeNormalize(Vector2.Zero)).SafeNormalize(Vector2.Zero) * 2;
            }

            NPC.realLife = NPC.whoAmI;

            //position hitbox segments
            List<RectangleHitboxData> hitboxes = new List<RectangleHitboxData>();
            for (int h = 0; h < numSegments; h++)
            {
                Vector2 spot = segmentPositions[h * 9 + 4];
                hitboxes.Add(new RectangleHitboxData(new Rectangle((int)spot.X - NPC.width / 2, (int)spot.Y - NPC.height / 2, NPC.width, NPC.height)));
            }
            NPC.GetGlobalNPC<MultiHitboxNPC>().AssignHitboxFrom(hitboxes);

            //head should emit light while not shielded
            if (!NPC.dontTakeDamage)
            {
                Lighting.AddLight(NPC.Center, new Vector3(159f / 255f, 233f / 255f, 1f) * NPC.ai[3]);
            }
            else
            {
                NPC.dontTakeDamage = false;
                NPC.GetGlobalNPC<PolaritiesNPC>().neutralTakenDamageMultiplier = 0.5f;
            }
        }

        public static Asset<Texture2D> LegTexture;
        public static Asset<Texture2D> ClawTexture;
        public static Asset<Texture2D> JawTexture;
        public static Asset<Texture2D> BodyTexture;
        public static Asset<Texture2D> TailTexture;

        public override void Load()
        {
            LegTexture = Request<Texture2D>(Texture + "_Leg");
            ClawTexture = Request<Texture2D>(Texture + "_Claw");
            JawTexture = Request<Texture2D>(Texture + "_Jaw");
            BodyTexture = Request<Texture2D>(Texture + "_Body");
            TailTexture = Request<Texture2D>(Texture + "_Tail");
        }

        public override void Unload()
        {
            LegTexture = null;
            ClawTexture = null;
            JawTexture = null;
            BodyTexture = null;
            TailTexture = null;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color lightColor)
        {
            if (NPC.IsABestiaryIconDummy)
            {
                NPC.rotation = MathHelper.Pi;
                NPC.Center -= new Vector2(20, 0);
                segmentPositions[0] = NPC.Center + new Vector2(NPC.width / 2 - 2, 0).RotatedBy(NPC.rotation);
                const float rotAmoutPerSegment = 0.01f;
                for (int i = 1; i < segmentPositions.Length; i++)
                {
                    segmentPositions[i] = segmentPositions[i - 1] - new Vector2(2, 0).RotatedBy(NPC.rotation + rotAmoutPerSegment * i);
                }

                NPC.ai[2] = 0f;
                NPC.ai[3] = 1f;
            }

            //draw legs
            Texture2D legTexture = LegTexture.Value;
            Texture2D clawTexture = ClawTexture.Value;
            for (int h = 1; h < numSegments - 1; h++)
            {
                int i = h * 9 + 4;

                Vector2 segmentPosition = (segmentPositions[i] + segmentPositions[i - 1]) / 2;
                float segmentRotation = (segmentPositions[i - 1] - segmentPositions[i]).ToRotation();

                float rotationOffset = (float)Math.Sin(NPC.ai[2] * 0.05f + h) + 1f;
                spriteBatch.Draw(legTexture, new Vector2(0, 8).RotatedBy(segmentRotation) + segmentPosition - screenPos, legTexture.Frame(), NPC.IsABestiaryIconDummy ? Color.White : NPC.GetNPCColorTintedByBuffs(Lighting.GetColor((int)(segmentPosition.X / 16), (int)(segmentPosition.Y / 16))), segmentRotation + rotationOffset, new Vector2(3, 3), 1f, SpriteEffects.FlipVertically, 0f);
                spriteBatch.Draw(clawTexture, new Vector2(0, 8).RotatedBy(segmentRotation) + new Vector2(14, 0).RotatedBy(segmentRotation + rotationOffset) + segmentPosition - screenPos, clawTexture.Frame(), NPC.IsABestiaryIconDummy ? Color.White : NPC.GetNPCColorTintedByBuffs(Lighting.GetColor((int)(segmentPosition.X / 16), (int)(segmentPosition.Y / 16))), segmentRotation + rotationOffset / 2f - 1f, new Vector2(19, 7), 1f, SpriteEffects.FlipVertically, 0f);

                spriteBatch.Draw(legTexture, new Vector2(0, -8).RotatedBy(segmentRotation) + segmentPosition - screenPos, legTexture.Frame(), NPC.IsABestiaryIconDummy ? Color.White : NPC.GetNPCColorTintedByBuffs(Lighting.GetColor((int)(segmentPosition.X / 16), (int)(segmentPosition.Y / 16))), segmentRotation - rotationOffset, new Vector2(3, 3), 1f, SpriteEffects.None, 0f);
                spriteBatch.Draw(clawTexture, new Vector2(0, -8).RotatedBy(segmentRotation) + new Vector2(14, 0).RotatedBy(segmentRotation - rotationOffset) + segmentPosition - screenPos, clawTexture.Frame(), NPC.IsABestiaryIconDummy ? Color.White : NPC.GetNPCColorTintedByBuffs(Lighting.GetColor((int)(segmentPosition.X / 16), (int)(segmentPosition.Y / 16))), segmentRotation - rotationOffset / 2f + 1f, new Vector2(19, 3), 1f, SpriteEffects.None, 0f);
            }

            //draw tail
            Texture2D tailTexture = TailTexture.Value;
            Vector2 tailPosition = segmentPositions[segmentPositions.Length - 1];
            float tailRotation = (segmentPositions[segmentPositions.Length - 2] - segmentPositions[segmentPositions.Length - 1]).ToRotation();
            spriteBatch.Draw(tailTexture, tailPosition - screenPos, tailTexture.Frame(), NPC.IsABestiaryIconDummy ? Color.White : NPC.GetNPCColorTintedByBuffs(Lighting.GetColor((int)(tailPosition.X / 16), (int)(tailPosition.Y / 16))), tailRotation, new Vector2(18, 9), 1f, SpriteEffects.None, 0f);

            //draw body
            Texture2D bodyTexture = BodyTexture.Value;
            for (int i = segmentPositions.Length - 1; i > 0; i--)
            {
                Vector2 drawPosition = (segmentPositions[i] + segmentPositions[i - 1]) / 2;
                float rotation = (segmentPositions[i - 1] - segmentPositions[i]).ToRotation();
                float scale = 1f;//(segmentPositions.Length - i) / (float)(segmentPositions.Length - 1);

                spriteBatch.Draw(bodyTexture, drawPosition - screenPos, new Rectangle(((segmentPositions.Length + 8 - i) % 9) * 2, 0, 4, 18), NPC.IsABestiaryIconDummy ? Color.White : NPC.GetNPCColorTintedByBuffs(Lighting.GetColor((int)(drawPosition.X / 16), (int)(drawPosition.Y / 16))), rotation, new Vector2(2, 9), new Vector2(scale, 1), SpriteEffects.None, 0f);
            }

            //draw head
            Texture2D headTexture = TextureAssets.Npc[Type].Value;
            spriteBatch.Draw(headTexture, NPC.Center - screenPos, headTexture.Frame(), NPC.IsABestiaryIconDummy ? Color.White : NPC.GetNPCColorTintedByBuffs(Lighting.GetColor((int)(NPC.Center.X / 16), (int)(NPC.Center.Y / 16))), NPC.rotation, new Vector2(12, 11), new Vector2(1, 1), SpriteEffects.None, 0f);

            //draw mandibles
            Texture2D mandibleTexture = JawTexture.Value;

            //mandible distance goes from -1 to 11
            float mandibleDistance = ModUtils.Lerp(-1, 11, NPC.ai[3]);

            spriteBatch.Draw(mandibleTexture, NPC.Center - screenPos, mandibleTexture.Frame(), NPC.IsABestiaryIconDummy ? Color.White : NPC.GetNPCColorTintedByBuffs(Lighting.GetColor((int)(NPC.Center.X / 16), (int)(NPC.Center.Y / 16))), NPC.rotation, new Vector2(14, mandibleDistance + 12), 1f, SpriteEffects.None, 0f);
            spriteBatch.Draw(mandibleTexture, NPC.Center - screenPos, mandibleTexture.Frame(), NPC.IsABestiaryIconDummy ? Color.White : NPC.GetNPCColorTintedByBuffs(Lighting.GetColor((int)(NPC.Center.X / 16), (int)(NPC.Center.Y / 16))), NPC.rotation, new Vector2(14, -mandibleDistance), 1f, SpriteEffects.FlipVertically, 0f);


            return false;
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            if (!spawnInfo.Granite || Main.tile[spawnInfo.PlayerFloorX, spawnInfo.PlayerFloorY].WallType != WallID.GraniteUnsafe) return 0f;
            //there can only be one
            if (NPC.AnyNPCs(NPC.type)) return 0f;

            return 0.025f;
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            npcLoot.Add(ItemDropRule.Common(ItemID.Spaghetti, 50));
            npcLoot.Add(ItemDropRule.Common(ItemID.Granite, 1, 5, 10));
            npcLoot.Add(ItemDropRule.Common(ItemID.NightVisionHelmet, 30));
            npcLoot.Add(ItemDropRule.Common(ItemID.Geode, 20));
            //TODO: npcLoot.Add(ItemDropRule.Common(ItemType<BlueQuartz>(), 2, 1, 2));
        }

        public override void ModifyHitPlayer(Player target, ref int damage, ref bool crit)
        {
            if (NPC.GetGlobalNPC<MultiHitboxNPC>().mostRecentHitbox.index != 0)
            {
                damage = (damage * 2) / 3;
            }
        }

        public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit)
        {
            if (NPC.GetGlobalNPC<MultiHitboxNPC>().mostRecentHitbox.index != 0)
            {
                damage = (damage * 2) / 3;
            }
        }
    }
}
