using MMG.Common;
using MMG.Dto;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MMG.Dao
{
    public class PlayerVSPlayerDao
    {
        public PlayerVSPlayerDao() { }

        public void SavePlayerVSPlayerDto(List<PlayerVSPlayerDto> pVSPDtoList, bool isBackup = true)
        {
            try
            {
                if (null == pVSPDtoList || pVSPDtoList.Count == 0)
                {
                    return;
                }

                string filePath = Common.Common.BASE_DATA_PATH + Common.Common.PLAYERVSPLAYER_FILE_NAME;
                string bakFilePath = string.Empty;

                if (isBackup)
                {
                    string bakDir = Common.Common.BASE_DATA_PATH + Common.Common.BACKUP_DIR + Common.Common.PLAYERVSPLAYER_FILE_NAME + "/";

                    if (!Directory.Exists(bakDir))
                    {
                        Directory.CreateDirectory(bakDir);
                    }

                    bakFilePath = bakDir + DateTime.Now.ToString("yyyyMMddHHmmss") + "_" + Common.Common.PLAYERVSPLAYER_FILE_NAME;
                }

                if (!string.IsNullOrEmpty(bakFilePath))
                {
                    File.Copy(filePath, bakFilePath);
                }

                CSVWriter csvWriter = new CSVWriter();

                csvWriter.WritePlayerVSPlayerCsvFile(pVSPDtoList, filePath);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public PlayerVSPlayerDto[] GetPlayerVSPlayerDtos()
        {
            PlayerVSPlayerDto[] playerVSPlayerDtos = null;

            try
            {
                CSVLoader csvLoader = new CSVLoader();

                playerVSPlayerDtos = csvLoader.ReadPlayerVSPlayerCsvFile(Common.Common.PLAYERVSPLAYER_FILE_PATH);

                var result = playerVSPlayerDtos.OrderBy(value => value.UId);

                if (null != result)
                {
                    playerVSPlayerDtos = result.ToList().ToArray();
                }
            }
            catch (Exception e)
            {
                throw e;
            }

            return playerVSPlayerDtos;
        }
    }
}
