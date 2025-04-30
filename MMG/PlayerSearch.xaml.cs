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
    /// PlayerSearch.xaml の相互作用ロジック
    /// </summary>
    public partial class PlayerSearch : Window
    {
        private readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().Name);

        public PlayerVSPlayer Pvsp { get; set; }
        public CharacterSpeed Cs { get; set; }
        public int FormationsPos { get; set; }

        private int type;

        public PlayerSearch(int type)
        {
            InitializeComponent();
            this.type = type;
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
                PlayerDao playerDao = new PlayerDao(true, false);
                List<PlayerDto> list = new List<PlayerDto>();

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

                if (CheckBGVSDelete.IsChecked.Value)
                {
                    players = Common.Common.DeleteGVSFormationData(players);
                }

                foreach (PlayerDto playerDto in players)
                {
                    list.Add(playerDto);
                }

                DgGuildMemberInfo.ItemsSource = list;
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
                DgGuildMemberInfo.ItemsSource = null;
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
                    if (0 == FormationsPos)
                    {
                        if (type == 1)
                        {
                            Pvsp.LeftPlayerDto = (PlayerDto)item;
                        }
                        else if (type == 2)
                        {
                            Cs.LeftPlayerDto = (PlayerDto)item;
                        }
                    }
                    else
                    {
                        if (type == 1)
                        {
                            Pvsp.RightPlayerDto = (PlayerDto)item;
                        }
                        else if (type == 2)
                        {
                            Cs.RightPlayerDto = (PlayerDto)item;
                        }
                    }

                    Close();
                }
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }
    }
}
