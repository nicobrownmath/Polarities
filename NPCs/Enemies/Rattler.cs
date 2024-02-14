using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using Terraria.Audio;
using Terraria.GameContent.ItemDropRules;
using Terraria.GameContent.Bestiary;
using Terraria.Localization;
using Terraria.GameContent;

namespace Polarities.NPCs.Enemies
{
    public class Rattler : ModNPC
    {
        private int attackCooldown
        {
            get => attackCooldown = (int)NPC.ai[0];
            set => NPC.ai[0] = (float)value;
        }

        private int rattleCooldown
        {
            get => (int)NPC.ai[1];
            set => NPC.ai[1] = value;
        }

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 12;

            NPCID.Sets.NPCBestiaryDrawModifiers drawModifiers = new NPCID.Sets.NPCBestiaryDrawModifiers(0)
            {
                SpriteDirection = 1
            };
            NPCID.Sets.NPCBestiaryDrawOffset.Add(NPC.type, drawModifiers);

            PolaritiesNPC.forceCountForRadar.Add(Type);
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
                //spawn conditions
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Desert,
				//flavor text
				this.TranslatedBestiaryEntry()
            });
        }

        public override void SetDefaults()
        {
            NPC.aiStyle = -1;
            NPC.width = 26;
            NPC.height = 18;
            NPC.defense = 5;
            NPC.damage = Main.expertMode ? 80 / 2 : 60;
            NPC.lifeMax = 40;
            NPC.knockBackResist = 0f;
            NPC.npcSlots = 1f;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.value = Item.buyPrice(0, 0, 1, 0);

            Banner = NPC.type;
            BannerItem = ItemType<Items.Placeable.Banners.RattlerBanner>();
        }

        public override void AI()
        {
            Player player = Main.player[NPC.target];
            if (!player.active || player.dead)
            {
                NPC.TargetClosest(false);
                player = Main.player[NPC.target];
            }

            if (NPC.life < NPC.lifeMax / 2)
            {
                NPC.catchItem = (short)ItemType<Items.Armor.Vanity.SnekHat>();
            }

            if (rattleCooldown > 0)
            {
                rattleCooldown--;
            }

            if (attackCooldown > 0)
            {
                attackCooldown--;
                NPC.damage = Main.expertMode ? 80 : 60;
                if (NPC.frame.Y < 2 * NPC.frame.Height)
                {
                    NPC.frame.Y = 1 * NPC.frame.Height;
                }
            }
            else
            {
                NPC.damage = 0;

                if ((NPC.Center - player.Center).Length() > player.velocity.Length() * 64 * Math.Cos(player.velocity.ToRotation() - (NPC.Center - player.Center).ToRotation()))
                {
                    NPC.frame.Y = 1 * NPC.frame.Height;
                    NPC.noGravity = false;
                    NPC.noTileCollide = false;
                }
                else if ((NPC.Center - player.Center).Length() > 50)
                {
                    NPC.noGravity = false;
                    NPC.noTileCollide = false;
                    if (rattleCooldown == 0)
                    {
                        SoundEngine.PlaySound(Sounds.Rattle, NPC.Center);
                        rattleCooldown = 40;
                    }
                }
                else
                {
                    NPC.noGravity = true;
                    NPC.noTileCollide = true;
                    NPC.frame.Y = 2 * NPC.frame.Height;
                    NPC.width = 60 * 2 - 26;
                    NPC.position.X -= (60 - 26);
                    attackCooldown = 60;
                }
            }

            if (attackCooldown < 60 - 7)
            {
                NPC.width = 26;
                NPC.direction = player.Center.X > NPC.Center.X ? -1 : 1;
                NPC.spriteDirection = NPC.direction;
            }

            if (attackCooldown == 60 - 8)
            {
                NPC.position.X += (60 - 26);
            }
        }

        public override void FindFrame(int frameHeight)
        {
            NPC.frame.Width = 68;
            if (Main.xMas)
            {
                NPC.frame.X = NPC.frame.Width;
            }
            else if (Main.halloween)
            {
                NPC.frame.X = NPC.frame.Width * 2;
            }
            else
            {
                NPC.frame.X = 0;
            }

            NPC.frameCounter++;
            if (NPC.frameCounter == 1)
            {
                NPC.frameCounter = 0;
                NPC.frame.Y += NPC.frame.Height;
                if (NPC.frame.Y == 2 * NPC.frame.Height || NPC.frame.Y == 12 * NPC.frame.Height)
                {
                    NPC.frame.Y = 0;
                }
            }
        }

        public override bool CheckDead()
        {
            for (int i = 1; i <= 4; i++)
                Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, Mod.Find<ModGore>("SnekGore" + i).Type);
            return true;
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo hurtInfo)
        {
            target.AddBuff(BuffID.Poisoned, 300);
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            return spawnInfo.SpawnTileY <= Main.worldSurface && spawnInfo.SpawnTileType == TileID.Sand && !spawnInfo.Water ? (Main.halloween || Main.xMas ? 0.25f : 0.5f) : 0f;
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            npcLoot.Add(ItemDropRule.Common(ItemType<Items.Materials.Rattle>(), 20));
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            //adapted from vanilla npc drawcode
            drawColor = NPC.GetNPCColorTintedByBuffs(drawColor);
            Vector2 halfSize = NPC.frame.Size() / 2;
            SpriteEffects spriteEffects = (SpriteEffects)0;
            if (NPC.spriteDirection == 1)
            {
                spriteEffects = (SpriteEffects)1;
            }

            Texture2D npcTexture = TextureAssets.Npc[Type].Value;

            int numHorizontalFrames = 3;

            Main.spriteBatch.Draw(npcTexture, new Vector2(NPC.Center.X - screenPos.X - npcTexture.Width * NPC.scale / 2f / numHorizontalFrames + halfSize.X * NPC.scale, NPC.position.Y - screenPos.Y + NPC.height - npcTexture.Height * NPC.scale / Main.npcFrameCount[Type] + 4f + halfSize.Y * NPC.scale + Main.NPCAddHeight(NPC) + NPC.gfxOffY), NPC.frame, NPC.GetAlpha(drawColor), NPC.rotation, halfSize, NPC.scale, spriteEffects, 0f);
            if (NPC.confused)
            {
                Main.spriteBatch.Draw(TextureAssets.Confuse.Value, new Vector2(NPC.position.X - screenPos.X + (float)(NPC.width / 2) - (float)TextureAssets.Npc[Type].Width() * NPC.scale / 2f / numHorizontalFrames + halfSize.X * NPC.scale, NPC.position.Y - screenPos.Y + (float)NPC.height - (float)TextureAssets.Npc[Type].Height() * NPC.scale / (float)Main.npcFrameCount[Type] + 4f + halfSize.Y * NPC.scale + Main.NPCAddHeight(NPC) - (float)TextureAssets.Confuse.Height() - 20f), (Rectangle?)new Rectangle(0, 0, TextureAssets.Confuse.Width(), TextureAssets.Confuse.Height()), new Color(250, 250, 250, 70), NPC.velocity.X * -0.05f, new Vector2((float)(TextureAssets.Confuse.Width() / 2), (float)(TextureAssets.Confuse.Height() / 2)), Main.essScale + 0.2f, (SpriteEffects)0, 0f);
            }

            return false;
        }
    }
}