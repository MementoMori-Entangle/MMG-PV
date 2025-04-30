using MMG.Dao;
using MMG.Dto;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MMG.Common
{
    public class Common
    {
        public const int MAIN_P_1 = 11;
        public const int MAIN_P_2 = 12;
        public const int SUB_P_1 = 21;
        public const int SUB_P_2 = 22;
        public const int DEBUFF_P_1 = 31;
        public const int DEBUFF_P_2 = 32;

        public static readonly string[] NG_FILE_NAME_STR = { "\\", "*", "⁄", ":", "?", "\"", ">", "<", "|" };

        // 関係者以外に漏れたらセキュリティの観点から即座に変更すること
        public static readonly string BOT_TOKEN = Properties.Settings.Default["BOT_TOKEN"].ToString();

        public static readonly string[] RARITY_TYPE = { "N", "R", "R+", "SR", "SR+", "SSR", "SSR+", "UR", "UR+", "LR" };
        public static readonly string[] FORMATION_ORDER = { "先鋒", "次鋒", "中堅", "副将", "大将" };

        public static readonly string BASE_DIR_PATH = AppDomain.CurrentDomain.BaseDirectory;
        public static readonly string BASE_DATA_PATH = BASE_DIR_PATH + Properties.Settings.Default["BASE_DATA_PATH"].ToString();
        public static readonly string PLAYER_FILE_NAME = Properties.Settings.Default["PLAYER_FILE_NAME"].ToString();
        public static readonly string FORMATION_FILE_NAME = Properties.Settings.Default["FORMATION_FILE_NAME"].ToString();
        public static readonly string CHARACTER_FILE_NAME = Properties.Settings.Default["CHARACTER_FILE_NAME"].ToString();
        public static readonly string GUILD_FILE_NAME = Properties.Settings.Default["GUILD_FILE_NAME"].ToString();
        public static readonly string PLAYERVSPLAYER_FILE_NAME = Properties.Settings.Default["PLAYERVSPLAYER_FILE_NAME"].ToString();
        public static readonly string FORMATIONVSFORMATION_FILE_NAME = Properties.Settings.Default["FORMATIONVSFORMATION_FILE_NAME"].ToString();
        public static readonly string DISCORD_FILE_NAME = Properties.Settings.Default["DISCORD_FILE_NAME"].ToString();
        public static readonly string PLAYER_FILE_PATH = BASE_DATA_PATH + PLAYER_FILE_NAME;
        public static readonly string FORMATION_FILE_PATH = BASE_DATA_PATH + FORMATION_FILE_NAME;
        public static readonly string CHARACTER_FILE_PATH = BASE_DATA_PATH + CHARACTER_FILE_NAME;
        public static readonly string GUILD_FILE_PATH = BASE_DATA_PATH + GUILD_FILE_NAME;
        public static readonly string PLAYERVSPLAYER_FILE_PATH = BASE_DATA_PATH + PLAYERVSPLAYER_FILE_NAME;
        public static readonly string FORMATIONVSFORMATION_FILE_PATH = BASE_DATA_PATH + FORMATIONVSFORMATION_FILE_NAME;
        public static readonly string DISCORD_FILE_PATH = BASE_DATA_PATH + DISCORD_FILE_NAME;
        public static readonly string SELECT_GUILD_ALL_STR = Properties.Settings.Default["SELECT_GUILD_ALL_STR"].ToString();
        public static readonly int SERVER_TIME_CORRECTION_SECONDS = (int)Properties.Settings.Default["SERVER_TIME_CORRECTION_SECONDS"];
        public static readonly float DEFENSE_TIME = (float)Properties.Settings.Default["DEFENSE_TIME"];
        public static readonly ulong DISCORD_SERVER_ADMIN_CLIENT_ID = (ulong)Properties.Settings.Default["DISCORD_SERVER_ADMIN_CLIENT_ID"];
        public static readonly ulong DISCORD_BOT_CLIENT_ID = (ulong)Properties.Settings.Default["DISCORD_BOT_CLIENT_ID"];
        public static readonly ulong DISCORD_WEEK_PASS_CHANNEL_ID = (ulong) Properties.Settings.Default["DISCORD_WEEK_PASS_CHANNEL_ID"];
        public static readonly ulong DISCORD_MMGP_CHANNEL_ID = (ulong)Properties.Settings.Default["DISCORD_MMGP_CHANNEL_ID"];
        public static readonly string DISCORD_WEEK_PASS_CHANNEL_CLIENT_IDS = Properties.Settings.Default["DISCORD_WEEK_PASS_CHANNEL_CLIENT_IDS"].ToString();
        public static readonly string BACKUP_DIR = Properties.Settings.Default["BACKUP_DIR"].ToString();
        public static readonly string PROCESS_TITLE_NAME = Properties.Settings.Default["PROCESS_TITLE_NAME"].ToString();
        public static readonly string MATCH_TEMP_DIR = Properties.Settings.Default["MATCH_TEMP_DIR"].ToString();
        public static readonly string MATCH_TEMP_DIV_DIR = Properties.Settings.Default["MATCH_TEMP_DIV_DIR"].ToString();
        public static readonly string MATCH_TEMP_DIR_PATH = BASE_DIR_PATH + MATCH_TEMP_DIR;
        public static readonly string MATCH_TEMP_DIV_DIR_PATH = MATCH_TEMP_DIR_PATH + MATCH_TEMP_DIV_DIR;
        public static readonly int MATCH_TEMP_DIR_MAX_SIZE = (int)Properties.Settings.Default["MATCH_TEMP_DIR_MAX_SIZE"]; // MB
        public static readonly string MATCH_CHARACTER_DIR = Properties.Settings.Default["MATCH_CHARACTER_DIR"].ToString();
        public static readonly double MATCH_THRESHOLD = (double)Properties.Settings.Default["MATCH_THRESHOLD"];
        public static readonly string CHARACTER_DIR = Properties.Settings.Default["FormationCharaImagePath"].ToString();
        public static readonly string MATCH_GUILDVS_CHARACTER_DIR = Properties.Settings.Default["MATCH_GUILDVS_CHARACTER_DIR"].ToString();
        public static readonly string MATCH_GUILDVS_PLAYER_DIR = Properties.Settings.Default["MATCH_GUILDVS_PLAYER_DIR"].ToString();
        public static readonly string MATCH_AUTO_INTERVAL_TIMES = Properties.Settings.Default["MATCH_AUTO_INTERVAL_TIMES"].ToString();
        public static readonly string MATCH_WINDOW_TITLE_NAMES = Properties.Settings.Default["MATCH_WINDOW_TITLE_NAMES"].ToString();

        public static string FormationCharaImagePath { get; set; }
        public static string FormationTokeNekoImagePath { get; set; }
        public static string FormationImageExtension { get; set; }
        public static string FormationImageNothing { get; set; }
        public static string GuildImagePath { get; set; }

        public static GuildDto GroupFilter(GuildDto guildDto, List<int> groupIdList)
        {
            if (groupIdList.Count == 0)
            {
                return guildDto;
            }

            PlayerDto[] playerDtos = guildDto.Members;

            playerDtos = playerDtos.Where(x => groupIdList.Contains(x.GBGroupId)).ToArray();

            if (playerDtos.Length == 0)
            {
                guildDto.MemberNum = 0;
                guildDto.Stamina = 0;
                guildDto.ForceValue = 0;
                guildDto.Members = playerDtos;

                return guildDto;
            }

            Dictionary<int, int> guildMemberNumDic = new Dictionary<int, int>();
            Dictionary<int, int> guildMemberStaminaDic = new Dictionary<int, int>();
            Dictionary<int, long> guildMemberForceValDic = new Dictionary<int, long>();
            Dictionary<int, List<PlayerDto>> guildMembersDic = new Dictionary<int, List<PlayerDto>>();

            // ギルド毎のメンバー数をカウント
            foreach (PlayerDto playerDto in playerDtos)
            {
                if (guildMemberNumDic.ContainsKey(playerDto.GuildId))
                {
                    guildMemberNumDic[playerDto.GuildId]++;
                }
                else
                {
                    guildMemberNumDic.Add(playerDto.GuildId, 1);
                }

                if (guildMemberStaminaDic.ContainsKey(playerDto.GuildId))
                {
                    guildMemberStaminaDic[playerDto.GuildId] += playerDto.Stamina;
                }
                else
                {
                    guildMemberStaminaDic.Add(playerDto.GuildId, playerDto.Stamina);
                }

                if (guildMemberForceValDic.ContainsKey(playerDto.GuildId))
                {
                    guildMemberForceValDic[playerDto.GuildId] += playerDto.ForceValue;
                }
                else
                {
                    guildMemberForceValDic.Add(playerDto.GuildId, playerDto.ForceValue);
                }

                if (guildMembersDic.ContainsKey(playerDto.GuildId))
                {
                    guildMembersDic[playerDto.GuildId].Add(playerDto);
                }
                else
                {
                    guildMembersDic.Add(playerDto.GuildId, new List<PlayerDto> { playerDto });
                }
            }

            // ギルド情報にメンバー情報と総戦力値更新
            if (guildMemberNumDic.ContainsKey(guildDto.Id))
            {
                guildDto.MemberNum = guildMemberNumDic[guildDto.Id];
            }

            if (guildMemberStaminaDic.ContainsKey(guildDto.Id))
            {
                guildDto.Stamina = guildMemberStaminaDic[guildDto.Id];
            }

            if (guildMemberForceValDic.ContainsKey(guildDto.Id))
            {
                guildDto.ForceValue = guildMemberForceValDic[guildDto.Id];
            }

            if (guildMembersDic.ContainsKey(guildDto.Id))
            {
                guildDto.Members = guildMembersDic[guildDto.Id].ToArray();
            }

            return guildDto;
        }

        public static string NGConvertString(string input)
        {
            string output = input;

            int cnt = NG_FILE_NAME_STR.Length;

            for (int i = 0; i < cnt; i++)
            {
                output = output.Replace("{" + i + "}", NG_FILE_NAME_STR[i]);
            }

            return output;
        }

        public static PlayerVSPlayerDto[] GetPlayerVSPlayerDto(FormationVSFormationDto[] formationVSFormationDtos)
        {
            List<PlayerVSPlayerDto> playerVSPlayerList = new List<PlayerVSPlayerDto>();

            List<string> formationIdsList = new List<string>();

            foreach (FormationVSFormationDto dto in formationVSFormationDtos)
            {
                formationIdsList.Add(dto.WinFormationId + "," + dto.LoseFormationId);
            }

            formationIdsList = formationIdsList.Distinct().ToList();

            FormationDao formationDao = new FormationDao();

            FormationDto[] formationDtos = formationDao.GetFormationDtos();

            long uid = 1;

            foreach (string idStr in formationIdsList)
            {
                string[] ids = idStr.Split(',');

                PlayerVSPlayerDto playerVSPlayerDto = new PlayerVSPlayerDto();

                playerVSPlayerDto.UId = uid;
                playerVSPlayerDto.WinPlayerId = long.Parse(ids[0]);
                playerVSPlayerDto.LosePlayerId = long.Parse(ids[1]);

                playerVSPlayerList.Add(playerVSPlayerDto);

                uid++;
            }

            return playerVSPlayerList.ToArray();
        }

        public static PlayerDto[] DeleteGVSFormationData(PlayerDto[] playerDtos)
        {
            foreach (PlayerDto playerDto in playerDtos)
            {
                playerDto.Formations = playerDto.Formations.Where(x => x.IsGVS == false).ToArray();
            }

            return playerDtos;
        }

        public static long GetDirectorySize(DirectoryInfo dirInfo)
        {
            long size = 0;

            foreach (FileInfo fi in dirInfo.GetFiles())
            {
                size += fi.Length;
            }

            foreach (DirectoryInfo di in dirInfo.GetDirectories())
            {
                size += GetDirectorySize(di);
            }

            return size;
        }

        public static int SearchFormationId(string playerName, string guildName, CharacterDto[] characterDtos)
        {
            int formationId = 0;

            PlayerDao playerDao = new PlayerDao();
            PlayerDto playerDto = playerDao.GetPlayerDto(playerName, guildName);

            if (null == playerDto)
            {
                return formationId;
            }

            List<string> targetNameList = new List<string>();

            foreach (CharacterDto characterDto in characterDtos)
            {
                targetNameList.Add(characterDto.Name);
            }

            foreach (FormationDto formationDto in playerDto.Formations)
            {
                if (formationDto.Characters.Length != characterDtos.Length)
                {
                    continue;
                }

                List<string> checkNameList = new List<string>();

                foreach (CharacterDto characterDto in formationDto.Characters)
                {
                    checkNameList.Add(characterDto.Name);
                }

                if (checkNameList.All(x => targetNameList.Contains(x)))
                {
                    formationId = formationDto.Id;
                    break;
                }
            }

            return formationId;
        }

        public static int GetFormationId(int guildId, int guildPlayerNo, int no)
        {
            string foramtionId;

            if (guildId > 0)
            {
                foramtionId = guildId.ToString() + "0" + guildPlayerNo.ToString("D2") + no.ToString("D2");
            }
            else
            {
                foramtionId = guildPlayerNo.ToString("D2") + no.ToString("D2");
            }

            return int.Parse(foramtionId);
        }

        public static bool IsDiscordAvailable()
        {
            bool isAva = true;

            if (string.IsNullOrEmpty(BOT_TOKEN) || DISCORD_SERVER_ADMIN_CLIENT_ID < 1 ||
                DISCORD_BOT_CLIENT_ID < 1 || DISCORD_WEEK_PASS_CHANNEL_ID < 1 ||
                DISCORD_MMGP_CHANNEL_ID < 1)
            {
                isAva = false;
            }

            return isAva;
        }

        public static string[] GetDivide(string str, int cnt)
        {
            if (null == str)
            {
                return null;
            }

            if (cnt == 0)
            {
                return new string[] { str };
            }

            var list = new List<string>();
            string a = string.Empty;

            foreach (var c in str)
            {
                a += c;

                if (a.Length % cnt == 0)
                {
                    list.Add(a);
                    a = string.Empty;
                }
            }

            if (a != string.Empty)
            {
                list.Add(a);
            }

            return list.ToArray();
        }

        public static CharacterDto[] GetCharacterDifference(BaseCharacterDto[] baseChara, CharacterDto[] nowChara)
        {
            List<CharacterDto> characterList = new List<CharacterDto>();

            foreach (BaseCharacterDto baseDto in baseChara)
            {
                if (!nowChara.Where(x => x.Id == baseDto.Id).Any())
                {
                    CharacterDto characterDto = new CharacterDto()
                    {
                        Id = baseDto.Id,
                        Name = baseDto.Name,
                        Rarity = baseDto.Rarity,
                        Speed = baseDto.Speed
                    };

                    characterList.Add(characterDto);
                }
            }

            return characterList.ToArray();
        }

        public static CharacterDto[] GetNewCharacterDtos(FormationDto formationDto, int charaCnt)
        {
            CharacterDto[] calcCharaDtos = new CharacterDto[charaCnt];

            for (int i = 0; i < charaCnt; i++)
            {
                calcCharaDtos[i] = new CharacterDto()
                {
                    Id = formationDto.Characters[i].Id,
                    FormationId = formationDto.Characters[i].FormationId,
                    PlayerId = formationDto.Characters[i].PlayerId,
                    SortNo = formationDto.Characters[i].SortNo,
                    Name = formationDto.Characters[i].Name,
                    Rarity = formationDto.Characters[i].Rarity,
                    Level = formationDto.Characters[i].Level,
                    ForceValue = formationDto.Characters[i].ForceValue,
                    SpeedRune1 = formationDto.Characters[i].SpeedRune1,
                    SpeedRune2 = formationDto.Characters[i].SpeedRune2,
                    SpeedRune3 = formationDto.Characters[i].SpeedRune3,
                    PiercingRune1 = formationDto.Characters[i].PiercingRune1,
                    PiercingRune2 = formationDto.Characters[i].PiercingRune2,
                    PiercingRune3 = formationDto.Characters[i].PiercingRune3,
                    LRNum = formationDto.Characters[i].LRNum,
                    URNum = formationDto.Characters[i].URNum,
                    SSRNum = formationDto.Characters[i].SSRNum,
                    SRNum = formationDto.Characters[i].SRNum,
                    RNum = formationDto.Characters[i].RNum,
                    EWeapon = formationDto.Characters[i].EWeapon
                };
            }

            return calcCharaDtos;
        }

        public static int[] CalcAddSpeedRune(CharacterDto characterDto, int speedDifference)
        {
            int[] speedRunes = new int[3];

            speedRunes[0] = characterDto.SpeedRune1;
            speedRunes[1] = characterDto.SpeedRune2;
            speedRunes[2] = characterDto.SpeedRune3;

            int idealSpeed = characterDto.Speed;
            int targetSpeed = characterDto.Speed - speedDifference;
            int runeMax = RuneDto.Speed.Length - 1;

            while (targetSpeed <= idealSpeed)
            {
                if (characterDto.SpeedRune1 < runeMax)
                {
                    targetSpeed += RuneDto.Speed[characterDto.SpeedRune1 + 1] - RuneDto.Speed[characterDto.SpeedRune1];
                    characterDto.SpeedRune1++;
                    speedRunes[0] = characterDto.SpeedRune1;
                }
                else if (characterDto.SpeedRune2 < runeMax)
                {
                    targetSpeed += RuneDto.Speed[characterDto.SpeedRune2 + 1] - RuneDto.Speed[characterDto.SpeedRune2];
                    characterDto.SpeedRune2++;
                    speedRunes[1] = characterDto.SpeedRune2;
                }
                else if (characterDto.SpeedRune3 < runeMax)
                {
                    targetSpeed += RuneDto.Speed[characterDto.SpeedRune3 + 1] - RuneDto.Speed[characterDto.SpeedRune3];
                    characterDto.SpeedRune3++;
                    speedRunes[2] = characterDto.SpeedRune3;
                }
                else
                {
                    break;
                }

                if (targetSpeed > idealSpeed)
                {
                    break;
                }
            }

            return speedRunes;
        }

        public static int CalcTopSpeed(CharacterDto[] characterDtos)
        {
            int speed = 0;

            foreach (CharacterDto characterDto in characterDtos)
            {
                if (speed < characterDto.Speed)
                {
                    speed = characterDto.Speed;
                }
            }

            return speed;
        }

        public static CharacterDto[] CalcCharacterSpeed(FormationDto formationDto)
        {
            int charaCnt = formationDto.Characters.Length;

            if (charaCnt == 0)
            {
                return new CharacterDto[0];
            }

            CharacterDto[] calcCharaDtos = GetNewCharacterDtos(formationDto, charaCnt);

            FormationDto calcFDto = new FormationDto()
            {
                Id = formationDto.Id,
                Characters = calcCharaDtos
            };

            BattleCalculations.CalculationSpeed(calcFDto, 0, 0, 0);

            return calcFDto.Characters;
        }

        public static string GetLastSpeedInfo(FormationDto formationDto, int[] initSpeed, int pos)
        {
            // ルーン、スキル、専用武器による速度バフ反映
            BattleCalculations.CalculationSpeed(formationDto, 0, 0, 0);

            string result;

            if (initSpeed[pos] == formationDto.Characters[pos].Speed)
            {
                result = formationDto.Characters[pos].Speed.ToString();
            }
            else
            {
                result = formationDto.Characters[pos].Speed.ToString() + " (" + initSpeed[pos] + ")";
            }

            return result;
        }

        public static string GetMaxSpeedRuneText(FormationDto formationDto, bool isDebuff = false)
        {
            string result = string.Empty;
            int maxSpeed = 0;

            if (!formationDto.IsDebuff || isDebuff)
            {
                foreach (CharacterDto characterDto in formationDto.Characters)
                {
                    if (characterDto.SpeedRune1 + characterDto.SpeedRune2 + characterDto.SpeedRune3 > maxSpeed)
                    {
                        maxSpeed = characterDto.SpeedRune1 + characterDto.SpeedRune2 + characterDto.SpeedRune3;
                        result = "(" + characterDto.SpeedRune1 + "," + characterDto.SpeedRune2 + "," + characterDto.SpeedRune3 + ")";
                    }
                }
            }

            return result;
        }

        public static string GetSpeedRuneText(CharacterDto characterDto, bool isZeroView = false)
        {
            if (isZeroView)
            {
                return "(" + characterDto.SpeedRune1 + "," + characterDto.SpeedRune2 + "," + characterDto.SpeedRune3 + ")";
            }
            else
            {
                if (0 < characterDto.SpeedRune1 + characterDto.SpeedRune2 + characterDto.SpeedRune3)
                {
                    return "(" + characterDto.SpeedRune1 + "," + characterDto.SpeedRune2 + "," + characterDto.SpeedRune3 + ")";
                }
                else
                {
                    return "-";
                }
            }
        }

        public static string GetLevelText(int level)
        {
            string levelText = string.Empty;

            if (level > 0)
            {
                levelText = "Lv " + level;
            }

            return levelText;
        }

        public static string GetRarityType(int type)
        {
            string rarityType;
            int rarityMax = RARITY_TYPE.Length;

            if (rarityMax > type)
            {
                rarityType = RARITY_TYPE[type];
            }
            else
            {
                rarityType = RARITY_TYPE[rarityMax - 1] + "+" + (type + 1 - rarityMax).ToString();
            }

            return rarityType;
        }

        public static int GetRarity(string type)
        {
            int rarity = Array.IndexOf(RARITY_TYPE, type);

            if (rarity >= 0)
            {
                return rarity;
            }

            rarity = RARITY_TYPE.Length + int.Parse(type.Remove(0, 3)) - 1;

            return rarity;
        }
    }
}
