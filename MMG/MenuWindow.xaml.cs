using log4net;
using MMG.Dto;
using System;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace MMG
{
    /// <summary>
    /// MenuWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MenuWindow : Window
    {
        private readonly MainWindow mainWin;

        private readonly string imgTokeNekoPath = Common.Common.FormationTokeNekoImagePath;
        private readonly string imgExt = Common.Common.FormationImageExtension;
        private readonly long cautionPMTSize = 1024 * 1000 * Common.Common.MATCH_TEMP_DIR_MAX_SIZE;

        private readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().Name);

        private Image[] imgNekos = null;
        private AuthDto authDto;

        private TextBlock mvpTB;

        public MenuWindow(MainWindow mainWin, AuthDto authDto)
        {
            this.mainWin = mainWin;
            this.authDto = authDto;

            InitializeComponent();

            DispatcherTimer timer = new DispatcherTimer();
            timer.Tick += TimerTick;
            timer.Interval = new TimeSpan(0, 0, 0, 0, 10);
            timer.Start();

            LoadCanvasNeko();

            string pmTFI = string.Empty;

            if (false == Directory.Exists(Common.Common.MATCH_TEMP_DIR_PATH))
            {
                Directory.CreateDirectory(Common.Common.MATCH_TEMP_DIR_PATH);
            }

            DirectoryInfo di = new DirectoryInfo(Common.Common.MATCH_TEMP_DIR_PATH);
            long dirSize = Common.Common.GetDirectorySize(di);

            if (cautionPMTSize < dirSize)
            {
                int mFileSize = (int)dirSize / (1024 * 1000);

                pmTFI = "キャラクターマッチング一時ファイル" + Environment.NewLine;
                pmTFI += "総ファイル数 : "
                      + Directory.GetFiles(Common.Common.MATCH_TEMP_DIR_PATH, "*", SearchOption.AllDirectories).Length
                      + Environment.NewLine;
                pmTFI += "総容量 : " + string.Format("{0}MByte", mFileSize);
            }

            TextBPMTempFileInfo.Text = pmTFI;
        }

        private void LoadCanvasNeko()
        {
            try
            {
                imgNekos = new Image[]
                {
                    new Image
                    {
                        Source = new BitmapImage(new Uri(imgTokeNekoPath + "1" + imgExt)),
                        Width = 25,
                        Height = 25
                    },
                    new Image
                    {
                        Source = new BitmapImage(new Uri(imgTokeNekoPath + "2" + imgExt)),
                        Width = 25,
                        Height = 25
                    },
                    new Image
                    {
                        Source = new BitmapImage(new Uri(imgTokeNekoPath + "3" + imgExt)),
                        Width = 25,
                        Height = 25
                    },
                    new Image
                    {
                        Source = new BitmapImage(new Uri(imgTokeNekoPath + "4" + imgExt)),
                        Width = 25,
                        Height = 25
                    },
                    new Image
                    {
                        Source = new BitmapImage(new Uri(imgTokeNekoPath + "5" + imgExt)),
                        Width = 25,
                        Height = 25
                    },
                    new Image
                    {
                        Source = new BitmapImage(new Uri(imgTokeNekoPath + "6" + imgExt)),
                        Width = 25,
                        Height = 25
                    },
                    new Image
                    {
                        Source = new BitmapImage(new Uri(imgTokeNekoPath + "21" + imgExt)),
                        Width = 50,
                        Height = 50
                    }
                };

                foreach (Image img in imgNekos)
                {
                    CanvasNeko.Children.Add(img);
                }

                Image image = new Image
                {
                    Source = new BitmapImage(new Uri(imgTokeNekoPath + "7" + imgExt)),
                    Width = 25,
                    Height = 25,
                    Margin = new Thickness(100, 120, 0, 0)
                };

                CanvasNeko.Children.Add(image);

                image = new Image
                {
                    Source = new BitmapImage(new Uri(imgTokeNekoPath + "8" + imgExt)),
                    Width = 25,
                    Height = 25,
                    Margin = new Thickness(205, 100, 0, 0)
                };

                CanvasNeko.Children.Add(image);

                image = new Image
                {
                    Source = new BitmapImage(new Uri(imgTokeNekoPath + "21" + imgExt)),
                    Width = 50,
                    Height = 50,
                    Margin = new Thickness(230, 80, 0, 0)
                };

                CanvasNeko.Children.Add(image);

                mvpTB = new TextBlock()
                {
                    Text = "MVP !!",
                    Margin = new Thickness(230, 70, 0, 0),
                    Foreground = new SolidColorBrush(Color.FromRgb(255, 0, 0)),
                    Visibility = Visibility.Hidden
                };

                CanvasNeko.Children.Add(mvpTB);

                TextBlock garuruTB = new TextBlock()
                {
                    Text = "ガルルルゥ",
                    Margin = new Thickness(120, 110, 0, 0),
                    Foreground = new SolidColorBrush(Color.FromRgb(255, 0, 0))
                };

                CanvasNeko.Children.Add(garuruTB);

                garuruTB = new TextBlock()
                {
                    Text = "ガルルゥ",
                    Margin = new Thickness(60, 120, 0, 0),
                    Foreground = new SolidColorBrush(Color.FromRgb(255, 0, 0))
                };

                CanvasNeko.Children.Add(garuruTB);
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }

        void TimerTick(object sender, EventArgs e)
        {
            TextTimeBlock.Text = DateTime.Now.ToString("HH:mm:ss.fff");

            int len = imgNekos.Length;

            for (int i = 0; i < len; i++)
            {
                imgNekos[i].Margin = new Thickness((25 * i) + DateTime.Now.Hour + i, (DateTime.Now.Second * 2) - (6 * i), 0, 0);
            }

            if (DateTime.Now.Second % 2 == 0)
            {
                mvpTB.Visibility = Visibility.Visible;
            }
            else
            {
                mvpTB.Visibility = Visibility.Hidden;
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            try
            {
                // 終了するかログイン画面へ復帰するかどうしょうか? 設定で変更できるようにするか?

                // ログイン画面へ戻すパターン
                mainWin.Show();
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }

        }

        private void BtnGuildInfoManagement_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                GuildInfoManagement guildIM = new GuildInfoManagement(BtnGuildInfoManagement);
                guildIM.Show();
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }

        private void BtnPlayerVSPlayer_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                PlayerVSPlayer playerVSPlayer = new PlayerVSPlayer();
                playerVSPlayer.Show();
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }

        private void BtnGuildVSAdmin_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                GuildVSAdmin guildVSAdmin = new GuildVSAdmin(authDto);
                guildVSAdmin.Show();
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }

        private void BtnGuildVSGuild_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                GuildVSGuild guildVSGuild = new GuildVSGuild();
                guildVSGuild.Show();
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }

        private void BtnCharacterSpeed_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                CharacterSpeed characterSpeed = new CharacterSpeed();
                characterSpeed.Show();
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }

        private void BtnGuildVSMap_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                GuildSelection guildSelection = new GuildSelection();
                guildSelection.ShowDialog();

                string[] size = guildSelection.SelectedGuildMapSize.Split('*');
                double width = double.Parse(size[0]);
                double height = double.Parse(size[1]);

                GuildVSMap guildVSMap = new GuildVSMap(guildSelection.SelectedGuildName, width, height);
                guildVSMap.Show();
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }

        private void BtnPlayerManagement_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                PlayerManagement playerManagement = new PlayerManagement();
                playerManagement.Show();
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }

        private void BtnCharacterImageRead_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                CharacterImageReader characterImageReader = new CharacterImageReader();
                characterImageReader.Show();
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }

        private void BtnGuildVSFormation_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                GuildVSFormation guildVSFormation = new GuildVSFormation();
                guildVSFormation.Show();
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }

        private void BtnVSDataManagement_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                FVSFManagement fVSFManagement = new FVSFManagement();
                fVSFManagement.Show();
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }
    }
}
