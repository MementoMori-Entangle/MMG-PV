using log4net;
using MMG.Dto;
using System;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MMG
{
    /// <summary>
    /// CharacterSpeed.xaml の相互作用ロジック
    /// </summary>
    public partial class CharacterSpeed : Window
    {
        private readonly string imgCharaPath = Common.Common.FormationCharaImagePath;
        private readonly string imgExt = Common.Common.FormationImageExtension;
        private readonly string LEFT_STR = ">";
        private readonly string RIGHT_STR = "<";
        private readonly string EQUAL_STR = "=";
        private readonly Color TOP_SPEED_COLOR = Color.FromRgb(255, 0, 0);
        private readonly Color BLACK_COLOR = Color.FromRgb(0, 0, 0);

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

        public CharacterSpeed()
        {
            InitializeComponent();
        }

        private void BtnPlayerLeft_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                PlayerSearch playerSearch = new PlayerSearch(2)
                {
                    Cs = this,
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
                PlayerSearch playerSearch = new PlayerSearch(2)
                {
                    Cs = this,
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

        private void LoadLeftFormation()
        {
            try
            {
                if (null == leftFormations || (null != leftFormations && 1 > leftFormations.Length))
                {
                    return;
                }

                leftPos = leftNowPage - 1;

                FormationDto formationDto = leftFormations[leftPos];

                if (null == formationDto)
                {
                    return;
                }

                LabFormationId_L.Content = formationDto.Id;
                TextFormationName_L.Text = formationDto.Name;
                TextFormationSort_L.Text = formationDto.SortNo.ToString();

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

                CharacterDto[] characters = formationDto.Characters;

                int charaCnt = characters.Length;
                int[] initSpeed = new int[charaCnt];

                CharacterDto[] calcCharaDtos = Common.Common.GetNewCharacterDtos(formationDto, charaCnt);

                for (int i = 0; i < charaCnt; i++)
                {
                    initSpeed[i] = formationDto.Characters[i].Speed;
                }

                FormationDto calcFDto = new FormationDto()
                {
                    Id = formationDto.Id,
                    Characters = calcCharaDtos
                };

                if (charaCnt > 0)
                {
                    Img1_L.Source = new BitmapImage(new Uri(imgCharaPath + characters[0].Name + imgExt));
                    Img1_L.ToolTip = characters[0].Name;
                    LabRarity1_L.Content = Common.Common.GetRarityType(characters[0].Rarity);
                    LabLevel1_L.Content = Common.Common.GetLevelText(characters[0].Level);
                    TextSpeedRune1_1_L.Text = characters[0].SpeedRune1.ToString();
                    TextSpeedRune1_2_L.Text = characters[0].SpeedRune2.ToString();
                    TextSpeedRune1_3_L.Text = characters[0].SpeedRune3.ToString();
                    LabLastSpeedL1.Content = Common.Common.GetLastSpeedInfo(calcFDto, initSpeed, 0);
                }
                else
                {
                    ClearLeftFormation(1);
                }

                if (charaCnt > 1)
                {
                    Img2_L.Source = new BitmapImage(new Uri(imgCharaPath + characters[1].Name + imgExt));
                    Img2_L.ToolTip = characters[1].Name;
                    LabRarity2_L.Content = Common.Common.GetRarityType(characters[1].Rarity);
                    LabLevel2_L.Content = Common.Common.GetLevelText(characters[1].Level);
                    TextSpeedRune2_1_L.Text = characters[1].SpeedRune1.ToString();
                    TextSpeedRune2_2_L.Text = characters[1].SpeedRune2.ToString();
                    TextSpeedRune2_3_L.Text = characters[1].SpeedRune3.ToString();
                    LabLastSpeedL2.Content = Common.Common.GetLastSpeedInfo(calcFDto, initSpeed, 1);
                }
                else
                {
                    ClearLeftFormation(2);
                }

                if (charaCnt > 2)
                {
                    Img3_L.Source = new BitmapImage(new Uri(imgCharaPath + characters[2].Name + imgExt));
                    Img3_L.ToolTip = characters[2].Name;
                    LabRarity3_L.Content = Common.Common.GetRarityType(characters[2].Rarity);
                    LabLevel3_L.Content = Common.Common.GetLevelText(characters[2].Level);
                    TextSpeedRune3_1_L.Text = characters[2].SpeedRune1.ToString();
                    TextSpeedRune3_2_L.Text = characters[2].SpeedRune2.ToString();
                    TextSpeedRune3_3_L.Text = characters[2].SpeedRune3.ToString();
                    LabLastSpeedL3.Content = Common.Common.GetLastSpeedInfo(calcFDto, initSpeed, 2);
                }
                else
                {
                    ClearLeftFormation(3);
                }

                if (charaCnt > 3)
                {
                    Img4_L.Source = new BitmapImage(new Uri(imgCharaPath + characters[3].Name + imgExt));
                    Img4_L.ToolTip = characters[3].Name;
                    LabRarity4_L.Content = Common.Common.GetRarityType(characters[3].Rarity);
                    LabLevel4_L.Content = Common.Common.GetLevelText(characters[3].Level);
                    TextSpeedRune4_1_L.Text = characters[3].SpeedRune1.ToString();
                    TextSpeedRune4_2_L.Text = characters[3].SpeedRune2.ToString();
                    TextSpeedRune4_3_L.Text = characters[3].SpeedRune3.ToString();
                    LabLastSpeedL4.Content = Common.Common.GetLastSpeedInfo(calcFDto, initSpeed, 3);
                }
                else
                {
                    ClearLeftFormation(4);
                }

                if (charaCnt > 4)
                {
                    Img5_L.Source = new BitmapImage(new Uri(imgCharaPath + characters[4].Name + imgExt));
                    Img5_L.ToolTip = characters[4].Name;
                    LabRarity5_L.Content = Common.Common.GetRarityType(characters[4].Rarity);
                    LabLevel5_L.Content = Common.Common.GetLevelText(characters[4].Level);
                    TextSpeedRune5_1_L.Text = characters[4].SpeedRune1.ToString();
                    TextSpeedRune5_2_L.Text = characters[4].SpeedRune2.ToString();
                    TextSpeedRune5_3_L.Text = characters[4].SpeedRune3.ToString();
                    LabLastSpeedL5.Content = Common.Common.GetLastSpeedInfo(calcFDto, initSpeed, 4);
                }
                else
                {
                    ClearLeftFormation(5);
                }

                ResetSpeedLRStr(0);
                CalcSpeedLRStr();
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
                        LabLastSpeedL1.Content = string.Empty;
                        LabLastSpeedL2.Content = string.Empty;
                        LabLastSpeedL3.Content = string.Empty;
                        LabLastSpeedL4.Content = string.Empty;
                        LabLastSpeedL5.Content = string.Empty;
                        break;
                    case 1:
                        Img1_L.Source = new BitmapImage(new Uri(imgCharaPath + Common.Common.FormationImageNothing + imgExt));
                        Img1_L.ToolTip = string.Empty;
                        LabRarity1_L.Content = string.Empty;
                        LabLevel1_L.Content = string.Empty;
                        TextSpeedRune1_1_L.Text = string.Empty;
                        TextSpeedRune1_2_L.Text = string.Empty;
                        TextSpeedRune1_3_L.Text = string.Empty;
                        LabLastSpeedL1.Content = string.Empty;
                        break;
                    case 2:
                        Img2_L.Source = new BitmapImage(new Uri(imgCharaPath + Common.Common.FormationImageNothing + imgExt));
                        Img2_L.ToolTip = string.Empty;
                        LabRarity2_L.Content = string.Empty;
                        LabLevel2_L.Content = string.Empty;
                        TextSpeedRune2_1_L.Text = string.Empty;
                        TextSpeedRune2_2_L.Text = string.Empty;
                        TextSpeedRune2_3_L.Text = string.Empty;
                        LabLastSpeedL2.Content = string.Empty;
                        break;
                    case 3:
                        Img3_L.Source = new BitmapImage(new Uri(imgCharaPath + Common.Common.FormationImageNothing + imgExt));
                        Img3_L.ToolTip = string.Empty;
                        LabRarity3_L.Content = string.Empty;
                        LabLevel3_L.Content = string.Empty;
                        TextSpeedRune3_1_L.Text = string.Empty;
                        TextSpeedRune3_2_L.Text = string.Empty;
                        TextSpeedRune3_3_L.Text = string.Empty;
                        LabLastSpeedL3.Content = string.Empty;
                        break;
                    case 4:
                        Img4_L.Source = new BitmapImage(new Uri(imgCharaPath + Common.Common.FormationImageNothing + imgExt));
                        Img4_L.ToolTip = string.Empty;
                        LabRarity4_L.Content = string.Empty;
                        LabLevel4_L.Content = string.Empty;
                        TextSpeedRune4_1_L.Text = string.Empty;
                        TextSpeedRune4_2_L.Text = string.Empty;
                        TextSpeedRune4_3_L.Text = string.Empty;
                        LabLastSpeedL4.Content = string.Empty;
                        break;
                    case 5:
                        Img5_L.Source = new BitmapImage(new Uri(imgCharaPath + Common.Common.FormationImageNothing + imgExt));
                        Img5_L.ToolTip = string.Empty;
                        LabRarity5_L.Content = string.Empty;
                        LabLevel5_L.Content = string.Empty;
                        TextSpeedRune5_1_L.Text = string.Empty;
                        TextSpeedRune5_2_L.Text = string.Empty;
                        TextSpeedRune5_3_L.Text = string.Empty;
                        LabLastSpeedL5.Content = string.Empty;
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

                FormationDto formationDto = rightFormations[rightPos];

                LabFormationId_R.Content = formationDto.Id;
                TextFormationName_R.Text = formationDto.Name;
                TextFormationSort_R.Text = formationDto.SortNo.ToString();

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

                CharacterDto[] characters = formationDto.Characters;

                int charaCnt = characters.Length;
                int[] initSpeed = new int[charaCnt];

                CharacterDto[] calcCharaDtos = Common.Common.GetNewCharacterDtos(formationDto, charaCnt);

                for (int i = 0; i < charaCnt; i++)
                {
                    initSpeed[i] = formationDto.Characters[i].Speed;
                }

                FormationDto calcFDto = new FormationDto()
                {
                    Id = formationDto.Id,
                    Characters = calcCharaDtos
                };

                if (charaCnt > 0)
                {
                    Img1_R.Source = new BitmapImage(new Uri(imgCharaPath + characters[0].Name + imgExt));
                    Img1_R.ToolTip = characters[0].Name;
                    LabRarity1_R.Content = Common.Common.GetRarityType(characters[0].Rarity);
                    LabLevel1_R.Content = Common.Common.GetLevelText(characters[0].Level);
                    TextSpeedRune1_1_R.Text = characters[0].SpeedRune1.ToString();
                    TextSpeedRune1_2_R.Text = characters[0].SpeedRune2.ToString();
                    TextSpeedRune1_3_R.Text = characters[0].SpeedRune3.ToString();
                    LabLastSpeedR1.Content = Common.Common.GetLastSpeedInfo(calcFDto, initSpeed, 0);
                }
                else
                {
                    ClearRightFormation(1);
                }

                if (charaCnt > 1)
                {
                    Img2_R.Source = new BitmapImage(new Uri(imgCharaPath + characters[1].Name + imgExt));
                    Img2_R.ToolTip = characters[1].Name;
                    LabRarity2_R.Content = Common.Common.GetRarityType(characters[1].Rarity);
                    LabLevel2_R.Content = Common.Common.GetLevelText(characters[1].Level);
                    TextSpeedRune2_1_R.Text = characters[1].SpeedRune1.ToString();
                    TextSpeedRune2_2_R.Text = characters[1].SpeedRune2.ToString();
                    TextSpeedRune2_3_R.Text = characters[1].SpeedRune3.ToString();
                    LabLastSpeedR2.Content = Common.Common.GetLastSpeedInfo(calcFDto, initSpeed, 1);
                }
                else
                {
                    ClearRightFormation(2);
                }

                if (charaCnt > 2)
                {
                    Img3_R.Source = new BitmapImage(new Uri(imgCharaPath + characters[2].Name + imgExt));
                    Img3_R.ToolTip = characters[2].Name;
                    LabRarity3_R.Content = Common.Common.GetRarityType(characters[2].Rarity);
                    LabLevel3_R.Content = Common.Common.GetLevelText(characters[2].Level);
                    TextSpeedRune3_1_R.Text = characters[2].SpeedRune1.ToString();
                    TextSpeedRune3_2_R.Text = characters[2].SpeedRune2.ToString();
                    TextSpeedRune3_3_R.Text = characters[2].SpeedRune3.ToString();
                    LabLastSpeedR3.Content = Common.Common.GetLastSpeedInfo(calcFDto, initSpeed, 2);
                }
                else
                {
                    ClearRightFormation(3);
                }

                if (charaCnt > 3)
                {
                    Img4_R.Source = new BitmapImage(new Uri(imgCharaPath + characters[3].Name + imgExt));
                    Img4_R.ToolTip = characters[3].Name;
                    LabRarity4_R.Content = Common.Common.GetRarityType(characters[3].Rarity);
                    LabLevel4_R.Content = Common.Common.GetLevelText(characters[3].Level);
                    TextSpeedRune4_1_R.Text = characters[3].SpeedRune1.ToString();
                    TextSpeedRune4_2_R.Text = characters[3].SpeedRune2.ToString();
                    TextSpeedRune4_3_R.Text = characters[3].SpeedRune3.ToString();
                    LabLastSpeedR4.Content = Common.Common.GetLastSpeedInfo(calcFDto, initSpeed, 3);
                }
                else
                {
                    ClearRightFormation(4);
                }

                if (charaCnt > 4)
                {
                    Img5_R.Source = new BitmapImage(new Uri(imgCharaPath + characters[4].Name + imgExt));
                    Img5_R.ToolTip = characters[4].Name;
                    LabRarity5_R.Content = Common.Common.GetRarityType(characters[4].Rarity);
                    LabLevel5_R.Content = Common.Common.GetLevelText(characters[4].Level);
                    TextSpeedRune5_1_R.Text = characters[4].SpeedRune1.ToString();
                    TextSpeedRune5_2_R.Text = characters[4].SpeedRune2.ToString();
                    TextSpeedRune5_3_R.Text = characters[4].SpeedRune3.ToString();
                    LabLastSpeedR5.Content = Common.Common.GetLastSpeedInfo(calcFDto, initSpeed, 4);
                }
                else
                {
                    ClearRightFormation(5);
                }

                ResetSpeedLRStr(1);
                CalcSpeedLRStr();
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
                        LabLastSpeedR1.Content = string.Empty;
                        LabLastSpeedR2.Content = string.Empty;
                        LabLastSpeedR3.Content = string.Empty;
                        LabLastSpeedR4.Content = string.Empty;
                        LabLastSpeedR5.Content = string.Empty;
                        break;
                    case 1:
                        Img1_R.Source = new BitmapImage(new Uri(imgCharaPath + Common.Common.FormationImageNothing + imgExt));
                        Img1_R.ToolTip = string.Empty;
                        LabRarity1_R.Content = string.Empty;
                        LabLevel1_R.Content = string.Empty;
                        TextSpeedRune1_1_R.Text = string.Empty;
                        TextSpeedRune1_2_R.Text = string.Empty;
                        TextSpeedRune1_3_R.Text = string.Empty;
                        LabLastSpeedR1.Content = string.Empty;
                        break;
                    case 2:
                        Img2_R.Source = new BitmapImage(new Uri(imgCharaPath + Common.Common.FormationImageNothing + imgExt));
                        Img2_R.ToolTip = string.Empty;
                        LabRarity2_R.Content = string.Empty;
                        LabLevel2_R.Content = string.Empty;
                        TextSpeedRune2_1_R.Text = string.Empty;
                        TextSpeedRune2_2_R.Text = string.Empty;
                        TextSpeedRune2_3_R.Text = string.Empty;
                        LabLastSpeedR2.Content = string.Empty;
                        break;
                    case 3:
                        Img3_R.Source = new BitmapImage(new Uri(imgCharaPath + Common.Common.FormationImageNothing + imgExt));
                        Img3_R.ToolTip = string.Empty;
                        LabRarity3_R.Content = string.Empty;
                        LabLevel3_R.Content = string.Empty;
                        TextSpeedRune3_1_R.Text = string.Empty;
                        TextSpeedRune3_2_R.Text = string.Empty;
                        TextSpeedRune3_3_R.Text = string.Empty;
                        LabLastSpeedR3.Content = string.Empty;
                        break;
                    case 4:
                        Img4_R.Source = new BitmapImage(new Uri(imgCharaPath + Common.Common.FormationImageNothing + imgExt));
                        Img4_R.ToolTip = string.Empty;
                        LabRarity4_R.Content = string.Empty;
                        LabLevel4_R.Content = string.Empty;
                        TextSpeedRune4_1_R.Text = string.Empty;
                        TextSpeedRune4_2_R.Text = string.Empty;
                        TextSpeedRune4_3_R.Text = string.Empty;
                        LabLastSpeedR4.Content = string.Empty;
                        break;
                    case 5:
                        Img5_R.Source = new BitmapImage(new Uri(imgCharaPath + Common.Common.FormationImageNothing + imgExt));
                        Img5_R.ToolTip = string.Empty;
                        LabRarity5_R.Content = string.Empty;
                        LabLevel5_R.Content = string.Empty;
                        TextSpeedRune5_1_R.Text = string.Empty;
                        TextSpeedRune5_2_R.Text = string.Empty;
                        TextSpeedRune5_3_R.Text = string.Empty;
                        LabLastSpeedR5.Content = string.Empty;
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

        private string JudgementLRStr(int left, int right, int speedDifference = 0)
        {
            if (left > right + speedDifference)
            {
                return LEFT_STR;
            }
            else if (left < right + speedDifference)
            {
                return RIGHT_STR;
            }
            else
            {
                return EQUAL_STR;
            }
        }

        private void ResetSpeedLRStr(int lr)
        {
            TextBVSL1.Foreground = new SolidColorBrush(BLACK_COLOR);
            TextBVSL2.Foreground = new SolidColorBrush(BLACK_COLOR);
            TextBVSL3.Foreground = new SolidColorBrush(BLACK_COLOR);
            TextBVSL4.Foreground = new SolidColorBrush(BLACK_COLOR);
            TextBVSL5.Foreground = new SolidColorBrush(BLACK_COLOR);

            if (lr == 0)
            {
                LabLastSpeedL1.Foreground = new SolidColorBrush(BLACK_COLOR);
                LabLastSpeedL2.Foreground = new SolidColorBrush(BLACK_COLOR);
                LabLastSpeedL3.Foreground = new SolidColorBrush(BLACK_COLOR);
                LabLastSpeedL4.Foreground = new SolidColorBrush(BLACK_COLOR);
                LabLastSpeedL5.Foreground = new SolidColorBrush(BLACK_COLOR);
            }
            else if (lr == 1)
            {
                LabLastSpeedR1.Foreground = new SolidColorBrush(BLACK_COLOR);
                LabLastSpeedR2.Foreground = new SolidColorBrush(BLACK_COLOR);
                LabLastSpeedR3.Foreground = new SolidColorBrush(BLACK_COLOR);
                LabLastSpeedR4.Foreground = new SolidColorBrush(BLACK_COLOR);
                LabLastSpeedR5.Foreground = new SolidColorBrush(BLACK_COLOR);
            }
        }

        private void CalcSpeedLRStr()
        {
            try
            {
                if (null == leftFormations || (null != leftFormations && 1 > leftFormations.Length) ||
                    null == rightFormations || (null != rightFormations && 1 > rightFormations.Length))
                {
                    return;
                }

                CharacterDto[] leftCharacterDtos = Common.Common.CalcCharacterSpeed(leftFormations[leftPos]);
                CharacterDto[] rightCharacterDtos = Common.Common.CalcCharacterSpeed(rightFormations[rightPos]);

                int leftTopSpeed = Common.Common.CalcTopSpeed(leftCharacterDtos);
                int rightTopSpeed = Common.Common.CalcTopSpeed(rightCharacterDtos);

                int leftLen = leftCharacterDtos.Length;
                int rightLen = rightCharacterDtos.Length;

                int[] leftSpeed = new int[leftLen];
                int speedWidth = int.Parse(TextSpeedWidth.Text);

                LabRightSpeed.Content = rightTopSpeed.ToString();
                TextBVS.Text = JudgementLRStr(leftTopSpeed, rightTopSpeed, speedWidth);

                if (0 < leftLen)
                {
                    TextBVSL1.Text = JudgementLRStr(leftCharacterDtos[0].Speed, rightTopSpeed, speedWidth);

                    if (leftTopSpeed == leftCharacterDtos[0].Speed)
                    {
                        TextBVSL1.Foreground = new SolidColorBrush(TOP_SPEED_COLOR);
                        LabLastSpeedL1.Foreground = new SolidColorBrush(TOP_SPEED_COLOR);
                    }
                }

                if (1 < leftLen)
                {
                    TextBVSL2.Text = JudgementLRStr(leftCharacterDtos[1].Speed, rightTopSpeed, speedWidth);

                    if (leftTopSpeed == leftCharacterDtos[1].Speed)
                    {
                        TextBVSL2.Foreground = new SolidColorBrush(TOP_SPEED_COLOR);
                        LabLastSpeedL2.Foreground = new SolidColorBrush(TOP_SPEED_COLOR);
                    }
                }

                if (2 < leftLen)
                {
                    TextBVSL3.Text = JudgementLRStr(leftCharacterDtos[2].Speed, rightTopSpeed, speedWidth);

                    if (leftTopSpeed == leftCharacterDtos[2].Speed)
                    {
                        TextBVSL3.Foreground = new SolidColorBrush(TOP_SPEED_COLOR);
                        LabLastSpeedL3.Foreground = new SolidColorBrush(TOP_SPEED_COLOR);
                    }
                }

                if (3 < leftLen)
                {
                    TextBVSL4.Text = JudgementLRStr(leftCharacterDtos[3].Speed, rightTopSpeed, speedWidth);

                    if (leftTopSpeed == leftCharacterDtos[3].Speed)
                    {
                        TextBVSL4.Foreground = new SolidColorBrush(TOP_SPEED_COLOR);
                        LabLastSpeedL4.Foreground = new SolidColorBrush(TOP_SPEED_COLOR);
                    }
                }

                if (4 < leftLen)
                {
                    TextBVSL5.Text = JudgementLRStr(leftCharacterDtos[4].Speed, rightTopSpeed, speedWidth);

                    if (leftTopSpeed == leftCharacterDtos[4].Speed)
                    {
                        TextBVSL5.Foreground = new SolidColorBrush(TOP_SPEED_COLOR);
                        LabLastSpeedL5.Foreground = new SolidColorBrush(TOP_SPEED_COLOR);
                    }
                }

                if (0 < rightLen)
                {
                    if (rightTopSpeed == rightCharacterDtos[0].Speed)
                    {
                        LabLastSpeedR1.Foreground = new SolidColorBrush(TOP_SPEED_COLOR);
                    }
                }

                if (1 < rightLen)
                {
                    if (rightTopSpeed == rightCharacterDtos[1].Speed)
                    {
                        LabLastSpeedR2.Foreground = new SolidColorBrush(TOP_SPEED_COLOR);
                    }
                }

                if (2 < rightLen)
                {
                    if (rightTopSpeed == rightCharacterDtos[2].Speed)
                    {
                        LabLastSpeedR3.Foreground = new SolidColorBrush(TOP_SPEED_COLOR);
                    }
                }

                if (3 < rightLen)
                {
                    if (rightTopSpeed == rightCharacterDtos[3].Speed)
                    {
                        LabLastSpeedR4.Foreground = new SolidColorBrush(TOP_SPEED_COLOR);
                    }
                }

                if (4 < rightLen)
                {
                    if (rightTopSpeed == rightCharacterDtos[4].Speed)
                    {
                        LabLastSpeedR5.Foreground = new SolidColorBrush(TOP_SPEED_COLOR);
                    }
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
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
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

        private void BtnSpeedLR_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                LoadLeftFormation();
                LoadRightFormation();
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }

        private void BtnAutoRuneSet_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (null == leftFormations || (null != leftFormations && 1 > leftFormations.Length) ||
                    null == rightFormations || (null != rightFormations && 1 > rightFormations.Length))
                {
                    MessageBox.Show("スピード調整するには編成してください。");
                    return;
                }

                CharacterDto[] leftCharacterDtos = Common.Common.CalcCharacterSpeed(leftFormations[leftPos]);
                CharacterDto[] rightCharacterDtos = Common.Common.CalcCharacterSpeed(rightFormations[rightPos]);

                int leftTopSpeed = Common.Common.CalcTopSpeed(leftCharacterDtos);
                int rightTopSpeed = Common.Common.CalcTopSpeed(rightCharacterDtos);

                int leftLen = leftCharacterDtos.Length;
                int speedWidth = int.Parse(TextSpeedWidth.Text);
                int charaCnt = leftFormations[leftPos].Characters.Length;

                CharacterDto[] newCharaDtos = Common.Common.GetNewCharacterDtos(leftFormations[leftPos], charaCnt);

                if (0 < leftLen)
                {
                    if (speedWidth + rightTopSpeed >= leftCharacterDtos[0].Speed)
                    {
                        int speedDifference = speedWidth + rightTopSpeed - leftCharacterDtos[0].Speed;

                        int[] newRunes = Common.Common.CalcAddSpeedRune(newCharaDtos[0], speedDifference);

                        TextSpeedRune1_1_L.Text = newRunes[0].ToString();
                        TextSpeedRune1_2_L.Text = newRunes[1].ToString();
                        TextSpeedRune1_3_L.Text = newRunes[2].ToString();
                    }
                }

                if (1 < leftLen)
                {
                    if (speedWidth + rightTopSpeed >= leftCharacterDtos[1].Speed)
                    {
                        int speedDifference = speedWidth + rightTopSpeed - leftCharacterDtos[1].Speed;

                        int[] newRunes = Common.Common.CalcAddSpeedRune(leftCharacterDtos[1], speedDifference);

                        TextSpeedRune2_1_L.Text = newRunes[0].ToString();
                        TextSpeedRune2_2_L.Text = newRunes[1].ToString();
                        TextSpeedRune2_3_L.Text = newRunes[2].ToString();
                    }
                }

                if (2 < leftLen)
                {
                    if (speedWidth + rightTopSpeed >= leftCharacterDtos[2].Speed)
                    {
                        int speedDifference = speedWidth + rightTopSpeed - leftCharacterDtos[2].Speed;

                        int[] newRunes = Common.Common.CalcAddSpeedRune(leftCharacterDtos[2], speedDifference);

                        TextSpeedRune3_1_L.Text = newRunes[0].ToString();
                        TextSpeedRune3_2_L.Text = newRunes[1].ToString();
                        TextSpeedRune3_3_L.Text = newRunes[2].ToString();
                    }
                }

                if (3 < leftLen)
                {
                    if (speedWidth + rightTopSpeed >= leftCharacterDtos[3].Speed)
                    {
                        int speedDifference = speedWidth + rightTopSpeed - leftCharacterDtos[3].Speed;

                        int[] newRunes = Common.Common.CalcAddSpeedRune(leftCharacterDtos[3], speedDifference);

                        TextSpeedRune4_1_L.Text = newRunes[0].ToString();
                        TextSpeedRune4_2_L.Text = newRunes[1].ToString();
                        TextSpeedRune4_3_L.Text = newRunes[2].ToString();
                    }
                }

                if (4 < leftLen)
                {
                    if (speedWidth + rightTopSpeed >= leftCharacterDtos[4].Speed)
                    {
                        int speedDifference = speedWidth + rightTopSpeed - leftCharacterDtos[4].Speed;

                        int[] newRunes = Common.Common.CalcAddSpeedRune(leftCharacterDtos[4], speedDifference);

                        TextSpeedRune5_1_L.Text = newRunes[0].ToString();
                        TextSpeedRune5_2_L.Text = newRunes[1].ToString();
                        TextSpeedRune5_3_L.Text = newRunes[2].ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }
    }
}
