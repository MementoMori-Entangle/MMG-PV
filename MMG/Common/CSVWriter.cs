using CsvHelper;
using CsvHelper.Configuration;
using MMG.csv;
using MMG.Dto;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace MMG.Common
{
    public class CSVWriter
    {
        IWriterConfiguration config;

        public CSVWriter()
        {
            config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                Delimiter = ",",
                NewLine = Environment.NewLine,
                IgnoreBlankLines = true,
                Encoding = Encoding.UTF8,
                AllowComments = true,
                Comment = '#',
                DetectColumnCountChanges = true,
                TrimOptions = TrimOptions.Trim,
                //ShouldQuote = (context) => true,
            };
        }

        public void WritePlayerVSPlayerCsvFile(List<PlayerVSPlayerDto> pVSPDtoList, string filePath)
        {
            try
            {
                List<PlayerVSPlayerCSV> pVSPCSV = new List<PlayerVSPlayerCSV>();

                foreach (PlayerVSPlayerDto dto in pVSPDtoList)
                {
                    pVSPCSV.Add(new PlayerVSPlayerCSV()
                    {
                        UId = dto.UId,
                        WinPlayerId = dto.WinPlayerId,
                        LosePlayerId = dto.LosePlayerId
                    });
                }

                using (var writer = new StreamWriter(filePath))
                {
                    using (var csv = new CsvWriter(writer, config))
                    {
                        csv.WriteRecords(pVSPCSV);
                    }
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public void WriteFormationVSFormationCsvFile(List<FormationVSFormationDto> fVSFDtoList, string filePath)
        {
            try
            {
                List<FormationVSFormationCSV> fVSFCSV = new List<FormationVSFormationCSV>();

                foreach (FormationVSFormationDto dto in fVSFDtoList)
                {
                    fVSFCSV.Add(new FormationVSFormationCSV()
                    {
                        UId = dto.UId,
                        WinFormationId = dto.WinFormationId,
                        LoseFormationId = dto.LoseFormationId,
                        DebuffKONum = dto.DebuffKONum
                    });
                }

                using (var writer = new StreamWriter(filePath))
                {
                    using (var csv = new CsvWriter(writer, config))
                    {
                        csv.WriteRecords(fVSFCSV);
                    }
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public void WriteCharacterCsvFile(List<CharacterDto> characterDtoList, string filePath)
        {
            try
            {
                List<CharacterCSV> formationCSV = new List<CharacterCSV>();

                foreach (CharacterDto dto in characterDtoList)
                {
                    formationCSV.Add(new CharacterCSV()
                    {
                        Id = dto.Id,
                        FormationId = dto.FormationId,
                        PlayerId = dto.PlayerId,
                        SortNo = dto.SortNo,
                        Name = dto.Name,
                        Rarity = dto.Rarity,
                        Level = dto.Level,
                        ForceValue = dto.ForceValue,
                        SpeedRune1 = dto.SpeedRune1,
                        SpeedRune2 = dto.SpeedRune2,
                        SpeedRune3 = dto.SpeedRune3,
                        PiercingRune1 = dto.PiercingRune1,
                        PiercingRune2 = dto.PiercingRune2,
                        PiercingRune3 = dto.PiercingRune3,
                        LRNum = dto.LRNum,
                        URNum = dto.URNum,
                        SSRNum = dto.SSRNum,
                        SRNum = dto.SRNum,
                        RNum = dto.RNum,
                        EWeapon = dto.EWeapon
                    });
                }

                using (var writer = new StreamWriter(filePath))
                {
                    using (var csv = new CsvWriter(writer, config))
                    {
                        csv.WriteRecords(formationCSV);
                    }
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public void WriteFormationCsvFile(List<FormationDto> formationDtoList, string filePath)
        {
            try
            {
                List<FormationCSV> formationCSV = new List<FormationCSV>();

                foreach (FormationDto dto in formationDtoList)
                {
                    formationCSV.Add(new FormationCSV()
                    {
                        Id = dto.Id,
                        PlayerId = dto.PlayerId,
                        SortNo = dto.SortNo,
                        Name = dto.Name,
                        IsSubParty = dto.IsSubParty,
                        IsDebuff = dto.IsDebuff,
                        Description = dto.Description,
                        IsGVS = dto.IsGVS
                    });
                }

                using (var writer = new StreamWriter(filePath))
                {
                    using (var csv = new CsvWriter(writer, config))
                    {
                        csv.WriteRecords(formationCSV);
                    }
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public void WriteDiscordCsvFile(List<DiscordDto> discordDtoList, string filePath)
        {
            try
            {
                List<DiscordCSV> discordCSV = new List<DiscordCSV>();

                foreach (DiscordDto dto in discordDtoList)
                {
                    discordCSV.Add(new DiscordCSV()
                    {
                        Id = dto.Id,
                        UserName = dto.UserName,
                        MMId = dto.MMId,
                        RemoteControlPermission = dto.RemoteControlPermission,
                        DedicatedChannelId = dto.DedicatedChannelId
                    });
                }

                using (var writer = new StreamWriter(filePath))
                {
                    using (var csv = new CsvWriter(writer, config))
                    {
                        csv.WriteRecords(discordCSV);
                    }
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public void WritePlayerCsvFile(List<PlayerDto> playerDtoList, string filePath)
        {
            try
            {
                List<PlayerCSV> playerCSV = new List<PlayerCSV>();

                foreach (PlayerDto dto in playerDtoList)
                {
                    playerCSV.Add(new PlayerCSV()
                    {
                        UId = dto.UId,
                        Id = dto.Id,
                        No = dto.No,
                        World = dto.World,
                        GuildId = dto.GuildId,
                        GuildName = dto.GuildName,
                        Name = dto.Name,
                        ForceValue = dto.ForceValue,
                        IsVC = dto.IsVC,
                        VirtualityFormation = dto.VirtualityFormation,
                        VFCNum = dto.VFCNum,
                        GBGroupId = dto.GBGroupId,
                        IsDelete = dto.IsDelete
                    });
                }

                using (var writer = new StreamWriter(filePath))
                {
                    using (var csv = new CsvWriter(writer, config))
                    {
                        csv.WriteRecords(playerCSV);
                    }
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public void WriteGuildCsvFile(List<GuildDto> guildDtoList, string filePath)
        {
            try
            {
                List<GuildCSV> guildCSV = new List<GuildCSV>();

                foreach (GuildDto dto in guildDtoList)
                {
                    guildCSV.Add(new GuildCSV()
                    {
                        Id = dto.Id,
                        No = dto.No,
                        World = dto.World,
                        Name = dto.Name,
                        Remarks = dto.Remarks,
                        IsDelete = dto.IsDelete
                    });
                }

                using (var writer = new StreamWriter(filePath))
                {
                    using (var csv = new CsvWriter(writer, config))
                    {
                        csv.WriteRecords(guildCSV);
                    }
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}
