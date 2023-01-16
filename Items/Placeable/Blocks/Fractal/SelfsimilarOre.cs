using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Polarities.Items.Placeable.Blocks.Fractal
{
    public class SelfsimilarOre : ModItem
    {
        public override void SetStaticDefaults()
        {
            SacrificeTotal = 100;
            ItemID.Sets.SortingPriorityMaterials[Type] = 58;
        }

        public override void SetDefaults()
        {
            Item.useStyle = 1;
            Item.useTurn = true;
            Item.useAnimation = 15;
            Item.useTime = 10;
            Item.autoReuse = true;
            Item.maxStack = 999;
            Item.consumable = true;
            Item.createTile = ModContent.TileType<SelfsimilarOreTile>();
            Item.rare = 7;
            Item.width = 38;
            Item.height = 28;
            Item.value = 2500;
        }
    }

	public class SelfsimilarOreTile : ModTile
	{
		public override void SetStaticDefaults()
		{
			TileID.Sets.Ore[Type] = true;
			Main.tileMergeDirt[Type] = false;
			Main.tileSpelunker[Type] = true;
			Main.tileLighted[Type] = true;
			Main.tileOreFinderPriority[Type] = 715;
			Main.tileShine2[Type] = true;
			Main.tileShine[Type] = 975;
			Main.tileSolid[Type] = true;
			Main.tileBlockLight[Type] = true;

			AddMapEntry(new Color(62, 8, 77), CreateMapEntryName());

			DustType = 118;
			ItemDrop = ModContent.ItemType<SelfsimilarOre>();
			HitSound = SoundID.Tink;
			MineResist = 6f;
			MinPick = 180;
		}

		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
		{
			r = 0.01f;
			g = 0.01f;
			b = 0.01f;
		}

		public override bool CanExplode(int i, int j)
		{
			return false;
		}

		public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem)
		{
			var player = Main.player?[Player.FindClosest(new Vector2(i * 16, j * 16), 16, 16)];

			if (player == null)
			{
				noItem = true;
			}
			//else if (Subworld.IsActive<FractalSubworld>() && !player.GetModPlayer<PolaritiesPlayer>().selfsimilarMining) // Avert if we're using a selfsimilar pick
			//{
			//	Vector2 position = new Vector2(i * 16 + 8, j * 16 + 8);

			//	Vector2 worldPosition = SelfsimilarSentinel.GetNearestArenaPosition(position);

			//	//if we're in an arena, fail and count up to your DOOM
			//	if ((worldPosition - position).Length() < SelfsimilarSentinel.ARENA_RADIUS && !effectOnly)
			//	{
			//		fail = true;

			//		if (!NPC.AnyNPCs(NPCType<SelfsimilarSentinel>()))
			//		{
			//			//get points in the vein
			//			List<Vector2> points = new List<Vector2>();
			//			for (int i2 = (int)(worldPosition.X - SelfsimilarSentinel.ARENA_RADIUS) / 16; i2 <= (int)(worldPosition.X + SelfsimilarSentinel.ARENA_RADIUS) / 16; i2++)
			//			{
			//				for (int j2 = (int)(worldPosition.Y - SelfsimilarSentinel.ARENA_RADIUS) / 16; j2 <= (int)(worldPosition.Y + SelfsimilarSentinel.ARENA_RADIUS) / 16; j2++)
			//				{
			//					if ((new Vector2(i2 * 16 + 8, j2 * 16 + 8) - worldPosition).Length() <= SelfsimilarSentinel.ARENA_RADIUS)
			//					{
			//						if (Main.tile[i2, j2].active() && Main.tile[i2, j2].type == TileType<Tiles.SelfsimilarOre>())
			//						{
			//							points.Add(new Vector2(i2 * 16 + 8, j2 * 16 + 8));
			//						}
			//					}
			//				}
			//			}

			//			//create projectiles from a bunch of random points
			//			float numShots = 16 / (float)Math.Max(1, 16 - player.GetModPlayer<PolaritiesPlayer>().selfsimilarHits);
			//			if (player.GetModPlayer<PolaritiesPlayer>().selfsimilarHits == 15) numShots *= 4;
			//			numShots = (numShots - 1) * 2 + 1;
			//			for (int n = 0; n < numShots; n++)
			//			{
			//				if (!Main.rand.NextBool(8))
			//				{
			//					Vector2 point = Main.rand.Next(points);

			//					HyperbolicTransform localTransform = HyperbolicTransform.FromPosition((point - worldPosition) / SelfsimilarSentinel.ARENA_RADIUS) * HyperbolicTransform.Rotation((point - worldPosition).ToRotation() + Main.rand.NextFloat(-0.5f, 0.5f));

			//					float speed = Main.rand.NextFloat(2, 6);
			//					if (player.GetModPlayer<PolaritiesPlayer>().selfsimilarHits == 15) speed = Main.rand.NextFloat(2, 12);
			//					HyperbolicWisp.NewProjectile(worldPosition, localTransform, new Vector2(speed, 0), 0, 0, extraUpdates: 3, ai0: Main.rand.NextFloat(-0.01f, 0.01f));
			//				}
			//			}

			//			HyperbolicTransform transform = HyperbolicTransform.FromPosition((position - worldPosition) / SelfsimilarSentinel.ARENA_RADIUS) * HyperbolicTransform.Rotation((position - worldPosition).ToRotation() + Main.rand.NextFloat(-0.5f, 0.5f));

			//			HyperbolicWisp.NewProjectile(worldPosition, transform, new Vector2(Main.rand.NextFloat(2, 6), 0), 0, 0, extraUpdates: 3, ai0: Main.rand.NextFloat(-0.01f, 0.01f));

			//			player.GetModPlayer<PolaritiesPlayer>().selfsimilarHits++;
			//			player.GetModPlayer<PolaritiesPlayer>().selfsimilarHitTimer = 60;

			//			if (player.GetModPlayer<PolaritiesPlayer>().selfsimilarHits >= 16)
			//			{
			//				foreach (Vector2 point in points)
			//				{
			//					if (Main.rand.NextBool())
			//					{
			//						HyperbolicTransform localTransform = HyperbolicTransform.FromPosition((point - worldPosition) / SelfsimilarSentinel.ARENA_RADIUS) * HyperbolicTransform.Rotation((point - worldPosition).ToRotation() + Main.rand.NextFloat(-1f, 1f));

			//						SentinelOreChunk.NewProjectile(worldPosition, localTransform, new Vector2(Main.rand.NextFloat(2, 16), 0), 0, 0, ai0: Main.rand.NextFloat(MathHelper.TwoPi));
			//					}
			//				}

			//				Main.PlaySound(SoundID.Shatter, worldPosition);

			//				SelfsimilarSentinel.SpawnSentinel(new Vector2(i * 16 + 8, j * 16 + 8));
			//			}
			//		}
			//	}
			//}
		}
	}
}
