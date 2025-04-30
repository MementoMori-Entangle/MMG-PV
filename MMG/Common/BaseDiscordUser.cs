using MMG.Dao;
using MMG.Dto;
using System.Collections.Generic;

namespace MMG.Common
{
    public class BaseDiscordUser
    {
        public static Dictionary<ulong, ulong> GetUserDedicatedChannelIdDic()
        {
            DiscordDao discordDao = new DiscordDao();

            DiscordDto[] discordDtos = discordDao.GetDiscordDtos();

            Dictionary<ulong, ulong> userDedicatedChannelIdDic = new Dictionary<ulong, ulong>();

            foreach (DiscordDto discordDto in discordDtos)
            {
                userDedicatedChannelIdDic.Add(discordDto.Id, discordDto.DedicatedChannelId);
            }

            return userDedicatedChannelIdDic;
        }

        public static Dictionary<ulong, bool> GetUserRCPermissionDic()
        {
            DiscordDao discordDao = new DiscordDao();

            DiscordDto[] discordDtos = discordDao.GetDiscordDtos();

            Dictionary<ulong, bool> userRCPermissionDic = new Dictionary<ulong, bool>();

            foreach (DiscordDto discordDto in discordDtos)
            {
                bool isPermission = false;

                if (discordDto.RemoteControlPermission == 1)
                {
                    isPermission = true;
                }

                userRCPermissionDic.Add(discordDto.Id, isPermission);
            }

            return userRCPermissionDic;
        }

        public static Dictionary<long, ulong> GetMMUserIdUserIdDic()
        {
            DiscordDao discordDao = new DiscordDao();

            DiscordDto[] discordDtos = discordDao.GetDiscordDtos();

            Dictionary<long, ulong> mmUserIdUserIdDic = new Dictionary<long, ulong>();

            foreach (DiscordDto discordDto in discordDtos)
            {
                mmUserIdUserIdDic.Add(discordDto.MMId, discordDto.Id);
            }

            return mmUserIdUserIdDic;
        }

        public static Dictionary<ulong, long> GetUserIdMMUserIdDic()
        {
            DiscordDao discordDao = new DiscordDao();

            DiscordDto[] discordDtos = discordDao.GetDiscordDtos();

            Dictionary<ulong, long> userIdMMUserIdDic = new Dictionary<ulong, long>();

            foreach (DiscordDto discordDto in discordDtos)
            {
                userIdMMUserIdDic.Add(discordDto.Id, discordDto.MMId);
            }

            return userIdMMUserIdDic;
        }

        public static Dictionary<ulong, string> GetUserIdNameDic()
        {
            DiscordDao discordDao = new DiscordDao();

            DiscordDto[] discordDtos = discordDao.GetDiscordDtos();

            Dictionary<ulong, string> userIdNameDic = new Dictionary<ulong, string>();

            foreach (DiscordDto discordDto in discordDtos)
            {
                userIdNameDic.Add(discordDto.Id, discordDto.UserName);
            }

            return userIdNameDic;
        }
    }
}
