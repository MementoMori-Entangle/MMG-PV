using MMG.Common;
using MMG.Dto;
using System.Linq;
using System;
using System.Collections.Generic;
using System.IO;

namespace MMG.Dao
{
    public class FormationVSFormationDao
    {
        public FormationVSFormationDao() { }

        public void SaveFormationVSFormationDto(List<FormationVSFormationDto> fVSFDtoList, bool isBackup = true)
        {
            try
            {
                if (null == fVSFDtoList || fVSFDtoList.Count == 0)
                {
                    return;
                }

                string filePath = Common.Common.BASE_DATA_PATH + Common.Common.FORMATIONVSFORMATION_FILE_NAME;
                string bakFilePath = string.Empty;

                if (isBackup)
                {
                    string bakDir = Common.Common.BASE_DATA_PATH + Common.Common.BACKUP_DIR + Common.Common.FORMATIONVSFORMATION_FILE_NAME + "/";

                    if (!Directory.Exists(bakDir))
                    {
                        Directory.CreateDirectory(bakDir);
                    }

                    bakFilePath = bakDir + DateTime.Now.ToString("yyyyMMddHHmmss") + "_" + Common.Common.FORMATIONVSFORMATION_FILE_NAME;
                }

                if (!string.IsNullOrEmpty(bakFilePath))
                {
                    File.Copy(filePath, bakFilePath);
                }

                CSVWriter csvWriter = new CSVWriter();

                csvWriter.WriteFormationVSFormationCsvFile(fVSFDtoList, filePath);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public FormationVSFormationDto[] GetFormationVSFormationDtos()
        {
            FormationVSFormationDto[] formationVSFormationDtos = null;

            try
            {
                CSVLoader csvLoader = new CSVLoader();

                formationVSFormationDtos = csvLoader.ReadFormationVSFormationCsvFile(Common.Common.FORMATIONVSFORMATION_FILE_PATH);

                var result = formationVSFormationDtos.OrderBy(value => value.UId);

                if (null != result)
                {
                    formationVSFormationDtos = result.ToList().ToArray();
                }
            }
            catch (Exception e)
            {
                throw e;
            }

            return formationVSFormationDtos;
        }
    }
}
