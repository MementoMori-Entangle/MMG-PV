using log4net;
using MMG.Common;
using MMG.Dao;
using MMG.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using MessageBox = System.Windows.MessageBox;

namespace MMG
{
    /// <summary>
    /// MemberInfo.xaml の相互作用ロジック
    /// </summary>
    public partial class MemberInfo : Window
    {
        private bool isChange;
        private bool isNew;
        private int formationsCnt;
        private int nowPage;
        private int pos;
        private PlayerDto member;
        private List<PlayerDto> memberList;
        private FormationDto[] formations;
        private readonly string imgPath = Common.Common.FormationCharaImagePath;
        private readonly string imgExt = Common.Common.FormationImageExtension;

        private readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().Name);

        public List<FormationDto> AddFormationList { get; set; }

        public MemberInfo()
        {
            InitializeComponent();
        }

        public MemberInfo(PlayerDto member)
        {
            InitializeComponent();
            LoadGuildItems();
            LoadVirtualityFormationItems();
            LoadGBGroupItems();
            this.member = member;
            CheckGVS.Visibility = Visibility.Hidden;
        }

        public MemberInfo(PlayerDto member, List<PlayerDto> memberList, bool isChange = false, bool isNew = false)
        {
            InitializeComponent();
            LoadGuildItems();
            LoadVirtualityFormationItems();
            LoadGBGroupItems();
            this.member = member;
            this.memberList = memberList;
            this.isChange = isChange;
            this.isNew = isNew;
            CheckGVS.Visibility = Visibility.Hidden;

            if (isNew)
            {
                BtnUpdate.Content = "登録";
            }

            if (isChange)
            {
                CheckGVS.Visibility = Visibility.Visible;
            }

            if (!isNew && isChange)
            {
                Img1.MouseLeftButtonUp += new MouseButtonEventHandler(MouseLeftButtonUp_Click);
                Img2.MouseLeftButtonUp += new MouseButtonEventHandler(MouseLeftButtonUp_Click);
                Img3.MouseLeftButtonUp += new MouseButtonEventHandler(MouseLeftButtonUp_Click);
                Img4.MouseLeftButtonUp += new MouseButtonEventHandler(MouseLeftButtonUp_Click);
                Img5.MouseLeftButtonUp += new MouseButtonEventHandler(MouseLeftButtonUp_Click);

                TextSpeedRune1_1.IsEnabled = true;
                TextSpeedRune1_2.IsEnabled = true;
                TextSpeedRune1_3.IsEnabled = true;
                TextPiercingRune1_1.IsEnabled = true;
                TextPiercingRune1_2.IsEnabled = true;
                TextPiercingRune1_3.IsEnabled = true;
                TextSpeedRune2_1.IsEnabled = true;
                TextSpeedRune2_2.IsEnabled = true;
                TextSpeedRune2_3.IsEnabled = true;
                TextPiercingRune2_1.IsEnabled = true;
                TextPiercingRune2_2.IsEnabled = true;
                TextPiercingRune2_3.IsEnabled = true;
                TextSpeedRune3_1.IsEnabled = true;
                TextSpeedRune3_2.IsEnabled = true;
                TextSpeedRune3_3.IsEnabled = true;
                TextPiercingRune3_1.IsEnabled = true;
                TextPiercingRune3_2.IsEnabled = true;
                TextPiercingRune3_3.IsEnabled = true;
                TextSpeedRune4_1.IsEnabled = true;
                TextSpeedRune4_2.IsEnabled = true;
                TextSpeedRune4_3.IsEnabled = true;
                TextPiercingRune4_1.IsEnabled = true;
                TextPiercingRune4_2.IsEnabled = true;
                TextPiercingRune4_3.IsEnabled = true;
                TextSpeedRune5_1.IsEnabled = true;
                TextSpeedRune5_2.IsEnabled = true;
                TextSpeedRune5_3.IsEnabled = true;
                TextPiercingRune5_1.IsEnabled = true;
                TextPiercingRune5_2.IsEnabled = true;
                TextPiercingRune5_3.IsEnabled = true;
            }
        }

        public void LoadMemberInfo()
        {
            try
            {
                if (isNew)
                {
                    TextMemberId.Text = string.Empty;
                    TextMemberNo.Text = string.Empty;
                    TextMemberWorld.Text = string.Empty;
                    TextMemberDiscordUserId.Text = string.Empty;
                    TextMemberDiscordUserChannelId.Text = string.Empty;

                    TextMemberId.IsEnabled = true;
                    TextMemberWorld.IsEnabled = true;
                    CombBMemberGuild.IsEnabled = true;
                }
                else
                {
                    TextMemberId.Text = member.Id.ToString();
                    TextMemberNo.Text = member.No.ToString();
                    TextMemberWorld.Text = member.World.ToString();

                    Dictionary<long, ulong> mmUserIdUserIdDic = BaseDiscordUser.GetMMUserIdUserIdDic();
                    Dictionary<ulong, ulong> userIdChannelIdDic = BaseDiscordUser.GetUserDedicatedChannelIdDic();
                    Dictionary<ulong, bool> userIdRCPermissionDic = BaseDiscordUser.GetUserRCPermissionDic();

                    if (mmUserIdUserIdDic.ContainsKey(member.Id))
                    {
                        TextMemberDiscordUserId.Text = mmUserIdUserIdDic[member.Id].ToString();

                        if (userIdChannelIdDic.ContainsKey(mmUserIdUserIdDic[member.Id]))
                        {
                            TextMemberDiscordUserChannelId.Text = userIdChannelIdDic[mmUserIdUserIdDic[member.Id]].ToString();
                        }
                        else
                        {
                            TextMemberDiscordUserChannelId.Text = string.Empty;
                        }

                        if (userIdRCPermissionDic.ContainsKey(mmUserIdUserIdDic[member.Id]))
                        {
                            CheckDiscordRCP.IsChecked = userIdRCPermissionDic[mmUserIdUserIdDic[member.Id]];
                        }
                    }
                    else
                    {
                        TextMemberDiscordUserId.Text = string.Empty;
                        TextMemberDiscordUserChannelId.Text = string.Empty;
                    }
                }

                CombBMemberGuild.SelectedItem = member.GuildName;

                VirtualityFormationDto formationDto = new VirtualityFormationDto();
                List<VirtualityFormationDto> list = formationDto.GetDefaultList();

                foreach (VirtualityFormationDto dto in list)
                {
                    if (dto.Id == member.VirtualityFormation)
                    {
                        CombBMemberVF.SelectedItem = dto.Name;
                        break;
                    }
                }

                GBGroupDto[] gBGroupDtos = BaseGBGroup.GetBaseGBGroup();

                foreach (GBGroupDto dto in gBGroupDtos)
                {
                    if (dto.Id == member.GBGroupId)
                    {
                        CombBMemberGBGroup.SelectedItem = dto.Name;
                        break;
                    }
                }

                TextMemberName.Text = member.Name;
                TextMemberStamina.Text = member.Stamina.ToString();
                TextMemberForceValue.Text = member.ForceValue.ToString();
                TextMemberVFCNum.Text = member.VFCNum.ToString();
                TextMemberMainFormation.Text = member.MainFormation.ToString();
                CheckVC.IsChecked = member.IsVC;

                if (isChange)
                {
                    TextMemberNo.IsEnabled = true;
                    TextMemberName.IsEnabled = true;
                    TextMemberVFCNum.IsEnabled = true;
                    TextMemberMainFormation.IsEnabled = true;
                    CheckVC.IsEnabled = true;
                    TextMemberDiscordUserId.IsEnabled = true;
                    CombBMemberVF.IsEnabled = true;
                    CombBMemberGBGroup.IsEnabled = true;
                    CheckDiscordRCP.IsEnabled = true;
                    TextMemberDiscordUserChannelId.IsEnabled = true;

                    if (isNew)
                    {
                        BtnNewFormation.Visibility = Visibility.Hidden;
                        BtnDelete.Visibility = Visibility.Hidden;
                    }
                }
                else
                {
                    BtnNewFormation.Visibility = Visibility.Hidden;
                    BtnUpdate.Visibility = Visibility.Hidden;
                    CheckBackup.Visibility = Visibility.Hidden;
                    BtnDelete.Visibility = Visibility.Hidden;
                }

                formationsCnt = member.Formations.Length;

                if (formationsCnt == 0)
                {
                    return;
                }

                formations = member.Formations;
                nowPage = 1;

                if (formations.Length > 0)
                {
                    LoadFormation();
                }
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }

        private void LoadFormation()
        {
            try
            {
                pos = nowPage - 1;

                FormationDto formation = formations[pos];

                if (isNew)
                {
                    if (formation.Id > 0)
                    {
                        LabFormationId.Content = formation.Id;
                    }
                }
                else
                {
                    LabFormationId.Content = formation.Id;
                }

                TextFormationName.Text = formation.Name;
                TextFormationSort.Text = formation.SortNo.ToString();
                TextFormationDescription.Text = formation.Description;

                if (isChange)
                {
                    TextFormationName.IsEnabled = true;
                    TextFormationSort.IsEnabled = true;
                    TextFormationDescription.IsEnabled = true;
                    CheckSubParty.IsEnabled = true;
                    CheckDebuff.IsEnabled = true;
                    CheckGVS.IsEnabled = true;
                }

                if (formationsCnt > nowPage)
                {
                    BtnNext.IsEnabled = true;
                }
                else
                {
                    BtnNext.IsEnabled = false;
                }

                if (nowPage > 1)
                {
                    BtnBack.IsEnabled = true;
                }
                else
                {
                    BtnBack.IsEnabled = false;
                }

                CheckSubParty.IsChecked = formation.IsSubParty;
                CheckDebuff.IsChecked = formation.IsDebuff;
                CheckGVS.IsChecked = formation.IsGVS;

                CharacterDto[] characters = formation.Characters;

                int charCnt = characters.Length;

                if (charCnt > 0)
                {
                    Img1.Source = new BitmapImage(new Uri(imgPath + characters[0].Name + imgExt));
                    Img1.ToolTip = characters[0].Name;
                    LabRarity1.Content = Common.Common.GetRarityType(characters[0].Rarity);
                    LabLevel1.Content = Common.Common.GetLevelText(characters[0].Level);
                    TextSpeedRune1_1.Text = characters[0].SpeedRune1.ToString();
                    TextSpeedRune1_2.Text = characters[0].SpeedRune2.ToString();
                    TextSpeedRune1_3.Text = characters[0].SpeedRune3.ToString();
                    TextPiercingRune1_1.Text = characters[0].PiercingRune1.ToString();
                    TextPiercingRune1_2.Text = characters[0].PiercingRune2.ToString();
                    TextPiercingRune1_3.Text = characters[0].PiercingRune3.ToString();
                }
                else
                {
                    Img1.Source = new BitmapImage(new Uri(imgPath + Common.Common.FormationImageNothing + imgExt));
                    Img1.ToolTip = string.Empty;
                    LabRarity1.Content = string.Empty;
                    LabLevel1.Content = string.Empty;
                    TextSpeedRune1_1.Text = string.Empty;
                    TextSpeedRune1_2.Text = string.Empty;
                    TextSpeedRune1_3.Text = string.Empty;
                    TextPiercingRune1_1.Text = string.Empty;
                    TextPiercingRune1_2.Text = string.Empty;
                    TextPiercingRune1_3.Text = string.Empty;
                }

                if (charCnt > 1)
                {
                    Img2.Source = new BitmapImage(new Uri(imgPath + characters[1].Name + imgExt));
                    Img2.ToolTip = characters[1].Name;
                    LabRarity2.Content = Common.Common.GetRarityType(characters[1].Rarity);
                    LabLevel2.Content = Common.Common.GetLevelText(characters[1].Level);
                    TextSpeedRune2_1.Text = characters[1].SpeedRune1.ToString();
                    TextSpeedRune2_2.Text = characters[1].SpeedRune2.ToString();
                    TextSpeedRune2_3.Text = characters[1].SpeedRune3.ToString();
                    TextPiercingRune2_1.Text = characters[1].PiercingRune1.ToString();
                    TextPiercingRune2_2.Text = characters[1].PiercingRune2.ToString();
                    TextPiercingRune2_3.Text = characters[1].PiercingRune3.ToString();
                }
                else
                {
                    Img2.Source = new BitmapImage(new Uri(imgPath + Common.Common.FormationImageNothing + imgExt));
                    Img2.ToolTip = string.Empty;
                    LabRarity2.Content = string.Empty;
                    LabLevel2.Content = string.Empty;
                    TextSpeedRune2_1.Text = string.Empty;
                    TextSpeedRune2_2.Text = string.Empty;
                    TextSpeedRune2_3.Text = string.Empty;
                    TextPiercingRune2_1.Text = string.Empty;
                    TextPiercingRune2_2.Text = string.Empty;
                    TextPiercingRune2_3.Text = string.Empty;
                }

                if (charCnt > 2)
                {
                    Img3.Source = new BitmapImage(new Uri(imgPath + characters[2].Name + imgExt));
                    Img3.ToolTip = characters[2].Name;
                    LabRarity3.Content = Common.Common.GetRarityType(characters[2].Rarity);
                    LabLevel3.Content = Common.Common.GetLevelText(characters[2].Level);
                    TextSpeedRune3_1.Text = characters[2].SpeedRune1.ToString();
                    TextSpeedRune3_2.Text = characters[2].SpeedRune2.ToString();
                    TextSpeedRune3_3.Text = characters[2].SpeedRune3.ToString();
                    TextPiercingRune3_1.Text = characters[2].PiercingRune1.ToString();
                    TextPiercingRune3_2.Text = characters[2].PiercingRune2.ToString();
                    TextPiercingRune3_3.Text = characters[2].PiercingRune3.ToString();
                }
                else
                {
                    Img3.Source = new BitmapImage(new Uri(imgPath + Common.Common.FormationImageNothing + imgExt));
                    Img3.ToolTip = string.Empty;
                    LabRarity3.Content = string.Empty;
                    LabLevel3.Content = string.Empty;
                    TextSpeedRune3_1.Text = string.Empty;
                    TextSpeedRune3_2.Text = string.Empty;
                    TextSpeedRune3_3.Text = string.Empty;
                    TextPiercingRune3_1.Text = string.Empty;
                    TextPiercingRune3_2.Text = string.Empty;
                    TextPiercingRune3_3.Text = string.Empty;
                }

                if (charCnt > 3)
                {
                    Img4.Source = new BitmapImage(new Uri(imgPath + characters[3].Name + imgExt));
                    Img4.ToolTip = characters[3].Name;
                    LabRarity4.Content = Common.Common.GetRarityType(characters[3].Rarity);
                    LabLevel4.Content = Common.Common.GetLevelText(characters[3].Level);
                    TextSpeedRune4_1.Text = characters[3].SpeedRune1.ToString();
                    TextSpeedRune4_2.Text = characters[3].SpeedRune2.ToString();
                    TextSpeedRune4_3.Text = characters[3].SpeedRune3.ToString();
                    TextPiercingRune4_1.Text = characters[3].PiercingRune1.ToString();
                    TextPiercingRune4_2.Text = characters[3].PiercingRune2.ToString();
                    TextPiercingRune4_3.Text = characters[3].PiercingRune3.ToString();
                }
                else
                {
                    Img4.Source = new BitmapImage(new Uri(imgPath + Common.Common.FormationImageNothing + imgExt));
                    Img4.ToolTip = string.Empty;
                    LabRarity4.Content = string.Empty;
                    LabLevel4.Content = string.Empty;
                    TextSpeedRune4_1.Text = string.Empty;
                    TextSpeedRune4_2.Text = string.Empty;
                    TextSpeedRune4_3.Text = string.Empty;
                    TextPiercingRune4_1.Text = string.Empty;
                    TextPiercingRune4_2.Text = string.Empty;
                    TextPiercingRune4_3.Text = string.Empty;
                }

                if (charCnt > 4)
                {
                    Img5.Source = new BitmapImage(new Uri(imgPath + characters[4].Name + imgExt));
                    Img5.ToolTip = characters[4].Name;
                    LabRarity5.Content = Common.Common.GetRarityType(characters[4].Rarity);
                    LabLevel5.Content = Common.Common.GetLevelText(characters[4].Level);
                    TextSpeedRune5_1.Text = characters[4].SpeedRune1.ToString();
                    TextSpeedRune5_2.Text = characters[4].SpeedRune2.ToString();
                    TextSpeedRune5_3.Text = characters[4].SpeedRune3.ToString();
                    TextPiercingRune5_1.Text = characters[4].PiercingRune1.ToString();
                    TextPiercingRune5_2.Text = characters[4].PiercingRune2.ToString();
                    TextPiercingRune5_3.Text = characters[4].PiercingRune3.ToString();
                }
                else
                {
                    Img5.Source = new BitmapImage(new Uri(imgPath + Common.Common.FormationImageNothing + imgExt));
                    Img5.ToolTip = string.Empty;
                    LabRarity5.Content = string.Empty;
                    LabLevel5.Content = string.Empty;
                    TextSpeedRune5_1.Text = string.Empty;
                    TextSpeedRune5_2.Text = string.Empty;
                    TextSpeedRune5_3.Text = string.Empty;
                    TextPiercingRune5_1.Text = string.Empty;
                    TextPiercingRune5_2.Text = string.Empty;
                    TextPiercingRune5_3.Text = string.Empty;
                }
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }

        private void ImageMouseLeftButtonUp()
        {
            try
            {
                if (null == formations || formations.Length == 0)
                {
                    return;
                }

                FormationDto formationDto = null;

                formationDto = formations[pos];

                UpdateFormation();

                CharacterSelection characterSelection = new CharacterSelection(formationDto);

                characterSelection.ShowDialog();

                LoadFormation();
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }

        private void MouseLeftButtonUp_Click(object sender, MouseButtonEventArgs e)
        {
            try
            {
                ImageMouseLeftButtonUp();
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }

        private void LoadGBGroupItems()
        {
            try
            {
                GBGroupDto[] gBGroupDtos = BaseGBGroup.GetBaseGBGroup();

                gBGroupDtos = gBGroupDtos.Concat(BaseGBGroup.GetBaseGBGroup(1)).ToArray();

                foreach (GBGroupDto dto in gBGroupDtos)
                {
                    CombBMemberGBGroup.Items.Add(dto.Name);
                }
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }

        private void LoadVirtualityFormationItems()
        {
            try
            {
                VirtualityFormationDto formationDto = new VirtualityFormationDto();
                List<VirtualityFormationDto> list = formationDto.GetDefaultList();

                foreach (VirtualityFormationDto dto in list)
                {
                    CombBMemberVF.Items.Add(dto.Name);
                }
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }

        private void LoadGuildItems()
        {
            try
            {
                GuildDao guildDao = new GuildDao();

                GuildDto[] guildDtos = guildDao.GetGuildDtos();

                CombBMemberGuild.Items.Add("");

                foreach (GuildDto guildDto in guildDtos)
                {
                    CombBMemberGuild.Items.Add(guildDto.Name);
                }
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }

        private bool IsFormationInputCheck()
        {
            bool isCheck = false;

            try
            {
                if (string.IsNullOrEmpty(TextFormationName.Text))
                {
                    throw new Exception("(必須)編成の名前を入力してください。");
                }

                if (string.IsNullOrEmpty(TextFormationSort.Text) ||
                    Regex.IsMatch(TextFormationSort.Text, "^[0-9]+$") == false)
                {
                    throw new Exception("(必須)編成の順序は半角数値で入力してください。");
                }

                if (!string.IsNullOrEmpty(TextSpeedRune1_1.Text) &&
                    Regex.IsMatch(TextSpeedRune1_1.Text, "^[0-9]+$") == false)
                {
                    throw new Exception("先鋒スピードルーン1は半角数値で入力してください。");
                }

                if (!string.IsNullOrEmpty(TextSpeedRune1_2.Text) &&
                    Regex.IsMatch(TextSpeedRune1_2.Text, "^[0-9]+$") == false)
                {
                    throw new Exception("先鋒スピードルーン2は半角数値で入力してください。");
                }

                if (!string.IsNullOrEmpty(TextSpeedRune1_3.Text) &&
                    Regex.IsMatch(TextSpeedRune1_3.Text, "^[0-9]+$") == false)
                {
                    throw new Exception("先鋒スピードルーン3は半角数値で入力してください。");
                }

                if (!string.IsNullOrEmpty(TextPiercingRune1_1.Text) &&
                    Regex.IsMatch(TextPiercingRune1_1.Text, "^[0-9]+$") == false)
                {
                    throw new Exception("先鋒物魔貫通ルーン1は半角数値で入力してください。");
                }

                if (!string.IsNullOrEmpty(TextPiercingRune1_2.Text) &&
                    Regex.IsMatch(TextPiercingRune1_2.Text, "^[0-9]+$") == false)
                {
                    throw new Exception("先鋒物魔貫通ルーン2は半角数値で入力してください。");
                }

                if (!string.IsNullOrEmpty(TextPiercingRune1_3.Text) &&
                    Regex.IsMatch(TextPiercingRune1_3.Text, "^[0-9]+$") == false)
                {
                    throw new Exception("先鋒物魔貫通ルーン3は半角数値で入力してください。");
                }

                if (!string.IsNullOrEmpty(TextSpeedRune2_1.Text) &&
                    Regex.IsMatch(TextSpeedRune2_1.Text, "^[0-9]+$") == false)
                {
                    throw new Exception("次鋒スピードルーン1は半角数値で入力してください。");
                }

                if (!string.IsNullOrEmpty(TextSpeedRune2_2.Text) &&
                    Regex.IsMatch(TextSpeedRune2_2.Text, "^[0-9]+$") == false)
                {
                    throw new Exception("次鋒スピードルーン2は半角数値で入力してください。");
                }

                if (!string.IsNullOrEmpty(TextSpeedRune2_3.Text) &&
                    Regex.IsMatch(TextSpeedRune2_3.Text, "^[0-9]+$") == false)
                {
                    throw new Exception("次鋒スピードルーン3は半角数値で入力してください。");
                }

                if (!string.IsNullOrEmpty(TextPiercingRune2_1.Text) &&
                    Regex.IsMatch(TextPiercingRune2_1.Text, "^[0-9]+$") == false)
                {
                    throw new Exception("次鋒物魔貫通ルーン1は半角数値で入力してください。");
                }

                if (!string.IsNullOrEmpty(TextPiercingRune2_2.Text) &&
                    Regex.IsMatch(TextPiercingRune2_2.Text, "^[0-9]+$") == false)
                {
                    throw new Exception("次鋒物魔貫通ルーン2は半角数値で入力してください。");
                }

                if (!string.IsNullOrEmpty(TextPiercingRune2_3.Text) &&
                    Regex.IsMatch(TextPiercingRune2_3.Text, "^[0-9]+$") == false)
                {
                    throw new Exception("次鋒物魔貫通ルーン3は半角数値で入力してください。");
                }

                if (!string.IsNullOrEmpty(TextSpeedRune3_1.Text) &&
                    Regex.IsMatch(TextSpeedRune3_1.Text, "^[0-9]+$") == false)
                {
                    throw new Exception("中堅スピードルーン1は半角数値で入力してください。");
                }

                if (!string.IsNullOrEmpty(TextSpeedRune3_2.Text) &&
                    Regex.IsMatch(TextSpeedRune3_2.Text, "^[0-9]+$") == false)
                {
                    throw new Exception("中堅スピードルーン2は半角数値で入力してください。");
                }

                if (!string.IsNullOrEmpty(TextSpeedRune3_3.Text) &&
                    Regex.IsMatch(TextSpeedRune3_3.Text, "^[0-9]+$") == false)
                {
                    throw new Exception("中堅スピードルーン3は半角数値で入力してください。");
                }

                if (!string.IsNullOrEmpty(TextPiercingRune3_1.Text) &&
                    Regex.IsMatch(TextPiercingRune3_1.Text, "^[0-9]+$") == false)
                {
                    throw new Exception("中堅物魔貫通ルーン1は半角数値で入力してください。");
                }

                if (!string.IsNullOrEmpty(TextPiercingRune3_2.Text) &&
                    Regex.IsMatch(TextPiercingRune3_2.Text, "^[0-9]+$") == false)
                {
                    throw new Exception("中堅物魔貫通ルーン2は半角数値で入力してください。");
                }

                if (!string.IsNullOrEmpty(TextPiercingRune3_3.Text) &&
                    Regex.IsMatch(TextPiercingRune3_3.Text, "^[0-9]+$") == false)
                {
                    throw new Exception("中堅物魔貫通ルーン3は半角数値で入力してください。");
                }

                if (!string.IsNullOrEmpty(TextSpeedRune4_1.Text) &&
                    Regex.IsMatch(TextSpeedRune4_1.Text, "^[0-9]+$") == false)
                {
                    throw new Exception("副将スピードルーン1は半角数値で入力してください。");
                }

                if (!string.IsNullOrEmpty(TextSpeedRune4_2.Text) &&
                    Regex.IsMatch(TextSpeedRune4_2.Text, "^[0-9]+$") == false)
                {
                    throw new Exception("副将スピードルーン2は半角数値で入力してください。");
                }

                if (!string.IsNullOrEmpty(TextSpeedRune4_3.Text) &&
                    Regex.IsMatch(TextSpeedRune4_3.Text, "^[0-9]+$") == false)
                {
                    throw new Exception("副将スピードルーン3は半角数値で入力してください。");
                }

                if (!string.IsNullOrEmpty(TextPiercingRune4_1.Text) &&
                    Regex.IsMatch(TextPiercingRune4_1.Text, "^[0-9]+$") == false)
                {
                    throw new Exception("副将物魔貫通ルーン1は半角数値で入力してください。");
                }

                if (!string.IsNullOrEmpty(TextPiercingRune4_2.Text) &&
                    Regex.IsMatch(TextPiercingRune4_2.Text, "^[0-9]+$") == false)
                {
                    throw new Exception("副将物魔貫通ルーン2は半角数値で入力してください。");
                }

                if (!string.IsNullOrEmpty(TextPiercingRune4_3.Text) &&
                    Regex.IsMatch(TextPiercingRune4_3.Text, "^[0-9]+$") == false)
                {
                    throw new Exception("副将物魔貫通ルーン3は半角数値で入力してください。");
                }

                if (!string.IsNullOrEmpty(TextSpeedRune5_1.Text) &&
                    Regex.IsMatch(TextSpeedRune5_1.Text, "^[0-9]+$") == false)
                {
                    throw new Exception("大将スピードルーン1は半角数値で入力してください。");
                }

                if (!string.IsNullOrEmpty(TextSpeedRune5_2.Text) &&
                    Regex.IsMatch(TextSpeedRune5_2.Text, "^[0-9]+$") == false)
                {
                    throw new Exception("大将スピードルーン2は半角数値で入力してください。");
                }

                if (!string.IsNullOrEmpty(TextSpeedRune5_3.Text) &&
                    Regex.IsMatch(TextSpeedRune5_3.Text, "^[0-9]+$") == false)
                {
                    throw new Exception("大将スピードルーン3は半角数値で入力してください。");
                }

                if (!string.IsNullOrEmpty(TextPiercingRune5_1.Text) &&
                    Regex.IsMatch(TextPiercingRune5_1.Text, "^[0-9]+$") == false)
                {
                    throw new Exception("大将物魔貫通ルーン1は半角数値で入力してください。");
                }

                if (!string.IsNullOrEmpty(TextPiercingRune5_2.Text) &&
                    Regex.IsMatch(TextPiercingRune5_2.Text, "^[0-9]+$") == false)
                {
                    throw new Exception("大将物魔貫通ルーン2は半角数値で入力してください。");
                }

                if (!string.IsNullOrEmpty(TextPiercingRune5_3.Text) &&
                    Regex.IsMatch(TextPiercingRune5_3.Text, "^[0-9]+$") == false)
                {
                    throw new Exception("大将物魔貫通ルーン3は半角数値で入力してください。");
                }
            }
            catch (Exception e)
            {
                throw e;
            }

            return isCheck;
        }

        private void UpdateFormation()
        {
            try
            {
                if (!isChange)
                {
                    return;
                }

                IsFormationInputCheck();

                FormationDto formation = formations[pos];

                formation.Name = TextFormationName.Text;
                formation.SortNo = int.Parse(TextFormationSort.Text);
                formation.IsSubParty = CheckSubParty.IsChecked.Value;
                formation.IsDebuff = CheckDebuff.IsChecked.Value;
                formation.Description = TextFormationDescription.Text;
                formation.IsGVS = CheckGVS.IsChecked.Value;

                UpdateCharacters();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void UpdateCharacters()
        {
            try
            {
                FormationDto formationDto = formations[pos];
                CharacterDto[] characterDtos = formationDto.Characters;

                if (null == characterDtos || characterDtos.Length == 0)
                {
                    return;
                }

                int sortNo = 1;

                foreach (CharacterDto dto in characterDtos)
                {
                    dto.FormationId = formationDto.Id;
                    dto.PlayerId = formationDto.PlayerId;
                    dto.SortNo = sortNo;

                    sortNo++;
                }

                int charaCnt = characterDtos.Length;

                if (charaCnt > 0)
                {
                    characterDtos[0].SpeedRune1 = int.Parse(TextSpeedRune1_1.Text);
                    characterDtos[0].SpeedRune2 = int.Parse(TextSpeedRune1_2.Text);
                    characterDtos[0].SpeedRune3 = int.Parse(TextSpeedRune1_3.Text);
                    characterDtos[0].PiercingRune1 = int.Parse(TextPiercingRune1_1.Text);
                    characterDtos[0].PiercingRune2 = int.Parse(TextPiercingRune1_2.Text);
                    characterDtos[0].PiercingRune3 = int.Parse(TextPiercingRune1_3.Text);
                }

                if (charaCnt > 1)
                {
                    characterDtos[1].SpeedRune1 = int.Parse(TextSpeedRune2_1.Text);
                    characterDtos[1].SpeedRune2 = int.Parse(TextSpeedRune2_2.Text);
                    characterDtos[1].SpeedRune3 = int.Parse(TextSpeedRune2_3.Text);
                    characterDtos[1].PiercingRune1 = int.Parse(TextPiercingRune2_1.Text);
                    characterDtos[1].PiercingRune2 = int.Parse(TextPiercingRune2_2.Text);
                    characterDtos[1].PiercingRune3 = int.Parse(TextPiercingRune2_3.Text);
                }

                if (charaCnt > 2)
                {
                    characterDtos[2].SpeedRune1 = int.Parse(TextSpeedRune3_1.Text);
                    characterDtos[2].SpeedRune2 = int.Parse(TextSpeedRune3_2.Text);
                    characterDtos[2].SpeedRune3 = int.Parse(TextSpeedRune3_3.Text);
                    characterDtos[2].PiercingRune1 = int.Parse(TextPiercingRune3_1.Text);
                    characterDtos[2].PiercingRune2 = int.Parse(TextPiercingRune3_2.Text);
                    characterDtos[2].PiercingRune3 = int.Parse(TextPiercingRune3_3.Text);
                }

                if (charaCnt > 3)
                {
                    characterDtos[3].SpeedRune1 = int.Parse(TextSpeedRune4_1.Text);
                    characterDtos[3].SpeedRune2 = int.Parse(TextSpeedRune4_2.Text);
                    characterDtos[3].SpeedRune3 = int.Parse(TextSpeedRune4_3.Text);
                    characterDtos[3].PiercingRune1 = int.Parse(TextPiercingRune4_1.Text);
                    characterDtos[3].PiercingRune2 = int.Parse(TextPiercingRune4_2.Text);
                    characterDtos[3].PiercingRune3 = int.Parse(TextPiercingRune4_3.Text);
                }

                if (charaCnt > 4)
                {
                    characterDtos[4].SpeedRune1 = int.Parse(TextSpeedRune5_1.Text);
                    characterDtos[4].SpeedRune2 = int.Parse(TextSpeedRune5_2.Text);
                    characterDtos[4].SpeedRune3 = int.Parse(TextSpeedRune5_3.Text);
                    characterDtos[4].PiercingRune1 = int.Parse(TextPiercingRune5_1.Text);
                    characterDtos[4].PiercingRune2 = int.Parse(TextPiercingRune5_2.Text);
                    characterDtos[4].PiercingRune3 = int.Parse(TextPiercingRune5_3.Text);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                try
                {
                    UpdateFormation();
                }
                catch (Exception ie)
                {
                    MessageBox.Show(ie.Message);
                    return;
                }

                nowPage--;
                LoadFormation();
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }

        private void BtnNext_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                try
                {
                    UpdateFormation();
                }
                catch (Exception ie)
                {
                    MessageBox.Show(ie.Message);
                    return;
                }

                nowPage++;
                LoadFormation();
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                member.IsOpenWindow = false;
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }

        public void AddFormation()
        {
            try
            {
                if (null == AddFormationList || AddFormationList.Count == 0)
                {
                    return;
                }

                PlayerDao playerDao = new PlayerDao(false);
                FormationDao formationDao = new FormationDao();
                GuildDao guildDao = new GuildDao();

                int guildId = guildDao.GetGuildId(CombBMemberGuild.SelectedItem.ToString());
                long playerId = long.Parse(TextMemberId.Text);
                int guildPlayerNo = playerDao.GetNo(guildId, playerId);
                int sortNo = formationDao.NextSortNo(guildId, playerId);

                foreach (FormationDto dto in AddFormationList)
                {
                    int cnt = member.Formations.Length + 1;
                    int fPos = cnt - 1;

                    formations = member.Formations;
                    Array.Resize(ref formations, cnt);

                    formations[fPos] = new FormationDto()
                    {
                        Id = Common.Common.GetFormationId(guildId, guildPlayerNo, cnt),
                        PlayerId = playerId,
                        SortNo = sortNo,
                        Name = "仮編成中(画像取込)",
                        IsSubParty = false,
                        IsDebuff = false,
                        Description = "",
                        Characters = dto.Characters
                    };

                    nowPage = cnt;
                    formationsCnt++;
                    sortNo++;
                    member.Formations = formations;
                }

                LoadFormation();
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }

        private void BtnNewFormation_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int cnt = member.Formations.Length + 1;
                int fPos = cnt - 1;

                formations = member.Formations;
                Array.Resize(ref formations, cnt);

                PlayerDao playerDao = new PlayerDao(false);
                FormationDao formationDao = new FormationDao();
                GuildDao guildDao = new GuildDao();

                int guildId = guildDao.GetGuildId(CombBMemberGuild.SelectedItem.ToString());
                long playerId = long.Parse(TextMemberId.Text);
                int guildPlayerNo = playerDao.GetNo(guildId, playerId);
                
                formations[fPos] = new FormationDto()
                {
                    Id = Common.Common.GetFormationId(guildId, guildPlayerNo, cnt),
                    PlayerId = playerId,
                    SortNo = formationDao.NextSortNo(guildId, playerId),
                    Name = "仮編成中",
                    IsSubParty = false,
                    IsDebuff = false,
                    Description = ""
                };

                nowPage = cnt;
                formationsCnt++;
                member.Formations = formations;

                LoadFormation();
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }

        private bool IsInputCheck()
        {
            bool isCheck = false;

            try
            {
                if (string.IsNullOrEmpty(TextMemberId.Text) ||
                    TextMemberId.Text.Length != 12 ||
                    Regex.IsMatch(TextMemberId.Text, "^[0-9]+$") == false)
                {
                    throw new Exception("(必須)IDはメメントモリID(12桁半角数値)で入力してください。");
                }

                if (isNew)
                {
                    PlayerDao playerDao = new PlayerDao();

                    if (playerDao.IsId(long.Parse(TextMemberId.Text)))
                    {
                        throw new Exception("IDは既に登録済みです。");
                    }
                }

                if (string.IsNullOrEmpty(TextMemberName.Text) ||
                    TextMemberName.Text.Length > 10)
                {
                    throw new Exception("(必須)名前は最大10桁で入力してください。");
                }

                if (string.IsNullOrEmpty(TextMemberWorld.Text) ||
                    Regex.IsMatch(TextMemberWorld.Text, "^[0-9]+$") == false)
                {
                    throw new Exception("(必須)ワールドは半角数値で入力してください。");
                }

                if (string.IsNullOrEmpty(TextMemberNo.Text) ||
                    Regex.IsMatch(TextMemberNo.Text, "^[0-9]+$") == false)
                {
                    throw new Exception("(必須)Noは半角数値で入力してください。");
                }

                if (!string.IsNullOrEmpty(TextMemberVFCNum.Text) &&
                    Regex.IsMatch(TextMemberVFCNum.Text, "^[0-9]+$") == false)
                {
                    throw new Exception("仮想編成キャラクター数は半角数値で入力してください。(必須ではありません)");
                }

                if (!string.IsNullOrEmpty(TextMemberDiscordUserId.Text) &&
                    Regex.IsMatch(TextMemberDiscordUserId.Text, "^[0-9]+$") == false)
                {
                    throw new Exception("DiscordユーザーIDは半角整数で入力してください。(必須ではありません)");
                }

                if (!string.IsNullOrEmpty(TextMemberDiscordUserChannelId.Text) &&
                    Regex.IsMatch(TextMemberDiscordUserChannelId.Text, "^[0-9]+$") == false)
                {
                    throw new Exception("Discordユーザー専用チャンネルIDは半角整数で入力してください。(必須ではありません)");
                }

                if (isNew)
                {
                    DiscordDao discordDao = new DiscordDao();

                    if (!string.IsNullOrEmpty(TextMemberDiscordUserId.Text) &&
                        discordDao.IsId(ulong.Parse(TextMemberDiscordUserId.Text)))
                    {
                        throw new Exception("DiscordユーザーIDは既に登録済みです。");
                    }
                }
            }
            catch (Exception e)
            {
                throw e;
            }

            return isCheck;
        }

        private void UpdateMember()
        {
            try
            {
                GuildDao guildDao = new GuildDao();

                int guildId = guildDao.GetGuildId(CombBMemberGuild.SelectedItem.ToString());

                member.Id = long.Parse(TextMemberId.Text);
                member.No = int.Parse(TextMemberNo.Text);
                member.World = int.Parse(TextMemberWorld.Text);
                member.GuildId = guildId;
                member.GuildName = CombBMemberGuild.SelectedItem.ToString();
                member.Name = TextMemberName.Text;
                member.MainFormation = TextMemberMainFormation.Text;
                member.IsVC = CheckVC.IsChecked.Value;
                member.VirtualityFormation = CombBMemberVF.SelectedIndex;
                member.VFCNum = int.Parse(TextMemberVFCNum.Text);

                GBGroupDto[] gBGroupDtos = BaseGBGroup.GetBaseGBGroup();

                gBGroupDtos = gBGroupDtos.Concat(BaseGBGroup.GetBaseGBGroup(1)).ToArray();

                GBGroupDto gBGroupDto = gBGroupDtos.Where(x => x.Name.Equals(CombBMemberGBGroup.Text)).FirstOrDefault();

                if (null != gBGroupDto)
                {
                    member.GBGroupId = gBGroupDto.Id;
                }

                if (member.UId == 0)
                {
                    // 新規
                    PlayerDao playerDao = new PlayerDao(false, false);

                    PlayerDto[] playerDtos = playerDao.GetPlayerDtos(guildId);

                    if (null == playerDtos || playerDtos.Length == 0)
                    {
                        member.UId = 1;
                    }
                    else
                    {
                        PlayerDto playerDto = playerDtos.OrderByDescending(x => x.UId).FirstOrDefault();
                        member.UId = playerDto.UId + 1;
                    }
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        private void BtnUpdate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 入力チェック
                try
                {
                    IsInputCheck();
                }
                catch (Exception ie)
                {
                    MessageBox.Show(ie.Message);
                    return;
                }

                UpdateMember();

                if (!isNew && isChange)
                {
                    UpdateFormation();
                }

                GuildDao guildDao = new GuildDao();
                PlayerDao playerDao = new PlayerDao(false, false);
                FormationDao formationDao = new FormationDao();
                CharacterDao characterDao = new CharacterDao();
                DiscordDao discordDao = new DiscordDao();

                int guildId = guildDao.GetGuildId(CombBMemberGuild.SelectedItem.ToString());
                long playerId = long.Parse(TextMemberId.Text);
                ulong.TryParse(TextMemberDiscordUserId.Text, out ulong discordId);
                ulong.TryParse(TextMemberDiscordUserChannelId.Text, out ulong discordChannelId);

                List<PlayerDto> playerList;
                List<FormationDto> formationList;
                List<CharacterDto> characterList;
                List<DiscordDto> discordList = discordDao.GetDiscordDtos().ToList();

                int permission = 0;

                if (CheckDiscordRCP.IsChecked.Value)
                {
                    permission = 1;
                }

                // 新規登録
                if (isNew)
                {
                    playerList = playerDao.GetPlayerDtos(guildId).ToList();
                    playerList?.Add(member);

                    DiscordDto discordDto = new DiscordDto()
                    {
                        Id = discordId,
                        UserName = TextMemberName.Text,
                        MMId = playerId,
                        RemoteControlPermission = permission,
                        DedicatedChannelId = discordChannelId
                    };

                    discordList.Add(discordDto);
                }
                else
                {
                    playerList = memberList.Where(x => x.GuildId == guildId).ToList();

                    DiscordDto discordDto = discordList.Where(x => x.MMId == playerId).FirstOrDefault();

                    if (null != discordDto)
                    {
                        discordDto.Id = discordId;
                        discordDto.DedicatedChannelId = discordChannelId;
                        discordDto.RemoteControlPermission = permission;
                    }

                    FormationDto[] fDtos = formationDao.GetFormationDtos(guildId).Where(x => x.PlayerId != playerId).ToArray();
                    formationList = fDtos.Concat(formations).ToList();

                    CharacterDto[] cDtos = characterDao.GetCharacterDtos(guildId).Where(x => x.PlayerId != playerId).ToArray();

                    foreach (FormationDto fdto in member.Formations)
                    {
                        cDtos = cDtos.Concat(fdto.Characters).ToArray();
                    }

                    characterList = cDtos.ToList();

                    formationDao.SaveFormationDto(formationList, guildId, CheckBackup.IsChecked.Value);
                    characterDao.SaveCharacterDto(characterList, guildId, CheckBackup.IsChecked.Value);
                }

                playerDao.SavePlayerDto(playerList, guildId, CheckBackup.IsChecked.Value);
                discordDao.SaveDiscordDto(discordList, CheckBackup.IsChecked.Value);

                string btnName = "更新";

                if (isNew)
                {
                    btnName = "登録";
                }

                MessageBoxResult messageBoxResult = MessageBox.Show(btnName + "が完了しました。画面を閉じますか?",
                                                                    btnName, (MessageBoxButton)MessageBoxButtons.YesNo);

                if (messageBoxResult == MessageBoxResult.Yes)
                {
                    Close();
                }
                else
                {
                    if (isNew)
                    {
                        BtnUpdate.Content = "更新";
                        isNew = false;
                        LoadMemberInfo();
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MessageBoxResult messageBoxResult = MessageBox.Show("本当に削除してもよろしいでしょうか?",
                                                                    "削除(論理)", (MessageBoxButton)MessageBoxButtons.YesNo);

                if (messageBoxResult == MessageBoxResult.No)
                {
                    return;
                }

                member.IsDelete = true;

                GuildDao guildDao = new GuildDao();
                PlayerDao playerDao = new PlayerDao(false);

                int guildId = guildDao.GetGuildId(CombBMemberGuild.SelectedItem.ToString());

                List<PlayerDto> playerList = memberList.Where(x => x.GuildId == guildId).ToList();

                playerDao.SavePlayerDto(playerList, guildId, CheckBackup.IsChecked.Value);

                messageBoxResult = MessageBox.Show("削除が完了しました。",
                                                   "削除(論理)", (MessageBoxButton)MessageBoxButtons.OK);

                if (messageBoxResult == MessageBoxResult.OK)
                {
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
