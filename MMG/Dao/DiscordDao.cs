using MMG.Common;
using MMG.Dto;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MMG.Dao
{
    public class DiscordDao
    {
        public DiscordDao() { }


        public void SaveDiscordDto(List<DiscordDto> discordDtoList, bool isBackup = true)
        {
            try
            {
                if (null == discordDtoList || discordDtoList.Count == 0)
                {
                    return;
                }

                string filePath = Common.Common.BASE_DATA_PATH + Common.Common.DISCORD_FILE_NAME;
                string bakFilePath = string.Empty;

                if (isBackup)
                {
                    string bakDir = Common.Common.BASE_DATA_PATH + Common.Common.BACKUP_DIR + Common.Common.DISCORD_FILE_NAME + "/";

                    if (!Directory.Exists(bakDir))
                    {
                        Directory.CreateDirectory(bakDir);
                    }

                    bakFilePath = bakDir + DateTime.Now.ToString("yyyyMMddHHmmss") + "_" + Common.Common.DISCORD_FILE_NAME;
                }

                if (!string.IsNullOrEmpty(bakFilePath))
                {
                    File.Copy(filePath, bakFilePath);
                }

                CSVWriter csvWriter = new CSVWriter();

                csvWriter.WriteDiscordCsvFile(discordDtoList, filePath);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public bool IsId(ulong id)
        {
            bool isId = false;

            try
            {
                DiscordDto[] discordDtos = GetDiscordDtos();

                if (discordDtos.Where(x => x.Id == id).Any())
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

        public DiscordDto[] GetDiscordDtos()
        {
            DiscordDto[] discordDtos;

            try
            {
                CSVLoader csvLoader = new CSVLoader();

                discordDtos = csvLoader.ReadDiscordCsvFile(Common.Common.DISCORD_FILE_PATH);
            }
            catch (Exception e)
            {
                throw e;
            }

            return discordDtos;
        }
    }
}
