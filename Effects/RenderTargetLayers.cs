using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace Polarities.Effects
{
    //TODO: Fix zoom jittering
    //TODO: Priority value
    public abstract class RenderTargetLayer : ARenderTargetContentByRequest, ILoadable
    {
        private static Dictionary<Type, RenderTargetLayer> renderTargetLayers = new Dictionary<Type, RenderTargetLayer>();

        public static T GetRenderTargetLayer<T>() where T : RenderTargetLayer
        {
            return renderTargetLayers[typeof(T)] as T;
        }

        private static bool patchesLoaded = false;

        public virtual void Load(Mod mod)
        {
            //register layer
            renderTargetLayers[this.GetType()] = this;
            ResetCaches();

            if (!patchesLoaded)
            {
                patchesLoaded = true;

                On.Terraria.Main.ClampScreenPositionToWorld += Main_ClampScreenPositionToWorld;
            }
        }

        private void Main_ClampScreenPositionToWorld(On.Terraria.Main.orig_ClampScreenPositionToWorld orig)
        {
            orig();

            foreach (RenderTargetLayer layer in renderTargetLayers.Values)
            {
                layer.Request();
                layer.PrepareRenderTarget(Main.graphics.GraphicsDevice, Main.spriteBatch);
            }

            //screen warp
            if (GetRenderTargetLayer<ScreenWarpTarget>().HasContent() || ParticleLayer.WarpParticles.particles.Count > 0)
            {
                if (!Terraria.Graphics.Effects.Filters.Scene["Polarities:ScreenWarp"].Active)
                    Terraria.Graphics.Effects.Filters.Scene.Activate("Polarities:ScreenWarp");

                Terraria.Graphics.Effects.Filters.Scene["Polarities:ScreenWarp"].GetShader().UseImage(GetRenderTargetLayer<ScreenWarpTarget>().GetTarget(), 0, SamplerState.PointWrap);
            }
            else if (Terraria.Graphics.Effects.Filters.Scene["Polarities:ScreenWarp"].Active)
                Terraria.Graphics.Effects.Filters.Scene.Deactivate("Polarities:ScreenWarp");
        }

        public virtual void Unload()
        {
            renderTargetLayers = null;
        }

        public static void AddProjectile<T>(int index) where T : RenderTargetLayer
        {
            GetRenderTargetLayer<T>().projCache.Add(index);
        }

        public static void AddNPC<T>(int index) where T : RenderTargetLayer
        {
            GetRenderTargetLayer<T>().npcCache.Add(index);
        }

        public static bool IsActive<T>() where T : RenderTargetLayer
        {
            return GetRenderTargetLayer<T>().active;
        }

        public bool HasContent()
        {
            return _target != null && layerHasContent;
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 offset, Color color, Vector2? center = null)
        {
            spriteBatch.Draw(_target, (useIdentityMatrix ? Vector2.Zero : Main.GameViewMatrix.Translation) + offset, null, color, 0f, center ?? Vector2.Zero, 1 / targetScale, SpriteEffects.None, 0f);
        }

        private List<int> projCache;
        private List<int> npcCache;
        private bool active;
        public bool layerHasContent;

        public BlendState blendState = BlendState.AlphaBlend;
        public bool behindTiles = false;
        public bool doWeResetSpritebatch = false;
        public float targetScale = 1f;

        public virtual int Width => Main.screenWidth;
        public virtual int Height => Main.screenHeight;
        public Vector2 Center => new Vector2(Width / 2, Height / 2);

        public virtual Color resetColor => Color.Transparent;
        public virtual bool useIdentityMatrix => false;

        public void ResetCaches()
        {
            projCache = new List<int>();
            npcCache = new List<int>();
        }

        protected override void HandleUseReqest(GraphicsDevice device, SpriteBatch spriteBatch)
        {
            layerHasContent = false;
            if (DoesTargetDraw() ?? (projCache.Count > 0 || npcCache.Count > 0))
            {
                layerHasContent = true;

                PrepareARenderTarget_AndListenToEvents(ref _target, device, Width, Height, RenderTargetUsage.PreserveContents);
                device.SetRenderTarget(_target);
                device.Clear(resetColor);

                Matrix drawMatrix = useIdentityMatrix ? Matrix.Identity : Matrix.CreateTranslation(0f - Main.GameViewMatrix.Translation.X, 0f - Main.GameViewMatrix.Translation.Y, 0f) * Matrix.CreateScale(targetScale, targetScale, 1f);
                Main.spriteBatch.Begin(0, blendState, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, drawMatrix);

                DoDraw();

                Main.spriteBatch.End();

                device.SetRenderTarget(null);
                _wasPrepared = true;
            }
        }

        public virtual bool? DoesTargetDraw() => null;

        public virtual void DoDraw()
        {
            active = true;
            for (int i = 0; i < projCache.Count; i++)
            {
                try
                {
                    Main.instance.DrawProj(projCache[i]);
                }
                catch (Exception e)
                {
                    TimeLogger.DrawException(e);
                    Main.projectile[projCache[i]].active = false;
                }
            }

            for (int i = 0; i < npcCache.Count; i++)
            {
                try
                {
                    Main.instance.DrawNPC(npcCache[i], behindTiles);
                }
                catch (Exception e)
                {
                    TimeLogger.DrawException(e);
                    Main.npc[npcCache[i]].active = false;
                }
            }

            ResetCaches();
            active = false;
        }
    }

    public class ConvectiveWandererTarget : RenderTargetLayer
    {
        public override void Load(Mod mod)
        {
            base.Load(mod);

            On.Terraria.GameContent.Events.ScreenObstruction.Draw += ScreenObstruction_Draw;
            On.Terraria.Main.DrawCachedNPCs += Main_DrawCachedNPCs;

            targetScale = 0.5f;
        }

        private void Main_DrawCachedNPCs(On.Terraria.Main.orig_DrawCachedNPCs orig, Main self, List<int> npcCache, bool behindTiles)
        {
            orig(self, npcCache, behindTiles);

            if (behindTiles && npcCache == Main.instance.DrawCacheNPCsMoonMoon)
            {
                if (GetRenderTargetLayer<ConvectiveEnemyTarget>().HasContent())
                    GetRenderTargetLayer<ConvectiveEnemyTarget>().Draw(Main.spriteBatch, Vector2.Zero, Color.Black);

                if (HasContent())
                {
                    Main.spriteBatch.End();
                    Main.spriteBatch.Begin(0, new BlendState()
                    {
                        //custom blendstate to make it so outlines in darker places are more darker
                        BlendFactor = Color.White,

                        AlphaBlendFunction = BlendFunction.Add,
                        AlphaSourceBlend = Blend.One,
                        AlphaDestinationBlend = Blend.InverseSourceAlpha,

                        ColorBlendFunction = BlendFunction.Add,
                        ColorSourceBlend = Blend.SourceColor,
                        ColorDestinationBlend = Blend.InverseSourceAlpha,
                    },
                    Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.Transform);

                    for (int i = 0; i < 4; i++)
                    {
                        Draw(Main.spriteBatch, new Vector2(1 / targetScale, 0).RotatedBy(i * MathHelper.PiOver2), Color.White);
                    }

                    Main.spriteBatch.End();
                    Main.spriteBatch.Begin(0, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.Transform);

                    Draw(Main.spriteBatch, Vector2.Zero, Color.Black);
                }
            }
        }

        public static float extraGlow = 0f;

        private void ScreenObstruction_Draw(On.Terraria.GameContent.Events.ScreenObstruction.orig_Draw orig, SpriteBatch spriteBatch)
        {
            spriteBatch.End();

            DrawLayer.GetDrawLayer<DrawLayerAdditiveAfterLiquids>().Draw();

            spriteBatch.Begin(0, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.Transform);

            ParticleLayer.AfterLiquidsAdditive.Draw(Main.spriteBatch);

            if (GetRenderTargetLayer<ConvectiveEnemyTarget>().HasContent())
                GetRenderTargetLayer<ConvectiveEnemyTarget>().Draw(spriteBatch, Vector2.Zero, Color.White);

            if (HasContent())
            {
                Draw(Main.spriteBatch, Vector2.Zero, Color.White);

                spriteBatch.End();
                spriteBatch.Begin((SpriteSortMode)1, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.Transform);

                GameShaders.Armor.GetShaderFromItemId(ItemID.ColorOnlyDye).Apply();

                Draw(Main.spriteBatch, Vector2.Zero, Color.White * extraGlow);
            }

            spriteBatch.End();
            DrawLayer.GetDrawLayer<DrawLayerBeforeScreenObstruction>().Draw();
            DrawLayer.GetDrawLayer<DrawLayerAdditiveBeforeScreenObstruction>().Draw();
            DrawLayer.GetDrawLayer<DrawLayerAfterAdditiveBeforeScreenObstruction>().Draw();
            spriteBatch.Begin(0, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.Transform);
            orig(spriteBatch);
        }
    }

    public class ConvectiveEnemyTarget : RenderTargetLayer { }

    public class ScreenWarpTarget : RenderTargetLayer
    {
        public override Color resetColor => new Color(128, 128, 128);

        public override bool? DoesTargetDraw()
        {
            return ParticleLayer.WarpParticles.particles.Count > 0 ? true : null;
        }

        public override void DoDraw()
        {
            base.DoDraw();

            ParticleLayer.WarpParticles.Draw(Main.spriteBatch);
        }
    }

    public class BehindTilesWithLightingTarget : RenderTargetLayer
    {
        public override void Load(Mod mod)
        {
            base.Load(mod);

            targetScale = 1f;
        }

        public override void DoDraw()
        {
            base.DoDraw();

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(0,
                new BlendState()
                {
                    AlphaBlendFunction = BlendFunction.Add,
                    AlphaSourceBlend = Blend.Zero,
                    AlphaDestinationBlend = Blend.One,
                    ColorBlendFunction = BlendFunction.Add,
                    ColorSourceBlend = Blend.Zero,
                    ColorDestinationBlend = Blend.SourceColor
                },
                Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Matrix.CreateTranslation(0f - Main.GameViewMatrix.Translation.X, 0f - Main.GameViewMatrix.Translation.Y, 0f) * Matrix.CreateScale(targetScale, targetScale, 1f));

            //draw lighting
            //Note: If I ever do another layer with lighting, I should move this to a separate rendertarget for ease of reuse
            //TODO: This can probably be made smoother, I should look at the tile drawing code for that
            for (int i = (int)Main.screenPosition.X / 16; i < (int)(Main.screenPosition.X + Main.screenWidth) / 16; i++)
            {
                for (int j = (int)Main.screenPosition.Y / 16; j < (int)(Main.screenPosition.Y + Main.screenHeight) / 16; j++)
                {
                    Color lightColor = Lighting.GetColor(i, j);
                    Vector2 drawPos = new Vector2(i, j) * 16 - Main.screenPosition;
                    Main.spriteBatch.Draw(Textures.PixelTexture.Value, drawPos, new Rectangle(0, 0, 16, 16), lightColor);
                }
            }
        }
    }
}