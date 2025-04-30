using log4net;
using MMG.Common;
using MMG.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MMG
{
    /// <summary>
    /// CharacterSelection.xaml の相互作用ロジック
    /// </summary>
    public partial class CharacterSelection : Window
    {
        private readonly int MAX_LINE_CHARACTER = 10;
        private readonly int IMAGE_WIDTH = 50;
        private readonly int IMAGE_HEIGHT = 50;
        private readonly int ADD_MARGIN = 60;

        private readonly string imgCharaPath = Common.Common.FormationCharaImagePath;
        private readonly string imgExt = Common.Common.FormationImageExtension;

        private readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().Name);

        private bool isDrag = false;
        private Point dragOffset;

        public FormationDto FormationDto { get; set; }

        public CharacterSelection(FormationDto formationDto)
        {
            InitializeComponent();
            LoadCombBoxRarity();
            FormationDto = formationDto;
            LoadCharacter();
        }

        private void LoadCombBoxRarity()
        {
            try
            {
                foreach (string rarity in CharacterDto.RARITY)
                {
                    CombR1.Items.Add(rarity);
                    CombR2.Items.Add(rarity);
                    CombR3.Items.Add(rarity);
                    CombR4.Items.Add(rarity);
                    CombR5.Items.Add(rarity);
                }
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }

        private void LoadCharacter()
        {
            try
            {
                List<string> selectedCharaNames = new List<string>();

                if (null != FormationDto)
                {
                    if (FormationDto.Characters.Length > 0)
                    {
                        Img1.Source = new BitmapImage(new Uri(imgCharaPath + FormationDto.Characters[0].Name + imgExt));
                        Img1.ToolTip = FormationDto.Characters[0].Name;
                        selectedCharaNames.Add(FormationDto.Characters[0].Name);
                        CombR1.SelectedIndex = FormationDto.Characters[0].Rarity;
                        TextLevel1.Text = FormationDto.Characters[0].Level.ToString();
                    }
                    else
                    {
                        Img1.Source = new BitmapImage(new Uri(imgCharaPath + Common.Common.FormationImageNothing + imgExt));
                        Img1.ToolTip = string.Empty;
                        CombR1.SelectedIndex = -1;
                        TextLevel1.Text = string.Empty;
                    }

                    if (FormationDto.Characters.Length > 1)
                    {
                        Img2.Source = new BitmapImage(new Uri(imgCharaPath + FormationDto.Characters[1].Name + imgExt));
                        Img2.ToolTip = FormationDto.Characters[1].Name;
                        selectedCharaNames.Add(FormationDto.Characters[1].Name);
                        CombR2.SelectedIndex = FormationDto.Characters[1].Rarity;
                        TextLevel2.Text = FormationDto.Characters[1].Level.ToString();
                    }
                    else
                    {
                        Img2.Source = new BitmapImage(new Uri(imgCharaPath + Common.Common.FormationImageNothing + imgExt));
                        Img2.ToolTip = string.Empty;
                        CombR2.SelectedIndex = -1;
                        TextLevel2.Text = string.Empty;
                    }

                    if (FormationDto.Characters.Length > 2)
                    {
                        Img3.Source = new BitmapImage(new Uri(imgCharaPath + FormationDto.Characters[2].Name + imgExt));
                        Img3.ToolTip = FormationDto.Characters[2].Name;
                        selectedCharaNames.Add(FormationDto.Characters[2].Name);
                        CombR3.SelectedIndex = FormationDto.Characters[2].Rarity;
                        TextLevel3.Text = FormationDto.Characters[2].Level.ToString();
                    }
                    else
                    {
                        Img3.Source = new BitmapImage(new Uri(imgCharaPath + Common.Common.FormationImageNothing + imgExt));
                        Img3.ToolTip = string.Empty;
                        CombR3.SelectedIndex = -1;
                        TextLevel3.Text = string.Empty;
                    }

                    if (FormationDto.Characters.Length > 3)
                    {
                        Img4.Source = new BitmapImage(new Uri(imgCharaPath + FormationDto.Characters[3].Name + imgExt));
                        Img4.ToolTip = FormationDto.Characters[3].Name;
                        selectedCharaNames.Add(FormationDto.Characters[3].Name);
                        CombR4.SelectedIndex = FormationDto.Characters[3].Rarity;
                        TextLevel4.Text = FormationDto.Characters[3].Level.ToString();
                    }
                    else
                    {
                        Img4.Source = new BitmapImage(new Uri(imgCharaPath + Common.Common.FormationImageNothing + imgExt));
                        Img4.ToolTip = string.Empty;
                        CombR4.SelectedIndex = -1;
                        TextLevel4.Text = string.Empty;
                    }

                    if (FormationDto.Characters.Length > 4)
                    {
                        Img5.Source = new BitmapImage(new Uri(imgCharaPath + FormationDto.Characters[4].Name + imgExt));
                        Img5.ToolTip = FormationDto.Characters[4].Name;
                        selectedCharaNames.Add(FormationDto.Characters[4].Name);
                        CombR5.SelectedIndex = FormationDto.Characters[4].Rarity;
                        TextLevel5.Text = FormationDto.Characters[4].Level.ToString();
                    }
                    else
                    {
                        Img5.Source = new BitmapImage(new Uri(imgCharaPath + Common.Common.FormationImageNothing + imgExt));
                        Img5.ToolTip = string.Empty;
                        CombR5.SelectedIndex = -1;
                        TextLevel5.Text = string.Empty;
                    }
                }

                // グリッドに最大キャラクター分のデータをロード
                BaseCharacterDto[] baseCharacterDtos = BaseCharacter.GetBaseCharacterDtos();

                int charaCnt = 1;
                int top = 10;
                int left = 10;
                int right = 0;
                int bottom = 0;

                foreach (BaseCharacterDto baseCharacterDto in baseCharacterDtos)
                {
                    Thickness margin = new Thickness(left, top, right, bottom);

                    Image image = new Image
                    {
                        Source = new BitmapImage(new Uri(imgCharaPath + baseCharacterDto.Name + imgExt)),
                        Name = "ImgList" + charaCnt,
                        ToolTip = baseCharacterDto.Name,
                        HorizontalAlignment = HorizontalAlignment.Left,
                        Width = IMAGE_WIDTH,
                        Height = IMAGE_HEIGHT,
                        Margin = margin,
                        VerticalAlignment = VerticalAlignment.Top
                    };

                    if (selectedCharaNames.Contains(baseCharacterDto.Name))
                    {
                        BitmapImage bmp = new BitmapImage(new Uri(imgCharaPath + baseCharacterDto.Name + imgExt));

                        DrawingGroup drawingGroup = new DrawingGroup();

                        using (DrawingContext drawContent = drawingGroup.Open())
                        {
                            // 画像を書いて、その上にテキストを書く
                            drawContent.DrawImage(bmp, new Rect(0, 0, bmp.PixelWidth, bmp.PixelHeight));
                            drawContent.DrawText(new FormattedText("選択済", System.Globalization.CultureInfo.CurrentUICulture,
                                                                             FlowDirection.LeftToRight,
                                                                             new Typeface("Verdana"), 30, Brushes.Red,
                                                                             VisualTreeHelper.GetDpi(this).PixelsPerDip),
                                                                             new Point(5, 10));
                        }

                        image.Source = new DrawingImage(drawingGroup);
                    }

                    image.MouseLeftButtonUp += new MouseButtonEventHandler(MouseLeftButtonUp_Click);

                    GridCharacter.Children.Add(image);

                    left += ADD_MARGIN;

                    if (charaCnt >= MAX_LINE_CHARACTER && charaCnt % MAX_LINE_CHARACTER == 0)
                    {
                        top += ADD_MARGIN;
                        left = 10;
                    }

                    charaCnt++;
                }
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }

        private void ClearFormationCharacter(string name)
        {
            try
            {
                FormationDto.Characters = FormationDto.Characters.Where(x => x.Name != name).ToArray();
                LoadCharacter();
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }

        private void MouseLeftButtonUp_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Image image = (Image)sender;

                string name = image.ToolTip.ToString();
                bool isFormationAdd = false;
                CharacterDto selectedCharacterDto = null;

                BaseCharacterDto[] baseCharacterDtos = BaseCharacter.GetBaseCharacterDtos();

                BaseCharacterDto baseCharacterDto = baseCharacterDtos.Where(x => x.Name == name).FirstOrDefault();

                // 選択済みの場合は編成解除し、アイコンの再描画
                if (null != FormationDto && FormationDto.Characters.Length > 0)
                {
                    selectedCharacterDto = FormationDto.Characters.Where(x => x.Name == name).FirstOrDefault();

                    if (null != selectedCharacterDto)
                    {
                        ClearFormationCharacter(name);
                    }
                    else
                    {
                        // 未選択の場合は、編成に空きがある場合は追加する
                        if (FormationDto.Characters.Length < 5)
                        {
                            isFormationAdd = true;

                            int level = 160;

                            if (baseCharacterDto.Rarity > 1)
                            {
                                level = FormationDto.Characters.Where(x => x.Name != name).FirstOrDefault().Level;
                            }

                            selectedCharacterDto = new CharacterDto() { Level = level };
                        }
                    }
                }
                else
                {
                    isFormationAdd = true;
                    selectedCharacterDto = new CharacterDto();
                }

                if (isFormationAdd)
                {
                    CharacterDto characterDto = new CharacterDto
                    {
                        Id = baseCharacterDto.Id,
                        Name = baseCharacterDto.Name,
                        Attribute = baseCharacterDto.Attribute,
                        Speed = baseCharacterDto.Speed,
                        Rarity = baseCharacterDto.Rarity,
                        Level = selectedCharacterDto.Level
                    };

                    List<CharacterDto> list = FormationDto.Characters.ToList();

                    list.Add(characterDto);

                    FormationDto.Characters = list.ToArray();

                    LoadCharacter();
                }
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }

        private void ClearSelectedFormation(string name)
        {
            try
            {
                // キャラクターが設定されている場合は編成から解除し、アイコンの再描画
                if (null != FormationDto && FormationDto.Characters.Length > 0)
                {
                    if (null != name)
                    {
                        CharacterDto selectedCharacterDto = FormationDto.Characters.Where(x => x.Name == name).FirstOrDefault();

                        if (null != selectedCharacterDto)
                        {
                            ClearFormationCharacter(name);
                        }
                    }
                    else
                    {
                        FormationDto.Characters = FormationDto.Characters.Where(x => x.Id == -999).ToArray();
                        LoadCharacter();
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }

        private void Img1_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Image image = (Image)sender;

                if (null != image.ToolTip)
                {
                    ClearSelectedFormation(image.ToolTip.ToString());
                }
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }

        private void Img2_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Image image = (Image)sender;

                if (null != image.ToolTip)
                {
                    ClearSelectedFormation(image.ToolTip.ToString());
                }
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }

        private void Img3_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Image image = (Image)sender;

                if (null != image.ToolTip)
                {
                    ClearSelectedFormation(image.ToolTip.ToString());
                }
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }

        private void Img4_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Image image = (Image)sender;

                if (null != image.ToolTip)
                {
                    ClearSelectedFormation(image.ToolTip.ToString());
                }
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }

        private void Img5_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Image image = (Image)sender;

                if (null != image.ToolTip)
                {
                    ClearSelectedFormation(image.ToolTip.ToString());
                }
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

        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ClearSelectedFormation(null);
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }

        private void Formation_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is UIElement el)
            {
                isDrag = true;
                dragOffset = e.GetPosition(el);
                el.CaptureMouse();
            }
        }

        private void Formation_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (isDrag)
            {
                UIElement el = sender as UIElement;
                el.ReleaseMouseCapture();
                isDrag = false;
            }
        }

        private void Formation_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDrag)
            {
                Point pt = Mouse.GetPosition(CanvasFormation);
                UIElement el = sender as UIElement;
                Canvas.SetLeft(el, pt.X - dragOffset.X);
                Canvas.SetTop(el, pt.Y - dragOffset.Y);
            }
        }

        private void CombR1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                FormationDto.Characters[0].Rarity = Common.Common.GetRarity(CombR1.SelectedItem.ToString());
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }

        private void CombR2_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                FormationDto.Characters[1].Rarity = Common.Common.GetRarity(CombR2.SelectedItem.ToString());
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }

        private void CombR3_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                FormationDto.Characters[2].Rarity = Common.Common.GetRarity(CombR3.SelectedItem.ToString());
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }

        private void CombR4_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                FormationDto.Characters[3].Rarity = Common.Common.GetRarity(CombR4.SelectedItem.ToString());
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }

        private void CombR5_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                FormationDto.Characters[4].Rarity = Common.Common.GetRarity(CombR5.SelectedItem.ToString());
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }

        private void TextLevel1_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                FormationDto.Characters[0].Level = int.Parse(TextLevel1.Text);
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }

        private void TextLevel2_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                FormationDto.Characters[1].Level = int.Parse(TextLevel2.Text);
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }

        private void TextLevel3_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                FormationDto.Characters[2].Level = int.Parse(TextLevel3.Text);
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }

        private void TextLevel4_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                FormationDto.Characters[3].Level = int.Parse(TextLevel4.Text);
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }

        private void TextLevel5_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                FormationDto.Characters[4].Level = int.Parse(TextLevel5.Text);
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }
    }
}
