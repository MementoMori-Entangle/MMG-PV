using log4net;
using MMG.Common;
using MMG.Dto;
using System;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

namespace MMG
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().Name);

        private bool isDiscord = false;

        private Discord.Program discordP;

        public MainWindow()
        {
            InitializeComponent();

            // 共通初期化
            Common.Common.FormationCharaImagePath = AppDomain.CurrentDomain.BaseDirectory + Properties.Settings.Default["FormationCharaImagePath"];
            Common.Common.FormationTokeNekoImagePath = AppDomain.CurrentDomain.BaseDirectory + Properties.Settings.Default["FormationTokeNekoImagePath"];
            Common.Common.FormationImageExtension = Properties.Settings.Default["FormationImageExtension"].ToString();
            Common.Common.FormationImageNothing = Properties.Settings.Default["FormationImageNothing"].ToString();
            Common.Common.GuildImagePath = AppDomain.CurrentDomain.BaseDirectory + Properties.Settings.Default["GuildImagePath"].ToString();

            discordP = new Discord.Program
            {
                ListBox = new ListBox()
            };

            if (Common.Common.IsDiscordAvailable())
            {
                BtnDiscord.IsEnabled = true;
            }
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // ログイン処理
                Auth auth = new Auth();

                AuthDto dto = new AuthDto()
                {
                    LoginId = TextLoginId.Text,
                    Password = PBPassword.Password
                };

                if (!AssemblyState.IsDebug)
                {
                    if (!auth.IsLogin(dto.Password))
                    {
                        MessageBox.Show("ログイン認証ができませんでした。");
                        return;
                    }
                }

                if (isDiscord)
                {
                    discordP.Dispose();
                }

                MenuWindow menuWin = new MenuWindow(this, dto);
                menuWin.Show();
                Hide();
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Close();
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }

        private void BtnDiscord_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (isDiscord)
                {
                    discordP.Dispose();
                    BtnDiscord.Content = "Discord連携 OFF";
                    isDiscord = false;
                }
                else
                {
                    if (string.IsNullOrEmpty(TextLoginId.Text))
                    {
                        MessageBox.Show("Discord連携するには、ログインIDにDiscordのユーザーIDを入力してください。");
                        return;
                    }

                    discordP.MainAsync().GetAwaiter().GetResult();
                    BtnDiscord.Content = "Discord連携 ON";
                    isDiscord = true;
                }
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
                if (isDiscord)
                {
                    discordP.Dispose();
                }
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }
    }

    internal static class AssemblyState
    {
        public const bool IsDebug =
#if DEBUG
        true;
#else
   false;
#endif
    }
}
