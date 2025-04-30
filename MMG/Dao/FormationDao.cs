using MMG.Common;
using MMG.Dto;
using System.Linq;
using System;
using System.Collections.Generic;
using System.IO;

namespace MMG.Dao
{
    public class FormationDao
    {
        private bool isGVS;

        public FormationDao()
        {
            isGVS = false;
        }

        public FormationDao(bool isGVS)
        {
            this.isGVS = isGVS;
        }

        public FormationDto GetFormationDto(int formationId)
        {
            FormationDto formationDto = null;

            try
            {
                CSVLoader csvLoader = new CSVLoader();
                GuildDao guildDao = new GuildDao();

                int[] guildIds = guildDao.GetGuildIdList();

                FormationDto[] formationDtos = csvLoader.ReadFormationCsvFile(Common.Common.FORMATION_FILE_PATH);

                foreach (int guildId in guildIds)
                {
                    string guildPath = Common.Common.BASE_DATA_PATH + guildId + "/" + Common.Common.FORMATION_FILE_NAME;

                    FormationDto[] fDtos = csvLoader.ReadFormationCsvFile(guildPath);
                    formationDtos = formationDtos.Concat(fDtos).ToArray();
                }

                if (isGVS)
                {
                    formationDtos = formationDtos.Where(x => x.IsGVS == isGVS).ToArray();
                }

                formationDto = formationDtos.Where(x => x.Id == formationId).FirstOrDefault();
            }
            catch (Exception e)
            {
                throw e;
            }

            return formationDto;
        }

        public void CreateFormationFile(int guildId)
        {
            try
            {
                if (guildId < 1)
                {
                    return;
                }

                string filePath = Common.Common.BASE_DATA_PATH + guildId + "/" + Common.Common.FORMATION_FILE_NAME;

                CSVWriter csvWriter = new CSVWriter();

                csvWriter.WriteFormationCsvFile(new List<FormationDto>(), filePath);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public void SaveFormationDto(List<FormationDto> formationDtoList, int guildId = 0, bool isBackup = true)
        {
            try
            {
                if (null == formationDtoList || formationDtoList.Count == 0)
                {
                    return;
                }

                string filePath = string.Empty;
                string bakFilePath = string.Empty;

                if (guildId > 0)
                {
                    filePath = Common.Common.BASE_DATA_PATH + guildId + "/" + Common.Common.FORMATION_FILE_NAME;

                    if (isBackup)
                    {
                        string bakDir = Common.Common.BASE_DATA_PATH + guildId + "/"
                                      + Common.Common.BACKUP_DIR + Common.Common.FORMATION_FILE_NAME + "/";

                        if (!Directory.Exists(bakDir))
                        {
                            Directory.CreateDirectory(bakDir);
                        }

                        bakFilePath = bakDir + DateTime.Now.ToString("yyyyMMddHHmmss") + "_" + Common.Common.FORMATION_FILE_NAME;
                    }
                }
                else
                {
                    filePath = Common.Common.BASE_DATA_PATH + Common.Common.FORMATION_FILE_NAME;

                    if (isBackup)
                    {
                        string bakDir = Common.Common.BASE_DATA_PATH + Common.Common.BACKUP_DIR + Common.Common.FORMATION_FILE_NAME + "/";

                        if (!Directory.Exists(bakDir))
                        {
                            Directory.CreateDirectory(bakDir);
                        }

                        bakFilePath = bakDir + DateTime.Now.ToString("yyyyMMddHHmmss") + "_" + Common.Common.FORMATION_FILE_NAME;
                    }
                }

                if (!string.IsNullOrEmpty(bakFilePath))
                {
                    File.Copy(filePath, bakFilePath);
                }

                CSVWriter csvWriter = new CSVWriter();

                csvWriter.WriteFormationCsvFile(formationDtoList, filePath);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public int NextSortNo(int guildId, long playerId)
        {
            int sortNo = 1;

            try
            {
                FormationDto[] formationDtos = GetFormationDtos(guildId);

                if (null == formationDtos || formationDtos.Length == 0)
                {
                    return sortNo;
                }

                FormationDto[] playerFormationDtos = formationDtos.Where(x => x.PlayerId == playerId).ToArray();

                if (null == playerFormationDtos || playerFormationDtos.Length == 0)
                {
                    return sortNo;
                }

                FormationDto formationDto = playerFormationDtos.OrderByDescending(x => x.SortNo).FirstOrDefault();

                if (null != formationDto)
                {
                    sortNo = formationDto.SortNo + 1;
                }
            }
            catch (Exception e)
            {
                throw e;
            }

            return sortNo;
        }

        public FormationDto[] GetFormationDtos(int guildId)
        {
            FormationDto[] formationDtos = null;

            try
            {
                CSVLoader csvLoader = new CSVLoader();

                if (guildId > 0)
                {
                    string formationPath = Common.Common.BASE_DATA_PATH + guildId + "/" + Common.Common.FORMATION_FILE_NAME;

                    formationDtos = csvLoader.ReadFormationCsvFile(formationPath);
                }
                else
                {
                    formationDtos = csvLoader.ReadFormationCsvFile(Common.Common.FORMATION_FILE_PATH);
                }

                if (isGVS)
                {
                    formationDtos = formationDtos.Where(x => x.IsGVS == isGVS).ToArray();
                }

                var result = formationDtos.OrderBy(value => value.Id);

                if (null != result)
                {
                    formationDtos = result.ToList().ToArray();
                }
            }
            catch (Exception e)
            {
                throw e;
            }

            return formationDtos;
        }

        public FormationDto[] GetFormationDtos(List<int> guildList = null)
        {
            FormationDto[] formationDtos = null;

            try
            {
                CSVLoader csvLoader = new CSVLoader();

                formationDtos = csvLoader.ReadFormationCsvFile(Common.Common.FORMATION_FILE_PATH);

                if (null == guildList)
                {
                    GuildDao guildDao = new GuildDao();

                    int[] guildIds = guildDao.GetGuildIdList();

                    guildList = guildIds.ToList();
                }

                // 各ギルド毎の編成をロードして結合
                foreach (int guildId in guildList)
                {
                    string guildPath = Common.Common.BASE_DATA_PATH + guildId + "/" + Common.Common.FORMATION_FILE_NAME;

                    FormationDto[] fDtos = csvLoader.ReadFormationCsvFile(guildPath);
                    formationDtos = formationDtos.Concat(fDtos).ToArray();
                }

                if (isGVS)
                {
                    formationDtos = formationDtos.Where(x => x.IsGVS == isGVS).ToArray();
                }

                var result = formationDtos.OrderBy(value => value.Id);

                if (null != result)
                {
                    formationDtos = result.ToList().ToArray();
                }
            }
            catch (Exception e)
            {
                throw e;
            }

            return formationDtos;
        }
    }
}
