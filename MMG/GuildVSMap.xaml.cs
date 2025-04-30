using log4net;
using MMG.Common;
using MMG.Dao;
using MMG.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace MMG
{
    /// <summary>
    /// GuildVSMap.xaml の相互作用ロジック
    /// </summary>
    public partial class GuildVSMap : Window
    {
        private readonly string imgGuildPath = Common.Common.GuildImagePath;
        private readonly string imgExt = Common.Common.FormationImageExtension;

        private readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().Name);

        private readonly double BASE_IMAGE_WIDTH = 1240;
        private readonly double MAP_SCALE_SIZE = 2.0;
        private readonly double MAP_TEMPLE_LEFT_LEFT = 40;
        private readonly double MAP_TEMPLE_LEFT_TOP = 0;
        private readonly double MAP_TEMPLE_LEFT_RIGHT = 40;
        private readonly double MAP_TEMPLE_LEFT_BOTTOM = 40;
        private readonly double MAP_TEMPLE_CENTER_LEFT = 15;
        private readonly double MAP_TEMPLE_CENTER_TOP = 65;
        private readonly double MAP_TEMPLE_CENTER_RIGHT = 40;
        private readonly double MAP_TEMPLE_CENTER_BOTTOM = 40;
        private readonly double MAP_TEMPLE_RIGHT_LEFT = 70;
        private readonly double MAP_TEMPLE_RIGHT_TOP = 0;
        private readonly double MAP_TEMPLE_RIGHT_RIGHT = 40;
        private readonly double MAP_TEMPLE_RIGHT_BOTTOM = 40;

        private readonly double MAP_CASTLE_LEFT_LEFT = 40;
        private readonly double MAP_CASTLE_LEFT_TOP = 0;
        private readonly double MAP_CASTLE_LEFT_RIGHT = 40;
        private readonly double MAP_CASTLE_LEFT_BOTTOM = 40;
        private readonly double MAP_CASTLE_CENTER_LEFT = 10;
        private readonly double MAP_CASTLE_CENTER_TOP = 45;
        private readonly double MAP_CASTLE_CENTER_RIGHT = 40;
        private readonly double MAP_CASTLE_CENTER_BOTTOM = 40;
        private readonly double MAP_CASTLE_RIGHT_LEFT = 55;
        private readonly double MAP_CASTLE_RIGHT_TOP = 0;
        private readonly double MAP_CASTLE_RIGHT_RIGHT = 40;
        private readonly double MAP_CASTLE_RIGHT_BOTTOM = 40;

        private readonly double MAP_CHURCH_LEFT_LEFT = 40;
        private readonly double MAP_CHURCH_LEFT_TOP = 0;
        private readonly double MAP_CHURCH_LEFT_RIGHT = 40;
        private readonly double MAP_CHURCH_LEFT_BOTTOM = 40;
        private readonly double MAP_CHURCH_CENTER_LEFT = 8;
        private readonly double MAP_CHURCH_CENTER_TOP = 40;
        private readonly double MAP_CHURCH_CENTER_RIGHT = 40;
        private readonly double MAP_CHURCH_CENTER_BOTTOM = 40;
        private readonly double MAP_CHURCH_RIGHT_LEFT = 55;
        private readonly double MAP_CHURCH_RIGHT_TOP = 0;
        private readonly double MAP_CHURCH_RIGHT_RIGHT = 40;
        private readonly double MAP_CHURCH_RIGHT_BOTTOM = 40;

        private readonly string MAP_GUILD_VS_BASE = "base_guild_battle";
        private readonly string MAP_GROUND_VS_BASE = "base_ground_battle";
        private readonly string PARTS_BASE = "parts_base";
        private readonly string PARTS_RECONNAISSANCE = "parts_reconnaissance";
        private readonly string PARTS_PROCLAMATION = "parts_proclamation";
        private readonly string PARTS_ADVANCE = "parts_advance";
        private readonly string PARTS_DEFENSE = "parts_defense";

        private GBGroupDto[] gBGroupDtos;
        private List<Image> imgPartsList;
        private ComboBox combBMode;
        private TextBlock textBlockTime;
        private ComboBox combVSType;

        private string selectedGuildName;

        private double reductionScale;
        private double expansionScale;

        public GuildVSMap(string selectedGuildName, double width = 0, double height = 0)
        {
            InitializeComponent();

            CanvasGuildMap.Background = new ImageBrush(new BitmapImage(new Uri(imgGuildPath + MAP_GUILD_VS_BASE + imgExt)));

            if (width > 0)
            {
                Width = width;
            }

            if (height > 0)
            {
                Height = height;
            }

            reductionScale = Width / BASE_IMAGE_WIDTH;
            expansionScale = BASE_IMAGE_WIDTH / Width;

            this.selectedGuildName = selectedGuildName;
            imgPartsList = new List<Image>();
            gBGroupDtos = BaseGBGroup.GetBaseGBGroup();
            LoadItems();
        }

        private void LoadItems()
        {
            try
            {
                List<string> modeList = new List<string>
                {
                    "布告前",
                    "布告時",
                    "開戦中(進行)",
                    "開戦中(偵察)",
                    "開戦中(防衛)",
                    "開戦終(偵察)",
                    "開戦終(防衛)"
                };

                List<string> vsTypeList = new List<string>
                {
                    "ギルドバトル",
                    "グランドバトル"
                };

                double bModeLeft = 1000 * reductionScale;
                double bModeTop = 50 * reductionScale;

                Grid grid = new Grid() { Margin = new Thickness(bModeLeft, bModeTop, 50, 50) };
                grid.ColumnDefinitions.Add(new ColumnDefinition() { });
                grid.RowDefinitions.Add(new RowDefinition());
                grid.RowDefinitions.Add(new RowDefinition());

                combBMode = new ComboBox
                {
                    ItemsSource = modeList,
                    SelectedIndex = 1
                };

                combBMode.SelectionChanged += (sender, e) => CombBoxMode_SelectionChanged(sender);

                grid.AddChild(combBMode, 0, 0, 1, 1);

                combVSType = new ComboBox
                {
                    ItemsSource = vsTypeList,
                    SelectedIndex = 0
                };

                combVSType.SelectionChanged += (sender, e) => CombBoxVSType_SelectionChanged(sender);

                grid.AddChild(combVSType, 1, 0, 1, 1);

                CanvasGuildMap.Children.Add(grid);

                double bTimeLeft = 580 * reductionScale;
                double bTimeFZ = 46 * reductionScale;

                textBlockTime = new TextBlock()
                {
                    TextAlignment = TextAlignment.Center,
                    FontSize = bTimeFZ,
                    Margin = new Thickness(bTimeLeft, 5, 50, 50),
                };

                CanvasGuildMap.Children.Add(textBlockTime);

                DispatcherTimer timer = new DispatcherTimer();
                timer.Tick += TimerTick;
                timer.Interval = new TimeSpan(0, 0, 0, 0, 10);
                timer.Start();

            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }

        void TimerTick(object sender, EventArgs e)
        {
            textBlockTime.Text = DateTime.Now.ToString("HH:mm:ss");
        }

        private void CombBoxVSType_SelectionChanged(object sender)
        {
            try
            {
                gBGroupDtos = BaseGBGroup.GetBaseGBGroup(combVSType.SelectedIndex);

                if (0 == combVSType.SelectedIndex)
                {
                    CanvasGuildMap.Background = new ImageBrush(new BitmapImage(new Uri(imgGuildPath + MAP_GUILD_VS_BASE + imgExt)));
                }
                else if (1 == combVSType.SelectedIndex)
                {
                    CanvasGuildMap.Background = new ImageBrush(new BitmapImage(new Uri(imgGuildPath + MAP_GROUND_VS_BASE + imgExt)));
                }

                foreach (Image image in imgPartsList)
                {
                    CanvasGuildMap.Children.Remove(image);
                }

                imgPartsList.Clear();
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }

        private void CombBoxMode_SelectionChanged(object sender)
        {
            try
            {
                foreach (Image image in imgPartsList)
                {
                    CanvasGuildMap.Children.Remove(image);
                }

                imgPartsList.Clear();
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }

        private void Window_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                foreach (Image image in imgPartsList)
                {
                    CanvasGuildMap.Children.Remove(image);
                }

                imgPartsList.Clear();

                Point mapPoint = e.GetPosition(this);

                mapPoint.X *= MAP_SCALE_SIZE * expansionScale;
                mapPoint.Y *= MAP_SCALE_SIZE * expansionScale;

                if (reductionScale == 0.5)
                {
                    mapPoint.X += 30;
                    mapPoint.Y += 40;
                }

                if (reductionScale == 0.75)
                {
                    mapPoint.X += 5;
                    mapPoint.Y += 10;
                }

                GBGroupDto gBGroupDto = gBGroupDtos.Where(x => x.BaseRect.Contains(mapPoint)).FirstOrDefault();

                if (null != gBGroupDto)
                {
                    // 指定拠点パーツを表示する場所の計算
                    double baseX = gBGroupDto.BaseRect.X / MAP_SCALE_SIZE * reductionScale;
                    double baseY = gBGroupDto.BaseRect.Y / MAP_SCALE_SIZE * reductionScale;

                    double leftLeft = 0;
                    double leftTop = 0;
                    double leftRight = 0;
                    double leftBottom = 0;
                    double centerLeft = 0;
                    double centerTop = 0;
                    double centerRight = 0;
                    double centerBottom = 0;
                    double rightLeft = 0;
                    double rightTop = 0;
                    double rightRight = 0;
                    double rightBottom = 0;

                    switch (gBGroupDto.Type)
                    {
                        case 1:
                            leftLeft = MAP_TEMPLE_LEFT_LEFT * reductionScale;
                            leftTop = MAP_TEMPLE_LEFT_TOP * reductionScale;
                            leftRight = MAP_TEMPLE_LEFT_RIGHT * reductionScale;
                            leftBottom = MAP_TEMPLE_LEFT_BOTTOM * reductionScale;
                            centerLeft = MAP_TEMPLE_CENTER_LEFT * reductionScale;
                            centerTop = MAP_TEMPLE_CENTER_TOP * reductionScale;
                            centerRight = MAP_TEMPLE_CENTER_RIGHT * reductionScale;
                            centerBottom = MAP_TEMPLE_CENTER_BOTTOM * reductionScale;
                            rightLeft = MAP_TEMPLE_RIGHT_LEFT * reductionScale;
                            rightTop = MAP_TEMPLE_RIGHT_TOP * reductionScale;
                            rightRight = MAP_TEMPLE_RIGHT_RIGHT * reductionScale;
                            rightBottom = MAP_TEMPLE_RIGHT_BOTTOM * reductionScale;
                            break;
                        case 2:
                            leftLeft = MAP_CASTLE_LEFT_LEFT * reductionScale;
                            leftTop = MAP_CASTLE_LEFT_TOP * reductionScale;
                            leftRight = MAP_CASTLE_LEFT_RIGHT * reductionScale;
                            leftBottom = MAP_CASTLE_LEFT_BOTTOM * reductionScale;
                            centerLeft = MAP_CASTLE_CENTER_LEFT * reductionScale;
                            centerTop = MAP_CASTLE_CENTER_TOP * reductionScale;
                            centerRight = MAP_CASTLE_CENTER_RIGHT * reductionScale;
                            centerBottom = MAP_CASTLE_CENTER_BOTTOM * reductionScale;
                            rightLeft = MAP_CASTLE_RIGHT_LEFT * reductionScale;
                            rightTop = MAP_CASTLE_RIGHT_TOP * reductionScale;
                            rightRight = MAP_CASTLE_RIGHT_RIGHT * reductionScale;
                            rightBottom= MAP_CASTLE_RIGHT_BOTTOM * reductionScale;
                            break;
                        case 3:
                            leftLeft = MAP_CHURCH_LEFT_LEFT * reductionScale;
                            leftTop = MAP_CHURCH_LEFT_TOP * reductionScale;
                            leftRight = MAP_CHURCH_LEFT_RIGHT * reductionScale;
                            leftBottom = MAP_CHURCH_LEFT_BOTTOM * reductionScale;
                            centerLeft = MAP_CHURCH_CENTER_LEFT * reductionScale;
                            centerTop = MAP_CHURCH_CENTER_TOP * reductionScale;
                            centerRight = MAP_CHURCH_CENTER_RIGHT * reductionScale;
                            centerBottom = MAP_CHURCH_CENTER_BOTTOM * reductionScale;
                            rightLeft = MAP_CHURCH_RIGHT_LEFT * reductionScale;
                            rightTop = MAP_CHURCH_RIGHT_TOP * reductionScale;
                            rightRight = MAP_CHURCH_RIGHT_RIGHT * reductionScale;
                            rightBottom = MAP_CHURCH_RIGHT_BOTTOM * reductionScale;
                            break;
                    }

                    if (reductionScale == 0.5)
                    {
                        baseX -= 5;
                        baseY -= 7;
                    }

                    if (reductionScale == 0.75)
                    {
                        baseX -= 3;
                        baseY -= 0;
                    }

                    string leftImageName = string.Empty;
                    string centerImageName = string.Empty;
                    string rightImageName = string.Empty;

                    switch (combBMode.SelectedIndex)
                    {
                        case 0:
                            // 布告前
                            leftImageName = PARTS_BASE;
                            centerImageName = PARTS_RECONNAISSANCE;
                            break;
                        case 1:
                            // 布告時
                            leftImageName = PARTS_BASE;
                            centerImageName = PARTS_PROCLAMATION;
                            rightImageName = PARTS_RECONNAISSANCE;
                            break;
                        case 2:
                            // 開戦中(進行)
                            leftImageName = PARTS_BASE;
                            centerImageName = PARTS_RECONNAISSANCE;
                            rightImageName = PARTS_ADVANCE;
                            break;
                        case 3:
                            // 開戦中(偵察)
                            leftImageName = PARTS_BASE;
                            centerImageName = PARTS_RECONNAISSANCE;
                            break;
                        case 4:
                            // 開戦中(防衛)
                            leftImageName = PARTS_BASE;
                            centerImageName = PARTS_RECONNAISSANCE;
                            rightImageName = PARTS_DEFENSE;
                            break;
                        case 5:
                            // 開戦終(偵察)
                            leftImageName = PARTS_BASE;
                            centerImageName = PARTS_RECONNAISSANCE;
                            break;
                        case 6:
                            // 開戦終(防衛)
                            leftImageName = PARTS_BASE;
                            centerImageName = PARTS_DEFENSE;
                            break;
                    }

                    if (!string.IsNullOrEmpty(leftImageName))
                    {
                        Image imgPartsLeft = new Image
                        {
                            Source = new BitmapImage(new Uri(imgGuildPath + leftImageName + imgExt)),
                            Width = 40 * reductionScale,
                            Height = 40 * reductionScale,
                            Margin = new Thickness(baseX - leftLeft, baseY + leftTop, baseX + leftRight, baseY + leftBottom)
                        };

                        CanvasGuildMap.Children.Add(imgPartsLeft);
                        imgPartsList.Add(imgPartsLeft);
                    }
                    
                    if (!string.IsNullOrEmpty(centerImageName))
                    {
                        Image imgPartsCenter = new Image
                        {
                            Source = new BitmapImage(new Uri(imgGuildPath + centerImageName + imgExt)),
                            Width = 40 * reductionScale,
                            Height = 40 * reductionScale,
                            Margin = new Thickness(baseX + centerLeft, baseY + centerTop, baseX + centerRight, baseY + centerBottom)
                        };

                        CanvasGuildMap.Children.Add(imgPartsCenter);
                        imgPartsList.Add(imgPartsCenter);
                    }

                    if (!string.IsNullOrEmpty(rightImageName))
                    {
                        Image imgPartsRight = new Image
                        {
                            Source = new BitmapImage(new Uri(imgGuildPath + rightImageName + imgExt)),
                            Width = 40 * reductionScale,
                            Height = 40 * reductionScale,
                            Margin = new Thickness(baseX + rightLeft, baseY + rightTop, baseX + rightRight, baseY + rightBottom)
                        };

                        CanvasGuildMap.Children.Add(imgPartsRight);
                        imgPartsList.Add(imgPartsRight);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }

        private void Window_MouseRightButtonUp(object obj, System.Windows.Input.MouseButtonEventArgs args)
        {
            try
            {
                ContextMenu = null;

                Point mapPoint = args.GetPosition(this);

                mapPoint.X *= MAP_SCALE_SIZE * expansionScale;
                mapPoint.Y *= MAP_SCALE_SIZE * expansionScale;

                GBGroupDto gBGroupDto = gBGroupDtos.Where(x => x.BaseRect.Contains(mapPoint)).FirstOrDefault();

                if (null != gBGroupDto)
                {
                    // 担当プレイヤーを表示
                    PlayerDao playerDao = new PlayerDao();

                    PlayerDto[] playerDtos = playerDao.GetPlayerDtos(selectedGuildName);
                    PlayerDto[] groupPDtos = playerDtos.Where(x => x.GBGroupId == gBGroupDto.Id).ToArray();

                    if (null == groupPDtos || groupPDtos.Length == 0)
                    {
                        return;
                    }

                    ContextMenu menu = new ContextMenu();

                    foreach (PlayerDto playerDto in groupPDtos)
                    {
                        MenuItem menuItem = new MenuItem
                        {
                            Header = playerDto.Name
                        };

                        menuItem.Click += (sender, e) => MenuItem_Click(sender);

                        menu.Items.Add(menuItem);
                    }

                    ContextMenu = menu;
                }
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }

        private void MenuItem_Click(object sender)
        {
            try
            {
                MenuItem menuItem = (MenuItem)sender;

                string name = menuItem.Header.ToString();

                PlayerDao playerDao = new PlayerDao();

                PlayerDto playerDto = playerDao.GetPlayerDto(name, selectedGuildName);

                MemberInfo memberInfo = new MemberInfo(playerDto);
                memberInfo.LoadMemberInfo();
                memberInfo.Show();
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }
    }
}
