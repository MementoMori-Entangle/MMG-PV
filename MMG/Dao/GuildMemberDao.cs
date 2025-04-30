using MMG.Dto;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MMG.Dao
{
    public class GuildMemberDao
    {
        public GuildMemberDao() { }

        public GuildDto GetGuildMembers(string name, Dictionary<long, int> vfDic = null, Dictionary<long, int> vfcnDic = null)
        {
            GuildDto guildDto = null;

            try
            {
                GuildDao guildDao = new GuildDao();

                GuildDto[] guildDtos = guildDao.GetGuildDtos();

                guildDto = guildDtos.Where(x => x.Name == name).FirstOrDefault();

                if (null == guildDto)
                {
                    return guildDto;
                }

                PlayerDao playerDao = new PlayerDao(vfDic, vfcnDic);

                PlayerDto[] playerDtos = playerDao.GetAllInfoPlayer();

                Dictionary<int, int> guildMemberNumDic = new Dictionary<int, int>();
                Dictionary<int, int> guildMemberStaminaDic = new Dictionary<int, int>();
                Dictionary<int, long> guildMemberForceValDic = new Dictionary<int, long>();
                Dictionary<int, List<PlayerDto>> guildMembersDic = new Dictionary<int, List<PlayerDto>>();

                // ギルド毎のメンバー数をカウント
                foreach (PlayerDto playerDto in playerDtos)
                {
                    if (!name.Equals(playerDto.GuildName))
                    {
                        continue;
                    }

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
            }
            catch (Exception e)
            {
                throw e;
            }

            return guildDto;
        }

        public GuildDto[] GetGuildMembers(string name, int world)
        {
            GuildDto[] guildDtos;
            try
            {
                GuildDao guildDao = new GuildDao();

                guildDtos = guildDao.GetGuildDtos(name, world);

                PlayerDao playerDao = new PlayerDao();

                PlayerDto[] playerDtos = playerDao.GetAllInfoPlayer();

                Dictionary<int, int> guildMemberNumDic = new Dictionary<int, int>();
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
                foreach (GuildDto guildDto in guildDtos)
                {
                    if (guildMemberNumDic.ContainsKey(guildDto.Id))
                    {
                        guildDto.MemberNum = guildMemberNumDic[guildDto.Id];
                    }

                    if (guildMemberForceValDic.ContainsKey(guildDto.Id))
                    {
                        guildDto.ForceValue = guildMemberForceValDic[guildDto.Id];
                    }

                    if (guildMembersDic.ContainsKey(guildDto.Id))
                    {
                        guildDto.Members = guildMembersDic[guildDto.Id].ToArray();
                    }
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
