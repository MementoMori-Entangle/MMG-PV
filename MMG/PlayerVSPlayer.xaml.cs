using log4net;
using MMG.Common;
using MMG.Dao;
using MMG.Dto;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using static System.Net.Mime.MediaTypeNames;

namespace MMG
{
    /// <summary>
    /// PlayerVSPlayer.xaml の相互作用ロジック
    /// </summary>
    public partial class PlayerVSPlayer : Window
    {
        private readonly string imgCharaPath = Common.Common.FormationCharaImagePath;
        private readonly string imgTokeNekoPath = Common.Common.FormationTokeNekoImagePath;
        private readonly string imgExt = Common.Common.FormationImageExtension;

        private readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().Name);

        private int formationsLeftCnt;
        private int formationsRightCnt;
        private int leftNowPage;
        private int rightNowPage;
        private int leftPos;
        private int rightPos;

        public PlayerDto LeftPlayerDto { get; set; }
        public PlayerDto RightPlayerDto { get; set; }

        private FormationDto[] leftFormations;
        private FormationDto[] rightFormations;

        private FVSFManagement fVSFManagement;

        public PlayerVSPlayer()
        {
            InitializeComponent();
        }

        public PlayerVSPlayer(FVSFManagement fVSFManagement)
        {
            InitializeComponent();

            this.fVSFManagement = fVSFManagement;
            BtnFVSFSet.Visibility = Visibility.Hidden;
        }

        public void LoadPlayerData(FormationVSFormationDto dto)
        {
            try
            {
                FormationDao formationDao = new FormationDao(false);
                
                FormationDto leftFormationDto = null;
                FormationDto rightFormationDto = null;

                if (dto.WinFormationId > 0)
                {
                    leftFormationDto = formationDao.GetFormationDto(dto.WinFormationId);
                }

                if (dto.LoseFormationId > 0)
                {
                    rightFormationDto = formationDao.GetFormationDto(dto.LoseFormationId);
                }

                PlayerDao playerDao = new PlayerDao();

                if (null != leftFormationDto)
                {
                    LeftPlayerDto = playerDao.GetPlayerDto(leftFormationDto.PlayerId);

                    LabMemberId_L.Content = LeftPlayerDto.Id;
                    TextMemberName_L.Text = LeftPlayerDto.Name;

                    for (int i = 0; i < 6; i++)
                    {
                        ClearLeftFormation(i);
                    }

                    leftFormations = LeftPlayerDto.Formations;
                    leftNowPage = leftFormationDto.SortNo;
                    formationsLeftCnt = leftFormations.Length;
                    LoadLeftFormation();
                }

                if (null != rightFormationDto)
                {
                    RightPlayerDto = playerDao.GetPlayerDto(rightFormationDto.PlayerId);

                    LabMemberId_R.Content = RightPlayerDto.Id;
                    TextMemberName_R.Text = RightPlayerDto.Name;

                    for (int i = 0; i < 6; i++)
                    {
                        ClearRightFormation(i);
                    }

                    rightFormations = RightPlayerDto.Formations;
                    rightNowPage = rightFormationDto.SortNo;
                    formationsRightCnt = rightFormations.Length;
                    LoadRightFormation();
                }

                TextDefenseKONum.Text = dto.DebuffKONum.ToString();

                BtnFVSFSet.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }

        private void BtnPlayerLeft_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                PlayerSearch playerSearch = new PlayerSearch(1)
                {
                    Pvsp = this,
                    FormationsPos = 0
                };

                playerSearch.ShowDialog();

                if (null == LeftPlayerDto)
                {
                    return;
                }

                LabMemberId_L.Content = LeftPlayerDto.Id;
                TextMemberName_L.Text = LeftPlayerDto.Name;

                for (int i = 0; i < 6; i++)
                {
                    ClearLeftFormation(i);
                }

                if (LeftPlayerDto.Formations.Length == 0)
                {
                    MessageBox.Show("選択プレイヤーは編成情報が存在しませんでした。");
                    return;
                }

                leftFormations = LeftPlayerDto.Formations;
                leftNowPage = 1;
                formationsLeftCnt = leftFormations.Length;
                LoadLeftFormation();
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }

        private void BtnPlayerRight_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                PlayerSearch playerSearch = new PlayerSearch(1)
                {
                    Pvsp = this,
                    FormationsPos = 1
                };

                playerSearch.ShowDialog();

                if (null == RightPlayerDto)
                {
                    return;
                }

                LabMemberId_R.Content = RightPlayerDto.Id;
                TextMemberName_R.Text = RightPlayerDto.Name;

                for (int i = 0; i < 6; i++)
                {
                    ClearRightFormation(i);
                }

                if (RightPlayerDto.Formations.Length == 0)
                {
                    MessageBox.Show("選択プレイヤーは編成情報が存在しませんでした。");
                    return;
                }

                rightFormations = RightPlayerDto.Formations;
                rightNowPage = 1;
                formationsRightCnt = rightFormations.Length;
                LoadRightFormation();
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }

        private void BtnVS_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (null == leftFormations || (null != leftFormations && 1 > leftFormations.Length) ||
                    null == rightFormations || (null != rightFormations && 1 > rightFormations.Length))
                {
                    MessageBox.Show("対戦するには編成してください。");
                    return;
                }

                BattleCalculations battleCalculations = new BattleCalculations();

                int.TryParse(TextDefenseKONum.Text, out int defenseKO);
                int.TryParse(TextAutoVSCnt.Text, out int autoVSCnt);

                if (autoVSCnt == 0)
                {
                    autoVSCnt = 1;
                }

                ListVSResult.Items.Clear();

                int leftWinCnt = 0;
                int rightWinCnt = 0;

                Random random = new Random();

                for (int i = 1; i <= autoVSCnt; i++)
                {
                    Dictionary<string, string> battleDic = battleCalculations.GetPlayerVSPlayer(random,
                                                               leftFormations[leftPos],
                                                               rightFormations[rightPos],
                                                               0, 0, defenseKO);

                    if ("left".Equals(battleDic["result"]))
                    {
                        ImgVSLeft.Source = new BitmapImage(new Uri(imgTokeNekoPath + "13" + imgExt));
                        ImgVSRight.Source = new BitmapImage(new Uri(imgTokeNekoPath + "17" + imgExt));
                        ListVSResult.Items.Add(i + "回目 : 左勝利");

                        leftWinCnt++;
                    }
                    else
                    {
                        ImgVSLeft.Source = new BitmapImage(new Uri(imgTokeNekoPath + "17" + imgExt));
                        ImgVSRight.Source = new BitmapImage(new Uri(imgTokeNekoPath + "13" + imgExt));
                        ListVSResult.Items.Add(i + "回目 : 右勝利");

                        rightWinCnt++;
                    }
                }

                if (autoVSCnt > 1)
                {
                    ListVSResult.Items.Add("左勝利数 : " + leftWinCnt + "/" + autoVSCnt);
                    ListVSResult.Items.Add("右勝利数 : " + rightWinCnt + "/" + autoVSCnt);
                }
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }

        private void BtnVS_MouseEnter(object sender, MouseEventArgs e)
        {
            try
            {
                ImgVS.Source = new BitmapImage(new Uri(imgTokeNekoPath + "19" + imgExt));
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }

        private void BtnVS_MouseLeave(object sender, MouseEventArgs e)
        {
            try
            {
                ImgVS.Source = new BitmapImage(new Uri(imgTokeNekoPath + "11" + imgExt));
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }

        private void LoadLeftFormation()
        {
            try
            {
                if (null == leftFormations || (null != leftFormations && 1 > leftFormations.Length))
                {
                    return;
                }

                leftPos = leftNowPage - 1;

                FormationDto formation = leftFormations[leftPos];

                if (null == formation)
                {
                    return;
                }

                LabFormationId_L.Content = formation.Id;
                TextFormationName_L.Text = formation.Name;
                TextFormationSort_L.Text = formation.SortNo.ToString();

                if (formationsLeftCnt > leftNowPage)
                {
                    BtnNext_L.IsEnabled = true;
                }
                else
                {
                    BtnNext_L.IsEnabled = false;
                }

                if (leftNowPage > 1)
                {
                    BtnBack_L.IsEnabled = true;
                }
                else
                {
                    BtnBack_L.IsEnabled = false;
                }

                CharacterDto[] characters = formation.Characters;

                int charCnt = characters.Length;

                if (charCnt > 0)
                {
                    Img1_L.Source = new BitmapImage(new Uri(imgCharaPath + characters[0].Name + imgExt));
                    Img1_L.ToolTip = characters[0].Name;
                    LabRarity1_L.Content = Common.Common.GetRarityType(characters[0].Rarity);
                    LabLevel1_L.Content = Common.Common.GetLevelText(characters[0].Level);
                    TextSpeedRune1_1_L.Text = characters[0].SpeedRune1.ToString();
                    TextSpeedRune1_2_L.Text = characters[0].SpeedRune2.ToString();
                    TextSpeedRune1_3_L.Text = characters[0].SpeedRune3.ToString();
                    TextPiercingRune1_1_L.Text = characters[0].PiercingRune1.ToString();
                    TextPiercingRune1_2_L.Text = characters[0].PiercingRune2.ToString();
                    TextPiercingRune1_3_L.Text = characters[0].PiercingRune3.ToString();
                }
                else
                {
                    ClearLeftFormation(1);
                }

                if (charCnt > 1)
                {
                    Img2_L.Source = new BitmapImage(new Uri(imgCharaPath + characters[1].Name + imgExt));
                    Img2_L.ToolTip = characters[1].Name;
                    LabRarity2_L.Content = Common.Common.GetRarityType(characters[1].Rarity);
                    LabLevel2_L.Content = Common.Common.GetLevelText(characters[1].Level);
                    TextSpeedRune2_1_L.Text = characters[1].SpeedRune1.ToString();
                    TextSpeedRune2_2_L.Text = characters[1].SpeedRune2.ToString();
                    TextSpeedRune2_3_L.Text = characters[1].SpeedRune3.ToString();
                    TextPiercingRune2_1_L.Text = characters[1].PiercingRune1.ToString();
                    TextPiercingRune2_2_L.Text = characters[1].PiercingRune2.ToString();
                    TextPiercingRune2_3_L.Text = characters[1].PiercingRune3.ToString();
                }
                else
                {
                    ClearLeftFormation(2);
                }

                if (charCnt > 2)
                {
                    Img3_L.Source = new BitmapImage(new Uri(imgCharaPath + characters[2].Name + imgExt));
                    Img3_L.ToolTip = characters[2].Name;
                    LabRarity3_L.Content = Common.Common.GetRarityType(characters[2].Rarity);
                    LabLevel3_L.Content = Common.Common.GetLevelText(characters[2].Level);
                    TextSpeedRune3_1_L.Text = characters[2].SpeedRune1.ToString();
                    TextSpeedRune3_2_L.Text = characters[2].SpeedRune2.ToString();
                    TextSpeedRune3_3_L.Text = characters[2].SpeedRune3.ToString();
                    TextPiercingRune3_1_L.Text = characters[2].PiercingRune1.ToString();
                    TextPiercingRune3_2_L.Text = characters[2].PiercingRune2.ToString();
                    TextPiercingRune3_3_L.Text = characters[2].PiercingRune3.ToString();
                }
                else
                {
                    ClearLeftFormation(3);
                }

                if (charCnt > 3)
                {
                    Img4_L.Source = new BitmapImage(new Uri(imgCharaPath + characters[3].Name + imgExt));
                    Img4_L.ToolTip = characters[3].Name;
                    LabRarity4_L.Content = Common.Common.GetRarityType(characters[3].Rarity);
                    LabLevel4_L.Content = Common.Common.GetLevelText(characters[3].Level);
                    TextSpeedRune4_1_L.Text = characters[3].SpeedRune1.ToString();
                    TextSpeedRune4_2_L.Text = characters[3].SpeedRune2.ToString();
                    TextSpeedRune4_3_L.Text = characters[3].SpeedRune3.ToString();
                    TextPiercingRune4_1_L.Text = characters[3].PiercingRune1.ToString();
                    TextPiercingRune4_2_L.Text = characters[3].PiercingRune2.ToString();
                    TextPiercingRune4_3_L.Text = characters[3].PiercingRune3.ToString();
                }
                else
                {
                    ClearLeftFormation(4);
                }

                if (charCnt > 4)
                {
                    Img5_L.Source = new BitmapImage(new Uri(imgCharaPath + characters[4].Name + imgExt));
                    Img5_L.ToolTip = characters[4].Name;
                    LabRarity5_L.Content = Common.Common.GetRarityType(characters[4].Rarity);
                    LabLevel5_L.Content = Common.Common.GetLevelText(characters[4].Level);
                    TextSpeedRune5_1_L.Text = characters[4].SpeedRune1.ToString();
                    TextSpeedRune5_2_L.Text = characters[4].SpeedRune2.ToString();
                    TextSpeedRune5_3_L.Text = characters[4].SpeedRune3.ToString();
                    TextPiercingRune5_1_L.Text = characters[4].PiercingRune1.ToString();
                    TextPiercingRune5_2_L.Text = characters[4].PiercingRune2.ToString();
                    TextPiercingRune5_3_L.Text = characters[4].PiercingRune3.ToString();
                }
                else
                {
                    ClearLeftFormation(5);
                }
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }

        private void ClearLeftFormation(int pos)
        {
            try
            {
                switch (pos)
                {
                    case 0:
                        LabFormationId_L.Content = string.Empty;
                        TextFormationName_L.Text = string.Empty;
                        TextFormationSort_L.Text = string.Empty;
                        BtnNext_L.IsEnabled = false;
                        BtnBack_L.IsEnabled = false;
                        leftNowPage = 1;
                        break;
                    case 1:
                        Img1_L.Source = new BitmapImage(new Uri(imgCharaPath + Common.Common.FormationImageNothing + imgExt));
                        Img1_L.ToolTip = string.Empty;
                        LabRarity1_L.Content = string.Empty;
                        LabLevel1_L.Content = string.Empty;
                        TextSpeedRune1_1_L.Text = string.Empty;
                        TextSpeedRune1_2_L.Text = string.Empty;
                        TextSpeedRune1_3_L.Text = string.Empty;
                        TextPiercingRune1_1_L.Text = string.Empty;
                        TextPiercingRune1_2_L.Text = string.Empty;
                        TextPiercingRune1_3_L.Text = string.Empty;
                        break;
                    case 2:
                        Img2_L.Source = new BitmapImage(new Uri(imgCharaPath + Common.Common.FormationImageNothing + imgExt));
                        Img2_L.ToolTip = string.Empty;
                        LabRarity2_L.Content = string.Empty;
                        LabLevel2_L.Content = string.Empty;
                        TextSpeedRune2_1_L.Text = string.Empty;
                        TextSpeedRune2_2_L.Text = string.Empty;
                        TextSpeedRune2_3_L.Text = string.Empty;
                        TextPiercingRune2_1_L.Text = string.Empty;
                        TextPiercingRune2_2_L.Text = string.Empty;
                        TextPiercingRune2_3_L.Text = string.Empty;
                        break;
                    case 3:
                        Img3_L.Source = new BitmapImage(new Uri(imgCharaPath + Common.Common.FormationImageNothing + imgExt));
                        Img3_L.ToolTip = string.Empty;
                        LabRarity3_L.Content = string.Empty;
                        LabLevel3_L.Content = string.Empty;
                        TextSpeedRune3_1_L.Text = string.Empty;
                        TextSpeedRune3_2_L.Text = string.Empty;
                        TextSpeedRune3_3_L.Text = string.Empty;
                        TextPiercingRune3_1_L.Text = string.Empty;
                        TextPiercingRune3_2_L.Text = string.Empty;
                        TextPiercingRune3_3_L.Text = string.Empty;
                        break;
                    case 4:
                        Img4_L.Source = new BitmapImage(new Uri(imgCharaPath + Common.Common.FormationImageNothing + imgExt));
                        Img4_L.ToolTip = string.Empty;
                        LabRarity4_L.Content = string.Empty;
                        LabLevel4_L.Content = string.Empty;
                        TextSpeedRune4_1_L.Text = string.Empty;
                        TextSpeedRune4_2_L.Text = string.Empty;
                        TextSpeedRune4_3_L.Text = string.Empty;
                        TextPiercingRune4_1_L.Text = string.Empty;
                        TextPiercingRune4_2_L.Text = string.Empty;
                        TextPiercingRune4_3_L.Text = string.Empty;
                        break;
                    case 5:
                        Img5_L.Source = new BitmapImage(new Uri(imgCharaPath + Common.Common.FormationImageNothing + imgExt));
                        Img5_L.ToolTip = string.Empty;
                        LabRarity5_L.Content = string.Empty;
                        LabLevel5_L.Content = string.Empty;
                        TextSpeedRune5_1_L.Text = string.Empty;
                        TextSpeedRune5_2_L.Text = string.Empty;
                        TextSpeedRune5_3_L.Text = string.Empty;
                        TextPiercingRune5_1_L.Text = string.Empty;
                        TextPiercingRune5_2_L.Text = string.Empty;
                        TextPiercingRune5_3_L.Text = string.Empty;
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }

        private void LoadRightFormation()
        {
            try
            {
                if (null == rightFormations || (null != rightFormations && 1 > rightFormations.Length))
                {
                    return;
                }

                rightPos = rightNowPage - 1;

                FormationDto formation = rightFormations[rightPos];

                LabFormationId_R.Content = formation.Id;
                TextFormationName_R.Text = formation.Name;
                TextFormationSort_R.Text = formation.SortNo.ToString();

                if (formationsRightCnt > rightNowPage)
                {
                    BtnNext_R.IsEnabled = true;
                }
                else
                {
                    BtnNext_R.IsEnabled = false;
                }

                if (rightNowPage > 1)
                {
                    BtnBack_R.IsEnabled = true;
                }
                else
                {
                    BtnBack_R.IsEnabled = false;
                }

                CharacterDto[] characters = formation.Characters;

                int charCnt = characters.Length;

                if (charCnt > 0)
                {
                    Img1_R.Source = new BitmapImage(new Uri(imgCharaPath + characters[0].Name + imgExt));
                    Img1_R.ToolTip = characters[0].Name;
                    LabRarity1_R.Content = Common.Common.GetRarityType(characters[0].Rarity);
                    LabLevel1_R.Content = Common.Common.GetLevelText(characters[0].Level);
                    TextSpeedRune1_1_R.Text = characters[0].SpeedRune1.ToString();
                    TextSpeedRune1_2_R.Text = characters[0].SpeedRune2.ToString();
                    TextSpeedRune1_3_R.Text = characters[0].SpeedRune3.ToString();
                    TextPiercingRune1_1_R.Text = characters[0].PiercingRune1.ToString();
                    TextPiercingRune1_2_R.Text = characters[0].PiercingRune2.ToString();
                    TextPiercingRune1_3_R.Text = characters[0].PiercingRune3.ToString();
                }
                else
                {
                    ClearRightFormation(1);
                }

                if (charCnt > 1)
                {
                    Img2_R.Source = new BitmapImage(new Uri(imgCharaPath + characters[1].Name + imgExt));
                    Img2_R.ToolTip = characters[1].Name;
                    LabRarity2_R.Content = Common.Common.GetRarityType(characters[1].Rarity);
                    LabLevel2_R.Content = Common.Common.GetLevelText(characters[1].Level);
                    TextSpeedRune2_1_R.Text = characters[1].SpeedRune1.ToString();
                    TextSpeedRune2_2_R.Text = characters[1].SpeedRune2.ToString();
                    TextSpeedRune2_3_R.Text = characters[1].SpeedRune3.ToString();
                    TextPiercingRune2_1_R.Text = characters[1].PiercingRune1.ToString();
                    TextPiercingRune2_2_R.Text = characters[1].PiercingRune2.ToString();
                    TextPiercingRune2_3_R.Text = characters[1].PiercingRune3.ToString();
                }
                else
                {
                    ClearRightFormation(2);
                }

                if (charCnt > 2)
                {
                    Img3_R.Source = new BitmapImage(new Uri(imgCharaPath + characters[2].Name + imgExt));
                    Img3_R.ToolTip = characters[2].Name;
                    LabRarity3_R.Content = Common.Common.GetRarityType(characters[2].Rarity);
                    LabLevel3_R.Content = Common.Common.GetLevelText(characters[2].Level);
                    TextSpeedRune3_1_R.Text = characters[2].SpeedRune1.ToString();
                    TextSpeedRune3_2_R.Text = characters[2].SpeedRune2.ToString();
                    TextSpeedRune3_3_R.Text = characters[2].SpeedRune3.ToString();
                    TextPiercingRune3_1_R.Text = characters[2].PiercingRune1.ToString();
                    TextPiercingRune3_2_R.Text = characters[2].PiercingRune2.ToString();
                    TextPiercingRune3_3_R.Text = characters[2].PiercingRune3.ToString();
                }
                else
                {
                    ClearRightFormation(3);
                }

                if (charCnt > 3)
                {
                    Img4_R.Source = new BitmapImage(new Uri(imgCharaPath + characters[3].Name + imgExt));
                    Img4_R.ToolTip = characters[3].Name;
                    LabRarity4_R.Content = Common.Common.GetRarityType(characters[3].Rarity);
                    LabLevel4_R.Content = Common.Common.GetLevelText(characters[3].Level);
                    TextSpeedRune4_1_R.Text = characters[3].SpeedRune1.ToString();
                    TextSpeedRune4_2_R.Text = characters[3].SpeedRune2.ToString();
                    TextSpeedRune4_3_R.Text = characters[3].SpeedRune3.ToString();
                    TextPiercingRune4_1_R.Text = characters[3].PiercingRune1.ToString();
                    TextPiercingRune4_2_R.Text = characters[3].PiercingRune2.ToString();
                    TextPiercingRune4_3_R.Text = characters[3].PiercingRune3.ToString();
                }
                else
                {
                    ClearRightFormation(4);
                }

                if (charCnt > 4)
                {
                    Img5_R.Source = new BitmapImage(new Uri(imgCharaPath + characters[4].Name + imgExt));
                    Img5_R.ToolTip = characters[4].Name;
                    LabRarity5_R.Content = Common.Common.GetRarityType(characters[4].Rarity);
                    LabLevel5_R.Content = Common.Common.GetLevelText(characters[4].Level);
                    TextSpeedRune5_1_R.Text = characters[4].SpeedRune1.ToString();
                    TextSpeedRune5_2_R.Text = characters[4].SpeedRune2.ToString();
                    TextSpeedRune5_3_R.Text = characters[4].SpeedRune3.ToString();
                    TextPiercingRune5_1_R.Text = characters[4].PiercingRune1.ToString();
                    TextPiercingRune5_2_R.Text = characters[4].PiercingRune2.ToString();
                    TextPiercingRune5_3_R.Text = characters[4].PiercingRune3.ToString();
                }
                else
                {
                    ClearRightFormation(5);
                }
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }

        private void ClearRightFormation(int pos)
        {
            try
            {
                switch (pos)
                {
                    case 0:
                        LabFormationId_R.Content = string.Empty;
                        TextFormationName_R.Text = string.Empty;
                        TextFormationSort_R.Text = string.Empty;
                        BtnNext_R.IsEnabled = false;
                        BtnBack_R.IsEnabled = false;
                        rightNowPage = 1;
                        break;
                    case 1:
                        Img1_R.Source = new BitmapImage(new Uri(imgCharaPath + Common.Common.FormationImageNothing + imgExt));
                        Img1_R.ToolTip = string.Empty;
                        LabRarity1_R.Content = string.Empty;
                        LabLevel1_R.Content = string.Empty;
                        TextSpeedRune1_1_R.Text = string.Empty;
                        TextSpeedRune1_2_R.Text = string.Empty;
                        TextSpeedRune1_3_R.Text = string.Empty;
                        TextPiercingRune1_1_R.Text = string.Empty;
                        TextPiercingRune1_2_R.Text = string.Empty;
                        TextPiercingRune1_3_R.Text = string.Empty;
                        break;
                    case 2:
                        Img2_R.Source = new BitmapImage(new Uri(imgCharaPath + Common.Common.FormationImageNothing + imgExt));
                        Img2_R.ToolTip = string.Empty;
                        LabRarity2_R.Content = string.Empty;
                        LabLevel2_R.Content = string.Empty;
                        TextSpeedRune2_1_R.Text = string.Empty;
                        TextSpeedRune2_2_R.Text = string.Empty;
                        TextSpeedRune2_3_R.Text = string.Empty;
                        TextPiercingRune2_1_R.Text = string.Empty;
                        TextPiercingRune2_2_R.Text = string.Empty;
                        TextPiercingRune2_3_R.Text = string.Empty;
                        break;
                    case 3:
                        Img3_R.Source = new BitmapImage(new Uri(imgCharaPath + Common.Common.FormationImageNothing + imgExt));
                        Img3_R.ToolTip = string.Empty;
                        LabRarity3_R.Content = string.Empty;
                        LabLevel3_R.Content = string.Empty;
                        TextSpeedRune3_1_R.Text = string.Empty;
                        TextSpeedRune3_2_R.Text = string.Empty;
                        TextSpeedRune3_3_R.Text = string.Empty;
                        TextPiercingRune3_1_R.Text = string.Empty;
                        TextPiercingRune3_2_R.Text = string.Empty;
                        TextPiercingRune3_3_R.Text = string.Empty;
                        break;
                    case 4:
                        Img4_R.Source = new BitmapImage(new Uri(imgCharaPath + Common.Common.FormationImageNothing + imgExt));
                        Img4_R.ToolTip = string.Empty;
                        LabRarity4_R.Content = string.Empty;
                        LabLevel4_R.Content = string.Empty;
                        TextSpeedRune4_1_R.Text = string.Empty;
                        TextSpeedRune4_2_R.Text = string.Empty;
                        TextSpeedRune4_3_R.Text = string.Empty;
                        TextPiercingRune4_1_R.Text = string.Empty;
                        TextPiercingRune4_2_R.Text = string.Empty;
                        TextPiercingRune4_3_R.Text = string.Empty;
                        break;
                    case 5:
                        Img5_R.Source = new BitmapImage(new Uri(imgCharaPath + Common.Common.FormationImageNothing + imgExt));
                        Img5_R.ToolTip = string.Empty;
                        LabRarity5_R.Content = string.Empty;
                        LabLevel5_R.Content = string.Empty;
                        TextSpeedRune5_1_R.Text = string.Empty;
                        TextSpeedRune5_2_R.Text = string.Empty;
                        TextSpeedRune5_3_R.Text = string.Empty;
                        TextPiercingRune5_1_R.Text = string.Empty;
                        TextPiercingRune5_2_R.Text = string.Empty;
                        TextPiercingRune5_3_R.Text = string.Empty;
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }

        private void BtnBack_L_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                leftNowPage--;
                LoadLeftFormation();
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }

        private void BtnNext_L_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                leftNowPage++;
                LoadLeftFormation();
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }

        private void BtnBack_R_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                rightNowPage--;
                LoadRightFormation();
            }
            catch (Exception)
            {

            }
        }

        private void BtnNext_R_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                rightNowPage++;
                LoadRightFormation();
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }

        private void TextSpeedRune1_1_L_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (null != leftFormations && int.TryParse(TextSpeedRune1_1_L.Text, out int value))
                {
                    FormationDto formation = leftFormations[leftPos];
                    CharacterDto characterDto = formation.Characters[0];
                    characterDto.SpeedRune1 = value;
                }
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }

        private void TextSpeedRune1_2_L_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (null != leftFormations && int.TryParse(TextSpeedRune1_2_L.Text, out int value))
                {
                    FormationDto formation = leftFormations[leftPos];
                    CharacterDto characterDto = formation.Characters[0];
                    characterDto.SpeedRune2 = value;
                }
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }

        private void TextSpeedRune1_3_L_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (null != leftFormations && int.TryParse(TextSpeedRune1_3_L.Text, out int value))
                {
                    FormationDto formation = leftFormations[leftPos];
                    CharacterDto characterDto = formation.Characters[0];
                    characterDto.SpeedRune3 = value;
                }
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }

        private void TextSpeedRune2_1_L_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (null != leftFormations && int.TryParse(TextSpeedRune2_1_L.Text, out int value))
                {
                    FormationDto formation = leftFormations[leftPos];
                    CharacterDto characterDto = formation.Characters[1];
                    characterDto.SpeedRune1 = value;
                }
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }

        private void TextSpeedRune2_2_L_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (null != leftFormations && int.TryParse(TextSpeedRune2_2_L.Text, out int value))
                {
                    FormationDto formation = leftFormations[leftPos];
                    CharacterDto characterDto = formation.Characters[1];
                    characterDto.SpeedRune2 = value;
                }
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }

        private void TextSpeedRune2_3_L_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (null != leftFormations && int.TryParse(TextSpeedRune2_3_L.Text, out int value))
                {
                    FormationDto formation = leftFormations[leftPos];
                    CharacterDto characterDto = formation.Characters[1];
                    characterDto.SpeedRune3 = value;
                }
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }

        private void TextSpeedRune3_1_L_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (null != leftFormations && int.TryParse(TextSpeedRune3_1_L.Text, out int value))
                {
                    FormationDto formation = leftFormations[leftPos];
                    CharacterDto characterDto = formation.Characters[2];
                    characterDto.SpeedRune1 = value;
                }
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }

        private void TextSpeedRune3_2_L_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (null != leftFormations && int.TryParse(TextSpeedRune3_2_L.Text, out int value))
                {
                    FormationDto formation = leftFormations[leftPos];
                    CharacterDto characterDto = formation.Characters[2];
                    characterDto.SpeedRune2 = value;
                }
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }

        private void TextSpeedRune3_3_L_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (null != leftFormations && int.TryParse(TextSpeedRune3_3_L.Text, out int value))
                {
                    FormationDto formation = leftFormations[leftPos];
                    CharacterDto characterDto = formation.Characters[2];
                    characterDto.SpeedRune3 = value;
                }
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }

        private void TextSpeedRune4_1_L_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (null != leftFormations && int.TryParse(TextSpeedRune4_1_L.Text, out int value))
                {
                    FormationDto formation = leftFormations[leftPos];
                    CharacterDto characterDto = formation.Characters[3];
                    characterDto.SpeedRune1 = value;
                }
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }

        private void TextSpeedRune4_2_L_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (null != leftFormations && int.TryParse(TextSpeedRune4_2_L.Text, out int value))
                {
                    FormationDto formation = leftFormations[leftPos];
                    CharacterDto characterDto = formation.Characters[3];
                    characterDto.SpeedRune2 = value;
                }
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }

        private void TextSpeedRune4_3_L_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (null != leftFormations && int.TryParse(TextSpeedRune4_3_L.Text, out int value))
                {
                    FormationDto formation = leftFormations[leftPos];
                    CharacterDto characterDto = formation.Characters[3];
                    characterDto.SpeedRune3 = value;
                }
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }

        private void TextSpeedRune5_1_L_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (null != leftFormations && int.TryParse(TextSpeedRune5_1_L.Text, out int value))
                {
                    FormationDto formation = leftFormations[leftPos];
                    CharacterDto characterDto = formation.Characters[4];
                    characterDto.SpeedRune1 = value;
                }
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }

        private void TextSpeedRune5_2_L_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (null != leftFormations && int.TryParse(TextSpeedRune5_2_L.Text, out int value))
                {
                    FormationDto formation = leftFormations[leftPos];
                    CharacterDto characterDto = formation.Characters[4];
                    characterDto.SpeedRune2 = value;
                }
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }

        private void TextSpeedRune5_3_L_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (null != leftFormations && int.TryParse(TextSpeedRune5_3_L.Text, out int value))
                {
                    FormationDto formation = leftFormations[leftPos];
                    CharacterDto characterDto = formation.Characters[4];
                    characterDto.SpeedRune3 = value;
                }
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }

        private void TextSpeedRune1_1_R_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (null != rightFormations && int.TryParse(TextSpeedRune1_1_R.Text, out int value))
                {
                    FormationDto formation = rightFormations[rightPos];
                    CharacterDto characterDto = formation.Characters[0];
                    characterDto.SpeedRune1 = value;
                }
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }

        private void TextSpeedRune1_2_R_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (null != rightFormations && int.TryParse(TextSpeedRune1_2_R.Text, out int value))
                {
                    FormationDto formation = rightFormations[rightPos];
                    CharacterDto characterDto = formation.Characters[0];
                    characterDto.SpeedRune2 = value;
                }
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }

        private void TextSpeedRune1_3_R_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (null != rightFormations && int.TryParse(TextSpeedRune1_3_R.Text, out int value))
                {
                    FormationDto formation = rightFormations[rightPos];
                    CharacterDto characterDto = formation.Characters[0];
                    characterDto.SpeedRune3 = value;
                }
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }

        private void TextSpeedRune2_1_R_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (null != rightFormations && int.TryParse(TextSpeedRune2_1_R.Text, out int value))
                {
                    FormationDto formation = rightFormations[rightPos];
                    CharacterDto characterDto = formation.Characters[1];
                    characterDto.SpeedRune1 = value;
                }
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }

        private void TextSpeedRune2_2_R_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (null != rightFormations && int.TryParse(TextSpeedRune2_2_R.Text, out int value))
                {
                    FormationDto formation = rightFormations[rightPos];
                    CharacterDto characterDto = formation.Characters[1];
                    characterDto.SpeedRune2 = value;
                }
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }

        private void TextSpeedRune2_3_R_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (null != rightFormations && int.TryParse(TextSpeedRune2_3_R.Text, out int value))
                {
                    FormationDto formation = rightFormations[rightPos];
                    CharacterDto characterDto = formation.Characters[1];
                    characterDto.SpeedRune3 = value;
                }
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }

        private void TextSpeedRune3_1_R_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (null != rightFormations && int.TryParse(TextSpeedRune3_1_R.Text, out int value))
                {
                    FormationDto formation = rightFormations[rightPos];
                    CharacterDto characterDto = formation.Characters[2];
                    characterDto.SpeedRune1 = value;
                }
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }

        private void TextSpeedRune3_2_R_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (null != rightFormations && int.TryParse(TextSpeedRune3_2_R.Text, out int value))
                {
                    FormationDto formation = rightFormations[rightPos];
                    CharacterDto characterDto = formation.Characters[2];
                    characterDto.SpeedRune2 = value;
                }
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }

        private void TextSpeedRune3_3_R_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (null != rightFormations && int.TryParse(TextSpeedRune3_3_R.Text, out int value))
                {
                    FormationDto formation = rightFormations[rightPos];
                    CharacterDto characterDto = formation.Characters[2];
                    characterDto.SpeedRune3 = value;
                }
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }

        private void TextSpeedRune4_1_R_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (null != rightFormations && int.TryParse(TextSpeedRune4_1_R.Text, out int value))
                {
                    FormationDto formation = rightFormations[rightPos];
                    CharacterDto characterDto = formation.Characters[3];
                    characterDto.SpeedRune1 = value;
                }
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }

        private void TextSpeedRune4_2_R_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (null != rightFormations && int.TryParse(TextSpeedRune4_2_R.Text, out int value))
                {
                    FormationDto formation = rightFormations[rightPos];
                    CharacterDto characterDto = formation.Characters[3];
                    characterDto.SpeedRune2 = value;
                }
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }

        private void TextSpeedRune4_3_R_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (null != rightFormations && int.TryParse(TextSpeedRune4_3_R.Text, out int value))
                {
                    FormationDto formation = rightFormations[rightPos];
                    CharacterDto characterDto = formation.Characters[3];
                    characterDto.SpeedRune3 = value;
                }
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }

        private void TextSpeedRune5_1_R_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (null != rightFormations && int.TryParse(TextSpeedRune5_1_R.Text, out int value))
                {
                    FormationDto formation = rightFormations[rightPos];
                    CharacterDto characterDto = formation.Characters[4];
                    characterDto.SpeedRune1 = value;
                }
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }

        private void TextSpeedRune5_2_R_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (null != rightFormations && int.TryParse(TextSpeedRune5_2_R.Text, out int value))
                {
                    FormationDto formation = rightFormations[rightPos];
                    CharacterDto characterDto = formation.Characters[4];
                    characterDto.SpeedRune2 = value;
                }
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }

        private void TextSpeedRune5_3_R_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (null != rightFormations && int.TryParse(TextSpeedRune5_3_R.Text, out int value))
                {
                    FormationDto formation = rightFormations[rightPos];
                    CharacterDto characterDto = formation.Characters[4];
                    characterDto.SpeedRune3 = value;
                }
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }

        private void LeftImageMouseLeftButtonUp()
        {
            try
            {
                FormationDto formationDto = null;
                bool isNewFormations = true;

                if (null != leftFormations)
                {
                    formationDto = leftFormations[leftPos];
                    isNewFormations = false;
                }
                else
                {
                    formationDto = new FormationDto
                    {
                        Name = "仮編成中",
                        Characters = new CharacterDto[0]
                    };
                }

                CharacterSelection characterSelection = new CharacterSelection(formationDto);

                characterSelection.ShowDialog();

                if (isNewFormations)
                {
                    leftFormations = new FormationDto[1];
                    leftFormations[0] = characterSelection.FormationDto;
                    leftNowPage = 1;
                }

                LoadLeftFormation();
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }

        private void RightImageMouseLeftButtonUp()
        {
            try
            {
                FormationDto formationDto = null;
                bool isNewFormations = true;

                if (null != rightFormations)
                {
                    formationDto = rightFormations[rightPos];
                    isNewFormations = false;
                }
                else
                {
                    formationDto = new FormationDto
                    {
                        Name = "仮編成中",
                        Characters = new CharacterDto[0]
                    };
                }

                CharacterSelection characterSelection = new CharacterSelection(formationDto);

                characterSelection.ShowDialog();

                if (isNewFormations)
                {
                    rightFormations = new FormationDto[1];
                    rightFormations[0] = characterSelection.FormationDto;
                    rightNowPage = 1;
                }

                LoadRightFormation();
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }

        private void Img1_L_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                LeftImageMouseLeftButtonUp();
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }

        private void Img2_L_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                LeftImageMouseLeftButtonUp();
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }

        private void Img3_L_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                LeftImageMouseLeftButtonUp();
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }

        private void Img4_L_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                LeftImageMouseLeftButtonUp();
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }

        private void Img5_L_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                LeftImageMouseLeftButtonUp();
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }

        private void Img1_R_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                RightImageMouseLeftButtonUp();
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }

        private void Img2_R_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                RightImageMouseLeftButtonUp();
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }

        private void Img3_R_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                RightImageMouseLeftButtonUp();
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }

        private void Img4_R_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                RightImageMouseLeftButtonUp();
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }

        private void Img5_R_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                RightImageMouseLeftButtonUp();
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }

        private void LabFormationId_L_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Label label = (Label)sender;

                Clipboard.SetData(DataFormats.Text, label.Content);
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }

        private void LabFormationId_R_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Label label = (Label)sender;

                Clipboard.SetData(DataFormats.Text, label.Content);
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }

        private void BtnFVSFSet_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int.TryParse(TextDefenseKONum.Text, out int deBuffOK);

                if (null != fVSFManagement)
                {
                    fVSFManagement.UpdateData(leftFormations[leftPos].Id,
                                              rightFormations[rightPos].Id,
                                              deBuffOK);
                }

                Close();
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }
    }
}
