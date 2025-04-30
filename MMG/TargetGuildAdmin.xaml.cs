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
    /// TargetGuildAdmin.xaml の相互作用ロジック
    /// </summary>
    public partial class TargetGuildAdmin : Window
    {
        private readonly int GRID_MAIN_COLUMN = 4;
        private readonly int MAX_GUILD_MEMBER = 50;
        private readonly string imgTokeNekoPath = Common.Common.FormationTokeNekoImagePath;
        private readonly string imgExt = Common.Common.FormationImageExtension;
        private readonly bool isDebuff30KO = false;
        private int guildStamina = 0;
        private int guildMaxStamina = 0;

        private readonly Color LABLE_ENTER_COLOR = Color.FromRgb(0, 255, 255);
        private readonly Color LABLE_LEAVEL_COLOR = Color.FromRgb(0, 0, 0);
        private readonly Color FST_MAX_COLOR = Color.FromRgb(147, 255, 153);
        private readonly Color FST_HALF_COLOR = Color.FromRgb(255, 242, 50);
        private readonly Color FST_ZERO_COLOR = Color.FromRgb(255, 48, 27);

        private readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().Name);

        private string targetGuildName = string.Empty;

        private Dictionary<long, int> virtualityFormationTargetDic;
        private Dictionary<long, int> virtualityFormationCharaNumTargetDic;
        private Dictionary<string, int> memberStaminaDic;
        private Dictionary<int, int> memberAttackIdDic;
        private Dictionary<int, string> memberAttackNameDic;
        private Dictionary<int, string> memberIdNameDic;
        private Dictionary<string, int> memberStaminaMaxDic;
        private Dictionary<string, List<int>> memberAttackButtonDic;
        private Dictionary<int, FormationDto> formationDic;
        private Dictionary<string, Label> memberStaminaLabDic;
        private Dictionary<int, Button> attackButtonDic;
        private List<int> formation30KOList;

        private GuildVSAdmin guildVSAdmin;

        public TargetGuildAdmin(GuildVSAdmin guildVSAdmin, string selectGuildName, string targetGuildName)
        {
            InitializeComponent();
            this.guildVSAdmin = guildVSAdmin;
            this.targetGuildName = targetGuildName;
            virtualityFormationTargetDic = new Dictionary<long, int>();
            virtualityFormationCharaNumTargetDic = new Dictionary<long, int>();
            LabSelectGuildName.Content = selectGuildName;
            LoadMemberArea(selectGuildName);
        }

        private void LoadMemberArea(string guildName)
        {
            try
            {
                GridMemberArea1.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(200) });
                GridMemberArea1.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(120) });
                GridMemberArea1.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(80) });
                GridMemberArea1.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(500) });


                GridMemberArea1.RowDefinitions.Add(new RowDefinition());

                Thickness marginTargetF = new Thickness(5, 5, 0, 0);
                Thickness marginName = new Thickness(5, 5, 0, 0);
                Thickness marginStamina = new Thickness(5, 5, 0, 0);
                Thickness marginFormation = new Thickness(5, 5, 0, 0);

                Label labTargetFormation = new Label
                {
                    Content = "勝利可能?プレイヤー(" + targetGuildName + ")",
                    ToolTip = "(KO後)適正編成を持っているプレイヤー",
                    Margin = marginTargetF
                };

                Label labName = new Label
                {
                    Content = "名前",
                    Margin = marginName
                };

                Label labStamina = new Label
                {
                    Content = "スタミナ",
                    Margin = marginStamina
                };

                Label labFormation = new Label
                {
                    Content = "編成",
                    Margin = marginFormation
                };

                GridMemberArea1.AddChild(labTargetFormation, 0, 0, 1, 1);
                GridMemberArea1.AddChild(labName, 0, 1, 1, 1);
                GridMemberArea1.AddChild(labStamina, 0, 2, 1, 1);
                GridMemberArea1.AddChild(labFormation, 0, 3, 1, 1);

                PlayerDao playerDao = new PlayerDao();
                PlayerDto[] vfPlayerDtos = playerDao.GetPlayerDtos(guildName);

                foreach (PlayerDto playerDto in vfPlayerDtos)
                {
                    if (playerDto.VirtualityFormation > 0)
                    {
                        virtualityFormationTargetDic.Add(playerDto.Id, playerDto.VirtualityFormation);
                    }

                    if (playerDto.VFCNum > 0)
                    {
                        virtualityFormationCharaNumTargetDic.Add(playerDto.Id, playerDto.VFCNum);
                    }
                }

                GuildMemberDao guildMemberDao = new GuildMemberDao();

                GuildDto guildDto = guildMemberDao.GetGuildMembers(guildName,
                                                                    virtualityFormationTargetDic,
                                                                    virtualityFormationCharaNumTargetDic);

                guildMaxStamina = guildDto.Stamina;
                guildStamina = guildDto.Stamina;
                LabMemberOnlineST.Content = guildStamina + " / " + guildDto.Stamina;

                for (int i = 0; i < MAX_GUILD_MEMBER; i++)
                {
                    GridMemberArea1.RowDefinitions.Add(new RowDefinition());
                }

                memberStaminaDic = new Dictionary<string, int>();
                memberAttackIdDic = new Dictionary<int, int>();
                memberAttackNameDic = new Dictionary<int, string>();
                memberIdNameDic = new Dictionary<int, string>();
                formationDic = new Dictionary<int, FormationDto>();
                memberStaminaLabDic = new Dictionary<string, Label>();
                memberStaminaMaxDic = new Dictionary<string, int>();
                attackButtonDic = new Dictionary<int, Button>();
                memberAttackButtonDic = new Dictionary<string, List<int>>();
                formation30KOList = new List<int>();

                PlayerDto[] playerDtos = guildDto.Members;

                if (CheckMinSTDown.IsChecked == true)
                {
                    playerDtos = playerDtos.OrderByDescending(value => value.Stamina).ToArray();
                }

                Dictionary<int, string> virtualityFormationDic = new VirtualityFormationDto().GetKeyValuePairs();

                PlayerVSPlayerDao playerVSPlayerDao = new PlayerVSPlayerDao();

                PlayerDto[] basePlayer = playerDao.GetPlayerDtos();
                PlayerVSPlayerDto[] playerVSPlayerDtos = playerVSPlayerDao.GetPlayerVSPlayerDtos();

                int cnt = 1;

                foreach (PlayerDto playerDto in playerDtos)
                {
                    WrapPanel wrapPanel = new WrapPanel();

                    PlayerVSPlayerDto[] winPlayerDtos = playerVSPlayerDtos.Where(x => x.LosePlayerId == playerDto.Id).ToArray();

                    if (winPlayerDtos.Length == 0)
                    {
                        wrapPanel.Children.Add(new Label()
                        {
                            Content = "プレイヤーなし"
                        }
                        );
                    }

                    foreach (PlayerVSPlayerDto playerVSPlayerDto in winPlayerDtos)
                    {
                        PlayerDto winPlayer = basePlayer.Where(x => x.Id == playerVSPlayerDto.WinPlayerId).FirstOrDefault();

                        if (null != winPlayer)
                        {
                            Label labWinPlayer = new Label()
                            {
                                Content = winPlayer.Name,
                                ToolTip = winPlayer.Id
                            };

                            labWinPlayer.MouseEnter += (sender, e) => LabelWinPlayer_MouseEnter(sender);
                            labWinPlayer.MouseLeave += (sender, e) => LabelWinPlayer_MouseLeave(sender);
                            labWinPlayer.MouseLeftButtonUp += (sender, e) => LabelWinPlayer_MouseLeftButtonUp(sender);

                            wrapPanel.Children.Add(labWinPlayer);
                        }
                    }

                    GridMemberArea1.AddChild(wrapPanel, cnt, 0, 1, 1);

                    Grid playerGrid = new Grid
                    {
                        Width = 120
                    };

                    playerGrid.ColumnDefinitions.Add(new ColumnDefinition() { });
                    playerGrid.RowDefinitions.Add(new RowDefinition());
                    playerGrid.RowDefinitions.Add(new RowDefinition());

                    Label labPName = new Label()
                    {
                        Content = playerDto.Name
                    };

                    playerGrid.AddChild(labPName, 0, 0, 1, 1);

                    Label labVFName = new Label();

                    if (virtualityFormationTargetDic.ContainsKey(playerDto.Id))
                    {
                        labVFName.Content += " (仮想編成中)";
                        labVFName.ToolTip = virtualityFormationDic[virtualityFormationTargetDic[playerDto.Id]];
                    }

                    if (virtualityFormationCharaNumTargetDic.ContainsKey(playerDto.Id))
                    {
                        labVFName.Content += " 自編追加";
                    }

                    playerGrid.AddChild(labVFName, 1, 0, 1, 1);

                    GridMemberArea1.AddChild(playerGrid, cnt, 1, 1, 1);

                    Label labPStamina = new Label()
                    {
                        ToolTip = playerDto.Name,
                        Content = playerDto.Stamina + " / " + playerDto.Stamina
                    };

                    memberStaminaDic.Add(playerDto.Name, playerDto.Stamina);
                    memberStaminaLabDic.Add(playerDto.Name, labPStamina);
                    memberStaminaMaxDic.Add(playerDto.Name, playerDto.Stamina);

                    GridMemberArea1.AddChild(labPStamina, cnt, 2, 1, 1);

                    // デバフ編成で30KO分をまとめて1つとする(30KOなかった場合は通常) 60KO分のデバフは主力1とデバフのみの場合だが、125キャラいないとできない、最短で2023/11半ばとなる
                    int debuffCnt = 0;

                    foreach (FormationDto formationDto in playerDto.Formations)
                    {
                        if (formationDto.IsDebuff)
                        {
                            foreach (CharacterDto characterDto in formationDto.Characters)
                            {
                                debuffCnt++;
                            }
                        }
                    }

                    List<int> formationIdList = new List<int>();

                    int formationHeight = 22;
                    string buttonNewLine = string.Empty;

                    wrapPanel = new WrapPanel()
                    {
                        Width = 480
                    };

                    int continue5F = 0;

                    foreach (FormationDto formationDto in playerDto.Formations)
                    {
                        Button btnFormation = new Button
                        {
                            ToolTip = formationDto.Id.ToString(),
                            Height = formationHeight,
                            Background = new SolidColorBrush(FST_MAX_COLOR)
                        };

                        btnFormation.Click += (sender, e) => Formation_Click(sender);
                        btnFormation.MouseRightButtonUp += (sender, e) => Formation_RightClick(sender);

                        // サブパ・デバフ要員ではなければ、スピードルーン記載
                        if (formationDto.IsDebuff)
                        {
                            if (debuffCnt >= 30 && isDebuff30KO == true)
                            {
                                btnFormation.Content = "2/2 30KO";
                                debuffCnt -= 30;
                                continue5F = 5;
                                formation30KOList.Add(formationDto.Id);
                            }
                            else if (isDebuff30KO == true && continue5F > 0)
                            {
                                // 5編成分スルーする
                                continue5F--;
                                btnFormation = null;
                                continue;
                            }
                            else
                            {
                                btnFormation.Content = "2/2 " + formationDto.Name;
                            }
                        }
                        else
                        {
                            btnFormation.Content = "2/2 " + formationDto.Name + Common.Common.GetMaxSpeedRuneText(formationDto);
                        }

                        if (formation30KOList.Contains(formationDto.Id))
                        {
                            memberAttackNameDic.Add(formationDto.Id, "30KO");
                        }
                        else
                        {
                            memberAttackNameDic.Add(formationDto.Id, formationDto.Name);
                        }

                        memberAttackIdDic.Add(formationDto.Id, 2);
                        memberIdNameDic.Add(formationDto.Id, playerDto.Name);
                        formationDic.Add(formationDto.Id, formationDto);
                        attackButtonDic.Add(formationDto.Id, btnFormation);

                        formationIdList.Add(formationDto.Id);

                        wrapPanel.Children.Add(btnFormation);
                    }

                    memberAttackButtonDic.Add(playerDto.Name, formationIdList);

                    GridMemberArea1.AddChild(wrapPanel, cnt, 3, 1, 1);

                    cnt++;
                }
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }

        private void Formation_RightClick(object sender)
        {
            try
            {
                Button button = (Button)sender;

                int formationId = int.Parse(button.ToolTip.ToString());

                ContextMenu menu = new ContextMenu();

                CharacterDto[] characterDtos = Common.Common.CalcCharacterSpeed(formationDic[formationId]);

                int topSpeed = Common.Common.CalcTopSpeed(characterDtos);

                foreach (CharacterDto characterDto in characterDtos)
                {
                    CharacterDto baseSpeed = formationDic[formationId].Characters.Where(x => x.Id == characterDto.Id).FirstOrDefault();

                    string name = string.Empty;

                    if (characterDto.Speed == baseSpeed.Speed)
                    {
                        name = characterDto.Speed + " " + characterDto.Name;
                    }
                    else
                    {
                        name = characterDto.Speed + "(" + baseSpeed.Speed + ") " + characterDto.Name;
                    }

                    if (characterDto.SpeedRune1 > 0 || characterDto.SpeedRune2 > 0 || characterDto.SpeedRune3 > 0)
                    {
                        name += " (" + characterDto.SpeedRune1 + "," + characterDto.SpeedRune2 + "," + characterDto.SpeedRune3 + ")";
                    }

                    if (topSpeed == characterDto.Speed)
                    {
                        Image iconSpeedTop = new Image
                        {
                            Source = new BitmapImage(new Uri(imgTokeNekoPath + "19" + imgExt)),
                            Width = 16,
                            Height = 16
                        };

                        Label labName = new Label
                        {
                            Content = name
                        };

                        WrapPanel wrapPanel = new WrapPanel();

                        wrapPanel.Children.Add(iconSpeedTop);
                        wrapPanel.Children.Add(labName);

                        menu.Items.Add(wrapPanel);
                    }
                    else
                    {
                        menu.Items.Add(name);
                    }
                }

                button.ContextMenu = menu;
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }

        private void LabelWinPlayer_MouseEnter(object sender)
        {
            try
            {
                Label label = (Label)sender;

                label.FontWeight = FontWeights.Bold;
                label.Foreground = new SolidColorBrush(LABLE_ENTER_COLOR);
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }

        private void LabelWinPlayer_MouseLeave(object sender)
        {
            try
            {
                Label label = (Label)sender;

                label.FontWeight = FontWeights.Normal;
                label.Foreground = new SolidColorBrush(LABLE_LEAVEL_COLOR);
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }

        private void LabelWinPlayer_MouseLeftButtonUp(object sender)
        {
            try
            {
                Label label = (Label)sender;

                long playerId = long.Parse(label.ToolTip.ToString());

                guildVSAdmin.Activate();
                guildVSAdmin.SelectGridMemberArea(playerId);
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }

        private void Formation_Click(object sender)
        {
            try
            {
                Button button = (Button)sender;

                int formationId = int.Parse(button.ToolTip.ToString());
                int stamina = formationDic[formationId].Characters.Length;

                if (formation30KOList.Contains(formationId))
                {
                    stamina *= 6;
                }

                if (memberAttackIdDic[formationId] > 0)
                {
                    memberAttackIdDic[formationId]--;
                    guildStamina -= stamina;
                    memberStaminaDic[memberIdNameDic[formationId]] -= stamina;

                    if (memberAttackIdDic[formationId] == 1)
                    {
                        button.Background = new SolidColorBrush(FST_HALF_COLOR);
                    }
                    else
                    {
                        button.Background = new SolidColorBrush(FST_ZERO_COLOR);
                    }
                }
                else
                {
                    // 間違えて押してしまったことを考えてリセット扱いにする
                    memberAttackIdDic[formationId] = 2;
                    guildStamina += stamina * 2;
                    memberStaminaDic[memberIdNameDic[formationId]] += stamina * 2;

                    button.Background = new SolidColorBrush(FST_MAX_COLOR);
                }

                Label label = memberStaminaLabDic[memberIdNameDic[formationId]];

                label.Content = memberStaminaDic[memberIdNameDic[formationId]] + " / " + memberStaminaMaxDic[memberIdNameDic[formationId]];

                button.Content = memberAttackIdDic[formationId] + "/2 " + memberAttackNameDic[formationId] + Common.Common.GetMaxSpeedRuneText(formationDic[formationId]);

                LabMemberOnlineST.Content = guildStamina + " / " + guildMaxStamina;

                SortStaminaValue();
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }

        private void CheckShowLine_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (CheckShowLine.IsChecked == true)
                {
                    GridMemberArea1.ShowGridLines = true;
                }
                else
                {
                    GridMemberArea1.ShowGridLines = false;
                }
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }

        private void SortStaminaValue()
        {
            try
            {
                if (CheckMinSTDown.IsChecked == true)
                {
                    // スタミナ順にグリッド列を入れ替える
                    int cnt = 1;
                    int index = GRID_MAIN_COLUMN;
                    int max = GridMemberArea1.Children.Count;

                    Dictionary<int, int> staminaDic = new Dictionary<int, int>();

                    for (int i = GRID_MAIN_COLUMN; i < max; i++)
                    {
                        UIElement element = GridMemberArea1.Children[i];

                        // 3カラム目のスタミナラベルから最大スタミナを求める
                        if (cnt == 3)
                        {
                            Label label = (Label)element;

                            string[] sp = label.Content.ToString().Split('/');

                            int st = int.Parse(sp[0].Trim());

                            staminaDic.Add(index, st);

                            index += GRID_MAIN_COLUMN;
                        }

                        cnt++;

                        if (cnt == 5)
                        {
                            cnt = 1;
                        }
                    }

                    IOrderedEnumerable<KeyValuePair<int, int>> sorted = staminaDic.OrderByDescending(x => x.Value);

                    cnt = GRID_MAIN_COLUMN;
                    int rowIdx = 1;

                    foreach (var pair in sorted)
                    {
                        max = cnt + GRID_MAIN_COLUMN;
                        int idx = pair.Key;
                        int columnIndex = 0;

                        for (int i = cnt; i < max; i++)
                        {
                            Grid.SetRow(GridMemberArea1.Children[idx], rowIdx);
                            Grid.SetColumn(GridMemberArea1.Children[idx], columnIndex);

                            idx++;
                            columnIndex++;
                        }

                        rowIdx++;
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }

        private void CheckMinSTDown_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SortStaminaValue();
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }
    }
}
