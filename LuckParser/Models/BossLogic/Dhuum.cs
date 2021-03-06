﻿using LuckParser.Models.DataModels;
using LuckParser.Models.ParseModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LuckParser.Models
{
    public class Dhuum : RaidLogic
    {
        public Dhuum()
        {
            MechanicList.AddRange(new List<Mechanic>
            {
            new Mechanic(48172, "Hateful Ephemera", Mechanic.MechType.SkillOnPlayer, ParseEnum.BossIDS.Dhuum, "symbol:'square',color:'rgb(255,140,0)',", "Glm.dmg","Hateful Ephemera (Golem AoE dmg)", "Golem Dmg",0), 
            new Mechanic(48121, "Arcing Affliction", Mechanic.MechType.SkillOnPlayer, ParseEnum.BossIDS.Dhuum, "symbol:'circle-open',color:'rgb(255,0,0)',", "B.dmg","Arcing Affliction (Bomb) hit", "Bomb dmg",0), 
            new Mechanic(47646, "Arcing Affliction", Mechanic.MechType.PlayerBoon, ParseEnum.BossIDS.Dhuum, "symbol:'circle',color:'rgb(255,0,0)',", "Bmb","Arcing Affliction (Bomb) application", "Bomb",0),
            //new Mechanic(47476, "Residual Affliction", Mechanic.MechType.PlayerBoon, ParseEnum.BossIDS.Dhuum, "symbol:'star-diamond',color:'rgb(255,200,0)',", "Bomb",0), //not needed, imho, applied at the same time as Arcing Affliction
            new Mechanic(47335, "Soul Shackle", Mechanic.MechType.PlayerOnPlayer, ParseEnum.BossIDS.Dhuum, "symbol:'diamond',color:'rgb(0,255,255)',", "Shckl","Soul Shackle (Tether) application", "Shackles",0),//  //also used for removal.
            new Mechanic(47164, "Soul Shackle", Mechanic.MechType.SkillOnPlayer, ParseEnum.BossIDS.Dhuum, "symbol:'diamond-open',color:'rgb(0,255,255)',", "Sh.Dmg","Soul Shackle (Tether) dmg ticks", "Shackles Dmg",0, (item => item.getDLog().GetDamage() > 0)),
            new Mechanic(47561, "Slash", Mechanic.MechType.SkillOnPlayer, ParseEnum.BossIDS.Dhuum, "symbol:'triangle',color:'rgb(0,128,0)',", "Cone","Boon ripping Cone Attack", "Cone",0),
            new Mechanic(48752, "Cull", Mechanic.MechType.SkillOnPlayer, ParseEnum.BossIDS.Dhuum, "symbol:'asterisk-open',color:'rgb(0,255,255)',", "Crk","Cull (Fearing Fissures)", "Cracks",0),
            new Mechanic(48760, "Putrid Bomb", Mechanic.MechType.SkillOnPlayer, ParseEnum.BossIDS.Dhuum, "symbol:'circle',color:'rgb(0,128,0)',", "Mrk","Necro Marks during Scythe attack", "Necro Marks",0), 
            new Mechanic(48398, "Cataclysmic Cycle", Mechanic.MechType.SkillOnPlayer, ParseEnum.BossIDS.Dhuum, "symbol:'circle-open',color:'rgb(255,140,0)',", "Sck.Dmg","Damage when sucked to close to middle", "Suck dmg",0),
            new Mechanic(48176, "Death Mark", Mechanic.MechType.SkillOnPlayer, ParseEnum.BossIDS.Dhuum, "symbol:'hexagon',color:'rgb(255,140,0)',", "Dip","Lesser Death Mark hit (Dip into ground)", "Dip AoE",0), 
            new Mechanic(48210, "Greater Death Mark", Mechanic.MechType.SkillOnPlayer, ParseEnum.BossIDS.Dhuum, "symbol:'circle',color:'rgb(255,140,0)',", "KB.Dmg","Knockback damage during Greater Deathmark (mid port)", "Knockback dmg",0),
          //  new Mechanic(48281, "Mortal Coil", Mechanic.MechType.PlayerBoon, ParseEnum.BossIDS.Dhuum, "symbol:'circle',color:'rgb(0,128,0)',", "Green Orbs",
            new Mechanic(46950, "Fractured Spirit", Mechanic.MechType.PlayerBoon, ParseEnum.BossIDS.Dhuum, "symbol:'square',color:'rgb(0,255,0)',", "Orb CD","Applied when taking green", "Green port",0), 
            new Mechanic(47076 , "Echo's Damage", Mechanic.MechType.SkillOnPlayer, ParseEnum.BossIDS.Dhuum, "symbol:'square',color:'rgb(255,0,0)',", "Echo","Damaged by Ender's Echo (pick up)", "Ender's Echo",5000),
            });
        }

        public override CombatReplayMap GetCombatMap()
        {
            return new CombatReplayMap("https://i.imgur.com/CLTwWBJ.png",
                            Tuple.Create(3763, 3383),
                            Tuple.Create(13524, -1334, 18039, 2735),
                            Tuple.Create(-21504, -12288, 24576, 12288),
                            Tuple.Create(19072, 15484, 20992, 16508));
        }

        public override List<PhaseData> GetPhases(Boss boss, ParsedLog log, List<CastLog> castLogs)
        {
            long start = 0;
            long end = 0;
            long fightDuration = log.FightData.FightDuration;
            List<PhaseData> phases = GetInitialPhase(log);
            // Sometimes the preevent is not in the evtc
            List<CastLog> dhuumCast = boss.GetCastLogs(log, 0, 20000);
            if (dhuumCast.Count > 0)
            {
                CastLog shield = castLogs.Find(x => x.GetID() == 47396);
                if (shield != null)
                {
                    end = shield.GetTime();
                    phases.Add(new PhaseData(start, end));
                    start = shield.GetTime() + shield.GetActDur();
                    if (start < fightDuration - 5000)
                    {
                        phases.Add(new PhaseData(start, fightDuration));
                    }
                }
                if (fightDuration - start > 5000 && start >= phases.Last().End)
                {
                    phases.Add(new PhaseData(start, fightDuration));
                }
                string[] namesDh = new [] { "Main Fight", "Ritual" };
                for (int i = 1; i < phases.Count; i++)
                {
                    phases[i].Name = namesDh[i - 1];
                }
            }
            else
            {
                CombatItem invulDhuum = log.GetBoonData(762).FirstOrDefault(x => x.IsBuffRemove != ParseEnum.BuffRemove.None && x.SrcInstid == boss.InstID && x.Time > 115000 + log.FightData.FightStart);
                if (invulDhuum != null)
                {
                    end = invulDhuum.Time - log.FightData.FightStart;
                    phases.Add(new PhaseData(start, end));
                    start = end + 1;
                    CastLog shield = castLogs.Find(x => x.GetID() == 47396);
                    if (shield != null)
                    {
                        end = shield.GetTime();
                        phases.Add(new PhaseData(start, end));
                        start = shield.GetTime() + shield.GetActDur();
                        if (start < fightDuration - 5000)
                        {
                            phases.Add(new PhaseData(start, fightDuration));
                        }
                    }
                }
                if (fightDuration - start > 5000 && start >= phases.Last().End)
                {
                    phases.Add(new PhaseData(start, fightDuration));
                }
                string[] namesDh = new [] { "Roleplay", "Main Fight", "Ritual" };
                for (int i = 1; i < phases.Count; i++)
                {
                    phases[i].Name = namesDh[i - 1];
                }
            }
            return phases;
        }

        public override List<ParseEnum.ThrashIDS> GetAdditionalData(CombatReplay replay, List<CastLog> cls, ParsedLog log)
        {
            // TODO: facing information (pull thingy)
            List<ParseEnum.ThrashIDS> ids = new List<ParseEnum.ThrashIDS>
                    {
                        ParseEnum.ThrashIDS.Echo,
                        ParseEnum.ThrashIDS.Enforcer,
                        ParseEnum.ThrashIDS.Messenger
                    };
            List<CastLog> deathmark = cls.Where(x => x.GetID() == 48176).ToList();
            CastLog majorSplit = cls.Find(x => x.GetID() == 47396);
            foreach (CastLog c in deathmark)
            {
                int start = (int)c.GetTime();
                int castEnd = start + c.GetActDur();
                int zoneEnd = castEnd + 120000;
                if (majorSplit != null)
                {
                    castEnd = Math.Min(castEnd, (int)majorSplit.GetTime());
                    zoneEnd = Math.Min(zoneEnd, (int)majorSplit.GetTime());
                }
                Point3D pos = replay.GetPositions().FirstOrDefault(x => x.Time > castEnd);
                if (pos != null)
                {
                    replay.AddCircleActor(new CircleActor(true, castEnd, 450, new Tuple<int, int>(start, castEnd), "rgba(200, 255, 100, 0.5)", pos));
                    replay.AddCircleActor(new CircleActor(false, 0, 450, new Tuple<int, int>(start, castEnd), "rgba(200, 255, 100, 0.5)", pos));
                    replay.AddCircleActor(new CircleActor(true, 0, 450, new Tuple<int, int>(castEnd, zoneEnd), "rgba(200, 255, 100, 0.5)", pos));
                }
            }
            List<CastLog> cataCycle = cls.Where(x => x.GetID() == 48398).ToList();
            foreach (CastLog c in cataCycle)
            {
                int start = (int)c.GetTime();
                int end = start + c.GetActDur();
                replay.AddCircleActor(new CircleActor(true, end, 300, new Tuple<int, int>(start, end), "rgba(255, 150, 0, 0.7)"));
                replay.AddCircleActor(new CircleActor(true, 0, 300, new Tuple<int, int>(start, end), "rgba(255, 150, 0, 0.5)"));
            }
            if (majorSplit != null)
            {
                int start = (int)majorSplit.GetTime();
                int end = (int)log.FightData.FightDuration;
                replay.AddCircleActor(new CircleActor(true, 0, 320, new Tuple<int, int>(start, end), "rgba(0, 180, 255, 0.2)"));
            }
            return ids;
        }

        public override void GetAdditionalPlayerData(CombatReplay replay, Player p, ParsedLog log)
        {
            // spirit transform
            List<CombatItem> spiritTransform = log.GetBoonData(46950).Where(x => x.DstInstid == p.InstID && x.IsBuffRemove == ParseEnum.BuffRemove.None).ToList();
            foreach (CombatItem c in spiritTransform)
            {
                int duration = 15000;
                int start = (int)(c.Time - log.FightData.FightStart);
                if (log.FightData.HealthOverTime.FirstOrDefault(x => x.X > start).Y < 1050)
                {
                    duration = 30000;
                }
                CombatItem removedBuff = log.GetBoonData(48281).FirstOrDefault(x => x.SrcInstid == p.InstID && x.IsBuffRemove == ParseEnum.BuffRemove.All && x.Time > c.Time && x.Time < c.Time + duration);
                int end = start + duration;
                if (removedBuff != null)
                {
                    end = (int)(removedBuff.Time - log.FightData.FightStart);
                }
                replay.AddCircleActor(new CircleActor(true, 0, 100, new Tuple<int, int>(start, end), "rgba(0, 50, 200, 0.3)"));
                replay.AddCircleActor(new CircleActor(true, start + duration, 100, new Tuple<int, int>(start, end), "rgba(0, 50, 200, 0.5)"));
            }
            // bomb
            List<CombatItem> bombDhuum = GetFilteredList(log, 47646, p.InstID);
            int bombDhuumStart = 0;
            foreach (CombatItem c in bombDhuum)
            {
                if (c.IsBuffRemove == ParseEnum.BuffRemove.None)
                {
                    bombDhuumStart = (int)(c.Time - log.FightData.FightStart);
                }
                else
                {
                    int bombDhuumEnd = (int)(c.Time - log.FightData.FightStart);
                    replay.AddCircleActor(new CircleActor(true, 0, 100, new Tuple<int, int>(bombDhuumStart, bombDhuumEnd), "rgba(80, 180, 0, 0.3)"));
                    replay.AddCircleActor(new CircleActor(true, bombDhuumStart + 13000, 100, new Tuple<int, int>(bombDhuumStart, bombDhuumEnd), "rgba(80, 180, 0, 0.5)"));
                }
            }
        }

        public override int IsCM(List<CombatItem> clist, int health)
        {
            return (health > 35e6) ? 1 : 0;
        }

        public override string GetReplayIcon()
        {
            return "https://i.imgur.com/RKaDon5.png";
        }
    }
}
