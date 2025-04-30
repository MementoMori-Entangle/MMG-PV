using log4net;
using MMG.Dao;
using MMG.Dto;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace MMG
{
    /// <summary>
    /// GuildStatistics.xaml の相互作用ロジック
    /// </summary>
    public partial class GuildStatistics : Window
    {
        private readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().Name);

        private DataTable statisticsDT;

        public GuildStatistics()
        {
            InitializeComponent();
        }

        public GuildStatistics(string guildA, string guildB) : this()
        {
            DgStatistics.Columns.Add(
                new DataGridTextColumn()
                {
                    Header = "ギルド名",
                    IsReadOnly = true,
                    Binding = new Binding("GuildName")
                }
            );

            DgStatistics.Columns.Add(
                new DataGridTextColumn()
                {
                    Header = "プレイヤー数",
                    IsReadOnly = true,
                    Binding = new Binding("PlayerNum")
                }
            );

            DgStatistics.Columns.Add(
                new DataGridTextColumn()
                {
                    Header = "キャラ数",
                    IsReadOnly = true,
                    Binding = new Binding("CharaNum")
                }
            );

            DgStatistics.Columns.Add(
                new DataGridTextColumn()
                {
                    Header = "主力編成数",
                    IsReadOnly = true,
                    Binding = new Binding("MainNum")
                }
            );

            DgStatistics.Columns.Add(
                new DataGridTextColumn()
                {
                    Header = "ルサルカ使用数",
                    IsReadOnly = true,
                    Binding = new Binding("RusarukaNum")
                }
            );

            DgStatistics.Columns.Add(
                new DataGridTextColumn()
                {
                    Header = "オフィーリア使用数",
                    IsReadOnly = true,
                    Binding = new Binding("OfuiriaNum")
                }
            );

            DgStatistics.Columns.Add(
                new DataGridTextColumn()
                {
                    Header = "回避編成使用数",
                    IsReadOnly = true,
                    Binding = new Binding("KaihiNum")
                }
            );

            DgStatistics.Columns.Add(
                new DataGridTextColumn()
                {
                    Header = "コルディ使用数",
                    IsReadOnly = true,
                    Binding = new Binding("KorudeiNum")
                }
            );

            DgStatistics.Columns.Add(
                new DataGridTextColumn()
                {
                    Header = "フローレンス使用数",
                    IsReadOnly = true,
                    Binding = new Binding("FurorensuNum")
                }
            );

            DgStatistics.Columns.Add(
                new DataGridTextColumn()
                {
                    Header = "サブ編成数",
                    IsReadOnly = true,
                    Binding = new Binding("SubNum")
                }
            );

            LoadGuildList(guildA, guildB);

            List<string> guildList = new List<string>();

            if (!string.IsNullOrEmpty(guildA))
            {
                guildList.Add(guildA);
            }

            if (!string.IsNullOrEmpty(guildB))
            {
                guildList.Add(guildB);
            }

            LoadDgStatistics(guildList);
        }

        private void LoadGuildList(string guildA = null, string guildB = null)
        {
            try
            {
                ListGuild.SelectedItems.Clear();

                PlayerDao playerDao = new PlayerDao();

                ListGuild.ItemsSource = playerDao.GetGuildNameItem();

                if (!string.IsNullOrEmpty(guildA))
                {
                    ListGuild.SelectedItems.Add(guildA);
                }

                if (!string.IsNullOrEmpty(guildB))
                {
                    ListGuild.SelectedItems.Add(guildB);
                }
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }

        private void LoadDgStatistics(List<string> guildList)
        {
            try
            {
                statisticsDT = new DataTable("GuildStatistics");
                statisticsDT.Columns.Add(new DataColumn("GuildName", typeof(string)));
                statisticsDT.Columns.Add(new DataColumn("PlayerNum", typeof(string)));
                statisticsDT.Columns.Add(new DataColumn("CharaNum", typeof(string)));
                statisticsDT.Columns.Add(new DataColumn("MainNum", typeof(string)));
                statisticsDT.Columns.Add(new DataColumn("RusarukaNum", typeof(string)));
                statisticsDT.Columns.Add(new DataColumn("OfuiriaNum", typeof(string)));
                statisticsDT.Columns.Add(new DataColumn("KaihiNum", typeof(string)));
                statisticsDT.Columns.Add(new DataColumn("KorudeiNum", typeof(string)));
                statisticsDT.Columns.Add(new DataColumn("FurorensuNum", typeof(string)));
                statisticsDT.Columns.Add(new DataColumn("SubNum", typeof(string)));

                GuildMemberDao guildMemberDao = new GuildMemberDao();

                foreach (string name in guildList)
                {
                    GuildDto guildDto = guildMemberDao.GetGuildMembers(name);

                    int playerCnt = guildDto.MemberNum;
                    int charaCnt = 0;
                    int charaVFCCnt = 0;
                    int mainCnt = 0;
                    int rusarukaCnt = 0;
                    int rusarukaLR5Cnt = 0;
                    int ofuiriaCnt = 0;
                    int ofuiriaLR5Cnt = 0;
                    int kaihiCnt = 0;
                    int korudeiCnt = 0;
                    int furorensuCnt = 0;
                    int subCnt = 0;

                    foreach (PlayerDto playerDto in guildDto.Members)
                    {
                        charaVFCCnt += playerDto.VFCNum;

                        foreach (FormationDto formationDto in playerDto.Formations)
                        {
                            charaCnt += formationDto.Characters.Length;

                            if (!formationDto.IsSubParty && !formationDto.IsDebuff)
                            {
                                mainCnt++;
                            }

                            if (formationDto.IsSubParty)
                            {
                                subCnt++;
                            }

                            if (formationDto.Characters.Where(x => x.Id == 1106 && x.Id == 1204).Any())
                            {
                                kaihiCnt++;
                            }

                            foreach (CharacterDto characterDto in formationDto.Characters)
                            {
                                if (characterDto.Id == 1404)
                                {
                                    rusarukaCnt++;

                                    if (characterDto.Rarity > 13)
                                    {
                                        rusarukaLR5Cnt++;
                                    }
                                }

                                if (characterDto.Id == 1504)
                                {
                                    ofuiriaCnt++;

                                    if (characterDto.Rarity > 13)
                                    {
                                        ofuiriaLR5Cnt++;
                                    }
                                }

                                if (characterDto.Id == 1204)
                                {
                                    kaihiCnt++;
                                }

                                if (characterDto.Id == 1203)
                                {
                                    korudeiCnt++;
                                }

                                if (characterDto.Id == 1004)
                                {
                                    furorensuCnt++;
                                }
                            }
                        }
                    }

                    DataRow newRowItem = statisticsDT.NewRow();

                    newRowItem["GuildName"] = guildDto.Name;
                    newRowItem["PlayerNum"] = playerCnt;
                    newRowItem["CharaNum"] = charaCnt + "(" + charaVFCCnt + ")";
                    newRowItem["MainNum"] = mainCnt;
                    newRowItem["RusarukaNum"] = rusarukaCnt + "(" + rusarukaLR5Cnt + ")";
                    newRowItem["OfuiriaNum"] = ofuiriaCnt + "(" + ofuiriaLR5Cnt + ")";
                    newRowItem["KaihiNum"] = kaihiCnt;
                    newRowItem["KorudeiNum"] = korudeiCnt;
                    newRowItem["FurorensuNum"] = furorensuCnt;
                    newRowItem["SubNum"] = subCnt;

                    statisticsDT.Rows.Add(newRowItem);
                }

                DgStatistics.DataContext = statisticsDT;
                DgStatistics.Items.Refresh();
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }

        private void BtnStatisticsView_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ListGuild.SelectedItems.Count == 0)
                {
                    ListGuild.Items.Clear();
                    return;
                }

                List<string> guildList = new List<string>();

                foreach (string guildName in ListGuild.SelectedItems)
                {
                    guildList.Add(guildName);
                }

                LoadDgStatistics(guildList);
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }
    }
}
