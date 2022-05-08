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

namespace Polarities.NPCs.Enemies.Limestone
{
    public class GlowWorm : ModNPC
    {
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 1;

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
            NPC.aiStyle = -1;
            NPC.width = 30;
            NPC.height = 30;
            DrawOffsetY = -4;

            NPC.damage = 30;
            NPC.lifeMax = 100;
            NPC.defense = 4;

            NPC.knockBackResist = 0f;
            NPC.npcSlots = 1f;
            NPC.value = Item.buyPrice(silver: 6);
            NPC.HitSound = SoundID.NPCHit2;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.lavaImmune = true;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.behindTiles = true;

            Banner = NPC.type;
            BannerItem = ItemType<GlowWormBanner>();

            SpawnModBiomes = new int[1] { GetInstance<LimestoneCave>().Type };
        }

        private Point rootTile;
        private float curvature;
        private float dCurvature;
        private Vector2 rootPosition
        {
            get => new Vector2(rootTile.X * 16 + 8, rootTile.Y * 16 + 24);
        }
        private float LENGTH = 1;
        private float maxLength
        {
            get => NPC.ai[0];
            set => NPC.ai[0] = value;
        }

        public override void AI()
        {
            if (NPC.localAI[0] == 0)
            {
                NPC.localAI[0] = 1;
                if (Main.netMode != 1)
                {
                    maxLength = Main.rand.Next(100, 160);
                }
                NPC.netUpdate = true;

                //placeholder rootTile location
                rootTile = new Point((int)NPC.Center.X / 16, (int)NPC.Center.Y / 16);
                while (rootTile.Y >= 0 && (!Main.tileSolid[Main.tile[rootTile.X, rootTile.Y].TileType] || !Main.tile[rootTile.X, rootTile.Y].HasUnactuatedTile))
                {
                    rootTile.Y--;
                }
                if (rootTile.Y < 0)
                {
                    NPC.active = false;
                    return;
                }
            }

            if (LENGTH < maxLength)
            {
                LENGTH += 1;
            }

            if (Main.tile[rootTile.X, rootTile.Y].TileType != TileType<LimestoneTile>() || !Main.tile[rootTile.X, rootTile.Y].HasUnactuatedTile)
            {
                NPC.active = false;
                return;
            }

            Lighting.AddLight(NPC.Center, 173f / 512, 217f / 512, 160f / 512);

            NPC.TargetClosest(false);
            Player player = Main.player[NPC.target];

            float playerFacingAmount = 0.002f;

            if (NPC.ai[1] != -1 && NPC.ai[1] != 179)
            {
                NPC.ai[1]++;

                if (((NPC.ai[1] > 30 && NPC.ai[1] < 90) || (NPC.ai[1] > 270 && NPC.ai[1] < 330)) && NPC.ai[1] % 10 == 0)
                {
                    if (Main.netMode != 1)
                    {
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, new Vector2(0, 3.5f).RotatedBy(NPC.rotation), ProjectileType<GlowWormDroplet>(), 13, 2f, Main.myPlayer);
                    }
                    SoundEngine.PlaySound(SoundID.Item, NPC.Center, 17);
                }
            }
            if (NPC.ai[1] == 120 || NPC.ai[1] == 360)
            {
                NPC.ai[1] = Main.rand.Next(new int[] { -1, -1, 179 });
            }

            if (Collision.CanHit(NPC.position, NPC.width, NPC.height, player.position, player.width, player.height))
            {
                if (NPC.ai[1] == -1 || NPC.ai[1] == 179)
                {
                    NPC.ai[1]++;
                }

                if (NPC.ai[1] <= 120)
                {
                    float playerRotation = (player.Center - NPC.Center).RotatedBy(-MathHelper.PiOver2).ToRotation();

                    if (playerRotation > curvature + dCurvature)
                    {
                        dCurvature += playerFacingAmount;
                    }
                    else if (playerRotation < curvature + dCurvature)
                    {
                        dCurvature -= playerFacingAmount;
                    }
                }
                else if (NPC.ai[1] <= 360)
                {
                    float height = 1;
                    if (curvature != 0)
                    {
                        height = (float)Math.Sin(curvature) / curvature;
                    }

                    //energy ~ c * dCurvature * dCurvature - height
                    //this all obviously isn't how the real physics of something that bends like this will work but it's close enough
                    if (10 * dCurvature * dCurvature - height < 0)
                    {
                        if (dCurvature < 0)
                        {
                            dCurvature -= playerFacingAmount;
                        }
                        else
                        {
                            dCurvature += playerFacingAmount;
                        }
                    }
                }
            }

            float gravityAmount = playerFacingAmount / MathHelper.Pi;
            dCurvature -= gravityAmount * curvature;

            if (Math.Abs(dCurvature) > 0.01f)
            {
                dCurvature *= 0.99f;
            }
            curvature += dCurvature;

            Vector2 displacementVector = new Vector2(0, 1);
            if (curvature != 0)
            {
                displacementVector = new Vector2(((float)Math.Cos(curvature) - 1) / curvature, (float)Math.Sin(curvature) / curvature);
            }
            NPC.Center = rootPosition + displacementVector * LENGTH;
            NPC.rotation = curvature;
        }

        static Asset<Texture2D> TailTexture;
        static Asset<Texture2D> TailMaskTexture;
        static Asset<Texture2D> ChainTexture;
        static Asset<Texture2D> ChainMaskTexture;

        public override void Load()
        {
            TailTexture = Request<Texture2D>(Texture + "_Tail");
            TailMaskTexture = Request<Texture2D>(Texture + "_Tail_Mask");
            ChainTexture = Request<Texture2D>(Texture + "_Chain");
            ChainMaskTexture = Request<Texture2D>(Texture + "_Chain_Mask");
        }

        public override void Unload()
        {
            TailTexture = null;
            TailMaskTexture = null;
            ChainTexture = null;
            ChainMaskTexture = null;
        }

        public override void FindFrame(int frameHeight)
        {
            if (NPC.IsABestiaryIconDummy)
            {
                NPC.frameCounter++;
            }
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color lightColor)
        {
            if (NPC.IsABestiaryIconDummy)
            {
                LENGTH = 48;
                curvature = 0.5f * (float)Math.Sin(NPC.frameCounter / 20f);
                NPC.rotation = curvature;
                Vector2 totalDisplacement = new Vector2(((float)Math.Cos(curvature) - 1) / curvature, (float)Math.Sin(curvature) / curvature) * LENGTH;

                NPC.Center += totalDisplacement - new Vector2(0, 30);

                spriteBatch.Draw(TailTexture.Value, NPC.Center - totalDisplacement - screenPos, new Rectangle(0, 0, TailTexture.Width(), TailTexture.Height()), NPC.GetNPCColorTintedByBuffs(Color.White), 0, new Vector2(TailTexture.Width() / 2, TailTexture.Height()), 1f, SpriteEffects.None, 0f);

                for (int i = 0; i < LENGTH; i += 2)
                {
                    float progress = i / LENGTH;

                    Vector2 displacementVector = new Vector2(0, progress);
                    if (curvature != 0)
                    {
                        displacementVector = new Vector2(((float)Math.Cos(progress * curvature) - 1) / curvature, (float)Math.Sin(progress * curvature) / curvature);
                    }
                    Vector2 drawPos = NPC.Center - totalDisplacement + displacementVector * LENGTH;

                    spriteBatch.Draw(ChainTexture.Value, drawPos - screenPos, new Rectangle(0, i % ChainTexture.Height(), ChainTexture.Width(), 4), NPC.GetNPCColorTintedByBuffs(Color.White), progress * curvature, new Vector2(ChainTexture.Width() / 2, 2), 1f, SpriteEffects.None, 0f);
                }

                return true;
            }

            spriteBatch.Draw(TailTexture.Value, rootPosition - screenPos, new Rectangle(0, 0, TailTexture.Width(), TailTexture.Height()), NPC.GetNPCColorTintedByBuffs(Lighting.GetColor((int)rootPosition.X / 16, (int)rootPosition.Y / 16)), 0, new Vector2(TailTexture.Width() / 2, TailTexture.Height()), 1f, SpriteEffects.None, 0f);
            spriteBatch.Draw(TailMaskTexture.Value, rootPosition - screenPos, new Rectangle(0, 0, TailMaskTexture.Width(), TailMaskTexture.Height()), NPC.GetNPCColorTintedByBuffs(Color.White), 0, new Vector2(TailMaskTexture.Width() / 2, TailMaskTexture.Height()), 1f, SpriteEffects.None, 0f);

            for (int i = 0; i < LENGTH; i += 2)
            {
                float progress = i / LENGTH;

                Vector2 displacementVector = new Vector2(0, progress);
                if (curvature != 0)
                {
                    displacementVector = new Vector2(((float)Math.Cos(progress * curvature) - 1) / curvature, (float)Math.Sin(progress * curvature) / curvature);
                }
                Vector2 drawPos = rootPosition + displacementVector * LENGTH;

                spriteBatch.Draw(ChainTexture.Value, drawPos - screenPos, new Rectangle(0, i % ChainTexture.Height(), ChainTexture.Width(), 4), NPC.GetNPCColorTintedByBuffs(Lighting.GetColor((int)drawPos.X / 16, (int)drawPos.Y / 16)), progress * curvature, new Vector2(ChainTexture.Width() / 2, 2), 1f, SpriteEffects.None, 0f);
                spriteBatch.Draw(ChainMaskTexture.Value, drawPos - screenPos, new Rectangle(0, i % ChainMaskTexture.Height(), ChainMaskTexture.Width(), 4), NPC.GetNPCColorTintedByBuffs(Color.White), progress * curvature, new Vector2(ChainMaskTexture.Width() / 2, 2), 1f, SpriteEffects.None, 0f);
            }

            return true;
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            if (!spawnInfo.Player.InModBiome(GetInstance<LimestoneCave>()))
            {
                return 0f;
            }

            rootTile = new Point(spawnInfo.SpawnTileX, spawnInfo.SpawnTileY - 1);
            if (Main.tile[rootTile.X, rootTile.Y].WallType != WallType<LimestoneWallNatural>())
            {
                return 0f;
            }

            while (rootTile.Y >= 0 && (!Main.tileSolid[Main.tile[rootTile.X, rootTile.Y].TileType] || !Main.tile[rootTile.X, rootTile.Y].HasUnactuatedTile))
            {
                rootTile.Y--;
            }
            if (rootTile.Y < 0 || (Math.Abs(rootTile.Y - spawnInfo.PlayerFloorY) < NPC.safeRangeY && Math.Abs(rootTile.X - spawnInfo.PlayerFloorX) < NPC.safeRangeX))
            {
                return 0f;
            }
            else if (Main.tile[rootTile.X, rootTile.Y].TileType == TileType<LimestoneTile>() && Main.tile[rootTile.X, rootTile.Y].WallType == WallType<LimestoneWallNatural>())
            {
                return 3.5f;
            }
            return 0f;
        }

        public override bool CheckDead()
        {
            for (int i = 0; i < LENGTH; i += 2)
            {
                float progress = i / LENGTH;

                Vector2 displacementVector = new Vector2(0, progress);
                if (curvature != 0)
                {
                    displacementVector = new Vector2(((float)Math.Cos(progress * curvature) - 1) / curvature, (float)Math.Sin(progress * curvature) / curvature);
                }
                Vector2 position = rootPosition + displacementVector * LENGTH;

                Dust.NewDustPerfect(position + new Vector2(Main.rand.Next(15), 0).RotatedByRandom(MathHelper.TwoPi), 74, Scale: 1.5f).noGravity = true; ;
            }

            for (int i = 0; i < 5; i++)
            {
                Main.dust[Dust.NewDust(NPC.position, NPC.width, NPC.height, 74, Scale: 1.75f)].noGravity = true;
            }
            Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, Mod.Find<ModGore>("GlowWormGore").Type);

            return true;
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            npcLoot.Add(ItemDropRule.Common(ItemType<CorrosivePolish>(), 20));
            npcLoot.Add(ItemDropRule.Common(ItemType<Items.Placeable.Blocks.Limestone>(), 1, 2, 4));
            npcLoot.Add(ItemDropRule.Common(ItemType<KeyLimePie>(), 50));
        }

        public class GlowWormDroplet : ModProjectile
        {
            public override string Texture => "Polarities/Items/Weapons/Melee/GlowYoDroplet";

            public override void SetStaticDefaults()
            {
                DisplayName.SetDefault("{$Mods.Polarities.ProjectileName.GlowYoDroplet}");

                ProjectileID.Sets.TrailCacheLength[Projectile.type] = 16;
                ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
            }

            public override void SetDefaults()
            {
                Projectile.extraUpdates = 0;
                Projectile.width = 16;
                Projectile.height = 16;
                DrawOffsetX = -1;
                DrawOriginOffsetY = -1;
                DrawOriginOffsetX = 0;

                Projectile.aiStyle = -1;
                Projectile.hostile = true;
                Projectile.penetrate = -1;
                Projectile.scale = 1f;

                Projectile.tileCollide = false;

                Projectile.timeLeft = 1200;
            }

            public override void AI()
            {
                Lighting.AddLight(Projectile.Center, 173f / 512, 217f / 512, 160f / 512);

                Projectile.rotation += 0.1f;

                Projectile.velocity.Y += 0.2f;
                Projectile.velocity *= 0.98f;

                if (Projectile.timeLeft < 1140)
                {
                    Projectile.tileCollide = true;
                }
            }

            public override bool PreDraw(ref Color lightColor)
            {
                Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;

                Color mainColor = Color.White;

                Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, new Rectangle(0, Projectile.frame * texture.Height / Main.projFrames[Projectile.type], texture.Width, texture.Height / Main.projFrames[Projectile.type]), mainColor, Projectile.rotation, new Vector2(texture.Width / 2, texture.Height / Main.projFrames[Projectile.type] / 2), Projectile.scale, SpriteEffects.None, 0f);

                for (int k = 0; k < Projectile.oldPos.Length; k++)
                {
                    Color color = mainColor * ((float)(Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length);
                    float scale = Projectile.scale * ((float)(Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length);

                    float rotation = Projectile.rotation;

                    Main.spriteBatch.Draw(texture, Projectile.Center - Projectile.position + Projectile.oldPos[k] - Main.screenPosition, new Rectangle(0, Projectile.frame * texture.Height / Main.projFrames[Projectile.type], texture.Width, texture.Height / Main.projFrames[Projectile.type]), color, rotation, new Vector2(texture.Width / 2, texture.Height / Main.projFrames[Projectile.type] / 2), scale, SpriteEffects.None, 0f);
                }
                return false;
            }

            public override void Kill(int timeLeft)
            {
                for (int i = 0; i < 10; i++)
                {
                    Main.dust[Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, 74, Scale: 1.5f)].noGravity = true;
                }
            }

            public override void OnHitPlayer(Player target, int damage, bool crit)
            {
                target.AddBuff(BuffType<Buffs.Corroding>(), 15 * 60);
            }
        }
    }
}