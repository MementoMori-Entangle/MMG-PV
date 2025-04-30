using log4net;
using MMG.Dto;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace MMG.Common
{
    public class GuildBattleProgressGenerator
    {
        private readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().Name);

        private readonly int FORMATION_UNIT_LINE_MAX_LENGTH = 60;
        private readonly int FORMATION_LINE_MAX_LENGTH = 40;

        public GuildBattleProgressGenerator() { }

        public string CreateFormationToString(GuildBattleProgressDto guildBattleProgressDto)
        {
            StringBuilder sb = new StringBuilder();

            try
            {
                Encoding encShiftJis = Encoding.GetEncoding("Shift_JIS");

                int strLen = 0;
                int sumFormation = 0;
                StringBuilder fSB = new StringBuilder();

                foreach (KeyValuePair<string, int> formation in guildBattleProgressDto.Formation)
                {
                    string formationId = "[" + guildBattleProgressDto.FormationIdName[formation.Key] + "]";
                    strLen = encShiftJis.GetByteCount(formation.Key + formationId);

                    if (sumFormation + strLen < FORMATION_UNIT_LINE_MAX_LENGTH)
                    {
                        fSB.Append(formation.Key);
                        fSB.Append(formationId);
                        fSB.Append("  ");
                        sumFormation += strLen + 2;
                    }
                    else
                    {
                        sb.Append(fSB);
                        sb.Append(Environment.NewLine);
                        fSB = new StringBuilder();
                        fSB.Append(formation.Key);
                        fSB.Append(formationId);
                        fSB.Append("  ");
                        sumFormation = strLen + 2;
                    }
                }

                if (fSB.Length > 0)
                {
                    sb.Append(fSB);
                }
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }

            return sb.ToString();
        }

        public string CreateGuildBattleProgressToString(List<GuildBattleProgressDto> guildBPList, string cmd = "", bool isNewLine = true)
        {
            StringBuilder sb = new StringBuilder();

            try
            {
                List<string> result = CreateGuildBattleProgress(guildBPList, cmd);

                foreach (string str in result)
                {
                    sb.Append(str);

                    if (isNewLine)
                    {
                        sb.Append(Environment.NewLine);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }

            return sb.ToString();
        }

        public List<string> CreateGuildBattleProgress(List<GuildBattleProgressDto> guildBPList, string cmd = "")
        {
            List<string> result = new List<string>();

            try
            {
                if (guildBPList.Count == 0)
                {
                    return result;
                }

                // ヘッダ
                result.Add("---------------------------------------------------------------------");
                result.Add("名前                スタミナ   編成");
                result.Add("---------------------------------------------------------------------");

                Encoding encShiftJis = Encoding.GetEncoding("Shift_JIS");

                foreach (GuildBattleProgressDto gbpDto in guildBPList)
                {
                    if (!gbpDto.IsOnline)
                    {
                        continue;
                    }
                    else if ("!mmgphs".Equals(cmd) && 0 == GetStamina(gbpDto.Stamina))
                    {
                        continue;
                    }
                    else if ("!mmgpms".Equals(cmd) && (!IsCanUseMainParty(gbpDto.Formation) || !IsCanUseSubParty(gbpDto.Formation)))
                    {
                        continue;
                    }
                    else if ("!mmgpm".Equals(cmd) && !IsCanUseMainParty(gbpDto.Formation))
                    {
                        continue;
                    }
                    else if ("!mmgps".Equals(cmd) && !IsCanUseSubParty(gbpDto.Formation))
                    {
                        continue;
                    }
                    else if ("!mmgpd".Equals(cmd) && !IsCanUseDebuffParty(gbpDto.Formation))
                    {
                        continue;
                    }
                    else if ("!mmgpmf".Equals(cmd) && !IsCanUseMFlorence(gbpDto.Formation))
                    {
                        continue;
                    }
                    else if ("!mmgpmc".Equals(cmd) && !IsCanUseMCordy(gbpDto.Formation))
                    {
                        continue;
                    }
                    else if ("!mmgpmk".Equals(cmd) && !IsCanUseMAvoidance(gbpDto.Formation))
                    {
                        continue;
                    }


                    StringBuilder sb = new StringBuilder();

                    int strLen = encShiftJis.GetByteCount(gbpDto.Name);

                    sb.Append(gbpDto.Name);

                    int maxLen = 20 - strLen;

                    for (int i = 0; i < maxLen; i++)
                    {
                        sb.Append(' ');
                    }

                    strLen = encShiftJis.GetByteCount(gbpDto.Stamina);

                    sb.Append(gbpDto.Stamina);

                    maxLen = 11 - strLen;

                    for (int i = 0; i < maxLen; i++)
                    {
                        sb.Append(' ');
                    }

                    int sumFormation = 0;
                    StringBuilder fSB = new StringBuilder();

                    foreach (KeyValuePair<string, int> formation in gbpDto.Formation)
                    {
                        strLen = encShiftJis.GetByteCount(formation.Key);

                        if (sumFormation + strLen < FORMATION_LINE_MAX_LENGTH)
                        {
                            fSB.Append(formation.Key);
                            fSB.Append("  ");
                            sumFormation += strLen + 2;
                        }
                        else
                        {
                            sb.Append(fSB);
                            sb.Append(Environment.NewLine);
                            sb.Append("                               ");
                            fSB = new StringBuilder();
                            fSB.Append(formation.Key);
                            fSB.Append("  ");
                            sumFormation = strLen + 2;
                        }
                    }

                    if (fSB.Length > 0)
                    {
                        sb.Append(fSB);
                    }

                    result.Add(sb.ToString());
                    result.Add("----------------------------------------------------------------------");
                }
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }

            return result;
        }

        private bool IsCanUseMAvoidance(Dictionary<string, int> formation)
        {
            bool isUse = false;

            foreach (KeyValuePair<string, int> keyValuePair in formation)
            {
                if ((Common.MAIN_P_1 == keyValuePair.Value || Common.MAIN_P_2 == keyValuePair.Value) &&
                    keyValuePair.Key.Contains("回避"))
                {
                    isUse = true;
                    break;
                }
            }

            return isUse;
        }

        private bool IsCanUseMCordy(Dictionary<string, int> formation)
        {
            bool isUse = false;

            foreach (KeyValuePair<string, int> keyValuePair in formation)
            {
                if ((Common.MAIN_P_1 == keyValuePair.Value || Common.MAIN_P_2 == keyValuePair.Value) &&
                    keyValuePair.Key.Contains("コル"))
                {
                    isUse = true;
                    break;
                }
            }

            return isUse;
        }

        private bool IsCanUseMFlorence(Dictionary<string, int> formation)
        {
            bool isUse = false;

            foreach (KeyValuePair<string, int> keyValuePair in formation)
            {
                if ((Common.MAIN_P_1 == keyValuePair.Value || Common.MAIN_P_2 == keyValuePair.Value) &&
                    keyValuePair.Key.Contains("フロ"))
                {
                    isUse = true;
                    break;
                }
            }

            return isUse;
        }

        private bool IsCanUseDebuffParty(Dictionary<string, int> formation)
        {
            bool isUse = false;

            foreach (KeyValuePair<string, int> keyValuePair in formation)
            {
                if (Common.DEBUFF_P_1 == keyValuePair.Value || Common.DEBUFF_P_2 == keyValuePair.Value)
                {
                    isUse = true;
                    break;
                }
            }

            return isUse;
        }

        private bool IsCanUseSubParty(Dictionary<string, int> formation)
        {
            bool isUse = false;

            foreach (KeyValuePair<string, int> keyValuePair in formation)
            {
                if (Common.SUB_P_1 == keyValuePair.Value || Common.SUB_P_2 == keyValuePair.Value)
                {
                    isUse = true;
                    break;
                }
            }

            return isUse;
        }

        private bool IsCanUseMainParty(Dictionary<string, int> formation)
        {
            bool isUse = false;

            foreach (KeyValuePair<string, int> keyValuePair in formation)
            {
                if (Common.MAIN_P_1 == keyValuePair.Value || Common.MAIN_P_2 == keyValuePair.Value)
                {
                    isUse = true;
                    break;
                }
            }

            return isUse;
        }

        private int GetStamina(string stamina)
        {
            string[] sp = stamina.ToString().Split('/');
            return int.Parse(sp[0].Trim());
        }
    }
}
