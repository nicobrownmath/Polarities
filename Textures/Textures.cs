using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Polarities
{
	public class Textures : ILoadable
	{
        public static Asset<Texture2D> PixelTexture;
        public static Asset<Texture2D> Glow58;
        public static Asset<Texture2D> Glow256;
        public static Asset<Texture2D> Shockwave72;

        public void Load(Mod mod)
        {
            PixelTexture = mod.GetAsset<Texture2D>("Textures/Pixel");
            Glow58 = mod.GetAsset<Texture2D>("Textures/Glow58");
            Glow256 = mod.GetAsset<Texture2D>("Textures/Glow256");
            Shockwave72 = mod.GetAsset<Texture2D>("Textures/Shockwave72");
        }

        public void Unload()
        {
            PixelTexture = null;
            Glow58 = null;
            Glow256 = null;
            Shockwave72 = null;
        }
    }
}

