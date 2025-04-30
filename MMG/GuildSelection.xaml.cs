using log4net;
using MMG.Dao;
using MMG.Dto;
using System;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

namespace MMG
{
    /// <summary>
    /// GuildSelection.xaml の相互作用ロジック
    /// </summary>
    public partial class GuildSelection : Window
    {
        private readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().Name);

        public string SelectedGuildName { get; set; } = string.Empty;
        public string SelectedGuildMapSize { get; set; } = string.Empty;

        public GuildSelection()
        {
            InitializeComponent();
            LoadGuildItems();
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

                CombGuildMapScale.Items.Add("1240*1140"); // 実際は元画像の1/2
                CombGuildMapScale.Items.Add("930*855");
                CombGuildMapScale.Items.Add("620*570");

                CombGuildMapScale.SelectedIndex = 0;

                SelectedGuildMapSize = "1240*1140";
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
                SelectedGuildName = CombGuild.SelectedItem.ToString();
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }

        private void BtnSet_Click(object sender, RoutedEventArgs e)
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

        private void CombGuildMapScale_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                SelectedGuildMapSize = CombGuildMapScale.SelectedItem.ToString();
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }
    }
}
