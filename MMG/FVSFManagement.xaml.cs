using log4net;
using MMG.Dao;
using MMG.Dto;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using DataGrid = System.Windows.Controls.DataGrid;
using MessageBox = System.Windows.MessageBox;

namespace MMG
{
    /// <summary>
    /// FVSFManagement.xaml の相互作用ロジック
    /// </summary>
    public partial class FVSFManagement : Window
    {
        private readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().Name);

        private List<FormationVSFormationDto> formationVSFList;

        public FVSFManagement()
        {
            InitializeComponent();

            formationVSFList = new List<FormationVSFormationDto>();

            LoadDgFVSF();
        }

        public void UpdateData(int leftFId, int rightFId, int deBuffOK)
        {
            try
            {
                var item = DgFVSF.SelectedItem;

                if (typeof(FormationVSFormationDto) == item.GetType())
                {
                    FormationVSFormationDto dto = (FormationVSFormationDto)item;

                    dto.WinFormationId = leftFId;
                    dto.LoseFormationId = rightFId;
                    dto.DebuffKONum = deBuffOK;
                }

                DgFVSF.Items.Refresh();
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        private void LoadDgFVSF()
        {
            try
            {
                FormationVSFormationDao formationVSFormationDao = new FormationVSFormationDao();

                formationVSFList = formationVSFormationDao.GetFormationVSFormationDtos().ToList();

                DgFVSF.ItemsSource = formationVSFList;
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                FormationVSFormationDao formationVSFormationDao = new FormationVSFormationDao();

                formationVSFormationDao.SaveFormationVSFormationDto(formationVSFList, CheckBackup.IsChecked.Value);

                // 編成VS編成からプレイヤーVSプレイヤー情報を生成
                PlayerVSPlayerDto[] playerVSPlayerDtos = Common.Common.GetPlayerVSPlayerDto(formationVSFList.ToArray());

                PlayerVSPlayerDao playerVSPlayerDao = new PlayerVSPlayerDao();

                playerVSPlayerDao.SavePlayerVSPlayerDto(playerVSPlayerDtos.ToList(), CheckBackup.IsChecked.Value);

                MessageBoxResult messageBoxResult = MessageBox.Show("保存が完了しました。画面を閉じますか?",
                                                                    "保存", (MessageBoxButton)MessageBoxButtons.YesNo);

                if (messageBoxResult == MessageBoxResult.Yes)
                {
                    Close();
                }
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }

        private void BtnReload_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                LoadDgFVSF();

                DgFVSF.Items.Refresh();
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }

        private void DgFVSF_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
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

                if (typeof(FormationVSFormationDto) == item.GetType())
                {
                    FormationVSFormationDto dto = (FormationVSFormationDto)item;

                    PlayerVSPlayer playerVSPlayer = new PlayerVSPlayer(this);

                    playerVSPlayer.LoadPlayerData(dto);
                    playerVSPlayer.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }
    }
}
