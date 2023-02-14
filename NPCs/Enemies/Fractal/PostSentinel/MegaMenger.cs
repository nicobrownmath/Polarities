using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Polarities.Biomes.Fractal;
using Polarities.Items;
using Polarities.Items.Pets;
using Polarities.Items.Placeable.Banners;
using Polarities.Items.Placeable.Blocks.Fractal;
using System;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Polarities.NPCs.Enemies.Fractal.PostSentinel
{
    public class MegaMenger : ModNPC
    {
        private float[] verticalHookPositions = new float[2];
        private float[] horizontalHookPositions = new float[2];

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 5;
        }

        public override void SetDefaults()
        {
            NPC.aiStyle = -1;
            NPC.width = 58;
            NPC.height = 58;
            DrawOffsetY = -4;

            NPC.defense = 30;
            NPC.damage = 96;
            NPC.lifeMax = 7200;
            NPC.knockBackResist = 0f;
            NPC.npcSlots = 1f;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.HitSound = SoundID.NPCHit4;
            NPC.DeathSound = SoundID.NPCDeath14;
            NPC.value = 5000;
            NPC.rarity = 1;

            NPC.buffImmune[BuffID.Confused] = true;
            NPC.buffImmune[BuffID.OnFire] = true;
            NPC.buffImmune[BuffID.Frostburn] = true;
            NPC.buffImmune[BuffID.CursedInferno] = true;
            NPC.buffImmune[BuffID.ShadowFlame] = true;
            NPC.buffImmune[BuffID.Poisoned] = true;
            NPC.buffImmune[BuffID.Venom] = true;

            Banner = NPC.type;
            BannerItem = ItemType<MegaMengerBanner>();

            this.SetModBiome<FractalBiome, FractalUGBiome>();
        }

        public override bool PreAI()
        {
            if (NPC.localAI[0] == 0)
            {
                NPC.localAI[0] = 1;
                horizontalHookPositions[0] = NPC.Center.X;
                horizontalHookPositions[1] = NPC.Center.X;
                verticalHookPositions[0] = NPC.Center.Y;
                verticalHookPositions[1] = NPC.Center.Y;
            }

            NPC.TargetClosest(false);
            Player player = Main.player[NPC.target];

            switch (NPC.ai[0])
            {
                case 0:
                    //extending horizontal hooks
                    bool attached = true;
                    for (int i = 0; i < 2; i++)
                    {
                        if ((int)(horizontalHookPositions[i] / 16) > 0 && (int)(horizontalHookPositions[i] / 16) < Main.maxTilesX &&
                            (int)(NPC.Center.Y / 16) > 0 && (int)(NPC.Center.Y / 16) < Main.maxTilesY)
                        {
                            Tile tile = Framing.GetTileSafely((int)(horizontalHookPositions[i] / 16), (int)(NPC.Center.Y / 16));
                            if (!(Main.tileSolid[tile.TileType] && !Main.tileSolidTop[tile.TileType]) || !tile.HasTile)
                            {
                                attached = false;
                                horizontalHookPositions[i] += i == 0 ? 16 : -16;
                            }
                        }
                    }
                    if (attached || Math.Abs(horizontalHookPositions[0] - NPC.Center.X) > 3600 || Math.Abs(horizontalHookPositions[1] - NPC.Center.X) > 3600)
                    {
                        NPC.ai[0] = 1;

                        ShootLasers();
                    }
                    else
                    {
                        attached = true;
                        for (int i = 0; i < 2; i++)
                        {
                            if ((int)(horizontalHookPositions[i] / 16) > 0 && (int)(horizontalHookPositions[i] / 16) < Main.maxTilesX &&
                                (int)(NPC.Center.Y / 16) > 0 && (int)(NPC.Center.Y / 16) < Main.maxTilesY)
                            {
                                Tile tile = Framing.GetTileSafely((int)(horizontalHookPositions[i] / 16), (int)(NPC.Center.Y / 16));
                                if (!(Main.tileSolid[tile.TileType] && !Main.tileSolidTop[tile.TileType]) || !tile.HasTile)
                                {
                                    attached = false;
                                    horizontalHookPositions[i] += i == 0 ? 16 : -16;
                                }
                            }
                        }
                        if (attached || Math.Abs(horizontalHookPositions[0] - NPC.Center.X) > 3600 || Math.Abs(horizontalHookPositions[1] - NPC.Center.X) > 3600)
                        {
                            NPC.ai[0] = 1;

                            ShootLasers();
                        }
                    }
                    break;
                case 1:
                    //retracting vertical hooks
                    bool retracted = true;
                    for (int i = 0; i < 2; i++)
                    {
                        if (Math.Abs(verticalHookPositions[i] - NPC.Center.Y) > 32)
                        {
                            retracted = false;
                            verticalHookPositions[i] -= verticalHookPositions[i] > NPC.Center.Y ? 32 : -32;
                        }
                        else
                        {
                            verticalHookPositions[i] = NPC.Center.Y;
                        }
                    }
                    if (retracted)
                    {
                        NPC.ai[0] = 2;

                        ShootSparks();
                    }
                    break;
                case 2:
                    //moving horizontally
                    if (horizontalHookPositions[0] < NPC.Center.X + 32 || horizontalHookPositions[1] > NPC.Center.X - 32)
                    {
                        if (horizontalHookPositions[0] < NPC.Center.X + 32 && horizontalHookPositions[1] > NPC.Center.X - 32)
                        {
                            NPC.ai[0] = 0;
                            NPC.velocity.X = 0;
                        }
                        else if (horizontalHookPositions[0] < NPC.Center.X + 32)
                        {
                            NPC.velocity.X = -6;
                        }
                        else
                        {
                            NPC.velocity.X = 6;
                        }
                        if (Main.rand.NextBool())
                        {
                            NPC.ai[0] = 3;
                            NPC.velocity.X = 0;
                        }
                        NPC.netUpdate = true;
                    }
                    if (Math.Abs(player.Center.X - NPC.Center.X) < 6 && Main.rand.NextBool() || Main.rand.NextBool(600))
                    {
                        NPC.ai[0] = 3;
                        NPC.velocity.X = 0;
                    }
                    if (NPC.ai[0] == 2 && NPC.velocity.X == 0)
                    {
                        NPC.velocity.X = player.Center.X > NPC.Center.X ? 6 : -6;
                    }
                    NPC.netUpdate = true;
                    break;
                case 3:
                    //extending vertical hooks
                    attached = true;
                    for (int i = 0; i < 2; i++)
                    {
                        if ((int)(NPC.Center.X / 16) > 0 && (int)(NPC.Center.X / 16) < Main.maxTilesX &&
                            (int)(verticalHookPositions[i] / 16) > 0 && (int)(verticalHookPositions[i] / 16) < Main.maxTilesY)
                        {
                            Tile tile = Framing.GetTileSafely((int)(NPC.Center.X / 16), (int)(verticalHookPositions[i] / 16));
                            if (!(Main.tileSolid[tile.TileType] && !Main.tileSolidTop[tile.TileType]) || !tile.HasTile)
                            {
                                attached = false;
                                verticalHookPositions[i] += i == 0 ? 16 : -16;
                            }
                        }
                    }
                    if (attached || Math.Abs(verticalHookPositions[0] - NPC.Center.Y) > 3600 || Math.Abs(verticalHookPositions[1] - NPC.Center.Y) > 3600)
                    {
                        NPC.ai[0] = 4;

                        ShootLasers();
                    }
                    else
                    {
                        attached = true;
                        for (int i = 0; i < 2; i++)
                        {
                            if ((int)(NPC.Center.X / 16) > 0 && (int)(NPC.Center.X / 16) < Main.maxTilesX &&
                                (int)(verticalHookPositions[i] / 16) > 0 && (int)(verticalHookPositions[i] / 16) < Main.maxTilesY)
                            {
                                Tile tile = Framing.GetTileSafely((int)(NPC.Center.X / 16), (int)(verticalHookPositions[i] / 16));
                                if (!(Main.tileSolid[tile.TileType] && !Main.tileSolidTop[tile.TileType]) || !tile.HasTile)
                                {
                                    attached = false;
                                    verticalHookPositions[i] += i == 0 ? 16 : -16;
                                }
                            }
                        }
                        if (attached || Math.Abs(verticalHookPositions[0] - NPC.Center.Y) > 3600 || Math.Abs(verticalHookPositions[1] - NPC.Center.Y) > 3600)
                        {
                            NPC.ai[0] = 4;

                            ShootLasers();
                        }
                    }
                    break;
                case 4:
                    //retracting horizontal hooks
                    retracted = true;
                    for (int i = 0; i < 2; i++)
                    {
                        if (Math.Abs(horizontalHookPositions[i] - NPC.Center.X) > 32)
                        {
                            retracted = false;
                            horizontalHookPositions[i] -= horizontalHookPositions[i] > NPC.Center.X ? 32 : -32;
                        }
                        else
                        {
                            horizontalHookPositions[i] = NPC.Center.X;
                        }
                    }
                    if (retracted)
                    {
                        NPC.ai[0] = 5;

                        ShootSparks();
                    }
                    break;
                case 5:
                    //moving vertically
                    if (verticalHookPositions[0] < NPC.Center.Y + 32 || verticalHookPositions[1] > NPC.Center.Y - 32)
                    {
                        if (verticalHookPositions[0] < NPC.Center.Y + 32 && verticalHookPositions[1] > NPC.Center.Y - 32)
                        {
                            NPC.ai[0] = 0;
                            NPC.velocity.Y = 0;
                        }
                        else if (verticalHookPositions[0] < NPC.Center.Y + 32)
                        {
                            NPC.velocity.Y = -6;
                        }
                        else
                        {
                            NPC.velocity.Y = 6;
                        }
                        if (Main.rand.NextBool())
                        {
                            NPC.ai[0] = 0;
                            NPC.velocity.Y = 0;
                        }
                        NPC.netUpdate = true;
                    }
                    if (Math.Abs(player.Center.Y - NPC.Center.Y) < 6 && Main.rand.NextBool() || Main.rand.NextBool(600))
                    {
                        NPC.ai[0] = 0;
                        NPC.velocity.Y = 0;
                    }
                    if (NPC.ai[0] == 5 && NPC.velocity.Y == 0)
                    {
                        NPC.velocity.Y = player.Center.Y > NPC.Center.Y ? 6 : -6;
                    }
                    NPC.netUpdate = true;
                    break;
            }

            if (NPC.ai[1] > 0)
            {
                NPC.ai[1]--;
            }

            return false;
        }

        private void ShootSparks()
        {
            Player player = Main.player[NPC.target];
            if (NPC.ai[1] == 0)
            {
                NPC.ai[1] = 120;
                for (int i = 0; i < 7; i++)
                {
                    Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, new Vector2(-8, 0).RotatedBy(i * MathHelper.TwoPi / 12), ProjectileType<FractalSpiritSpark>(), 14, 0, Main.myPlayer, player.whoAmI);
                }
                SoundEngine.PlaySound(SoundID.Item93, NPC.Center);
            }
        }

        private void ShootLasers()
        {
            if (Main.expertMode)
            {
                SoundEngine.PlaySound(SoundID.Item33, NPC.Center);

                for (int i = 0; i < 4; i++)
                {
                    Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, new Vector2(32, 0).RotatedBy(i * MathHelper.PiOver2), ProjectileType<MegaMengerLaser>(), 30, 4, Main.myPlayer);
                }
            }
        }

        public override void FindFrame(int frameHeight)
        {
            NPC.frame.Y = frameHeight * (NPC.velocity.X == 0 ? NPC.velocity.Y == 0 ? 0 : NPC.velocity.Y > 0 ? 4 : 2 : NPC.velocity.X > 0 ? 3 : 1);
        }

        public override bool CheckDead()
        {
            //Gore.NewGore(NPC.position, NPC.velocity, Mod.GetGoreSlot("Gores/MegaMengerGore"));
            //Gore.NewGore(NPC.position, NPC.velocity, Mod.GetGoreSlot("Gores/MegaMengerGore"));
            return true;
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            if (FractalSubworld.Active && spawnInfo.Player.GetFractalization() > FractalSubworld.POST_SENTINEL_TIME)
            {
                return 1.5f * FractalSubworld.SpawnConditionFractalUnderground(spawnInfo);
            }
            return 0f;
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
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<FractalOre>(), minimumDropped: 1, maximumDropped: 3));
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<FractalKey>(), chanceDenominator: 3));
            //npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<InfinitySponge>(), chanceDenominator: 20));
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<MiniMengerItem>(), chanceDenominator: 20));
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D legTexture = ModContent.Request<Texture2D>($"{Texture}Chain").Value;
            Texture2D texture = ModContent.Request<Texture2D>($"{Texture}Hook").Value;
            for (int i = 0; i < 2; i++)
            {

                Vector2 constructCenter = NPC.Center;
                Vector2 center = new Vector2(NPC.Center.X, verticalHookPositions[i]);
                Vector2 distToNPC = constructCenter - center;
                float projRotation = distToNPC.ToRotation() - MathHelper.PiOver2;
                float distance = distToNPC.Length();
                while (distance > 16f && !float.IsNaN(distance))
                {
                    distToNPC.Normalize();                 //get unit vector
                    distToNPC *= 16f;                      //speed = 24
                    center += distToNPC;                   //update draw position
                    distToNPC = constructCenter - center;    //update distance
                    distance = distToNPC.Length();

                    spriteBatch.Draw(legTexture, new Vector2(center.X - Main.screenPosition.X, center.Y - Main.screenPosition.Y),
                        new Rectangle(0, 0, 6, 16), Color.White, projRotation,
                        new Vector2(6 * 0.5f, 16 * 0.5f), 1f, SpriteEffects.None, 0f);
                }


                Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, texture.Height * 0.5f);
                center = new Vector2(NPC.Center.X, verticalHookPositions[i]);
                Vector2 drawPos = center - Main.screenPosition;
                spriteBatch.Draw(texture, drawPos, new Rectangle(0, 0, texture.Width, texture.Height), Lighting.GetColor((int)(center.X / 16), (int)(center.Y / 16)), projRotation, drawOrigin, NPC.scale, SpriteEffects.None, 0f);
            }
            for (int i = 0; i < 2; i++)
            {
                Vector2 constructCenter = NPC.Center;
                Vector2 center = new Vector2(horizontalHookPositions[i], NPC.Center.Y);
                Vector2 distToNPC = constructCenter - center;
                float projRotation = distToNPC.ToRotation() - MathHelper.PiOver2;
                float distance = distToNPC.Length();
                while (distance > 16f && !float.IsNaN(distance))
                {
                    //Draw chain

                    distToNPC.Normalize();                 //get unit vector
                    distToNPC *= 16f;                      //speed = 24
                    center += distToNPC;                   //update draw position
                    distToNPC = constructCenter - center;    //update distance
                    distance = distToNPC.Length();

                    spriteBatch.Draw(legTexture, new Vector2(center.X - Main.screenPosition.X, center.Y - Main.screenPosition.Y),
                        new Rectangle(0, 0, 6, 16), Color.White, projRotation,
                        new Vector2(6 * 0.5f, 16 * 0.5f), 1f, SpriteEffects.None, 0f);
                }

                Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, texture.Height * 0.5f);
                center = new Vector2(horizontalHookPositions[i], NPC.Center.Y);
                Vector2 drawPos = center - Main.screenPosition;
                spriteBatch.Draw(texture, drawPos, new Rectangle(0, 0, texture.Width, texture.Height), Lighting.GetColor((int)(center.X / 16), (int)(center.Y / 16)), projRotation, drawOrigin, NPC.scale, SpriteEffects.None, 0f);
            }
            return true;
        }

        public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D texture = ModContent.Request<Texture2D>($"{Texture}_Mask").Value;
            Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, NPC.height * 0.5f);
            Vector2 drawPos = NPC.Center - Main.screenPosition;
            spriteBatch.Draw(texture, drawPos, NPC.frame, Color.White, NPC.rotation, drawOrigin, NPC.scale, SpriteEffects.None, 0f);
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            for (int i = 0; i < 2; i++)
            {
                writer.Write(verticalHookPositions[i]);
                writer.Write(horizontalHookPositions[i]);
            }
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            for (int i = 0; i < 4; i++)
            {
                verticalHookPositions[i] = reader.ReadSingle();
                horizontalHookPositions[i] = reader.ReadSingle();
            }
        }
    }
    public class MegaMengerLaser : ModProjectile
    {
        public override void SetStaticDefaults()
        {
        }
        public override void SetDefaults()
        {
            Projectile.width = 8;
            Projectile.height = 8;
            Projectile.aiStyle = -1;
            Projectile.hostile = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 600;
            Projectile.tileCollide = true;
            Projectile.alpha = 32;
            Projectile.light = 1f;
        }

        public override void AI()
        {
            Main.dust[Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Electric, newColor: Color.LightBlue, Scale: 1f)].noGravity = true;
            Projectile.rotation = Projectile.velocity.ToRotation() + (float)Math.PI / 2;
        }
    }
}