using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace Polarities.Effects
{
    public abstract class DrawLayer : ILoadable
    {
        private static Dictionary<Type, DrawLayer> drawLayers = new Dictionary<Type, DrawLayer>();

        public static T GetDrawLayer<T>() where T : DrawLayer
        {
            return drawLayers[typeof(T)] as T;
        }

        public virtual void Load(Mod mod)
        {
            //register layer
            drawLayers[this.GetType()] = this;
            ResetCaches();
        }

        public virtual void Unload()
        {
            drawLayers = null;
        }

        public static void AddProjectile<T>(int index) where T : DrawLayer
        {
            GetDrawLayer<T>().projCache.Add(index);
        }

        public static void AddNPC<T>(int index) where T : DrawLayer
        {
            GetDrawLayer<T>().npcCache.Add(index);
        }

        public static bool IsActive<T>() where T : DrawLayer
        {
            return GetDrawLayer<T>().active;
        }

        private List<int> projCache;
        private List<int> npcCache;
        private bool active;

        public BlendState blendState = BlendState.AlphaBlend;
        public bool behindTiles = false;
        public bool doWeResetSpritebatch = false;

        public void ResetCaches()
        {
            projCache = new List<int>();
            npcCache = new List<int>();
        }

        public void Draw()
        {
            if (projCache.Count > 0 || npcCache.Count > 0)
            {
                active = true;
                if (doWeResetSpritebatch)
                {
                    Main.spriteBatch.End();
                }
                Main.spriteBatch.Begin((SpriteSortMode)0, blendState, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, (Effect)null, Main.Transform);

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

                Main.spriteBatch.End();
                if (doWeResetSpritebatch)
                {
                    Main.spriteBatch.Begin((SpriteSortMode)0, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, (Effect)null, Main.Transform);
                }

                ResetCaches();
                active = false;
            }
        }
    }

    public class DrawLayerAdditiveAfterProjectiles : DrawLayer
    {
        public override void Load(Mod mod)
        {
            base.Load(mod);

            blendState = BlendState.Additive;

            On.Terraria.Main.DrawProjectiles += Main_DrawProjectiles;
        }

        private void Main_DrawProjectiles(On.Terraria.Main.orig_DrawProjectiles orig, Main self)
        {
            orig(self);
            Draw();
        }
    }

    public class DrawLayerAdditiveBeforeNPCs : DrawLayer
    {
        public override void Load(Mod mod)
        {
            base.Load(mod);

            blendState = BlendState.Additive;

            On.Terraria.Main.DrawCachedProjs += Main_DrawCachedProjs;
        }

        private void Main_DrawCachedProjs(On.Terraria.Main.orig_DrawCachedProjs orig, Main self, List<int> projCache, bool startSpriteBatch)
        {
            if (projCache == Main.instance.DrawCacheProjsBehindNPCsAndTiles)
            {
                if (RenderTargetLayer.GetRenderTargetLayer<BehindTilesWithLightingTarget>().HasContent())
                {
                    Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, (Effect)null, Main.Transform);
                    RenderTargetLayer.GetRenderTargetLayer<BehindTilesWithLightingTarget>().Draw(Main.spriteBatch, Vector2.Zero, Color.White);
                    Main.spriteBatch.End();
                }
            }

            orig(self, projCache, startSpriteBatch);

            if (projCache == Main.instance.DrawCacheProjsBehindNPCs)
            {
                Draw();
                
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, (Effect)null, Main.Transform);
                ParticleLayer.BeforeNPCsAdditive.Draw(Main.spriteBatch);
                Main.spriteBatch.End();
            }
        }
    }

    public class DrawLayerAdditiveAfterNPCs : DrawLayer
    {
        public override void Load(Mod mod)
        {
            base.Load(mod);

            blendState = BlendState.Additive;
            doWeResetSpritebatch = true;

            On.Terraria.Main.DrawNPCs += Main_DrawNPCs;
        }

        private void Main_DrawNPCs(On.Terraria.Main.orig_DrawNPCs orig, Main self, bool behindTiles)
        {
            orig(self, behindTiles);

            if (!behindTiles)
            {
                Draw();
                GetDrawLayer<DrawLayerAfterAdditiveAfterNPCs>().Draw();
            }
        }
    }

    public class DrawLayerAfterAdditiveAfterNPCs : DrawLayer
    {
        public override void Load(Mod mod)
        {
            base.Load(mod);

            doWeResetSpritebatch = true;
        }
    }

    public class DrawLayerAdditiveAfterLiquids : DrawLayer
    {
        public override void Load(Mod mod)
        {
            base.Load(mod);

            blendState = BlendState.Additive;
            doWeResetSpritebatch = false;
        }
    }

    public class DrawLayerBeforeScreenObstruction : DrawLayer
    {
        public override void Load(Mod mod)
        {
            base.Load(mod);

            blendState = BlendState.AlphaBlend;
            doWeResetSpritebatch = false;
        }
    }
}