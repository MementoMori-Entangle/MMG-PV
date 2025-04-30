using MMG.Common;
using MMG.Dto;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Controls;

namespace MMG.Dao
{
    public class PlayerDao
    {
        Dictionary<long, int> vfDic;
        Dictionary<long, int> vfcnDic;

        private bool isDeleteFlg;
        private bool isGVS;

        public PlayerDao()
        {
            isDeleteFlg = true;
            isGVS = true;
        }

        public PlayerDao(bool isDeleteFlg = true)
        {
            this.isDeleteFlg = isDeleteFlg;
            isGVS =true;
        }

        public PlayerDao(bool isDeleteFlg = true, bool isGVS = true)
        {
            this.isDeleteFlg = isDeleteFlg;
            this.isGVS = isGVS;
        }

        public PlayerDao(Dictionary<long, int> vfDic = null, Dictionary<long, int> vfcnDic = null)
        {
            this.vfDic = vfDic;
            this.vfcnDic = vfcnDic;
            isDeleteFlg = true;
            isGVS = true;
        }

        public PlayerDao(Dictionary<long, int> vfDic = null, Dictionary<long, int> vfcnDic = null, bool isDeleteFlg = true)
        {
            this.vfDic = vfDic;
            this.vfcnDic = vfcnDic;
            this.isDeleteFlg = isDeleteFlg;
            isGVS = true;
        }

        public PlayerDao(Dictionary<long, int> vfDic = null, Dictionary<long, int> vfcnDic = null, bool isDeleteFlg = true, bool isGVS = true)
        {
            this.vfDic = vfDic;
            this.vfcnDic = vfcnDic;
            this.isDeleteFlg = isDeleteFlg;
            this.isGVS = isGVS;
        }

        public void CreatePlayerFile(int guildId)
        {
            try
            {
                if (guildId < 1)
                {
                    return;
                }

                string filePath = Common.Common.BASE_DATA_PATH + guildId + "/" + Common.Common.PLAYER_FILE_NAME;

                CSVWriter csvWriter = new CSVWriter();

                csvWriter.WritePlayerCsvFile(new List<PlayerDto>(), filePath);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public void SavePlayerDto(List<PlayerDto> playerDtoList, int guildId = 0, bool isBackup = true)
        {
            try
            {
                if (null == playerDtoList || playerDtoList.Count == 0)
                {
                    return;
                }

                string filePath = string.Empty;
                string bakFilePath = string.Empty;

                if (guildId > 0)
                {
                    filePath = Common.Common.BASE_DATA_PATH + guildId + "/" + Common.Common.PLAYER_FILE_NAME;

                    if (isBackup)
                    {
                        string bakDir = Common.Common.BASE_DATA_PATH + guildId + "/"
                                      + Common.Common.BACKUP_DIR + Common.Common.PLAYER_FILE_NAME + "/";

                        if (!Directory.Exists(bakDir))
                        {
                            Directory.CreateDirectory(bakDir);
                        }

                        bakFilePath = bakDir + DateTime.Now.ToString("yyyyMMddHHmmss") + "_" + Common.Common.PLAYER_FILE_NAME;
                    }
                }
                else
                {
                    filePath = Common.Common.BASE_DATA_PATH + Common.Common.PLAYER_FILE_NAME;

                    if (isBackup)
                    {
                        string bakDir = Common.Common.BASE_DATA_PATH + Common.Common.BACKUP_DIR + Common.Common.PLAYER_FILE_NAME + "/";

                        if (!Directory.Exists(bakDir))
                        {
                            Directory.CreateDirectory(bakDir);
                        }

                        bakFilePath = bakDir + DateTime.Now.ToString("yyyyMMddHHmmss") + "_" + Common.Common.PLAYER_FILE_NAME;
                    }
                }

                if (!string.IsNullOrEmpty(bakFilePath))
                {
                    File.Copy(filePath, bakFilePath);
                }

                CSVWriter csvWriter = new CSVWriter();
                
                csvWriter.WritePlayerCsvFile(playerDtoList, filePath);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public int GetNo(int guildId, long playerId)
        {
            int no = 1;

            try
            {
                PlayerDto[] playerDtos = GetPlayerDtos(guildId);

                if (null == playerDtos)
                {
                    return no;
                }

                if (isDeleteFlg)
                {
                    playerDtos = playerDtos.Where(x => x.IsDelete == false).ToArray();
                }

                PlayerDto playerDto = playerDtos.Where(x => x.Id == playerId).FirstOrDefault();

                if (null != playerDto)
                {
                    no = playerDto.No;
                }
            }
            catch (Exception e)
            {
                throw e;
            }

            return no;
        }

        public bool IsId(long id)
        {
            bool isId = false;

            try
            {
                CSVLoader csvLoader = new CSVLoader();
                GuildDao guildDao = new GuildDao();

                int[] guildIds = guildDao.GetGuildIdList();

                PlayerDto[] playerDtos = csvLoader.ReadPlayerCsvFile(Common.Common.PLAYER_FILE_PATH);

                foreach (int guildId in guildIds)
                {
                    string playerPath = Common.Common.BASE_DATA_PATH + guildId + "/" + Common.Common.PLAYER_FILE_NAME;

                    PlayerDto[] pDtos = csvLoader.ReadPlayerCsvFile(playerPath);
                    playerDtos = playerDtos.Concat(pDtos).ToArray();
                }

                if (isDeleteFlg)
                {
                    playerDtos = playerDtos.Where(x => x.IsDelete == false).ToArray();
                }

                if (playerDtos.Where(x => x.Id == id).Any())
                {
                    isId = true;
                }
            }
            catch (Exception e)
            {
                throw e;
            }

            return isId;
        }

        public PlayerDto[] GetPlayerDtos(int guildId)
        {
            PlayerDto[] playerDtos = null;

            try
            {
                CSVLoader csvLoader = new CSVLoader();

                if (guildId > 0)
                {
                    string playerPath = Common.Common.BASE_DATA_PATH + guildId + "/" + Common.Common.PLAYER_FILE_NAME;

                    playerDtos = csvLoader.ReadPlayerCsvFile(playerPath);
                }
                else
                {
                    playerDtos = csvLoader.ReadPlayerCsvFile(Common.Common.PLAYER_FILE_PATH);
                }

                if (isDeleteFlg)
                {
                    playerDtos = playerDtos.Where(x => x.IsDelete == false).ToArray();
                }
            }
            catch (Exception e)
            {
                throw e;
            }

            return playerDtos;
        }

        public PlayerDto GetPlayerDto(long id)
        {
            PlayerDto playerDto = null;

            try
            {
                CSVLoader csvLoader = new CSVLoader();

                PlayerDto[] playerDtos = GetAllInfoPlayer();

                if (isDeleteFlg)
                {
                    playerDtos = playerDtos.Where(x => x.IsDelete == false).ToArray();
                }

                if (playerDtos.Any(x => x.Id == id))
                {
                    playerDto = playerDtos.Where(x => x.Id == id).FirstOrDefault();
                }
            }
            catch (Exception e)
            {
                throw e;
            }

            return playerDto;
        }

        public PlayerDto GetPlayerDto(string name, string guildName = "")
        {
            PlayerDto playerDto = null;

            try
            {
                CSVLoader csvLoader = new CSVLoader();

                PlayerDto[] playerDtos = GetAllInfoPlayer();

                if (isDeleteFlg)
                {
                    playerDtos = playerDtos.Where(x => x.IsDelete == false).ToArray();
                }

                if (playerDtos.Any(x => x.GuildName.Contains(guildName) && x.Name.Equals(name)))
                {
                    playerDto = playerDtos.Where(x => x.GuildName.Contains(guildName) && x.Name.Equals(name)).FirstOrDefault();
                }
            }
            catch (Exception e)
            {
                throw e;
            }

            return playerDto;
        }

        public PlayerDto[] GetAllInfoPlayerDtos(string guildName = "")
        {
            PlayerDto[] playerDtos = null;

            try
            {
                CSVLoader csvLoader = new CSVLoader();

                playerDtos = GetAllInfoPlayer();

                if (isDeleteFlg)
                {
                    playerDtos = playerDtos.Where(x => x.IsDelete == false).ToArray();
                }

                if (playerDtos.Any(x => x.GuildName.Contains(guildName)))
                {
                    playerDtos = playerDtos.Where(x => x.GuildName.Contains(guildName)).ToArray();
                }
            }
            catch (Exception e)
            {
                throw e;
            }

            return playerDtos;
        }

        public PlayerDto[] GetPlayerDtos(string guildName = "")
        {
            PlayerDto[] playerDtos = null;

            try
            {
                CSVLoader csvLoader = new CSVLoader();
                GuildDao guildDao = new GuildDao();

                // プレイヤー情報を取得
                playerDtos = csvLoader.ReadPlayerCsvFile(Common.Common.PLAYER_FILE_PATH);

                int[] guildIds = guildDao.GetGuildIdList(guildName);

                foreach (int guildId in guildIds)
                {
                    string playerPath = Common.Common.BASE_DATA_PATH + guildId + "/" + Common.Common.PLAYER_FILE_NAME;

                    PlayerDto[] pDtos = csvLoader.ReadPlayerCsvFile(playerPath);
                    playerDtos = playerDtos.Concat(pDtos).ToArray();
                }

                if (isDeleteFlg)
                {
                    playerDtos = playerDtos.Where(x => x.IsDelete == false).ToArray();
                }

                if (playerDtos.Any(x => x.GuildName.Contains(guildName)))
                {
                    playerDtos = playerDtos.Where(x => x.GuildName.Contains(guildName)).ToArray();
                }
            }
            catch (Exception e)
            {
                throw e;
            }

            return playerDtos;
        }

        public PlayerDto[] GetAllInfoPlayer(int id = 0, string name = "", int world = 0, string guildName = "")
        {
            PlayerDto[] playerDtos = null;

            try
            {
                CSVLoader csvLoader = new CSVLoader();
                GuildDao guildDao = new GuildDao();

                int[] guildIds = guildDao.GetGuildIdList();

                // プレイヤー情報を取得
                playerDtos = csvLoader.ReadPlayerCsvFile(Common.Common.PLAYER_FILE_PATH);

                // 編成情報を取得
                FormationDto[] formationDtos = csvLoader.ReadFormationCsvFile(Common.Common.FORMATION_FILE_PATH);

                // 編成キャラクター情報を取得
                CharacterDto[] characterDtos = csvLoader.ReadCharacterCsvFile(Common.Common.CHARACTER_FILE_PATH);

                foreach (int guildId in guildIds)
                {
                    string playerPath = Common.Common.BASE_DATA_PATH + guildId + "/" + Common.Common.PLAYER_FILE_NAME;

                    PlayerDto[] pDtos = csvLoader.ReadPlayerCsvFile(playerPath);
                    playerDtos = playerDtos.Concat(pDtos).ToArray();

                    string formationPath = Common.Common.BASE_DATA_PATH + guildId + "/" + Common.Common.FORMATION_FILE_NAME;

                    FormationDto[] fDtos = csvLoader.ReadFormationCsvFile(formationPath);
                    formationDtos = formationDtos.Concat(fDtos).ToArray();

                    string characterPath = Common.Common.BASE_DATA_PATH + guildId + "/" + Common.Common.CHARACTER_FILE_NAME;

                    CharacterDto[] cDtos = csvLoader.ReadCharacterCsvFile(characterPath);
                    characterDtos = characterDtos.Concat(cDtos).ToArray();
                }

                if (isDeleteFlg)
                {
                    playerDtos = playerDtos.Where(x => x.IsDelete == false).ToArray();
                }

                if (isGVS)
                {
                    FormationDto[] delChara = formationDtos.Where(x => x.IsGVS == false).ToArray();

                    foreach (FormationDto dto in delChara)
                    {
                        characterDtos = characterDtos.Where(x => x.FormationId != dto.Id).ToArray();
                    }

                    formationDtos = formationDtos.Where(x => x.IsGVS == isGVS).ToArray();
                }

                BaseCharacterDto[] baseCharacterDtos = BaseCharacter.GetBaseCharacterDtos();

                int lastPlayerFormationId = 10001;

                // 編成に対応するキャラクター情報を設定
                foreach (PlayerDto playerDto in playerDtos)
                {
                    FormationDto[] playerFDtos = formationDtos.Where(x => x.PlayerId == playerDto.Id).OrderBy(value => value.SortNo).ToArray();

                    if ((null == vfDic || !vfDic.ContainsKey(playerDto.Id)) && (null != playerFDtos && playerFDtos.Length > 0))
                    {
                        foreach (FormationDto formationDto in playerFDtos)
                        {
                            CharacterDto[] playerFCDtos = characterDtos.Where(x => x.FormationId == formationDto.Id).OrderBy(value => value.SortNo).ToArray();

                            if (null == playerFCDtos && playerFCDtos.Length == 0)
                            {
                                continue;
                            }

                            formationDto.Characters = playerFCDtos;

                            foreach (CharacterDto characterDto in playerFCDtos)
                            {
                                BaseCharacterDto baseCharacterDto = baseCharacterDtos.Where(x => x.Id == characterDto.Id).FirstOrDefault();

                                characterDto.Speed = baseCharacterDto.Speed;
                                characterDto.Attribute = baseCharacterDto.Attribute;
                            }
                        }
                    }
                    else if (null != vfDic && vfDic.ContainsKey(playerDto.Id))
                    {
                        // 仮想情報を設定
                        int vfPlayerId = vfDic[playerDto.Id] * -1;

                        FormationDto[] tempFDtos = formationDtos.Where(x => x.PlayerId == vfPlayerId).OrderBy(value => value.SortNo).ToArray();

                        FormationDto[] playerVFDtos = tempFDtos.DeepCopy();

                        foreach (FormationDto formationDto in playerVFDtos)
                        {
                            CharacterDto[] tempCDtos = characterDtos.Where(x => x.FormationId == formationDto.Id).OrderBy(value => value.SortNo).ToArray();

                            CharacterDto[] playerVFCDtos = tempCDtos.DeepCopy();

                            // ID系更新
                            foreach (CharacterDto dto in playerVFCDtos)
                            {
                                dto.UpdatePlayerId(playerDto.Id);
                                dto.UpdateFormationId(playerDto.UId);
                            }

                            if (null == playerVFCDtos && playerVFCDtos.Length == 0)
                            {
                                continue;
                            }

                            formationDto.Characters = playerVFCDtos;

                            foreach (CharacterDto characterDto in playerVFCDtos)
                            {
                                BaseCharacterDto baseCharacterDto = baseCharacterDtos.Where(x => x.Id == characterDto.Id).FirstOrDefault();

                                characterDto.Speed = baseCharacterDto.Speed;
                                characterDto.Attribute = baseCharacterDto.Attribute;
                            }

                            formationDto.UpdateId(playerDto.UId);
                            formationDto.UpdatePlayerId(playerDto.Id);
                        }

                        playerFDtos = playerVFDtos;
                    }

                    CharacterDto[] playerCharaDtos = null;

                    if (null != vfDic && vfDic.ContainsKey(playerDto.Id))
                    {
                        int vfPlayerId = vfDic[playerDto.Id] * -1;

                        CharacterDto[] tempCDto = characterDtos.Where(x => x.PlayerId == vfPlayerId).ToArray();

                        playerCharaDtos = tempCDto.DeepCopy();
                        playerDto.Stamina = characterDtos.Where(x => x.PlayerId == vfPlayerId).Count() * 2;
                    }
                    else
                    {
                        playerCharaDtos = characterDtos.Where(x => x.PlayerId == playerDto.Id).ToArray();
                        playerDto.Stamina = characterDtos.Where(x => x.PlayerId == playerDto.Id).Count() * 2;
                    }

                    if (null != vfcnDic && vfcnDic.ContainsKey(playerDto.Id))
                    {
                        int nowCharaCnt = 0;
                        int maxCharaCnt = vfcnDic[playerDto.Id];

                        if (null != vfDic && vfDic.ContainsKey(playerDto.Id))
                        {
                            int vfPlayerId = vfDic[playerDto.Id] * -1;

                            nowCharaCnt = playerCharaDtos.Where(x => x.PlayerId == vfPlayerId).Count();
                        }
                        else
                        {
                            nowCharaCnt = characterDtos.Where(x => x.PlayerId == playerDto.Id).Count();
                        }

                        if (maxCharaCnt > baseCharacterDtos.Length)
                        {
                            maxCharaCnt = baseCharacterDtos.Length;
                        }

                        if (nowCharaCnt < maxCharaCnt)
                        {
                            CharacterDto[] addCharacters = Common.Common.GetCharacterDifference(baseCharacterDtos, playerCharaDtos);

                            FormationDto lastDto = playerFDtos.OrderByDescending(v => v.Id).FirstOrDefault();

                            int pos = 0;
                            int cnt = 0;
                            int addFormationId = 0;

                            if (null != lastDto)
                            {
                                lastPlayerFormationId = lastDto.Id;
                                addFormationId = lastDto.Id + 1;
                            }
                            else
                            {
                                addFormationId = (lastPlayerFormationId * 10) + 1;
                            }

                            Dictionary<int, CharacterDto[]> newFormationDic = new Dictionary<int, CharacterDto[]>();
                            List<CharacterDto> newCharacterList = new List<CharacterDto>();

                            for (int i = nowCharaCnt; i < maxCharaCnt; i++)
                            {
                                addCharacters[pos].PlayerId = playerDto.Id;
                                addCharacters[pos].SortNo = i;
                                addCharacters[pos].FormationId = addFormationId;

                                newCharacterList.Add(addCharacters[pos]);

                                playerCharaDtos = playerCharaDtos.Append(addCharacters[pos]).ToArray();

                                pos++;
                                cnt++;

                                if (cnt > 4)
                                {
                                    newFormationDic.Add(addFormationId, newCharacterList.ToArray());

                                    addFormationId++;
                                    cnt = 0;
                                    newCharacterList = new List<CharacterDto>();
                                }
                            }

                            foreach (KeyValuePair<int, CharacterDto[]> keyValuePair in newFormationDic)
                            {
                                FormationDto formation = new FormationDto()
                                {
                                    Id = keyValuePair.Key,
                                    PlayerId = playerDto.Id,
                                    SortNo = keyValuePair.Key,
                                    Name = "自編" + keyValuePair.Key,
                                    IsDebuff = true,
                                    Characters = keyValuePair.Value
                                };

                                playerFDtos = playerFDtos.Append(formation).ToArray();
                            }

                            if (newCharacterList.Count > 0)
                            {
                                FormationDto formation = new FormationDto()
                                {
                                    Id = addFormationId,
                                    PlayerId = playerDto.Id,
                                    SortNo = addFormationId,
                                    Name = "自編" + addFormationId,
                                    IsDebuff = true,
                                    Characters = newCharacterList.ToArray()
                                };

                                playerFDtos = playerFDtos.Append(formation).ToArray();
                            }

                            if (null != vfDic && vfDic.ContainsKey(playerDto.Id) && null != vfcnDic && vfcnDic.ContainsKey(playerDto.Id))
                            {
                                int vfPlayerId = vfDic[playerDto.Id] * -1;

                                playerDto.Stamina = playerCharaDtos.Where(x => x.PlayerId == vfPlayerId).Count() * 2
                                                  + playerCharaDtos.Where(x => x.PlayerId == playerDto.Id).Count() * 2;

                            }
                            else if (null != vfDic && vfDic.ContainsKey(playerDto.Id))
                            {
                                int vfPlayerId = vfDic[playerDto.Id] * -1;

                                playerDto.Stamina = playerCharaDtos.Where(x => x.PlayerId == vfPlayerId).Count() * 2;
                            }
                            else
                            {
                                playerDto.Stamina = playerCharaDtos.Where(x => x.PlayerId == playerDto.Id).Count() * 2;
                            }
                        }
                    }

                    foreach (CharacterDto characterDto in playerCharaDtos)
                    {
                        playerDto.ForceValue += characterDto.ForceValue;
                    }

                    playerDto.Formations = playerFDtos;
                }

                if (playerDtos.Any(x => x.Id == id || x.Name.Contains(name) || x.World == world || x.GuildName.Contains(guildName)))
                {
                    playerDtos = playerDtos.Where(x => x.Id == id || x.Name.Contains(name) || x.World == world || x.GuildName.Contains(guildName)).ToArray();
                }
            }
            catch (Exception e)
            {
                throw e;
            }

            return playerDtos;
        }

        public ItemCollection GetGuildNameItem()
        {
            ListBox listBox = new ListBox();

            try
            {
                GuildDao guildDao = new GuildDao();

                GuildDto[] guildDtos = guildDao.GetGuildDtos();

                foreach (GuildDto guildDto in guildDtos)
                {
                    listBox.Items.Add(guildDto.Name);
                }
            }
            catch (Exception e)
            {
                throw e;
            }

            return listBox.Items;
        }

        public ItemCollection GetWorldNoItem()
        {
            ListBox listBox = new ListBox();

            try
            {
                // 全プレイヤーの一意ワールドリスト
                CSVLoader csvLoader = new CSVLoader();
                GuildDao guildDao = new GuildDao();

                // プレイヤー情報を取得
                PlayerDto[] playerDtos = csvLoader.ReadPlayerCsvFile(Common.Common.PLAYER_FILE_PATH);

                int[] guildIds = guildDao.GetGuildIdList();

                foreach (int guildId in guildIds)
                {
                    string playerPath = Common.Common.BASE_DATA_PATH + guildId + "/" + Common.Common.PLAYER_FILE_NAME;

                    PlayerDto[] pDtos = csvLoader.ReadPlayerCsvFile(playerPath);
                    playerDtos = playerDtos.Concat(pDtos).ToArray();
                }

                if (isDeleteFlg)
                {
                    playerDtos = playerDtos.Where(x => x.IsDelete == false).ToArray();
                }

                var result = playerDtos.OrderBy(value => value.World);

                if (null != result)
                {
                    playerDtos = result.Where(x => x.IsDelete == false).ToArray();
                }

                List<int> worldList = new List<int>();

                foreach (PlayerDto playerDto in playerDtos)
                {
                    if (!worldList.Contains(playerDto.World))
                    {
                        listBox.Items.Add(playerDto.World);
                        worldList.Add(playerDto.World);
                    }
                }
            }
            catch (Exception e)
            {
                throw e;
            }

            return listBox.Items;
        }
    }
}
