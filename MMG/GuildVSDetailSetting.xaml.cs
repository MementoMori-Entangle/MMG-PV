using log4net;
using MMG.Common;
using MMG.Dao;
using MMG.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;

namespace MMG
{
    /// <summary>
    /// GuildVSDetailSetting.xaml の相互作用ロジック
    /// </summary>
    public partial class GuildVSDetailSetting : Window
    {
        private readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().Name);

        public List<GuildVSDetailMemberDto> GuildVSDetailMemberDtos { get; set; }

        public List<int> SelectedGroupIdList { get; set; }

        public GuildVSDetailSetting(string guildName, List<GuildVSDetailMemberDto> guildVSDetailMemberDtos)
        {
            InitializeComponent();
            LabSelectGuildName.Content = guildName;
            GuildVSDetailMemberDtos = guildVSDetailMemberDtos;
            SelectedGroupIdList = new List<int>();
            LoadGuildMemberInfo(guildName);
        }

        private void LoadGuildMemberInfo(string guildName)
        {
            try
            {
                GuildMemberDao guildMemberDao = new GuildMemberDao();

                GuildDto guildDto = guildMemberDao.GetGuildMembers(guildName);

                List<VirtualityFormationDto> vfDtos = new VirtualityFormationDto().GetDefaultList();
                List<GBGroupDto> gbGroups = BaseGBGroup.GetBaseGBGroup().ToList();
                List<GBGroupDto> gbGroups2 = BaseGBGroup.GetBaseGBGroup(1).ToList();

                gbGroups = gbGroups.Concat(gbGroups2).ToList();

                foreach (GBGroupDto dto in gbGroups)
                {
                    if (string.IsNullOrEmpty(dto.Name))
                    {
                        continue;
                    }

                    ListBGroupFilter.Items.Add(dto.Name);
                }

                foreach (PlayerDto playerDto in guildDto.Members)
                {
                    int charaNum = 0;

                    foreach (FormationDto formationDto in playerDto.Formations)
                    {
                        foreach (CharacterDto characterDto in formationDto.Characters)
                        {
                            charaNum++;
                        }
                    }

                    int virtualityFormation = 0;

                    if (playerDto.Formations.Length == 0 || charaNum == 0)
                    {
                        virtualityFormation = playerDto.VirtualityFormation;
                    }

                    GuildVSDetailMemberDto memberDto = new GuildVSDetailMemberDto()
                    {
                        Id = playerDto.Id,
                        Name = playerDto.Name,
                        FormationNum = playerDto.Formations.Length,
                        CharacterNum = charaNum,
                        VirtualityFormation = virtualityFormation,
                        VFCNum = playerDto.VFCNum,
                        GBGroupId = playerDto.GBGroupId
                    };

                    GuildVSDetailMemberDtos.Add(memberDto);
                }

                DgGuildMemberInfo.ItemsSource = GuildVSDetailMemberDtos;

                VFCB.ItemsSource = vfDtos;
                GuildBattleGroup.ItemsSource = gbGroups;
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }

        private void BtnSetting_Click(object sender, RoutedEventArgs e)
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

        private void BtnVFAllClear_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                foreach (GuildVSDetailMemberDto dto in GuildVSDetailMemberDtos)
                {
                    dto.VirtualityFormation = 0;
                }

                DgGuildMemberInfo.Items.Refresh();
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }

        private void BtnGBGAllClear_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                foreach (GuildVSDetailMemberDto dto in GuildVSDetailMemberDtos)
                {
                    dto.GBGroupId = 0;
                }

                DgGuildMemberInfo.Items.Refresh();
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            try
            {
                List<GBGroupDto> gbGroups = BaseGBGroup.GetBaseGBGroup().ToList();
                List<GBGroupDto> gbGroups2 = BaseGBGroup.GetBaseGBGroup(1).ToList();

                gbGroups = gbGroups.Concat(gbGroups2).ToList();

                foreach (string name in ListBGroupFilter.SelectedItems)
                {
                    GBGroupDto gBGroupDto = gbGroups.Where(x => x.Name.Equals(name)).FirstOrDefault();

                    SelectedGroupIdList.Add(gBGroupDto.Id);
                }
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }
    }
}
