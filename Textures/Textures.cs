using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using System;
using System.IO;
using Microsoft.Xna.Framework;

namespace Polarities
{
	public class Textures : ILoadable
	{
        public static Asset<Texture2D> PixelTexture;
        public static Asset<Texture2D> Glow58;
        public static Asset<Texture2D> Glow256;
        public static Asset<Texture2D> Shockwave72;
        public static Asset<Texture2D> WarpZoom256;

        public void Load(Mod mod)
        {
            PixelTexture = mod.GetAsset<Texture2D>("Textures/Pixel");
            Glow58 = mod.GetAsset<Texture2D>("Textures/Glow58");
            Glow256 = mod.GetAsset<Texture2D>("Textures/Glow256");
            Shockwave72 = mod.GetAsset<Texture2D>("Textures/Shockwave72");
            WarpZoom256 = mod.GetAsset<Texture2D>("Textures/WarpZoom256");

            /*IL.Terraria.Main.UpdateMenu += Main_UpdateMenu;
		}

        private void Main_UpdateMenu(MonoMod.Cil.ILContext il)
        {
            MonoMod.Cil.ILCursor c = new MonoMod.Cil.ILCursor(il);

			c.EmitDelegate<Action>(() =>
			{
				if (!(bool)(typeof(ModLoader).GetField("isLoading", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic).GetValue(null)))
				{
					String filePath = Main.SavePath + Path.DirectorySeparatorChar + "ModSources/Polarities/Textures/WarpZoom256.png";

					if (!System.IO.File.Exists(filePath))
					{
						Terraria.Utilities.UnifiedRandom rand = new Terraria.Utilities.UnifiedRandom(278539);
						const int textureSize = 256;

						Texture2D texture = new Texture2D(Main.spriteBatch.GraphicsDevice, textureSize, textureSize, false, SurfaceFormat.Color);
						System.Collections.Generic.List<Color> list = new System.Collections.Generic.List<Color>();
						for (int i = 0; i < texture.Width; i++)
						{
							for (int j = 0; j < texture.Height; j++)
							{
								float x = (2 * j / (float)(texture.Width - 1) - 1);
								float y = (2 * i / (float)(texture.Height - 1) - 1);

                                float baseAlpha = (float)Math.Pow(1 - x * x - y * y, 2);
                                if (x * x + y * y >= 1) baseAlpha = 0;

                                Color baseColor = new Color((int)(128 + 128 * x), (int)(128 + 128 * y), 128);
                                int r = (int)baseColor.R;
								int g = (int)baseColor.G;
								int b = (int)baseColor.B;
								int alpha = (int)(255 * baseAlpha);

								list.Add(new Color((int)(r * alpha / 255f), (int)(g * alpha / 255f), (int)(b * alpha / 255f), alpha));
							}
						}
						texture.SetData(list.ToArray());
						texture.SaveAsPng(new System.IO.FileStream(filePath, System.IO.FileMode.Create), texture.Width, texture.Height);
					}
				}
			});*/
        }

        public void Unload()
        {
            PixelTexture = null;
            Glow58 = null;
            Glow256 = null;
            Shockwave72 = null;
            WarpZoom256 = null;
        }
    }
}

