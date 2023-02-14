using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Polarities.Biomes.Fractal;
using System;
using System.Collections;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;

namespace Polarities
{
    public class FractalSubworldSky : CustomSky
    {
        private bool isActive;
        private static readonly int maxClouds = 256;
        private float[,] clouds = new float[maxClouds, 7];

        private static readonly int maxBubbles = 128;
        private float[,] bubbles = new float[maxBubbles, 7];

        //clouds[i,0] = x position
        //clouds[i,1] = y position
        //clouds[i,2] = parallax multiplier
        //clouds[i,3] = secondary scale multiplier
        //clouds[i,4] = cloud type
        //clouds[i,5] = cloud rotation
        //clouds[i,6] = spriteEffects

        private static readonly int BackgroundTypes = 2;

        private static readonly int DefaultBackground = 0;
        private static readonly int OceanBackground = 1;

        private float[] backgroundAlpha = new float[BackgroundTypes];

        public override void OnLoad()
        {
        }

        public override void Update(GameTime gameTime)
        {
            //update clouds
            for (int i = 0; i < maxClouds; i++)
            {
                Vector2 drawPos = new Vector2(clouds[i, 0], clouds[i, 1]) + Main.screenPosition * clouds[i, 2] - Main.screenPosition;
                if ((drawPos - new Vector2(Main.screenWidth, Main.screenHeight) / 2).Length() > (new Vector2(Main.screenWidth, Main.screenHeight) * 2).Length())
                {
                    clouds[i, 2] = Main.rand.NextFloat(0.5f, 0.1f);
                    clouds[i, 3] = Main.rand.NextFloat(0.75f, 1.33f);
                    float parallaxScale = clouds[i, 2];
                    Vector2 offset = new Vector2(Main.screenWidth, Main.screenHeight) * 10;

                    //make it so offset can be onscreen if the player teleports/has just arrived in the dimension
                    if ((drawPos - new Vector2(Main.screenWidth, Main.screenHeight) / 2).Length() > (new Vector2(Main.screenWidth, Main.screenHeight) * 3).Length())
                    {
                        while (offset.Length() > (new Vector2(Main.screenWidth, Main.screenHeight) * 2).Length())
                        {
                            offset = new Vector2(Main.rand.NextFloat(-Main.screenWidth, Main.screenWidth), Main.rand.NextFloat(-Main.screenHeight, Main.screenHeight)) * 2;
                        }
                    }
                    else
                    {
                        offset = new Vector2(Main.screenWidth, Main.screenHeight).RotatedByRandom(MathHelper.TwoPi);
                    }

                    clouds[i, 0] = Main.screenPosition.X * (1 - parallaxScale) + Main.screenWidth / 2 + offset.X;
                    clouds[i, 1] = Main.screenPosition.Y * (1 - parallaxScale) + Main.screenHeight / 2 + offset.Y;
                    clouds[i, 4] = Main.rand.Next(1, 12);
                    clouds[i, 5] = Main.rand.NextFloat(MathHelper.TwoPi);
                    clouds[i, 6] = Main.rand.Next(2);
                }
            }

            //update bubbles
            for (int i = 0; i < maxBubbles; i++)
            {
                //rise very slowly
                bubbles[i, 1] -= 0.01f / bubbles[i, 2];

                Vector2 drawPos = new Vector2(bubbles[i, 0], bubbles[i, 1]) + Main.screenPosition * bubbles[i, 2] - Main.screenPosition;
                if ((drawPos - new Vector2(Main.screenWidth, Main.screenHeight) / 2).Length() > (new Vector2(Main.screenWidth, Main.screenHeight) * 2).Length())
                {
                    bubbles[i, 2] = Main.rand.NextFloat(0.5f, 0.1f);
                    bubbles[i, 3] = Main.rand.NextFloat(0.75f, 1.33f);
                    float parallaxScale = bubbles[i, 2];
                    Vector2 offset = new Vector2(Main.screenWidth, Main.screenHeight) * 10;

                    //make it so offset can be onscreen if the player teleports/has just arrived in the dimension
                    if ((drawPos - new Vector2(Main.screenWidth, Main.screenHeight) / 2).Length() > (new Vector2(Main.screenWidth, Main.screenHeight) * 3).Length())
                    {
                        while (offset.Length() > (new Vector2(Main.screenWidth, Main.screenHeight) * 2).Length())
                        {
                            offset = new Vector2(Main.rand.NextFloat(-Main.screenWidth, Main.screenWidth), Main.rand.NextFloat(-Main.screenHeight, Main.screenHeight)) * 2;
                        }
                    }
                    else
                    {
                        offset = new Vector2(Main.screenWidth, Main.screenHeight).RotatedByRandom(MathHelper.TwoPi);
                    }

                    bubbles[i, 0] = Main.screenPosition.X * (1 - parallaxScale) + Main.screenWidth / 2 + offset.X;
                    bubbles[i, 1] = Main.screenPosition.Y * (1 - parallaxScale) + Main.screenHeight / 2 + offset.Y;
                    bubbles[i, 4] = Main.rand.Next(1, 5);
                    bubbles[i, 6] = Main.rand.Next(2);

                    //bubbles don't rotate
                    bubbles[i, 5] = 0;
                }
            }
        }

        public override void Draw(SpriteBatch spriteBatch, float minDepth, float maxDepth)
        {
            //draw the sky
            if (maxDepth >= 0 && minDepth < 0)
            {
                float value = 2 - 2 * (Main.player[Main.myPlayer].position.Y / ((float)Main.maxTilesY * 16));
                Color drawColor = new Color((int)(value * 128 + (128 - 44) * value * (value - 2)), (int)(value * 128 + (128 - 124) * value * (value - 2)), (int)(value * 128 + (128 - 154) * value * (value - 2)));

                spriteBatch.Draw(TextureAssets.BlackTile.Value, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), drawColor);
            }

            if (Main.BackgroundEnabled)
            {
                //update background alphas
                int goalBackground = GetGoalBackground();

                float backgroundTransformationSpeed = 1 / 60f;
                int numFadingBackgrounds = 0;
                bool justStarted = true;
                for (int i = 0; i < BackgroundTypes; i++)
                {
                    if (i != goalBackground)
                    {
                        numFadingBackgrounds++;
                    }
                    if (backgroundAlpha[i] != 0)
                    {
                        justStarted = false;
                    }
                }

                if (justStarted)
                {
                    backgroundAlpha[goalBackground] = 1;
                }
                else
                {
                    for (int i = 0; i < BackgroundTypes; i++)
                    {
                        if (i == goalBackground)
                        {
                            backgroundAlpha[i] += backgroundTransformationSpeed;
                            if (backgroundAlpha[i] > 1)
                            {
                                backgroundAlpha[i] = 1;
                            }
                        }
                        else
                        {
                            backgroundAlpha[i] -= backgroundTransformationSpeed / numFadingBackgrounds;
                            if (backgroundAlpha[i] < 0)
                            {
                                backgroundAlpha[i] = 0;
                            }
                        }
                    }
                }

                //draw the repeating backgrounds
                //make horizontal parallax more than vertical parallax because mind bendingness
                DrawFractalBackgroundTexture(spriteBatch, ModContent.Request<Texture2D>("Polarities/Biomes/Fractal/FractalBackgroundFar").Value, 0.05f, 0.04f, backgroundAlpha[DefaultBackground] + backgroundAlpha[OceanBackground]);
                DrawFractalBackgroundTexture(spriteBatch, ModContent.Request<Texture2D>("Polarities/Biomes/Fractal/FractalBackgroundMid").Value, 0.1f, 0.08f, backgroundAlpha[DefaultBackground] + backgroundAlpha[OceanBackground]);
                DrawFractalBackgroundTexture(spriteBatch, ModContent.Request<Texture2D>("Polarities/Biomes/Fractal/FractalBackgroundClose").Value, 0.15f, 0.12f, backgroundAlpha[DefaultBackground]);
                DrawFractalBackgroundTexture(spriteBatch, ModContent.Request<Texture2D>("Polarities/Biomes/Fractal/FractalOceanBackgroundClose").Value, 0.15f, 0.12f, backgroundAlpha[OceanBackground]);
            }

            //draw the clouds
            if (backgroundAlpha[DefaultBackground] > 0)
            {
                int[] cloudOrder = new int[maxClouds];
                for (int i = 0; i < maxClouds; i++)
                {
                    cloudOrder[i] = i;
                }
                CloudComparer cloudComparer = new CloudComparer(clouds);
                Array.Sort(cloudOrder, cloudComparer);

                for (int j = 0; j < maxClouds; j++)
                {
                    int i = cloudOrder[j];

                    float zoom = Main.GameZoomTarget;

                    Texture2D cloudTexture = ModContent.Request<Texture2D>("Polarities/Biomes/Fractal/notcloud_" + (int)Math.Max(clouds[i, 4], 1)).Value;
                    Vector2 drawPos = new Vector2(clouds[i, 0], clouds[i, 1]) + Main.screenPosition * clouds[i, 2] - Main.screenPosition;

                    drawPos -= new Vector2(Main.screenWidth / 2, Main.screenHeight / 2);
                    drawPos *= zoom;
                    drawPos += new Vector2(Main.screenWidth / 2, Main.screenHeight / 2);

                    float textureRadius = cloudTexture.Size().Length() / 2;
                    if (drawPos.X > -textureRadius && drawPos.X < Main.screenWidth + textureRadius && drawPos.Y > -textureRadius && drawPos.Y < Main.screenHeight + textureRadius)
                        spriteBatch.Draw(cloudTexture, drawPos, cloudTexture.Frame(), Color.White * backgroundAlpha[DefaultBackground], clouds[i, 5], cloudTexture.Size() / 2, 1.33f * (1 - clouds[i, 2]) * clouds[i, 3] * zoom, (SpriteEffects)clouds[i, 6], 0f);
                }
            }

            //draw the bubbles
            if (backgroundAlpha[OceanBackground] > 0)
            {
                int[] bubbleOrder = new int[maxBubbles];
                for (int i = 0; i < maxBubbles; i++)
                {
                    bubbleOrder[i] = i;
                }
                CloudComparer bubbleComparer = new CloudComparer(bubbles);
                Array.Sort(bubbleOrder, bubbleComparer);

                for (int j = 0; j < maxBubbles; j++)
                {
                    int i = bubbleOrder[j];

                    float zoom = Main.GameZoomTarget;

                    float value = 2 - 2 * (Main.player[Main.myPlayer].position.Y / ((float)Main.maxTilesY * 16));
                    Color drawColor = new Color((int)(value * 128 + (128 - 44) * value * (value - 2)), (int)(value * 128 + (128 - 124) * value * (value - 2)), (int)(value * 128 + (128 - 154) * value * (value - 2)));

                    Texture2D bubbleTexture = ModContent.Request<Texture2D>("Polarities/Biomes/Fractal/bobble_" + (int)bubbles[i, 4]).Value;
                    Vector2 drawPos = new Vector2(bubbles[i, 0], bubbles[i, 1]) + Main.screenPosition * bubbles[i, 2] - Main.screenPosition;

                    drawPos -= new Vector2(Main.screenWidth / 2, Main.screenHeight / 2);
                    drawPos *= zoom;
                    drawPos += new Vector2(Main.screenWidth / 2, Main.screenHeight / 2);

                    float textureRadius = bubbleTexture.Size().Length() / 2;
                    if (drawPos.X > -textureRadius && drawPos.X < Main.screenWidth + textureRadius && drawPos.Y > -textureRadius && drawPos.Y < Main.screenHeight + textureRadius)
                        spriteBatch.Draw(bubbleTexture, drawPos, bubbleTexture.Frame(), drawColor * backgroundAlpha[OceanBackground], bubbles[i, 5], bubbleTexture.Size() / 2, 1.33f * (1 - bubbles[i, 2]) * bubbles[i, 3] * zoom, (SpriteEffects)bubbles[i, 6], 0f);
                }
            }
        }

        private int GetGoalBackground()
        {
            if (Main.player[Main.myPlayer].InModBiome<FractalSkyBiome>())
            {
                return DefaultBackground;
            }
            if (Main.player[Main.myPlayer].InModBiome<FractalOceanBiome>())
            {
                return OceanBackground;
            }
            return DefaultBackground;
        }

        private void DrawFractalBackgroundTexture(SpriteBatch spriteBatch, Texture2D texture, float largeBackgroundParallaxFactorX, float largeBackgroundParallaxFactorY, float alpha = 1)
        {
            if (alpha == 0)
            {
                return;
            }

            Vector2 basePosition = new Vector2(Main.maxTilesX * 8, Main.maxTilesY * 8);

            basePosition.Y += Main.maxTilesY * 8 * (float)Math.Pow((Main.screenPosition.X - Main.maxTilesX * 8) / (Main.maxTilesX * 8), 2);

            Vector2 largeBackgroundPosition = new Vector2(largeBackgroundParallaxFactorX * basePosition.X, largeBackgroundParallaxFactorY * basePosition.Y);
            Vector2 parallaxVector = new Vector2((1f - largeBackgroundParallaxFactorX) * Main.screenPosition.X, (1f - largeBackgroundParallaxFactorY) * Main.screenPosition.Y);

            Vector2 drawPos = largeBackgroundPosition + parallaxVector - Main.screenPosition;

            drawPos += new Vector2(Main.screenWidth / 2, Main.screenHeight / 2);

            //move to center
            while (drawPos.X < 0)
            {
                drawPos.X += texture.Width;
            }
            while (drawPos.X >= Main.screenWidth)
            {
                drawPos.X -= texture.Width;
            }

            for (int i = -2; i <= 2; i++)
            {
                float value = 2 - 2 * (Main.player[Main.myPlayer].position.Y / ((float)Main.maxTilesY * 16));
                Color drawColor = new Color((int)(value * 128 + (128 - 44) * value * (value - 2)), (int)(value * 128 + (128 - 124) * value * (value - 2)), (int)(value * 128 + (128 - 154) * value * (value - 2)));
                drawColor *= alpha;

                int yOffsetFromBottomOfScreen = (int)((Main.screenPosition.Y + Main.screenHeight) - (drawPos.Y + texture.Height));

                spriteBatch.Draw(texture, drawPos + i * new Vector2(texture.Width, 0), new Rectangle(0, 0, texture.Width, texture.Height + yOffsetFromBottomOfScreen), drawColor, 0f, texture.Size() / 2, 1f, SpriteEffects.None, 0f);
            }
        }

        public override float GetCloudAlpha()
        {
            return 0f;
        }

        public override void Activate(Vector2 position, params object[] args)
        {
            isActive = true;
        }

        public override void Deactivate(params object[] args)
        {
            isActive = false;
        }

        public override void Reset()
        {
            isActive = false;
        }

        public override bool IsActive()
        {
            return isActive;
        }
    }

    public class FractalScreenShaderData : ScreenShaderData
    {
        public FractalScreenShaderData(string passName)
            : base(passName)
        {
        }

        public override void Apply()
        {
            //base.Apply();
        }
    }

    internal class CloudComparer : IComparer<int>
    {
        private float[,] clouds;

        public CloudComparer(float[,] cloudInit)
        {
            clouds = cloudInit;
        }

        public int Compare(int x, int y)
        {
            return ((new CaseInsensitiveComparer()).Compare(clouds[y, 2], clouds[x, 2]));
        }
    }
}
