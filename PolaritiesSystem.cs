using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Graphics.Shaders;
using static Terraria.ModLoader.ModContent;
using Terraria.GameInput;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using System.Collections.Generic;
using Polarities.Items;
using Polarities.NPCs;
using MonoMod.Cil;
using Terraria.ModLoader.IO;
using Terraria.Enums;
using Terraria.Utilities;
using Terraria.WorldBuilding;
using Terraria.GameContent.Generation;
using Terraria.IO;
using Polarities.Items.Placeable.Blocks;
using Polarities.Items.Placeable.Walls;
using Polarities.Biomes;
using Terraria.GameContent;
using System.Reflection;
using Mono.Cecil.Cil;
using Terraria.Localization;
using Polarities.NPCs.ConvectiveWanderer;
using Polarities.Items.Placeable.Furniture.Salt;
using Terraria.GameContent.Events;
using Polarities.NPCs.TownNPCs;

namespace Polarities
{
	public class PolaritiesSystem : ModSystem
	{
        public override void Load()
		{
			//prevent random clutter in limestone/salt caves
			On.Terraria.WorldGen.PlaceTight += WorldGen_PlaceTight;
            On.Terraria.WorldGen.PlaceSmallPile += WorldGen_PlaceSmallPile;

			//for disabling world evil spread
			IL.Terraria.WorldGen.hardUpdateWorld += WorldGen_hardUpdateWorld;
			IL.Terraria.WorldGen.SpreadDesertWalls += WorldGen_SpreadDesertWalls;
		}

        private bool WorldGen_PlaceSmallPile(On.Terraria.WorldGen.orig_PlaceSmallPile orig, int i, int j, int X, int Y, ushort type)
        {
			if (Main.tile[i, j + 1].TileType == TileType<SaltTile>() || Main.tile[i, j + 1].TileType == TileType<RockSaltTile>() || Main.tile[i, j + 1].TileType == TileType<LimestoneTile>()) return false;
			return orig(i, j, X, Y, type);
		}

        private void WorldGen_PlaceTight(On.Terraria.WorldGen.orig_PlaceTight orig, int x, int y, bool spiders)
        {
			if (Main.tile[x, y - 1].TileType == TileType<SaltTile>() || Main.tile[x, y - 1].TileType == TileType<RockSaltTile>() || Main.tile[x, y - 1].TileType == TileType<LimestoneTile>()) return;
			orig(x, y, spiders);
        }

		private void WorldGen_SpreadDesertWalls(ILContext il)
		{
			var c = new ILCursor(il);

			ILLabel label = null;

			if (!c.TryGotoNext(MoveType.Before,
				i => i.MatchLdloc(0),
				i => i.MatchBrtrue(out label),
				i => i.MatchRet()
				))
			{
				GetInstance<Polarities>().Logger.Debug("Failed to find patch location");
				return;
			}

			c.Index++;

			c.EmitDelegate<Func<int, bool>>((num) => {
				if (num == 0)
				{
					return false;
				}
				else if (num == 1 && disabledEvilSpread)
				{
					//corruption
					return false;
				}
				else if (num == 3 && disabledEvilSpread)
				{
					//crimson
					return false;
				}
				else if (num == 2 && disabledHallowSpread)
				{
					//hallow
					return false;
				}
				return true;
			});
		}

		private void WorldGen_hardUpdateWorld(ILContext il)
		{
			var c = new ILCursor(il);

			ILLabel label = null;

			if (!c.TryGotoNext(MoveType.Before,
				i => i.MatchLdsfld(typeof(NPC).GetField("downedPlantBoss", BindingFlags.Public | BindingFlags.Static)),
				i => i.MatchBrfalse(out label)
				))
			{
				GetInstance<Polarities>().Logger.Debug("Failed to find patch location");
				return;
			}

			c.Index++;

			//skip over the plantera corruption spread slowdown
			//we need to let the get plantera downed instruction run because it's jumped to from elsewhere, so take it as pointless input here and run it again at the end
			c.Emit(OpCodes.Ldloc_0);
			c.EmitDelegate<Func<bool, int, bool>>((downedPlant, type) => {
				//don't spread (return true) if spread for the type is disabled
				if (disabledEvilSpread && (type == 23 || type == 25 || type == 32 || type == 112 || type == 163 || type == 400 || type == 398))
				{
					return true;
				}
				else if (disabledEvilSpread && (type == 199 || type == 200 || type == 201 || type == 203 || type == 205 || type == 234 || type == 352 || type == 401 || type == 399))
				{
					return true;
				}
				else if (disabledHallowSpread && (type == 109 || type == 110 || type == 113 || type == 115 || type == 116 || type == 117 || type == 164 || type == 402 || type == 403))
				{
					return true;
				}
				else
				{
					return false;
				}
			});
			//skip over slowdown if false
			c.Emit(OpCodes.Brfalse_S, label);
			//else ret
			c.Emit(OpCodes.Ret);
			c.Emit(OpCodes.Ldsfld, typeof(NPC).GetField("downedPlantBoss", BindingFlags.Public | BindingFlags.Static));
		}

		public static bool downedStormCloudfish;
		public static bool downedStarConstruct;
		public static bool downedGigabat;
		public static bool downedRiftDenizen;
		public static bool downedSunPixie;
		public static bool downedEsophage;
		public static bool downedConvectiveWanderer;
		public static bool downedSelfsimilarSentinel;
		public static bool downedEclipxie;
		public static bool downedHemorrphage;
		public static bool downedPolarities;

		public static bool downedEaterOfWorlds;
		public static bool downedBrainOfCthulhu;

		public static bool hallowInvasion;
		public static bool downedHallowInvasion;
		public static int sunPixieSpawnTimer;
		public static int hallowInvasionSize;
		public static int hallowInvasionSizeStart;

		public static bool worldEvilInvasion;
		public static bool downedWorldEvilInvasion;
		public static int esophageSpawnTimer;
		public static int worldEvilInvasionSize;
		public static int worldEvilInvasionSizeStart;

		public static bool disabledEvilSpread;
		public static bool disabledHallowSpread;

		public static int convectiveWandererSpawnTimer;

        public static int timer;

		public override void OnWorldLoad()
		{
			downedStormCloudfish = false;
			downedStarConstruct = false;
			downedGigabat = false;
			downedRiftDenizen = false;
			downedSunPixie = false;
			downedEsophage = false;
			downedSelfsimilarSentinel = false;
			downedConvectiveWanderer = false;
			downedEclipxie = false;
			downedHemorrphage = false;
			downedPolarities = false;

			downedEaterOfWorlds = false;
			downedBrainOfCthulhu = false;

			hallowInvasion = false;
			downedHallowInvasion = false;
			sunPixieSpawnTimer = 0;
			hallowInvasionSize = 0;
			hallowInvasionSizeStart = 0;

			worldEvilInvasion = false;
			downedWorldEvilInvasion = false;
			esophageSpawnTimer = 0;
			worldEvilInvasionSize = 0;
			worldEvilInvasionSizeStart = 0;

			disabledEvilSpread = false;
			disabledHallowSpread = false;

			convectiveWandererSpawnTimer = 0;
        }

        public override void OnWorldUnload()
		{
			downedStormCloudfish = false;
			downedStarConstruct = false;
			downedGigabat = false;
			downedRiftDenizen = false;
			downedSunPixie = false;
			downedEsophage = false;
			downedConvectiveWanderer = false;
			downedSelfsimilarSentinel = false;
			downedEclipxie = false;
			downedHemorrphage = false;
			downedPolarities = false;

			downedEaterOfWorlds = false;
			downedBrainOfCthulhu = false;

			hallowInvasion = false;
			downedHallowInvasion = false;
			sunPixieSpawnTimer = 0;
			hallowInvasionSize = 0;
			hallowInvasionSizeStart = 0;

			worldEvilInvasion = false;
			downedWorldEvilInvasion = false;
			esophageSpawnTimer = 0;
			worldEvilInvasionSize = 0;
			worldEvilInvasionSizeStart = 0;

			disabledEvilSpread = false;
			disabledHallowSpread = false;

			convectiveWandererSpawnTimer = 0;
        }

        public override void SaveWorldData(TagCompound tag)
		{
            if (downedStormCloudfish) tag["downedStormCloudfish"] = true;
            if (downedStarConstruct) tag["downedStarConstruct"] = true;
			if (downedGigabat) tag["downedGigabat"] = true;
			if (downedRiftDenizen) tag["downedRiftDenizen"] = true;
			if (downedSunPixie) tag["downedSunPixie"] = true;
			if (downedEsophage) tag["downedEsophage"] = true;
			if (downedConvectiveWanderer) tag["downedConvectiveWanderer"] = true;
			if (downedSelfsimilarSentinel) tag["downedSelfsimilarSentinel"] = true;
			if (downedEclipxie) tag["downedEclipxie"] = true;
			if (downedHemorrphage) tag["downedHemorrphage"] = true;
			if (downedPolarities) tag["downedPolarities"] = true;

			if (downedEaterOfWorlds) tag["downedEaterOfWorlds"] = true;
			if (downedBrainOfCthulhu) tag["downedBrainOfCthulhu"] = true;

			if (hallowInvasion) tag["hallowInvasion"] = true;
			if (downedHallowInvasion) tag["downedHallowInvasion"] = true;
			tag["hallowInvasionSize"] = hallowInvasionSize;
			tag["hallowInvasionSizeStart"] = hallowInvasionSizeStart;

			if (worldEvilInvasion) tag["worldEvilInvasion"] = true;
			if (downedWorldEvilInvasion) tag["downedWorldEvilInvasion"] = true;
			tag["worldEvilInvasionSize"] = worldEvilInvasionSize;
			tag["worldEvilInvasionSizeStart"] = worldEvilInvasionSizeStart;

			if (disabledEvilSpread) tag["disabledEvilSpread"] = true;
			if (disabledHallowSpread) tag["disabledHallowSpread"] = true;
		}

        public override void LoadWorldData(TagCompound tag)
        {
			downedStormCloudfish = tag.ContainsKey("downedStormCloudfish");
			downedStarConstruct = tag.ContainsKey("downedStarConstruct");
			downedGigabat = tag.ContainsKey("downedGigabat");
			downedRiftDenizen = tag.ContainsKey("downedRiftDenizen");
			downedSunPixie = tag.ContainsKey("downedSunPixie");
			downedEsophage = tag.ContainsKey("downedEsophage");
			downedConvectiveWanderer = tag.ContainsKey("downedConvectiveWanderer");
			downedSelfsimilarSentinel = tag.ContainsKey("downedSelfsimilarSentinel");
			downedEclipxie = tag.ContainsKey("downedEclipxie");
			downedHemorrphage = tag.ContainsKey("downedHemorrphage");
			downedPolarities = tag.ContainsKey("downedPolarities");

			downedEaterOfWorlds = tag.ContainsKey("downedEaterOfWorlds");
			downedBrainOfCthulhu = tag.ContainsKey("downedBrainOfCthulhu");

			hallowInvasion = tag.ContainsKey("hallowInvasion");
			downedHallowInvasion = tag.ContainsKey("downedHallowInvasion");
			hallowInvasionSize = tag.ContainsKey("hallowInvasionSize") ? tag.GetAsInt("hallowInvasionSize") : 0;
			hallowInvasionSizeStart = tag.ContainsKey("hallowInvasionSizeStart") ? tag.GetAsInt("hallowInvasionSizeStart") : 0;

			worldEvilInvasion = tag.ContainsKey("worldEvilInvasion");
			downedWorldEvilInvasion = tag.ContainsKey("downedWorldEvilInvasion");
			worldEvilInvasionSize = tag.ContainsKey("worldEvilInvasionSize") ? tag.GetAsInt("worldEvilInvasionSize") : 0;
			worldEvilInvasionSizeStart = tag.ContainsKey("worldEvilInvasionSizeStart") ? tag.GetAsInt("worldEvilInvasionSizeStart") : 0;

			disabledHallowSpread = tag.ContainsKey("disabledHallowSpread");
			disabledEvilSpread = tag.ContainsKey("disabledEvilSpread");
		}

        public override void ModifyWorldGenTasks(List<GenPass> tasks, ref float totalWeight)
		{
			int skyChestIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Floating Island Houses"));
			if (skyChestIndex != -1)
			{
				tasks.Insert(skyChestIndex + 1, new PassLegacy("More Sky Island Loot", AddSkyIslandLoot));
			}

			int graniteAndMarbleIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Granite"));
			if (graniteAndMarbleIndex != -1)
			{
				tasks.Insert(graniteAndMarbleIndex + 1, new PassLegacy("Limestone Caves", GenLimestoneCaves));
				tasks.Insert(graniteAndMarbleIndex + 2, new PassLegacy("Salt Caves", GenSaltCaves));
			}

            int potsIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Pots"));
            if (potsIndex != -1)
            {
                tasks.Insert(potsIndex + 1, new PassLegacy("More Pots", PolaritiesPots));
            }

            int settleLiquidsIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Settle Liquids"));
			if (settleLiquidsIndex != -1)
			{
				tasks.Insert(settleLiquidsIndex + 1, new PassLegacy("Salt Cave Obsidian Removal", SaltCavesRemoveObsidian));
			}

			int settleLiquidsAgainIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Settle Liquids Again"));
			if (settleLiquidsAgainIndex != -1)
			{
				tasks.Insert(settleLiquidsAgainIndex + 1, new PassLegacy("Salt Cave Lava Replacement Again", SaltCavesLavaReplace));
			}

			int finalCleanupIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Final Cleanup"));
			if (finalCleanupIndex != -1)
			{
				tasks.Insert(finalCleanupIndex + 1, new PassLegacy("Final Final Cleanup", FinalFinalCleanup));
			}

			int dungeonIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Dungeon"));
			if (dungeonIndex != -1)
			{
				tasks.Insert(dungeonIndex + 1, new PassLegacy("More Biome Chests", AddBiomeChests));
				tasks.Insert(dungeonIndex + 2, new PassLegacy("Making Hell More Hellish", CustomHellGeneration));
            }

            int guideIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Guide"));
            if (guideIndex != -1)
            {
                tasks.Insert(guideIndex + 1, new PassLegacy("More Friends", AddExtraNPCs));
            }
        }

		//spawn ghostwriter in celebrationmk10 worlds
		private void AddExtraNPCs(GenerationProgress progress, GameConfiguration configuration)
		{
			if (Main.tenthAnniversaryWorld)
			{
                Point adjustedFloorPosition = (Point)typeof(WorldGen).GetMethod("GetAdjustedFloorPosition", BindingFlags.NonPublic | BindingFlags.Static).Invoke(null, new object[] { Main.spawnTileX + 6, Main.spawnTileY });
				int num241 = NPC.NewNPC(new EntitySource_WorldGen(), adjustedFloorPosition.X * 16, adjustedFloorPosition.Y * 16, NPCType<Ghostwriter>());
				Main.npc[num241].homeTileX = adjustedFloorPosition.X;
				Main.npc[num241].homeTileY = adjustedFloorPosition.Y;
				Main.npc[num241].direction = -1;
				Main.npc[num241].homeless = true;
				BirthdayParty.CelebratingNPCs.Add(num241);
			}
        }

        private void AddBiomeChests(GenerationProgress progress, GameConfiguration configuration)
        {
            progress.Message = Language.GetTextValue("Mods.Polarities.WorldGenPass.AddBiomeChests");

            bool flag6 = false;
			if (WorldGen.drunkWorldGen)
			{
				while (!flag6)
				{
					int num79 = WorldGen.genRand.Next(0, Main.maxTilesX);
					int num78 = WorldGen.genRand.Next((int)Main.worldSurface, Main.maxTilesY);
					if (!Main.wallDungeon[Main.tile[num79, num78].WallType] || Main.tile[num79, num78].HasTile)
					{
						continue;
					}
					int contain = 0;
					int style2 = 0;
					if (WorldGen.crimson)
					{
						style2 = 24;
						contain = 1571;
					}
					else
					{
						style2 = 25;
						contain = 1569;
					}
					flag6 = WorldGen.AddBuriedChest(num79, num78, contain, notNearOtherChests: false, style2);
				}
			}
            /*TODO: flag6 = false;
            while (!flag6)
            {
                int i = WorldGen.genRand.Next(0, Main.maxTilesX);
                int j = WorldGen.genRand.Next((int)Main.worldSurface, Main.maxTilesY);
                if (!Main.wallDungeon[Main.tile[i, j].WallType] || Main.tile[i, j].HasTile)
                {
                    continue;
                }
                int contain = ItemType<Radial>();

                flag6 = AddBuriedModChest(i, j, contain, (ushort)TileType<Tiles.Furniture.FractalBiomeChest>(), notNearOtherChests: false, Style: 1);
            }*/
        }

        static bool NonReplaceableTileForBiomes(Tile tile)
        {
			int tileType = tile.TileType;
			int wallType = tile.WallType;
			return tileType == TileID.Granite || tileType == TileID.Marble || tileType == TileID.SnowBlock || tileType == TileID.IceBlock || tileType == TileID.BreakableIce || tileType == TileID.MushroomGrass || tileType == TileID.MushroomTrees || tileType == TileID.Sandstone || tileType == TileID.HardenedSand || wallType == WallID.Sandstone || wallType == WallID.HardenedSand
				|| tileType == TileType<RockSaltTile>() || tileType == TileType<SaltTile>() || tileType == TileType<LimestoneTile>()
				|| (tileType == TileID.JungleGrass && !WorldGen.notTheBees);
        }

		void AddSkyIslandLoot(GenerationProgress progress, GameConfiguration configuration)
		{
            progress.Message = Language.GetTextValue("Mods.Polarities.WorldGenPass.AddSkyIslandLoot");

            //add stellar remotes to sky island chests
            for (int chestIndex = 0; chestIndex < 1000; chestIndex++)
			{
				progress.Set(chestIndex / 1000f);

				Chest chest = Main.chest[chestIndex];
				if (chest != null)
				{
					if (Main.rand.NextBool(2) &&
						((!Main.getGoodWorld && Main.tile[chest.x, chest.y].TileType == TileID.Containers && Main.tile[chest.x, chest.y].TileFrameX == 13 * 36) ||
                        (Main.getGoodWorld && Main.tile[chest.x, chest.y].TileType == TileID.Containers && Main.tile[chest.x, chest.y].TileFrameX == 2 * 36 && chest.y < Main.worldSurface / 16)))
					{
						for (int inventoryIndex = 0; inventoryIndex < 40; inventoryIndex++)
						{
							if (chest.item[inventoryIndex].type == ItemID.None)
							{
								chest.item[inventoryIndex].SetDefaults(ItemType<StarConstructSummonItem>());
								break;
							}
						}
					}
					else if (Main.rand.NextBool(4) && Main.tile[chest.x, chest.y].TileType == TileID.Containers && Main.tile[chest.x, chest.y].TileFrameX == 0 * 36)
					{
						for (int inventoryIndex = 0; inventoryIndex < 40; inventoryIndex++)
						{
							if (chest.item[inventoryIndex].type == ItemID.None)
							{
								switch (Main.rand.Next(2))
								{
									case 0:
										chest.item[inventoryIndex].SetDefaults(ItemType<StormCloudfishSummonItem>());
										break;
									case 1:
										chest.item[inventoryIndex].SetDefaults(ItemID.SlimeCrown);
										break;
								}
								break;
							}
						}
					}
				}
            }
		}

		//TODO: Make salt and limestone generate ambience that doesn't override things like heart crystals but does override generic ambience, a la the granite/marble biomes
		//TODO: Maybe do submerged salt crates to incentivize a bit more diving and to hint at salt fishing
		private void GenSaltCaves(GenerationProgress progress, GameConfiguration configuration)
		{
            progress.Message = Language.GetTextValue("Mods.Polarities.WorldGenPass.GenSaltCaves");

            int saltTile = WorldGen.drunkWorldGen ? TileType<LimestoneTile>() : TileType<SaltTile>();
			int rockSaltTile = WorldGen.drunkWorldGen ? TileType<LimestoneTile>() : TileType<RockSaltTile>();
			int rockSaltWall = WorldGen.drunkWorldGen ? WallType<LimestoneWallNatural>() : WallType<RockSaltWallNatural>();

			int ambient1 = !WorldGen.drunkWorldGen ? TileType<Tiles.AmbientTiles.SaltAmbientTile1>() : TileType<Tiles.AmbientTiles.LimestoneAmbientTile1>();
			int ambient2 = !WorldGen.drunkWorldGen ? TileType<Tiles.AmbientTiles.SaltAmbientTile2>() : TileType<Tiles.AmbientTiles.LimestoneAmbientTile2>();
			int ambient3 = !WorldGen.drunkWorldGen ? TileType<Tiles.AmbientTiles.SaltAmbientTile3>() : TileType<Tiles.AmbientTiles.LimestoneAmbientTile3>();
			int ambient4 = !WorldGen.drunkWorldGen ? TileType<Tiles.AmbientTiles.SaltAmbientTile4>() : TileType<Tiles.AmbientTiles.LimestoneAmbientTile4>();
			int ambient5 = !WorldGen.drunkWorldGen ? TileType<Tiles.AmbientTiles.SaltAmbientTile5>() : TileType<Tiles.AmbientTiles.LimestoneAmbientTile5>();

			float totalCaves = (WorldGen.getGoodWorldGen ? 3 : 2) * Main.maxTilesY / 600;

			int numCavesGenerated = 0;
			for (int tries = 0; tries < 10000; tries++)
			{
				//Generate a salt cavern
				int left = WorldGen.genRand.Next(Main.maxTilesX);
				int top = (int)Main.rockLayer + WorldGen.genRand.Next(Main.maxTilesY - 300 - (int)Main.rockLayer);

				int halfWidth = 160 + WorldGen.genRand.Next(40);
				int halfHeight = 40 + WorldGen.genRand.Next(20);

				float rNoiseOffset = WorldGen.genRand.NextFloat() * 1000000;
				float noiseOffset = WorldGen.genRand.NextFloat() * 1000000;

				bool areaClear = true;

				for (int x = left - halfWidth - 20; x < left + halfWidth + 20; x++)
				{
					for (int y = top - halfHeight - 20; y < top + halfHeight + 20; y++)
					{
						if (x >= 0 && x < Main.maxTilesX && y >= 0 && y < Main.maxTilesY)
						{
							Tile tile = Framing.GetTileSafely(x, y);
							if (NonReplaceableTileForBiomes(tile))
							{
								areaClear = false;
								break;
							}
						}
					}
					if (!areaClear)
					{
						break;
					}
				}
				if (areaClear)
				{
					for (int j = -halfWidth - 20; j < halfWidth + 20; j++)
					{
						for (int k = -halfHeight - 20; k < halfHeight + 20; k++)
						{
							float angle = (new Vector2(((float)j / halfWidth), ((float)k / halfHeight))).ToRotation();
							float rNoise = (float)(Math.Sin(rNoiseOffset + 20 * angle) + Math.Sin(rNoiseOffset + 20 * 1.618 * angle)) / 20f;
							float r = rNoise + ((float)j / halfWidth) * ((float)j / halfWidth) + ((float)k / halfHeight) * ((float)k / halfHeight);

							int x = left + j;
							int y = top + k;

							if (r < 0.98 && x >= 0 && x < Main.maxTilesX && y >= 0 && y < Main.maxTilesY)
							{
								float val = ((float)k / halfHeight) / (r * r - 1);
								float noise = (float)(Math.Sin(4 * Math.Floor(noiseOffset + 5 * (float)j / halfWidth * 2)) + Math.Sin(4 * Math.Floor(noiseOffset + 5 * 1.618 * (float)j / halfWidth * 2))) / 10f;
								float id = val + noise;

								Tile tile = Framing.GetTileSafely(x, y);
								if (r < 0.96)
								{
									tile.WallType = (ushort)rockSaltWall;
									//tile.LiquidType = 0;
								}
								if (id < -0.7)
								{
									tile.TileType = (ushort)rockSaltTile;
									tile.HasTile = true;
								}
								else if (id < -0.05)
								{
									tile.TileType = (ushort)saltTile;
									tile.HasTile = true;
								}
								else if (id < -0.025)
								{
									//tile.LiquidType = 0;
									tile.HasTile = false;
									//drunk world limestone caves don't have water
									if (!Main.drunkWorld) tile.LiquidAmount = 255;
								}
								else if (id < 0.7)
								{
									tile.HasTile = false;
								}
								else
								{
									tile.TileType = (ushort)rockSaltTile;
									tile.HasTile = true;
								}
							}
						}
					}

					//generate waterfalls
					for (int j = -halfWidth - 20; j < 0; j++)
					{
						for (int k = -halfHeight - 20; k < halfHeight + 20; k++)
						{
							int x = left + j;
							int y = top + k;
							if (Main.rand.NextBool(8))
							{
								int tryDirection = WorldGen.genRand.NextBool() ? 1 : -1;
								if (Framing.GetTileSafely(x, y).HasTile && Framing.GetTileSafely(x, y).TileType == rockSaltTile &&
									Framing.GetTileSafely(x, y + 1).HasTile && Framing.GetTileSafely(x, y + 1).TileType == rockSaltTile &&
									Framing.GetTileSafely(x + 1, y).HasTile && Framing.GetTileSafely(x + 1, y).TileType == rockSaltTile &&
									Framing.GetTileSafely(x - 1, y).HasTile && Framing.GetTileSafely(x - 1, y).TileType == rockSaltTile &&
									!Framing.GetTileSafely(x + tryDirection * 2, y).HasTile)
								{
									Framing.GetTileSafely(x, y).HasTile = false;
									Framing.GetTileSafely(x, y).LiquidType = 0;
									Framing.GetTileSafely(x, y).LiquidAmount = 255;
									Framing.GetTileSafely(x + tryDirection, y).IsHalfBlock = true;
								}
							}
						}
					}

					for (int j = -halfWidth - 20; j < halfWidth + 20; j++)
					{
						for (int k = -halfHeight - 20; k < halfHeight + 20; k++)
						{
							int x = left + j;
							int y = top + k;

							if (Main.rand.NextBool(4) && x >= 5 && x < Main.maxTilesX - 5 && y >= 5 && y < Main.maxTilesY - 5)

							{
								Tile tile = Framing.GetTileSafely(x, y);

								if (!tile.HasTile)
								{
									switch (Main.rand.Next(5))
									{
										case 0:
											if (
												!Framing.GetTileSafely(x + 1, y).HasTile &&
												Framing.GetTileSafely(x, y + 1).HasTile && (Framing.GetTileSafely(x, y + 1).TileType == rockSaltTile || Framing.GetTileSafely(x, y + 1).TileType == saltTile) &&
												Framing.GetTileSafely(x + 1, y + 1).HasTile && (Framing.GetTileSafely(x + 1, y + 1).TileType == rockSaltTile || Framing.GetTileSafely(x + 1, y + 1).TileType == saltTile)
											)
											{
												WorldGen.Place2x1(x, y, (ushort)ambient1, Main.rand.Next(3));
											}
											break;
										case 1:
											if (
												Framing.GetTileSafely(x, y + 1).HasTile && (Framing.GetTileSafely(x, y + 1).TileType == rockSaltTile || Framing.GetTileSafely(x, y + 1).TileType == saltTile)
											)
											{
												WorldGen.PlaceTile(x, y, (ushort)ambient2, mute: true, style: Main.rand.Next(3));
											}
											break;
										case 2:
											if (
												!Framing.GetTileSafely(x, y - 1).HasTile &&
												Framing.GetTileSafely(x, y + 1).HasTile && (Framing.GetTileSafely(x, y + 1).TileType == rockSaltTile || Framing.GetTileSafely(x, y + 1).TileType == saltTile)
											)
											{
												WorldGen.PlaceObject(x, y, (ushort)ambient3, mute: true, style: Main.rand.Next(2));
											}
											break;
										case 3:
											if (
												!Framing.GetTileSafely(x, y + 1).HasTile &&
												Framing.GetTileSafely(x, y - 1).HasTile && (Framing.GetTileSafely(x, y - 1).TileType == rockSaltTile || Framing.GetTileSafely(x, y - 1).TileType == saltTile)
											)
											{
												WorldGen.PlaceObject(x, y, (ushort)ambient4, mute: true, style: Main.rand.Next(2));
											}
											break;
										case 4:
											if (
												Framing.GetTileSafely(x, y - 1).HasTile && (Framing.GetTileSafely(x, y - 1).TileType == rockSaltTile || Framing.GetTileSafely(x, y - 1).TileType == saltTile)
											)
											{
												WorldGen.PlaceTile(x, y, (ushort)ambient5, mute: true, style: Main.rand.Next(2));
											}
											break;
									}
								}
							}
						}
					}

					numCavesGenerated++;
					progress.Set(numCavesGenerated / (float)totalCaves);
					if (numCavesGenerated >= totalCaves) break;
				}
			}
		}

		private void SaltCavesRemoveObsidian(GenerationProgress progress, GameConfiguration configuration)
		{
            progress.Message = Language.GetTextValue("Mods.Polarities.WorldGenPass.SaltCavesRemoveObsidian");

            for (int x = 0; x < Main.maxTilesX; x++)
			{
				for (int y = 0; y < Main.maxTilesY; y++)
				{
					Tile tile = Framing.GetTileSafely(x, y);
					if (tile.WallType == WallType<RockSaltWallNatural>())
					{
						if (tile.TileType == TileID.Obsidian) tile.HasTile = false;
					}
				}
			}
		}

		private void SaltCavesLavaReplace(GenerationProgress progress, GameConfiguration configuration)
        {
            progress.Message = Language.GetTextValue("Mods.Polarities.WorldGenPass.SaltCavesLavaReplace");

            for (int x = 0; x < Main.maxTilesX; x++)
            {
				for (int y = 0; y < Main.maxTilesY; y++)
                {
					Tile tile = Framing.GetTileSafely(x, y);
					if (tile.WallType == WallType<RockSaltWallNatural>())
                    {
						tile.LiquidType = 0;
						if (tile.TileType == TileID.Obsidian) tile.HasTile = false;
                    }
                }
            }
        }

		private void GenLimestoneCaves(GenerationProgress progress, GameConfiguration configuration)
		{
            progress.Message = Language.GetTextValue("Mods.Polarities.WorldGenPass.GenLimestoneCaves");

            int limestoneTile = WorldGen.drunkWorldGen ? TileType<SaltTile>() : TileType<LimestoneTile>();
			int limestoneWall = WorldGen.drunkWorldGen ? WallType<RockSaltWallNatural>() : WallType<LimestoneWallNatural>();

			int ambient1 = WorldGen.drunkWorldGen ? TileType<Tiles.AmbientTiles.SaltAmbientTile1>() : TileType<Tiles.AmbientTiles.LimestoneAmbientTile1>();
			int ambient2 = WorldGen.drunkWorldGen ? TileType<Tiles.AmbientTiles.SaltAmbientTile2>() : TileType<Tiles.AmbientTiles.LimestoneAmbientTile2>();
			int ambient3 = WorldGen.drunkWorldGen ? TileType<Tiles.AmbientTiles.SaltAmbientTile3>() : TileType<Tiles.AmbientTiles.LimestoneAmbientTile3>();
			int ambient4 = WorldGen.drunkWorldGen ? TileType<Tiles.AmbientTiles.SaltAmbientTile4>() : TileType<Tiles.AmbientTiles.LimestoneAmbientTile4>();
			int ambient5 = WorldGen.drunkWorldGen ? TileType<Tiles.AmbientTiles.SaltAmbientTile5>() : TileType<Tiles.AmbientTiles.LimestoneAmbientTile5>();

			float totalCaves = (WorldGen.getGoodWorldGen ? 3 : 2) * Main.maxTilesY / 600;

			WorldGen.genRand.NextBool();
			int numCavesGenerated = 0;
			for (int tries = 0; tries < 10000; tries++)
			{
				//Generate a limestone cavern
				//to do this, generate a bunch of points with weights scattered in an area
				//assign each tile a value based on sum of normal distributions to points or something
				//then do nothing for tiles with very low values, set to limestone for tiles with middling values, and set to air for tiles with high values
				//to choose points and weights, we will pick a center point, then repeatedly create 'branch' points to form a fractal tree-like structure

				const int maxPointsWithBranches = 32;
				const float startingWeight = 20f;
				const float weightDecay = 0.9f;
				const int maxSize = 300;

				Vector2[] points = new Vector2[maxPointsWithBranches * 3];
				float[] weights = new float[maxPointsWithBranches * 3];

				int whereAreWeInList = 0;

				points[0] = new Vector2(WorldGen.genRand.Next(Main.maxTilesX), (int)Main.rockLayer + WorldGen.genRand.Next(Main.maxTilesY - 200 - maxSize / 2 - (int)Main.rockLayer));
				weights[0] = startingWeight;

				bool areaClear = true;
				for (int x = Math.Max(0, (int)points[0].X - maxSize / 2); x < Math.Min(Main.maxTilesX, points[0].X + maxSize / 2); x++)
				{
					for (int y = Math.Max(0, (int)points[0].Y - maxSize / 2); y < Math.Min(Main.maxTilesY, points[0].Y + maxSize / 2); y++)
					{
						if (x >= 0 && x < Main.maxTilesX && y >= 0 && y < Main.maxTilesY)
						{
							if (NonReplaceableTileForBiomes(Framing.GetTileSafely(x, y)))
							{
								areaClear = false;
								break;
							}
						}
					}
					if (!areaClear)
					{
						break;
					}
				}

				if (areaClear)
				{
					//generates a set of weighted points that control the value function
					for (int pointIndex = 0; pointIndex < maxPointsWithBranches; pointIndex++)
					{
						for (int branchIndex = 0; branchIndex < WorldGen.genRand.Next(2, 4); branchIndex++)
						{
							whereAreWeInList++;

							Vector2 newOffset = new Vector2(weights[pointIndex], 0).RotatedBy(WorldGen.genRand.NextFloat(0, 2 * MathHelper.Pi));
							newOffset.X *= 2f;

							points[whereAreWeInList] = points[pointIndex] + newOffset;
							weights[whereAreWeInList] = weights[pointIndex] * weightDecay;
						}
					}

					float[,] values = new float[maxSize, maxSize];
					List<Point> edgePoints = new List<Point>();

					float[] noise1D = WorldGen.genRand.FractalNoise1D(maxSize, startFactor: 1); //noise for shelf variation

                    for (int y = 0; y < maxSize; y++)
					{
						//variation in value multiplier with height for shelf gen
						float rowValue = 0;
                        for (int index = 0; index <= whereAreWeInList; index++)
                        {
                            rowValue += weights[index] / weights[0] * (float)Math.Exp(-Math.Pow(weights[index], -2) * (y + (int)points[0].Y - maxSize / 2 - points[index].Y) * (y + (int)points[0].Y - maxSize / 2 - points[index].Y));
                        }

						float yVariation = (float)Math.Sin(y * 0.25f + noise1D[y] * 20f); //varying value between -1 and 1;
                        float valueMultiplier = (1 + 3 / rowValue + yVariation) / (2 + 3 / rowValue);

                        for (int x = 0; x < maxSize; x++)
						{
							values[x, y] = 0;

							//computes the value function at each point
							for (int index = 0; index <= whereAreWeInList; index++)
							{
								values[x, y] += weights[index] * (float)Math.Exp(-Math.Pow(weights[index], -2) * ((x + (int)points[0].X - maxSize / 2 - points[index].X) * (x + (int)points[0].X - maxSize / 2 - points[index].X) + (y + (int)points[0].Y - maxSize / 2 - points[index].Y) * (y + (int)points[0].Y - maxSize / 2 - points[index].Y)));
							}

							float valueWithoutModification = values[x, y];

							//modification to the value for shelf gen
							values[x, y] -= weights[0] / 5;
                            values[x, y] *= valueMultiplier;
                            values[x, y] += weights[0] / 5;

                            //adjusts the tile at this point based on the value function
                            if (x + (int)points[0].X - maxSize / 2 >= 0 && x + (int)points[0].X - maxSize / 2 < Main.maxTilesX &&
								y + (int)points[0].Y - maxSize / 2 >= 0 && y + (int)points[0].Y - maxSize / 2 < Main.maxTilesY)
							{
								Tile tile = Framing.GetTileSafely(x + (int)points[0].X - maxSize / 2, y + (int)points[0].Y - maxSize / 2);

								if (valueWithoutModification > 1.3f * weights[0] / 5)
								{
									//make walls
									tile.WallType = (ushort)limestoneWall;
									tile.Clear(TileDataType.Liquid);
                                }
                                if (values[x, y] > 1.3f * weights[0] / 5 && values[x, y] < 1.4f * weights[0] / 5 && WorldGen.genRand.NextBool(3))
                                {
                                    //add an edge point for hole/spike generation
                                    edgePoints.Add(new Point(x + (int)points[0].X - maxSize / 2, y + (int)points[0].Y - maxSize / 2));
                                }
                                if (values[x, y] > 2.5f * weights[0] / 5)
								{
									//make air
									tile.HasTile = false;
								}
								else if (values[x, y] > weights[0] / 5)
								{
									//make limestone
									tile.TileType = (ushort)limestoneTile;
									tile.HasTile = true;
									tile.Slope = SlopeType.Solid;
									tile.IsHalfBlock = false;
								}
								if (Main.drunkWorld && values[x, y] > 10f * weights[0] / 5)
                                {
									//water for drunk world
									//tile.LiquidType = 0;
									tile.LiquidAmount = 255;
                                }
							}
						}
					}

                    //generate holes in the walls
                    foreach (Point point in edgePoints)
					{
						if (Main.rand.NextBool(3))
						{
							int holeSize = WorldGen.genRand.Next(40, 64);
							List<Point> holePoints = new List<Point>();
							holePoints.Add(point);
							Framing.GetTileSafely(point).HasTile = false;

							int i = 0;
							while (i < holeSize)
							{
								Point holePoint = WorldGen.genRand.Next(holePoints);
								Point newPoint = new Point();
								switch (Main.drunkWorld ? (WorldGen.genRand.Next(3) + 2) % 4 : WorldGen.genRand.Next(4))
								{
									case 0:
										newPoint = new Point(holePoint.X, holePoint.Y + 1);
										break;
									case 1:
										newPoint = new Point(holePoint.X, holePoint.Y - 1);
										break;
									case 2:
										newPoint = new Point(holePoint.X + 1, holePoint.Y);
										break;
									case 3:
										newPoint = new Point(holePoint.X - 1, holePoint.Y);
										break;
								}
								if (!holePoints.Contains(newPoint) && newPoint.X >= 0 && newPoint.X < Main.maxTilesX && newPoint.Y >= 0 && newPoint.Y < Main.maxTilesY)
								{
									holePoints.Add(newPoint);

									Tile tile = Framing.GetTileSafely(newPoint);
									if (tile.TileType == limestoneTile)
										tile.HasTile = false;

									i++;
								}
							}
						}
					}

					//generate downwards spikes
					foreach (Point point in edgePoints)
					{
						if (Framing.GetTileSafely(point).HasTile)
						{
							int spikeSize = WorldGen.genRand.Next(30, 60);
							float spikeShapeIndex = WorldGen.genRand.NextFloat(0.05f, 0.15f);
							float spikeHorizontalSlant = WorldGen.genRand.NextFloat(0.45f, 0.55f);

							List<Point> spikePoints = new List<Point>();
							spikePoints.Add(point);

							int i = 0;
							while (i < spikeSize)
							{
								Point spikePoint = WorldGen.genRand.Next(spikePoints);
								Point newPoint = new Point();
								Point newPoint2 = new Point(spikePoint.X, spikePoint.Y + 1);

								if (WorldGen.genRand.NextFloat() > spikeShapeIndex)
								{
									newPoint = new Point(spikePoint.X, spikePoint.Y + 1);
								}
								else
								{
									if (WorldGen.genRand.NextFloat() > spikeHorizontalSlant)
									{
										newPoint = new Point(spikePoint.X - 1, spikePoint.Y);
									}
									else
									{
										newPoint = new Point(spikePoint.X + 1, spikePoint.Y);
									}
								}
								if (!spikePoints.Contains(newPoint) && newPoint.X >= 0 && newPoint.X < Main.maxTilesX && newPoint.Y >= 0 && newPoint.Y < Main.maxTilesY)
								{
									spikePoints.Add(newPoint);

									Tile tile = Framing.GetTileSafely(newPoint);

									if (tile.WallType == (ushort)limestoneWall)
									{
										tile.TileType = (ushort)limestoneTile;
										tile.HasTile = true;
										tile.Slope = SlopeType.Solid;
										tile.IsHalfBlock = false;
									}
									i++;
								}
								if (!spikePoints.Contains(newPoint2) && newPoint2.X >= 0 && newPoint2.X < Main.maxTilesX && newPoint2.Y >= 0 && newPoint2.Y < Main.maxTilesY)
								{
									spikePoints.Add(newPoint2);

									Tile tile = Framing.GetTileSafely(newPoint2);

									if (tile.WallType == (ushort)limestoneWall)
									{
										tile.TileType = (ushort)limestoneTile;
										tile.HasTile = true;
										tile.Slope = SlopeType.Solid;
										tile.IsHalfBlock = false;
									}
									i++;
								}
							}
						}
					}

					//generates upwards spikes
					foreach (Point point in edgePoints)
					{
						if (Framing.GetTileSafely(point).HasTile && Main.rand.NextBool())
						{
							int spikeSize = WorldGen.genRand.Next(30, 60);
							float spikeShapeIndex = WorldGen.genRand.NextFloat(0.15f, 0.3f);
							float spikeHorizontalSlant = WorldGen.genRand.NextFloat(0.4f, 0.6f);

							List<Point> spikePoints = new List<Point>();
							spikePoints.Add(point);

							int i = 0;
							while (i < spikeSize)
							{
								Point spikePoint = WorldGen.genRand.Next(spikePoints);
								Point newPoint = new Point();
								Point newPoint2 = new Point(spikePoint.X, spikePoint.Y - 1);

								if (WorldGen.genRand.NextFloat() > spikeShapeIndex)
								{
									newPoint = new Point(spikePoint.X, spikePoint.Y - 1);
								}
								else
								{
									if (WorldGen.genRand.NextFloat() > spikeHorizontalSlant)
									{
										newPoint = new Point(spikePoint.X - 1, spikePoint.Y);
									}
									else
									{
										newPoint = new Point(spikePoint.X + 1, spikePoint.Y);
									}
								}
								if (!spikePoints.Contains(newPoint) && newPoint.X >= 0 && newPoint.X < Main.maxTilesX && newPoint.Y >= 0 && newPoint.Y < Main.maxTilesY)
								{
									spikePoints.Add(newPoint);

									Tile tile = Framing.GetTileSafely(newPoint);

									if (tile.WallType == (ushort)limestoneWall)
									{
										tile.TileType = (ushort)limestoneTile;
										tile.HasTile = true;
										tile.Slope = SlopeType.Solid;
										tile.IsHalfBlock = false;
									}
									i++;
								}
								if (!spikePoints.Contains(newPoint2) && newPoint2.X >= 0 && newPoint2.X < Main.maxTilesX && newPoint2.Y >= 0 && newPoint2.Y < Main.maxTilesY)
								{
									spikePoints.Add(newPoint2);

									Tile tile = Framing.GetTileSafely(newPoint2);

									if (tile.WallType == (ushort)limestoneWall)
									{
										tile.TileType = (ushort)limestoneTile;
										tile.HasTile = true;
										tile.Slope = SlopeType.Solid;
										tile.IsHalfBlock = false;
									}
									i++;
								}
							}
						}
					}

					//generate ambient tiles
					for (int x = Math.Max(0, (int)points[0].X - maxSize / 2); x < Math.Min(Main.maxTilesX, points[0].X + maxSize / 2); x++)
					{
						for (int y = Math.Max(0, (int)points[0].Y - maxSize / 2); y < Math.Min(Main.maxTilesY, points[0].Y + maxSize / 2); y++)
						{
							if (Main.rand.NextBool(3) && x >= 5 && x < Main.maxTilesX - 5 && y >= 5 && y < Main.maxTilesY - 5)
							{
								Tile tile = Framing.GetTileSafely(x, y);

								if (!tile.HasTile)
								{
									switch (Main.rand.Next(5))
									{
										case 0:
											if (
												!Framing.GetTileSafely(x + 1, y).HasTile &&
												Framing.GetTileSafely(x, y + 1).HasTile && (Framing.GetTileSafely(x, y + 1).TileType == limestoneTile) &&
												Framing.GetTileSafely(x + 1, y + 1).HasTile && (Framing.GetTileSafely(x + 1, y + 1).TileType == limestoneTile)
											)
											{
												WorldGen.Place2x1(x, y, (ushort)ambient1, Main.rand.Next(3));
											}
											break;
										case 1:
											if (
												Framing.GetTileSafely(x, y + 1).HasTile && (Framing.GetTileSafely(x, y + 1).TileType == limestoneTile)
											)
											{
												WorldGen.PlaceTile(x, y, (ushort)ambient2, mute: true, style: Main.rand.Next(3));
											}
											break;
										case 2:
											if (
												!Framing.GetTileSafely(x, y - 1).HasTile &&
												Framing.GetTileSafely(x, y + 1).HasTile && (Framing.GetTileSafely(x, y + 1).TileType == limestoneTile)
											)
											{
												WorldGen.PlaceObject(x, y, (ushort)ambient3, mute: true, style: Main.rand.Next(2));
											}
											break;
										case 3:
											if (
												!Framing.GetTileSafely(x, y + 1).HasTile &&
												Framing.GetTileSafely(x, y - 1).HasTile && (Framing.GetTileSafely(x, y - 1).TileType == limestoneTile)
											)
											{
												WorldGen.PlaceObject(x, y, (ushort)ambient4, mute: true, style: Main.rand.Next(2));
											}
											break;
										case 4:
											if (
												Framing.GetTileSafely(x, y - 1).HasTile && (Framing.GetTileSafely(x, y - 1).TileType == limestoneTile)
											)
											{
												WorldGen.PlaceTile(x, y, (ushort)ambient5, mute: true, style: Main.rand.Next(2));
											}
											break;
									}
								}
							}
						}
					}

					numCavesGenerated++;
					progress.Set(numCavesGenerated / (float)totalCaves);
					if (numCavesGenerated >= totalCaves) break;
				}
			}

			if (Main.drunkWorld)
			{
				for (int k = 0; k < Main.maxTilesX; k++)
				{
					for (int l = 0; l < Main.maxTilesY; l++)
					{
						if (Main.tile[k, l].HasTile && (!WorldGen.SolidTile(k, l + 1) || !WorldGen.SolidTile(k, l + 2)))
						{
							if (Main.tile[k, l].TileType == TileType<SaltTile>())
							{
								Main.tile[k, l].TileType = (ushort)TileType<RockSaltTile>();
							}
						}
					}
				}
			}
		}

		private void CustomHellGeneration(GenerationProgress progress, GameConfiguration configuration)
		{
            progress.Message = Language.GetTextValue("Mods.Polarities.WorldGenPass.CustomHellGeneration");

            int x;
			int y;

			int direction = WorldGen.dungeonX < Main.maxTilesX / 2 ? 1 : -1;
			int GetX()
			{
				if (WorldGen.drunkWorldGen)
                {
					return x + (int)(Main.maxTilesX * 0.27f);
                }

				if (direction == 1)
				{
					return x;
				}
				return Main.maxTilesX - 1 - x;
			}

			int maxLakeX = WorldGen.drunkWorldGen ? (int)(Main.maxTilesX * 0.46f) : (int)(Main.maxTilesX * 0.23f);
			int lakeSurfaceY = Main.maxTilesY - 130;

			Tile frameTile;

			//generate the general lake
			//random walk thing is semi-adapted from vanilla
			int walkHeight = Main.maxTilesY - WorldGen.genRand.Next(50, 70);
			for (x = 0; x < maxLakeX; x++)
			{
				walkHeight += WorldGen.genRand.Next(-3, 4);
				if (walkHeight < Main.maxTilesY - 70) walkHeight = Main.maxTilesY - 70;
				if (walkHeight > Main.maxTilesY - 50) walkHeight = Main.maxTilesY - 50;

				for (y = Main.maxTilesY - 150; y < Main.maxTilesY; y++)
				{
					frameTile = Framing.GetTileSafely(GetX(), y);

					if (y < lakeSurfaceY)
					{
						frameTile.HasTile = false;
						frameTile.LiquidAmount = 0;
					}
					else if (y < walkHeight)
					{
						frameTile.HasTile = false;
						frameTile.LiquidType = LiquidID.Lava;
						frameTile.LiquidAmount = 255;
					}
					else
					{
						WorldGen.PlaceTile(GetX(), y, TileID.Ash, mute: true, forced: true);
						Framing.GetTileSafely(GetX(), y).Slope = 0;
						Framing.GetTileSafely(GetX(), y).IsHalfBlock = false;
					}
				}

				//clear things above the start
				y = Main.maxTilesY - 151;
				frameTile = Framing.GetTileSafely(GetX(), y);
				while ((frameTile.HasTile || frameTile.LiquidAmount > 0) && y > Main.maxTilesY - 180)
				{
					frameTile.HasTile = false;
					frameTile.LiquidAmount = 0;
					y--;
					frameTile = Framing.GetTileSafely(GetX(), y);
				}
			}

			//Border transition with normal hell
			x = maxLakeX;
			y = Main.maxTilesY - 150;
			frameTile = Framing.GetTileSafely(GetX(), y);
			while (!frameTile.HasTile && y < Main.maxTilesY - 70)
			{
				y++;
				frameTile = Framing.GetTileSafely(GetX(), y);
			}
			int borderTransitionHeight = y;
			int borderTransitionCurrHeight = borderTransitionHeight;
			x--;
			while (borderTransitionCurrHeight > 0 && x > 0 && !Main.tile[GetX(), borderTransitionCurrHeight].HasTile)
			{
				for (y = borderTransitionCurrHeight; y < Main.maxTilesY; y++)
				{
					WorldGen.PlaceTile(GetX(), y, TileID.Ash, mute: true, forced: true);
				}
				x--;
				borderTransitionCurrHeight += WorldGen.genRand.Next(-2, 4);
				if (borderTransitionCurrHeight < borderTransitionHeight) borderTransitionCurrHeight = borderTransitionHeight;
			}
			x = maxLakeX;
			borderTransitionCurrHeight = borderTransitionHeight;
			while (borderTransitionCurrHeight > 0 && GetX() < Main.maxTilesX && GetX() >= 0)
			{
				for (y = borderTransitionCurrHeight; y < Main.maxTilesY; y++)
				{
					if (!Main.tile[GetX(), y].HasTile)
						WorldGen.PlaceTile(GetX(), y, TileID.Ash, mute: true, forced: true);
				}
				x++;
				borderTransitionCurrHeight += WorldGen.genRand.Next(-2, 7);
				if (borderTransitionCurrHeight < borderTransitionHeight) borderTransitionCurrHeight = borderTransitionHeight;
			}

			//special case for drunk worlds
			if (WorldGen.drunkWorldGen)
            {
                x = -1;
                y = Main.maxTilesY - 150;
                frameTile = Framing.GetTileSafely(GetX(), y);
                while (!frameTile.HasTile && y < Main.maxTilesY - 70)
                {
                    y++;
                    frameTile = Framing.GetTileSafely(GetX(), y);
                }
                borderTransitionHeight = y;
                borderTransitionCurrHeight = borderTransitionHeight;
                x++;
                while (borderTransitionCurrHeight > 0 && x < maxLakeX && !Main.tile[GetX(), borderTransitionCurrHeight].HasTile)
                {
                    for (y = borderTransitionCurrHeight; y < Main.maxTilesY; y++)
                    {
                        WorldGen.PlaceTile(GetX(), y, TileID.Ash, mute: true, forced: true);
                    }
                    x++;
                    borderTransitionCurrHeight += WorldGen.genRand.Next(-2, 4);
                    if (borderTransitionCurrHeight < borderTransitionHeight) borderTransitionCurrHeight = borderTransitionHeight;
                }
                x = -1;
                borderTransitionCurrHeight = borderTransitionHeight;
                while (borderTransitionCurrHeight > 0 && GetX() < Main.maxTilesX && GetX() >= 0)
                {
                    for (y = borderTransitionCurrHeight; y < Main.maxTilesY; y++)
                    {
                        if (!Main.tile[GetX(), y].HasTile)
                            WorldGen.PlaceTile(GetX(), y, TileID.Ash, mute: true, forced: true);
                    }
                    x--;
                    borderTransitionCurrHeight += WorldGen.genRand.Next(-2, 7);
                    if (borderTransitionCurrHeight < borderTransitionHeight) borderTransitionCurrHeight = borderTransitionHeight;
                }
            }

			//extend lava into normal hell until it hits something
			x = maxLakeX;
			y = lakeSurfaceY;
			while (!Main.tile[GetX(), y].HasTile)
			{
				while (y < Main.maxTilesY && !Main.tile[GetX(), y].HasTile && Main.tile[GetX(), y].LiquidAmount == 0)
				{
					frameTile = Framing.GetTileSafely(GetX(), y);
					frameTile.LiquidType = 1;
					frameTile.LiquidAmount = 255;
					y++;
				}

				y = lakeSurfaceY;
				x++;
			}

			//Special case for drunk worlds
			if (WorldGen.drunkWorldGen)
			{
                x = 0;
                y = lakeSurfaceY;
                while (!Main.tile[GetX(), y].HasTile)
                {
                    while (y < Main.maxTilesY && !Main.tile[GetX(), y].HasTile && Main.tile[GetX(), y].LiquidAmount == 0)
                    {
                        frameTile = Framing.GetTileSafely(GetX(), y);
                        frameTile.LiquidType = 1;
                        frameTile.LiquidAmount = 255;
                        y++;
                    }

                    y = lakeSurfaceY;
                    x--;
                }
            }

            //generate ash islands and fissures
            int lastIslandGenAt = 0;
			for (x = WorldGen.drunkWorldGen ? 100 : 0; x < maxLakeX - 100; x++)
			{
				int testWidth = WorldGen.genRand.Next(2, 25) * 2;

				if (x - lastIslandGenAt > testWidth / 2 + 4 && WorldGen.genRand.NextBool(20))
				{
					lastIslandGenAt = x;
					float verticalScale = WorldGen.genRand.NextFloat(0.75f, 1f) * testWidth / 20f;
					GenAshIslandAt(GetX(), lakeSurfaceY, testWidth, verticalScale);
					int fissureGenHeight = (int)(lakeSurfaceY + 24 * verticalScale + 1);

					GenLavaLakeFissureAt(GetX(), fissureGenHeight, testWidth * 0.25f + 5);

					x += testWidth / 2;
				}
			}

			//Re-generate lava pockets and hellstone
			//adapted from vanilla
			for (int num552 = 0; num552 < maxLakeX; num552++)
			{
				x = WorldGen.genRand.Next(20, maxLakeX - 20);
				WorldGen.TileRunner(GetX(), WorldGen.genRand.Next(Main.maxTilesY - 180, Main.maxTilesY - 10), WorldGen.genRand.Next(2, 7), WorldGen.genRand.Next(2, 7), -2);
			}
			for (int num554 = 0; num554 < (int)((double)(Main.maxTilesX * Main.maxTilesY) * 0.0008 * (maxLakeX / (double)Main.maxTilesX)); num554++)
			{
				x = WorldGen.genRand.Next(0, maxLakeX);
				WorldGen.TileRunner(GetX(), WorldGen.genRand.Next(Main.maxTilesY - 140, Main.maxTilesY), WorldGen.genRand.Next(2, 7), WorldGen.genRand.Next(3, 7), TileID.Hellstone);
			}
		}

		void GenAshIslandAt(int atX, int atY, int width, float verticalScale)
		{
			int halfWidth = width / 2;

			int x;
			int y;

			float upBaseHeight = 2.5f * Main.rand.NextFloat(0.75f, 1.33f) * verticalScale;
			float upMaxVariance = 0.6f * upBaseHeight;
			float upGeneralVariance = 2 * verticalScale;

			float downBaseHeight = 10 * Main.rand.NextFloat(0.75f, 1.33f) * verticalScale;
			float downMaxVariance = 0.8f * downBaseHeight;
			float downGeneralVariance = 4 * verticalScale;

			float walkHeightUp = upBaseHeight + WorldGen.genRand.NextFloat(-upMaxVariance, upMaxVariance + 1);
			float walkHeightDown = downBaseHeight + WorldGen.genRand.NextFloat(-downMaxVariance, downMaxVariance + 1);
			for (x = -halfWidth; x <= halfWidth; x++)
			{
				walkHeightUp += WorldGen.genRand.NextFloat(-upGeneralVariance, upGeneralVariance + 1);
				walkHeightUp = Utils.Clamp(walkHeightUp, upBaseHeight - upMaxVariance, upBaseHeight + upMaxVariance);

				walkHeightDown += WorldGen.genRand.NextFloat(-downGeneralVariance, downGeneralVariance + 1);
				walkHeightDown = Utils.Clamp(walkHeightDown, downBaseHeight - downMaxVariance, downBaseHeight + downMaxVariance);

				int trueHeightUp = (int)(walkHeightUp * (1 - (x / (float)halfWidth) * (x / (float)halfWidth)));
				int trueHeightDown = (int)(walkHeightDown * (1 - (x / (float)halfWidth) * (x / (float)halfWidth)));

				for (y = -trueHeightUp; y <= trueHeightDown; y++)
				{
					WorldGen.PlaceTile(x + atX, y + atY, TileID.Ash, mute: true, forced: true);
				}
			}
		}

		void GenLavaLakeFissureAt(int atX, int atY, float width)
		{
			int x;
			int y;

			float xOffset = 0;
			float levelWidth = width;
			for (y = atY; y < Main.maxTilesY - 1; y++)
			{
				for (x = (int)(atX + xOffset - levelWidth / 2); x <= atX + xOffset + levelWidth / 2; x++)
				{
					Tile frameTile = Framing.GetTileSafely(x, y);
					frameTile.HasTile = false;
					frameTile.LiquidType = LiquidID.Lava;
					frameTile.LiquidAmount = 255;
				}
				levelWidth += Main.rand.NextFloat(-2.5f, 2f);
				if (levelWidth > width + 2) levelWidth = width + 2;
				if (levelWidth < 2) levelWidth = 2;

				xOffset += Main.rand.NextFloat(-1f, 1f);
			}
		}

		private void FinalFinalCleanup(GenerationProgress progress, GameConfiguration configuration)
		{
            progress.Message = Language.GetTextValue("Mods.Polarities.WorldGenPass.FinalFinalCleanup");

            for (int k = 0; k < Main.maxTilesX; k++)
			{
				for (int l = 0; l < Main.maxTilesY; l++)
				{
					if (Main.tile[k, l].HasTile && (!WorldGen.SolidTile(k, l + 1) || !WorldGen.SolidTile(k, l + 2)))
					{
						if (Main.tile[k, l].TileType == TileType<SaltTile>())
						{
							Main.tile[k, l].TileType = (ushort)TileType<RockSaltTile>();
						}
					}
				}
			}

			//fill in the gaps in the lava ocean
			{
				int x;
				int y;

				int direction = WorldGen.dungeonX < Main.maxTilesX / 2 ? 1 : -1;
                int GetX()
                {
                    if (WorldGen.drunkWorldGen)
                    {
                        return x + (int)(Main.maxTilesX * 0.27f);
                    }

                    if (direction == 1)
                    {
                        return x;
                    }
                    return Main.maxTilesX - 1 - x;
                }

                int maxLakeX = WorldGen.drunkWorldGen ? (int)(Main.maxTilesX * 0.46f) : (int)(Main.maxTilesX * 0.23f);
                int lakeSurfaceY = Main.maxTilesY - 130;

				for (x = 0; x < maxLakeX; x++)
				{
					for (y = lakeSurfaceY - 20; y < Main.maxTilesY; y++)
					{
						Tile frameTile = Framing.GetTileSafely(GetX(), y);

						if (!frameTile.HasTile)
						{
							if (y >= lakeSurfaceY)
							{
								frameTile.LiquidType = LiquidID.Lava;
								frameTile.LiquidAmount = 255;
							}
							else
                            {
                                Tile aboveTile = Framing.GetTileSafely(GetX(), y - 1);
								if (!aboveTile.HasTile && aboveTile.LiquidAmount == 0)
								{
									frameTile.LiquidType = LiquidID.Water;
									frameTile.LiquidAmount = 0;
								}
                            }
						}
					}
				}
			}

            //TODO: progress.Message = "Generating subworlds (this can take a while!)";
        }

        private void PolaritiesPots(GenerationProgress progress, GameConfiguration configuration)
        {
            progress.Message = Language.GetTextValue("Mods.Polarities.WorldGenPass.PolaritiesPots");

            for (int i = 1; i < Main.maxTilesX - 2; i++)
            {
                for (int j = 1; j < Main.maxTilesY - 2; j++)
                {
                    progress.Set(i * j / (float)(Main.maxTilesX * Main.maxTilesY));
                    if (Main.tile[i, j].TileType == TileID.Pots && Main.tile[i, j + 1].TileType == TileID.Pots && Main.tile[i + 1, j].TileType == TileID.Pots && Main.tile[i + 1, j + 1].TileType == TileID.Pots)
                    {
                        if (Main.tile[i, j + 2].TileType == (ushort)TileType<SaltTile>() || Main.tile[i, j + 2].TileType == (ushort)TileType<RockSaltTile>())
                        {
                            WorldGen.KillTile(i, j);
                            WorldGen.KillTile(i, j + 1);
                            WorldGen.KillTile(i + 1, j);
                            WorldGen.KillTile(i + 1, j + 1);
                            WorldGen.PlacePot(i, j + 1, (ushort)TileType<SaltPotTile>(), 0);
                        }
                    }
                }
            }
        }

        public static void GenerateMantellarOre()
		{
			/*TODO: if (SLWorld.subworld)
				return;*/

			Main.NewText(Language.GetTextValue("Mods.Polarities.StatusMessage.GenMantellarOre"), 255, 200, 0);

			int x;
			int y;

			int direction = Main.dungeonX < Main.maxTilesX / 2 ? 1 : -1;
            int GetX()
            {
                if (Main.drunkWorld)
                {
                    return x + (int)(Main.maxTilesX * 0.27f);
                }

                if (direction == 1)
                {
                    return x;
                }
                return Main.maxTilesX - 1 - x;
            }

            int maxLakeX = WorldGen.drunkWorldGen ? (int)(Main.maxTilesX * 0.46f) : (int)(Main.maxTilesX * 0.23f);

            for (x = 0; x < maxLakeX; x++)
			{
				int startX = x;

				y = Main.maxTilesY - 2;
				if (!Main.tile[GetX(), y].HasTile && Main.tile[GetX(), y].LiquidType == LiquidID.Lava && Main.tile[GetX(), y].LiquidAmount == 255)
				{
					bool canContinue = true;

					int height = WorldGen.genRand.Next(60, 80) + WorldGen.genRand.Next(20);

					while (y > Main.maxTilesY - height && canContinue)
					{
						int walkDir = WorldGen.genRand.Next(2) * 2 - 1;

						x += walkDir;
						if (!Main.tile[GetX(), y - 1].HasTile && Main.tile[GetX(), y - 1].LiquidType == LiquidID.Lava && Main.tile[GetX(), y - 1].LiquidAmount == 255)
						{
							y--;
						}
						else
						{
							x -= 2 * walkDir;
							if (!Main.tile[GetX(), y - 1].HasTile && Main.tile[GetX(), y - 1].LiquidType == LiquidID.Lava && Main.tile[GetX(), y - 1].LiquidAmount == 255)
							{
								y--;
							}
							else
							{
								x += walkDir;
								if (!Main.tile[GetX(), y - 1].HasTile && Main.tile[GetX(), y - 1].LiquidType == LiquidID.Lava && Main.tile[GetX(), y - 1].LiquidAmount == 255)
								{
									y--;
								}
								else
								{
									canContinue = false;
								}
							}
						}
					}

					if (canContinue)
					{
						Tile tile = Framing.GetTileSafely(GetX(), y);
						if (tile.LiquidAmount > 0 && tile.LiquidType == LiquidID.Lava)
						{
							List<(int, int)> coords = new List<(int, int)>();
							(int, int) currentCoord = (GetX(), y);
							for (int i = 0; i < WorldGen.genRand.Next(10, 80); i++)
							{
								while (coords.Contains(currentCoord))
								{
									if (!WorldGen.genRand.NextBool(8))
									{
										currentCoord.Item2 += WorldGen.genRand.Next(2) * 2 - 1;
									}
									else
									{
										currentCoord.Item1 += WorldGen.genRand.Next(2) * 2 - 1;
									}
								}
								if (tile.LiquidAmount > 0 && tile.LiquidType == LiquidID.Lava)
								{
									WorldGen.PlaceTile(currentCoord.Item1, currentCoord.Item2, TileType<MantellarOreTile>());
								}
								coords.Add(currentCoord);
								currentCoord = (GetX(), y);
							}
						}
					}
				}

				//reset our x value to ensure the for loop continues as normal
				x = startX;
			}
		}

		public static bool timeAccelerate = true;
		float timeRateMultiplier;
		public override void ModifyTimeRate(ref double timeRate, ref double tileUpdateRate, ref double eventUpdateRate)
		{
			if (!timeAccelerate)
			{
				timeRateMultiplier = 1f;
			}
			else
            {
				timeRateMultiplier += 1 / 5f;
			}
			timeAccelerate = false;

			timeRate *= timeRateMultiplier;
		}

        public override void PreUpdateWorld()
		{
			LoopedSound.UpdateLoopedSounds();
		}

        public static bool ranGemflyAmbience;
		public override void PreUpdateNPCs()
        {
			ranGemflyAmbience = false;
		}

		public int saltBlockCount;
		public int limestoneBlockCount;

		public override void TileCountsAvailable(ReadOnlySpan<int> tileCounts)
		{
			saltBlockCount = tileCounts[TileType<SaltTile>()] + tileCounts[TileType<RockSaltTile>()];
			limestoneBlockCount = tileCounts[TileType<LimestoneTile>()];
		}

        public override void PostUpdateEverything()
		{
			if (NPC.downedMechBossAny && !downedHallowInvasion && Main.invasionType == 0 /*TODO: && !SLWorld.subworld*/ && Main.rand.NextBool(2 * 24 * 60 * 60))
			{
				HallowInvasion.StartInvasion();
			}
			if (NPC.downedMechBoss1 && NPC.downedMechBoss2 && NPC.downedMechBoss3 && !downedWorldEvilInvasion && Main.invasionType == 0 /*TODO: && !SLWorld.subworld*/ && Main.rand.NextBool(2 * 24 * 60 * 60))
			{
				WorldEvilInvasion.StartInvasion();
			}

			if (worldEvilInvasion)
			{
				WorldEvilInvasion.CheckInvasionProgress();
				WorldEvilInvasion.UpdateInvasion();
			}
			if (esophageSpawnTimer > 0)
			{
				WorldEvilInvasion.UpdateEsophageSpawning();
			}
			else if (esophageSpawnTimer < 0)
			{
				esophageSpawnTimer++;
			}

			if (convectiveWandererSpawnTimer > 0)
            {
				ConvectiveWanderer.UpdateConvectiveWandererSpawning();
            }

            if (hallowInvasion)
			{
				HallowInvasion.CheckInvasionProgress();
				HallowInvasion.UpdateInvasion();
			}
			if (sunPixieSpawnTimer > 0)
			{
				HallowInvasion.UpdateSunPixieSpawning();
			}
			else if (sunPixieSpawnTimer < 0)
			{
				sunPixieSpawnTimer++;
			}

			timer++;
		}

		public override void PostDrawInterface(SpriteBatch spriteBatch)
		{
			Player player = Main.LocalPlayer;

			Texture2D texture2D4 = TextureAssets.Extra[9].Value;
			string text7 = "";
			Color c = Color.White;

			bool isInvasion = false;

			if (worldEvilInvasion && (player.ZoneCorrupt || player.ZoneCrimson) && player.ZoneOverworldHeight)
			{
				texture2D4 = WorldEvilInvasion.EventIcon.Value;
				text7 = Language.GetTextValue("Mods.Polarities.BiomeName.WorldEvilInvasion");
				c = new Color(128, 0, 96);
				isInvasion = true;
			}
			else if (hallowInvasion && player.ZoneHallow && player.ZoneOverworldHeight)
			{
				texture2D4 = HallowInvasion.EventIcon.Value;
				text7 = Language.GetTextValue("Mods.Polarities.BiomeName.HallowInvasion");
				c = new Color(255, 96, 0);
				isInvasion = true;
			}

			//custom invasion icons
			if (Main.invasionProgress == -1)
			{
				return;
			}
			if (Main.invasionProgressMode == 2 && Main.invasionProgressNearInvasion && Main.invasionProgressDisplayLeft < 160)
			{
				Main.invasionProgressDisplayLeft = 160;
			}
			if (Main.invasionProgressAlpha <= 0f)
			{
				return;
			}
			float num18 = 0.5f + Main.invasionProgressAlpha * 0.5f;

			if (isInvasion)
			{
				Vector2 value = FontAssets.MouseText.Value.MeasureString(text7);
				float num13 = 120f;
				if (value.X > 200f)
				{
					num13 += value.X - 200f;
				}
				Rectangle r3 = Utils.CenteredRectangle(new Vector2((float)Main.screenWidth - num13, (float)(Main.screenHeight - 80)), (value + new Vector2((float)(texture2D4.Width + 12), 6f)) * num18);
				Utils.DrawInvBG(spriteBatch, r3, c);
				spriteBatch.Draw(texture2D4, r3.Left() + Vector2.UnitX * num18 * 8f, (Rectangle?)null, Color.White * Main.invasionProgressAlpha, 0f, new Vector2(0f, (float)(texture2D4.Height / 2)), num18 * 0.8f, (SpriteEffects)0, 0f);
				Utils.DrawBorderString(spriteBatch, text7, r3.Right() + Vector2.UnitX * num18 * -22f, Color.White * Main.invasionProgressAlpha, num18 * 0.9f, 1f, 0.4f);
			}
		}
	}
}

