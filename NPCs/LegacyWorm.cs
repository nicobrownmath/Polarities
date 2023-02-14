using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace Polarities.NPCs
{
    public abstract class LegacyWorm : ModNPC
    {
        /* ai[0] = follower
		 * ai[1] = following
		 * ai[2] = distanceFromTail
		 * ai[3] = head
		 */
        public bool head;
        public bool tail;
        public int minLength;
        public int maxLength;
        public int headType;
        public int bodyType;
        public int tailType;
        public bool flies = false;
        public bool directional = false;
        public float speed;
        public float turnSpeed;
        public int segmentLength;
        public bool digSounds = false;

        public override void AI()
        {
            if (NPC.localAI[1] == 0f)
            {
                NPC.localAI[1] = 1f;
                Init();
            }
            if (NPC.ai[3] > 0f)
            {
                NPC.realLife = (int)NPC.ai[3];
            }
            if (!head && NPC.timeLeft < 300)
            {
                NPC.timeLeft = 300;
            }
            if (NPC.target < 0 || NPC.target == 255 || Main.player[NPC.target].dead)
            {
                NPC.TargetClosest(true);
            }
            if (Main.player[NPC.target].dead && NPC.timeLeft > 300)
            {
                NPC.timeLeft = 300;
            }
            if (Main.netMode != 1)
            {
                if (!tail && NPC.ai[0] == 0f)
                {
                    if (head)
                    {
                        NPC.ai[3] = NPC.whoAmI;
                        NPC.realLife = NPC.whoAmI;
                        NPC.ai[2] = Main.rand.Next(minLength, maxLength + 1);
                        NPC.ai[0] = NPC.NewNPC(NPC.GetSource_FromThis(), (int)(NPC.position.X + NPC.width / 2), (int)(NPC.position.Y + NPC.height) + 2, bodyType, NPC.whoAmI);
                    }
                    else if (NPC.ai[2] > 0f)
                    {
                        NPC.ai[0] = NPC.NewNPC(NPC.GetSource_FromThis(), (int)(NPC.position.X + NPC.width / 2), (int)(NPC.position.Y + NPC.height) + 2, bodyType, NPC.whoAmI);
                    }
                    else
                    {
                        NPC.ai[0] = NPC.NewNPC(NPC.GetSource_FromThis(), (int)(NPC.position.X + NPC.width / 2), (int)(NPC.position.Y + NPC.height) + 2, tailType, NPC.whoAmI);
                    }

                    Main.npc[(int)NPC.ai[0]].ai[3] = NPC.ai[3];
                    Main.npc[(int)NPC.ai[0]].realLife = NPC.realLife;
                    Main.npc[(int)NPC.ai[0]].ai[1] = NPC.whoAmI;
                    Main.npc[(int)NPC.ai[0]].ai[2] = NPC.ai[2] - 1f;
                    NPC.netUpdate = true;
                }
                if (!head && (!Main.npc[(int)NPC.ai[1]].active || Main.npc[(int)NPC.ai[1]].type != headType && Main.npc[(int)NPC.ai[1]].type != bodyType))
                {
                    NPC.life = 0;
                    NPC.HitEffect(0, 10.0);
                }
                if (!tail && (!Main.npc[(int)NPC.ai[0]].active || Main.npc[(int)NPC.ai[0]].type != bodyType && Main.npc[(int)NPC.ai[0]].type != tailType))
                {
                    NPC.life = 0;
                    NPC.HitEffect(0, 10.0);
                }
                if (!NPC.active && Main.netMode == 2)
                {
                    NetMessage.SendData(28, -1, -1, null, NPC.whoAmI, -1f, 0f, 0f, 0, 0, 0);
                }
            }
            int num180 = (int)(NPC.position.X / 16f) - 1;
            int num181 = (int)((NPC.position.X + NPC.width) / 16f) + 2;
            int num182 = (int)(NPC.position.Y / 16f) - 1;
            int num183 = (int)((NPC.position.Y + NPC.height) / 16f) + 2;
            if (num180 < 0)
            {
                num180 = 0;
            }
            if (num181 > Main.maxTilesX)
            {
                num181 = Main.maxTilesX;
            }
            if (num182 < 0)
            {
                num182 = 0;
            }
            if (num183 > Main.maxTilesY)
            {
                num183 = Main.maxTilesY;
            }
            bool flag18 = flies;
            if (!flag18)
            {
                for (int num184 = num180; num184 < num181; num184++)
                {
                    for (int num185 = num182; num185 < num183; num185++)
                    {
                        if (Main.tile[num184, num185] != null && (Main.tile[num184, num185].HasUnactuatedTile && (Main.tileSolid[Main.tile[num184, num185].TileType] || Main.tileSolidTop[Main.tile[num184, num185].TileType] && Main.tile[num184, num185].TileFrameY == 0) || Main.tile[num184, num185].LiquidAmount > 64))
                        {
                            Vector2 vector17;
                            vector17.X = num184 * 16;
                            vector17.Y = num185 * 16;
                            if (NPC.position.X + NPC.width > vector17.X && NPC.position.X < vector17.X + 16f && NPC.position.Y + NPC.height > vector17.Y && NPC.position.Y < vector17.Y + 16f)
                            {
                                flag18 = true;
                                if (digSounds && Main.rand.NextBool(100) && NPC.behindTiles && Main.tile[num184, num185].HasUnactuatedTile)
                                {
                                    WorldGen.KillTile(num184, num185, true, true, false);
                                }
                                if (Main.netMode != 1 && Main.tile[num184, num185].TileType == 2)
                                {
                                    ushort arg_BFCA_0 = Main.tile[num184, num185 - 1].TileType;
                                }
                            }
                        }
                    }
                }
            }
            if (!flag18 && head)
            {
                /*Rectangle rectangle = new Rectangle((int)npc.position.X, (int)npc.position.Y, npc.width, npc.height);
				int num186 = 1000;
				bool flag19 = true;
				for (int num187 = 0; num187 < 255; num187++) {
					if (Main.player[num187].active) {
						Rectangle rectangle2 = new Rectangle((int)Main.player[num187].position.X - num186, (int)Main.player[num187].position.Y - num186, num186 * 2, num186 * 2);
						if (rectangle.Intersects(rectangle2)) {
							flag19 = false;
							break;
						}
					}
				}
				if (flag19) {
					flag18 = true;
				}*/
            }
            if (directional)
            {
                if (NPC.velocity.X < 0f)
                {
                    NPC.spriteDirection = 1;
                }
                else if (NPC.velocity.X > 0f)
                {
                    NPC.spriteDirection = -1;
                }
            }
            Vector2 vector18 = new Vector2(NPC.position.X + NPC.width * 0.5f, NPC.position.Y + NPC.height * 0.5f);
            float num191 = Main.player[NPC.target].position.X + Main.player[NPC.target].width / 2;
            float num192 = Main.player[NPC.target].position.Y + Main.player[NPC.target].height / 2;
            num191 = (int)(num191 / 16f) * 16;
            num192 = (int)(num192 / 16f) * 16;
            vector18.X = (int)(vector18.X / 16f) * 16;
            vector18.Y = (int)(vector18.Y / 16f) * 16;
            num191 -= vector18.X;
            num192 -= vector18.Y;
            float num193 = (float)Math.Sqrt(num191 * num191 + num192 * num192);
            if (NPC.ai[1] > 0f && NPC.ai[1] < Main.npc.Length)
            {
                try
                {
                    vector18 = NPC.Center;
                    num191 = Main.npc[(int)NPC.ai[1]].Center.X - NPC.Center.X + (0.5f * segmentLength * new Vector2(0, 1).RotatedBy(Main.npc[(int)NPC.ai[1]].rotation)).X;
                    num192 = Main.npc[(int)NPC.ai[1]].Center.Y - NPC.Center.Y + (0.5f * segmentLength * new Vector2(0, 1).RotatedBy(Main.npc[(int)NPC.ai[1]].rotation)).Y;
                }
                catch
                {
                    NPC.netUpdate = true;
                    vector18 = NPC.Center;
                    num191 = Main.npc[(int)NPC.ai[1]].Center.X - NPC.Center.X + (0.5f * segmentLength * new Vector2(0, 1).RotatedBy(Main.npc[(int)NPC.ai[1]].rotation)).X;
                    num192 = Main.npc[(int)NPC.ai[1]].Center.Y - NPC.Center.Y + (0.5f * segmentLength * new Vector2(0, 1).RotatedBy(Main.npc[(int)NPC.ai[1]].rotation)).Y;
                }
                NPC.rotation = (float)Math.Atan2(num192, num191) + 1.57f;
                num193 = (float)Math.Sqrt(num191 * num191 + num192 * num192);
                int num194 = NPC.width / 2;
                num193 = (num193 - num194) / num193;
                num191 *= num193;
                num192 *= num193;
                NPC.velocity = Vector2.Zero;
                NPC.position.X = NPC.position.X + num191;
                NPC.position.Y = NPC.position.Y + num192;
                if (directional)
                {
                    if (num191 < 0f)
                    {
                        NPC.spriteDirection = 1;
                    }
                    if (num191 > 0f)
                    {
                        NPC.spriteDirection = -1;
                    }
                }
            }
            else
            {
                if (!flag18)
                {
                    NPC.TargetClosest(true);
                    NPC.velocity.Y = NPC.velocity.Y + 0.11f;
                    if (NPC.velocity.Y > speed)
                    {
                        NPC.velocity.Y = speed;
                    }
                    if (Math.Abs(NPC.velocity.X) + Math.Abs(NPC.velocity.Y) < speed * 0.4)
                    {
                        if (NPC.velocity.X < 0f)
                        {
                            NPC.velocity.X = NPC.velocity.X - turnSpeed * 1.1f;
                        }
                        else
                        {
                            NPC.velocity.X = NPC.velocity.X + turnSpeed * 1.1f;
                        }
                    }
                    else if (NPC.velocity.Y == speed)
                    {
                        if (NPC.velocity.X < num191)
                        {
                            NPC.velocity.X = NPC.velocity.X + turnSpeed;
                        }
                        else if (NPC.velocity.X > num191)
                        {
                            NPC.velocity.X = NPC.velocity.X - turnSpeed;
                        }
                    }
                    else if (NPC.velocity.Y > 4f)
                    {
                        if (NPC.velocity.X < 0f)
                        {
                            NPC.velocity.X = NPC.velocity.X + turnSpeed * 0.9f;
                        }
                        else
                        {
                            NPC.velocity.X = NPC.velocity.X - turnSpeed * 0.9f;
                        }
                    }
                }
                else
                {
                    if (!flies && NPC.behindTiles && NPC.soundDelay == 0)
                    {
                        float num195 = num193 / 40f;
                        if (num195 < 10f)
                        {
                            num195 = 10f;
                        }
                        if (num195 > 20f)
                        {
                            num195 = 20f;
                        }
                        NPC.soundDelay = (int)num195;

                        if (digSounds)
                        {
                            SoundEngine.PlaySound(SoundID.WormDig, NPC.position);
                        }
                    }
                    num193 = (float)Math.Sqrt(num191 * num191 + num192 * num192);
                    float num196 = Math.Abs(num191);
                    float num197 = Math.Abs(num192);
                    float num198 = speed / num193;
                    num191 *= num198;
                    num192 *= num198;
                    if (ShouldRun())
                    {
                        bool flag20 = true;
                        for (int num199 = 0; num199 < 255; num199++)
                        {
                            if (Main.player[num199].active && !Main.player[num199].dead && Main.player[num199].ZoneCorrupt)
                            {
                                flag20 = false;
                            }
                        }
                        if (flag20)
                        {
                            if (Main.netMode != 1 && NPC.position.Y / 16f > (Main.rockLayer + Main.maxTilesY) / 2.0)
                            {
                                NPC.active = false;
                                int num200 = (int)NPC.ai[0];
                                while (num200 > 0 && num200 < 200 && Main.npc[num200].active && Main.npc[num200].aiStyle == NPC.aiStyle)
                                {
                                    int num201 = (int)Main.npc[num200].ai[0];
                                    Main.npc[num200].active = false;
                                    NPC.life = 0;
                                    if (Main.netMode == 2)
                                    {
                                        NetMessage.SendData(23, -1, -1, null, num200, 0f, 0f, 0f, 0, 0, 0);
                                    }
                                    num200 = num201;
                                }
                                if (Main.netMode == 2)
                                {
                                    NetMessage.SendData(23, -1, -1, null, NPC.whoAmI, 0f, 0f, 0f, 0, 0, 0);
                                }
                            }
                            num191 = 0f;
                            num192 = speed;
                        }
                    }
                    bool flag21 = false;
                    if (NPC.type == NPCID.WyvernHead)
                    {
                        if ((NPC.velocity.X > 0f && num191 < 0f || NPC.velocity.X < 0f && num191 > 0f || NPC.velocity.Y > 0f && num192 < 0f || NPC.velocity.Y < 0f && num192 > 0f) && Math.Abs(NPC.velocity.X) + Math.Abs(NPC.velocity.Y) > turnSpeed / 2f && num193 < 300f)
                        {
                            flag21 = true;
                            if (Math.Abs(NPC.velocity.X) + Math.Abs(NPC.velocity.Y) < speed)
                            {
                                NPC.velocity *= 1.1f;
                            }
                        }
                        if (NPC.position.Y > Main.player[NPC.target].position.Y || Main.player[NPC.target].position.Y / 16f > Main.worldSurface || Main.player[NPC.target].dead)
                        {
                            flag21 = true;
                            if (Math.Abs(NPC.velocity.X) < speed / 2f)
                            {
                                if (NPC.velocity.X == 0f)
                                {
                                    NPC.velocity.X = NPC.velocity.X - NPC.direction;
                                }
                                NPC.velocity.X = NPC.velocity.X * 1.1f;
                            }
                            else
                            {
                                if (NPC.velocity.Y > -speed)
                                {
                                    NPC.velocity.Y = NPC.velocity.Y - turnSpeed;
                                }
                            }
                        }
                    }
                    if (!flag21)
                    {
                        if (NPC.velocity.X > 0f && num191 > 0f || NPC.velocity.X < 0f && num191 < 0f || NPC.velocity.Y > 0f && num192 > 0f || NPC.velocity.Y < 0f && num192 < 0f)
                        {
                            if (NPC.velocity.X < num191)
                            {
                                NPC.velocity.X = NPC.velocity.X + turnSpeed;
                            }
                            else
                            {
                                if (NPC.velocity.X > num191)
                                {
                                    NPC.velocity.X = NPC.velocity.X - turnSpeed;
                                }
                            }
                            if (NPC.velocity.Y < num192)
                            {
                                NPC.velocity.Y = NPC.velocity.Y + turnSpeed;
                            }
                            else
                            {
                                if (NPC.velocity.Y > num192)
                                {
                                    NPC.velocity.Y = NPC.velocity.Y - turnSpeed;
                                }
                            }
                            if (Math.Abs(num192) < speed * 0.2 && (NPC.velocity.X > 0f && num191 < 0f || NPC.velocity.X < 0f && num191 > 0f))
                            {
                                if (NPC.velocity.Y > 0f)
                                {
                                    NPC.velocity.Y = NPC.velocity.Y + turnSpeed * 2f;
                                }
                                else
                                {
                                    NPC.velocity.Y = NPC.velocity.Y - turnSpeed * 2f;
                                }
                            }
                            if (Math.Abs(num191) < speed * 0.2 && (NPC.velocity.Y > 0f && num192 < 0f || NPC.velocity.Y < 0f && num192 > 0f))
                            {
                                if (NPC.velocity.X > 0f)
                                {
                                    NPC.velocity.X = NPC.velocity.X + turnSpeed * 2f;
                                }
                                else
                                {
                                    NPC.velocity.X = NPC.velocity.X - turnSpeed * 2f;
                                }
                            }
                        }
                        else
                        {
                            if (num196 > num197)
                            {
                                if (NPC.velocity.X < num191)
                                {
                                    NPC.velocity.X = NPC.velocity.X + turnSpeed * 1.1f;
                                }
                                else if (NPC.velocity.X > num191)
                                {
                                    NPC.velocity.X = NPC.velocity.X - turnSpeed * 1.1f;
                                }
                                if (Math.Abs(NPC.velocity.X) + Math.Abs(NPC.velocity.Y) < speed * 0.5)
                                {
                                    if (NPC.velocity.Y > 0f)
                                    {
                                        NPC.velocity.Y = NPC.velocity.Y + turnSpeed;
                                    }
                                    else
                                    {
                                        NPC.velocity.Y = NPC.velocity.Y - turnSpeed;
                                    }
                                }
                            }
                            else
                            {
                                if (NPC.velocity.Y < num192)
                                {
                                    NPC.velocity.Y = NPC.velocity.Y + turnSpeed * 1.1f;
                                }
                                else if (NPC.velocity.Y > num192)
                                {
                                    NPC.velocity.Y = NPC.velocity.Y - turnSpeed * 1.1f;
                                }
                                if (Math.Abs(NPC.velocity.X) + Math.Abs(NPC.velocity.Y) < speed * 0.5)
                                {
                                    if (NPC.velocity.X > 0f)
                                    {
                                        NPC.velocity.X = NPC.velocity.X + turnSpeed;
                                    }
                                    else
                                    {
                                        NPC.velocity.X = NPC.velocity.X - turnSpeed;
                                    }
                                }
                            }
                        }
                    }
                }
                NPC.rotation = (float)Math.Atan2(NPC.velocity.Y, NPC.velocity.X) + 1.57f;
                if (head)
                {
                    if (flag18)
                    {
                        if (NPC.localAI[0] != 1f)
                        {
                            NPC.netUpdate = true;
                        }
                        NPC.localAI[0] = 1f;
                    }
                    else
                    {
                        if (NPC.localAI[0] != 0f)
                        {
                            NPC.netUpdate = true;
                        }
                        NPC.localAI[0] = 0f;
                    }
                    if ((NPC.velocity.X > 0f && NPC.oldVelocity.X < 0f || NPC.velocity.X < 0f && NPC.oldVelocity.X > 0f || NPC.velocity.Y > 0f && NPC.oldVelocity.Y < 0f || NPC.velocity.Y < 0f && NPC.oldVelocity.Y > 0f) && !NPC.justHit)
                    {
                        NPC.netUpdate = true;
                        return;
                    }
                }
            }

            if (!tail && (!Main.npc[(int)NPC.ai[0]].active || (NPC.Center - Main.npc[(int)NPC.ai[0]].Center).Length() > 4 * segmentLength) || !head && (!Main.npc[(int)NPC.ai[1]].active || (NPC.Center - Main.npc[(int)NPC.ai[1]].Center).Length() > 4 * segmentLength))
            {
                NPC.active = false;
            }

            CustomBehavior();
        }

        public virtual void Init()
        {
        }

        public virtual bool ShouldRun()
        {
            return false;
        }

        public virtual void CustomBehavior()
        {
        }

        public override bool? DrawHealthBar(byte hbPosition, ref float scale, ref Vector2 position)
        {
            return head ? null : false;
        }
    }
}