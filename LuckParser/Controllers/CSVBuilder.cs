﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using LuckParser.Models.DataModels;
using LuckParser.Models.ParseModels;

namespace LuckParser.Controllers
{
    class CSVBuilder
    {
        readonly SettingsContainer _settings;

        readonly ParsedLog _log;

        readonly Statistics _statistics;
        readonly StreamWriter _sw;
        readonly string _delimiter;
        public static void UpdateStatisticSwitches(StatisticsCalculator.Switches switches)
        {
            switches.CalculateBoons = true;
            switches.CalculateDPS = true;
            switches.CalculateConditions = true;
            switches.CalculateDefense = true;
            switches.CalculateStats = true;
            switches.CalculateSupport = true;
            switches.CalculateCombatReplay = true;
            switches.CalculateMechanics = true;
        }

        public CSVBuilder(StreamWriter sw, String delimiter,ParsedLog log, SettingsContainer settings, Statistics statistics)
        {
            _log = log;
            _sw = sw;
            _delimiter = delimiter;
            _settings = settings;

            _statistics = statistics;
        }
        private void WriteCell(string content)
        {
            _sw.Write(content + _delimiter, Encoding.GetEncoding(1252));
        }
        private void WriteCells(List<string> content)
        {
            foreach (string cont in content)
            {
                _sw.Write(cont + _delimiter, Encoding.GetEncoding(1252));
            }
        }
        private void WriteCells(string[] content)
        {
            foreach (string cont in content)
            {
                _sw.Write(cont + _delimiter, Encoding.GetEncoding(1252));
            }
        }
        private void NewLine()
        {
            _sw.Write("\r\n", Encoding.GetEncoding(1252));
        }
        private void WriteLine(string[] content)
        {
            foreach (string cont in content)
            {
                _sw.Write(cont + _delimiter, Encoding.GetEncoding(1252));
            }
            NewLine();
        }
        //Creating CSV---------------------------------------------------------------------------------
        public void CreateCSV()
        {       
            double fightDuration = (_log.FightData.FightDuration) / 1000.0;
            TimeSpan duration = TimeSpan.FromSeconds(fightDuration);
            string durationString = duration.ToString("mm") + "m " + duration.ToString("ss") + "s";
            if (duration.ToString("hh") != "00")
            {
                durationString = duration.ToString("hh") + "h " + durationString;
            }
            string bossname = _log.FightData.Name;
            //header
            WriteLine(new [] { "Elite Insights Version", Application.ProductVersion });
            WriteLine(new [] { "ARC Version", _log.LogData.BuildVersion});
            WriteLine(new [] { "Boss ID", _log.FightData.ID.ToString() });
            WriteLine(new [] { "Recorded By", _log.LogData.PoV.Split(':')[0] });
            WriteLine(new [] { "Time Start", _log.LogData.LogStart });
            WriteLine(new [] { "Time End", _log.LogData.LogEnd });
            NewLine();
            NewLine();
            NewLine();
            NewLine();
            //Boss card
            WriteLine(new [] { "Boss", bossname });
            WriteLine(new [] { "Success", _log.LogData.Success.ToString() });
            WriteLine(new [] { "Total Boss Health", _log.FightData.Health.ToString() });
            int finalBossHealth = _log.FightData.HealthOverTime.Count > 0 ? _log.FightData.HealthOverTime.Last().Y : 10000;
            WriteLine(new [] { "Final Boss Health", (_log.FightData.Health * (100.0 - finalBossHealth * 0.01)).ToString() });
            WriteLine(new [] { "Boss Health Burned %", (100.0 - finalBossHealth * 0.01).ToString() });
            WriteLine(new [] { "Duration", durationString });

            //DPSStats
            CreateDPSTable(0);

            //DMGStatsBoss
            CreateBossDMGStatsTable(0);

            //DMGStats All
            CreateDmgStatsTable(0);

            //Defensive Stats
            CreateDefTable(0);

            //Support Stats
            CreateSupTable(0);

            // boons
            CreateUptimeTable(_statistics.PresentBoons, 0);

            //boonGenSelf
            CreateGenSelfTable(_statistics.PresentBoons, 0);

            // boonGenGroup
            CreateGenGroupTable(_statistics.PresentBoons, 0);

            // boonGenOGroup
            CreateGenOGroupTable(_statistics.PresentBoons, 0);

            //  boonGenSquad
            CreateGenSquadTable(_statistics.PresentBoons, 0);

            //Offensive Buffs stats
            // boons
            CreateUptimeTable(_statistics.PresentOffbuffs, 0);

            //boonGenSelf
            CreateGenSelfTable(_statistics.PresentOffbuffs, 0);

            // boonGenGroup
            CreateGenGroupTable(_statistics.PresentOffbuffs, 0);

            // boonGenOGroup
            CreateGenOGroupTable(_statistics.PresentOffbuffs, 0);

            //  boonGenSquad
            CreateGenSquadTable(_statistics.PresentOffbuffs, 0);

            //Defensive Buffs stats
            // boons
            CreateUptimeTable(_statistics.PresentDefbuffs, 0);

            //boonGenSelf
            CreateGenSelfTable(_statistics.PresentDefbuffs, 0);

            // boonGenGroup
            CreateGenGroupTable(_statistics.PresentDefbuffs, 0);

            // boonGenOGroup
            CreateGenOGroupTable(_statistics.PresentDefbuffs, 0);

            //  boonGenSquad
            CreateGenSquadTable(_statistics.PresentDefbuffs, 0);

            //Mechanics
            CreateMechanicTable(0);

            //Mech List
            CreateMechList(0);

            //Condi Uptime
            CreateCondiUptime(0);
            //Condi Gen
            CreateCondiGen(0);
            //Boss boons
            CreateBossBoonUptime(0);
        }
        private void CreateDPSTable(int phaseIndex)
        {
            PhaseData phase = _statistics.Phases[phaseIndex];
            if (phase.Redirection.Count > 0)
            {
                WriteLine(new [] { "Sub Group", "Profession","Role","Name","Account","WepSet1_1","WepSet1_2","WepSet2_1","WepSet2_2",
                "Adds DPS","Adds DMG","Adds Power DPS","Adds Power DMG","Adds Condi DPS","Adds Condi DMG",
                "All DPS","All DMG","All Power DPS","All Power DMG","All Condi DPS","All Condi DMG",
                "Times Downed", "Time Died","Percent Alive"});
            }
            else
            {
                WriteLine(new [] { "Sub Group", "Profession","Role","Name","Account","WepSet1_1","WepSet1_2","WepSet2_1","WepSet2_2",
                "Boss DPS","Boss DMG","Boss Power DPS","Boss Power DMG","Boss Condi DPS","Boss Condi DMG",
                "All DPS","All DMG","All Power DPS","All Power DMG","All Condi DPS","All Condi DMG",
                "Times Downed", "Time Died","Percent Alive"});
            }
            int count = 0;
            foreach (Player player in _log.PlayerList)
            {
                Statistics.FinalDPS dps = _statistics.Dps[player][phaseIndex];
                Statistics.FinalStats stats = _statistics.Stats[player][phaseIndex];
                TimeSpan timedead = TimeSpan.FromMilliseconds(stats.Died);
                long fightDuration = phase.GetDuration("s");
                string[] wep = player.GetWeaponsArray(_log);
                string build = "";
                if (player.Condition > 0)
                {
                    build += " Condi:" + player.Condition;
                }
                if (player.Concentration > 0)
                {
                    build += " Concentration:" + player.Concentration;
                }
                if (player.Healing > 0)
                {
                    build += " Healing:" + player.Healing;
                }
                if (player.Toughness > 0)
                {
                    build += " Toughness:" + player.Toughness;
                }
                WriteLine(new [] { player.Group.ToString(), player.Prof,build,player.Character, player.Account.TrimStart(':') ,wep[0],wep[1],wep[2],wep[3],
                dps.BossDps.ToString(),dps.BossDamage.ToString(),dps.BossPowerDps.ToString(),dps.BossPowerDamage.ToString(),dps.BossCondiDps.ToString(),dps.BossCondiDamage.ToString(),
                dps.AllDps.ToString(),dps.AllDamage.ToString(),dps.AllPowerDps.ToString(),dps.AllPowerDamage.ToString(),dps.AllCondiDps.ToString(),dps.AllCondiDamage.ToString(),
                stats.DownCount.ToString(), timedead.Minutes + " m " + timedead.Seconds + " s",Math.Round((timedead.TotalSeconds / fightDuration) * 100,1) +"%"});
                count++;
            }
            while (count < 15)//so each graph has equal spacing
            {
                NewLine();
                count++;
            }
        }
        private void CreateBossDMGStatsTable(int phaseIndex)
        {
            //generate dmgstats table=
            WriteLine(new [] { "Sub Group", "Profession", "Name" ,
                "Critical%","Critical hits","Critical DMG",
                "Scholar%","Scholar hits","Scholar DMG","Scholar % increase",
                "Moving%","Moving Hits","Moving DMG","Moving % increase",
                "Flanking%","Flanking hits",
                "Glancing%","Glancing Hits",
                "Blind%","Blind Hits",
                "Total Hits",
                "Hits to Interupt","Hits Invulned","Time wasted","Time saved","Weapon Swaps"});
            int count = 0;
            foreach (Player player in _log.PlayerList)
            {
                Statistics.FinalStats stats = _statistics.Stats[player][phaseIndex];
                Statistics.FinalDPS dps = _statistics.Dps[player][phaseIndex];

                WriteLine(new [] { player.Group.ToString(), player.Prof, player.Character,
                Math.Round((Double)(stats.CriticalRateBoss) / stats.CritablePowerLoopCountBoss * 100,1).ToString(), stats.CriticalRateBoss.ToString(),stats.CriticalDmgBoss.ToString(),
                Math.Round((Double)(stats.ScholarRateBoss) / stats.PowerLoopCountBoss * 100,1).ToString(),stats.ScholarRateBoss.ToString(),stats.ScholarDmgBoss.ToString(),Math.Round(100.0 * (dps.PlayerBossPowerDamage / (Double)(dps.PlayerBossPowerDamage - stats.ScholarDmgBoss) - 1.0), 3).ToString(),
                Math.Round((Double)(stats.MovingRateBoss) / stats.PowerLoopCountBoss * 100,1).ToString(),stats.MovingRateBoss.ToString(),stats.MovingDamageBoss.ToString(),Math.Round(100.0 * (dps.PlayerBossPowerDamage / (Double)(dps.PlayerBossPowerDamage - stats.MovingDamageBoss) - 1.0), 3).ToString(),
                Math.Round(stats.FlankingRateBoss / (Double)stats.PowerLoopCountBoss * 100,1).ToString(),stats.FlankingRateBoss.ToString(),
                Math.Round(stats.GlanceRateBoss / (Double)stats.PowerLoopCountBoss * 100,1).ToString(),stats.GlanceRateBoss.ToString(),
                Math.Round(stats.MissedBoss / (Double)stats.PowerLoopCountBoss * 100,1).ToString(),stats.MissedBoss.ToString(),
                stats.PowerLoopCountBoss.ToString(),
                stats.InterruptsBoss.ToString(),stats.InvulnedBoss.ToString(),stats.TimeWasted.ToString(),stats.TimeSaved.ToString(),stats.SwapCount.ToString() });
                count++;
            }
            while (count < 15)//so each graph has equal spacing
            {
                NewLine();
                count++;
            }
        }
        private void CreateDmgStatsTable(int phaseIndex)
        {
            //generate dmgstats table
            WriteLine(new [] { "Sub Group", "Profession", "Name" ,
                "Critical%","Critical hits","Critical DMG",
                "Scholar%","Scholar hits","Scholar DMG","Scholar % increase",
                "Moving%","Moving Hits","Moving DMG","Moving % increase",
                "Flanking%","Flanking hits",
                "Glancing%","Glancing Hits",
                "Blind%","Blind Hits",
                "Total Hits",
                "Hits to Interupt","Hits Invulned","Time wasted","Time saved","Weapon Swaps"});
            int count = 0;
            foreach (Player player in _log.PlayerList)
            {
                Statistics.FinalStats stats = _statistics.Stats[player][phaseIndex];
                Statistics.FinalDPS dps = _statistics.Dps[player][phaseIndex];

                WriteLine(new [] { player.Group.ToString(), player.Prof, player.Character,
                Math.Round((Double)(stats.CriticalRate) / stats.CritablePowerLoopCount * 100,1).ToString(), stats.CriticalRate.ToString(),stats.CriticalDmg.ToString(),
                Math.Round((Double)(stats.ScholarRate) / stats.PowerLoopCount * 100,1).ToString(),stats.ScholarRate.ToString(),stats.ScholarDmg.ToString(),Math.Round(100.0 * (dps.PlayerPowerDamage / (Double)(dps.PlayerPowerDamage - stats.ScholarDmg) - 1.0), 3).ToString(),
                Math.Round((Double)(stats.MovingRate) / stats.PowerLoopCount * 100,1).ToString(),stats.MovingRate.ToString(),stats.MovingDamage.ToString(),Math.Round(100.0 * (dps.PlayerPowerDamage / (Double)(dps.PlayerPowerDamage - stats.MovingDamage) - 1.0), 3).ToString(),
                Math.Round(stats.FlankingRate / (Double)stats.PowerLoopCount * 100,1).ToString(),stats.FlankingRate.ToString(),
                Math.Round(stats.GlanceRate / (Double)stats.PowerLoopCount * 100,1).ToString(),stats.GlanceRate.ToString(),
                Math.Round(stats.Missed / (Double)stats.PowerLoopCount * 100,1).ToString(),stats.Missed.ToString(),
                stats.PowerLoopCount.ToString(),
                stats.Interrupts.ToString(),stats.Invulned.ToString(),stats.TimeWasted.ToString(),stats.TimeSaved.ToString(),stats.SwapCount.ToString() });
                count++;
            }
            while (count < 15)//so each graph has equal spacing
            {
                NewLine();
                count++;
            }
        }
        private void CreateDefTable(int phaseIndex)
        {
            //generate defstats table
            WriteLine(new [] { "Sub Group", "Profession", "Name" ,
                "DMG Taken","DMG Barrier","Blocked","Invulned","Evaded","Dodges" });
            int count = 0;
            foreach (Player player in _log.PlayerList)
            {
                Statistics.FinalDefenses defenses = _statistics.Defenses[player][phaseIndex];
                Statistics.FinalStats stats = _statistics.Stats[player][phaseIndex];

                WriteLine(new [] { player.Group.ToString(), player.Prof, player.Character,
                defenses.DamageTaken.ToString(),defenses.DamageBarrier.ToString(),defenses.BlockedCount.ToString(),defenses.InvulnedCount.ToString(),defenses.EvadedCount.ToString(),stats.DodgeCount.ToString() });
                count++;
            }
            while (count < 15)//so each graph has equal spacing
            {
                NewLine();
                count++;
            }
        }
        private void CreateSupTable(int phaseIndex)
        {
            //generate supstats table
            WriteLine(new [] { "Sub Group", "Profession", "Name" ,
                "Condi Cleanse","Condi Cleanse time","Resurrects","Time Resurecting" });
            int count = 0;
            foreach (Player player in _log.PlayerList)
            {
                Statistics.FinalSupport support = _statistics.Support[player][phaseIndex];

                WriteLine(new [] { player.Group.ToString(), player.Prof, player.Character,
                support.CondiCleanse.ToString(),support.CondiCleanseTime.ToString(),support.Resurrects.ToString(),support.ResurrectTime.ToString() });
                count++;
            }
            while (count < 15)//so each graph has equal spacing
            {
                NewLine();
                count++;
            }
        }
        private void CreateUptimeTable(List<Boon> listToUse, int phaseIndex)
        {
            //generate Uptime Table table
            PhaseData phase = _statistics.Phases[phaseIndex];
            long fightDuration = phase.GetDuration();

            WriteCells(new [] { "Name", "Avg Boons" });
            foreach (Boon boon in listToUse)
            {
                WriteCell(boon.Name);

            }
            NewLine();

            int count = 0;
            foreach (Player player in _log.PlayerList)
            {
                Dictionary<long, Statistics.FinalBoonUptime> boons = _statistics.SelfBoons[player][phaseIndex];
                Dictionary<long, long> boonPresence = player.GetBoonPresence(_log, phaseIndex);
                double avgBoons = 0.0;
                foreach (long duration in boonPresence.Values)
                {
                    avgBoons += duration;
                }
                avgBoons /= fightDuration;

                WriteCell(player.Character);
                WriteCell(Math.Round(avgBoons, 1).ToString());
                foreach (Boon boon in listToUse)
                {
                    if (boons.ContainsKey(boon.ID))
                    {

                        if (boon.Type == Boon.BoonType.Duration)
                        {
                            WriteCell(boons[boon.ID].Uptime + "%");
                        }
                        else if (boon.Type == Boon.BoonType.Intensity)
                        {
                            WriteCell(boons[boon.ID].Uptime.ToString());
                        }

                    }
                    else
                    {
                        WriteCell("0");

                    }
                }
                NewLine();
                count++;
            }
            while (count < 15)//so each graph has equal spacing
            {
                NewLine();
                count++;
            }
        }
        private void CreateGenSelfTable(List<Boon> listToUse, int phaseIndex)
        {
            //generate Uptime Table table
            WriteCell("Name");
            foreach (Boon boon in listToUse)
            {
                WriteCell(boon.Name);
                WriteCell(boon.Name + " Overstack");
            }
            NewLine();

            int count = 0;
            foreach (Player player in _log.PlayerList)
            {
                Dictionary<long, Statistics.FinalBoonUptime> uptimes = _statistics.SelfBoons[player][phaseIndex];

                WriteCell(player.Character);
                foreach (Boon boon in listToUse)
                {
                    string rate = "0";
                    string overstack = "0";
                    Statistics.FinalBoonUptime uptime = uptimes[boon.ID];
                    if (uptime.Generation > 0 || uptime.Overstack > 0)
                    {
                        if (boon.Type == Boon.BoonType.Duration)
                        {
                            rate = uptime.Generation.ToString() + "%";
                            overstack = uptime.Overstack.ToString() + "%";
                        }
                        else if (boon.Type == Boon.BoonType.Intensity)
                        {
                            rate = uptime.Generation.ToString();
                            overstack = uptime.Overstack.ToString();
                        }

                    }
                    WriteCell(rate);
                    WriteCell(overstack);
                }
                NewLine();
                count++;
            }
            while (count < 15)//so each graph has equal spacing
            {
                NewLine();
                count++;
            }
        }
        private void CreateGenGroupTable(List<Boon> listToUse, int phaseIndex)
        {
            //generate Uptime Table table
            WriteCell("Name");
            foreach (Boon boon in listToUse)
            {
                WriteCell(boon.Name);
                WriteCell(boon.Name + " Overstack");
            }
            NewLine();

            int count = 0;
            foreach (Player player in _log.PlayerList)
            {
                Dictionary<long, Statistics.FinalBoonUptime> boons =
                            _statistics.GroupBoons[player][phaseIndex];

                WriteCell(player.Character);
                foreach (Boon boon in listToUse)
                {
                    string rate = "0";
                    string overstack = "0";
                    Statistics.FinalBoonUptime uptime = boons[boon.ID];
                    if (uptime.Generation > 0 || uptime.Overstack > 0)
                    {
                        if (boon.Type == Boon.BoonType.Duration)
                        {
                            rate = uptime.Generation.ToString() + "%";
                            overstack = uptime.Overstack.ToString() + "%";
                        }
                        else if (boon.Type == Boon.BoonType.Intensity)
                        {
                            rate = uptime.Generation.ToString();
                            overstack = uptime.Overstack.ToString();
                        }

                    }
                    WriteCell(rate);
                    WriteCell(overstack);
                }
                NewLine();
                count++;
            }
            while (count < 15)//so each graph has equal spacing
            {
                NewLine();
                count++;
            }
        }
        private void CreateGenOGroupTable(List<Boon> listToUse, int phaseIndex)
        {
            //generate Uptime Table table
            WriteCell("Name");
            foreach (Boon boon in listToUse)
            {
                WriteCell(boon.Name);
                WriteCell(boon.Name + " Overstack");
            }
            NewLine();

            int count = 0;
            foreach (Player player in _log.PlayerList)
            {
                Dictionary<long, Statistics.FinalBoonUptime> boons =
                              _statistics.OffGroupBoons[player][phaseIndex];

                WriteCell(player.Character);
                foreach (Boon boon in listToUse)
                {
                    string rate = "0";
                    string overstack = "0";
                    Statistics.FinalBoonUptime uptime = boons[boon.ID];
                    if (uptime.Generation > 0 || uptime.Overstack > 0)
                    {
                        if (boon.Type == Boon.BoonType.Duration)
                        {
                            rate = uptime.Generation.ToString() + "%";
                            overstack = uptime.Overstack.ToString() + "%";
                        }
                        else if (boon.Type == Boon.BoonType.Intensity)
                        {
                            rate = uptime.Generation.ToString();
                            overstack = uptime.Overstack.ToString();
                        }

                    }
                    WriteCell(rate);
                    WriteCell(overstack);
                }
                NewLine();
                count++;
            }
            while (count < 15)//so each graph has equal spacing
            {
                NewLine();
                count++;
            }
        }
        private void CreateGenSquadTable(List<Boon> listToUse, int phaseIndex)
        {
            //generate Uptime Table table
            WriteCell("Name");
            foreach (Boon boon in listToUse)
            {
                WriteCell(boon.Name);
                WriteCell(boon.Name + " Overstack");
            }
            NewLine();

            int count = 0;
            foreach (Player player in _log.PlayerList)
            {
                Dictionary<long, Statistics.FinalBoonUptime> boons =
                            _statistics.SquadBoons[player][phaseIndex];
                WriteCell(player.Character);
                foreach (Boon boon in listToUse)
                {
                    string rate = "0";
                    string overstack = "0";
                    Statistics.FinalBoonUptime uptime = boons[boon.ID];
                    if (uptime.Generation > 0 || uptime.Overstack > 0)
                    {
                        if (boon.Type == Boon.BoonType.Duration)
                        {
                            rate = uptime.Generation.ToString() + "%";
                            overstack = uptime.Overstack.ToString() + "%";
                        }
                        else if (boon.Type == Boon.BoonType.Intensity)
                        {
                            rate = uptime.Generation.ToString();
                            overstack = uptime.Overstack.ToString();
                        }

                    }
                    WriteCell(rate);
                    WriteCell(overstack);
                }
                NewLine();
                count++;
            }
            while (count < 15)//so each graph has equal spacing
            {
                NewLine();
                count++;
            }
        }
        private void CreateMechanicTable(int phaseIndex)
        {
            HashSet<Mechanic> presMech = _log.MechanicData.GetPresentPlayerMechs(phaseIndex);
            //Dictionary<string, HashSet<Mechanic>> presEnemyMech = log.MechanicData.getPresentEnemyMechs(phaseIndex);
            PhaseData phase = _statistics.Phases[phaseIndex];
            //List<AbstractMasterPlayer> enemyList = log.MechanicData.getEnemyList(phaseIndex);
            int countLines = 0;
            if (presMech.Count > 0)
            {
                WriteCell("Name");
                foreach (Mechanic mech in presMech)
                {
                    WriteCell(mech.GetDescription());
                }
                NewLine();

                foreach (Player p in _log.PlayerList)
                {
                    WriteCell(p.Character);
                    foreach (Mechanic mech in presMech)
                    {
                        int count = _log.MechanicData[mech].Count(x => x.GetPlayer().InstID == p.InstID && phase.InInterval(x.GetTime()));
                        WriteCell(count.ToString());
                    }
                    NewLine();
                    countLines++;

                }

            }
            while (countLines < 15)//so each graph has equal spacing
            {
                NewLine();
                countLines++;
            }
        }
        private void CreateMechList(int phaseIndex)
        {
            MechanicData mData = _log.MechanicData;
            List<MechanicLog> mLogs = new List<MechanicLog>();
            foreach (List<MechanicLog> mLs in mData.Values)
            {
                mLogs.AddRange(mLs);
            }
            mLogs = mLogs.OrderBy(x => x.GetTime()).ToList();
            int count = 0;
            WriteCell("Time");
            foreach (MechanicLog m in mLogs)
            {
                WriteCell((m.GetTime() / 1000f).ToString());
            }
            NewLine();
            count++;
            WriteCell("Player");
            foreach (MechanicLog m in mLogs)
            {
                WriteCell(m.GetPlayer().Character);
            }
            NewLine();
            count++;
            WriteCell("Mechanic");
            foreach (MechanicLog m in mLogs)
            {
                WriteCell(m.GetDescription());
            }
            NewLine();
            count++;
            while (count < 15)//so each graph has equal spacing
            {
                NewLine();
                count++;
            }
        }
        private void CreateCondiUptime(int phaseIndex)
        {
            Boss boss = _log.Boss;
            List<PhaseData> phases = _statistics.Phases;
            long fightDuration = phases[phaseIndex].GetDuration();
            Dictionary<long, Statistics.FinalBossBoon> conditions = _statistics.BossConditions[phaseIndex];
            Dictionary<long, long> condiPresence = boss.GetCondiPresence(_log, phaseIndex);
            double avgCondis = 0.0;
            foreach (long duration in condiPresence.Values)
            {
                avgCondis += duration;
            }
            avgCondis /= fightDuration;


            WriteCell("Name");
            WriteCell("Avg");
            foreach (Boon boon in _statistics.PresentConditions)
            {
                if (boon.Name == "Retaliation")
                {
                    continue;
                }
                WriteCell(boon.Name);
            }

            NewLine();
            int count = 0;
            WriteCell(boss.Character);
            WriteCell(Math.Round(avgCondis, 1).ToString());
            foreach (Boon boon in _statistics.PresentConditions)
            {
                if (boon.Name == "Retaliation")
                {
                    continue;
                }
                if (boon.Type == Boon.BoonType.Duration)
                {
                    WriteCell(conditions[boon.ID].Uptime.ToString() + "%");
                }
                else
                {
                    WriteCell(conditions[boon.ID].Uptime.ToString());
                }
            }
            count++;

            while (count < 15)//so each graph has equal spacing
            {
                NewLine();
                count++;
            }
        }
        private void CreateBossBoonUptime(int phaseIndex)
        {
            Boss boss = _log.Boss;
            List<PhaseData> phases = _statistics.Phases;
            Dictionary<long, Statistics.FinalBossBoon> conditions = _statistics.BossConditions[phaseIndex];
            WriteCell("Name");
            WriteCell("Avg");
            foreach (Boon boon in _statistics.PresentBoons)
            {
                WriteCell(boon.Name);
            }

            NewLine();
            int count = 0;
            WriteCell(boss.Character);
            foreach (Boon boon in _statistics.PresentBoons)
            {
                if (boon.Type == Boon.BoonType.Duration)
                {
                    WriteCell(conditions[boon.ID].Uptime.ToString() + "%");
                }
                else
                {
                    WriteCell(conditions[boon.ID].Uptime.ToString());
                }
            }
            count++;

            while (count < 15)//so each graph has equal spacing
            {
                NewLine();
                count++;
            }
        }
        private void CreateCondiGen(int phaseIndex)
        {
            List<PhaseData> phases = _statistics.Phases;
            Dictionary<long, Statistics.FinalBossBoon> conditions = _statistics.BossConditions[phaseIndex];
            //bool hasBoons = false;
            int count = 0;
            WriteCell("Name");
            foreach (Boon boon in _statistics.PresentConditions)
            {
                if (boon.Name == "Retaliation")
                {
                    continue;
                }
                WriteCell(boon.Name);
                WriteCell(boon.Name + " Overstack");
            }
            NewLine();
            foreach (Player player in _log.PlayerList)
            {
                WriteCell(player.Character);
                foreach (Boon boon in _statistics.PresentConditions)
                {
                    if (boon.Name == "Retaliation")
                    {
                        continue;
                    }
                    if (boon.Type == Boon.BoonType.Duration)
                    {
                        WriteCell(conditions[boon.ID].Generated[player].ToString() + "%");
                        WriteCell(conditions[boon.ID].Overstacked[player].ToString() + "%");
                    }
                    else
                    {
                        WriteCell(conditions[boon.ID].Generated[player].ToString());
                        WriteCell(conditions[boon.ID].Overstacked[player].ToString());
                    }
                }
                NewLine();
                count++;
            }


            while (count < 15)//so each graph has equal spacing
            {
                NewLine();
                count++;
            }
        }
    }
}
