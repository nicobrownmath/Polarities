using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace Polarities.Biomes
{
    public class SaltCave : ModBiome
    {
        public override SceneEffectPriority Priority => SceneEffectPriority.BiomeLow;

        public override int Music => -1;

        public override ModWaterStyle WaterStyle => ModContent.GetInstance<SaltWaterStyle>();

        public override string BestiaryIcon => (GetType().Namespace + "." + Name).Replace('.', '/') + "_BestiaryIcon";
        public override string BackgroundPath => base.BackgroundPath;
        public override Color? BackgroundColor => base.BackgroundColor;

        public override bool IsBiomeActive(Player player)
        {
            return ModContent.GetInstance<PolaritiesSystem>().saltBlockCount >= 500;
        }
    }

    public class SaltWaterStyle : ModWaterStyle
    {
        public override int ChooseWaterfallStyle()
            => ModContent.GetInstance<SaltWaterfallStyle>().Slot;

        public override int GetSplashDust()
            => ModContent.DustType<Dusts.SaltWaterSplash>();

        public override int GetDropletGore()
            => ModContent.GoreType<SaltDroplet>();

        public override void LightColorMultiplier(ref float r, ref float g, ref float b)
        {
            r = 0.2f;
            g = 0.5f;
            b = 0.5f;
        }

        public override Color BiomeHairColor()
            => new Color(0, 94, 94);
    }

    public class SaltWaterfallStyle : ModWaterfallStyle { }

    public class SaltDroplet : ModGore
    {
        public override void OnSpawn(Gore gore, IEntitySource source)
        {
            gore.numFrames = 15;
            gore.behindTiles = true;
            gore.timeLeft = Gore.goreTime * 3;
        }

        public override bool Update(Gore gore)
        {
            gore.alpha = gore.position.Y < Main.worldSurface * 16.0 + 8.0
                ? 0
                : 100;

            int frameDuration = 4;
            gore.frameCounter += 1;
            if (gore.frame <= 4)
            {
                int tileX = (int)(gore.position.X / 16f);
                int tileY = (int)(gore.position.Y / 16f) - 1;
                if (WorldGen.InWorld(tileX, tileY, 0) && !Main.tile[tileX, tileY].HasTile)
                {
                    gore.active = false;
                }
                if (gore.frame == 0 || gore.frame == 1 || gore.frame == 2)
                {
                    frameDuration = 24 + Main.rand.Next(256);
                }
                if (gore.frame == 3)
                {
                    frameDuration = 24 + Main.rand.Next(96);
                }
                if (gore.frameCounter >= frameDuration)
                {
                    gore.frameCounter = 0;
                    gore.frame += 1;
                    if (gore.frame == 5)
                    {
                        int droplet = Gore.NewGore(null, gore.position, gore.velocity, gore.type, 1f);
                        Main.gore[droplet].frame = 9;
                        Main.gore[droplet].velocity *= 0f;
                    }
                }
            }
            else if (gore.frame <= 6)
            {
                frameDuration = 8;
                if (gore.frameCounter >= frameDuration)
                {
                    gore.frameCounter = 0;
                    gore.frame += 1;
                    if (gore.frame == 7)
                    {
                        gore.active = false;
                    }
                }
            }
            else if (gore.frame <= 9)
            {
                frameDuration = 6;
                gore.velocity.Y += 0.2f;
                if (gore.velocity.Y < 0.5f)
                {
                    gore.velocity.Y = 0.5f;
                }
                if (gore.velocity.Y > 12f)
                {
                    gore.velocity.Y = 12f;
                }
                if (gore.frameCounter >= frameDuration)
                {
                    gore.frameCounter = 0;
                    gore.frame += 1;
                }
                if (gore.frame > 9)
                {
                    gore.frame = 7;
                }
            }
            else
            {
                gore.velocity.Y += 0.1f;
                if (gore.frameCounter >= frameDuration)
                {
                    gore.frameCounter = 0;
                    gore.frame += 1;
                }
                gore.velocity *= 0f;
                if (gore.frame > 14)
                {
                    gore.active = false;
                }
            }

            Vector2 oldVelocity = gore.velocity;
            gore.velocity = Collision.TileCollision(gore.position, gore.velocity, 16, 14, false, false, 1);
            if (gore.velocity != oldVelocity)
            {
                if (gore.frame < 10)
                {
                    gore.frame = 10;
                    gore.frameCounter = 0;
                    SoundEngine.PlaySound(new SoundStyle("Terraria/Sounds/Drip_" + Main.rand.Next(2)), gore.position + new Vector2(8));
                }
            }
            else if (Collision.WetCollision(gore.position + gore.velocity, 16, 14))
            {
                if (gore.frame < 10)
                {
                    gore.frame = 10;
                    gore.frameCounter = 0;
                    SoundEngine.PlaySound(new SoundStyle("Terraria/Sounds/Drip_2"), gore.position + new Vector2(8));
                }
                int tileX = (int)(gore.position.X + 8f) / 16;
                int tileY = (int)(gore.position.Y + 14f) / 16;
                if (Main.tile[tileX, tileY] != null && Main.tile[tileX, tileY].LiquidAmount > 0)
                {
                    gore.velocity *= 0f;
                    gore.position.Y = tileY * 16 - Main.tile[tileX, tileY].LiquidAmount / 16;
                }
            }

            gore.position += gore.velocity;
            return false;
        }
    }
}

