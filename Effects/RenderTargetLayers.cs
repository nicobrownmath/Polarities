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
using Terraria.DataStructures;
using Polarities.Items.Materials;
using Terraria.ModLoader.Utilities;
using System.Collections.Generic;
using Terraria.Graphics.Effects;

namespace Polarities.Effects
{
	//TODO: Fix zoom jittering
	public abstract class RenderTargetLayer : ARenderTargetContentByRequest, ILoadable
	{
		private static Dictionary<Type, RenderTargetLayer> renderTargetLayers = new Dictionary<Type, RenderTargetLayer>();

		public static T GetRenderTargetLayer<T>() where T : RenderTargetLayer
		{
			return renderTargetLayers[typeof(T)] as T;
		}

		bool patchesLoaded = false;

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

		private List<int> projCache;
		private List<int> npcCache;
		private bool active;
		public bool layerHasContent;

		public BlendState blendState = BlendState.AlphaBlend;
		public bool behindTiles = false;
		public bool doWeResetSpritebatch = false;
		public float targetScale = 1f;

		public void ResetCaches()
		{
			projCache = new List<int>();
			npcCache = new List<int>();
		}

		protected override void HandleUseReqest(GraphicsDevice device, SpriteBatch spriteBatch)
		{
			layerHasContent = false;
			if (projCache.Count > 0 || npcCache.Count > 0)
			{
				layerHasContent = true;

				PrepareARenderTarget_AndListenToEvents(ref _target, device, Main.screenWidth, Main.screenHeight, RenderTargetUsage.PreserveContents);
				device.SetRenderTarget(_target);
				device.Clear(Color.Transparent);

				Main.spriteBatch.Begin((SpriteSortMode)0, blendState, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, (Effect)null, Matrix.CreateTranslation(0f - Main.GameViewMatrix.Translation.X, 0f - Main.GameViewMatrix.Translation.Y, 0f) * Matrix.CreateScale(targetScale, targetScale, 1f));

				DoDraw();

				Main.spriteBatch.End();

				device.SetRenderTarget(null);
				_wasPrepared = true;
			}
		}

		public void DoDraw()
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
				if (_target != null && layerHasContent)
				{
					for (int i = 0; i < 4; i++)
					{
						Main.spriteBatch.Draw(_target, Main.GameViewMatrix.Translation + new Vector2(1 / targetScale, 0).RotatedBy(i * MathHelper.PiOver2), null, new Color(64, 64, 64), 0f, Vector2.Zero, 1 / targetScale, SpriteEffects.None, 0f);
					}
					Main.spriteBatch.Draw(_target, Main.GameViewMatrix.Translation, null, Color.Black, 0f, Vector2.Zero, 1 / targetScale, SpriteEffects.None, 0f);
				}
			}
		}

        private void ScreenObstruction_Draw(On.Terraria.GameContent.Events.ScreenObstruction.orig_Draw orig, SpriteBatch spriteBatch)
		{
			if (_target != null && layerHasContent)
			{
				spriteBatch.End();
				spriteBatch.Begin((SpriteSortMode)0, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, (Effect)null, Main.Transform);

				spriteBatch.Draw(_target, Main.GameViewMatrix.Translation, null, Color.White, 0f, Vector2.Zero, 1 / targetScale, SpriteEffects.None, 0f);

				spriteBatch.End();
				spriteBatch.Begin((SpriteSortMode)0, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, (Effect)null, Main.Transform);
			}

			orig(spriteBatch);
		}
	}
}

