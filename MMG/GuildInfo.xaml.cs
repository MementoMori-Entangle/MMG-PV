using log4net;
using MMG.Dao;
using MMG.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Forms;
using MessageBox = System.Windows.MessageBox;

namespace MMG
{
    /// <summary>
    /// GuildInfo.xaml の相互作用ロジック
    /// </summary>
    public partial class GuildInfo : Window
    {
        private readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().Name);

        private bool isChange;
        private bool isNew;

        private GuildDto guildDto;
        private List<GuildDto> guildList;

        public GuildInfo(GuildDto guildDto, List<GuildDto> guildList, bool isChange = false, bool isNew = false)
        {
            InitializeComponent();

            this.guildDto = guildDto;
            this.guildList = guildList;
            this.isChange = isChange;
            this.isNew = isNew;

            if (isNew)
            {
                if (null == guildDto)
                {
                    this.guildDto = new GuildDto();
                }

                BtnUpdate.Content = "登録";
            }

            LoadGuildInfo();
        }

        private void LoadGuildInfo()
        {
            try
            {
                if (isNew)
                {
                    LabId.Content = string.Empty;
                    TextName.Text = string.Empty;
                    TextWorld.Text = string.Empty;
                    TextNo.Text = string.Empty;
                    TextRemarks.Text = string.Empty;
                }
                else
                {
                    LabId.Content = guildDto.Id;
                    TextName.Text = guildDto.Name;
                    TextWorld.Text = guildDto.World.ToString();
                    TextNo.Text = guildDto.No.ToString();
                    TextRemarks.Text = guildDto.Remarks;
                }

                if (isChange)
                {
                    if (isNew)
                    {
                        BtnDelete.Visibility = Visibility.Hidden;
                    }
                }
                else
                {
                    BtnUpdate.Visibility = Visibility.Hidden;
                    CheckBackup.Visibility = Visibility.Hidden;
                }

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
                if (string.IsNullOrEmpty(TextName.Text) ||
                    TextName.Text.Length > 10)
                {
                    throw new Exception("(必須)名前は最大10桁で入力してください。");
                }

                if (string.IsNullOrEmpty(TextWorld.Text) ||
                    Regex.IsMatch(TextWorld.Text, "^[0-9]+$") == false)
                {
                    throw new Exception("(必須)ワールドは半角数値で入力してください。");
                }

                if (string.IsNullOrEmpty(TextNo.Text) ||
                    Regex.IsMatch(TextNo.Text, "^[0-9]+$") == false)
                {
                    throw new Exception("(必須)Noは半角数値で入力してください。");
                }
            }
            catch (Exception e)
            {
                throw e;
            }

            return isCheck;
        }

        private void UpdateGuildInfo()
        {
            try
            {
                string guildId = LabId.Content.ToString();

                guildDto.No = int.Parse(TextNo.Text);
                guildDto.World = int.Parse(TextWorld.Text);
                guildDto.Name = TextName.Text;
                guildDto.Remarks = TextRemarks.Text;

                if (string.IsNullOrEmpty(guildId))
                {
                    // 新規
                    GuildDao guildDao = new GuildDao(false);

                    GuildDto[] guildDtos = guildDao.GetGuildDtos();

                    if (null == guildDtos || guildDtos.Length == 0)
                    {
                        guildDto.Id = 1;
                    }
                    else
                    {
                        GuildDto dto = guildDtos.OrderByDescending(x => x.Id).FirstOrDefault();
                        guildDto.Id = dto.Id + 1;
                    }
                }
                else
                {
                    guildDto.Id = int.Parse(guildId);
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

                UpdateGuildInfo();

                GuildDao guildDao = new GuildDao();

                if (isNew)
                {
                    guildList?.Add(guildDto);
                }

                guildDao.SaveGuildDto(guildList, guildDto.Id, CheckBackup.IsChecked.Value);

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
                        LoadGuildInfo();
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

                guildDto.IsDelete = true;

                GuildDao guildDao = new GuildDao();

                guildDao.SaveGuildDto(guildList, 0, CheckBackup.IsChecked.Value);

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
