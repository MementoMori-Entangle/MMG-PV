using log4net;
using MMG.Dao;
using MMG.Dto;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

namespace MMG
{
    /// <summary>
    /// PlayerManagement.xaml の相互作用ロジック
    /// </summary>
    public partial class PlayerManagement : Window
    {
        private readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().Name);

        private List<PlayerDto> playerList;

        public PlayerManagement()
        {
            InitializeComponent();
            LoadSearchItem();
        }

        private void LoadSearchItem()
        {
            try
            {
                TextId.Text = string.Empty;
                TextName.Text = string.Empty;
                ListWorld.SelectedItems.Clear();
                ListGuild.SelectedItems.Clear();

                PlayerDao playerDao = new PlayerDao();

                ListWorld.ItemsSource = playerDao.GetWorldNoItem();
                ListGuild.ItemsSource = playerDao.GetGuildNameItem();
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }

        private void BtnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                LoadPlayerData();
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }

        private void LoadPlayerData()
        {
            try
            {
                PlayerDao playerDao = new PlayerDao(true, false);
                playerList = new List<PlayerDto>();

                int id = 0;
                string name = "  ";
                int world = 0;
                string guildName = "  ";

                if (null != TextId && string.Empty != TextId.Text)
                {
                    id = int.Parse(TextId.Text);
                }

                if (!string.IsNullOrEmpty(TextName.Text))
                {
                    name = TextName.Text;
                }

                if (null != ListWorld && null != ListWorld.SelectedItem &&
                    !string.IsNullOrEmpty(ListWorld.SelectedItem.ToString()))
                {
                    world = int.Parse(ListWorld.SelectedItem.ToString());
                }

                if (null != ListGuild && null != ListGuild.SelectedItem &&
                    !string.IsNullOrEmpty(ListGuild.SelectedItem.ToString()))
                {
                    guildName = ListGuild.SelectedItem.ToString();
                }

                PlayerDto[] players = playerDao.GetAllInfoPlayer(id, name, world, guildName);

                foreach (PlayerDto playerDto in players)
                {
                    playerList.Add(playerDto);
                }

                DgPlayerInfo.ItemsSource = playerList;
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
                TextId.Text = string.Empty;
                TextName.Text = string.Empty;
                ListWorld.SelectedItems.Clear();
                ListGuild.SelectedItems.Clear();
                DgPlayerInfo.ItemsSource = null;
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }

        private void BtnNewReg_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                PlayerDto playerDto = new PlayerDto
                {
                    IsOpenWindow = true
                };

                MemberInfo memberInfo = new MemberInfo(playerDto, playerList, true, true);
                memberInfo.LoadMemberInfo();
                memberInfo.ShowDialog();

                LoadPlayerData();
                DgPlayerInfo.Items.Refresh();
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }

        private void DgPlayerInfo_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
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

                    MemberInfo memberInfo = new MemberInfo(playerDto, playerList, true);
                    memberInfo.LoadMemberInfo();
                    memberInfo.ShowDialog();

                    LoadPlayerData();
                    DgPlayerInfo.Items.Refresh();
                }
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }
    }
}
