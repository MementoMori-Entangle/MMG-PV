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
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Button = System.Windows.Controls.Button;
using Point = OpenCvSharp.Point;
using Window = System.Windows.Window;

namespace MMG
{
    /// <summary>
    /// CharacterImageReader.xaml の相互作用ロジック
    /// </summary>
    public partial class CharacterImageReader : Window
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        private readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().Name);

        private readonly string imgCharaPath = Common.Common.FormationCharaImagePath;
        private readonly string imgExt = Common.Common.FormationImageExtension;
        private readonly string MATCH_TEMP_FILE_NAME = "_match.bmp";
        private readonly string NUMBER_PARSER_ERROR_MSG = "編成{fNo}の{順序}のレベルは半角数値で入力してください。";
        private readonly string MOVE_LEFT_STR = "Left";
        private readonly string MOVE_RIGHT_STR = "Right";
        private readonly string CHARACTER_NOTHING = "未指定";

        private string captureName; // 現状メメントモリのプロセス名とタイトル名は同一
        private string tempFilePath;
        private double matchThreshold;

        private Dictionary<string, Point> characterPointDic;
        private Dictionary<int, Point> playerPointDic;
        private List<string> playerNameList;

        private List<FormationDto> formationDtoList;

        public CharacterImageReader()
        {
            InitializeComponent();

            captureName = Common.Common.PROCESS_TITLE_NAME;
            matchThreshold = Common.Common.MATCH_THRESHOLD;
            characterPointDic = new Dictionary<string, Point>();
            playerPointDic = new Dictionary<int, Point>();
            playerNameList = new List<string>();

            LoadCombBoxRarity();

            LabNowLoading.Visibility = Visibility.Hidden;

            CombBTargetScreen.Items.Add("キャラ->編成");
            CombBTargetScreen.Items.Add("ギルドバトル->偵察情報");
            CombBTargetScreen.SelectedIndex = 1;

            string[] wtns = Common.Common.MATCH_WINDOW_TITLE_NAMES.Split(',');

            CombBWTPName.Items.Add("");

            foreach (string name in wtns)
            {
                CombBWTPName.Items.Add(name);
            }
        }

        private void PlayerMatching(string imgFilePath)
        {
            if (CombBTargetScreen.SelectedIndex == 0)
            {
                return;
            }

            var targetMat = new Mat(imgFilePath);

            string pmDirPath = AppDomain.CurrentDomain.BaseDirectory + Common.Common.MATCH_GUILDVS_PLAYER_DIR;

            string[] templateFiles = Directory.GetFiles(pmDirPath, "*", SearchOption.AllDirectories);

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
            string pmDirPath = string.Empty;

            if (CombBTargetScreen.SelectedIndex == 0)
            {
                pmDirPath = AppDomain.CurrentDomain.BaseDirectory + Common.Common.MATCH_CHARACTER_DIR;
            }
            else if (CombBTargetScreen.SelectedIndex == 1)
            {
                pmDirPath = AppDomain.CurrentDomain.BaseDirectory + Common.Common.MATCH_GUILDVS_CHARACTER_DIR;
            }

            string[] templateFiles = Directory.GetFiles(pmDirPath, "*", SearchOption.TopDirectoryOnly);

            foreach (var fileName in templateFiles)
            {
                var templateMat = new Mat(fileName);

                var match = Matching(targetMat, templateMat, out var maxPoint);

                if (match < 0)
                {
                    continue;
                }

                MMatching(targetMat, templateMat);

                // マッチした箇所を赤で囲む
                //targetMat.Rectangle(maxPoint, new Point(maxPoint.X + templateMat.Width, maxPoint.Y + templateMat.Height), Scalar.Red, 2, LineTypes.AntiAlias, 0);

                // マッチ度を画面上部に表示
                //targetMat.Rectangle(new Point(0, 0), new Point(800, 60), Scalar.White, -1, LineTypes.AntiAlias, 0);
                //Cv2.PutText(targetMat, match.ToString("0.##"), new Point(0, 50), HersheyFonts.HersheyPlain, 2, Scalar.Black, 1, LineTypes.AntiAlias);

                string charaFilePath = string.Empty;

                string name = Path.GetFileNameWithoutExtension(fileName);
                string[] names = name.Split('_');
                string newFileName = Path.GetDirectoryName(fileName) + "/" + names[0] + Path.GetExtension(fileName);

                if (CombBTargetScreen.SelectedIndex == 0)
                {
                    charaFilePath = newFileName.Replace(Common.Common.MATCH_CHARACTER_DIR, Common.Common.CHARACTER_DIR);
                }
                else if (CombBTargetScreen.SelectedIndex == 1)
                {
                    charaFilePath = newFileName.Replace(Common.Common.MATCH_GUILDVS_CHARACTER_DIR, Common.Common.CHARACTER_DIR);
                }

                charaFilePath = charaFilePath.Replace(".bmp", ".jpg");

                string charaFileName = Path.GetFileNameWithoutExtension(charaFilePath);

                if (characterPointDic.ContainsKey(charaFileName))
                {
                    continue;
                }
                else
                {
                    characterPointDic.Add(charaFileName, maxPoint);
                }

                System.Windows.Controls.Image image = new System.Windows.Controls.Image
                {
                    Source = new BitmapImage(new Uri(charaFilePath)),
                    ToolTip = charaFileName,
                    Width = 75,
                    Height = 75
                };

                ListBMatchCharacter.Items.Add(image);
            }

            LabMCNum.Content = "マッチングキャラクター:" + ListBMatchCharacter.Items.Count;

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

        private void BtnPlayerFormation_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (null != ListBPlayer.SelectedItem)
                {
                    PlayerDao playerDao = new PlayerDao();

                    PlayerDto playerDto = playerDao.GetPlayerDto(ListBPlayer.SelectedItem.ToString());
                    List<PlayerDto> playerList = playerDao.GetAllInfoPlayerDtos(playerDto.GuildName).ToList();
                    playerDto = playerList.Where(x => x.Name.Equals(ListBPlayer.SelectedItem.ToString())).FirstOrDefault();

                    UpdateCharacter();

                    MemberInfo memberInfo = new MemberInfo(playerDto, playerList, true);
                    memberInfo.AddFormationList = formationDtoList;
                    memberInfo.LoadMemberInfo();
                    memberInfo.AddFormation();
                    memberInfo.ShowDialog();
                }
                else
                {
                    MessageBox.Show("編成を追加するプレイヤーを選択してください。");
                }
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }

        private int CheckAndParser(string num, int fNo, int sNo)
        {
            try
            {
                return int.Parse(num);
            }
            catch (Exception e)
            {
                string msg = NUMBER_PARSER_ERROR_MSG.Replace("{fNo}", fNo.ToString())
                                                    .Replace("{順序}", Common.Common.FORMATION_ORDER[sNo]);
                MessageBox.Show(msg);
                throw e;
            }
        }

        private void UpdateCharacter()
        {
            try
            {
                int max = formationDtoList.Count;

                for (int i = 1; i <= max; i++)
                {
                    FormationDto dto = formationDtoList[i - 1];

                    switch (i)
                    {
                        case 1:
                            if (dto.Characters.Length > 0)
                            {
                                dto.Characters[0].Rarity = CombR1_1.SelectedIndex;
                                dto.Characters[0].Level = CheckAndParser(TextLevel1_1.Text, i, 0);
                            }
                            if (dto.Characters.Length > 1)
                            {
                                dto.Characters[1].Rarity = CombR1_2.SelectedIndex;
                                dto.Characters[1].Level = CheckAndParser(TextLevel1_2.Text, i, 1);
                            }
                            if (dto.Characters.Length > 2)
                            {
                                dto.Characters[2].Rarity = CombR1_3.SelectedIndex;
                                dto.Characters[2].Level = CheckAndParser(TextLevel1_3.Text, i, 2);
                            }
                            if (dto.Characters.Length > 3)
                            {
                                dto.Characters[3].Rarity = CombR1_4.SelectedIndex;
                                dto.Characters[3].Level = CheckAndParser(TextLevel1_4.Text, i, 3);
                            }
                            if (dto.Characters.Length > 4)
                            {
                                dto.Characters[4].Rarity = CombR1_5.SelectedIndex;
                                dto.Characters[4].Level = CheckAndParser(TextLevel1_5.Text, i, 4);
                            }
                            break;
                        case 2:
                            if (dto.Characters.Length > 0)
                            {
                                dto.Characters[0].Rarity = CombR2_1.SelectedIndex;
                                dto.Characters[0].Level = CheckAndParser(TextLevel2_1.Text, i, 0);
                            }
                            if (dto.Characters.Length > 1)
                            {
                                dto.Characters[1].Rarity = CombR2_2.SelectedIndex;
                                dto.Characters[1].Level = CheckAndParser(TextLevel2_2.Text, i, 1);
                            }
                            if (dto.Characters.Length > 2)
                            {
                                dto.Characters[2].Rarity = CombR2_3.SelectedIndex;
                                dto.Characters[2].Level = CheckAndParser(TextLevel2_3.Text, i, 2);
                            }
                            if (dto.Characters.Length > 3)
                            {
                                dto.Characters[3].Rarity = CombR2_4.SelectedIndex;
                                dto.Characters[3].Level = CheckAndParser(TextLevel2_4.Text, i, 3);
                            }
                            if (dto.Characters.Length > 4)
                            {
                                dto.Characters[4].Rarity = CombR2_5.SelectedIndex;
                                dto.Characters[4].Level = CheckAndParser(TextLevel2_5.Text, i, 4);
                            }
                            break;
                        case 3:
                            if (dto.Characters.Length > 0)
                            {
                                dto.Characters[0].Rarity = CombR3_1.SelectedIndex;
                                dto.Characters[0].Level = CheckAndParser(TextLevel3_1.Text, i, 0);
                            }
                            if (dto.Characters.Length > 1)
                            {
                                dto.Characters[1].Rarity = CombR3_2.SelectedIndex;
                                dto.Characters[1].Level = CheckAndParser(TextLevel3_2.Text, i, 1);
                            }
                            if (dto.Characters.Length > 2)
                            {
                                dto.Characters[2].Rarity = CombR3_3.SelectedIndex;
                                dto.Characters[2].Level = CheckAndParser(TextLevel3_3.Text, i, 2);
                            }
                            if (dto.Characters.Length > 3)
                            {
                                dto.Characters[3].Rarity = CombR3_4.SelectedIndex;
                                dto.Characters[3].Level = CheckAndParser(TextLevel3_4.Text, i, 3);
                            }
                            if (dto.Characters.Length > 4)
                            {
                                dto.Characters[4].Rarity = CombR3_5.SelectedIndex;
                                dto.Characters[4].Level = CheckAndParser(TextLevel3_5.Text, i, 4);
                            }
                            break;
                        case 4:
                            if (dto.Characters.Length > 0)
                            {
                                dto.Characters[0].Rarity = CombR4_1.SelectedIndex;
                                dto.Characters[0].Level = CheckAndParser(TextLevel4_1.Text, i, 0);
                            }
                            if (dto.Characters.Length > 1)
                            {
                                dto.Characters[1].Rarity = CombR4_2.SelectedIndex;
                                dto.Characters[1].Level = CheckAndParser(TextLevel4_2.Text, i, 1);
                            }
                            if (dto.Characters.Length > 2)
                            {
                                dto.Characters[2].Rarity = CombR4_3.SelectedIndex;
                                dto.Characters[2].Level = CheckAndParser(TextLevel4_3.Text, i, 2);
                            }
                            if (dto.Characters.Length > 3)
                            {
                                dto.Characters[3].Rarity = CombR4_4.SelectedIndex;
                                dto.Characters[3].Level = CheckAndParser(TextLevel4_4.Text, i, 3);
                            }
                            if (dto.Characters.Length > 4)
                            {
                                dto.Characters[4].Rarity = CombR4_5.SelectedIndex;
                                dto.Characters[4].Level = CheckAndParser(TextLevel4_5.Text, i, 4);
                            }
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        private void FormationChatacterMove_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (null == formationDtoList || formationDtoList.Count == 0)
                {
                    return;
                }

                Button button = (Button)sender;

                string[] parts = button.Name.Split('_');

                int formationNo = int.Parse(parts[0].Replace("Btn", ""));
                
                if (formationDtoList.Count < formationNo)
                {
                    return;
                }

                int pos = int.Parse(parts[1].Substring(0, 1));
                string moveStr = parts[1].Substring(1);

                FormationDto formationDto = formationDtoList[formationNo - 1];

                if (formationDto.Characters.Length < pos ||
                    (formationDto.Characters.Length == pos && moveStr.Equals(MOVE_RIGHT_STR)))
                {
                    return;
                }

                int movePos = 0;

                if (moveStr.Equals(MOVE_RIGHT_STR))
                {
                    movePos = pos + 1;
                }
                else if (moveStr.Equals(MOVE_LEFT_STR))
                {
                    movePos = pos - 1;
                }

                List<CharacterDto> characterList = formationDto.Characters.ToList();

                (characterList[pos - 1], characterList[movePos - 1]) = (characterList[movePos - 1], characterList[pos - 1]);

                formationDto.Characters = characterList.ToArray();

                LoadFormation(formationNo);
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }

        private void ImageMouseLeftButtonUp(int pos)
        {
            try
            {
                FormationDto formationDto = formationDtoList[pos - 1];

                //UpdateFormation();

                CharacterSelection characterSelection = new CharacterSelection(formationDto);

                characterSelection.ShowDialog();

                LoadFormation(pos);
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }

        private void Img_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                System.Windows.Controls.Image image = (System.Windows.Controls.Image)sender;

                if (null != image.ToolTip)
                {
                    string[] parts = image.Name.Split('_');

                    int formationNo = int.Parse(parts[0].Replace("Img", ""));

                    ImageMouseLeftButtonUp(formationNo);
                }
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }

        private void Img_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                System.Windows.Controls.Image image = (System.Windows.Controls.Image)sender;

                if (null != image.ToolTip)
                {
                    string[] parts = image.Name.Split('_');

                    int formationNo = int.Parse(parts[0].Replace("Img", ""));

                    ClearSelectedFormation(image.ToolTip.ToString(), formationNo);
                }
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }

        private void ClearSelectedFormation(string name, int pos)
        {
            try
            {
                FormationDto formationDto = formationDtoList[pos - 1];

                // キャラクターが設定されている場合は編成から解除し、アイコンの再描画
                if (formationDto.Characters.Length > 0)
                {
                    if (null != name)
                    {
                        CharacterDto selectedCharacterDto = formationDto.Characters.Where(x => x.Name == name).FirstOrDefault();

                        if (null != selectedCharacterDto)
                        {
                            ClearFormationCharacter(name, pos);
                        }
                    }
                    else
                    {
                        formationDto.Characters = formationDto.Characters.Where(x => x.Id == -999).ToArray();
                        LoadFormation(pos);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }

        private void ClearFormationCharacter(string name, int pos)
        {
            try
            {
                FormationDto formationDto = formationDtoList[pos - 1];

                formationDto.Characters = formationDto.Characters.Where(x => x.Name != name).ToArray();
                LoadFormation(pos);
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

        private string[] GetDivImageFilePath(string baseFilePath)
        {
            List<string> resultList = new List<string>();

            try
            {
                string inFilePath = Common.Common.MATCH_TEMP_DIV_DIR_PATH + Path.GetFileNameWithoutExtension(baseFilePath);

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

        private void LoadFCharacterMatch()
        {
            try
            {
                Dispatcher.Invoke(() =>
                {
                    ListBMatchCharacter.Items.Clear();
                    characterPointDic.Clear();
                    formationDtoList = new List<FormationDto>();

                    if (!string.IsNullOrEmpty(TextBImageFilePath.Text))
                    {
                        CharacterMatching(TextBImageFilePath.Text);
                    }
                    else if (!string.IsNullOrEmpty(tempFilePath))
                    {
                        CharacterMatching(tempFilePath);
                    }

                    if (CheckBAutoFormation.IsChecked.Value)
                    {
                        // 高さがテンプレート画像によって±の誤差が発生するため、範囲内の高さを中間値で固定する。(ソート対策)
                        FixedIntermediateValueWithInHeightRange();

                        // マッチング位置から、キャラクターのグルーピングを行う(最大4編成分となる)
                        IOrderedEnumerable<KeyValuePair<string, Point>> sorted = characterPointDic.OrderBy(y => y.Value.Y).ThenBy(x => x.Value.X);

                        Dictionary<int, List<CharacterDto>> characterDic = new Dictionary<int, List<CharacterDto>>();
                        int no = 1;
                        int cnt = 0;

                        BaseCharacterDto[] baseCharacterDtos = BaseCharacter.GetBaseCharacterDtos();

                        foreach (KeyValuePair<string, Point> keyValuePair in sorted)
                        {
                            if (cnt > 4)
                            {
                                no++;
                                cnt = 0;
                            }

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

                            cnt++;
                        }

                        // 編成データ作成
                        for (int i = 1; i <= no; i++)
                        {
                            FormationDto formationDto = new FormationDto
                            {
                                Id = -1000000 - i,
                                SortNo = i,
                                Name = "仮編成" + i,
                                Characters = characterDic[i].ToArray()
                            };

                            formationDtoList.Add(formationDto);
                        }

                        LoadFormation();
                    }
                });
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }

        private void LoadGVSCharacterMatch()
        {
            try
            {
                Dispatcher.Invoke(() =>
                {
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

                    if (!Directory.Exists(Common.Common.MATCH_TEMP_DIV_DIR_PATH))
                    {
                        Directory.CreateDirectory(Common.Common.MATCH_TEMP_DIV_DIR_PATH);
                    }

                    string[] filePaths = GetDivImageFilePath(baseFilePath);

                    BaseCharacterDto[] baseCharacterDtos = BaseCharacter.GetBaseCharacterDtos();

                    Dictionary<int, List<CharacterDto>> characterDic = new Dictionary<int, List<CharacterDto>>();

                    int no = 0;

                    playerPointDic.Clear();
                    playerNameList.Clear();
                    ListBMatchCharacter.Items.Clear();

                    foreach (string filePath in filePaths)
                    {
                        characterPointDic.Clear();
                        formationDtoList = new List<FormationDto>();

                        CharacterMatching(filePath);
                        PlayerMatching(filePath);

                        // 高さがテンプレート画像によって±の誤差が発生するため、範囲内の高さを中間値で固定する。(ソート対策)
                        FixedIntermediateValueWithInHeightRange();

                        // マッチング位置から、キャラクターのグルーピングを行う
                        IOrderedEnumerable<KeyValuePair<string, Point>> sorted = characterPointDic.OrderBy(y => y.Value.Y).ThenBy(x => x.Value.X);

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
                    }

                    if (CheckBAutoFormation.IsChecked.Value)
                    {
                        // 編成データ作成
                        for (int i = 1; i <= no; i++)
                        {
                            if (!characterDic.ContainsKey(i))
                            {
                                continue;
                            }

                            FormationDto formationDto = new FormationDto
                            {
                                Id = -1000000 - i,
                                SortNo = i,
                                Name = "仮編成" + i,
                                Characters = characterDic[i].ToArray()
                            };

                            formationDtoList.Add(formationDto);

                            if (playerNameList.Count > (i - 1))
                            {
                                switch (i)
                                {
                                    case 1:
                                        BtnPlayerFormation1.IsEnabled = true;
                                        BtnPlayerFormation1.ToolTip = playerNameList[i - 1];
                                        LabPlayerFormation1.Content = playerNameList[i - 1];
                                        break;
                                    case 2:
                                        BtnPlayerFormation2.IsEnabled = true;
                                        BtnPlayerFormation2.ToolTip = playerNameList[i - 1];
                                        LabPlayerFormation2.Content = playerNameList[i - 1];
                                        break;
                                    case 3:
                                        BtnPlayerFormation3.IsEnabled = true;
                                        BtnPlayerFormation3.ToolTip = playerNameList[i - 1];
                                        LabPlayerFormation3.Content = playerNameList[i - 1];
                                        break;
                                    case 4:
                                        BtnPlayerFormation4.IsEnabled = true;
                                        BtnPlayerFormation4.ToolTip = playerNameList[i - 1];
                                        LabPlayerFormation4.Content = playerNameList[i - 1];
                                        break;
                                }
                            }
                        }

                        LoadFormation();
                    }
                });
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }

        private void ClearPlayerFormation()
        {
            try
            {
                BtnPlayerFormation1.IsEnabled = false;
                BtnPlayerFormation1.ToolTip = null;
                LabPlayerFormation1.Content = null;
                BtnPlayerFormation2.IsEnabled = false;
                BtnPlayerFormation2.ToolTip = null;
                LabPlayerFormation2.Content = null;
                BtnPlayerFormation3.IsEnabled = false;
                BtnPlayerFormation3.ToolTip = null;
                LabPlayerFormation3.Content = null;
                BtnPlayerFormation4.IsEnabled = false;
                BtnPlayerFormation4.ToolTip = null;
                LabPlayerFormation4.Content = null;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        private async void BtnCharacterMatch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                BtnCapture.IsEnabled = false;
                BtnImageLoad.IsEnabled = false;
                BtnCharacterMatch.IsEnabled = false;

                ClearPlayerFormation();

                if (null == ImgTarget.Source)
                {
                    MessageBox.Show("対象画像を読み込んでください。");

                    BtnCapture.IsEnabled = true;
                    BtnImageLoad.IsEnabled = true;
                    BtnCharacterMatch.IsEnabled = true;
                    return;
                }

                ClearFormation();

                LabNowLoading.Visibility = Visibility.Visible;
                LabNowLoading.Content = "Now Loading ...";

                await Task.Delay(50);

                if (CombBTargetScreen.SelectedIndex == 0)
                {
                    LoadFCharacterMatch();
                }
                else if(CombBTargetScreen.SelectedIndex == 1)
                {
                    LoadGVSCharacterMatch();
                }

                CombGuild.Items.Clear();
                LoadGuildItems();

                PlayerDao playerDao = new PlayerDao();
                PlayerDto[] playerDtos = playerDao.GetPlayerDtos();
                ListBPlayer.Items.Clear();

                foreach (PlayerDto dto in playerDtos)
                {
                    ListBPlayer.Items.Add(dto.Name);
                }

                LabNowLoading.Content = "Loading completed.";

                BtnCapture.IsEnabled = true;
                BtnImageLoad.IsEnabled = true;
                BtnCharacterMatch.IsEnabled = true;
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }

        private void PlayerFormation(string playerName, int formationNo)
        {
            try
            {
                PlayerDao playerDao = new PlayerDao();

                PlayerDto playerDto = playerDao.GetPlayerDto(playerName);
                List<PlayerDto> playerList = playerDao.GetAllInfoPlayerDtos(playerDto.GuildName).ToList();
                playerDto = playerList.Where(x => x.Name.Equals(playerName)).FirstOrDefault();

                UpdateCharacter();

                MemberInfo memberInfo = new MemberInfo(playerDto, playerList, true);

                List<FormationDto> targetFDList = new List<FormationDto>
                {
                    formationDtoList[formationNo - 1]
                };

                memberInfo.AddFormationList = targetFDList;
                memberInfo.LoadMemberInfo();
                memberInfo.AddFormation();
                memberInfo.ShowDialog();
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        private void BtnPlayerFormation1_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Button button = (Button)sender;

                string name = button.ToolTip.ToString();

                PlayerFormation(name, 1);
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }

        private void BtnPlayerFormation2_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Button button = (Button)sender;

                string name = button.ToolTip.ToString();

                PlayerFormation(name, 2);
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }

        private void BtnPlayerFormation3_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Button button = (Button)sender;

                string name = button.ToolTip.ToString();

                PlayerFormation(name, 3);
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }

        private void BtnPlayerFormation4_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Button button = (Button)sender;

                string name = button.ToolTip.ToString();

                PlayerFormation(name, 4);
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }

        private void ClearFormation()
        {
            try
            {
                Img1_1.Source = new BitmapImage(new Uri(imgCharaPath + CHARACTER_NOTHING + imgExt));
                Img1_1.ToolTip = string.Empty;
                CombR1_1.SelectedItem = null;
                Img1_2.Source = new BitmapImage(new Uri(imgCharaPath + CHARACTER_NOTHING + imgExt));
                Img1_2.ToolTip = string.Empty;
                CombR1_2.SelectedItem = null;
                Img1_3.Source = new BitmapImage(new Uri(imgCharaPath + CHARACTER_NOTHING + imgExt));
                Img1_3.ToolTip = string.Empty;
                CombR1_3.SelectedItem = null;
                Img1_4.Source = new BitmapImage(new Uri(imgCharaPath + CHARACTER_NOTHING + imgExt));
                Img1_4.ToolTip = string.Empty;
                CombR1_4.SelectedItem = null;
                Img1_5.Source = new BitmapImage(new Uri(imgCharaPath + CHARACTER_NOTHING + imgExt));
                Img1_5.ToolTip = string.Empty;
                CombR1_5.SelectedItem = null;

                Img2_1.Source = new BitmapImage(new Uri(imgCharaPath + CHARACTER_NOTHING + imgExt));
                Img2_1.ToolTip = string.Empty;
                CombR2_1.SelectedItem = null;
                Img2_2.Source = new BitmapImage(new Uri(imgCharaPath + CHARACTER_NOTHING + imgExt));
                Img2_2.ToolTip = string.Empty;
                CombR2_2.SelectedItem = null;
                Img2_3.Source = new BitmapImage(new Uri(imgCharaPath + CHARACTER_NOTHING + imgExt));
                Img2_3.ToolTip = string.Empty;
                CombR2_3.SelectedItem = null;
                Img2_4.Source = new BitmapImage(new Uri(imgCharaPath + CHARACTER_NOTHING + imgExt));
                Img2_4.ToolTip = string.Empty;
                CombR2_4.SelectedItem = null;
                Img2_5.Source = new BitmapImage(new Uri(imgCharaPath + CHARACTER_NOTHING + imgExt));
                Img2_5.ToolTip = string.Empty;
                CombR2_5.SelectedItem = null;

                Img3_1.Source = new BitmapImage(new Uri(imgCharaPath + CHARACTER_NOTHING + imgExt));
                Img3_1.ToolTip = string.Empty;
                CombR3_1.SelectedItem = null;
                Img3_2.Source = new BitmapImage(new Uri(imgCharaPath + CHARACTER_NOTHING + imgExt));
                Img3_2.ToolTip = string.Empty;
                CombR3_2.SelectedItem = null;
                Img3_3.Source = new BitmapImage(new Uri(imgCharaPath + CHARACTER_NOTHING + imgExt));
                Img3_3.ToolTip = string.Empty;
                CombR3_3.SelectedItem = null;
                Img3_4.Source = new BitmapImage(new Uri(imgCharaPath + CHARACTER_NOTHING + imgExt));
                Img3_4.ToolTip = string.Empty;
                CombR3_4.SelectedItem = null;
                Img3_5.Source = new BitmapImage(new Uri(imgCharaPath + CHARACTER_NOTHING + imgExt));
                Img3_5.ToolTip = string.Empty;
                CombR3_5.SelectedItem = null;

                Img4_1.Source = new BitmapImage(new Uri(imgCharaPath + CHARACTER_NOTHING + imgExt));
                Img4_1.ToolTip = string.Empty;
                CombR4_1.SelectedItem = null;
                Img4_2.Source = new BitmapImage(new Uri(imgCharaPath + CHARACTER_NOTHING + imgExt));
                Img4_2.ToolTip = string.Empty;
                CombR4_2.SelectedItem = null;
                Img4_3.Source = new BitmapImage(new Uri(imgCharaPath + CHARACTER_NOTHING + imgExt));
                Img4_3.ToolTip = string.Empty;
                CombR4_3.SelectedItem = null;
                Img4_4.Source = new BitmapImage(new Uri(imgCharaPath + CHARACTER_NOTHING + imgExt));
                Img4_4.ToolTip = string.Empty;
                CombR4_4.SelectedItem = null;
                Img4_5.Source = new BitmapImage(new Uri(imgCharaPath + CHARACTER_NOTHING + imgExt));
                Img4_5.ToolTip = string.Empty;
                CombR4_5.SelectedItem = null;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        private void LoadFormation(int pos)
        {
            try
            {
                FormationDto dto = formationDtoList[pos - 1];

                switch (pos)
                {
                    case 1:
                        if (dto.Characters.Length > 0)
                        {
                            Img1_1.Source = new BitmapImage(new Uri(imgCharaPath + dto.Characters[0].Name + imgExt));
                            Img1_1.ToolTip = dto.Characters[0].Name;
                            CombR1_1.SelectedIndex = dto.Characters[0].Rarity;
                            TextLevel1_1.Text = dto.Characters[0].Level.ToString();
                        }
                        else
                        {
                            Img1_1.Source = new BitmapImage(new Uri(imgCharaPath + Common.Common.FormationImageNothing + imgExt));
                            Img1_1.ToolTip = string.Empty;
                            CombR1_1.SelectedIndex = -1;
                            TextLevel1_1.Text = string.Empty;
                        }
                        if (dto.Characters.Length > 1)
                        {
                            Img1_2.Source = new BitmapImage(new Uri(imgCharaPath + dto.Characters[1].Name + imgExt));
                            Img1_2.ToolTip = dto.Characters[1].Name;
                            CombR1_2.SelectedIndex = dto.Characters[1].Rarity;
                            TextLevel1_2.Text = dto.Characters[1].Level.ToString();
                        }
                        else
                        {
                            Img1_2.Source = new BitmapImage(new Uri(imgCharaPath + Common.Common.FormationImageNothing + imgExt));
                            Img1_2.ToolTip = string.Empty;
                            CombR1_2.SelectedIndex = -1;
                            TextLevel1_2.Text = string.Empty;
                        }
                        if (dto.Characters.Length > 2)
                        {
                            Img1_3.Source = new BitmapImage(new Uri(imgCharaPath + dto.Characters[2].Name + imgExt));
                            Img1_3.ToolTip = dto.Characters[2].Name;
                            CombR1_3.SelectedIndex = dto.Characters[2].Rarity;
                            TextLevel1_3.Text = dto.Characters[2].Level.ToString();
                        }
                        else
                        {
                            Img1_3.Source = new BitmapImage(new Uri(imgCharaPath + Common.Common.FormationImageNothing + imgExt));
                            Img1_3.ToolTip = string.Empty;
                            CombR1_3.SelectedIndex = -1;
                            TextLevel1_3.Text = string.Empty;
                        }
                        if (dto.Characters.Length > 3)
                        {
                            Img1_4.Source = new BitmapImage(new Uri(imgCharaPath + dto.Characters[3].Name + imgExt));
                            Img1_4.ToolTip = dto.Characters[3].Name;
                            CombR1_4.SelectedIndex = dto.Characters[3].Rarity;
                            TextLevel1_4.Text = dto.Characters[3].Level.ToString();
                        }
                        else
                        {
                            Img1_4.Source = new BitmapImage(new Uri(imgCharaPath + Common.Common.FormationImageNothing + imgExt));
                            Img1_4.ToolTip = string.Empty;
                            CombR1_4.SelectedIndex = -1;
                            TextLevel1_4.Text = string.Empty;
                        }
                        if (dto.Characters.Length > 4)
                        {
                            Img1_5.Source = new BitmapImage(new Uri(imgCharaPath + dto.Characters[4].Name + imgExt));
                            Img1_5.ToolTip = dto.Characters[4].Name;
                            CombR1_5.SelectedIndex = dto.Characters[4].Rarity;
                            TextLevel1_5.Text = dto.Characters[4].Level.ToString();
                        }
                        else
                        {
                            Img1_5.Source = new BitmapImage(new Uri(imgCharaPath + Common.Common.FormationImageNothing + imgExt));
                            Img1_5.ToolTip = string.Empty;
                            CombR1_5.SelectedIndex = -1;
                            TextLevel1_5.Text = string.Empty;
                        }
                        break;
                    case 2:
                        if (dto.Characters.Length > 0)
                        {
                            Img2_1.Source = new BitmapImage(new Uri(imgCharaPath + dto.Characters[0].Name + imgExt));
                            Img2_1.ToolTip = dto.Characters[0].Name;
                            CombR2_1.SelectedIndex = dto.Characters[0].Rarity;
                            TextLevel2_1.Text = dto.Characters[0].Level.ToString();
                        }
                        else
                        {
                            Img2_1.Source = new BitmapImage(new Uri(imgCharaPath + Common.Common.FormationImageNothing + imgExt));
                            Img2_1.ToolTip = string.Empty;
                            CombR2_1.SelectedIndex = -1;
                            TextLevel2_1.Text = string.Empty;
                        }
                        if (dto.Characters.Length > 1)
                        {
                            Img2_2.Source = new BitmapImage(new Uri(imgCharaPath + dto.Characters[1].Name + imgExt));
                            Img2_2.ToolTip = dto.Characters[1].Name;
                            CombR2_2.SelectedIndex = dto.Characters[1].Rarity;
                            TextLevel2_2.Text = dto.Characters[1].Level.ToString();
                        }
                        else
                        {
                            Img2_2.Source = new BitmapImage(new Uri(imgCharaPath + Common.Common.FormationImageNothing + imgExt));
                            Img2_2.ToolTip = string.Empty;
                            CombR2_2.SelectedIndex = -1;
                            TextLevel2_2.Text = string.Empty;
                        }
                        if (dto.Characters.Length > 2)
                        {
                            Img2_3.Source = new BitmapImage(new Uri(imgCharaPath + dto.Characters[2].Name + imgExt));
                            Img2_3.ToolTip = dto.Characters[2].Name;
                            CombR2_3.SelectedIndex = dto.Characters[2].Rarity;
                            TextLevel2_3.Text = dto.Characters[2].Level.ToString();
                        }
                        else
                        {
                            Img2_3.Source = new BitmapImage(new Uri(imgCharaPath + Common.Common.FormationImageNothing + imgExt));
                            Img2_3.ToolTip = string.Empty;
                            CombR2_3.SelectedIndex = -1;
                            TextLevel2_3.Text = string.Empty;
                        }
                        if (dto.Characters.Length > 3)
                        {
                            Img2_4.Source = new BitmapImage(new Uri(imgCharaPath + dto.Characters[3].Name + imgExt));
                            Img2_4.ToolTip = dto.Characters[3].Name;
                            CombR2_4.SelectedIndex = dto.Characters[3].Rarity;
                            TextLevel2_4.Text = dto.Characters[3].Level.ToString();
                        }
                        else
                        {
                            Img2_4.Source = new BitmapImage(new Uri(imgCharaPath + Common.Common.FormationImageNothing + imgExt));
                            Img2_4.ToolTip = string.Empty;
                            CombR2_4.SelectedIndex = -1;
                            TextLevel2_4.Text = string.Empty;
                        }
                        if (dto.Characters.Length > 4)
                        {
                            Img2_5.Source = new BitmapImage(new Uri(imgCharaPath + dto.Characters[4].Name + imgExt));
                            Img2_5.ToolTip = dto.Characters[4].Name;
                            CombR2_5.SelectedIndex = dto.Characters[4].Rarity;
                            TextLevel2_5.Text = dto.Characters[4].Level.ToString();
                        }
                        else
                        {
                            Img2_5.Source = new BitmapImage(new Uri(imgCharaPath + Common.Common.FormationImageNothing + imgExt));
                            Img2_5.ToolTip = string.Empty;
                            CombR2_5.SelectedIndex = -1;
                            TextLevel2_5.Text = string.Empty;
                        }
                        break;
                    case 3:
                        if (dto.Characters.Length > 0)
                        {
                            Img3_1.Source = new BitmapImage(new Uri(imgCharaPath + dto.Characters[0].Name + imgExt));
                            Img3_1.ToolTip = dto.Characters[0].Name;
                            CombR3_1.SelectedIndex = dto.Characters[0].Rarity;
                            TextLevel3_1.Text = dto.Characters[0].Level.ToString();
                        }
                        else
                        {
                            Img3_1.Source = new BitmapImage(new Uri(imgCharaPath + Common.Common.FormationImageNothing + imgExt));
                            Img3_1.ToolTip = string.Empty;
                            CombR3_1.SelectedIndex = -1;
                            TextLevel3_1.Text = string.Empty;
                        }
                        if (dto.Characters.Length > 1)
                        {
                            Img3_2.Source = new BitmapImage(new Uri(imgCharaPath + dto.Characters[1].Name + imgExt));
                            Img3_2.ToolTip = dto.Characters[1].Name;
                            CombR3_2.SelectedIndex = dto.Characters[1].Rarity;
                            TextLevel3_2.Text = dto.Characters[1].Level.ToString();
                        }
                        else
                        {
                            Img3_2.Source = new BitmapImage(new Uri(imgCharaPath + Common.Common.FormationImageNothing + imgExt));
                            Img3_2.ToolTip = string.Empty;
                            CombR3_2.SelectedIndex = -1;
                            TextLevel3_2.Text = string.Empty;
                        }
                        if (dto.Characters.Length > 2)
                        {
                            Img3_3.Source = new BitmapImage(new Uri(imgCharaPath + dto.Characters[2].Name + imgExt));
                            Img3_3.ToolTip = dto.Characters[2].Name;
                            CombR3_3.SelectedIndex = dto.Characters[2].Rarity;
                            TextLevel3_3.Text = dto.Characters[2].Level.ToString();
                        }
                        else
                        {
                            Img3_3.Source = new BitmapImage(new Uri(imgCharaPath + Common.Common.FormationImageNothing + imgExt));
                            Img3_3.ToolTip = string.Empty;
                            CombR3_3.SelectedIndex = -1;
                            TextLevel3_3.Text = string.Empty;
                        }
                        if (dto.Characters.Length > 3)
                        {
                            Img3_4.Source = new BitmapImage(new Uri(imgCharaPath + dto.Characters[3].Name + imgExt));
                            Img3_4.ToolTip = dto.Characters[3].Name;
                            CombR3_4.SelectedIndex = dto.Characters[3].Rarity;
                            TextLevel3_4.Text = dto.Characters[3].Level.ToString();
                        }
                        else
                        {
                            Img3_4.Source = new BitmapImage(new Uri(imgCharaPath + Common.Common.FormationImageNothing + imgExt));
                            Img3_4.ToolTip = string.Empty;
                            CombR3_4.SelectedIndex = -1;
                            TextLevel3_4.Text = string.Empty;
                        }
                        if (dto.Characters.Length > 4)
                        {
                            Img3_5.Source = new BitmapImage(new Uri(imgCharaPath + dto.Characters[4].Name + imgExt));
                            Img3_5.ToolTip = dto.Characters[4].Name;
                            CombR3_5.SelectedIndex = dto.Characters[4].Rarity;
                            TextLevel3_5.Text = dto.Characters[4].Level.ToString();
                        }
                        else
                        {
                            Img3_5.Source = new BitmapImage(new Uri(imgCharaPath + Common.Common.FormationImageNothing + imgExt));
                            Img3_5.ToolTip = string.Empty;
                            CombR3_5.SelectedIndex = -1;
                            TextLevel3_5.Text = string.Empty;
                        }
                        break;
                    case 4:
                        if (dto.Characters.Length > 0)
                        {
                            Img4_1.Source = new BitmapImage(new Uri(imgCharaPath + dto.Characters[0].Name + imgExt));
                            Img4_1.ToolTip = dto.Characters[0].Name;
                            CombR4_1.SelectedIndex = dto.Characters[0].Rarity;
                            TextLevel4_1.Text = dto.Characters[0].Level.ToString();
                        }
                        else
                        {
                            Img4_1.Source = new BitmapImage(new Uri(imgCharaPath + Common.Common.FormationImageNothing + imgExt));
                            Img4_1.ToolTip = string.Empty;
                            CombR4_1.SelectedIndex = -1;
                            TextLevel4_1.Text = string.Empty;
                        }
                        if (dto.Characters.Length > 1)
                        {
                            Img4_2.Source = new BitmapImage(new Uri(imgCharaPath + dto.Characters[1].Name + imgExt));
                            Img4_2.ToolTip = dto.Characters[1].Name;
                            CombR4_2.SelectedIndex = dto.Characters[1].Rarity;
                            TextLevel4_2.Text = dto.Characters[1].Level.ToString();
                        }
                        else
                        {
                            Img4_2.Source = new BitmapImage(new Uri(imgCharaPath + Common.Common.FormationImageNothing + imgExt));
                            Img4_2.ToolTip = string.Empty;
                            CombR4_2.SelectedIndex = -1;
                            TextLevel4_2.Text = string.Empty;
                        }
                        if (dto.Characters.Length > 2)
                        {
                            Img4_3.Source = new BitmapImage(new Uri(imgCharaPath + dto.Characters[2].Name + imgExt));
                            Img4_3.ToolTip = dto.Characters[2].Name;
                            CombR4_3.SelectedIndex = dto.Characters[2].Rarity;
                            TextLevel4_3.Text = dto.Characters[2].Level.ToString();
                        }
                        else
                        {
                            Img4_3.Source = new BitmapImage(new Uri(imgCharaPath + Common.Common.FormationImageNothing + imgExt));
                            Img4_3.ToolTip = string.Empty;
                            CombR4_3.SelectedIndex = -1;
                            TextLevel4_3.Text = string.Empty;
                        }
                        if (dto.Characters.Length > 3)
                        {
                            Img4_4.Source = new BitmapImage(new Uri(imgCharaPath + dto.Characters[3].Name + imgExt));
                            Img4_4.ToolTip = dto.Characters[3].Name;
                            CombR4_4.SelectedIndex = dto.Characters[3].Rarity;
                            TextLevel4_4.Text = dto.Characters[3].Level.ToString();
                        }
                        else
                        {
                            Img4_4.Source = new BitmapImage(new Uri(imgCharaPath + Common.Common.FormationImageNothing + imgExt));
                            Img4_4.ToolTip = string.Empty;
                            CombR4_4.SelectedIndex = -1;
                            TextLevel4_4.Text = string.Empty;
                        }
                        if (dto.Characters.Length > 4)
                        {
                            Img4_5.Source = new BitmapImage(new Uri(imgCharaPath + dto.Characters[4].Name + imgExt));
                            Img4_5.ToolTip = dto.Characters[4].Name;
                            CombR4_5.SelectedIndex = dto.Characters[4].Rarity;
                            TextLevel4_5.Text = dto.Characters[4].Level.ToString();
                        }
                        else
                        {
                            Img4_5.Source = new BitmapImage(new Uri(imgCharaPath + Common.Common.FormationImageNothing + imgExt));
                            Img4_5.ToolTip = string.Empty;
                            CombR4_5.SelectedIndex = -1;
                            TextLevel4_5.Text = string.Empty;
                        }
                        break;
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        private void LoadFormation()
        {
            try
            {
                if (null == formationDtoList)
                {
                    return;
                }

                int max = formationDtoList.Count;

                for (int i = 1; i <= max; i++)
                {
                    LoadFormation(i);
                }
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }

        private void ListBPlayer_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            try
            {
                if (null == ListBPlayer.SelectedItem)
                {
                    return;
                }

                PlayerDao playerDao = new PlayerDao();
                PlayerDto playerDto = playerDao.GetPlayerDto(ListBPlayer.SelectedItem.ToString());

                if (playerDto.Formations.Length == 0 || playerDto.Formations[0].Characters.Length == 0)
                {
                    return;
                }

                CharacterDto characterDto = playerDto.Formations[0].Characters.OrderByDescending(x => x.Level).FirstOrDefault();

                string level = characterDto.Level.ToString();
                int max = formationDtoList.Count;

                for (int i = 1; i <= max; i++)
                {
                    FormationDto dto = formationDtoList[i - 1];

                    switch (i)
                    {
                        case 1:
                            if (dto.Characters.Length > 0)
                            {
                                TextLevel1_1.Text = level;
                                dto.Characters[0].Level = characterDto.Level;
                            }
                            if (dto.Characters.Length > 1)
                            {
                                TextLevel1_2.Text = level;
                                dto.Characters[1].Level = characterDto.Level;
                            }
                            if (dto.Characters.Length > 2)
                            {
                                TextLevel1_3.Text = level;
                                dto.Characters[2].Level = characterDto.Level;
                            }
                            if (dto.Characters.Length > 3)
                            {
                                TextLevel1_4.Text = level;
                                dto.Characters[3].Level = characterDto.Level;
                            }
                            if (dto.Characters.Length > 4)
                            {
                                TextLevel1_5.Text = level;
                                dto.Characters[4].Level = characterDto.Level;
                            }
                            break;
                        case 2:
                            if (dto.Characters.Length > 0)
                            {
                                TextLevel2_1.Text = level;
                                dto.Characters[0].Level = characterDto.Level;
                            }
                            if (dto.Characters.Length > 1)
                            {
                                TextLevel2_2.Text = level;
                                dto.Characters[1].Level = characterDto.Level;
                            }
                            if (dto.Characters.Length > 2)
                            {
                                TextLevel2_3.Text = level;
                                dto.Characters[2].Level = characterDto.Level;
                            }
                            if (dto.Characters.Length > 3)
                            {
                                TextLevel2_4.Text = level;
                                dto.Characters[3].Level = characterDto.Level;
                            }
                            if (dto.Characters.Length > 4)
                            {
                                TextLevel2_5.Text = level;
                                dto.Characters[4].Level = characterDto.Level;
                            }
                            break;
                        case 3:
                            if (dto.Characters.Length > 0)
                            {
                                TextLevel3_1.Text = level;
                                dto.Characters[0].Level = characterDto.Level;
                            }
                            if (dto.Characters.Length > 1)
                            {
                                TextLevel3_2.Text = level;
                                dto.Characters[1].Level = characterDto.Level;
                            }
                            if (dto.Characters.Length > 2)
                            {
                                TextLevel3_3.Text = level;
                                dto.Characters[2].Level = characterDto.Level;
                            }
                            if (dto.Characters.Length > 3)
                            {
                                TextLevel3_4.Text = level;
                                dto.Characters[3].Level = characterDto.Level;
                            }
                            if (dto.Characters.Length > 4)
                            {
                                TextLevel3_5.Text = level;
                                dto.Characters[4].Level = characterDto.Level;
                            }
                            break;
                        case 4:
                            if (dto.Characters.Length > 0)
                            {
                                TextLevel4_1.Text = level;
                                dto.Characters[0].Level = characterDto.Level;
                            }
                            if (dto.Characters.Length > 1)
                            {
                                TextLevel4_2.Text = level;
                                dto.Characters[1].Level = characterDto.Level;
                            }
                            if (dto.Characters.Length > 2)
                            {
                                TextLevel4_3.Text = level;
                                dto.Characters[2].Level = characterDto.Level;
                            }
                            if (dto.Characters.Length > 3)
                            {
                                TextLevel4_4.Text = level;
                                dto.Characters[3].Level = characterDto.Level;
                            }
                            if (dto.Characters.Length > 4)
                            {
                                TextLevel4_5.Text = level;
                                dto.Characters[4].Level = characterDto.Level;
                            }
                            break;
                    }
                }
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

        private void BtnCapture_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                TextBImageFilePath.Text = string.Empty;

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
                    ImgTarget.Source = null;
                    return;
                }

                // マッチングのために一時ファイルとして出力
                string matchTempDirPath = AppDomain.CurrentDomain.BaseDirectory + Common.Common.MATCH_TEMP_DIR;

                if (!Directory.Exists(matchTempDirPath))
                {
                    Directory.CreateDirectory(matchTempDirPath);
                }

                tempFilePath = matchTempDirPath + DateTime.Now.ToString("yyyyMMddHHmmss") + MATCH_TEMP_FILE_NAME;

                bitmap.Save(tempFilePath, ImageFormat.Bmp);

                ImgTarget.Source = ToImageSource(bitmap);
            }
            catch (Exception ex)
            {
                log.Error(MethodBase.GetCurrentMethod().Name + ":" + ex.ToString());
            }
        }

        private void CombGuild_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            try
            {
                PlayerDao playerDao = new PlayerDao();
                PlayerDto[] playerDtos = playerDao.GetPlayerDtos(CombGuild.SelectedItem.ToString());
                ListBPlayer.Items.Clear();

                foreach (PlayerDto dto in playerDtos)
                {
                    ListBPlayer.Items.Add(dto.Name);
                }
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

                CombGuild.Items.Add("");

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

        private void LoadCombBoxRarity()
        {
            try
            {
                foreach (string rarity in CharacterDto.RARITY)
                {
                    CombR1_1.Items.Add(rarity);
                    CombR1_2.Items.Add(rarity);
                    CombR1_3.Items.Add(rarity);
                    CombR1_4.Items.Add(rarity);
                    CombR1_5.Items.Add(rarity);

                    CombR2_1.Items.Add(rarity);
                    CombR2_2.Items.Add(rarity);
                    CombR2_3.Items.Add(rarity);
                    CombR2_4.Items.Add(rarity);
                    CombR2_5.Items.Add(rarity);

                    CombR3_1.Items.Add(rarity);
                    CombR3_2.Items.Add(rarity);
                    CombR3_3.Items.Add(rarity);
                    CombR3_4.Items.Add(rarity);
                    CombR3_5.Items.Add(rarity);

                    CombR4_1.Items.Add(rarity);
                    CombR4_2.Items.Add(rarity);
                    CombR4_3.Items.Add(rarity);
                    CombR4_4.Items.Add(rarity);
                    CombR4_5.Items.Add(rarity);
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

    class NativeMethods
    {
        [DllImport("gdi32.dll", EntryPoint = "DeleteObject")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DeleteObject([In] IntPtr hObject);
    }
}
