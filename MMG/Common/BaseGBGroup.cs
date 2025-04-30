using MMG.Dto;
using System.Windows;

namespace MMG.Common
{
    public class BaseGBGroup
    {
        private static readonly string[][] baseNames = new string[][]
        {
            new string[]
            {
                "", "ブラッセル", "グラベンスティン", "ウィスケルケー", "シメイ",
                "モダーヴ", "サルヴァトール", "バーフ", "カンブル", "イーペル", 
                "コルトレイク", "クリストフ", "モンス", "ワーヴル", "ナミュール",
                "クインティヌス", "ランベール", "サンジャック", "ミヒャエル",
                "シャルルロア", "エノー", "アルゼット"
            },
            new string[]
            {
                "", "アイン", "テファレト", "イエソド", "ケテル",
                "マルクト", "ガネット", "ルラ", "クシェル", "フロライト",
                "オニキス", "ジルコン", "ラペン", "アメト", "ファリア",
                "シトリ", "トパズ", "メラル", "ペリド",
                "ラピス", "マリン", "ラリマル"
            }
        };

        private static readonly int[][] baseIds = new int[][]
        {
            new int[]
            {
                100, 101, 102, 103, 104, 105, 106, 107, 108, 109, 110, 111,
                112, 113, 114, 115, 116, 117, 118, 119, 120, 121, 122
            },
            new int[]
            {
                200, 201, 202, 203, 204, 205, 206, 207, 208, 209, 210, 211,
                212, 213, 214, 215, 216, 217, 218, 219, 220, 221, 222
            }
        };

        private static readonly int[][] baseCharacterNums = new int[][]
        {
            new int []
            {
                0, 3, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2
            },
            new int []
            {
                0, 3, 2, 2, 2, 2, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1
            }
        };

        public BaseGBGroup() { }

        public static GBGroupDto[] GetBaseGBGroup(int type = 0)
        {
            GBGroupDto[] gBGroupDto = new GBGroupDto[]
            {
                new GBGroupDto()
                {
                    Id = baseIds[type][0],
                    Type = 0,
                    No = 0,
                    CharacterNum = baseCharacterNums[type][0],
                    Name = baseNames[type][0]
                },
                new GBGroupDto()
                {
                    Id = baseIds[type][1],
                    Type = 1,
                    No = 1,
                    CharacterNum = baseCharacterNums[type][1],
                    BaseRect = new Rect(1161, 932, 132, 120),
                    Name = baseNames[type][1]
                },
                new GBGroupDto()
                {
                    Id = baseIds[type][2],
                    Type = 2,
                    No = 2,
                    CharacterNum = baseCharacterNums[type][2],
                    BaseRect = new Rect(853, 712, 111, 96),
                    Name = baseNames[type][2]
                },
                new GBGroupDto()
                {
                    Id = baseIds[type][3],
                    Type = 2,
                    No = 3,
                    CharacterNum = baseCharacterNums[type][3],
                    BaseRect = new Rect(1605, 872, 111, 96),
                    Name = baseNames[type][3]
                },
                new GBGroupDto()
                {
                    Id = baseIds[type][4],
                    Type = 2,
                    No = 4,
                    CharacterNum = baseCharacterNums[type][4],
                    BaseRect = new Rect(753, 1212, 111, 96),
                    Name = baseNames[type][4]
                },
                new GBGroupDto()
                {
                    Id = baseIds[type][5],
                    Type = 2,
                    No = 5,
                    CharacterNum = baseCharacterNums[type][5],
                    BaseRect = new Rect(1378, 1222, 111, 96),
                    Name = baseNames[type][5]
                },
                new GBGroupDto()
                {
                    Id = baseIds[type][6],
                    Type = 3,
                    No = 6,
                    CharacterNum = baseCharacterNums[type][6],
                    BaseRect = new Rect(636, 312, 111, 72),
                    Name = baseNames[type][6]
                },
                new GBGroupDto()
                {
                    Id = baseIds[type][7],
                    Type = 3,
                    No = 7,
                    CharacterNum = baseCharacterNums[type][7],
                    BaseRect = new Rect(1030, 236, 111, 72),
                    Name = baseNames[type][7]
                },
                new GBGroupDto()
                {
                    Id = baseIds[type][8],
                    Type = 3,
                    No = 8,
                    CharacterNum = baseCharacterNums[type][8],
                    BaseRect = new Rect(1317, 584, 111, 72),
                    Name = baseNames[type][8]
                },
                new GBGroupDto()
                {
                    Id = baseIds[type][9],
                    Type = 3,
                    No = 9,
                    CharacterNum = baseCharacterNums[type][9],
                    BaseRect = new Rect(445, 596, 111, 72),
                    Name = baseNames[type][9]
                },
                new GBGroupDto()
                {
                    Id = baseIds[type][10],
                    Type = 3,
                    No = 10,
                    CharacterNum = baseCharacterNums[type][10],
                    BaseRect = new Rect(305, 936, 111, 72),
                    Name = baseNames[type][10]
                },
                new GBGroupDto()
                {
                    Id = baseIds[type][11],
                    Type = 3,
                    No = 11,
                    CharacterNum = baseCharacterNums[type][11],
                    BaseRect = new Rect(437, 1294, 111, 72),
                    Name = baseNames[type][11]
                },
                new GBGroupDto()
                {
                    Id = baseIds[type][12],
                    Type = 3,
                    No = 12,
                    CharacterNum = baseCharacterNums[type][12],
                    BaseRect = new Rect(785, 1802, 111, 72),
                    Name = baseNames[type][12]
                },
                new GBGroupDto()
                {
                    Id = baseIds[type][13],
                    Type = 3,
                    No = 13,
                    CharacterNum = baseCharacterNums[type][13],
                    BaseRect = new Rect(1022, 1434, 111, 72),
                    Name = baseNames[type][13]
                },
                new GBGroupDto()
                {
                    Id = baseIds[type][14],
                    Type = 3,
                    No = 14,
                    CharacterNum = baseCharacterNums[type][14],
                    BaseRect = new Rect(1508, 230, 111, 72),
                    Name = baseNames[type][14]
                },
                new GBGroupDto()
                {
                    Id = baseIds[type][15],
                    Type = 3,
                    No = 15,
                    CharacterNum = baseCharacterNums[type][15],
                    BaseRect = new Rect(1877, 426, 111, 72),
                    Name = baseNames[type][15]
                },
                new GBGroupDto()
                {
                    Id = baseIds[type][16],
                    Type = 3,
                    No = 16,
                    CharacterNum = baseCharacterNums[type][16],
                    BaseRect = new Rect(2160, 636, 111, 72),
                    Name = baseNames[type][16]
                },
                new GBGroupDto()
                {
                    Id = baseIds[type][17],
                    Type = 3,
                    No = 17,
                    CharacterNum = baseCharacterNums[type][17],
                    BaseRect = new Rect(2053, 1034, 111, 72),
                    Name = baseNames[type][17]
                },
                new GBGroupDto()
                {
                    Id = baseIds[type][18],
                    Type = 3,
                    No = 18,
                    CharacterNum = baseCharacterNums[type][18],
                    BaseRect = new Rect(1776, 1216, 111, 72),
                    Name = baseNames[type][18]
                },
                new GBGroupDto()
                {
                    Id = baseIds[type][19],
                    Type = 3,
                    No = 19,
                    CharacterNum = baseCharacterNums[type][19],
                    BaseRect = new Rect(1542, 1558, 111, 72),
                    Name = baseNames[type][19]
                },
                new GBGroupDto()
                {
                    Id = baseIds[type][20],
                    Type = 3,
                    No = 20,
                    CharacterNum = baseCharacterNums[type][20],
                    BaseRect = new Rect(1190, 1732, 111, 72),
                    Name = baseNames[type][20]
                },
                new GBGroupDto()
                {
                    Id = baseIds[type][21],
                    Type = 3,
                    No = 21,
                    CharacterNum = baseCharacterNums[type][21],
                    BaseRect = new Rect(1414, 1960, 111, 72),
                    Name = baseNames[type][21]
                }
            };

            return gBGroupDto;
        }
    }
}
