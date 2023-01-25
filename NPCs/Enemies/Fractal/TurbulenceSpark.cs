using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Polarities.Biomes.Fractal;
using Polarities.Items.Materials;
using Polarities.Items.Placeable.Banners;
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
    public class TurbulenceSpark : ModNPC
    {
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 4;
        }

        public override void SetDefaults()
        {
            NPC.aiStyle = -1;
            NPC.width = 40;
            NPC.height = 40;

            NPC.defense = 8;
            NPC.damage = 30;
            NPC.lifeMax = 200;
            NPC.knockBackResist = 0.01f;
            NPC.npcSlots = 1f;
            NPC.HitSound = SoundID.NPCHit30;
            NPC.DeathSound = SoundID.NPCDeath33;
            NPC.value = 500;
            NPC.noTileCollide = true;
            NPC.noGravity = true;

            Banner = NPC.type;
            BannerItem = ItemType<TurbulenceSparkBanner>();

            this.SetModBiome<FractalBiome, FractalOceanBiome>();
        }

        public override void AI()
        {
            NPC.TargetClosest(true);
            Player player = Main.player[NPC.target];

            //if activated, yeet self at player
            if (NPC.life < NPC.lifeMax)
            {
                Vector2 goalVelocity = (player.Center - NPC.Center).SafeNormalize(Vector2.Zero) * 16;
                NPC.velocity += (goalVelocity - NPC.velocity) / 20f;
            }
            else
            {
                //these are basically just fractal points until activated
                NPC.velocity *= 0.98f;

                NPC.velocity += new Vector2(0.05f, 0).RotatedByRandom(MathHelper.TwoPi);
            }
        }

        public override void OnHitPlayer(Player target, int damage, bool crit)
        {
            //target.AddBuff(BuffType<Turbulence>(), 5 * 60);

            //explode
            Explode();
        }
        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            //explode
            Explode();
        }

        public override bool CheckDead()
        {
            //explode
            Explode();

            return true;
        }

        private void Explode()
        {
            SoundEngine.PlaySound(SoundID.NPCDeath33, NPC.Center);
            for (int i = 0; i < 16; i++)
            {
                Dust.NewDustPerfect(NPC.Center, DustID.Electric, new Vector2(Main.rand.Next(1, 5)).RotatedByRandom(MathHelper.TwoPi), Scale: 1f).noGravity = true;
            }
            NPC.active = false;
        }

        public override void FindFrame(int frameHeight)
        {
            NPC.frameCounter++;
            if (NPC.frameCounter == 5)
            {
                NPC.frameCounter = 0;
                NPC.frame.Y = (NPC.frame.Y + frameHeight) % (4 * frameHeight);
            }
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Color color = Color.White;
            Vector2 drawOrigin = new Vector2(TextureAssets.Npc[NPC.type].Value.Width * 0.5f, NPC.height * 0.5f);
            Vector2 drawPos = NPC.Center - screenPos;

            spriteBatch.Draw(TextureAssets.Npc[NPC.type].Value, drawPos, NPC.frame, color, NPC.rotation, drawOrigin, NPC.scale, SpriteEffects.None, 0f);
            spriteBatch.Draw(ModContent.Request<Texture2D>($"{Texture}_Mask").Value, drawPos, NPC.frame, color, NPC.rotation, drawOrigin, NPC.scale, SpriteEffects.None, 0f);

            return false;
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            //if (Subworld.IsActive<FractalSubworld>())
            //{
            //    return 0.75f * FractalSubworld.SpawnConditionFractalWaters(spawnInfo) * (1 - FractalSubworld.SpawnConditionFractalSky(spawnInfo));
            //}
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
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<FractalResidue>()));
        }
    }
}