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
using static MMG.ControlParts.MultiSelectComboBox;
using Label = System.Windows.Controls.Label;

namespace MMG
{
    /// <summary>
    /// GuildVSAdmin.xaml の相互作用ロジック
    /// </summary>
    public partial class GuildVSAdmin : Window
    {
        // メメモリ7鯖の時間が1秒はやいような?
        private readonly TimeSpan SERVER_CORRECTION_TIME = new TimeSpan(0, 0, Common.Common.SERVER_TIME_CORRECTION_SECONDS);
        private readonly int GRID_MAIN_COLUMN = 4;
        private readonly int MAX_GUILD_MEMBER = 50;
        private readonly float DEFENSE_TIME = Common.Common.DEFENSE_TIME;
        private readonly string imgTokeNekoPath = Common.Common.FormationTokeNekoImagePath;
        private readonly string imgExt = Common.Common.FormationImageExtension;

        private readonly Color ONLINE_COLOR = Color.FromRgb(101, 216, 255);
        private readonly Color OFFLINE_COLOR = Color.FromRgb(255, 101, 101);
        private readonly Color FST_MAX_COLOR = Color.FromRgb(147, 255, 153);
        private readonly Color FST_HALF_COLOR = Color.FromRgb(255, 242, 50);
        private readonly Color FST_ZERO_COLOR = Color.FromRgb(255, 48, 27);
        private readonly Color SELECT_COLOR = Color.FromRgb(210, 240, 240);
        private readonly Color NOT_SELECT_COLOR = Color.FromRgb(240, 247, 246);
        private readonly Color BEFORE_15MIN_VS_COLOR = Color.FromRgb(255, 242, 50);
        private readonly Color VS_COLOR = Color.FromRgb(255, 101, 101);

        private readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().Name);

        private int onlineNum = 0;
        private int onlineMaxNum = 0;
        private int guildStamina = 0;
        private int guildMaxStamina = 0;
        private bool isDiscord = false;

        private TimeSpan before15minSTS = new TimeSpan(0, 0, 45, 0);
        private TimeSpan before15minETS = new TimeSpan(0, 1, 0, 0);
        private TimeSpan vsSTS = new TimeSpan(0, 0, 0, 0);
        private TimeSpan vsETS = new TimeSpan(0, 0, 45, 0);

        private Dictionary<int, Button> formationButtonDic;
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
        private Dictionary<long, int> targetGridIndexDic;
        private Dictionary<long, int> updateGBGroupIdDic;
        private List<int> formation30KOList;
        private List<GuildBattleProgressDto> guildBPList;
        private List<int> selectedGroupIdList;

        private Discord.Program discordP;
        private AuthDto authDto;

        public string SelectGuildName { get; set; } = string.Empty;
        public string SelectFVGuildName { get; set; } = string.Empty;
        public string SelectGVSFGuildName { get; set; } = string.Empty;

        public List<string> Items { get; set; }
        public List<string> SelectedItems { get; set; }

        public GuildVSAdmin(AuthDto authDto)
        {
            InitializeComponent();
            LoadGuildItems();
            virtualityFormationTargetDic = new Dictionary<long, int>();
            virtualityFormationCharaNumTargetDic = new Dictionary<long, int>();
            updateGBGroupIdDic = new Dictionary<long, int>();
            guildBPList = new List<GuildBattleProgressDto>();
            selectedGroupIdList = new List<int>();

            MSCombTargetGuild.OnFruitSelected += OnFruit_Selected;
            MSCombGuildFV.OnFruitSelected += OnFruit2_Selected;
            MSCombGuildVSF.OnFruitSelected += OnFruit3_Selected;

            DispatcherTimer timer = new DispatcherTimer();
            timer.Tick += TimerTick;
            timer.Interval = new TimeSpan(0, 0, 0, 0, 10);
            timer.Start();

            DispatcherTimer timer2 = new DispatcherTimer();
            timer2.Tick += TimerTick2;
            timer2.Interval = new TimeSpan(0, 0, 0, 0, 10);
            timer2.Start();

            this.authDto = authDto;

            discordP = new Discord.Program
            {
                GuildVSAdmin = this,
                ListBox = ListBoxDiscord,
                AuthDto = authDto,
                GuildBPList = guildBPList,
                Labstack1 = Labstack1.Content.ToString(),
                Labstack2 = Labstack2.Content.ToString(),
                Labstack3 = Labstack3.Content.ToString(),
                LabMemberOnlineST = LabMemberOnlineST.Content.ToString(),
                LabMemberOnline = LabMemberOnline.Content.ToString()
            };

            if (Common.Common.IsDiscordAvailable())
            {
                BtnDiscord.IsEnabled = true;
                ListBoxDiscord.IsEnabled = true;
            }
        }

        private void OnFruit_Selected(OnFruitSelectedEventArgs e)
        {
            try
            {
               TargetGuildAdmin targetGuildAdmin = new TargetGuildAdmin(
                                                        this, e.strFruit, SelectGuildName
                                                    );
               targetGuildAdmin.Show();
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }

        private void OnFruit2_Selected(OnFruitSelectedEventArgs e)
        {
            try
            {
                SelectFVGuildName = e.strFruit;
                GuildVSGuild guildVSGuild = new GuildVSGuild(this);
                guildVSGuild.Show();
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }

        private void OnFruit3_Selected(OnFruitSelectedEventArgs e)
        {
            try
            {
                SelectGVSFGuildName = e.strFruit;
                GuildVSFormation guildVSFormation = new GuildVSFormation(this);
                guildVSFormation.Show();
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

                foreach (GuildDto guildDto in guildDtos)
                {
                    CombGuild.Items.Add(guildDto.Name);
                }
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }

        private void LoadTargetGuildItems()
        {
            try
            {
                GuildDao guildDao = new GuildDao();

                GuildDto[] guildDtos = guildDao.GetGuildDtos();

                Items = new List<string>();
                SelectedItems = new List<string>();

                foreach (GuildDto guildDto in guildDtos)
                {
                    if (!SelectGuildName.Equals(guildDto.Name))
                    {
                        CombTargetGuild.Items.Add(guildDto.Name);
                        Items.Add(guildDto.Name);
                    }
                }

                MSCombTargetGuild.ItemsSource = Items;
                MSCombGuildFV.ItemsSource = Items;
                MSCombGuildVSF.ItemsSource = Items;
                MSCombTargetGuild.SelectedItems = SelectedItems;
                MSCombGuildFV.SelectedItems = SelectedItems;
                MSCombGuildVSF.SelectedItems = SelectedItems;
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }

        private void LoadMemberArea(string guildName)
        {
            try
            {
                GridMemberArea1.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(85) });
                GridMemberArea1.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(120) });
                GridMemberArea1.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(60) });
                GridMemberArea1.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(610) });

                GridMemberArea1.RowDefinitions.Add(new RowDefinition());

                Thickness marginOnline = new Thickness(5, 5, 0, 0);
                Thickness marginName = new Thickness(5, 5, 0, 0);
                Thickness marginStamina = new Thickness(5, 5, 0, 0);
                Thickness marginFormation = new Thickness(5, 5, 0, 0);

                Label labOnline = new Label
                {
                    Content = "ステータス",
                    Margin = marginOnline
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

                GridMemberArea1.AddChild(labOnline, 0, 0, 1, 1);
                GridMemberArea1.AddChild(labName, 0, 1, 1, 1);
                GridMemberArea1.AddChild(labStamina, 0, 2, 1, 1);
                GridMemberArea1.AddChild(labFormation, 0, 3, 1, 1);

                GuildMemberDao guildMemberDao = new GuildMemberDao();

                GuildDto guildDto = guildMemberDao.GetGuildMembers(guildName,
                                                                   virtualityFormationTargetDic,
                                                                   virtualityFormationCharaNumTargetDic);

                // グループフィルタリング
                guildDto = Common.Common.GroupFilter(guildDto, selectedGroupIdList);

                onlineMaxNum = guildDto.MemberNum;
                onlineNum = guildDto.MemberNum;
                LabMemberOnline.Content = onlineNum + " / " + guildDto.MemberNum;

                discordP.LabMemberOnline = LabMemberOnline.Content.ToString();

                guildMaxStamina = guildDto.Stamina;
                guildStamina = guildDto.Stamina;
                LabMemberOnlineST.Content = guildStamina + " / " + guildDto.Stamina;

                discordP.LabMemberOnlineST = LabMemberOnlineST.Content.ToString();

                UpdateDefenseTimeFromStamina();

                for (int i = 0; i < MAX_GUILD_MEMBER; i++)
                {
                    GridMemberArea1.RowDefinitions.Add(new RowDefinition());
                }

                formationButtonDic = new Dictionary<int, Button>();
                memberStaminaDic = new Dictionary<string, int>();
                memberAttackIdDic = new Dictionary<int, int>();
                memberAttackNameDic = new Dictionary<int, string>();
                memberIdNameDic = new Dictionary<int, string>();
                formationDic = new Dictionary<int, FormationDto>();
                memberStaminaLabDic = new Dictionary<string, Label>();
                memberStaminaMaxDic = new Dictionary<string, int>();
                attackButtonDic = new Dictionary<int, Button>();
                memberAttackButtonDic = new Dictionary<string, List<int>>();
                targetGridIndexDic = new Dictionary<long, int>();
                formation30KOList = new List<int>();

                PlayerDto[] playerDtos = guildDto.Members;

                if (CheckMinSTDown.IsChecked == true)
                {
                    playerDtos = playerDtos.OrderByDescending(value => value.Stamina).ToArray();
                }

                Dictionary<int, string> virtualityFormationDic = new VirtualityFormationDto().GetKeyValuePairs();
                GBGroupDto[] gBGroupDtos = BaseGBGroup.GetBaseGBGroup();
                GBGroupDto[] gBGroup2Dtos = BaseGBGroup.GetBaseGBGroup(1);
                gBGroupDtos = gBGroupDtos.Concat(gBGroup2Dtos).ToArray();

                int cnt = 1;

                guildBPList.Clear();

                foreach (PlayerDto playerDto in playerDtos)
                {
                    if (updateGBGroupIdDic.ContainsKey(playerDto.Id))
                    {
                        playerDto.GBGroupId = updateGBGroupIdDic[playerDto.Id];
                    }

                    GuildBattleProgressDto gbpDto = new GuildBattleProgressDto
                    {
                        MMId = playerDto.Id,
                        Name = playerDto.Name
                    };

                    targetGridIndexDic.Add(playerDto.Id, cnt);

                    Grid statusGrid = new Grid
                    {
                        Width = 85
                    };

                    statusGrid.ColumnDefinitions.Add(new ColumnDefinition() { });
                    statusGrid.RowDefinitions.Add(new RowDefinition());
                    statusGrid.RowDefinitions.Add(new RowDefinition());

                    Button btnOnline = new Button()
                    {
                        Content = "オンライン",
                        ToolTip = playerDto.Name,
                        Width = 55,
                        Height = 20,
                        HorizontalAlignment = HorizontalAlignment.Left,
                        Background = new SolidColorBrush(ONLINE_COLOR)
                    };

                    btnOnline.Click += (sender, e) => Online_Click(sender);

                    statusGrid.AddChild(btnOnline, 0, 0, 1, 1);

                    // 担当グループ
                    GBGroupDto gBGroupDto = gBGroupDtos.Where(x => x.Id == playerDto.GBGroupId).FirstOrDefault();
  
                    Label labGBGroup = new Label()
                    {
                        Content = gBGroupDto?.Name
                    };

                    statusGrid.AddChild(labGBGroup, 1, 0, 1, 1);

                    GridMemberArea1.AddChild(statusGrid, cnt, 0, 1, 1);

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

                    CheckBox checkBox = new CheckBox()
                    {
                        ToolTip = playerDto.Name,
                        Content = "VC",
                        IsChecked = playerDto.IsVC
                    };

                    if (virtualityFormationTargetDic.ContainsKey(playerDto.Id))
                    {
                        checkBox.Content += " (仮想編成中)";
                        checkBox.ToolTip = virtualityFormationDic[virtualityFormationTargetDic[playerDto.Id]];
                    }

                    if (virtualityFormationCharaNumTargetDic.ContainsKey(playerDto.Id))
                    {
                        checkBox.Content += " 自編追加";
                    }

                    playerGrid.MouseLeftButtonUp += (sender, e) => PlayerGrid_MouseLeftButtonUp(sender);

                    playerGrid.AddChild(checkBox, 1, 0, 1, 1);

                    GridMemberArea1.AddChild(playerGrid, cnt, 1, 1, 1);

                    Label labPStamina = new Label()
                    {
                        ToolTip = playerDto.Name,
                        Content = playerDto.Stamina + " / " + playerDto.Stamina
                    };

                    gbpDto.Stamina = labPStamina.Content.ToString();

                    memberStaminaDic.Add(playerDto.Name, playerDto.Stamina);
                    memberStaminaLabDic.Add(playerDto.Name, labPStamina);
                    memberStaminaMaxDic.Add(playerDto.Name, playerDto.Stamina);

                    labPStamina.MouseLeftButtonUp += (sender, e) => LabPStamina_MouseLeftButtonUp(sender);

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

                    WrapPanel wrapPanel = new WrapPanel()
                    {
                        Width = 580
                    };

                    int continue5F = 0;

                    Dictionary<string, int> gbpFormation = new Dictionary<string, int>();
                    Dictionary<string, int> gbpFormationIdName = new Dictionary<string, int>();

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
                            if (debuffCnt >= 30 && CheckDebuff30KO.IsChecked == true)
                            {
                                btnFormation.Content = "2/2 30KO";
                                debuffCnt -= 30;
                                continue5F = 5;
                                formation30KOList.Add(formationDto.Id);
                            }
                            else if (CheckDebuff30KO.IsChecked == true && continue5F > 0)
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

                        int bit = 0;

                        if (formationDto.IsDebuff)
                        {
                            bit = Common.Common.DEBUFF_P_2;
                        }
                        else if (formationDto.IsSubParty)
                        {
                            bit = Common.Common.SUB_P_2;
                        }
                        else
                        {
                            bit = Common.Common.MAIN_P_2;
                        }

                        if (gbpFormation.ContainsKey(btnFormation.Content.ToString()))
                        {
                            gbpFormation.Add(btnFormation.Content.ToString() + "_" + btnFormation.ToolTip, bit);
                            gbpFormationIdName.Add(btnFormation.Content.ToString() + "_" + btnFormation.ToolTip, formationDto.Id);
                        }
                        else
                        {
                            gbpFormation.Add(btnFormation.Content.ToString(), bit);
                            gbpFormationIdName.Add(btnFormation.Content.ToString(), formationDto.Id);
                        }

                        memberAttackIdDic.Add(formationDto.Id, 2);
                        memberIdNameDic.Add(formationDto.Id, playerDto.Name);
                        formationDic.Add(formationDto.Id, formationDto);
                        attackButtonDic.Add(formationDto.Id, btnFormation);

                        formationIdList.Add(formationDto.Id);

                        formationButtonDic.Add(formationDto.Id, btnFormation);

                        wrapPanel.Children.Add(btnFormation);
                    }

                    gbpDto.Formation = gbpFormation;
                    gbpDto.FormationIdName = gbpFormationIdName;

                    guildBPList.Add(gbpDto);

                    memberAttackButtonDic.Add(playerDto.Name, formationIdList);

                    wrapPanel.MouseLeftButtonUp += (sender, e) => FormationWrapPanel_MouseLeftButtonUp(sender);

                    GridMemberArea1.AddChild(wrapPanel, cnt, 3, 1, 1);

                    cnt++;
                }
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }

        private void UpdateDefenseTimeFromStamina()
        {
            try
            {
                Dispatcher.Invoke(() =>
                {
                    int stack1 = (int)(guildStamina * DEFENSE_TIME);
                    int stack2 = (int)(guildStamina * DEFENSE_TIME) / 2;
                    int stack3 = (int)(guildStamina * DEFENSE_TIME) / 3;

                    TimeSpan span1 = new TimeSpan(0, 0, stack1);
                    TimeSpan span2 = new TimeSpan(0, 0, stack2);
                    TimeSpan span3 = new TimeSpan(0, 0, stack3);

                    Labstack1.Content = stack1 + " 秒" + " / " + span1.ToString(@"h\時\間m\分s\秒");
                    Labstack2.Content = stack2 + " 秒" + " / " + span2.ToString(@"m\分s\秒");
                    Labstack3.Content = stack3 + " 秒" + " / " + span3.ToString(@"m\分s\秒");

                    discordP.Labstack1 = Labstack1.Content.ToString();
                    discordP.Labstack2 = Labstack2.Content.ToString();
                    discordP.Labstack3 = Labstack3.Content.ToString();
                });
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

        private void PlayerGrid_MouseLeftButtonUp(object sender)
        {
            try
            {
                Grid gird = (Grid)sender;
                gird.Background = new SolidColorBrush(NOT_SELECT_COLOR);
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }

        private void LabPStamina_MouseLeftButtonUp(object sender)
        {
            try
            {
                Label label = (Label)sender;
                label.Background = new SolidColorBrush(NOT_SELECT_COLOR);
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }

        private void FormationWrapPanel_MouseLeftButtonUp(object sender)
        {
            try
            {
                WrapPanel wrapPanel = (WrapPanel)sender;
                wrapPanel.Background = new SolidColorBrush(NOT_SELECT_COLOR);
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }

        public string CheckAttackCoordination(int formationId)
        {
            string result = string.Empty;

            try
            {
                if (!attackButtonDic[formationId].IsEnabled)
                {
                    result = "オフラインのため侵攻できません。";
                }
                else if (memberAttackIdDic[formationId] == 0)
                {
                    result = "この編成は使い切りました。";
                }
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }

            return result;
        }

        public void FormationButtonExecute(int formationId)
        {
            try
            {
                Button button = formationButtonDic[formationId];

                if (null == button)
                {
                    MessageBox.Show(formationId + "が見つかりませんでした。");
                    return;
                }

                int stamina = formationDic[formationId].Characters.Length;

                if (formation30KOList.Contains(formationId))
                {
                    stamina *= 6;
                }

                GuildBattleProgressDto gbpDto = guildBPList.Where(x => x.Name.Equals(memberIdNameDic[formationId])).FirstOrDefault();
                int addBit = 0;

                if (memberAttackIdDic[formationId] > 0)
                {
                    memberAttackIdDic[formationId]--;
                    guildStamina -= stamina;
                    memberStaminaDic[memberIdNameDic[formationId]] -= stamina;

                    if (memberAttackIdDic[formationId] == 1)
                    {
                        Dispatcher.Invoke(() => { button.Background = new SolidColorBrush(FST_HALF_COLOR); });
                    }
                    else
                    {
                        Dispatcher.Invoke(() => { button.Background = new SolidColorBrush(FST_ZERO_COLOR); });
                    }

                    addBit = -1;
                }
                else
                {
                    // 間違えて押してしまったことを考えてリセット扱いにする
                    memberAttackIdDic[formationId] = 2;
                    guildStamina += stamina * 2;
                    memberStaminaDic[memberIdNameDic[formationId]] += stamina * 2;

                    Dispatcher.Invoke(() => { button.Background = new SolidColorBrush(FST_MAX_COLOR); });

                    addBit = 2;
                }

                UpdateDefenseTimeFromStamina();

                Dispatcher.Invoke(() =>
                {
                    Label label = memberStaminaLabDic[memberIdNameDic[formationId]];
                    int bit = 0;

                    label.Content = memberStaminaDic[memberIdNameDic[formationId]] + " / " + memberStaminaMaxDic[memberIdNameDic[formationId]];

                    gbpDto.Stamina = label.Content.ToString();

                    bit = gbpDto.Formation[button.Content.ToString()] + addBit;

                    gbpDto.Formation.Remove(button.Content.ToString());

                    gbpDto.FormationIdName.Remove(button.Content.ToString());

                    button.Content = memberAttackIdDic[formationId] + "/2 " + memberAttackNameDic[formationId] + Common.Common.GetMaxSpeedRuneText(formationDic[formationId]);
                    
                    gbpDto.Formation.Add(button.Content.ToString(), bit);
                    gbpDto.FormationIdName.Add(button.Content.ToString(), formationId);
                    
                    LabMemberOnlineST.Content = guildStamina + " / " + guildMaxStamina;
                    
                    discordP.LabMemberOnlineST = LabMemberOnlineST.Content.ToString();
                });

                SortStaminaValue();
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

                FormationButtonExecute(formationId);
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }

        private void Online_Click(object sender)
        {
            try
            {
                Button button = (Button)sender;

                bool isOnline = true;

                if ("オンライン".Equals(button.Content))
                {
                    button.Content = "オフライン";
                    button.Background = new SolidColorBrush(OFFLINE_COLOR);
                    onlineNum--;
                    guildMaxStamina -= memberStaminaMaxDic[button.ToolTip.ToString()];
                    guildStamina -= memberStaminaDic[button.ToolTip.ToString()];
                    isOnline = false;
                    Label label = memberStaminaLabDic[button.ToolTip.ToString()];
                    label.Content = "0 / " + memberStaminaMaxDic[button.ToolTip.ToString()];

                    GuildBattleProgressDto gbpDto = guildBPList.Where(x => x.Name.Equals(button.ToolTip.ToString())).FirstOrDefault();
                    gbpDto.Stamina = label.Content.ToString();
                    gbpDto.IsOnline = false;
                }
                else if ("オフライン".Equals(button.Content))
                {
                    button.Content = "オンライン";
                    button.Background = new SolidColorBrush(ONLINE_COLOR);
                    onlineNum++;
                    guildMaxStamina += memberStaminaMaxDic[button.ToolTip.ToString()];
                    guildStamina += memberStaminaDic[button.ToolTip.ToString()];
                    Label label = memberStaminaLabDic[button.ToolTip.ToString()];
                    label.Content = memberStaminaDic[button.ToolTip.ToString()] + " / " + memberStaminaMaxDic[button.ToolTip.ToString()];

                    GuildBattleProgressDto gbpDto = guildBPList.Where(x => x.Name.Equals(button.ToolTip.ToString())).FirstOrDefault();
                    gbpDto.Stamina = label.Content.ToString();
                    gbpDto.IsOnline = true;
                }

                UpdateDefenseTimeFromStamina();

                List<int> ids = memberAttackButtonDic[button.ToolTip.ToString()];

                foreach (int id in ids)
                {
                    attackButtonDic[id].IsEnabled = isOnline;
                }

                LabMemberOnline.Content = onlineNum + " / " + onlineMaxNum;

                discordP.LabMemberOnline = LabMemberOnline.Content.ToString();

                LabMemberOnlineST.Content = guildStamina + " / " + guildMaxStamina;

                discordP.LabMemberOnlineST = LabMemberOnlineST.Content.ToString();

                SortStaminaValue();
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }

        private void CombGuild_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                SelectGuildName = CombGuild.SelectedItem.ToString();

                if (CheckDetailSetting.IsChecked.Value)
                {
                    GuildVSDetailSetting guildVSDetailSetting = new GuildVSDetailSetting(
                                                                        SelectGuildName,
                                                                        new List<GuildVSDetailMemberDto>());

                    guildVSDetailSetting.ShowDialog();

                    foreach (GuildVSDetailMemberDto member in guildVSDetailSetting.GuildVSDetailMemberDtos)
                    {
                        if (member.VirtualityFormation > 0)
                        {
                            virtualityFormationTargetDic.Add(member.Id, member.VirtualityFormation);
                        }

                        if (member.VFCNum > 0)
                        {
                            virtualityFormationCharaNumTargetDic.Add(member.Id, member.VFCNum);
                        }

                        updateGBGroupIdDic.Add(member.Id, member.GBGroupId);
                    }

                    foreach (int id in guildVSDetailSetting.SelectedGroupIdList)
                    {
                        selectedGroupIdList.Add(id);
                    }
                }
                else
                {
                    // 既存の仮想編成をロード
                    PlayerDao playerDao = new PlayerDao();
                    PlayerDto[] playerDtos = playerDao.GetPlayerDtos(SelectGuildName);

                    foreach (PlayerDto playerDto in playerDtos)
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
                }

                LoadMemberArea(SelectGuildName);
                CombGuild.IsEnabled = false;
                CheckDebuff30KO.IsEnabled = false;
                CheckDetailSetting.IsEnabled = false;

                LoadTargetGuildItems();
                CombTargetGuild.IsEnabled = true;
                MSCombTargetGuild.IsEnabled = true;
                MSCombGuildFV.IsEnabled = true;
                MSCombGuildVSF.IsEnabled = true;
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
                Dispatcher.Invoke(() =>
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

                        Dictionary<long, int> sortTargetGridIndexDic = new Dictionary<long, int>();

                        foreach (var pair in sorted)
                        {
                            max = cnt + GRID_MAIN_COLUMN;
                            int idx = pair.Key;
                            int columnIndex = 0;

                            if (targetGridIndexDic.ContainsValue(idx))
                            {
                                sortTargetGridIndexDic.Add(targetGridIndexDic.FirstOrDefault(x => x.Value == idx).Key, rowIdx);
                            }

                            for (int i = cnt; i < max; i++)
                            {
                                Grid.SetRow(GridMemberArea1.Children[idx], rowIdx);
                                Grid.SetColumn(GridMemberArea1.Children[idx], columnIndex);

                                idx++;
                                columnIndex++;
                            }

                            rowIdx++;
                        }

                        targetGridIndexDic = sortTargetGridIndexDic;
                    }
                });
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

        void TimerTick(object sender, EventArgs e)
        {
            TextTimeBlock.Text = DateTime.Now.ToString("HH:mm:ss.fff");
        }

        void TimerTick2(object sender, EventArgs e)
        {
            TimeSpan endTS = new TimeSpan(1, 21, 30, 0);
            TimeSpan nowTS = new TimeSpan(1, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);
            TimeSpan RemainTS = endTS - nowTS - SERVER_CORRECTION_TIME;

            if (RemainTS > before15minSTS && RemainTS < before15minETS)
            {
                TextTimeBlockVSTime.Background = new SolidColorBrush(BEFORE_15MIN_VS_COLOR);
            }
            else if (RemainTS > vsSTS && RemainTS < vsETS)
            {
                TextTimeBlockVSTime.Background = new SolidColorBrush(VS_COLOR);
            }
            else
            {
                TextTimeBlockVSTime.Background = null;
            }

            TextTimeBlockVSTime.Text = RemainTS.ToString(@"hh\:mm\:ss");
        }

        private void CombTargetGuild_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                TargetGuildAdmin targetGuildAdmin = new TargetGuildAdmin(
                                                        this, CombTargetGuild.SelectedItem.ToString(), SelectGuildName
                                                    );
                targetGuildAdmin.Show();
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }

        public void SelectGridMemberArea(long playerId)
        {
            try
            {
                if (targetGridIndexDic.ContainsKey(playerId))
                {
                    int gridIndex = targetGridIndexDic[playerId];

                    int cnt = 1;
                    int index = 1;
                    int max = GridMemberArea1.Children.Count;

                    for (int i = GRID_MAIN_COLUMN; i < max; i++)
                    {
                        UIElement element = GridMemberArea1.Children[i];

                        if (index == gridIndex)
                        {
                            switch (cnt)
                            {
                                case 1:
                                    //Button button = (Button)element;
                                    break;
                                case 2:
                                    Grid gird = (Grid)element;
                                    gird.Background = new SolidColorBrush(SELECT_COLOR);
                                    break;
                                case 3:
                                    //Label label = (Label)element;
                                    //label.Background = new SolidColorBrush(SELECT_COLOR);
                                    break;
                                case 4:
                                    //WrapPanel wrapPanel = (WrapPanel)element;
                                    //wrapPanel.Background = new SolidColorBrush(SELECT_COLOR);
                                    break;
                            }
                        }

                        cnt++;

                        if (cnt == 5)
                        {
                            cnt = 1;
                            index++;
                        }
                    }
                }
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
                    if (!AssemblyState.IsDebug)
                    {
                        if (string.IsNullOrEmpty(authDto.LoginId))
                        {
                            MessageBox.Show("Discord連携するには、ログイン時にDiscordのユーザーIDを入力してください。");
                            return;
                        }
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

    public static class GridExtensions
    {
        public static Grid AddChild(this Grid grid, UIElement element, int rowIndex, int columnIndex, int rowSpan, int columnSpan)
        {
            if (rowSpan < 1)
            {
                throw new ArgumentOutOfRangeException("rowSpan");
            }

            if (columnSpan < 1)
            {
                throw new ArgumentOutOfRangeException("columnSpan");
            }

            grid.Children.Add(element);

            Grid.SetRow(element, rowIndex);
            Grid.SetColumn(element, columnIndex);

            Grid.SetRowSpan(element, rowSpan);
            Grid.SetColumnSpan(element, columnSpan);

            return grid;
        }
    }
}
