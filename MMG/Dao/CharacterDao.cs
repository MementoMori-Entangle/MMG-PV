using MMG.Common;
using MMG.Dto;
using System.Linq;
using System;
using System.Collections.Generic;
using System.IO;

namespace MMG.Dao
{
    public class CharacterDao
    {
        public CharacterDao() { }

        public void CreateCharacterFile(int guildId)
        {
            try
            {
                if (guildId < 1)
                {
                    return;
                }

                string filePath = Common.Common.BASE_DATA_PATH + guildId + "/" + Common.Common.CHARACTER_FILE_NAME;

                CSVWriter csvWriter = new CSVWriter();

                csvWriter.WriteCharacterCsvFile(new List<CharacterDto>(), filePath);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public void SaveCharacterDto(List<CharacterDto> characterDtoList, int guildId = 0, bool isBackup = true)
        {
            try
            {
                if (null == characterDtoList || characterDtoList.Count == 0)
                {
                    return;
                }

                string filePath = string.Empty;
                string bakFilePath = string.Empty;

                if (guildId > 0)
                {
                    filePath = Common.Common.BASE_DATA_PATH + guildId + "/" + Common.Common.CHARACTER_FILE_NAME;

                    if (isBackup)
                    {
                        string bakDir = Common.Common.BASE_DATA_PATH + guildId + "/"
                                      + Common.Common.BACKUP_DIR + Common.Common.CHARACTER_FILE_NAME + "/";

                        if (!Directory.Exists(bakDir))
                        {
                            Directory.CreateDirectory(bakDir);
                        }

                        bakFilePath = bakDir + DateTime.Now.ToString("yyyyMMddHHmmss") + "_" + Common.Common.CHARACTER_FILE_NAME;
                    }
                }
                else
                {
                    filePath = Common.Common.BASE_DATA_PATH + Common.Common.CHARACTER_FILE_NAME;

                    if (isBackup)
                    {
                        string bakDir = Common.Common.BASE_DATA_PATH + Common.Common.BACKUP_DIR + Common.Common.CHARACTER_FILE_NAME + "/";

                        if (!Directory.Exists(bakDir))
                        {
                            Directory.CreateDirectory(bakDir);
                        }

                        bakFilePath = bakDir + DateTime.Now.ToString("yyyyMMddHHmmss") + "_" + Common.Common.CHARACTER_FILE_NAME;
                    }
                }

                if (!string.IsNullOrEmpty(bakFilePath))
                {
                    File.Copy(filePath, bakFilePath);
                }

                CSVWriter csvWriter = new CSVWriter();

                csvWriter.WriteCharacterCsvFile(characterDtoList, filePath);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public CharacterDto[] GetCharacterDtos(int guildId)
        {
            CharacterDto[] characterDtos = null;

            try
            {
                CSVLoader csvLoader = new CSVLoader();

                if (guildId > 0)
                {
                    string charaPath = Common.Common.BASE_DATA_PATH + guildId + "/" + Common.Common.CHARACTER_FILE_NAME;

                    characterDtos = csvLoader.ReadCharacterCsvFile(charaPath);
                }
                else
                {
                    characterDtos = csvLoader.ReadCharacterCsvFile(Common.Common.CHARACTER_FILE_PATH);
                }

                var result = characterDtos.OrderBy(value => value.FormationId);

                if (null != result)
                {
                    characterDtos = result.ToList().ToArray();
                }
            }
            catch (Exception e)
            {
                throw e;
            }

            return characterDtos;
        }
    }
}
