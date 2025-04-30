using MMG.Common;
using MMG.Dto;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MMG.Dao
{
    public class GuildDao
    {
        private bool isDeleteFlg;

        public GuildDao()
        {
            isDeleteFlg = true;
        }

        public GuildDao(bool isDeleteFlg = true)
        {
            this.isDeleteFlg = isDeleteFlg;
        }

        public void SaveGuildDto(List<GuildDto> guildDtoList, int id = 0, bool isBackup = true)
        {
            try
            {
                if (null == guildDtoList || guildDtoList.Count == 0)
                {
                    return;
                }

                string filePath = Common.Common.BASE_DATA_PATH + Common.Common.GUILD_FILE_NAME;
                string bakFilePath = string.Empty;

                if (isBackup)
                {
                    string bakDir = Common.Common.BASE_DATA_PATH + Common.Common.BACKUP_DIR + Common.Common.GUILD_FILE_NAME + "/";

                    if (!Directory.Exists(bakDir))
                    {
                        Directory.CreateDirectory(bakDir);
                    }

                    bakFilePath = bakDir + DateTime.Now.ToString("yyyyMMddHHmmss") + "_" + Common.Common.GUILD_FILE_NAME;
                }

                if (!string.IsNullOrEmpty(bakFilePath))
                {
                    File.Copy(filePath, bakFilePath);
                }

                CSVWriter csvWriter = new CSVWriter();

                csvWriter.WriteGuildCsvFile(guildDtoList, filePath);

                // ギルドIDに対するギルドフォルダが存在と必須ファイルが存在しない場合は、生成する。
                if (id < 1)
                {
                    return;
                }

                string guildFolderPath = Common.Common.BASE_DATA_PATH + id + "/";

                if (!Directory.Exists(guildFolderPath))
                {
                    Directory.CreateDirectory(guildFolderPath);
                }

                string playerFilePath = guildFolderPath + Common.Common.PLAYER_FILE_NAME;
                string formationFilePath = guildFolderPath + Common.Common.FORMATION_FILE_NAME;
                string characterFilePath = guildFolderPath + Common.Common.CHARACTER_FILE_NAME;

                if (!File.Exists(playerFilePath))
                {
                    PlayerDao playerDao = new PlayerDao();
                    playerDao.CreatePlayerFile(id);
                }

                if (!File.Exists(formationFilePath))
                {
                    FormationDao formationDao = new FormationDao();
                    formationDao.CreateFormationFile(id);
                }

                if (!File.Exists(characterFilePath))
                {
                    CharacterDao characterDao = new CharacterDao();
                    characterDao.CreateCharacterFile(id);
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public GuildDto GetGuildDto(int id)
        {
            GuildDto guildDto = null;

            try
            {
                CSVLoader csvLoader = new CSVLoader();

                GuildDto[] guildDtos = csvLoader.ReadGuildCsvFile(Common.Common.GUILD_FILE_PATH);

                if (isDeleteFlg)
                {
                    guildDtos = guildDtos.Where(x => x.IsDelete == false).ToArray();
                }

                guildDto = guildDtos.Where(x => x.Id == id).FirstOrDefault();
            }
            catch (Exception e)
            {
                throw e;
            }

            return guildDto;
        }

        public int GetGuildId(string name)
        {
            int guildId = 0;

            try
            {
                GuildDto[] guildDtos = GetGuildDtos();

                if (isDeleteFlg)
                {
                    guildDtos = guildDtos.Where(x => x.IsDelete == false).ToArray();
                }

                GuildDto guildDto = guildDtos.Where(x => x.Name.Equals(name)).FirstOrDefault();

                if (null != guildDto)
                {
                    guildId = guildDto.Id;
                }
            }
            catch (Exception e)
            {
                throw e;
            }

            return guildId;
        }

        public int[] GetGuildIdList(string name = null)
        {
            List<int> idList = new List<int>();

            try
            {
                GuildDto[] guildDtos = GetGuildDtos();

                if (isDeleteFlg)
                {
                    guildDtos = guildDtos.Where(x => x.IsDelete == false).ToArray();
                }

                if (!string.IsNullOrEmpty(name))
                {
                    guildDtos = guildDtos.Where(x => x.Name.Equals(name)).ToArray();
                }

                foreach (GuildDto guildDto in guildDtos)
                {
                    idList.Add(guildDto.Id);
                }
            }
            catch (Exception e)
            {
                throw e;
            }

            return idList.ToArray();
        }

        public GuildDto[] GetGuildDtos(string name = null, int world = 0)
        {
            GuildDto[] guildDtos = null;

            try
            {
                CSVLoader csvLoader = new CSVLoader();

                guildDtos = csvLoader.ReadGuildCsvFile(Common.Common.GUILD_FILE_PATH);

                if (isDeleteFlg)
                {
                    guildDtos = guildDtos.Where(x => x.IsDelete == false).ToArray();
                }

                guildDtos = guildDtos.Where(x => x.Name.Contains(name) || x.World == world)
                                     .OrderBy(value => value.No).ToArray();
            }
            catch (Exception e)
            {
                throw e;
            }

            return guildDtos;
        }

        public GuildDto[] GetGuildDtos()
        {
            GuildDto[] guildDtos = null;

            try
            {
                CSVLoader csvLoader = new CSVLoader();

                guildDtos = csvLoader.ReadGuildCsvFile(Common.Common.GUILD_FILE_PATH);

                if (isDeleteFlg)
                {
                    guildDtos = guildDtos.Where(x => x.IsDelete == false).ToArray();
                }

                var result = guildDtos.OrderBy(value => value.No);

                if (null != result)
                {
                    guildDtos = result.ToList().ToArray();
                }
            }
            catch (Exception e)
            {
                throw e;
            }

            return guildDtos;
        }
    }
}
