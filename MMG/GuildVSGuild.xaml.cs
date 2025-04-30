using log4net;
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

namespace MMG
{
    /// <summary>
    /// GuildVSGuild.xaml の相互作用ロジック
    /// </summary>
    public partial class GuildVSGuild : Window
    {
        private readonly int IMAGE_WIDTH = 50;
        private readonly int IMAGE_HEIGHT = 50;
        private readonly Color KO_COLOR = Color.FromRgb(255, 0, 0);

        private readonly string imgCharaPath = Common.Common.FormationCharaImagePath;
        private readonly string imgExt = Common.Common.FormationImageExtension;

        private readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().Name);

        private string selectAttackGuildName = string.Empty;
        private string selectDefenseGuildName = string.Empty;
        private bool isSpeedView = true;

        private Dictionary<long, int> virtualityFormationAttackDic;
        private Dictionary<long, int> virtualityFormationDefenseDic;

        private GuildVSAdmin guildVSAdmin;

        public GuildVSGuild()
        {
            InitializeComponent();

            virtualityFormationAttackDic = new Dictionary<long, int>();
            virtualityFormationDefenseDic = new Dictionary<long, int>();

            LoadDefenseGuildItems();
        }

        public GuildVSGuild(GuildVSAdmin guildVSAdmin) : this()
        {
            this.guildVSAdmin = guildVSAdmin;

            LoadDefenseGuildItems(guildVSAdmin.SelectFVGuildName);
        }

        private void LoadAttackGuildItems(string guildName = null)
        {
            try
            {
                GuildDao guildDao = new GuildDao();

                GuildDto[] guildDtos = guildDao.GetGuildDtos();

                foreach (GuildDto guildDto in guildDtos)
                {
                    if (!selectDefenseGuildName.Equals(guildDto.Name))
                    {
                        CombAttackGuild.Items.Add(guildDto.Name);
                    }
                }

                if (!string.IsNullOrEmpty(guildName))
                {
                    CombAttackGuild.SelectedValue = guildName;
                }
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }

        private void LoadDefenseGuildItems(string guildName = null)
        {
            try
            {
                GuildDao guildDao = new GuildDao();

                GuildDto[] guildDtos = guildDao.GetGuildDtos();

                foreach (GuildDto guildDto in guildDtos)
                {
                    CombDefenseGuild.Items.Add(guildDto.Name);
                }

                if (!string.IsNullOrEmpty(guildName))
                {
                    CombDefenseGuild.SelectedValue = guildName;
                }
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }

        private void LoadDefenseFormation(string guildName)
        {
            try
            {
                // グリッドに防衛編成のデータをロード(デバフは表示しない)
                GuildMemberDao guildMemberDao = new GuildMemberDao();

                GuildDto guildDto = guildMemberDao.GetGuildMembers(guildName, virtualityFormationDefenseDic);
                PlayerDto[] playerDtos = guildDto.Members;

                int charaCnt = 1;
                int formationCnt = 0;
                int top = 10;
                int left = 10;
                int right = 0;
                int bottom = 0;

                GridDefenseFormation.HorizontalAlignment = HorizontalAlignment.Left;
                GridDefenseFormation.VerticalAlignment = VerticalAlignment.Top;

                GridDefenseFormation.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(100) });
                GridDefenseFormation.ColumnDefinitions.Add(new ColumnDefinition() { });

                foreach (PlayerDto playerDto in playerDtos)
                {
                    foreach (FormationDto formationDto in playerDto.Formations)
                    {
                        if (formationDto.IsDebuff)
                        {
                            continue;
                        }

                        formationCnt++;
                    }
                }

                for (int i = 0; i < formationCnt; i++)
                {
                    GridDefenseFormation.RowDefinitions.Add(new RowDefinition());
                }

                formationCnt = 0;

                foreach (PlayerDto playerDto in playerDtos)
                {
                    foreach (FormationDto formationDto in playerDto.Formations)
                    {
                        if (formationDto.IsDebuff)
                        {
                            continue;
                        }

                        if (formationDto.Id  < 0) 
                        {
                            formationDto.RestorationId(playerDto.UId);
                        }
                        
                        WrapPanel wrapPanel = new WrapPanel();
                        Grid playerGrid = new Grid();

                        playerGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(80) });
                        playerGrid.RowDefinitions.Add(new RowDefinition());
                        playerGrid.RowDefinitions.Add(new RowDefinition());

                        Label label = new Label()
                        {
                            Content = playerDto.Name + Environment.NewLine + formationDto.Name
                        };

                        playerGrid.AddChild(label, 0, 0, 1, 1);

                        Button button = new Button()
                        {
                            Content = "有効編成",
                            ToolTip = formationDto.Id.ToString(),
                            Height = 22,
                            Width = 52,
                        };

                        button.Click += (sender, e) => ValidFormationView_Click(sender);

                        playerGrid.AddChild(button, 1, 0, 1, 1);

                        GridDefenseFormation.AddChild(playerGrid, formationCnt, 0, 1, 1);

                        foreach (CharacterDto characterDto in formationDto.Characters)
                        {
                            Thickness margin = new Thickness(left, top, right, bottom);

                            Image image = new Image
                            {
                                Source = new BitmapImage(new Uri(imgCharaPath + characterDto.Name + imgExt)),
                                Name = "ImgList" + charaCnt,
                                ToolTip = characterDto.Name,
                                HorizontalAlignment = HorizontalAlignment.Left,
                                Width = IMAGE_WIDTH,
                                Height = IMAGE_HEIGHT,
                                Margin = margin,
                                VerticalAlignment = VerticalAlignment.Top
                            };

                            if (isSpeedView)
                            {
                                BitmapImage bmp = new BitmapImage(new Uri(imgCharaPath + characterDto.Name + imgExt));

                                DrawingGroup drawingGroup = new DrawingGroup();

                                using (DrawingContext drawContent = drawingGroup.Open())
                                {
                                    // 画像を書いて、その下にテキストを書く
                                    drawContent.DrawImage(bmp, new Rect(0, 0, bmp.PixelWidth, bmp.PixelHeight));
                                    drawContent.DrawText(new FormattedText(Common.Common.GetSpeedRuneText(characterDto),
                                                                            System.Globalization.CultureInfo.CurrentUICulture,
                                                                            FlowDirection.LeftToRight,
                                                                            new Typeface("Verdana"), 22, Brushes.Red,
                                                                            VisualTreeHelper.GetDpi(this).PixelsPerDip),
                                                                            new Point(5, 100));
                                }

                                image.Source = new DrawingImage(drawingGroup);
                            }

                            wrapPanel.Children.Add(image);
                            charaCnt++;
                        }

                        GridDefenseFormation.AddChild(wrapPanel, formationCnt, 1, 1, 1);

                        formationCnt++;
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }

        private void LoadAttackFormation(string guildName, int defenseFormationId)
        {
            try
            {
                GridAttackFormation.Children.Clear();
                GridAttackFormation.ColumnDefinitions.Clear();
                GridAttackFormation.RowDefinitions.Clear();

                // グリッドに防衛選択編成に有効な編成をロード
                GuildMemberDao guildMemberDao = new GuildMemberDao();

                GuildDto guildDto = guildMemberDao.GetGuildMembers(guildName, virtualityFormationAttackDic);
                PlayerDto[] playerDtos = guildDto.Members;

                FormationVSFormationDao formationVSFormationDao = new FormationVSFormationDao();

                FormationVSFormationDto[] formationVSFormationDtos = formationVSFormationDao.GetFormationVSFormationDtos();

                int charaCnt = 1;
                int formationCnt = 0;
                int top = 10;
                int left = 10;
                int right = 0;
                int bottom = 0;

                GridAttackFormation.HorizontalAlignment = HorizontalAlignment.Left;
                GridAttackFormation.VerticalAlignment = VerticalAlignment.Top;

                GridAttackFormation.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(100) });
                GridAttackFormation.ColumnDefinitions.Add(new ColumnDefinition() { });

                FormationVSFormationDto[] winDtos = new FormationVSFormationDto[0];

                if (CheckAttackJudgFixed.IsChecked.Value)
                {
                    winDtos = formationVSFormationDtos.Where(x => x.LoseFormationId == defenseFormationId).ToArray();
                }
                else
                {
                    // TODO : ギルド内の編成で勝てる可能性のある編成を計算して求める
                }

                foreach (PlayerDto playerDto in playerDtos)
                {
                    foreach (FormationDto formationDto in playerDto.Formations)
                    {
                        if (formationDto.IsDebuff)
                        {
                            continue;
                        }

                        // 勝利の可能性がある編成の数を求める
                        if (winDtos.Length > 0)
                        {
                            foreach (FormationVSFormationDto dto in winDtos)
                            {
                                if (dto.WinFormationId == formationDto.Id)
                                {
                                    formationCnt++;
                                }
                            }
                        }
                    }
                }

                for (int i = 0; i < formationCnt; i++)
                {
                    GridAttackFormation.RowDefinitions.Add(new RowDefinition());
                }

                if (formationCnt == 0)
                {
                    Label label = new Label
                    {
                        Content = "勝利可能編成は設定されていませんでした。",
                        FontSize = 16,
                        Foreground = new SolidColorBrush(KO_COLOR)
                    };

                    GridAttackFormation.AddChild(label, 0, 1, 1, 1);

                    return;
                }

                formationCnt = 0;

                foreach (PlayerDto playerDto in playerDtos)
                {
                    foreach (FormationDto formationDto in playerDto.Formations)
                    {
                        if (formationDto.IsDebuff ||
                            winDtos.Where(x => x.WinFormationId == formationDto.Id && x.LoseFormationId == defenseFormationId).Count() == 0)
                        {
                            continue;
                        }

                        FormationVSFormationDto winDto = winDtos.Where(x => x.WinFormationId == formationDto.Id &&
                                                                            x.LoseFormationId == defenseFormationId).FirstOrDefault();

                        WrapPanel wrapPanel = new WrapPanel();
                        Grid playerGrid = new Grid
                        {
                            HorizontalAlignment = HorizontalAlignment.Left,
                            VerticalAlignment = VerticalAlignment.Top
                        };

                        playerGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(80) });
                        playerGrid.RowDefinitions.Add(new RowDefinition());
                        playerGrid.RowDefinitions.Add(new RowDefinition());

                        Label label = new Label()
                        {
                            Content = playerDto.Name + "\n" + formationDto.Name
                        };

                        playerGrid.AddChild(label, 0, 0, 1, 1);

                        Grid invasionGrid = new Grid
                        {
                            HorizontalAlignment = HorizontalAlignment.Left,
                            VerticalAlignment = VerticalAlignment.Top
                        };

                        invasionGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(45) });

                        int col = 0;

                        // 進攻ボタンは、ギルドバトル進捗管理画面から起動されている場合にのみ表示する。
                        if (null != guildVSAdmin)
                        {
                            invasionGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(38) });
                            invasionGrid.RowDefinitions.Add(new RowDefinition());

                            Button button = new Button()
                            {
                                Content = "進攻",
                                ToolTip = formationDto.Id.ToString(),
                                HorizontalAlignment = HorizontalAlignment.Left,
                                Height = 22,
                                Width = 28,
                            };

                            string checkAC = guildVSAdmin.CheckAttackCoordination(formationDto.Id);

                            if (!string.IsNullOrEmpty(checkAC))
                            {
                                button.IsEnabled = false;
                            }
                            else
                            {
                                button.Click += (sender, e) => AttackCoordination_Click(sender);
                            }

                            invasionGrid.AddChild(button, 0, 0, 1, 1);
                            col++;
                        }
                        else
                        {
                            invasionGrid.RowDefinitions.Add(new RowDefinition());
                        }
                        
                        label = new Label();

                        if (winDto.DebuffKONum > 0)
                        {
                            label.Content = winDto.DebuffKONum + "KO";
                            label.Foreground = new SolidColorBrush(KO_COLOR);
                        }

                        invasionGrid.AddChild(label, 0, col, 1, 1);

                        playerGrid.AddChild(invasionGrid, 1, 0, 1, 1);

                        GridAttackFormation.AddChild(playerGrid, formationCnt, 0, 1, 1);

                        foreach (CharacterDto characterDto in formationDto.Characters)
                        {
                            Thickness margin = new Thickness(left, top, right, bottom);

                            Image image = new Image
                            {
                                Source = new BitmapImage(new Uri(imgCharaPath + characterDto.Name + imgExt)),
                                Name = "ImgList" + charaCnt,
                                ToolTip = characterDto.Name,
                                HorizontalAlignment = HorizontalAlignment.Left,
                                Width = IMAGE_WIDTH,
                                Height = IMAGE_HEIGHT,
                                Margin = margin,
                                VerticalAlignment = VerticalAlignment.Top
                            };

                            if (isSpeedView)
                            {
                                BitmapImage bmp = new BitmapImage(new Uri(imgCharaPath + characterDto.Name + imgExt));

                                DrawingGroup drawingGroup = new DrawingGroup();

                                using (DrawingContext drawContent = drawingGroup.Open())
                                {
                                    // 画像を書いて、その下にテキストを書く
                                    drawContent.DrawImage(bmp, new Rect(0, 0, bmp.PixelWidth, bmp.PixelHeight));
                                    drawContent.DrawText(new FormattedText(Common.Common.GetSpeedRuneText(characterDto),
                                                                            System.Globalization.CultureInfo.CurrentUICulture,
                                                                            FlowDirection.LeftToRight,
                                                                            new Typeface("Verdana"), 22, Brushes.Red,
                                                                            VisualTreeHelper.GetDpi(this).PixelsPerDip),
                                                                            new Point(5, 100));
                                }

                                image.Source = new DrawingImage(drawingGroup);
                            }

                            wrapPanel.Children.Add(image);
                            charaCnt++;
                        }

                        GridAttackFormation.AddChild(wrapPanel, formationCnt, 1, 1, 1);

                        formationCnt++;
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }

        private void AttackCoordination_Click(object sender)
        {
            try
            {
                Button button = (Button)sender;

                int formationId = int.Parse(button.ToolTip.ToString());

                // ギルドバトル進捗管理画面と連携している場合は、スタミナが残っている場合は消費する
                if (null != guildVSAdmin)
                {
                    guildVSAdmin.FormationButtonExecute(formationId);
                }
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }

        private void ValidFormationView_Click(object sender)
        {
            try
            {
                if (null == CombAttackGuild.SelectedItem || string.Empty.Equals(CombAttackGuild.SelectedItem.ToString()))
                {
                    MessageBox.Show("進攻ギルドを選択して下さい。");
                    return;
                }

                Button button = (Button)sender;

                int formationId = int.Parse(button.ToolTip.ToString());

                LoadAttackFormation(selectAttackGuildName, formationId);
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }

        private void CombAttackGuild_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                selectAttackGuildName = CombAttackGuild.SelectedItem.ToString();

                if (CheckAttackDetailSetting.IsChecked.Value)
                {
                    GuildVSDetailSetting guildVSDetailSetting = new GuildVSDetailSetting(
                                                                        selectAttackGuildName,
                                                                        new List<GuildVSDetailMemberDto>());

                    guildVSDetailSetting.ShowDialog();

                    foreach (GuildVSDetailMemberDto member in guildVSDetailSetting.GuildVSDetailMemberDtos)
                    {
                        if (member.VirtualityFormation > 0)
                        {
                            virtualityFormationAttackDic.Add(member.Id, member.VirtualityFormation);
                        }
                    }
                }
                else
                {
                    // 既存の仮想編成をロード
                    PlayerDao playerDao = new PlayerDao();
                    PlayerDto[] playerDtos = playerDao.GetPlayerDtos(selectAttackGuildName);

                    foreach (PlayerDto playerDto in playerDtos)
                    {
                        if (playerDto.VirtualityFormation > 0)
                        {
                            virtualityFormationAttackDic.Add(playerDto.Id, playerDto.VirtualityFormation);
                        }
                    }
                }

                CombAttackGuild.IsEnabled = false;
                CheckAttackDetailSetting.IsEnabled = false;
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }

        private void CombDefenseGuild_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                selectDefenseGuildName = CombDefenseGuild.SelectedItem.ToString();

                if (CheckDefenseDetailSetting.IsChecked.Value)
                {
                    GuildVSDetailSetting guildVSDetailSetting = new GuildVSDetailSetting(
                                                                        selectDefenseGuildName,
                                                                        new List<GuildVSDetailMemberDto>());

                    guildVSDetailSetting.ShowDialog();

                    foreach (GuildVSDetailMemberDto member in guildVSDetailSetting.GuildVSDetailMemberDtos)
                    {
                        if (member.VirtualityFormation > 0)
                        {
                            virtualityFormationDefenseDic.Add(member.Id, member.VirtualityFormation);
                        }
                    }
                }
                else
                {
                    // 既存の仮想編成をロード
                    PlayerDao playerDao = new PlayerDao();
                    PlayerDto[] playerDtos = playerDao.GetPlayerDtos(selectDefenseGuildName);

                    foreach (PlayerDto playerDto in playerDtos)
                    {
                        if (playerDto.VirtualityFormation > 0)
                        {
                            virtualityFormationDefenseDic.Add(playerDto.Id, playerDto.VirtualityFormation);
                        }
                    }
                }

                LoadDefenseFormation(selectDefenseGuildName);
                CombDefenseGuild.IsEnabled = false;
                CheckDefenseDetailSetting.IsEnabled = false;

                if (null != guildVSAdmin)
                {
                    LoadAttackGuildItems(guildVSAdmin.SelectGuildName);
                }
                else
                {
                    LoadAttackGuildItems();
                }

                CombAttackGuild.IsEnabled = true;
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }

        private void CheckAttackShowLine_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (CheckAttackShowLine.IsChecked == true)
                {
                    GridAttackFormation.ShowGridLines = true;
                }
                else
                {
                    GridAttackFormation.ShowGridLines = false;
                }
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }

        private void CheckDefenseShowLine_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (CheckDefenseShowLine.IsChecked == true)
                {
                    GridDefenseFormation.ShowGridLines = true;
                }
                else
                {
                    GridDefenseFormation.ShowGridLines = false;
                }
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }

        private void BtnGuildStatistics_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                GuildStatistics guildStatistics = new GuildStatistics(selectDefenseGuildName, selectAttackGuildName);
                guildStatistics.Show();
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }
    }
}
