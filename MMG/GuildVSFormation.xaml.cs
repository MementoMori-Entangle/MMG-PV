using log4net;
using Microsoft.Win32;
using MMG.Common;
using MMG.Dao;
using MMG.Dto;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Brushes = System.Windows.Media.Brushes;
using Color = System.Windows.Media.Color;
using Image = System.Windows.Controls.Image;
using Point = OpenCvSharp.Point;
using Window = System.Windows.Window;

namespace MMG
{
    /// <summary>
    /// GuildVSFormation.xaml の相互作用ロジック
    /// </summary>
    public partial class GuildVSFormation : Window
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        private readonly int IMAGE_WIDTH = 50;
        private readonly int IMAGE_HEIGHT = 50;
        private readonly Color KO_COLOR = Color.FromRgb(255, 0, 0);

        private readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().Name);

        private readonly string imgCharaPath = Common.Common.FormationCharaImagePath;
        private readonly string imgExt = Common.Common.FormationImageExtension;
        private readonly string MATCH_TEMP_FILE_NAME = "_match.bmp";

        private string selectAttackGuildName = string.Empty;
        private string selectDefenseGuildName = string.Empty;
        private string captureName;
        private string tempFilePath;
        private double matchThreshold;
        private bool isSpeedView = true;

        private Dictionary<string, Point> characterPointDic;
        private Dictionary<int, Point> playerPointDic;
        private Dictionary<long, int> virtualityFormationAttackDic;
        private Dictionary<long, int> virtualityFormationDefenseDic;
        private List<string> playerNameList;

        private Timer timer;

        private GuildVSAdmin guildVSAdmin;

        public GuildVSFormation()
        {
            InitializeComponent();

            captureName = Common.Common.PROCESS_TITLE_NAME;
            matchThreshold = Common.Common.MATCH_THRESHOLD;
            characterPointDic = new Dictionary<string, Point>();
            playerPointDic = new Dictionary<int, Point>();
            virtualityFormationAttackDic = new Dictionary<long, int>();
            virtualityFormationDefenseDic = new Dictionary<long, int>();
            playerNameList = new List<string>();

            LabNowLoading.Visibility = Visibility.Hidden;

            string[] times = Common.Common.MATCH_AUTO_INTERVAL_TIMES.Split(',');

            foreach (string time in times)
            {
                CombBAutoTime.Items.Add(time);
            }
            
            CombBAutoTime.SelectedIndex = 0;

            LoadDefenseGuildItems();

            string[] wtns = Common.Common.MATCH_WINDOW_TITLE_NAMES.Split(',');

            CombBWTPName.Items.Add("");

            foreach (string name in wtns)
            {
                CombBWTPName.Items.Add(name);
            }
        }

        public GuildVSFormation(GuildVSAdmin guildVSAdmin) : this()
        {
            this.guildVSAdmin = guildVSAdmin;

            if (null != guildVSAdmin && !string.IsNullOrEmpty(guildVSAdmin.SelectGVSFGuildName))
            {
                CombDefenseGuild.SelectedItem = guildVSAdmin.SelectGVSFGuildName;
            }
        }

        private void WindowCapture()
        {
            try
            {
                Dispatcher.Invoke(() =>
                {
                    TextBImageFilePath.Text = string.Empty;
                });

                Bitmap bitmap;

                if (null == CombBWTPName.SelectedItem || string.IsNullOrEmpty(CombBWTPName.SelectedItem.ToString()))
                {
                    bitmap = Capture(captureName, captureName);
                }
                else
                {
                    bitmap = Capture(CombBWTPName.SelectedItem.ToString(), CombBWTPName.SelectedItem.ToString());
                }

                if (null == bitmap)
                {
                    return;
                }

                // マッチングのために一時ファイルとして出力
                string matchTempDirPath;

                if (null == CombBWTPName.SelectedItem || string.IsNullOrEmpty(CombBWTPName.SelectedItem.ToString()))
                {
                    matchTempDirPath = AppDomain.CurrentDomain.BaseDirectory + Common.Common.MATCH_TEMP_DIR;
                }
                else
                {
                    matchTempDirPath = AppDomain.CurrentDomain.BaseDirectory + Common.Common.MATCH_TEMP_DIR
                                     + CombBWTPName.SelectedItem.ToString() + "/";
                }

                if (!Directory.Exists(matchTempDirPath))
                {
                    Directory.CreateDirectory(matchTempDirPath);
                }

                tempFilePath = matchTempDirPath + DateTime.Now.ToString("yyyyMMddHHmmssfff") + MATCH_TEMP_FILE_NAME;

                bitmap.Save(tempFilePath, ImageFormat.Bmp);

                Dispatcher.Invoke(() =>
                {
                    ImgTarget.Source = ToImageSource(bitmap);
                });
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }

        private void BtnCapture_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                WindowCapture();
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }

        private void BtnImageLoad_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                TextBImageFilePath.Text = string.Empty;

                OpenFileDialog dialog = new OpenFileDialog
                {
                    Filter = "画像ファイル (*.bmp)|*.bmp"
                };

                if (dialog.ShowDialog() == true)
                {
                    TextBImageFilePath.Text = dialog.FileName;

                    ImgTarget.Source = new BitmapImage(new Uri(dialog.FileName));
                }
                else
                {
                    ImgTarget.Source = null;
                }
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }

        private void AutoCharacterMatch()
        {
            try
            {
                BtnAutoStop.IsEnabled = true;

                double time = double.Parse(CombBAutoTime.Text);

                timer = new Timer(time * 1000);

                timer.Elapsed += (sender, e) =>
                {
                    WindowCapture();
                    LoadCharacterMatch();
                };

                timer.Start();
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }

        private string[] GetDivImageFilePath(string baseFilePath)
        {
            List<string> resultList = new List<string>();

            try
            {
                string inFilePath;

                if (null == CombBWTPName.SelectedItem || string.IsNullOrEmpty(CombBWTPName.SelectedItem.ToString()))
                {
                    inFilePath = Common.Common.MATCH_TEMP_DIV_DIR_PATH;
                }
                else
                {
                    inFilePath = Common.Common.MATCH_TEMP_DIR_PATH + CombBWTPName.SelectedItem.ToString() + "/"
                               + Common.Common.MATCH_TEMP_DIV_DIR;
                }

                inFilePath += Path.GetFileNameWithoutExtension(baseFilePath);

                resultList.Add(inFilePath + "_1.bmp");
                resultList.Add(inFilePath + "_2.bmp");
                resultList.Add(inFilePath + "_3.bmp");
                resultList.Add(inFilePath + "_4.bmp");

                Bitmap srcImage = new Bitmap(baseFilePath);

                int.TryParse(TextBPointYAdd.Text, out int addY);

                // 画像を切り抜く範囲を指定(偵察情報画面を基準とする)
                int x = 280;
                int y = 210 + addY;
                int width = 1000;
                int height = 80;
                int heightCnt = 0;

                foreach (string filePath in resultList)
                {
                    Rectangle rect = new Rectangle(x, y, width, height);
                    Bitmap destImage = srcImage.Clone(rect, srcImage.PixelFormat);

                    destImage.Save(filePath, ImageFormat.Bmp);
                    destImage.Dispose();

                    y += 95 + heightCnt;
                    heightCnt += 5;
                }

                srcImage.Dispose();
            }
            catch (Exception e)
            {
                throw e;
            }

            return resultList.ToArray();
        }

        private async void LoadCharacterMatch()
        {
            try
            {
                Dispatcher.Invoke(() =>
                {
                    if (null == ImgTarget.Source)
                    {
                        MessageBox.Show("対象画像を読み込んでください。");

                        BtnCapture.IsEnabled = true;
                        BtnImageLoad.IsEnabled = true;
                        BtnCharacterMatch.IsEnabled = true;
                        return;
                    }

                    LabNowLoading.Visibility = Visibility.Visible;
                    LabNowLoading.Content = "Now Loading ...";

                    GridAttackFormation1.Children.Clear();
                    GridAttackFormation1.ColumnDefinitions.Clear();
                    GridAttackFormation1.RowDefinitions.Clear();
                    GridAttackFormation2.Children.Clear();
                    GridAttackFormation2.ColumnDefinitions.Clear();
                    GridAttackFormation2.RowDefinitions.Clear();
                    GridAttackFormation3.Children.Clear();
                    GridAttackFormation3.ColumnDefinitions.Clear();
                    GridAttackFormation3.RowDefinitions.Clear();
                    GridAttackFormation4.Children.Clear();
                    GridAttackFormation4.ColumnDefinitions.Clear();
                    GridAttackFormation4.RowDefinitions.Clear();
                });

                await Task.Delay(50);

                Dispatcher.Invoke(() =>
                {
                    // 取り込んだ画像を4編成分の区切り加工して4画像ファイル分とする。(マッチング精度を上げる)
                    string baseFilePath = string.Empty;

                    if (!string.IsNullOrEmpty(TextBImageFilePath.Text))
                    {
                        baseFilePath = TextBImageFilePath.Text;
                    }
                    else if (!string.IsNullOrEmpty(tempFilePath))
                    {
                        baseFilePath = tempFilePath;
                    }

                    if (string.IsNullOrEmpty(baseFilePath))
                    {
                        return;
                    }

                    string matchTempDirPath;

                    if (null == CombBWTPName.SelectedItem || string.IsNullOrEmpty(CombBWTPName.SelectedItem.ToString()))
                    {
                        matchTempDirPath = Common.Common.MATCH_TEMP_DIV_DIR_PATH;
                    }
                    else
                    {
                        matchTempDirPath = Common.Common.MATCH_TEMP_DIR_PATH + CombBWTPName.SelectedItem.ToString() + "/"
                                   + Common.Common.MATCH_TEMP_DIV_DIR;
                    }

                    if (!Directory.Exists(matchTempDirPath))
                    {
                        Directory.CreateDirectory(matchTempDirPath);
                    }

                    string[] filePaths = GetDivImageFilePath(baseFilePath);
                    BaseCharacterDto[] baseCharacterDtos = BaseCharacter.GetBaseCharacterDtos();

                    int no = 0;

                    foreach (string filePath in filePaths)
                    {
                        CharacterMatching(filePath);
                        PlayerMatching(filePath);

                        // 高さがテンプレート画像によって±の誤差が発生するため、範囲内の高さを中間値で固定する。(ソート対策)
                        FixedIntermediateValueWithInHeightRange();

                        // マッチング位置から、キャラクターのグルーピングを行う
                        IOrderedEnumerable<KeyValuePair<string, Point>> sorted = characterPointDic.OrderBy(y => y.Value.Y).ThenBy(x => x.Value.X);

                        Dictionary<int, List<CharacterDto>> characterDic = new Dictionary<int, List<CharacterDto>>();

                        no++;

                        foreach (KeyValuePair<string, Point> keyValuePair in sorted)
                        {
                            if (!characterDic.ContainsKey(no))
                            {
                                characterDic.Add(no, new List<CharacterDto>());
                            }

                            BaseCharacterDto baseCharacterDto = baseCharacterDtos.Where(x => x.Name.Contains(keyValuePair.Key)).FirstOrDefault();

                            CharacterDto characterDto = new CharacterDto()
                            {
                                Id = baseCharacterDto.Id,
                                Name = keyValuePair.Key,
                                Attribute = baseCharacterDto.Attribute,
                                Rarity = baseCharacterDto.Rarity
                            };

                            characterDic[no].Add(characterDto);
                        }

                        LabNowLoading.Content = "Loading No." + no + " completed.";

                        if (!characterDic.ContainsKey(no) || characterDic[no].Count != 5)
                        {
                            // 基本的にフルパーティを対象とする。(稀にガチ装備の2～4編成があるが想定外とする)
                            ListBMsg.Items.Add(filePath + "から5キャラ編成をマッチング出来ませんでした。");
                            continue;
                        }
                        else if (playerNameList.Count == 0)
                        {
                            // プレイヤー名が取得出来なかった場合
                            ListBMsg.Items.Add(filePath + "からプレイヤー名をマッチング出来ませんでした。");
                            continue;
                        }

                        int formationId = Common.Common.SearchFormationId(playerNameList[0],
                                                                          selectDefenseGuildName,
                                                                          characterDic[no].ToArray());

                        if (!string.IsNullOrEmpty(selectAttackGuildName))
                        {
                            LoadAttackFormation(selectAttackGuildName, formationId, no);
                        }
                    }

                    BtnCapture.IsEnabled = true;
                    BtnImageLoad.IsEnabled = true;
                    BtnCharacterMatch.IsEnabled = true;
                });
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }

        private void BtnAutoStop_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                timer.Stop();
                BtnAutoStop.IsEnabled = false;
                BtnCapture.IsEnabled = true;
                BtnImageLoad.IsEnabled = true;
                BtnCharacterMatch.IsEnabled = true;
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }

        private void BtnCharacterMatch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                BtnCapture.IsEnabled = false;
                BtnImageLoad.IsEnabled = false;
                BtnCharacterMatch.IsEnabled = false;

                ListBMsg.Items.Clear();

                if (CheckBAuto.IsChecked.Value)
                {
                    string titleName = Common.Common.PROCESS_TITLE_NAME;

                    Process[] proc = GetProcessesByWindowTitle(titleName);

                    if (proc.Length != 1)
                    {
                        MessageBox.Show(titleName + "ウィンドウが起動されていません。");

                        BtnCapture.IsEnabled = true;
                        BtnImageLoad.IsEnabled = true;
                        BtnCharacterMatch.IsEnabled = true;
                        return;
                    }

                    AutoCharacterMatch();
                }
                else
                {
                    LoadCharacterMatch();
                }
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }

        private void FixedIntermediateValueWithInHeightRange()
        {
            try
            {
                Dictionary<string, Point> newDic = new Dictionary<string, Point>();

                // Pointから範囲内の高さの集合体から平均値を求めて、その値で上書きする。
                foreach (KeyValuePair<string, Point> keyValuePair in characterPointDic)
                {
                    string sd = keyValuePair.Value.Y.ToString().Substring(0, 1) + "." + keyValuePair.Value.Y.ToString().Substring(1);
                    double yd = double.Parse(sd);
                    int y = (int)Math.Ceiling(yd);

                    Point point = characterPointDic[keyValuePair.Key];
                    Point newP = new Point(point.X, y);

                    newDic.Add(keyValuePair.Key, newP);
                }

                characterPointDic = newDic;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        private void PlayerMatching(string imgFilePath)
        {
            var targetMat = new Mat(imgFilePath);

            string pmDirPath = AppDomain.CurrentDomain.BaseDirectory + Common.Common.MATCH_GUILDVS_PLAYER_DIR;

            string[] templateFiles = Directory.GetFiles(pmDirPath, "*", SearchOption.AllDirectories);

            playerPointDic.Clear();
            playerNameList.Clear();

            foreach (var fileName in templateFiles)
            {
                var templateMat = new Mat(fileName);

                var match = Matching(targetMat, templateMat, out var maxPoint);

                if (match < 0)
                {
                    continue;
                }

                MMatching(targetMat, templateMat);

                string playerFileName = Path.GetFileNameWithoutExtension(fileName);

                // Windowsで使用できないファイル名が使用されていた場合に対応
                playerFileName = Common.Common.NGConvertString(playerFileName);

                playerNameList.Add(playerFileName);
                playerPointDic.Add(playerNameList.Count - 1, maxPoint);
            }

            if (CheckBPMView.IsChecked.Value)
            {
                Cv2.ImShow("マッチング結果", targetMat);
                Cv2.WaitKey(0);
            }
        }

        private void CharacterMatching(string imgFilePath)
        {
            var targetMat = new Mat(imgFilePath);

            // テンプレート画像で回してマッチするものを探す
            string pmDirPath = AppDomain.CurrentDomain.BaseDirectory + Common.Common.MATCH_GUILDVS_CHARACTER_DIR;

            string[] templateFiles = Directory.GetFiles(pmDirPath, "*", SearchOption.TopDirectoryOnly);

            characterPointDic.Clear();

            foreach (var fileName in templateFiles)
            {
                var templateMat = new Mat(fileName);

                var match = Matching(targetMat, templateMat, out var maxPoint);

                if (match < 0)
                {
                    continue;
                }

                MMatching(targetMat, templateMat);

                string charaFilePath = fileName.Replace(Common.Common.MATCH_GUILDVS_CHARACTER_DIR, Common.Common.CHARACTER_DIR);

                string charaFileName = Path.GetFileNameWithoutExtension(charaFilePath);

                string[] names = charaFileName.Split('_');

                if (characterPointDic.ContainsKey(names[0]))
                {
                    continue;
                }
                else
                {
                    characterPointDic.Add(names[0], maxPoint);
                }
            }

            if (CheckBCMView.IsChecked.Value)
            {
                Cv2.ImShow("マッチング結果", targetMat);
                Cv2.WaitKey(0);
            }
        }

        private void MMatching(Mat targetMat, Mat templateMat)
        {
            // 検索対象の画像とテンプレート画像
            Mat mat = targetMat;
            Mat temp = templateMat;

            using (Mat result = new Mat())
            {

                // テンプレートマッチ
                Cv2.MatchTemplate(mat, temp, result, TemplateMatchModes.CCoeffNormed);

                // しきい値の範囲に絞る
                Cv2.Threshold(result, result, matchThreshold, 1.0, ThresholdTypes.Tozero);

                while (true)
                {
                    // 類似度が最大/最小となる画素の位置を調べる
                    Point minloc, maxloc;
                    double minval, maxval;
                    Cv2.MinMaxLoc(result, out minval, out maxval, out minloc, out maxloc);

                    if (maxval >= matchThreshold)
                    {

                        // 見つかった場所に赤枠を表示
                        OpenCvSharp.Rect rect = new OpenCvSharp.Rect(maxloc.X, maxloc.Y, temp.Width, temp.Height);
                        Cv2.Rectangle(mat, rect, new Scalar(0, 0, 255), 2);

                        // 見つかった箇所は塗りつぶす
                        OpenCvSharp.Rect outRect;
                        Cv2.FloodFill(result, maxloc, new Scalar(0), out outRect, new Scalar(0.1),
                                    new Scalar(1.0), FloodFillFlags.Link4);
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }

        private double Matching(Mat targetMat, Mat templateMat, out Point matchPoint)
        {
            // 探索画像を二値化
            var targetBinMat = new Mat();
            Cv2.CvtColor(targetMat, targetBinMat, ColorConversionCodes.BGR2GRAY);
            Cv2.Threshold(targetBinMat, targetBinMat, 128, 255, ThresholdTypes.Binary);

            // テンプレ画像を二値化
            var templateBinMat = new Mat();
            Cv2.CvtColor(templateMat, templateBinMat, ColorConversionCodes.BGR2GRAY);
            Cv2.Threshold(templateBinMat, templateBinMat, 128, 255, ThresholdTypes.Binary);

            // マッチング
            var resultMat = new Mat();
            Cv2.MatchTemplate(targetBinMat, templateBinMat, resultMat, TemplateMatchModes.CCoeffNormed);

            // 一番マッチした箇所のマッチ具合（0～1）と、その位置を取得する（画像内でマッチした左上座標）
            Cv2.MinMaxLoc(resultMat, out _, out var maxVal, out _, out matchPoint);

            if (maxVal < matchThreshold)
            {
                return -1.0;
            }

            // 閾値超えのマッチ箇所を強調させておく
            var binMat = new Mat();
            Cv2.Threshold(resultMat, binMat, matchThreshold, 1.0, ThresholdTypes.Binary);

            return maxVal;
        }

        private void LoadAttackFormation(string guildName, int defenseFormationId, int formationNo)
        {
            try
            {
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

                switch (formationNo)
                {
                    case 1:
                        GridAttackFormation1.HorizontalAlignment = HorizontalAlignment.Left;
                        GridAttackFormation1.VerticalAlignment = VerticalAlignment.Top;
                        GridAttackFormation1.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(100) });
                        GridAttackFormation1.ColumnDefinitions.Add(new ColumnDefinition() { });
                        break;
                    case 2:
                        GridAttackFormation2.HorizontalAlignment = HorizontalAlignment.Left;
                        GridAttackFormation2.VerticalAlignment = VerticalAlignment.Top;
                        GridAttackFormation2.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(100) });
                        GridAttackFormation2.ColumnDefinitions.Add(new ColumnDefinition() { });
                        break;
                    case 3:
                        GridAttackFormation3.HorizontalAlignment = HorizontalAlignment.Left;
                        GridAttackFormation3.VerticalAlignment = VerticalAlignment.Top;
                        GridAttackFormation3.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(100) });
                        GridAttackFormation3.ColumnDefinitions.Add(new ColumnDefinition() { });
                        break;
                    case 4:
                        GridAttackFormation4.HorizontalAlignment = HorizontalAlignment.Left;
                        GridAttackFormation4.VerticalAlignment = VerticalAlignment.Top;
                        GridAttackFormation4.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(100) });
                        GridAttackFormation4.ColumnDefinitions.Add(new ColumnDefinition() { });
                        break;
                }

                FormationVSFormationDto[] winDtos = formationVSFormationDtos.Where(x => x.LoseFormationId == defenseFormationId).ToArray();

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

                switch (formationNo)
                {
                    case 1:
                        for (int i = 0; i < formationCnt; i++)
                        {
                            GridAttackFormation1.RowDefinitions.Add(new RowDefinition());
                        }
                        break;
                    case 2:
                        for (int i = 0; i < formationCnt; i++)
                        {
                            GridAttackFormation2.RowDefinitions.Add(new RowDefinition());
                        }
                        break;
                    case 3:
                        for (int i = 0; i < formationCnt; i++)
                        {
                            GridAttackFormation3.RowDefinitions.Add(new RowDefinition());
                        }
                        break;
                    case 4:
                        for (int i = 0; i < formationCnt; i++)
                        {
                            GridAttackFormation4.RowDefinitions.Add(new RowDefinition());
                        }
                        break;
                }

                if (formationCnt == 0)
                {
                    Label label = new Label
                    {
                        Content = "勝利可能編成は設定されていませんでした。",
                        FontSize = 16,
                        Foreground = new SolidColorBrush(KO_COLOR)
                    };

                    switch (formationNo)
                    {
                        case 1:
                            GridAttackFormation1.AddChild(label, 0, 1, 1, 1);
                            break;
                        case 2:
                            GridAttackFormation2.AddChild(label, 0, 1, 1, 1);
                            break;
                        case 3:
                            GridAttackFormation3.AddChild(label, 0, 1, 1, 1);
                            break;
                        case 4:
                            GridAttackFormation4.AddChild(label, 0, 1, 1, 1);
                            break;
                    }

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

                        switch (formationNo)
                        {
                            case 1:
                                GridAttackFormation1.AddChild(playerGrid, formationCnt, 0, 1, 1);
                                break;
                            case 2:
                                GridAttackFormation2.AddChild(playerGrid, formationCnt, 0, 1, 1);
                                break;
                            case 3:
                                GridAttackFormation3.AddChild(playerGrid, formationCnt, 0, 1, 1);
                                break;
                            case 4:
                                GridAttackFormation4.AddChild(playerGrid, formationCnt, 0, 1, 1);
                                break;
                        }

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
                                    drawContent.DrawImage(bmp, new System.Windows.Rect(0, 0, bmp.PixelWidth, bmp.PixelHeight));
                                    drawContent.DrawText(new FormattedText(Common.Common.GetSpeedRuneText(characterDto),
                                                                            System.Globalization.CultureInfo.CurrentUICulture,
                                                                            FlowDirection.LeftToRight,
                                                                            new Typeface("Verdana"), 22, Brushes.Red,
                                                                            VisualTreeHelper.GetDpi(this).PixelsPerDip),
                                                                            new System.Windows.Point(5, 100));
                                }

                                image.Source = new DrawingImage(drawingGroup);
                            }

                            wrapPanel.Children.Add(image);
                            charaCnt++;
                        }

                        switch (formationNo)
                        {
                            case 1:
                                GridAttackFormation1.AddChild(wrapPanel, formationCnt, 1, 1, 1);
                                break;
                            case 2:
                                GridAttackFormation2.AddChild(wrapPanel, formationCnt, 1, 1, 1);
                                break;
                            case 3:
                                GridAttackFormation3.AddChild(wrapPanel, formationCnt, 1, 1, 1);
                                break;
                            case 4:
                                GridAttackFormation4.AddChild(wrapPanel, formationCnt, 1, 1, 1);
                                break;
                        }

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

        private void CheckAttackShowLine_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (CheckAttackShowLine.IsChecked == true)
                {
                    GridAttackFormation1.ShowGridLines = true;
                    GridAttackFormation2.ShowGridLines = true;
                    GridAttackFormation3.ShowGridLines = true;
                    GridAttackFormation4.ShowGridLines = true;
                }
                else
                {
                    GridAttackFormation1.ShowGridLines = false;
                    GridAttackFormation2.ShowGridLines = false;
                    GridAttackFormation3.ShowGridLines = false;
                    GridAttackFormation4.ShowGridLines = false;
                }
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }

        private ImageSource ToImageSource(Bitmap bmp)
        {
            if (null == bmp) return null;

            var hBitmap = bmp.GetHbitmap();

            try
            {
                return Imaging.CreateBitmapSourceFromHBitmap(
                            hBitmap,
                            IntPtr.Zero,
                            Int32Rect.Empty,
                            BitmapSizeOptions.FromEmptyOptions()
                        );
            }
            catch (Exception)
            {
                return null;
            }
            finally
            {
                NativeMethods.DeleteObject(hBitmap);
            }
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hwnd, out RECT lpRect);

        private Bitmap Capture(string processName, string titleName)
        {
            foreach (Process p in Process.GetProcesses())
            {
                if (0 <= p.MainWindowTitle.IndexOf(processName))
                {
                    SetForegroundWindow(p.MainWindowHandle);
                    break;
                }
            }

            Process[] proc = GetProcessesByWindowTitle(titleName);

            if (proc.Length != 1)
            {
                MessageBox.Show(titleName + "ウィンドウが起動されていません。");
                return null;
            }

            IntPtr handle = proc[0].MainWindowHandle;

            GetWindowRect(handle, out RECT rect);

            int rectWidth = rect.right - rect.left;
            int rectHeight = rect.bottom - rect.top;

            Bitmap bmp = new Bitmap(rectWidth, rectHeight);
            Graphics g = Graphics.FromImage(bmp);

            g.CopyFromScreen(new System.Drawing.Point(rect.left, rect.top), new System.Drawing.Point(0, 0), bmp.Size);
            g.Dispose();

            return bmp;
        }

        private Process[] GetProcessesByWindowTitle(string windowTitle)
        {
            System.Collections.ArrayList list = new System.Collections.ArrayList();

            foreach (Process p in Process.GetProcesses())
            {
                if (p.MainWindowTitle.Equals(windowTitle))
                {
                    list.Add(p);
                }
            }

            return (Process[])list.ToArray(typeof(Process));
        }
    }
}
