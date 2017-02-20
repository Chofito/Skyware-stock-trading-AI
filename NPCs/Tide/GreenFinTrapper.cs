using Terraria;
using System;
using Terraria.ID;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Terraria.ModLoader;

namespace SpiritMod.NPCs.Tide
{
    public class GreenFinTrapper : ModNPC
    {

        public override void SetDefaults()
        {
            npc.name = "Greenfin Trapper";
            npc.displayName = "Greenfin Trapper";
            npc.width = 80;
            npc.height = 52;
            npc.damage = 26;
            npc.defense = 9;
            npc.lifeMax = 132;
            npc.HitSound = SoundID.NPCHit12;
            npc.DeathSound = SoundID.NPCDeath8;
            npc.value = 2329f;
            npc.knockBackResist = .30f;
            npc.aiStyle = 26;
            aiType = NPCID.Wolf;
            Main.npcFrameCount[npc.type] = 7;

        }
        public override void NPCLoot()
        {
            InvasionWorld.invasionSize -= 1;
            if (InvasionWorld.invasionSize < 0)
                InvasionWorld.invasionSize = 0;
            if (Main.netMode != 1)
                InvasionHandler.ReportInvasionProgress(InvasionWorld.invasionSizeStart - InvasionWorld.invasionSize, InvasionWorld.invasionSizeStart, 0);
            if (Main.netMode != 2)
                return;
            NetMessage.SendData(78, -1, -1, "", InvasionWorld.invasionProgress, (float)InvasionWorld.invasionProgressMax, (float)Main.invasionProgressIcon, 0.0f, 0, 0, 0);
        }

        public override float CanSpawn(NPCSpawnInfo spawnInfo)
        {
            if (InvasionWorld.invasionType == SpiritMod.customEvent)
                return 5f;

            return 0;
        }
        public override void OnHitPlayer(Player target, int damage, bool crit)
        {
            if (Main.rand.Next(8) == 1)
            {
                target.AddBuff(mod.BuffType("Trapped"), 120);
            }
        }
        public override void FindFrame(int frameHeight)
        {
            npc.frameCounter += 0.25f;
            npc.frameCounter %= Main.npcFrameCount[npc.type];
            int frame = (int)npc.frameCounter;
            npc.frame.Y = frame * frameHeight;
        }
        public override void AI()
        {
            npc.spriteDirection = npc.direction;
        }
    }
}
