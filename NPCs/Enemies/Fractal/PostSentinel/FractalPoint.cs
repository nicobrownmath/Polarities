using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
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

namespace Polarities.NPCs.Enemies.Fractal.PostSentinel
{
    public class FractalPoint : ModNPC
    {
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 1;
        }

        public override void SetDefaults()
        {
            NPC.aiStyle = -1;
            NPC.width = 30;
            NPC.height = 30;
            NPC.damage = 190;
            NPC.lifeMax = 200;
            NPC.knockBackResist = 1f;
            NPC.npcSlots = 1f;
            NPC.value = 1;
            NPC.HitSound = SoundID.NPCHit54;
            NPC.DeathSound = SoundID.NPCDeath52;
            NPC.lavaImmune = true;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.dontTakeDamage = true;

            //Banner = NPC.type;
            //BannerItem = ItemType<FractalPointBanner>();
        }

        public override void ScaleExpertStats(int numPlayers, float bossLifeScale)
        {
            NPC.damage = 250;
            NPC.lifeMax = 400;
        }

        public override void OnHitPlayer(Player target, int damage, bool crit)
        {
            //target.AddBuff(BuffType<FractalSubworldDebuff>(), 60 * 60);
            //target.GetModPlayer<PolaritiesPlayer>().suddenFractalizationChange = true;

            //explode
            Explode();
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            //explode
            Explode();
        }

        private void Explode()
        {
            SoundEngine.PlaySound(SoundID.NPCDeath14, NPC.Center);
            for (int i = 0; i < 24; i++)
            {
                Dust.NewDustPerfect(NPC.Center, DustID.Electric, new Vector2(Main.rand.Next(1, 5)).RotatedByRandom(MathHelper.TwoPi), Scale: 1f).noGravity = true;
            }
            NPC.active = false;
        }

        public override void AI()
        {
            if (NPC.ai[0] < 30)
            {
                NPC.ai[0]++;
                if (NPC.ai[0] == 30)
                {
                    NPC.dontTakeDamage = false;
                }
            }

            Lighting.AddLight(NPC.Center, 0.75f, 0.75f, 1f);

            NPC.velocity *= 0.98f;

            NPC.velocity += new Vector2(0.05f, 0).RotatedByRandom(MathHelper.TwoPi);
        }

        public override bool CheckDead()
        {
            if (Main.rand.NextBool())
            {
                for (int i = 0; i < 2; i++)
                    Main.npc[NPC.NewNPC(NPC.GetSource_Death(), (int)NPC.Center.X, (int)NPC.Center.Y, NPC.type)].velocity = new Vector2(3, 0).RotatedByRandom(MathHelper.TwoPi);
            }

            //explode
            Explode();

            return true;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Color color = Color.White;
            Vector2 drawOrigin = new Vector2(TextureAssets.Npc[NPC.type].Value.Width * 0.5f, NPC.height * 0.5f);
            Vector2 drawPos = NPC.position - Main.screenPosition + drawOrigin + new Vector2(0f, NPC.gfxOffY);

            spriteBatch.Draw(TextureAssets.Npc[NPC.type].Value, drawPos, NPC.frame, color * 0.5f, NPC.rotation, drawOrigin, NPC.scale * (1 + (float)Math.Cos(Main.GameUpdateCount / 5f)) * 0.25f + 1, SpriteEffects.None, 0f);
            spriteBatch.Draw(TextureAssets.Npc[NPC.type].Value, drawPos, NPC.frame, color * 0.5f, NPC.rotation, drawOrigin, NPC.scale * (1 - (float)Math.Cos(Main.GameUpdateCount / 5f)) * 0.25f + 1, SpriteEffects.None, 0f);
            spriteBatch.Draw(TextureAssets.Npc[NPC.type].Value, drawPos, NPC.frame, color * 0.5f, NPC.rotation + Main.GameUpdateCount / 20f, drawOrigin, NPC.scale * 1.25f, SpriteEffects.None, 0f);
            spriteBatch.Draw(TextureAssets.Npc[NPC.type].Value, drawPos, NPC.frame, color * 0.5f, NPC.rotation - Main.GameUpdateCount / 20f, drawOrigin, NPC.scale * 1.25f, SpriteEffects.None, 0f);
            spriteBatch.Draw(TextureAssets.Npc[NPC.type].Value, drawPos, NPC.frame, color, NPC.rotation, drawOrigin, NPC.scale, SpriteEffects.None, 0f);
            spriteBatch.Draw(ModContent.Request<Texture2D>($"{Texture}Eyes").Value, drawPos + (Main.player[Main.myPlayer].Center - NPC.Center).SafeNormalize(Vector2.Zero) * 2, new Rectangle(0, 0, 6, 4), color, NPC.rotation, new Vector2(3, 2), NPC.scale, SpriteEffects.None, 0f);

            return false;
        }

        public override void OnKill()
        {
            //if (Main.rand.NextBool(20))
            //{
            //    Item.NewItem(NPC.getRect(), ItemType<Items.Placeable.DendriticEnergy>());
            //}
        }
    }
}