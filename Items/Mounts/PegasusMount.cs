using System;
using Microsoft.Xna.Framework;
using Polarities;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Polarities.Items.Mounts
{
	public class PegasusMountItem : ModItem
	{
		public override void SetStaticDefaults()
		{
			this.SetResearch(1);
		}

		public override void SetDefaults()
		{
			Item.width = 28;
			Item.height = 32;
			Item.useTime = 20;
			Item.useAnimation = 20;
			Item.useStyle = 1;

			Item.value = Item.sellPrice(gold: 5);
			Item.rare = ItemRarityID.LightPurple;

			Item.UseSound = SoundID.Item79;
			Item.noMelee = true;
			Item.mountType = MountType<PegasusMount>();
		}
	}

	public class PegasusMountBuff : ModBuff
	{
		public override void SetStaticDefaults()
		{
			Main.buffNoTimeDisplay[Type] = true;
			Main.buffNoSave[Type] = true;
		}

		public override void Update(Player player, ref int buffIndex)
		{
			player.mount.SetMount(MountType<PegasusMount>(), player);
			player.buffTime[buffIndex] = 10;
		}
	}

	public class PegasusMount : ModMount
	{
		public override void SetStaticDefaults()
		{
			MountData.spawnDust = 15;
			MountData.buff = BuffType<PegasusMountBuff>();

			MountData.heightBoost = 34;
			MountData.fallDamage = 0f;

			//MountData.flightTimeMax = 320;
			MountData.fatigueMax = 640;
			MountData.usesHover = true;

			MountData.runSpeed = 10f;//4f;
			MountData.dashSpeed = 10f;
			MountData.acceleration = 0.3f;

			MountData.jumpHeight = 10;
			MountData.jumpSpeed = 8.01f;

			MountData.blockExtraJumps = true;

			MountData.totalFrames = 15;

			int[] array = new int[MountData.totalFrames];
			for (int l = 0; l < array.Length; l++)
			{
				array[l] = 28;
			}
			array[3] += 2;
			array[4] += 2;
			array[7] += 2;
			array[8] += 2;
			array[12] += 2;
			array[13] += 2;

			MountData.playerYOffsets = array;
			MountData.xOffset = 5;
			MountData.bodyFrame = 3;
			MountData.yOffset = 1;

			MountData.playerHeadOffset = 31;

			MountData.standingFrameCount = 1;
			MountData.standingFrameDelay = 12;
			MountData.standingFrameStart = 0;

			MountData.runningFrameCount = 7;
			MountData.runningFrameDelay = 15;
			MountData.runningFrameStart = 1;

			MountData.dashingFrameCount = 6;
			MountData.dashingFrameDelay = 40;
			MountData.dashingFrameStart = 9;

			MountData.flyingFrameCount = 6;
			MountData.flyingFrameDelay = 4;
			MountData.flyingFrameStart = 9;

			MountData.inAirFrameCount = 6;
			MountData.inAirFrameDelay = 4;
			MountData.inAirFrameStart = 9;

			MountData.idleFrameCount = 0;
			MountData.idleFrameDelay = 0;
			MountData.idleFrameStart = 0;
			MountData.idleFrameLoop = false;

			MountData.swimFrameCount = 6;
			MountData.swimFrameDelay = 12;
			MountData.swimFrameStart = 9;

			if (Main.netMode == NetmodeID.Server)
			{
				return;
			}

			MountData.textureWidth = MountData.backTexture.Width();
			MountData.textureHeight = MountData.backTexture.Height();
		}

		public override void UpdateEffects(Player player)
		{
			if (Math.Abs(player.velocity.X) > player.mount.DashSpeed - player.mount.RunSpeed / 2f)
			{
				player.noKnockback = true;
			}

			if (player.wet && player.velocity.Y != 0)
			{
				player.controlDown = false;
				player.controlUp = true;
				if (player.mount._fatigue >= player.mount._fatigueMax)
					player.mount._fatigue--;
			}
		}

		public override bool UpdateFrame(Player mountedPlayer, int state, Vector2 velocity)
		{
			int realState = state;

			//adapted from vanilla mount framing code
			if (mountedPlayer.mount._frameState != realState)
			{
				mountedPlayer.mount._frameState = realState;
				mountedPlayer.mount._frameCounter = 0f;
			}
			if (realState != 0)
			{
				mountedPlayer.mount._idleTime = 0;
			}
			if (mountedPlayer.mount._data.emitsLight)
			{
				Point point = mountedPlayer.Center.ToTileCoordinates();
				Lighting.AddLight(point.X, point.Y, mountedPlayer.mount._data.lightColor.X, mountedPlayer.mount._data.lightColor.Y, mountedPlayer.mount._data.lightColor.Z);
			}


			//adapted from unicorn stuff specifically
			bool flag3 = Math.Abs(velocity.X) > mountedPlayer.mount.DashSpeed - 2f; //mountedPlayer.mount.RunSpeed / 2f;
			if (realState == 1)
			{
				if (flag3)
				{
					realState = 5;
					mountedPlayer.mount._frameExtra++;
				}
				else
				{
					mountedPlayer.mount._frameExtra = 0;
				}
			}


			//general mount stuff again
			switch (realState)
			{
				case 0:
					if (mountedPlayer.mount._data.idleFrameCount != 0)
					{
						if (mountedPlayer.mount._idleTime == 0)
						{
							mountedPlayer.mount._idleTimeNext = Main.rand.Next(900, 1500);
						}
						mountedPlayer.mount._idleTime++;
					}
					mountedPlayer.mount._frameCounter += 1f;
					if (mountedPlayer.mount._data.idleFrameCount != 0 && mountedPlayer.mount._idleTime >= mountedPlayer.mount._idleTimeNext)
					{
						float num18 = mountedPlayer.mount._data.idleFrameDelay;

						int num17 = (int)((float)(mountedPlayer.mount._idleTime - mountedPlayer.mount._idleTimeNext) / num18);
						if (num17 >= mountedPlayer.mount._data.idleFrameCount)
						{
							if (mountedPlayer.mount._data.idleFrameLoop)
							{
								mountedPlayer.mount._idleTime = mountedPlayer.mount._idleTimeNext;
								mountedPlayer.mount._frame = mountedPlayer.mount._data.idleFrameStart;
							}
							else
							{
								mountedPlayer.mount._frameCounter = 0f;
								mountedPlayer.mount._frame = mountedPlayer.mount._data.standingFrameStart;
								mountedPlayer.mount._idleTime = 0;
							}
						}
						else
						{
							mountedPlayer.mount._frame = mountedPlayer.mount._data.idleFrameStart + num17;
						}
					}
					else
					{
						if (mountedPlayer.mount._frameCounter > (float)mountedPlayer.mount._data.standingFrameDelay)
						{
							mountedPlayer.mount._frameCounter -= mountedPlayer.mount._data.standingFrameDelay;
							mountedPlayer.mount._frame++;
						}
						if (mountedPlayer.mount._frame < mountedPlayer.mount._data.standingFrameStart || mountedPlayer.mount._frame >= mountedPlayer.mount._data.standingFrameStart + mountedPlayer.mount._data.standingFrameCount)
						{
							mountedPlayer.mount._frame = mountedPlayer.mount._data.standingFrameStart;
						}
					}
					break;
				case 1:
					{
						float num19 = Math.Abs(velocity.X);

						mountedPlayer.mount._frameCounter += num19;
						if (num19 >= 0f)
						{
							if (mountedPlayer.mount._frameCounter > (float)mountedPlayer.mount._data.runningFrameDelay)
							{
								mountedPlayer.mount._frameCounter -= mountedPlayer.mount._data.runningFrameDelay;
								mountedPlayer.mount._frame++;
							}
							if (mountedPlayer.mount._frame < mountedPlayer.mount._data.runningFrameStart || mountedPlayer.mount._frame >= mountedPlayer.mount._data.runningFrameStart + mountedPlayer.mount._data.runningFrameCount)
							{
								mountedPlayer.mount._frame = mountedPlayer.mount._data.runningFrameStart;
							}
						}
						else
						{
							if (mountedPlayer.mount._frameCounter < 0f)
							{
								mountedPlayer.mount._frameCounter += mountedPlayer.mount._data.runningFrameDelay;
								mountedPlayer.mount._frame--;
							}
							if (mountedPlayer.mount._frame < mountedPlayer.mount._data.runningFrameStart || mountedPlayer.mount._frame >= mountedPlayer.mount._data.runningFrameStart + mountedPlayer.mount._data.runningFrameCount)
							{
								mountedPlayer.mount._frame = mountedPlayer.mount._data.runningFrameStart + mountedPlayer.mount._data.runningFrameCount - 1;
							}
						}
						break;
					}
				case 3:
					mountedPlayer.mount._frameCounter += 1f;
					if (mountedPlayer.mount._frameCounter > (float)mountedPlayer.mount._data.flyingFrameDelay)
					{
						mountedPlayer.mount._frameCounter -= mountedPlayer.mount._data.flyingFrameDelay;
						mountedPlayer.mount._frame++;
					}
					if (mountedPlayer.mount._frame < mountedPlayer.mount._data.flyingFrameStart || mountedPlayer.mount._frame >= mountedPlayer.mount._data.flyingFrameStart + mountedPlayer.mount._data.flyingFrameCount)
					{
						mountedPlayer.mount._frame = mountedPlayer.mount._data.flyingFrameStart;
					}
					break;
				case 2:
					mountedPlayer.mount._frameCounter += 1f;
					if (mountedPlayer.mount._frameCounter > (float)mountedPlayer.mount._data.inAirFrameDelay)
					{
						mountedPlayer.mount._frameCounter -= mountedPlayer.mount._data.inAirFrameDelay;
						mountedPlayer.mount._frame++;
					}
					if (mountedPlayer.mount._frame < mountedPlayer.mount._data.inAirFrameStart || mountedPlayer.mount._frame >= mountedPlayer.mount._data.inAirFrameStart + mountedPlayer.mount._data.inAirFrameCount)
					{
						mountedPlayer.mount._frame = mountedPlayer.mount._data.inAirFrameStart;
					}
					break;
				case 4:
					mountedPlayer.mount._frameCounter += (int)((Math.Abs(velocity.X) + Math.Abs(velocity.Y)) / 2f);
					if (mountedPlayer.mount._frameCounter > (float)mountedPlayer.mount._data.swimFrameDelay)
					{
						mountedPlayer.mount._frameCounter -= mountedPlayer.mount._data.swimFrameDelay;
						mountedPlayer.mount._frame++;
					}
					if (mountedPlayer.mount._frame < mountedPlayer.mount._data.swimFrameStart || mountedPlayer.mount._frame >= mountedPlayer.mount._data.swimFrameStart + mountedPlayer.mount._data.swimFrameCount)
					{
						mountedPlayer.mount._frame = mountedPlayer.mount._data.swimFrameStart;
					}
					break;
				case 5:
					{
						float num21 = Math.Abs(velocity.X);

						mountedPlayer.mount._frameCounter += num21;
						if (num21 >= 0f)
						{
							if (mountedPlayer.mount._frameCounter > (float)mountedPlayer.mount._data.dashingFrameDelay)
							{
								mountedPlayer.mount._frameCounter -= mountedPlayer.mount._data.dashingFrameDelay;
								mountedPlayer.mount._frame++;
							}
							if (mountedPlayer.mount._frame < mountedPlayer.mount._data.dashingFrameStart || mountedPlayer.mount._frame >= mountedPlayer.mount._data.dashingFrameStart + mountedPlayer.mount._data.dashingFrameCount)
							{
								mountedPlayer.mount._frame = mountedPlayer.mount._data.dashingFrameStart;
							}
						}
						else
						{
							if (mountedPlayer.mount._frameCounter < 0f)
							{
								mountedPlayer.mount._frameCounter += mountedPlayer.mount._data.dashingFrameDelay;
								mountedPlayer.mount._frame--;
							}
							if (mountedPlayer.mount._frame < mountedPlayer.mount._data.dashingFrameStart || mountedPlayer.mount._frame >= mountedPlayer.mount._data.dashingFrameStart + mountedPlayer.mount._data.dashingFrameCount)
							{
								mountedPlayer.mount._frame = mountedPlayer.mount._data.dashingFrameStart + mountedPlayer.mount._data.dashingFrameCount - 1;
							}
						}
						break;
					}
			}

			return false;
		}
	}
}