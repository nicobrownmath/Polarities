using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using Polarities.Items;
using Polarities.NPCs;
using ReLogic.Content;
using System;
using System.Reflection;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.Utilities;
using static Terraria.ModLoader.ModContent;

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

                _miscShaderDataImage0 = typeof(MiscShaderData).GetField("_uImage0", BindingFlags.NonPublic | BindingFlags.Instance);
                _miscShaderDataImage1 = typeof(MiscShaderData).GetField("_uImage1", BindingFlags.NonPublic | BindingFlags.Instance);
                _miscShaderDataImage2 = typeof(MiscShaderData).GetField("_uImage2", BindingFlags.NonPublic | BindingFlags.Instance);

            }
            catch (Exception e)
            {
                Logging.PublicLogger.Debug(e);
            }

            IL.Terraria.GameContent.Drawing.TileDrawing.DrawMultiTileVines += TileDrawing_DrawMultiTileVines;
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

            _miscShaderDataImage0 = null;
            _miscShaderDataImage1 = null;
            _miscShaderDataImage2 = null;

        }

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
            {
                GetInstance<Polarities>().Logger.Debug("Failed to find patch location");
                return;
            }

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

        public static bool CheckAABBvDisc(Rectangle rectangle, Circle circle)
        {
            float nearestX = Math.Max(rectangle.X, Math.Min(circle.Center.X, rectangle.X + rectangle.Size().X));
            float nearestY = Math.Max(rectangle.Y, Math.Min(circle.Center.Y, rectangle.Y + rectangle.Size().Y));
            return (new Vector2(circle.Center.X - nearestX, circle.Center.Y - nearestY)).Length() < circle.radius;
        }

        public static bool CheckAABBvSegment(Rectangle rectangle, Vector2 start, Vector2 end)
        {
            if ((start.X < rectangle.Left && end.X < rectangle.Left) || (start.X > rectangle.Right && end.X > rectangle.Right) || (start.Y < rectangle.Top && end.Y < rectangle.Top) || (start.Y > rectangle.Bottom && end.Y > rectangle.Bottom))
                return false;

            float f1 = (end.Y - start.Y) * rectangle.Left + (start.X - end.X) * rectangle.Top + (end.X * start.Y - start.X * end.Y);
            float f2 = (end.Y - start.Y) * rectangle.Left + (start.X - end.X) * rectangle.Bottom + (end.X * start.Y - start.X * end.Y);
            float f3 = (end.Y - start.Y) * rectangle.Right + (start.X - end.X) * rectangle.Top + (end.X * start.Y - start.X * end.Y);
            float f4 = (end.Y - start.Y) * rectangle.Right + (start.X - end.X) * rectangle.Bottom + (end.X * start.Y - start.X * end.Y);

            if (f1 < 0 && f2 < 0 && f3 < 0 && f4 < 0) return false;
            if (f1 > 0 && f2 > 0 && f3 > 0 && f4 > 0) return false;

            return true;
        }

        public static bool CheckPointvTriangle(Vector2 point, Vector2 vertex0, Vector2 vertex1, Vector2 vertex2)
        {
            var s = vertex0.Y * vertex2.X - vertex0.X * vertex2.Y + (vertex2.Y - vertex0.Y) * point.X + (vertex0.X - vertex2.X) * point.Y;
            var t = vertex0.X * vertex1.Y - vertex0.Y * vertex1.X + (vertex0.Y - vertex1.Y) * point.X + (vertex1.X - vertex0.X) * point.Y;

            if ((s < 0) != (t < 0) || s == 0 || t == 0)
                return false;

            var A = -vertex1.Y * vertex2.X + vertex0.Y * (vertex2.X - vertex1.X) + vertex0.X * (vertex1.Y - vertex2.Y) + vertex1.X * vertex2.Y;

            return A < 0 ?
                    (s < 0 && s + t > A) :
                    (s > 0 && s + t < A);
        }

        public static bool CheckAABBvTriangle(Rectangle rectangle, Vector2 vertex0, Vector2 vertex1, Vector2 vertex2)
        {
            if (CheckAABBvPoint(rectangle, vertex0))
                return true;
            if (CheckAABBvPoint(rectangle, vertex1))
                return true;
            if (CheckAABBvPoint(rectangle, vertex2))
                return true;
            if (CheckAABBvSegment(rectangle, vertex0, vertex1))
                return true;
            if (CheckAABBvSegment(rectangle, vertex1, vertex2))
                return true;
            if (CheckAABBvSegment(rectangle, vertex2, vertex0))
                return true;
            if (CheckPointvTriangle(rectangle.TopLeft(), vertex0, vertex1, vertex2))
                return true;
            return false;
        }

        public static float AngleBetween(Vector2 a, Vector2 b)
        {
            return (float)Math.Acos(Vector2.Dot(a, b) / (a.Length() * b.Length()));
        }

        public static bool CheckAABBvCircle(Rectangle rectangle, Circle circle, float thickness = 0f)
        {
            float nearestX = Math.Max(rectangle.X, Math.Min(circle.Center.X, rectangle.X + rectangle.Size().X));
            float nearestY = Math.Max(rectangle.Y, Math.Min(circle.Center.Y, rectangle.Y + rectangle.Size().Y));
            bool isInside = (new Vector2(circle.Center.X - nearestX, circle.Center.Y - nearestY)).Length() < circle.radius + thickness / 2;

            float furthestX = rectangle.X + rectangle.Size().X / 2 - circle.Center.X > 0 ? rectangle.X + rectangle.Size().X : rectangle.X;
            float furthestY = rectangle.Y + rectangle.Size().Y / 2 - circle.Center.Y > 0 ? rectangle.Y + rectangle.Size().Y : rectangle.Y;
            bool isOutside = (new Vector2(circle.Center.X - furthestX, circle.Center.Y - furthestY)).Length() > circle.radius - thickness / 2;

            return isInside && isOutside;
        }

        public static bool CheckArcAngle(Vector2 a, Arc arc)
        {
            float angle = (a - arc.Center).ToRotation();
            float startAngle = arc.StartAngle();

            if (startAngle <= angle && angle <= startAngle + arc.Angle) return true;
            if (startAngle <= angle + MathHelper.TwoPi && angle + MathHelper.TwoPi <= startAngle + arc.Angle) return true;
            return false;
        }

        public static bool CheckSegmentvArc(Vector2 a, Vector2 b, Arc arc)
        {
            Circle circle = arc.ToCircle();

            float A = (a.X - b.X) * (a.X - b.X) + (a.Y - b.Y) * (a.Y - b.Y);
            float B = 2 * (a.X - b.X) * (b.X - circle.Center.X) + 2 * (a.Y - b.Y) * (b.Y - circle.Center.Y);
            float C = (b.X - circle.Center.X) * (b.X - circle.Center.X) + (b.Y - circle.Center.Y) * (b.Y - circle.Center.Y) - circle.radius * circle.radius;

            float det = B * B - 4 * A * C;
            if (det < 0) return false;

            float t0 = (-B + (float)Math.Sqrt(det)) / (2 * A);
            float t1 = (-B - (float)Math.Sqrt(det)) / (2 * A);

            if (t0 >= 0 && t0 <= 1)
            {
                if (CheckArcAngle(a * t0 + b * (1 - t0), arc)) return true;
            }
            if (t1 >= 0 && t1 <= 1)
            {
                if (CheckArcAngle(a * t1 + b * (1 - t1), arc)) return true;
            }
            return false;
        }

        public static bool CheckAABBvPoint(Rectangle rectangle, float pointX, float pointY)
        {
            return pointX >= rectangle.X && pointX <= rectangle.Right && pointY >= rectangle.Y && pointY <= rectangle.Bottom;
        }
        public static bool CheckAABBvPoint(Rectangle rectangle, Vector2 point)
        {
            return point.X >= rectangle.X && point.X <= rectangle.Right && point.Y >= rectangle.Y && point.Y <= rectangle.Bottom;
        }

        public static bool CheckAABBvArc(Rectangle rectangle, Arc arc)
        {
            if (!CheckAABBvCircle(rectangle, arc.ToCircle())) return false;

            if (CheckSegmentvArc(rectangle.TopLeft(), rectangle.TopRight(), arc)) return true;
            if (CheckSegmentvArc(rectangle.TopLeft(), rectangle.BottomLeft(), arc)) return true;
            if (CheckSegmentvArc(rectangle.TopRight(), rectangle.BottomRight(), arc)) return true;
            if (CheckSegmentvArc(rectangle.BottomLeft(), rectangle.BottomRight(), arc)) return true;

            return CheckAABBvPoint(rectangle, arc.Start);
        }

        public static void SetMerge(int type1, int type2, bool merge = true)
        {
            if (type1 != type2)
            {
                Main.tileMerge[type1][type2] = merge;
                Main.tileMerge[type2][type1] = merge;
            }
        }
        public static void SetMerge<T>(int type2, bool merge = true) where T : ModTile
        {
            SetMerge(ModContent.TileType<T>(), type2, merge);
        }
        public static void SetMerge<T, T2>(int type2, bool merge = true) where T : ModTile where T2 : ModTile
        {
            SetMerge(ModContent.TileType<T>(), ModContent.TileType<T2>(), merge);
        }
        public static void SetMerge(this ModTile modTile, int type2, bool merge = true)
        {
            SetMerge(modTile.Type, type2, merge);
        }
        public static void SetMerge<T2>(this ModTile modTile, bool merge = true) where T2 : ModTile
        {
            SetMerge(modTile.Type, ModContent.TileType<T2>(), merge);
        }

        public static void SetModBiome<T, T2, T3>(this ModNPC modNPC) where T : ModBiome where T2 : ModBiome where T3 : ModBiome
        {
            modNPC.SpawnModBiomes = new int[] { ModContent.GetInstance<T>().Type, ModContent.GetInstance<T2>().Type, ModContent.GetInstance<T3>().Type };
        }
        public static void SetModBiome<T, T2>(this ModNPC modNPC) where T : ModBiome where T2 : ModBiome
        {
            modNPC.SpawnModBiomes = new int[] { ModContent.GetInstance<T>().Type, ModContent.GetInstance<T2>().Type, };
        }
        public static void SetModBiome<T>(this ModNPC modNPC) where T : ModBiome
        {
            modNPC.SpawnModBiomes = new int[] { ModContent.GetInstance<T>().Type, };
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

        private static FieldInfo _miscShaderDataImage0;
        private static FieldInfo _miscShaderDataImage1;
        private static FieldInfo _miscShaderDataImage2;

        public static MiscShaderData UseImage0(this MiscShaderData shaderData, Asset<Texture2D> image)
        {
            _miscShaderDataImage0.SetValue(shaderData, image);
            return shaderData;
        }
        public static MiscShaderData UseImage1(this MiscShaderData shaderData, Asset<Texture2D> image)
        {
            _miscShaderDataImage1.SetValue(shaderData, image);
            return shaderData;
        }
        public static MiscShaderData UseImage2(this MiscShaderData shaderData, Asset<Texture2D> image)
        {
            _miscShaderDataImage2.SetValue(shaderData, image);
            return shaderData;
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

        public static int GetFractalization(this Player player)
        {
            return player.Polarities().GetFractalization();
        }
        public static PolaritiesPlayer Polarities(this Player player)
        {
            return player.GetModPlayer<PolaritiesPlayer>();
        }

        public static int SpawnSentry(this Player player, IEntitySource source, int ownerIndex, int sentryProjectileId, int originalDamageNotScaledByMinionDamage, float KnockBack, bool spawnWithGravity = true, Vector2 offsetFromDefaultPosition = default)
        {
            int num5 = (int)(Main.mouseX + Main.screenPosition.X) / 16;
            int num6 = (int)(Main.mouseY + Main.screenPosition.Y) / 16;
            if (player.gravDir == -1f)
            {
                num6 = (int)(Main.screenPosition.Y + Main.screenHeight - Main.mouseY) / 16;
            }
            if (spawnWithGravity)
            {
                for (; num6 < Main.maxTilesY - 10 && Main.tile[num5, num6] != null && !WorldGen.SolidTile2(num5, num6) && Main.tile[num5 - 1, num6] != null && !WorldGen.SolidTile2(num5 - 1, num6) && Main.tile[num5 + 1, num6] != null && !WorldGen.SolidTile2(num5 + 1, num6); num6++)
                {
                }
                num6--;
            }
            int num7 = Projectile.NewProjectile(source, Main.mouseX + Main.screenPosition.X + offsetFromDefaultPosition.X, num6 * 16 - 24 + offsetFromDefaultPosition.Y, 0f, spawnWithGravity ? 15f : 0f, sentryProjectileId, originalDamageNotScaledByMinionDamage, KnockBack, ownerIndex);
            Main.projectile[num7].originalDamage = originalDamageNotScaledByMinionDamage;
            player.UpdateMaxTurrets();
            return num7;
        }

        public static bool IsTypeSummon(this Projectile projectile)
        {
            return projectile.minion || projectile.sentry || ProjectileID.Sets.MinionShot[projectile.type] || ProjectileID.Sets.SentryShot[projectile.type] || projectile.DamageType == DamageClass.Summon || projectile.DamageType.GetEffectInheritance(DamageClass.Summon);
        }

        private static MethodInfo _AddHappinessReportText;

        public static void AddHappinessReportText(this ShopHelper shopHelper, string textKeyInCategory, object substitutes = null)
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
                Main.spriteBatch.Begin(0, blendState, Main.DefaultSamplerState, DepthStencilState.None, priorRrasterizerState, null, Main.UIScaleMatrix);
            }
            else
            {
                Main.spriteBatch.End();
                Main.spriteBatch.Begin(0, blendState, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.Transform);
            }
        }



        //perlin/fractal noise based on this tutorial: https://dens.website/articles/procedural-generation/perlin-noise
        public static Vector2[,] PerlinBaseVectors(this UnifiedRandom rand, int width, int height)
        {
            Vector2[,] output = new Vector2[width, height];
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    output[i, j] = new Vector2(0, 1).RotatedBy(rand.NextFloat(MathHelper.TwoPi));
                }
            }
            return output;
        }

        public static float PerlinSmoothStep(float amount)
        {
            return amount * amount * amount * (amount * (amount * 6 - 15) + 10); ;
        }

        public static float PerlinSmooth(Vector2 v00, Vector2 v01, Vector2 v10, Vector2 v11, float xAmount, float yAmount)
        {
            float d00 = Vector2.Dot(v00, new Vector2(xAmount, yAmount));
            float d01 = Vector2.Dot(v01, new Vector2(xAmount, yAmount - 1));
            float d10 = Vector2.Dot(v10, new Vector2(xAmount - 1, yAmount));
            float d11 = Vector2.Dot(v11, new Vector2(xAmount - 1, yAmount - 1));

            float xLerp = PerlinSmoothStep(xAmount);
            float yLerp = PerlinSmoothStep(yAmount);
            return Lerp(Lerp(d00, d01, yLerp), Lerp(d10, d11, yLerp), xLerp);
        }

        public static float PerlinNoiseValue(Vector2[,] perlinBaseVectors, float xAmount, float yAmount)
        {
            int xIndex = (int)(xAmount * perlinBaseVectors.GetLength(0));
            float residualXAmount = (xAmount * perlinBaseVectors.GetLength(0)) % 1;

            int yIndex = (int)(yAmount * perlinBaseVectors.GetLength(1));
            float residualYAmount = (yAmount * perlinBaseVectors.GetLength(1)) % 1;

            return PerlinSmooth(perlinBaseVectors[xIndex, yIndex], perlinBaseVectors[xIndex, (yIndex + 1) % perlinBaseVectors.GetLength(1)], perlinBaseVectors[(xIndex + 1) % perlinBaseVectors.GetLength(0), yIndex], perlinBaseVectors[(xIndex + 1) % perlinBaseVectors.GetLength(0), (yIndex + 1) % perlinBaseVectors.GetLength(1)], residualXAmount, residualYAmount) * (float)Math.Sqrt(0.5f);
        }

        public static float[,] FractalNoise(this UnifiedRandom rand, int size, int startFactor = 1)
        {
            int scaleSize = size / startFactor;

            float[,] output = new float[size, size];

            int depth = 0;

            while (scaleSize > 0)
            {
                Vector2[,] perlinBaseVectors = rand.PerlinBaseVectors(size / scaleSize, size / scaleSize);

                float amplitude = (float)Math.Pow(0.5f, depth);

                for (int i = 0; i < size; i++)
                {
                    for (int j = 0; j < size; j++)
                    {
                        output[i, j] += PerlinNoiseValue(perlinBaseVectors, i / (float)size, j / (float)size) * amplitude;
                    }
                }

                scaleSize /= 2;
                depth++;
            }

            return output;
        }

        public static float[] FractalNoise1D(this UnifiedRandom rand, int size, int startFactor = 1)
        {
            int scaleSize = size / startFactor;

            float[] output = new float[size];

            int depth = 0;

            while (scaleSize > 0)
            {
                Vector2[,] perlinBaseVectors = rand.PerlinBaseVectors(size / scaleSize, 1);

                float amplitude = (float)Math.Pow(0.5f, depth);

                for (int i = 0; i < size; i++)
                {
                    output[i] += PerlinNoiseValue(perlinBaseVectors, i / (float)size, 0) * amplitude;
                }

                scaleSize /= 2;
                depth++;
            }

            return output;
        }


        public static Color ConvectiveFlameColor(float progress)
        {
            float clampedProgress = Math.Clamp(progress, 0, 1);
            float r = 1.25f - clampedProgress / 2;
            float g = clampedProgress < 0.5f ? 4 * clampedProgress * (1 - clampedProgress) : 13 / 12f - clampedProgress / 6f;
            float b = 2 * clampedProgress;
            return new Color(r, g, b);
        }
    }

    public readonly struct Arc
    {
        public readonly Vector2 Center;
        public readonly Vector2 Start;
        public readonly float Angle;

        public Arc(Vector2 _Center, Vector2 _Start, float _angle)
        {
            Center = _Center;
            Start = _Start;
            Angle = _angle;

            if (Angle < 0)
            {
                Start = Move(1);
                Angle = -Angle;
            }
        }

        public float StartAngle()
        {
            return (Start - Center).ToRotation();
        }

        public float Radius()
        {
            return (Start - Center).Length();
        }

        public Vector2 Move(float progress)
        {
            return Center + (Start - Center).RotatedBy(Angle * progress);
        }

        public Circle ToCircle()
        {
            return new Circle(Center, (Start - Center).Length());
        }
    }
}