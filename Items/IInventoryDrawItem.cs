using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;

namespace Polarities.Items
{
    public interface IInventoryDrawItem
    {
        public virtual bool PreInventoryDraw(SpriteBatch spriteBatch, Item[] inv, int context, int slot, Vector2 position, Color lightColor) { return true; }
        public virtual void PostInventoryDraw(SpriteBatch spriteBatch, Item[] inv, int context, int slot, Vector2 position, Color lightColor) { }
    }
}

