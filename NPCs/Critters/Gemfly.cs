using Microsoft.Xna.Framework;
using MonoMod.Cil;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent.Bestiary;
using ReLogic.Content;
using Terraria.Audio;
using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.Localization;

namespace Polarities.NPCs.Critters
{
	public abstract class Gemfly : ModNPC
	{
		public override string Texture => "Polarities/NPCs/Critters/Gemfly";

		static Asset<Texture2D> Glow;
		static bool hasAddedShakeTreeEdit;

		public abstract int Index { get; }
		public abstract int CatchItem { get; }
		public virtual int ExpectedTime => 60 * 30 * (Index + 1);

        public override void Load()
        {
			if (Glow == null) Glow = Request<Texture2D>(Texture + "_Mask");

			//spawn gemflies and other misc loot from shaking gem trees
			if (!hasAddedShakeTreeEdit)
			{
				On.Terraria.WorldGen.ShakeTree += WorldGen_ShakeTree;
				hasAddedShakeTreeEdit = true;
			}
		}

		private static void GetGemTreeBottom(int i, int j, out int x, out int y)
        {
			x = i;
			y = j;
			Tile tileSafely = Framing.GetTileSafely(x, y);
			int num = tileSafely.TileFrameX / 22;
			int num2 = tileSafely.TileFrameY / 22;
			if (num == 3 && num2 <= 2)
			{
				x++;
			}
			else if (num == 4 && num2 >= 3 && num2 <= 5)
			{
				x--;
			}
			else if (num == 1 && num2 >= 6 && num2 <= 8)
			{
				x--;
			}
			else if (num == 2 && num2 >= 6 && num2 <= 8)
			{
				x++;
			}
			else if (num == 2 && num2 >= 9)
			{
				x++;
			}
			else if (num == 3 && num2 >= 9)
			{
				x--;
			}
			tileSafely = Framing.GetTileSafely(x, y);
			while (y < Main.maxTilesY - 50 && (!tileSafely.HasTile || TileID.Sets.IsATreeTrunk[tileSafely.TileType]))
			{
				y++;
				tileSafely = Framing.GetTileSafely(x, y);
			}
		}

        private void WorldGen_ShakeTree(On.Terraria.WorldGen.orig_ShakeTree orig, int i, int j)
        {
			List<int> gemTrees = new List<int> { TileID.TreeAmethyst, TileID.TreeTopaz, TileID.TreeSapphire, TileID.TreeEmerald, TileID.TreeRuby, TileID.TreeDiamond };

			if (!gemTrees.Contains(Main.tile[i, j].TileType))
			{
				orig(i, j);
				return;
			}

			if (WorldGen.numTreeShakes == WorldGen.maxTreeShakes)
			{
				return;
			}
			GetGemTreeBottom(i, j, out var x, out var y);
			int num = y;

			for (int k = 0; k < WorldGen.numTreeShakes; k++)
			{
				if (WorldGen.treeShakeX[k] == x && WorldGen.treeShakeY[k] == y)
				{
					return;
				}
			}
			WorldGen.treeShakeX[WorldGen.numTreeShakes] = x;
			WorldGen.treeShakeY[WorldGen.numTreeShakes] = y;
			WorldGen.numTreeShakes++;
			y--;
			while (y > 10 && Main.tile[x, y].HasTile && TileID.Sets.IsShakeable[Main.tile[x, y].TileType])
			{
				y--;
			}
			y++;

			bool isAGemTreetop = false;
            {
				Tile tileSafely = Framing.GetTileSafely(x, y);
				if (tileSafely.HasTile && TileID.Sets.GetsCheckedForLeaves[tileSafely.TileType])
				{
					if (tileSafely.TileFrameX == 22 && tileSafely.TileFrameY >= 198 && tileSafely.TileFrameY <= 242)
					{
						isAGemTreetop = true;
					}
				}
			}

			if (!isAGemTreetop || Collision.SolidTiles(x - 2, x + 2, y - 2, y + 2))
			{
				return;
			}
			bool flag = false;
			if (Main.getGoodWorld && WorldGen.genRand.Next(15) == 0)
			{
				Projectile.NewProjectile(new EntitySource_ShakeTree(x, y), x * 16, y * 16, (float)Main.rand.Next(-100, 101) * 0.002f, 0f, 28, 0, 0f, Player.FindClosest(new Vector2((float)(x * 16), (float)(y * 16)), 16, 16));
			}
			else if (WorldGen.genRand.NextBool())
			{
				flag = true;
				int itemID;
				switch (Main.tile[x, y].TileType)
				{
					case TileID.TreeAmethyst:
						itemID = ItemID.Amethyst;
						break;
					case TileID.TreeTopaz:
						itemID = ItemID.Topaz;
						break;
					case TileID.TreeSapphire:
						itemID = ItemID.Sapphire;
						break;
					case TileID.TreeEmerald:
						itemID = ItemID.Emerald;
						break;
					case TileID.TreeRuby:
						itemID = ItemID.Ruby;
						break;
					case TileID.TreeDiamond:
						itemID = ItemID.Diamond;
						break;
					case TileID.TreeAmber:
						itemID = ItemID.Amber;
						break;
					default:
						return;
				}
				Item.NewItem(new EntitySource_ShakeTree(x, y), x * 16, y * 16, 16, 16, itemID);
			}
			else if (WorldGen.genRand.NextBool(3))
			{
				flag = true;
				int itemID;
				switch (Main.tile[x, y].TileType)
				{
					case TileID.TreeAmethyst:
						itemID = ItemID.GemTreeAmethystSeed;
						break;
					case TileID.TreeTopaz:
						itemID = ItemID.GemTreeTopazSeed;
						break;
					case TileID.TreeSapphire:
						itemID = ItemID.GemTreeSapphireSeed;
						break;
					case TileID.TreeEmerald:
						itemID = ItemID.GemTreeEmeraldSeed;
						break;
					case TileID.TreeRuby:
						itemID = ItemID.GemTreeRubySeed;
						break;
					case TileID.TreeDiamond:
						itemID = ItemID.GemTreeDiamondSeed;
						break;
					case TileID.TreeAmber:
						itemID = ItemID.GemTreeAmberSeed;
						break;
					default:
						return;
				}
				Item.NewItem(new EntitySource_ShakeTree(x, y), x * 16, y * 16, 16, 16, itemID);
			}
			else if (WorldGen.genRand.NextBool())
            {
				flag = true;
				int npcID;
				switch(Main.tile[x, y].TileType)
                {
					case TileID.TreeAmethyst:
						npcID = NPCType<AmethystGemfly>();
						break;
					case TileID.TreeTopaz:
						npcID = NPCType<TopazGemfly>();
						break;
					case TileID.TreeSapphire:
						npcID = NPCType<SapphireGemfly>();
						break;
					case TileID.TreeEmerald:
						npcID = NPCType<EmeraldGemfly>();
						break;
					case TileID.TreeRuby:
						npcID = NPCType<RubyGemfly>();
						break;
					case TileID.TreeDiamond:
						npcID = NPCType<DiamondGemfly>();
						break;
					case TileID.TreeAmber:
						npcID = NPCType<AmberGemfly>();
						break;
					default:
						return;
                }

				int numFlies = WorldGen.genRand.Next(1, 3);
				//spawns 1-2 gemflies
				for (int c = 0; c < numFlies; c++)
				{
					NPC.NewNPC(new EntitySource_ShakeTree(x, y), x * 16, y * 16, npcID, ai2: -1f);
				}
			}
			else if (WorldGen.genRand.NextBool())
			{
				flag = true;
				int npcID;
				switch (Main.tile[x, y].TileType)
				{
					case TileID.TreeAmethyst:
						npcID = NPCID.GemSquirrelAmethyst;
						break;
					case TileID.TreeTopaz:
						npcID = NPCID.GemSquirrelTopaz;
						break;
					case TileID.TreeSapphire:
						npcID = NPCID.GemSquirrelSapphire;
						break;
					case TileID.TreeEmerald:
						npcID = NPCID.GemSquirrelEmerald;
						break;
					case TileID.TreeRuby:
						npcID = NPCID.GemSquirrelRuby;
						break;
					case TileID.TreeDiamond:
						npcID = NPCID.GemSquirrelDiamond;
						break;
					case TileID.TreeAmber:
						npcID = NPCID.GemSquirrelAmber;
						break;
					default:
						return;
				}

				//spawn a gem squirrel
				NPC.NewNPC(new EntitySource_ShakeTree(x, y), x * 16, y * 16, npcID);
			}

			if (flag)
			{
				int treeHeight = 0;
				int treeFrame = 0;
				int passStyle = 0;
				WorldGen.GetTreeLeaf(x, Main.tile[x, y], Main.tile[x, num], ref treeHeight, out treeFrame, out passStyle);
				if (Main.netMode == 2)
				{
					NetMessage.SendData(MessageID.SpecialFX, -1, -1, null, 1, x, y, 1f, passStyle);
				}
				if (Main.netMode == 0)
				{
					WorldGen.TreeGrowFX(x, y, 1, passStyle, hitTree: true);
				}
			}
		}

        public override void Unload()
        {
			Glow = null;
		}

        public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("{$Mods.Polarities.NPCName.Gemfly}");

			Main.npcFrameCount[Type] = Main.npcFrameCount[NPCID.LightningBug] * 6;
			Main.npcCatchable[Type] = true;

			NPCID.Sets.CountsAsCritter[Type] = true;

			Polarities.customNPCBestiaryStars.Add(Type, 2);
		}

		public override void SetDefaults()
		{
			NPC.CloneDefaults(NPCID.Firefly);
			AIType = NPCID.LightningBug;
			NPC.scale = 1f;
			AnimationType = NPCID.LightningBug;

			DrawOffsetY = -4;

			NPC.catchItem = (short)CatchItem;
		}

		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
		{
			bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
                //spawn conditions
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Caverns,
				//flavor text
				new FlavorTextBestiaryInfoElement(Language.GetTextValue("Mods.Polarities.Bestiary.Gemfly"))
			});
		}

		float gigabatPreSpawnEffectTimer;

		public override void AI()
		{
			bool canWeSpawnGigabat = false;

			//ai[2] = -1 if naturally-spawned
			if (NPC.ai[2] != -1)
			{
				if ((Main.LocalPlayer.ZoneRockLayerHeight || Main.LocalPlayer.ZoneDirtLayerHeight) && Main.LocalPlayer.Distance(NPC.Center) < 1000 && !NPC.AnyNPCs(NPCType<NPCs.Gigabat.Gigabat>()))
				{
					canWeSpawnGigabat = true;
					if (Main.rand.NextBool(ExpectedTime) && Main.netMode != 1)
					{
						NPC.SpawnOnPlayer(Main.myPlayer, NPCType<NPCs.Gigabat.Gigabat>());
						SoundEngine.PlaySound(new SoundStyle("Terraria/Sounds/NPC_Killed_4")
						{
							Volume = 2f,
							Pitch = -8f
						}, Main.LocalPlayer.position);
					}
				}

				if (canWeSpawnGigabat && !PolaritiesSystem.ranGemflyAmbience)
                {
					PolaritiesSystem.ranGemflyAmbience = true;
					if (NPC.soundDelay <= 0)
                    {
						if (!Main.rand.NextBool(3) || NPC.ai[2] == 0)
                        {
							NPC.ai[2] = 1;
							NPC.soundDelay = Main.rand.Next(90, 360);
                        }
						else
						{
							NPC.soundDelay = 30;
						}
                    }
					else if (NPC.soundDelay == 1)
                    {
						//spooky wingbeat
						SoundEngine.PlaySound(SoundID.Item32, Main.LocalPlayer.Center);
					}
                }

				if (canWeSpawnGigabat || gigabatPreSpawnEffectTimer > 0)
				{
					gigabatPreSpawnEffectTimer++;
					if (gigabatPreSpawnEffectTimer >= 60)
					{
						gigabatPreSpawnEffectTimer = 0;
					}
				}
			}
		}

		public override void FindFrame(int frameHeight)
		{
			NPC.frame.Y += frameHeight * Index * Main.npcFrameCount[NPCID.LightningBug];
		}

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (gigabatPreSpawnEffectTimer > 0)
            {
				spriteBatch.Draw(Textures.Shockwave72.Value, NPC.Center - screenPos, Textures.Shockwave72.Frame(), Color.White * (1 - gigabatPreSpawnEffectTimer / 60f), 0f, Textures.Shockwave72.Size() / 2, gigabatPreSpawnEffectTimer / 60f, SpriteEffects.None, 0f);
            }
			return true;
        }

        public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			Vector2 drawOrigin = new Vector2(7, 5);
			Vector2 drawPos = NPC.Center - screenPos + new Vector2(0, -NPC.scale * 9 + 9);
			if (NPC.localAI[2] > 3f)
			{
				spriteBatch.Draw(Glow.Value, drawPos, new Rectangle(0, 10 * Index, 14, 10), Color.White, NPC.rotation, drawOrigin, NPC.scale, NPC.spriteDirection == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0f);
			}
		}
	}

	public abstract class GemflyItem : ModItem
	{
		public abstract int Bait { get; }
		public abstract int GemItem { get; }
		public abstract int MakeNPC { get; }

        public override void SetStaticDefaults()
		{
			SacrificeTotal = (5);

			DisplayName.SetDefault(Lang.GetItemNameValue(GemItem) + "{$Mods.Polarities.ItemName.GemflyItem}");
			Tooltip.SetDefault("{$Mods.Polarities.ItemTooltip.GemflyItem}");
		}

		public override void SetDefaults()
		{
			Item.CloneDefaults(ItemID.Firefly);
			Item.bait = Bait;
			Item.rare = 1;
			Item.makeNPC = (short)MakeNPC;
		}

        public override void AddRecipes()
		{
			CreateRecipe()
				.AddIngredient(ItemID.Firefly)
				.AddIngredient(GemItem)
				.Register();
		}
	}

	public class DiamondGemfly : Gemfly { public override int Index => 0; public override int CatchItem => ItemType<DiamondGemflyItem>(); }
	public class DiamondGemflyItem : GemflyItem { public override int Bait => 44; public override int GemItem => ItemID.Diamond; public override int MakeNPC => NPCType<DiamondGemfly>(); }

	public class RubyGemfly : Gemfly { public override int Index => 1; public override int CatchItem => ItemType<RubyGemflyItem>(); }
	public class RubyGemflyItem : GemflyItem { public override int Bait => 40; public override int GemItem => ItemID.Ruby; public override int MakeNPC => NPCType<RubyGemfly>(); }

	public class EmeraldGemfly : Gemfly { public override int Index => 2; public override int CatchItem => ItemType<EmeraldGemflyItem>(); }
	public class EmeraldGemflyItem : GemflyItem { public override int Bait => 36; public override int GemItem => ItemID.Emerald; public override int MakeNPC => NPCType<EmeraldGemfly>(); }

	public class SapphireGemfly : Gemfly { public override int Index => 3; public override int CatchItem => ItemType<SapphireGemflyItem>(); }
	public class SapphireGemflyItem : GemflyItem { public override int Bait => 32; public override int GemItem => ItemID.Sapphire; public override int MakeNPC => NPCType<SapphireGemfly>(); }

	public class TopazGemfly : Gemfly { public override int Index => 4; public override int CatchItem => ItemType<TopazGemflyItem>(); }
	public class TopazGemflyItem : GemflyItem { public override int Bait => 28; public override int GemItem => ItemID.Topaz; public override int MakeNPC => NPCType<TopazGemfly>(); }

	public class AmethystGemfly : Gemfly { public override int Index => 5; public override int CatchItem => ItemType<AmethystGemflyItem>(); }
	public class AmethystGemflyItem : GemflyItem { public override int Bait => 24; public override int GemItem => ItemID.Amethyst; public override int MakeNPC => NPCType<AmethystGemfly>(); }
	
	public class AmberGemfly : Gemfly { public override int Index => 6; public override int ExpectedTime => 3600; public override int CatchItem => ItemType<AmberGemflyItem>(); }
	public class AmberGemflyItem : GemflyItem { public override int Bait => 40; public override int GemItem => ItemID.Amber; public override int MakeNPC => NPCType<AmberGemfly>(); }
}

