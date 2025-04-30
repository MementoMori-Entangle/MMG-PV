using Discord;
using Discord.WebSocket;
using MMG.Common;
using MMG.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace MMG.Discord
{
    public class Program
    {
        private readonly int MAX_MESSAGE_LENGTH = 1950; // 最大2000文字(課金していると4000文字)制限

        private string botToken = Common.Common.BOT_TOKEN;
        private ulong adminClientId = Common.Common.DISCORD_SERVER_ADMIN_CLIENT_ID;
        private ulong weekPassChannelId = Common.Common.DISCORD_WEEK_PASS_CHANNEL_ID;
        private ulong botClientId = Common.Common.DISCORD_BOT_CLIENT_ID;
        private ulong mmgpChannelId = Common.Common.DISCORD_MMGP_CHANNEL_ID;
        private string weekPassClientIds = Common.Common.DISCORD_WEEK_PASS_CHANNEL_CLIENT_IDS;

        private DiscordSocketClient Client;

        public GuildVSAdmin GuildVSAdmin { get; set; }
        public ListBox ListBox { get; set; }
        public List<GuildBattleProgressDto> GuildBPList { get; set; }
        public AuthDto AuthDto { get; set; }
        public string Labstack1 { get; set; }
        public string Labstack2 { get; set; }
        public string Labstack3 { get; set; }
        public string LabMemberOnlineST { get; set; }
        public string LabMemberOnline { get; set; }

        private async Task Client_MessageReceived(SocketMessage arg)
        {
            if (arg.Author.IsBot || botClientId == arg.Author.Id)
            {
                return;
            }

            string result = string.Empty;

            Dictionary<ulong, ulong> userDedicatedChannelIdDic = BaseDiscordUser.GetUserDedicatedChannelIdDic();
            Dictionary<ulong, long> userIdMMUserIdDic = BaseDiscordUser.GetUserIdMMUserIdDic();

            // ギルドバトル進捗管理で表示されているプレイヤーのみ対象とすることで、
            // 当ボットが多重起動されていても処理的にはここで弾くようにする。
            // 返事を多重化させないようにできる。(ギルドバトル進捗管理で同じプレイヤーを含む画面を多重起動されないこと)
            if (!userIdMMUserIdDic.ContainsKey(arg.Author.Id) ||
                (null != GuildBPList && !GuildBPList.Where(x => x.MMId == userIdMMUserIdDic[arg.Author.Id]).Any()))
            {
                return;
            }

            if (mmgpChannelId == arg.Channel.Id ||
                (userDedicatedChannelIdDic.ContainsKey(arg.Author.Id) && userDedicatedChannelIdDic[arg.Author.Id] == arg.Channel.Id))
            {
                string commandName = arg.Content;

                // 引数が存在するか確認し分割
                string[] commandLines = commandName.Split(' ');

                if (1 < commandLines.Length)
                {
                    commandName = commandLines[0];
                }

                // ギルバト進捗用
                switch (commandName)
                {
                    case "!mmgp":
                    case "!mmgphs":
                    case "!mmgpms":
                    case "!mmgpm":
                    case "!mmgps":
                    case "!mmgpd":
                    case "!mmgpmf":
                    case "!mmgpmc":
                    case "!mmgpmk":
                        result = MMGPALLReply(commandName);
                        break;
                    case "!mmggo":
                        result = MMGGOReply();
                        break;
                    case "!mmggs":
                        result = MMGGSReply();
                        break;
                    case "!mmgd1":
                        result = MMGD1Reply();
                        break;
                    case "!mmgd2":
                        result = MMGD2Reply();
                        break;
                    case "!mmgd3":
                        result = MMGD3Reply();
                        break;
                    case "!mmglu":
                        result = MMGLUReply();
                        break;
                    case "!mmgv":
                        result = MMGVReply();
                        break;
                    case "!cmdl":
                        result = MMGCMDLReply();
                        break;
                    case "!mmgpmfl":
                        result = MMGPMFLReply(arg.Author.Id);
                        break;
                    case "!mmgpmfa":
                        int.TryParse(commandLines[1], out int formaitonId);
                        result = MMGPMFAReply(arg.Author.Id, formaitonId);
                        break;
                }
            }
            else if (weekPassChannelId == arg.Channel.Id && "!mmgwp".Equals(arg.Content))
            {
                // 週刊パスワード用
                if (adminClientId == arg.Author.Id || weekPassClientIds.Contains(arg.Author.Id.ToString()))
                {
                    result = MMGWPReply();
                }
            }

            string[] results = Common.Common.GetDivide(result, MAX_MESSAGE_LENGTH);

            foreach (string str in results)
            {
                await arg.Channel.SendMessageAsync(str);
            }
        }

        private string MMGPMFAReply(ulong clientId, int formaitonId)
        {
            string result = string.Empty;

            Dictionary<ulong, long> userIdMMUserIdDic = BaseDiscordUser.GetUserIdMMUserIdDic();

            if (!userIdMMUserIdDic.ContainsKey(clientId))
            {
                return result;
            }

            if (!GuildBPList.Where(x => x.MMId == userIdMMUserIdDic[clientId]).Any())
            {
                return result;
            }

            Dictionary<ulong, bool> userRCPermissionDic = BaseDiscordUser.GetUserRCPermissionDic();

            if (!userRCPermissionDic.ContainsKey(clientId))
            {
                return result;
            }

            if (!userRCPermissionDic[clientId])
            {
                return "遠隔操作権限が設定されていません。MMG起動者に確認してください。";
            }

            GuildBattleProgressDto guildBattleProgressDto = GuildBPList.Where(x => x.MMId == userIdMMUserIdDic[clientId]).FirstOrDefault();

            if (guildBattleProgressDto.FormationIdName.Where(x => x.Value == formaitonId).Any())
            {
                Dictionary<ulong, string> userIdNameDic = BaseDiscordUser.GetUserIdNameDic();

                KeyValuePair<string, int> idNameKV = guildBattleProgressDto.FormationIdName.Where(x => x.Value == formaitonId).FirstOrDefault();

                string before = idNameKV.Key;

                GuildVSAdmin.FormationButtonExecute(formaitonId);

                idNameKV = guildBattleProgressDto.FormationIdName.Where(x => x.Value == formaitonId).FirstOrDefault();

                result = before + " -> " + idNameKV.Key;

                GuildVSAdmin.Dispatcher.Invoke(() =>
                {
                    ListBox.Items.Add(userIdNameDic[clientId] + "が!mmgpmfaを使用して、" + result);
                }); 
            }
            else
            {
                result = "編成ID:" + formaitonId + "は存在しませんでした。";
            }

            return result;
        }

        private string MMGPMFLReply(ulong clientId)
        {
            string result = string.Empty;

            // DiscordのユーザーIDとメメモリIDの相関マップに登録があれば、編成リスト情報を返却
            Dictionary<ulong, long> userIdMMUserIdDic = BaseDiscordUser.GetUserIdMMUserIdDic();

            if (!userIdMMUserIdDic.ContainsKey(clientId))
            {
                return result;
            }

            if (!GuildBPList.Where(x => x.MMId == userIdMMUserIdDic[clientId]).Any())
            {
                return result;
            }

            GuildBattleProgressDto guildBattleProgressDto = GuildBPList.Where(x => x.MMId == userIdMMUserIdDic[clientId]).FirstOrDefault();

            GuildBattleProgressGenerator guildBattleProgressGenerator = new GuildBattleProgressGenerator();

            result = guildBattleProgressGenerator.CreateFormationToString(guildBattleProgressDto);

            return result;
        }

        private string MMGPALLReply(string content)
        {
            GuildBattleProgressGenerator guildBattleProgressGenerator = new GuildBattleProgressGenerator();
            return guildBattleProgressGenerator.CreateGuildBattleProgressToString(GuildBPList, content);
        }

        private string MMGGOReply()
        {
            return "オンライン " + LabMemberOnline;
        }

        private  string MMGGSReply()
        {
            return "スタミナ " + LabMemberOnlineST;
        }

        private string MMGD1Reply()
        {
            return "キャラ1積みの場合、防衛可能時間は、" + Labstack1 + "です。";
        }

        private string MMGD2Reply()
        {
            return "キャラ2積みの場合、防衛可能時間は、" + Labstack2 + "です。";
        }

        private string MMGD3Reply()
        {
            return "キャラ3積みの場合、防衛可能時間は、" + Labstack3 + "です。";
        }

        private string MMGLUReply()
        {
            string result;

            if (null != AuthDto && !string.IsNullOrEmpty(AuthDto.LoginId))
            {
                if (ulong.TryParse(AuthDto.LoginId, out ulong loginId))
                {
                    Dictionary<ulong, string> userIdNameDic = BaseDiscordUser.GetUserIdNameDic();

                    if (userIdNameDic.ContainsKey(loginId))
                    {
                        result = "MMGを起動しているユーザーは" + userIdNameDic[loginId] + "です。";
                    }
                    else
                    {
                        result = "MMGを起動しているユーザーはMMGで未設定のユーザーです。";
                    }
                }
                else
                {
                    result = "MMGを起動しているユーザーは" + AuthDto.LoginId + "です。";
                }
            }
            else
            {
                result = "MMGを起動しているユーザーは不明です。";
            }

            return result;
        }

        private string MMGVReply()
        {
            Assembly asm = Assembly.GetExecutingAssembly();
            Version ver = asm.GetName().Version;
            return "MMGバージョンは" + ver + "です。";
        }

        private string MMGWPReply()
        {
            Auth auth = new Auth();
            return auth.GetLoginPassowrd();
        }

        private string MMGCMDLReply()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("!cmdl      : コマンド一覧表示");
            sb.Append(Environment.NewLine);
            sb.Append("!mmgp      : 最新のギルドバトル進捗情報一覧を無条件表示");
            sb.Append(Environment.NewLine);
            sb.Append("!mmgphs    : !mmgpの情報をスタミナが残っているプレイヤーのみ表示");
            sb.Append(Environment.NewLine);
            sb.Append("!mmgpms    : !mmgpの情報をメインとサブパが残っているプレイヤーのみ表示");
            sb.Append(Environment.NewLine);
            sb.Append("!mmgpm     : !mmgpの情報をメインが残っているプレイヤー情報のみ表示");
            sb.Append(Environment.NewLine);
            sb.Append("!mmgps     : !mmgpの情報をサブパが残っているプレイヤーのみ表示");
            sb.Append(Environment.NewLine);
            sb.Append("!mmgpd     : !mmgpの情報をデバフが残っているプレイヤーのみ表示");
            sb.Append(Environment.NewLine);
            sb.Append("!mmgpmf    : !mmgpの情報をメイン:フロパが残っているプレイヤーのみ表示");
            sb.Append(Environment.NewLine);
            sb.Append("!mmgpmc    : !mmgpの情報をメイン:コルパが残っているプレイヤーのみ表示");
            sb.Append(Environment.NewLine);
            sb.Append("!mmgpmk    : !mmgpの情報をメイン:回避パが残っているプレイヤーのみ表示");
            sb.Append(Environment.NewLine);
            sb.Append("!mmgpmfl   : !mmgpの情報をコマンド実行者編成のみ表示");
            sb.Append(Environment.NewLine);
            sb.Append("!mmgpmfa [formationId]   : !mmgpの情報をコマンド実行者編成IDを指定して1回分の編成ボタン押下を遠隔操作(MMG側権限設定必須)");
            sb.Append(Environment.NewLine);
            sb.Append("!mmggo     : ギルドのオンライン状況を表示");
            sb.Append(Environment.NewLine);
            sb.Append("!mmggs     : ギルドのスタミナ状況を表示");
            sb.Append(Environment.NewLine);
            sb.Append("!mmgd1     : 現状の総スタミナにて1キャラ積みで防衛した場合の耐久時間を表示");
            sb.Append(Environment.NewLine);
            sb.Append("!mmgd2     : 現状の総スタミナにて2キャラ積みで防衛した場合の耐久時間を表示");
            sb.Append(Environment.NewLine);
            sb.Append("!mmgd3     : 現状の総スタミナにて3キャラ積みで防衛した場合の耐久時間を表示");
            sb.Append(Environment.NewLine);
            sb.Append("!mmglu     : MMG起動しているユーザー名を表示(ログイン時ID)");
            sb.Append(Environment.NewLine);
            sb.Append("!mmgv      : MMGバージョンを表示");
            sb.Append(Environment.NewLine);
            sb.Append("!mmgwp     : MMGログインパスワードを表示(特定ユーザーのみ)");

            return sb.ToString();
        }

        internal void Dispose()
        {
            Client.Dispose();
        }

        internal async Task MainAsync()
        {
            Client = new DiscordSocketClient(
                                new DiscordSocketConfig
                                {
                                    LogLevel = LogSeverity.Info,
                                    GatewayIntents = GatewayIntents.MessageContent |
                                    GatewayIntents.Guilds |
                                    GatewayIntents.GuildMembers |
                                    GatewayIntents.GuildMessageReactions |
                                    GatewayIntents.GuildMessages |
                                    GatewayIntents.GuildVoiceStates
                                });

            Client.Log += Log;
            Client.MessageReceived += Client_MessageReceived;

            await Client.LoginAsync(TokenType.Bot, botToken);
            await Client.StartAsync();
        }

        private Task Log(LogMessage message)
        {
            ListBox.Items.Add(message.ToString());
            return Task.CompletedTask;
        }
    }
}
