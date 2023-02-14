using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Polarities.Biomes.Fractal;
using Polarities.Items.Materials;
using Polarities.Items.Placeable.Banners;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Polarities.NPCs.Enemies.Fractal
{
    public class FractalSlimeSmall : ModNPC
    {
        public override void SetStaticDefaults()
        {
            NPCID.Sets.NPCBestiaryDrawOffset[Type] = new NPCID.Sets.NPCBestiaryDrawModifiers(0) { Hide = true, };
        }

        public override void SetDefaults()
        {
            NPC.aiStyle = -1;
            NPC.width = 16;
            NPC.height = 16;

            NPC.defense = 4;
            NPC.damage = 20;
            NPC.lifeMax = 50;
            NPC.knockBackResist = 0f;
            NPC.npcSlots = 1f;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.value = Item.buyPrice(copper: 50);
            NPC.noTileCollide = true;
            NPC.noGravity = true;

            NPC.alpha = 128;

            Banner = NPC.type;
            BannerItem = ItemType<FractalSlimeBanner>();
        }

        public override void AI()
        {
            if (NPC.localAI[0] == 0)
            {
                NPC.localAI[0] = 1;
                NPC.velocity = new Vector2(4f, 0).RotatedByRandom(MathHelper.TwoPi);
            }

            bool xCollision = Collision.TileCollision(NPC.position, new Vector2(NPC.velocity.X, 0), NPC.width, NPC.height, true, true) != new Vector2(NPC.velocity.X, 0);
            bool yCollision = Collision.TileCollision(NPC.position, new Vector2(0, NPC.velocity.Y), NPC.width, NPC.height, true, true) != new Vector2(0, NPC.velocity.Y);

            if (xCollision)
            {
                NPC.velocity.X *= -1;
            }
            if (yCollision)
            {
                NPC.velocity.Y *= -1;
            }
            if (xCollision && yCollision)
            {
                if (NPC.ai[1] > 600)
                {
                    SoundEngine.PlaySound(SoundID.Item35, NPC.Center);
                }
                NPC.ai[1] = 0;
            }

            NPC.ai[0]++;
            NPC.ai[1]++;
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            if (FractalSubworld.Active)
            {
                return 0.15f;
            }
            return 0f;
        }

        public override void HitEffect(int hitDirection, double damage)
        {
            if (NPC.life <= 0)
            {
                for (int i = 0; i < 6; i++)
                {
                    Main.dust[Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.t_Slime, 0, 0, 64, newColor: new Color(192, 192, 384), Scale: 1.5f)].noGravity = true;
                }
            }
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<FractalResidue>(), chanceDenominator: 16));
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D smallSlimeTexture = TextureAssets.Npc[NPC.type].Value;

            Vector2 scale = new Vector2(1, 1) + new Vector2(1, -1) * (float)Math.Sin(NPC.ai[0] / 5) * 0.1f;

            spriteBatch.Draw(smallSlimeTexture, NPC.Center - screenPos, new Rectangle(0, 0, smallSlimeTexture.Width, smallSlimeTexture.Height), NPC.GetNPCColorTintedByBuffs(drawColor) * ((255f - NPC.alpha) / 255f), 0, new Vector2(smallSlimeTexture.Width / 2, smallSlimeTexture.Height / 2), scale * NPC.scale, SpriteEffects.None, 0f);

            return false;
        }
    }

    public class FractalSlimeMedium : ModNPC
    {
        public override void SetStaticDefaults()
        {
            NPCID.Sets.NPCBestiaryDrawOffset[Type] = new NPCID.Sets.NPCBestiaryDrawModifiers(0) { Hide = true, };
        }

        public override void SetDefaults()
        {
            NPC.aiStyle = -1;
            NPC.width = 32;
            NPC.height = 32;
            DrawOffsetY = -5;

            NPC.defense = 8;
            NPC.damage = 30;
            NPC.lifeMax = 100;
            NPC.knockBackResist = 0f;
            NPC.npcSlots = 1f;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.value = Item.buyPrice(copper: 50);
            NPC.noTileCollide = true;
            NPC.noGravity = true;

            NPC.alpha = 128;

            Banner = NPCType<FractalSlimeSmall>();
            BannerItem = ItemType<FractalSlimeBanner>();
        }

        public override void AI()
        {
            if (NPC.localAI[0] == 0)
            {
                NPC.localAI[0] = 1;
                NPC.velocity = new Vector2(5f, 0).RotatedByRandom(MathHelper.TwoPi);
            }

            bool xCollision = Collision.TileCollision(NPC.position, new Vector2(NPC.velocity.X, 0), NPC.width, NPC.height, true, true) != new Vector2(NPC.velocity.X, 0);
            bool yCollision = Collision.TileCollision(NPC.position, new Vector2(0, NPC.velocity.Y), NPC.width, NPC.height, true, true) != new Vector2(0, NPC.velocity.Y);

            if (xCollision)
            {
                NPC.velocity.X *= -1;
            }
            if (yCollision)
            {
                NPC.velocity.Y *= -1;
            }
            if (xCollision && yCollision)
            {
                if (NPC.ai[1] > 600)
                {
                    SoundEngine.PlaySound(SoundID.Item35, NPC.Center);
                }
                NPC.ai[1] = 0;
            }

            NPC.rotation += 0.05f;

            NPC.ai[0]++;
            NPC.ai[1]++;
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            //if (FractalSubworld.Active)
            //{
            //    return 0.15f;
            //}
            return 0f;
        }

        public override bool CheckDead()
        {
            for (int i = 0; i < 4; i++)
            {
                Vector2 scale = new Vector2(1, 1) + new Vector2(1, -1) * (float)Math.Sin(NPC.ai[0] / 5) * 0.1f;

                Vector2 offset = new Vector2(8, 0).RotatedBy(NPC.rotation + i * MathHelper.PiOver2);
                offset.X *= scale.X;
                offset.Y *= scale.Y;

                Vector2 position = NPC.Center + offset;
                NPC.NewNPC(NPC.GetSource_Death(), (int)position.X, (int)position.Y, NPCType<FractalSlimeSmall>());
            }
            for (int i = 0; i < 12; i++)
            {
                Main.dust[Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.t_Slime, 0, 0, 64, newColor: new Color(192, 192, 384), Scale: 1.5f)].noGravity = true;
            }
            return true;
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<FractalResidue>(), chanceDenominator: 4));
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D smallSlimeTexture = TextureAssets.Npc[NPCType<FractalSlimeSmall>()].Value;
            Texture2D mediumSlimeTexture = TextureAssets.Npc[NPC.type].Value;

            Vector2 scale = new Vector2(1, 1) + new Vector2(1, -1) * (float)Math.Sin(NPC.ai[0] / 5) * 0.1f;
            drawColor = NPC.GetNPCColorTintedByBuffs(drawColor);
            for (int i = 0; i < 4; i++)
            {
                Vector2 offset = new Vector2(8, 0).RotatedBy(NPC.rotation + i * MathHelper.PiOver2);
                offset.X *= scale.X;
                offset.Y *= scale.Y;

                Vector2 drawPos = NPC.Center + offset - screenPos;

                spriteBatch.Draw(smallSlimeTexture, drawPos, new Rectangle(0, 0, smallSlimeTexture.Width, smallSlimeTexture.Height), drawColor * ((255f - NPC.alpha) / 255f), 0, new Vector2(smallSlimeTexture.Width / 2, smallSlimeTexture.Height / 2), scale * NPC.scale, SpriteEffects.None, 0f);
            }

            spriteBatch.Draw(mediumSlimeTexture, NPC.Center - screenPos, new Rectangle(0, 0, mediumSlimeTexture.Width, mediumSlimeTexture.Height), drawColor * ((255f - NPC.alpha) / 255f), 0, new Vector2(mediumSlimeTexture.Width / 2, mediumSlimeTexture.Height / 2), scale * NPC.scale, SpriteEffects.None, 0f);

            return false;
        }
    }

    public class FractalSlimeLarge : ModNPC
    {
        public override void SetStaticDefaults()
        {
        }

        public override void SetDefaults()
        {
            NPC.aiStyle = -1;
            NPC.width = 64;
            NPC.height = 64;

            NPC.defense = 16;
            NPC.damage = 40;
            NPC.lifeMax = 200;
            NPC.knockBackResist = 0f;
            NPC.npcSlots = 1f;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.value = Item.buyPrice(copper: 50);
            NPC.noTileCollide = true;
            NPC.noGravity = true;

            NPC.alpha = 128;

            Banner = NPCType<FractalSlimeSmall>();
            BannerItem = ItemType<FractalSlimeBanner>();

            this.SetModBiome<FractalBiome>();
        }

        public override void AI()
        {
            if (NPC.localAI[0] == 0)
            {
                NPC.localAI[0] = 1;
                NPC.velocity = new Vector2(6f, 0).RotatedByRandom(MathHelper.TwoPi);
            }

            bool xCollision = Collision.TileCollision(NPC.position, new Vector2(NPC.velocity.X, 0), NPC.width, NPC.height, true, true) != new Vector2(NPC.velocity.X, 0);
            bool yCollision = Collision.TileCollision(NPC.position, new Vector2(0, NPC.velocity.Y), NPC.width, NPC.height, true, true) != new Vector2(0, NPC.velocity.Y);

            if (xCollision)
            {
                NPC.velocity.X *= -1;
            }
            if (yCollision)
            {
                NPC.velocity.Y *= -1;
            }
            if (xCollision && yCollision)
            {
                if (NPC.ai[1] > 600)
                {
                    SoundEngine.PlaySound(SoundID.Item35, NPC.Center);
                }
                NPC.ai[1] = 0;
            }

            NPC.rotation += 0.05f;

            NPC.ai[0]++;
            NPC.ai[1]++;
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            //if (FractalSubworld.Active)
            //{
            //    return 0.15f;
            //}
            return 0f;
        }

        public override bool CheckDead()
        {
            for (int i = 0; i < 4; i++)
            {
                Vector2 scale = new Vector2(1, 1) + new Vector2(1, -1) * (float)Math.Sin(NPC.ai[0] / 5) * 0.1f;

                Vector2 offset = new Vector2(16, 0).RotatedBy(NPC.rotation + i * MathHelper.PiOver2);
                offset.X *= scale.X;
                offset.Y *= scale.Y;

                Vector2 position = NPC.Center + offset;
                NPC.NewNPC(NPC.GetSource_Death(), (int)position.X, (int)position.Y, NPCType<FractalSlimeMedium>());
            }
            for (int i = 0; i < 24; i++)
            {
                Main.dust[Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.t_Slime, 0, 0, 64, newColor: new Color(192, 192, 384), Scale: 1.5f)].noGravity = true;
            }
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
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<FractalResidue>()));
        }

        public override void FindFrame(int frameHeight)
        {
            if (NPC.IsABestiaryIconDummy)
            {
                NPC.rotation += 0.05f;
                NPC.ai[0]++;
                NPC.ai[1]++;
            }
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D largeSlimeTexture = TextureAssets.Npc[NPC.type].Value;
            Texture2D mediumSlimeTexture = TextureAssets.Npc[NPCType<FractalSlimeMedium>()].Value;
            Texture2D smallSlimeTexture = TextureAssets.Npc[NPCType<FractalSlimeSmall>()].Value;

            Vector2 scale = new Vector2(1, 1) + new Vector2(1, -1) * (float)Math.Sin(NPC.ai[0] / 5) * 0.1f;
            drawColor = NPC.GetNPCColorTintedByBuffs(drawColor);
            for (int i = 0; i < 4; i++)
            {
                Vector2 offset = new Vector2(16, 0).RotatedBy(NPC.rotation + i * MathHelper.PiOver2);
                offset.X *= scale.X;
                offset.Y *= scale.Y;

                Vector2 drawPos = NPC.Center + offset - screenPos;

                for (int j = 0; j < 4; j++)
                {
                    offset = new Vector2(8, 0).RotatedBy(NPC.rotation * 2 + j * MathHelper.PiOver2);
                    offset.X *= scale.X;
                    offset.Y *= scale.Y;

                    spriteBatch.Draw(smallSlimeTexture, drawPos + offset, new Rectangle(0, 0, smallSlimeTexture.Width, smallSlimeTexture.Height), drawColor * ((255f - NPC.alpha) / 255f), 0, new Vector2(smallSlimeTexture.Width / 2, smallSlimeTexture.Height / 2), scale * NPC.scale, SpriteEffects.None, 0f);
                }

                spriteBatch.Draw(mediumSlimeTexture, drawPos, new Rectangle(0, 0, mediumSlimeTexture.Width, mediumSlimeTexture.Height), drawColor * ((255f - NPC.alpha) / 255f), 0, new Vector2(mediumSlimeTexture.Width / 2, mediumSlimeTexture.Height / 2), scale * NPC.scale, SpriteEffects.None, 0f);
            }

            spriteBatch.Draw(largeSlimeTexture, NPC.Center - screenPos, new Rectangle(0, 0, largeSlimeTexture.Width, largeSlimeTexture.Height), drawColor * ((255f - NPC.alpha) / 255f), 0, new Vector2(largeSlimeTexture.Width / 2, largeSlimeTexture.Height / 2), scale * NPC.scale, SpriteEffects.None, 0f);

            return false;
        }
    }
}