using System;
using System.Reflection;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.Creative;
using Terraria.Localization;
using Terraria.ModLoader;
using MonoMod.RuntimeDetour.HookGen;
using Terraria.DataStructures;
using Terraria.Utilities;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.GameContent.ItemDropRules;
using Polarities.NPCs;
using Terraria.ID;
using Terraria.GameContent;

namespace Polarities
{
	public static class ModUtils
	{
		public static void Load()
		{
			try
			{
				_addSpecialPointSpecialPositions = typeof(Terraria.GameContent.Drawing.TileDrawing).GetField("_specialPositions", BindingFlags.NonPublic | BindingFlags.Instance);
				_addSpecialPointSpecialsCount = typeof(Terraria.GameContent.Drawing.TileDrawing).GetField("_specialsCount", BindingFlags.NonPublic | BindingFlags.Instance);

				_AddHappinessReportText = typeof(Terraria.GameContent.ShopHelper).GetMethod("AddHappinessReportText", BindingFlags.NonPublic | BindingFlags.Instance);

				_AI_007_FindGoodRestingSpot = typeof(NPC).GetMethod("AI_007_FindGoodRestingSpot", BindingFlags.NonPublic | BindingFlags.Instance);
				_AI_007_TownEntities_IsInAGoodRestingSpot = typeof(NPC).GetMethod("AI_007_TownEntities_IsInAGoodRestingSpot", BindingFlags.NonPublic | BindingFlags.Instance);
				_AI_007_TownEntities_TeleportToHome = typeof(NPC).GetMethod("AI_007_TownEntities_TeleportToHome", BindingFlags.NonPublic | BindingFlags.Instance);
				_AI_007_TownEntities_GetWalkPrediction = typeof(NPC).GetMethod("AI_007_TownEntities_GetWalkPrediction", BindingFlags.NonPublic | BindingFlags.Instance);
				_AI_007_TryForcingSitting = typeof(NPC).GetMethod("AI_007_TryForcingSitting", BindingFlags.NonPublic | BindingFlags.Instance);
			}
			catch (Exception e)
			{
				Logging.PublicLogger.Debug(e);
			}

			IL.Terraria.GameContent.Drawing.TileDrawing.DrawMultiTileVines += TileDrawing_DrawMultiTileVines;
			//IL_DrawMultiTileVines += TileDrawing_DrawMultiTileVines;
		}

		public static void Unload()
		{
			_addSpecialPointSpecialPositions = null;
			_addSpecialPointSpecialsCount = null;

			_AddHappinessReportText = null;

			_AI_007_FindGoodRestingSpot = null;
			_AI_007_TownEntities_IsInAGoodRestingSpot = null;
			_AI_007_TownEntities_TeleportToHome = null;
			_AI_007_TownEntities_GetWalkPrediction = null;
			_AI_007_TryForcingSitting = null;

			//IL_DrawMultiTileVines -= TileDrawing_DrawMultiTileVines;
		}

		//this does not actually need a manipulator
		//left in for future reference on how they work in the event that I ever need one
		/*private static event ILContext.Manipulator IL_DrawMultiTileVines
		{
			add => HookEndpointManager.Modify(typeof(Terraria.GameContent.Drawing.TileDrawing).GetMethod("DrawMultiTileVines", BindingFlags.NonPublic | BindingFlags.Instance), value);
			remove => HookEndpointManager.Unmodify(typeof(Terraria.GameContent.Drawing.TileDrawing).GetMethod("DrawMultiTileVines", BindingFlags.NonPublic | BindingFlags.Instance), value);
		}*/

		private static void TileDrawing_DrawMultiTileVines(ILContext il)
		{
			ILCursor c = new ILCursor(il);

			if (!c.TryGotoNext(MoveType.After,
				i => i.MatchLdloc(9),
				i => i.MatchLdnull(),
				i => i.MatchCall(out _),
				i => i.MatchBrfalse(out _),
				i => i.MatchLdloca(9),
				i => i.MatchCall(out _),
				i => i.MatchBrfalse(out _)
				))
				return;

			c.Emit(OpCodes.Ldloc, 9);
			c.EmitDelegate<Func<Tile, int>>((Tile tile) =>
			{
				if (tile.TileType == ModContent.TileType<Items.Placeable.Banners.BannerTile>())
				{
					return 3;
				}
				return 1;
			});
			c.Emit(OpCodes.Stloc, 8);
		}

		public static void SetResearch(this ModItem modItem, int researchValue)
		{
			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[modItem.Type] = researchValue;
		}

		public static FlavorTextBestiaryInfoElement TranslatedBestiaryEntry(this ModNPC modNPC)
		{
			return new FlavorTextBestiaryInfoElement(Language.GetTextValue("Mods.Polarities.Bestiary." + modNPC.GetType().Name));
		}

		private static FieldInfo _addSpecialPointSpecialPositions;
		private static FieldInfo _addSpecialPointSpecialsCount;

		public static void AddSpecialPoint(this Terraria.GameContent.Drawing.TileDrawing tileDrawing, int x, int y, int type)
		{
			if (_addSpecialPointSpecialPositions.GetValue(tileDrawing) is Point[][] _specialPositions)
			{
				if (_addSpecialPointSpecialsCount.GetValue(tileDrawing) is int[] _specialsCount)
				{
					_specialPositions[type][_specialsCount[type]++] = new Point(x, y);
				}
			}
		}

		public static Color ColorLerpCycle(float time, float cycleTime, params Color[] colors)
		{
			if (colors.Length == 0) return default(Color);

			int index = (int)(time / cycleTime * colors.Length) % colors.Length;
			float lerpAmount = time / cycleTime * colors.Length % 1;

			return Color.Lerp(colors[index], colors[(index + 1) % colors.Length], lerpAmount);
		}

		//based on the Box-Muller transformation, which is something I actually had to learn about so that's cool
		public static float NextNormallyDistributedFloat(this UnifiedRandom rand, float timeMultiplier = 1)
		{
			return (float)(Math.Sqrt(-2 * Math.Log(rand.NextFloat())) * Math.Cos(rand.NextFloat(MathHelper.TwoPi)) * Math.Sqrt(timeMultiplier));
		}

		public static float Lerp(float x, float y, float progress)
		{
			return x * (1 - progress) + y * progress;
		}

		public static Vector2 BezierCurve(Vector2[] bezierPoints, float bezierProgress)
		{
			if (bezierPoints.Length == 1)
			{
				return bezierPoints[0];
			}
			else
			{
				Vector2[] newBezierPoints = new Vector2[bezierPoints.Length - 1];
				for (int i = 0; i < bezierPoints.Length - 1; i++)
				{
					newBezierPoints[i] = bezierPoints[i] * bezierProgress + bezierPoints[i + 1] * (1 - bezierProgress);
				}
				return BezierCurve(newBezierPoints, bezierProgress);
			}
		}

		public static Vector2 BezierCurveDerivative(Vector2[] bezierPoints, float bezierProgress)
		{
			if (bezierPoints.Length == 2)
			{
				return bezierPoints[0] - bezierPoints[1];
			}
			else
			{
				Vector2[] newBezierPoints = new Vector2[bezierPoints.Length - 1];
				for (int i = 0; i < bezierPoints.Length - 1; i++)
				{
					newBezierPoints[i] = bezierPoints[i] * bezierProgress + bezierPoints[i + 1] * (1 - bezierProgress);
				}
				return BezierCurveDerivative(newBezierPoints, bezierProgress);
			}
		}

		public static Asset<T> GetAsset<T>(this Mod mod, string textureName) where T : class
		{
			return mod.Assets.Request<T>(textureName);
		}

		public static IItemDropRule MasterModeDropOnAllPlayersOrFlawless(int Type, int chanceDenominator, int amountDroppedMinimum = 1, int amountDroppedMaximum = 1, int chanceNumerator = 1)
		{
			return new DropBasedOnMasterMode(ItemDropRule.ByCondition(new FlawlessDropCondition(), Type, amountDroppedMinimum, amountDroppedMaximum), new FlawlessOrRandomDropRule(Type, chanceDenominator, amountDroppedMinimum, amountDroppedMaximum, chanceNumerator));
		}

		public static int SpawnSentry(this Player player, IEntitySource source, int ownerIndex, int sentryProjectileId, int originalDamageNotScaledByMinionDamage, float KnockBack, bool spawnWithGravity = true, Vector2 offsetFromDefaultPosition = default)
		{
			int num5 = (int)((float)Main.mouseX + Main.screenPosition.X) / 16;
			int num6 = (int)((float)Main.mouseY + Main.screenPosition.Y) / 16;
			if (player.gravDir == -1f)
			{
				num6 = (int)(Main.screenPosition.Y + (float)Main.screenHeight - (float)Main.mouseY) / 16;
			}
			if (spawnWithGravity)
			{
				for (; num6 < Main.maxTilesY - 10 && Main.tile[num5, num6] != null && !WorldGen.SolidTile2(num5, num6) && Main.tile[num5 - 1, num6] != null && !WorldGen.SolidTile2(num5 - 1, num6) && Main.tile[num5 + 1, num6] != null && !WorldGen.SolidTile2(num5 + 1, num6); num6++)
				{
				}
				num6--;
			}
			int num7 = Projectile.NewProjectile(source, (float)Main.mouseX + Main.screenPosition.X + offsetFromDefaultPosition.X, num6 * 16 - 24 + offsetFromDefaultPosition.Y, 0f, spawnWithGravity ? 15f : 0f, sentryProjectileId, originalDamageNotScaledByMinionDamage, KnockBack, ownerIndex);
			Main.projectile[num7].originalDamage = originalDamageNotScaledByMinionDamage;
			player.UpdateMaxTurrets();
			return num7;
		}

		public static bool IsTypeSummon(this Projectile projectile)
		{
			return projectile.minion || projectile.sentry || ProjectileID.Sets.MinionShot[projectile.type] || ProjectileID.Sets.SentryShot[projectile.type] || projectile.DamageType == DamageClass.Summon || projectile.DamageType.GetEffectInheritance(DamageClass.Summon);
		}

		private static MethodInfo _AddHappinessReportText;

		public static void AddHappinessReportText(this ShopHelper shopHelper, String textKeyInCategory, object substitutes = null)
		{
			_AddHappinessReportText.Invoke(shopHelper, new object[] { textKeyInCategory, substitutes });
		}

		private static MethodInfo _AI_007_FindGoodRestingSpot;
		private static MethodInfo _AI_007_TownEntities_IsInAGoodRestingSpot;
		private static MethodInfo _AI_007_TownEntities_TeleportToHome;
		private static MethodInfo _AI_007_TownEntities_GetWalkPrediction;
		private static MethodInfo _AI_007_TryForcingSitting;

		public static void AI_007_FindGoodRestingSpot(this NPC npc, int myTileX, int myTileY, out int floorX, out int floorY)
		{
			object[] parameters = new object[] { myTileX, myTileY, null, null };
			_AI_007_FindGoodRestingSpot.Invoke(npc, parameters);

			floorX = (int)parameters[2];
			floorY = (int)parameters[3];
		}

		public static bool AI_007_TownEntities_IsInAGoodRestingSpot(this NPC npc, int tileX, int tileY, int idealRestX, int idealRestY)
		{
			return (bool)_AI_007_TownEntities_IsInAGoodRestingSpot.Invoke(npc, new object[] { tileX, tileY, idealRestX, idealRestY });
		}

		public static void AI_007_TownEntities_TeleportToHome(this NPC npc, int homeFloorX, int homeFloorY)
		{
			_AI_007_TownEntities_TeleportToHome.Invoke(npc, new object[] { homeFloorX, homeFloorY });
		}

		public static void AI_007_TownEntities_GetWalkPrediction(this NPC npc, int myTileX, int homeFloorX, bool canBreathUnderWater, bool currentlyDrowning, int tileX, int tileY, out bool keepwalking, out bool avoidFalling)
		{
			object[] parameters = new object[] { myTileX, homeFloorX, canBreathUnderWater, currentlyDrowning, tileX, tileY, null, null };
			_AI_007_TownEntities_GetWalkPrediction.Invoke(npc, parameters);

			keepwalking = (bool)parameters[6];
			avoidFalling = (bool)parameters[7];
		}

		public static void AI_007_TryForcingSitting(this NPC npc, int homeFloorX, int homeFloorY)
		{
			_AI_007_TryForcingSitting.Invoke(npc, new object[] { homeFloorX, homeFloorY });
		}

		public static int BinomialCoefficient(int n, int k)
        {
			if (k > n / 2) return BinomialCoefficient(n, n - k);
			int output = 1;
			for (int i = 1; i <= k; i++)
            {
				output *= n - k + i;
				output /= i;
            }
			return output;
        }

		//for bestiary compatibility
		public static void SetBlendState(this NPC npc, SpriteBatch spriteBatch, BlendState blendState)
        {
			if (npc.IsABestiaryIconDummy)
			{
				RasterizerState priorRrasterizerState = spriteBatch.GraphicsDevice.RasterizerState;
				Rectangle priorScissorRectangle = spriteBatch.GraphicsDevice.ScissorRectangle;
				Main.spriteBatch.End();
				spriteBatch.GraphicsDevice.RasterizerState = priorRrasterizerState;
				spriteBatch.GraphicsDevice.ScissorRectangle = priorScissorRectangle;
				Main.spriteBatch.Begin((SpriteSortMode)0, blendState, Main.DefaultSamplerState, DepthStencilState.None, priorRrasterizerState, (Effect)null, Main.UIScaleMatrix);
			}
			else
            {
				Main.spriteBatch.End();
				Main.spriteBatch.Begin((SpriteSortMode)0, blendState, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, (Effect)null, Main.Transform);
			}
        }
	}
}