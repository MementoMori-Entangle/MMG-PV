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
using System.Windows.Media.Imaging;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;
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
        private PlayerDto member;
        private List<PlayerDto> memberList;
        private FormationDto[] formations;
        private readonly string imgPath = Common.Common.FormationCharaImagePath;
        private readonly string imgExt = Common.Common.FormationImageExtension;

        private readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().Name);

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

                    TextMemberId.IsEnabled = true;
                    TextMemberWorld.IsEnabled = true;
                    CombBMemberGuild.IsEnabled = true;
                }
                else
                {
                    TextMemberId.Text = member.Id.ToString();
                    TextMemberNo.Text = member.No.ToString();
                    TextMemberWorld.Text = member.World.ToString();

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

                    gBGroupDtos = gBGroupDtos.Concat(BaseGBGroup.GetBaseGBGroup(1)).ToArray();

                    foreach (GBGroupDto dto in gBGroupDtos)
                    {
                        if (dto.Id == member.GBGroupId)
                        {
                            CombBMemberGBGroup.SelectedItem = dto.Name;
                            break;
                        }
                    }

                    Dictionary<long, ulong> mmUserIdUserIdDic = BaseDiscordUser.GetMMUserIdUserIdDic();

                    if (mmUserIdUserIdDic.ContainsKey(member.Id))
                    {
                        TextMemberDiscordUserId.Text = mmUserIdUserIdDic[member.Id].ToString();
                    }
                    else
                    {
                        TextMemberDiscordUserId.Text = string.Empty;
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
                }
                else
                {
                    BtnNewFormation.Visibility = Visibility.Hidden;
                    BtnUpdate.Visibility = Visibility.Hidden;
                    BtnDelete.Visibility = Visibility.Hidden;
                    CheckBackup.Visibility = Visibility.Hidden;
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
                int pos = nowPage - 1;

                FormationDto formation = formations[pos];

                if (isNew)
                {

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

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            try
            {
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

        private void BtnNewFormation_Click(object sender, RoutedEventArgs e)
        {
            try
            {

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
                    throw new Exception("DiscordIdは半角数値で入力してください。(必須ではありません)");
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
                    PlayerDao playerDao = new PlayerDao(false);

                    PlayerDto[] playerDtos = playerDao.GetPlayerDtos(guildId);

                    if (null == playerDtos)
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

                GuildDao guildDao = new GuildDao();
                PlayerDao playerDao = new PlayerDao(false);

                int guildId = guildDao.GetGuildId(CombBMemberGuild.SelectedItem.ToString());

                List<PlayerDto> playerList;

                // 新規登録
                if (null == memberList)
                {
                    playerList = playerDao.GetPlayerDtos(guildId).ToList();
                    playerList?.Add(member);
                }
                else
                {
                    playerList = memberList.Where(x => x.GuildId == guildId).ToList();
                }

                playerDao.SavePlayerDto(playerList, guildId, CheckBackup.IsChecked.Value);

                MessageBoxResult messageBoxResult = MessageBox.Show("更新が完了しました。画面を閉じますか?",
                                                                    "更新", (MessageBoxButton)MessageBoxButtons.YesNo);

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

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            try
            {

            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }
    }
}
