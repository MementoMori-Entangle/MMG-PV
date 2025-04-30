using log4net;
using MMG.Dao;
using MMG.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

namespace MMG
{
    /// <summary>
    /// GuildInfoManagement.xaml の相互作用ロジック
    /// </summary>
    public partial class GuildInfoManagement : Window
    {
        private readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().Name);

        private Button button;
        private Dictionary<int, List<PlayerDto>> playerDic;
        private List<GuildDto> guildList;

        public GuildInfoManagement()
        {
            InitializeComponent();
            LoadSearchItem();
            LoadGuildInfo();
        }

        public GuildInfoManagement(Button button)
        {
            InitializeComponent();
            button.IsEnabled = false;
            this.button = button;
            playerDic = new Dictionary<int, List<PlayerDto>>();
            guildList = new List<GuildDto>();
            LoadSearchItem();
            LoadGuildInfo();
        }

        private void LoadSearchItem()
        {
            try
            {
                TextName.Text = string.Empty;
                CombBWorld.Items.Clear();

                PlayerDao playerDao = new PlayerDao();

                CombBWorld.ItemsSource = playerDao.GetWorldNoItem();
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }

        public void LoadGuildInfo()
        {
            LoadGuildIS();
            DgGuildInfo.ItemsSource = guildList;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                button.IsEnabled = true;
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }

        private void DgGuildInfo_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            try
            {
                var dataGrid = sender as DataGrid;
                var cellInfos = dataGrid.SelectedCells;

                if (cellInfos.Count == 0)
                {
                    return;
                }

                var cellInfo = cellInfos[0];
                var item = cellInfo.Item;

                if (typeof(GuildDto) == item.GetType())
                {
                    GuildDto guild = (GuildDto)item;

                    List<PlayerDto> playerList = null;

                    if (playerDic.ContainsKey(guild.Id))
                    {
                        playerList = playerDic[guild.Id];
                    }
                    else
                    {
                        playerList = GetGuildPlayerIS(guild.Id);
                        playerDic.Add(guild.Id, playerList);
                    }

                    DgGuildMemberInfo.ItemsSource = playerList;
                }
                else
                {
                    DgGuildMemberInfo.Items.Clear();
                }
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }

        private void DgGuildMemberInfo_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            try
            {
                var dataGrid = sender as DataGrid;
                var cellInfos = dataGrid.SelectedCells;

                if (cellInfos.Count == 0)
                {
                    return;
                }

                var cellInfo = cellInfos[0];
                var item = cellInfo.Item;

                if (typeof(PlayerDto) == item.GetType())
                {
                    PlayerDto playerDto = (PlayerDto)item;

                    if (playerDto.IsOpenWindow)
                    {
                        return;
                    }

                    playerDto.IsOpenWindow = true;

                    MemberInfo memberInfo = new MemberInfo(playerDto);
                    memberInfo.LoadMemberInfo();
                    memberInfo.Show();
                    DgGuildMemberInfo.UnselectAll();
                }
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }

        private List<PlayerDto> GetGuildPlayerIS(int id)
        {
            List<PlayerDto> list = new List<PlayerDto>();

            GuildDto[] guilds = GetGuilds();

            int num = guilds.Length;

            for (int i = 0; i < num; i++)
            {
                if (id == guilds[i].Id)
                {
                    int memberNum = guilds[i].Members.Length;

                    for (int j = 0; j < memberNum; j++)
                    {
                        list.Add(guilds[i].Members[j]);
                    }
                }
            }

            return list;
        }

        private void LoadGuildIS()
        {
            guildList.Clear();

            GuildDto[] guilds = GetGuilds();

            int num = guilds.Length;

            for (int i = 0; i < num; i++)
            {
                guildList.Add(guilds[i]);
            }
        }

        private GuildDto[] GetGuilds()
        {
            GuildDto[] guildDtos = new GuildDto[0];

            try
            {
                GuildMemberDao memberDao = new GuildMemberDao();

                guildDtos = memberDao.GetGuildMembers(string.Empty, 0);
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }

            return guildDtos;
        }

        private void SearchGuild()
        {
            try
            {
                string name = "  ";
                int world = 0;
                bool isSearch = false;

                if (!string.IsNullOrEmpty(TextName.Text))
                {
                    name = TextName.Text;
                    isSearch = true;
                }

                if (null != CombBWorld.SelectedItem &&
                    !string.IsNullOrEmpty(CombBWorld.SelectedItem.ToString()))
                {
                    world = int.Parse(CombBWorld.SelectedItem.ToString());
                    isSearch = true;
                }

                GuildMemberDao memberDao = new GuildMemberDao();

                if (isSearch)
                {
                    guildList = memberDao.GetGuildMembers(name, world).ToList();
                }
                else
                {
                    guildList = memberDao.GetGuildMembers(string.Empty, 0).ToList();
                }

                DgGuildInfo.ItemsSource = guildList;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void BtnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SearchGuild();
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }

        private void BtnInputClear_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                TextName.Text = string.Empty;
                CombBWorld.SelectedItem = null;
                DgGuildInfo.ItemsSource = null;
                DgGuildMemberInfo.ItemsSource = null;
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }

        private void BtnNew_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                GuildInfo guildInfo = new GuildInfo(null, guildList, true, true);
                guildInfo.ShowDialog();

                SearchGuild();
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }

        private void DgGuildInfo_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                var dataGrid = sender as DataGrid;
                var cellInfos = dataGrid.SelectedCells;

                if (cellInfos.Count == 0)
                {
                    return;
                }

                var cellInfo = cellInfos[0];
                var item = cellInfo.Item;

                if (typeof(GuildDto) == item.GetType())
                {
                    GuildDto guild = (GuildDto)item;

                    GuildInfo guildInfo = new GuildInfo(guild, guildList, true);
                    guildInfo.ShowDialog();

                    SearchGuild();
                }
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }
    }
}
