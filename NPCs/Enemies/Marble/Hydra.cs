using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Polarities.Items.Accessories;
using Polarities.Items.Materials;
using Polarities.Items.Placeable.Banners;
using Polarities.Items.Weapons.Melee;
using Polarities.Items.Weapons.Summon.Whips;
using ReLogic.Content;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Polarities.NPCs.Enemies.Marble
{
    public class HydraBody : ModNPC
    {
        //TODO: Custom bestiary image, proper bestiary loot drops
        public override void SetStaticDefaults()
        {
            NPCID.Sets.NPCBestiaryDrawModifiers drawModifiers = new NPCID.Sets.NPCBestiaryDrawModifiers(0)
            {
                SpriteDirection = 1
            };
            NPCID.Sets.NPCBestiaryDrawOffset.Add(NPC.type, drawModifiers);

            Main.npcFrameCount[Type] = 1;
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
                //spawn conditions
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Marble,
				//flavor text
				this.TranslatedBestiaryEntry()
            });
        }

        public override void SetDefaults()
        {
            NPC.aiStyle = -1;
            NPC.width = 72;
            NPC.height = 44;
            NPC.defense = 30;
            NPC.lifeMax = 2800;
            NPC.damage = 0;
            NPC.knockBackResist = 0f;
            NPC.npcSlots = 1f;

            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;

            NPC.value = Item.buyPrice(gold: 2);

            NPC.hide = true;

            Banner = Type;
            BannerItem = ItemType<HydraBanner>();
        }

        public override void AI()
        {
            if (NPC.localAI[0] == 0)
            {
                NPC.localAI[0] = 1;

                if (Main.netMode != 1)
                {
                    NPC.NewNPC(NPC.GetSource_FromAI(), (int)NPC.Center.X, (int)NPC.Center.Y, NPCType<HydraHead>(), ai0: NPC.whoAmI);

                    if (Main.expertMode)
                    {
                        for (int i = 0; i < 2; i++)
                        {
                            if (Main.rand.NextBool(2))
                            {
                                NPC.NewNPC(NPC.GetSource_FromAI(), (int)NPC.Center.X, (int)NPC.Center.Y, NPCType<HydraHead>(), ai0: NPC.whoAmI);
                            }
                        }
                    }
                }
            }

            NPC.TargetClosest(true);

            NPC.spriteDirection = -NPC.direction;
        }

        public override bool CheckDead()
        {
            Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, Mod.Find<ModGore>("HydraGore").Type);

            return true;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (NPC.IsABestiaryIconDummy)
            {
                HydraHead headDummy = GetModNPC(NPCType<HydraHead>()) as HydraHead;
                for (int i = 0; i < 2; i++)
                {
                    Vector2 rootPosition = NPC.Center + new Vector2(24 * NPC.direction, -6);
                    float rotationAmount = ((float)i + 1) / 3 * 2 - 1;
                    Vector2 goalPosition = rootPosition + new Vector2(0, -52 - 12).RotatedBy(rotationAmount);
                    headDummy.DrawAt(NPC, goalPosition, spriteBatch, screenPos, drawColor, true);
                }
            }
            return true;
        }

        public override void DrawBehind(int index)
        {
            Main.instance.DrawCacheNPCProjectiles.Add(index);
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            if (!spawnInfo.Marble || Main.tile[spawnInfo.PlayerFloorX, spawnInfo.PlayerFloorY].WallType != WallID.MarbleUnsafe) return 0f;
            return (Main.hardMode) ? 0.025f : 0f;
        }
    }

    public class HydraHead : ModNPC
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("{$Mods.Polarities.NPCName.HydraBody}");

            Main.npcFrameCount[NPC.type] = 2;

            var drawModifier = new NPCID.Sets.NPCBestiaryDrawModifiers(0)
            {
                //don't show up in bestiary
                Hide = true
            };
            NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, drawModifier);

            NPCID.Sets.PositiveNPCTypesExcludedFromDeathTally[Type] = true;
        }

        public override void SetDefaults()
        {
            NPC.aiStyle = -1;
            NPC.width = 34;
            NPC.height = 34;

            NPC.defense = 10;
            NPC.lifeMax = Main.masterMode ? 400 / 3 : Main.expertMode ? 300 / 2 : 200;

            NPC.damage = 44;
            NPC.knockBackResist = 0.05f;
            NPC.npcSlots = 0f;
            NPC.dontCountMe = true;

            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.dontTakeDamage = true;

            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;

            Banner = NPCType<HydraBody>();
            BannerItem = ItemType<HydraBanner>();
        }

        public override void AI()
        {
            if (NPC.localAI[0] < 30)
            {
                NPC.localAI[0]++;
                if (NPC.localAI[0] == 30)
                {
                    NPC.dontTakeDamage = false;
                }
            }

            NPC.TargetClosest(false);
            Player player = Main.player[NPC.target];
            NPC owner = Main.npc[(int)NPC.ai[0]];

            if (!owner.active)
            {
                NPC.StrikeNPC(1000, 0, 0);
                return;
            }

            switch (NPC.ai[1])
            {
                case 0:
                    //idle

                    //index and count include all heads for this
                    int index = 0;
                    int count = 0;
                    for (int i = 0; i < Main.maxNPCs; i++)
                    {
                        if (NPC.type == Main.npc[i].type && NPC.ai[0] == Main.npc[i].ai[0] && Main.npc[i].active)
                        {
                            count++;
                            if (i < NPC.whoAmI) index++;
                        }
                    }

                    if (owner.direction == -1)
                    {
                        index = count - index - 1;
                    }

                    Vector2 rootPosition = owner.Center + new Vector2(24 * owner.direction, -6);
                    float rotationAmount = ((float)index + 1) / (count + 1) * 2 - 1;
                    Vector2 goalPosition = rootPosition + new Vector2(0, -52 - count * 6).RotatedBy(rotationAmount);

                    //orbit around the goal position at a very short distance, clockwise if the goal is -1 and counterclockwise if it's 1, with more horizontal motion than vertical, with each head at an offset from its neighbor
                    Vector2 goalPositionModifier = new Vector2(10, 0).RotatedBy(owner.direction * PolaritiesSystem.timer * 0.1f + ((float)index) / count * MathHelper.TwoPi);
                    goalPositionModifier.Y *= 0.5f;

                    goalPosition += goalPositionModifier;

                    NPC.velocity += (((goalPosition - NPC.Center) / 4) - NPC.velocity) / 10;

                    NPC.rotation = 0;
                    NPC.direction = owner.direction;
                    NPC.spriteDirection = -NPC.direction;

                    NPC.ai[2]++;
                    if (NPC.ai[2] >= 120)
                    {
                        NPC.ai[2] = 0;

                        if (Main.netMode != 1 && (player.Center - rootPosition).Length() < 1200 && Collision.CanHit(NPC.position, NPC.width, NPC.height, player.position, player.width, player.height))
                        {
                            NPC.ai[1] = Main.rand.Next(1, 4);
                        }
                        NPC.netUpdate = true;
                    }
                    break;
                case 1:
                    //spray venom at player
                    if (NPC.ai[2] == 0 && !Collision.CanHit(NPC.position, NPC.width, NPC.height, player.position, player.width, player.height))
                    {
                        NPC.ai[2] = 119;
                    }

                    //index and count include all heads for this
                    index = 0;
                    count = 0;
                    for (int i = 0; i < Main.maxNPCs; i++)
                    {
                        if (NPC.type == Main.npc[i].type && NPC.ai[0] == Main.npc[i].ai[0] && Main.npc[i].active)
                        {
                            count++;
                            if (i < NPC.whoAmI) index++;
                        }
                    }

                    if (owner.direction == -1)
                    {
                        index = count - index - 1;
                    }

                    rootPosition = owner.Center + new Vector2(24 * owner.direction, -6);
                    rotationAmount = ((float)index + 1) / (count + 1) * 2 - 1;
                    goalPosition = rootPosition + new Vector2(0, -52 - count * 6).RotatedBy(rotationAmount);

                    //orbit around the goal position at a very short distance, clockwise if the goal is -1 and counterclockwise if it's 1, with more horizontal motion than vertical, with each head at an offset from its neighbor
                    goalPositionModifier = new Vector2(10, 0).RotatedBy(owner.direction * PolaritiesSystem.timer * 0.1f + ((float)index) / count * MathHelper.TwoPi);
                    goalPositionModifier.Y *= 0.5f;

                    goalPosition += goalPositionModifier;

                    NPC.velocity += (((goalPosition - NPC.Center) / 4) - NPC.velocity) / 10;

                    NPC.direction = NPC.Center.X > player.Center.X ? -1 : 1; ;
                    NPC.spriteDirection = -NPC.direction;
                    NPC.rotation = (player.Center - NPC.Center).ToRotation();
                    if (NPC.direction == -1)
                    {
                        NPC.rotation += MathHelper.Pi;
                    }

                    if (NPC.ai[2] >= 100 && NPC.ai[2] % 5 == 0 && Main.netMode != 1)
                    {
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, 16 * (player.Center - NPC.Center).SafeNormalize(Vector2.Zero).RotatedByRandom(0.05f), ProjectileType<HydraVenom>(), 20, 1f, Main.myPlayer);
                    }
                    if (NPC.ai[2] == 100)
                    {
                        SoundEngine.PlaySound(SoundID.Item13, NPC.Center);
                    }

                    NPC.ai[2]++;
                    if (NPC.ai[2] == 120)
                    {
                        NPC.ai[1] = 0;
                        NPC.ai[2] = 0;
                    }
                    break;
                case 2:
                    //spray venom directly outwards
                    if (NPC.ai[2] == 0 && !Collision.CanHit(NPC.position, NPC.width, NPC.height, player.position, player.width, player.height))
                    {
                        NPC.ai[2] = 119;
                    }

                    //index and count include all heads for this
                    index = 0;
                    count = 0;
                    for (int i = 0; i < Main.maxNPCs; i++)
                    {
                        if (NPC.type == Main.npc[i].type && NPC.ai[0] == Main.npc[i].ai[0] && Main.npc[i].active)
                        {
                            count++;
                            if (i < NPC.whoAmI) index++;
                        }
                    }

                    if (owner.direction == -1)
                    {
                        index = count - index - 1;
                    }

                    rootPosition = owner.Center + new Vector2(24 * owner.direction, -6);
                    rotationAmount = ((float)index + 1) / (count + 1) * 2 - 1;
                    goalPosition = rootPosition + new Vector2(0, -52 - count * 6).RotatedBy(rotationAmount);

                    //orbit around the goal position at a very short distance, clockwise if the goal is -1 and counterclockwise if it's 1, with more horizontal motion than vertical, with each head at an offset from its neighbor
                    goalPositionModifier = new Vector2(10, 0).RotatedBy(owner.direction * PolaritiesSystem.timer * 0.1f + ((float)index) / count * MathHelper.TwoPi);
                    goalPositionModifier.Y *= 0.5f;

                    goalPosition += goalPositionModifier;

                    NPC.velocity += (((goalPosition - NPC.Center) / 4) - NPC.velocity) / 10;

                    NPC.direction = rootPosition.X > NPC.Center.X ? -1 : 1;
                    NPC.spriteDirection = -NPC.direction;
                    NPC.rotation = (NPC.Center - rootPosition).ToRotation();
                    if (NPC.direction == -1)
                    {
                        NPC.rotation += MathHelper.Pi;
                    }

                    if (NPC.ai[2] >= 100 && NPC.ai[2] % 5 == 0 && Main.netMode != 1)
                    {
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, 16 * (NPC.Center - rootPosition).SafeNormalize(Vector2.Zero), ProjectileType<HydraVenom>(), 20, 1f, Main.myPlayer);
                    }
                    if (NPC.ai[2] == 100)
                    {
                        SoundEngine.PlaySound(SoundID.Item13, NPC.Center);
                    }

                    NPC.ai[2]++;
                    if (NPC.ai[2] == 120)
                    {
                        NPC.ai[1] = 0;
                        NPC.ai[2] = 0;
                    }
                    break;
                case 3:
                    //hover in area to the side of player before lunging
                    if (NPC.ai[2] == 0 && !Collision.CanHit(NPC.position, NPC.width, NPC.height, player.position, player.width, player.height))
                    {
                        NPC.ai[2] = 119;
                    }

                    if (NPC.ai[2] < 100)
                    {
                        NPC.direction = NPC.Center.X > player.Center.X ? -1 : 1; ;
                        NPC.spriteDirection = -NPC.direction;
                        NPC.rotation = (player.Center - NPC.Center).ToRotation();
                        if (NPC.direction == -1)
                        {
                            NPC.rotation += MathHelper.Pi;
                        }

                        goalPosition = player.Center + new Vector2(-NPC.direction * 120, 0);
                        NPC.velocity += (((goalPosition - NPC.Center) / 30) - NPC.velocity) / 15;
                    }
                    else if (NPC.ai[2] == 105)
                    {
                        NPC.velocity = (player.Center - NPC.Center).SafeNormalize(Vector2.Zero) * 9f;
                    }

                    NPC.ai[2]++;
                    if (NPC.ai[2] == 120)
                    {
                        NPC.ai[1] = 0;
                        NPC.ai[2] = 0;
                    }
                    break;
            }
        }

        public override void FindFrame(int frameHeight)
        {
            NPC.frame.Y = 0;
            if (NPC.ai[1] != 0 && NPC.ai[2] >= 100)
            {
                NPC.frame.Y = frameHeight;
            }
        }

        public override bool CanHitPlayer(Player target, ref int cooldownSlot)
        {
            if (NPC.ai[1] == 3 && NPC.ai[2] >= 100) return true;
            return false;
        }

        public override void OnHitPlayer(Player target, int damage, bool crit)
        {
            target.AddBuff(BuffID.Venom, 300);
        }

        public override void OnHitByProjectile(Projectile projectile, int damage, float knockback, bool crit)
        {
            List<int> hitboxIds = new List<int>();
            for (int i = 0; i < Main.npc.Length; i++)
            {
                if (Main.npc[i].type == NPC.type && Main.npc[i].ai[0] == NPC.ai[0])
                {
                    hitboxIds.Add(i);
                }
            }
            if (projectile.usesLocalNPCImmunity || projectile.localNPCImmunity[NPC.whoAmI] != 0)
            {
                foreach (int who in hitboxIds)
                {
                    projectile.localNPCImmunity[who] = projectile.localNPCImmunity[NPC.whoAmI];
                    Main.npc[who].immune[projectile.owner] = NPC.immune[projectile.owner];
                }
            }
            else if (projectile.usesIDStaticNPCImmunity)
            {
                foreach (int who in hitboxIds)
                {

                    Projectile.perIDStaticNPCImmunity[projectile.type][who] = Projectile.perIDStaticNPCImmunity[projectile.type][NPC.whoAmI];
                }
            }
            else
            {
                foreach (int who in hitboxIds)
                {
                    Main.npc[who].immune[projectile.owner] = NPC.immune[projectile.owner];
                }
            }
        }

        public override void OnHitByItem(Player player, Item item, int damage, float knockback, bool crit)
        {
            List<int> hitboxIds = new List<int>();
            for (int i = 0; i < Main.npc.Length; i++)
            {
                if (Main.npc[i].type == NPC.type && Main.npc[i].ai[0] == NPC.ai[0])
                {
                    hitboxIds.Add(i);
                }
            }
            foreach (int who in hitboxIds)
            {
                Main.npc[who].immune[player.whoAmI] = NPC.immune[player.whoAmI];
            }
        }

        public override bool CheckActive()
        {
            return false;
        }

        public override bool CheckDead()
        {
            Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, Mod.Find<ModGore>("HydraHeadGore" + Main.rand.Next(1, 3)).Type);

            NPC owner = Main.npc[(int)NPC.ai[0]];
            if (!owner.active)
            {
                return true;
            }

            NPC.NewNPC(NPC.GetSource_Death(), (int)NPC.Center.X, (int)NPC.Center.Y, NPCType<HydraHead>(), ai0: NPC.ai[0]);

            int count = 0;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                if (NPC.type == Main.npc[i].type && NPC.ai[0] == Main.npc[i].ai[0] && Main.npc[i].active && NPC.whoAmI != i)
                {
                    count++;
                }
            }

            if (count < (Main.expertMode ? 9 : 7))
            {
                NPC.NewNPC(NPC.GetSource_Death(), (int)NPC.Center.X, (int)NPC.Center.Y, NPCType<HydraHead>(), ai0: NPC.ai[0]);
            }

            return true;
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            npcLoot.Add(ItemDropRule.Common(ItemType<VenomGland>(), 25, 1, 3));
            npcLoot.Add(ItemDropRule.Common(ItemType<Lernaean>(), 200));
            npcLoot.Add(ItemDropRule.Common(ItemType<HydraHide>(), 200));
            npcLoot.Add(ItemDropRule.Common(ItemType<HeadSplitter>(), 200));
        }

        public static Asset<Texture2D> ChainTexture;

        public override void Load()
        {
            ChainTexture = Request<Texture2D>(Texture + "_Chain");
        }

        public override void Unload()
        {
            ChainTexture = null;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            drawColor = NPC.GetNPCColorTintedByBuffs(drawColor);

            NPC owner = Main.npc[(int)NPC.ai[0]];
            DrawAt(owner, NPC.Center, spriteBatch, screenPos, drawColor);
            return true;
        }

        public void DrawAt(NPC owner, Vector2 center, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor, bool bestiaryDummy = false)
        {
            int direction = bestiaryDummy ? -1 : NPC.direction;

            Vector2 rootPosition = owner.Center + new Vector2(20 * owner.direction, 4);
            Vector2 endPosition = center + new Vector2(-14 * direction, 0).RotatedBy(NPC.rotation);

            float distMult = (rootPosition - endPosition).Length();

            Vector2[] bezierPoints = { rootPosition, rootPosition + new Vector2(0, -distMult / 3f), center + new Vector2(-(14 + distMult / 4f) * direction, 0).RotatedBy(NPC.rotation), endPosition };
            float bezierProgress = 0;
            float bezierIncrement = 2;

            Texture2D texture = ChainTexture.Value;
            Vector2 textureCenter = direction == 1 ? new Vector2(1, 17) : new Vector2(1, 15);

            float rotation;

            int index = 0;
            while (bezierProgress < 1)
            {
                //draw stuff
                Vector2 oldPos = ModUtils.BezierCurve(bezierPoints, bezierProgress);

                //increment progress
                while ((oldPos - ModUtils.BezierCurve(bezierPoints, bezierProgress)).Length() < bezierIncrement)
                {
                    bezierProgress += 0.05f / ModUtils.BezierCurveDerivative(bezierPoints, bezierProgress).Length();
                }

                Vector2 newPos = ModUtils.BezierCurve(bezierPoints, bezierProgress);
                rotation = (newPos - oldPos).ToRotation() + MathHelper.Pi;

                Rectangle frame = new Rectangle(index * 6 % 7 * 2, 0, 4, texture.Height);

                spriteBatch.Draw(texture, (oldPos + newPos) / 2 - screenPos, frame, drawColor, rotation, textureCenter, NPC.scale, direction == 1 ? SpriteEffects.None : SpriteEffects.FlipVertically, 0f);

                index++;
            }

            if (bestiaryDummy)
            {
                float num246 = Main.NPCAddHeight(NPC);
                SpriteEffects spriteEffects = (SpriteEffects)1;
                Vector2 halfSize = new Vector2(TextureAssets.Npc[Type].Width() / 2, TextureAssets.Npc[Type].Height() / Main.npcFrameCount[Type] / 2);
                Rectangle frame = new Rectangle(0, 0, (int)halfSize.X * 2, (int)halfSize.Y * 2);

                spriteBatch.Draw(TextureAssets.Npc[Type].Value, center + new Vector2(0, 17) - screenPos + new Vector2(-TextureAssets.Npc[Type].Width() * NPC.scale / 2f + halfSize.X * NPC.scale, -TextureAssets.Npc[Type].Height() * NPC.scale / Main.npcFrameCount[Type] + 4f + halfSize.Y * NPC.scale + num246 + NPC.gfxOffY), frame, drawColor, NPC.rotation, halfSize, NPC.scale, spriteEffects, 0f);
            }
        }
    }

    public class HydraVenom : ModProjectile
    {
        public override string Texture => "Polarities/Textures/Pixel";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("{$Mods.Polarities.ProjectileName.SpitterVenom}");
        }

        public override void SetDefaults()
        {
            Projectile.aiStyle = -1;
            Projectile.hostile = true;
            Projectile.width = 2;
            Projectile.height = 2;
            Projectile.alpha = 0;
            Projectile.timeLeft = 3600;
            Projectile.penetrate = 1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.hide = true;
        }

        public override void AI()
        {
            Main.dust[Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.ToxicBubble, Scale: 1.5f)].noGravity = true;
            Main.dust[Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.ToxicBubble, Scale: 1.5f)].noGravity = true;
            Projectile.velocity.Y += 0.2f;
        }
        public override void OnHitPlayer(Player target, int damage, bool crit)
        {
            target.AddBuff(BuffID.Venom, 300);
        }
    }
}