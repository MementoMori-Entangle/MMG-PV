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
    public class CSVLoader
    {
        IReaderConfiguration config;

        public CSVLoader()
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
            };
        }

        public DiscordDto[] ReadDiscordCsvFile(string fileName)
        {
            List<DiscordDto> discordDtoList = new List<DiscordDto>();

            try
            {
                using (var reader = new StreamReader(fileName, Encoding.UTF8))
                using (var csv = new CsvReader(reader, config))
                {
                    var records = csv.GetRecords<DiscordDto>();

                    foreach (var record in records)
                    {
                        DiscordDto discordDto = new DiscordDto
                        {
                            Id = record.Id,
                            UserName = record.UserName,
                            MMId = record.MMId,
                            RemoteControlPermission = record.RemoteControlPermission,
                            DedicatedChannelId = record.DedicatedChannelId
                        };

                        discordDtoList.Add(discordDto);
                    }
                }
            }
            catch (Exception e)
            {
                throw e;
            }

            return discordDtoList.ToArray();
        }

        public FormationVSFormationDto[] ReadFormationVSFormationCsvFile(string fileName)
        {
            List<FormationVSFormationDto> formationVSFormationDtoList = new List<FormationVSFormationDto>();

            try
            {
                using (var reader = new StreamReader(fileName, Encoding.UTF8))
                using (var csv = new CsvReader(reader, config))
                {
                    var records = csv.GetRecords<FormationVSFormationDto>();

                    foreach (var record in records)
                    {
                        FormationVSFormationDto formationVSFormationDto = new FormationVSFormationDto
                        {
                            UId = record.UId,
                            WinFormationId = record.WinFormationId,
                            LoseFormationId = record.LoseFormationId,
                            DebuffKONum = record.DebuffKONum
                        };

                        formationVSFormationDtoList.Add(formationVSFormationDto);
                    }
                }
            }
            catch (Exception e)
            {
                throw e;
            }

            return formationVSFormationDtoList.ToArray();
        }

        public PlayerVSPlayerDto[] ReadPlayerVSPlayerCsvFile(string fileName)
        {
            List<PlayerVSPlayerDto> playerVSPlayerDtoList = new List<PlayerVSPlayerDto>();

            try
            {
                using (var reader = new StreamReader(fileName, Encoding.UTF8))
                using (var csv = new CsvReader(reader, config))
                {
                    var records = csv.GetRecords<PlayerVSPlayerDto>();

                    foreach (var record in records)
                    {
                        PlayerVSPlayerDto playerVSPlayerDto = new PlayerVSPlayerDto
                        {
                            UId = record.UId,
                            WinPlayerId = record.WinPlayerId,
                            LosePlayerId = record.LosePlayerId
                        };

                        playerVSPlayerDtoList.Add(playerVSPlayerDto);
                    }
                }
            }
            catch (Exception e)
            {
                throw e;
            }

            return playerVSPlayerDtoList.ToArray();
        }

        public GuildDto[] ReadGuildCsvFile(string fileName)
        {
            List<GuildDto> guildDtoList = new List<GuildDto>();

            try
            {
                using (var reader = new StreamReader(fileName, Encoding.UTF8))
                using (var csv = new CsvReader(reader, config))
                {
                    var records = csv.GetRecords<GuildCSV>();

                    foreach (var record in records)
                    {
                        GuildDto guildDto = new GuildDto
                        {
                            Id = record.Id,
                            No = record.No,
                            World = record.World,
                            Name = record.Name,
                            Remarks = record.Remarks,
                            IsDelete = record.IsDelete
                        };

                        guildDtoList.Add(guildDto);
                    }
                }
            }
            catch (Exception e)
            {
                throw e;
            }

            return guildDtoList.ToArray();
        }

        public CharacterDto[] ReadCharacterCsvFile(string fileName)
        {
            List<CharacterDto> characterDtoList = new List<CharacterDto>();

            try
            {
                using (var reader = new StreamReader(fileName, Encoding.UTF8))
                using (var csv = new CsvReader(reader, config))
                {
                    var records = csv.GetRecords<CharacterCSV>();

                    foreach (var record in records)
                    {
                        CharacterDto characterDto = new CharacterDto
                        {
                            Id = record.Id,
                            FormationId = record.FormationId,
                            PlayerId = record.PlayerId,
                            SortNo = record.SortNo,
                            Name = record.Name,
                            Rarity = record.Rarity,
                            Level = record.Level,
                            ForceValue = record.ForceValue,
                            SpeedRune1 = record.SpeedRune1,
                            SpeedRune2 = record.SpeedRune2,
                            SpeedRune3 = record.SpeedRune3,
                            PiercingRune1 = record.PiercingRune1,
                            PiercingRune2 = record.PiercingRune2,
                            PiercingRune3 = record.PiercingRune3,
                            LRNum = record.LRNum,
                            URNum = record.URNum,
                            SSRNum = record.SSRNum,
                            SRNum = record.SRNum,
                            RNum = record.RNum,
                            EWeapon = record.EWeapon
                        };

                        characterDtoList.Add(characterDto);
                    }
                }
            }
            catch (Exception e)
            {
                throw e;
            }

            return characterDtoList.ToArray();
        }

        public FormationDto[] ReadFormationCsvFile(string fileName)
        {
            List<FormationDto> formationDtoList = new List<FormationDto>();

            try
            {
                using (var reader = new StreamReader(fileName, Encoding.UTF8))
                using (var csv = new CsvReader(reader, config))
                {
                    var records = csv.GetRecords<FormationCSV>();

                    foreach (var record in records)
                    {
                        FormationDto formationDto = new FormationDto
                        {
                            Id = record.Id,
                            PlayerId = record.PlayerId,
                            SortNo = record.SortNo,
                            Name = record.Name,
                            IsSubParty = record.IsSubParty,
                            IsDebuff = record.IsDebuff,
                            Description = record.Description,
                            IsGVS = record.IsGVS
                        };

                        formationDtoList.Add(formationDto);
                    }
                }
            }
            catch (Exception e)
            {
                throw e;
            }

            return formationDtoList.ToArray();
        }

        public PlayerDto[] ReadPlayerCsvFile(string fileName)
        {
            List<PlayerDto> playerDtoList = new List<PlayerDto>();

            try
            {
                using (var reader = new StreamReader(fileName, Encoding.UTF8))
                using (var csv = new CsvReader(reader, config))
                {
                    var records = csv.GetRecords<PlayerCSV>();

                    foreach (var record in records)
                    {
                        PlayerDto playerDto = new PlayerDto
                        {
                            UId = record.UId,
                            Id = record.Id,
                            No = record.No,
                            World = record.World,
                            GuildId = record.GuildId,
                            GuildName = record.GuildName,
                            Name = record.Name,
                            ForceValue = record.ForceValue,
                            IsVC = record.IsVC,
                            VirtualityFormation = record.VirtualityFormation,
                            VFCNum = record.VFCNum,
                            GBGroupId = record.GBGroupId,
                            IsDelete = record.IsDelete
                        };

                        playerDtoList.Add(playerDto);
                    }
                }
            }
            catch (Exception e)
            {
                throw e;
            }

            return playerDtoList.ToArray();
        }
    }
}
