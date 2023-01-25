using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Polarities.Biomes.Fractal;
using Polarities.Items.Materials;
using Polarities.Items.Placeable.Banners;
using Polarities.Items.Placeable.Blocks.Fractal;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Polarities.NPCs.Enemies.Fractal
{
    public class BisectorHead : Bisector
    {
        //custom hitbox code is how it's done in Imperious from QWERTY's mod
        private int[] hitBoxSegmentIds = { -1, -1, -1, -1 };

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 1;
            NPCID.Sets.NPCBestiaryDrawOffset[Type] = new NPCID.Sets.NPCBestiaryDrawModifiers(0) { CustomTexturePath = "Polarities/Textures/Bestiary/Bisector", };
        }

        public override void SetDefaults()
        {
            NPC.aiStyle = -1;
            NPC.width = 22;
            NPC.height = 22;
            DrawOffsetY = 50;

            NPC.defense = 40;
            NPC.damage = 50;
            NPC.knockBackResist = 0f;
            NPC.lifeMax = 320;
            NPC.noTileCollide = true;
            NPC.noGravity = true;
            NPC.lavaImmune = true;
            NPC.npcSlots = 1f;
            NPC.behindTiles = true;
            NPC.value = Item.buyPrice(silver: 1);
            NPC.HitSound = SoundID.NPCHit2;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.buffImmune[BuffID.Confused] = true;

            NPC.chaseable = false;

            Banner = NPCType<BisectorHead>();
            BannerItem = ItemType<BisectorBanner>();

            this.SetModBiome<FractalBiome, FractalUGBiome, FractalWastesBiome>();
        }

        public override void Init()
        {
            base.Init();
            head = true;
            directional = true;
        }

        public override bool? CanBeHitByItem(Player player, Item item)
        {
            return false;
        }

        public override bool? CanBeHitByProjectile(Projectile projectile)
        {
            return false;
        }

        public override bool? DrawHealthBar(byte hbPosition, ref float scale, ref Vector2 position)
        {
            position.Y -= 54;
            return true;
        }

        private int attackCounter;

        public override void CustomBehavior()
        {
            NPC.hide = Lighting.Brightness((int)(NPC.Center.X / 16), (int)(NPC.Center.Y / 16)) == 0f;

            Player player = Main.player[NPC.target];
            if (!player.active || player.dead)
            {
                NPC.TargetClosest(false);
                player = Main.player[NPC.target];
            }

            NPC.noGravity = Main.tile[(int)(NPC.Center.X / 16), (int)(NPC.Center.Y / 16)].LiquidAmount > 0 || Main.tile[(int)(NPC.Center.X / 16), (int)(NPC.Center.Y / 16)].HasTile && Main.tileSolid[Main.tile[(int)(NPC.Center.X / 16), (int)(NPC.Center.Y / 16)].TileType];

            NPC.netUpdate = true;

            if (NPC.noGravity)
            {
                Vector2 targetPoint = player.Center;
                Vector2 velocityGoal = 16 * (targetPoint - NPC.Center).SafeNormalize(Vector2.Zero);
                NPC.velocity += (velocityGoal - NPC.velocity) / 60;
                NPC.netUpdate = true;

                attackCounter++;
                if (attackCounter >= 60)
                {
                    int numDirections = 4;
                    if (Main.expertMode && NPC.life * 2 < NPC.lifeMax)
                    {
                        numDirections = 8;
                    }

                    for (int i = 0; i < numDirections; i++)
                    {
                        if ((NPC.DirectionTo(player.Center) - new Vector2(1, 0).RotatedBy(MathHelper.TwoPi * i / numDirections)).Length() < 0.02f)
                        {
                            attackCounter = 0;
                            NPC.velocity = new Vector2(12, 0).RotatedBy(MathHelper.TwoPi * i / numDirections);
                            break;
                        }
                    }
                }
            }

            if (NPC.velocity.Length() < 6 && NPC.velocity.Length() > 0.1f && NPC.noGravity)
            {
                NPC.velocity = 6 * NPC.velocity / NPC.velocity.Length();
            }


            //Custom hitbox based on Qwerty's mod
            //position hitbox segments
            NPC.realLife = NPC.whoAmI;
            for (int h = 0; h < hitBoxSegmentIds.Length; h++)
            {
                Vector2 spot = NPC.Center + NPC.velocity + new Vector2(0, -h * (54 / hitBoxSegmentIds.Length)).RotatedBy(NPC.rotation); //QwertyMethods.PolarVector((totalLength - bladeLength - 18) + h * (bladeLength / (hitBoxSegmentIds.Length + 1)) + bladeWidth / 2, npc.rotation);
                if (hitBoxSegmentIds[h] == -1 || !Main.npc[hitBoxSegmentIds[h]].active || Main.npc[hitBoxSegmentIds[h]].type != NPCType<BisectorHeadHitbox>())
                {
                    if (Main.netMode != 1)
                    {
                        hitBoxSegmentIds[h] = NPC.NewNPC(NPC.GetSource_FromThis(), (int)spot.X, (int)spot.Y, NPCType<BisectorHeadHitbox>(), ai0: NPC.whoAmI);
                        NPC.netUpdate = true;
                    }
                }
                else
                {
                    Main.npc[hitBoxSegmentIds[h]].Center = spot;
                    Main.npc[hitBoxSegmentIds[h]].timeLeft = 10;
                    Main.npc[hitBoxSegmentIds[h]].dontTakeDamage = NPC.dontTakeDamage;
                    Main.npc[hitBoxSegmentIds[h]].defense = NPC.defense;
                    Main.npc[hitBoxSegmentIds[h]].defDefense = NPC.defDefense;
                    Main.npc[hitBoxSegmentIds[h]].damage = NPC.damage;
                }
            }
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            for (int i = 0; i < hitBoxSegmentIds.Length; i++)
            {
                writer.Write(hitBoxSegmentIds[i]);
            }
        }
        public override void ReceiveExtraAI(BinaryReader reader)
        {
            for (int i = 0; i < hitBoxSegmentIds.Length; i++)
            {
                hitBoxSegmentIds[i] = reader.ReadInt32();
            }
        }

        public override void HitEffect(int hitDirection, double damage)
        {
            if (NPC.life <= 0 && Main.netMode != NetmodeID.Server)
            {
                GoreHelper.DeathGore(NPC, "BisectorGore1", velocity: NPC.velocity);
                GoreHelper.DeathGore(NPC, "BisectorGore2", velocity: NPC.velocity);
            }
        }
        public override bool CheckDead()
        {
            return true;
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
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<FractalDuststone>(), minimumDropped: 1, maximumDropped: 3));
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<FractalResidue>(), chanceDenominator: 2));
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            //if (Subworld.IsActive<FractalSubworld>())
            //{
            //    return FractalSubworld.SpawnConditionFractalWastes(spawnInfo) * FractalSubworld.SpawnConditionFractalUnderground(spawnInfo);
            //}
            return 0f;
        }
    }

    public class BisectorHeadHitbox : ModNPC
    {
        public override string Texture => Polarities.CallShootProjectile;

        public override void SetStaticDefaults()
        {
            NPCID.Sets.NPCBestiaryDrawOffset[Type] = new NPCID.Sets.NPCBestiaryDrawModifiers(0) { Hide = true, };
        }

        public override void SetDefaults()
        {
            NPC.aiStyle = -1;
            NPC.width = 22;
            NPC.height = 22;

            NPC.defense = 40;
            NPC.damage = 50;
            NPC.lifeMax = 320;
            NPC.knockBackResist = 0f;

            NPC.value = 5000;
            NPC.npcSlots = 0f;
            NPC.dontCountMe = true;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.HitSound = SoundID.NPCHit2;
            NPC.DeathSound = SoundID.NPCDeath1;

            NPC.buffImmune[BuffID.Confused] = true;

            Banner = NPCType<BisectorHead>();
            BannerItem = ItemType<BisectorBanner>();
        }

        public override bool CheckActive()
        {
            return Main.npc[NPC.realLife].active;
        }

        public override void AI()
        {
            NPC.realLife = (int)NPC.ai[0];
            NPC.active = Main.npc[NPC.realLife].active;
        }

        public override void OnHitByProjectile(Projectile projectile, int damage, float knockback, bool crit)
        {
            List<int> hitboxIds = new List<int>();
            for (int i = 0; i < Main.npc.Length; i++)
            {
                if (Main.npc[i].type == NPC.type && Main.npc[i].realLife == NPC.realLife)
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
                if (Main.npc[i].type == NPC.type && Main.npc[i].realLife == NPC.realLife)
                {
                    hitboxIds.Add(i);
                }
            }
            foreach (int who in hitboxIds)
            {
                Main.npc[who].immune[player.whoAmI] = NPC.immune[player.whoAmI];
            }
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            return false;
        }

        public override bool? DrawHealthBar(byte hbPosition, ref float scale, ref Vector2 position)
        {
            return false;
        }
    }

    public class BisectorBody1 : Bisector
    {
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 1;
            NPCID.Sets.NPCBestiaryDrawOffset[Type] = new NPCID.Sets.NPCBestiaryDrawModifiers(0) { Hide = true, };
        }

        public override void SetDefaults()
        {
            NPC.aiStyle = -1;
            NPC.width = 20;
            NPC.height = 20;
            DrawOffsetY = -4;

            NPC.defense = 0;
            NPC.damage = 20;
            NPC.knockBackResist = 0f;
            NPC.lifeMax = 320;
            NPC.noTileCollide = true;
            NPC.noGravity = true;
            NPC.lavaImmune = true;
            NPC.npcSlots = 0f;
            NPC.behindTiles = true;
            NPC.value = Item.buyPrice(silver: 1);
            NPC.dontCountMe = true;
            NPC.HitSound = SoundID.NPCHit2;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.buffImmune[BuffID.Confused] = true;

            Banner = NPCType<BisectorHead>();
            BannerItem = ItemType<BisectorBanner>();
        }

        public override void Init()
        {
            base.Init();
            head = false;
            directional = true;

            bodyType = NPCType<BisectorBody2>();
            segmentLength = 10;
        }

        public override void HitEffect(int hitDirection, double damage)
        {
            if (NPC.life <= 0 && Main.netMode != NetmodeID.Server)
            {
                GoreHelper.DeathGore(NPC, "BisectorGore3", velocity: NPC.velocity);
            }
        }

        public override void CustomBehavior()
        {
            NPC.life = NPC.lifeMax;
            if (!Main.npc[NPC.realLife].active && Main.npc[NPC.realLife].life <= 1)
            {
                NPC.life = -1;
                NPC.HitEffect();
                NPC.active = false;
            }
        }
    }

    public class BisectorBody2 : Bisector
    {
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 1;
            NPCID.Sets.NPCBestiaryDrawOffset[Type] = new NPCID.Sets.NPCBestiaryDrawModifiers(0) { Hide = true, };
        }
        public override void SetDefaults()
        {
            NPC.aiStyle = -1;
            NPC.width = 12;
            NPC.height = 12;
            DrawOffsetY = -4;

            NPC.defense = 0;
            NPC.damage = 20;
            NPC.knockBackResist = 0f;
            NPC.lifeMax = 320;
            NPC.noTileCollide = true;
            NPC.noGravity = true;
            NPC.lavaImmune = true;
            NPC.npcSlots = 0f;
            NPC.behindTiles = true;
            NPC.value = Item.buyPrice(silver: 1);
            NPC.dontCountMe = true;
            NPC.HitSound = SoundID.NPCHit2;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.buffImmune[BuffID.Confused] = true;

            Banner = NPCType<BisectorHead>();
            BannerItem = ItemType<BisectorBanner>();
        }

        public override void Init()
        {
            base.Init();
            head = false;
            directional = true;

            segmentLength = 6;
        }

        public override void HitEffect(int hitDirection, double damage)
        {
            if (NPC.life <= 0 && Main.netMode != NetmodeID.Server)
            {
                GoreHelper.DeathGore(NPC, "BisectorGore4", velocity: NPC.velocity);
            }
        }

        public override void CustomBehavior()
        {
            NPC.life = NPC.lifeMax;
            if (!Main.npc[NPC.realLife].active && Main.npc[NPC.realLife].life <= 1)
            {
                NPC.life = -1;
                NPC.HitEffect();
                NPC.active = false;
            }
        }
    }

    public class BisectorTail : Bisector
    {
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 1;
            NPCID.Sets.NPCBestiaryDrawOffset[Type] = new NPCID.Sets.NPCBestiaryDrawModifiers(0) { Hide = true, };
        }
        public override void SetDefaults()
        {
            NPC.aiStyle = -1;
            NPC.width = 12;
            NPC.height = 12;
            DrawOffsetY = -4;

            NPC.defense = 0;
            NPC.damage = 20;
            NPC.knockBackResist = 0f;
            NPC.lifeMax = 280;
            NPC.noTileCollide = true;
            NPC.noGravity = true;
            NPC.lavaImmune = true;
            NPC.npcSlots = 0f;
            NPC.behindTiles = true;
            NPC.value = Item.buyPrice(silver: 1);
            NPC.dontCountMe = true;
            NPC.HitSound = SoundID.NPCHit2;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.buffImmune[BuffID.Confused] = true;

            Banner = NPCType<BisectorHead>();
            BannerItem = ItemType<BisectorBanner>();
        }

        public override void Init()
        {
            base.Init();
            tail = true;
            directional = true;

            segmentLength = 6;
        }

        public override void HitEffect(int hitDirection, double damage)
        {
            if (NPC.life <= 0 && Main.netMode != NetmodeID.Server)
            {
                GoreHelper.DeathGore(NPC, "BisectorGore5", velocity: NPC.velocity);
            }
        }

        public override void CustomBehavior()
        {
            NPC.life = NPC.lifeMax;
            if (!Main.npc[NPC.realLife].active && Main.npc[NPC.realLife].life <= 1)
            {
                NPC.life = -1;
                NPC.HitEffect();
                NPC.active = false;
            }
        }
    }

    public abstract class Bisector : LegacyWorm
    {
        public override void SetDefaults()
        {
        }

        public override void Init()
        {
            maxLength = 4 - 2;
            minLength = 4 - 2;
            tailType = NPCType<BisectorTail>();
            bodyType = NPCType<BisectorBody1>();
            headType = NPCType<BisectorHead>();
            segmentLength = 11;
            speed = 12f;
            turnSpeed = 0f;
            digSounds = true;
        }
    }
}