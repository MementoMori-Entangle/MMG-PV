using MMG.Dto;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MMG.Common
{
    public class BattleCalculations
    {
        public readonly int DETERMINED_SPEED_DIFFERENCE = 135;

        public BattleCalculations() { }

        public Dictionary<string, string> GetPlayerVSPlayer(Random random, FormationDto leftFD, FormationDto rightFD, int turnCnt = 0, int invasionKO = 0, int defenseKO = 0)
        {
            Dictionary<string, string> battleDic = new Dictionary<string, string>();

            try
            {
                /* レベル・戦力差(攻撃力、貫通、HPなど)によるスピード関係なく絶対的に
                 * 勝てる勝てない境界値を判断するのが難しいため、段階的に計算を追加する。
                 * 1. スピード差による勝敗判定(乱数で最大135までは確定しない)
                 * 　 一般的には最大火力に最大スピードルーンが設定されている(オフィーリアやアーティなどは特殊枠)
                 * 2.  + レベル・戦力差による勝利判定(装備を無視すればレベル50差?くらいまではスピード勝ちすれば勝利する確率が高い?)
                 * 3.  + カウンタ(オフィーリア)編成による勝利判定(こちらのドレインと相手のカウンタ・ドレインを追加でみる必要?)
                 * 
                 * ギルド計算資料
                 * https://wikiwiki.jp/mememori/%E3%82%AE%E3%83%AB%E3%83%89%E3%83%90%E3%83%88%E3%83%AB
                 */

                // コルディ、フローレンス、ニーナなら一先ず確定で勝利テスト
                // 編成にルサールカメタトロン武器で初回スピード30%アップ場合、
                // スピードルーン合計330から290以下になるとコルディより先にルサールカが対象となるため、次点もチェックする。
                // スキルでトロポン永続スピード20%アップ、ステラ、初回25%アップもあるので注意

                // スピードトップのキャラの火力で相手側を確実に殲滅できるか判定
                // 1ターン目で殲滅判定が出ない場合は2ターン目・・・最大40ターンまで判定する必要があるが、
                // 計算が難しすぎるので1ターンで勝てない場合は、負けということにする。

                // 侵攻KO数と防衛KO数では疲労度の値が異なるので注意
                // 侵攻は1KO毎に1%、11KO後は1KO毎に0.2%減る、防衛は1KO毎0.3%減る(スピード限定)

                CalculationSpeed(leftFD, turnCnt, invasionKO, 0);
                CalculationSpeed(rightFD, turnCnt, 0, defenseKO);

                // コルディ、フローレンス、ニーナいたら基本勝ちとする、お互いにいた場合はスピード対決で判定
                battleDic = JudgeAppropriately(random, leftFD, rightFD);
            }
            catch (Exception e)
            {
                throw e;
            }

            return battleDic;
        }

        private Dictionary<string, string> JudgeAppropriately(Random random, FormationDto leftFD, FormationDto rightFD)
        {
            Dictionary<string, string> battleDic = new Dictionary<string, string>();

            try
            {
                int leftCheckCnt = leftFD.Characters.Length;
                int rightCheckCnt = rightFD.Characters.Length;
                bool isLeftWin = false;
                bool isRightWin = false;
                string winNames = "コルディ、フローレンス、ニーナ?"; // 各種ステータスで複雑な計算は難しいので・・・
                int leftProbability = random.Next(1, 9);
                int rightProbability = random.Next(1, 9);

                for (int i = 0; i < leftCheckCnt; i++)
                {
                    CharacterDto leftCheckSpeedDto = leftFD.Characters.OrderByDescending(characterDto => characterDto.Speed).Skip(i).FirstOrDefault();

                    if (winNames.Contains(leftCheckSpeedDto.Name))
                    {
                        // スピード対決
                        for (int j = 0; j < rightCheckCnt; j++)
                        {
                            CharacterDto rightCheckSpeedDto = rightFD.Characters.OrderByDescending(characterDto => characterDto.Speed).Skip(j).FirstOrDefault();

                            if (winNames.Contains(rightCheckSpeedDto.Name))
                            {
                                if (IsSpeedDifferenceJudgment(leftCheckSpeedDto.Speed, rightCheckSpeedDto.Speed, leftProbability, rightProbability))
                                {
                                    isLeftWin = true;
                                }
                                else
                                {
                                    isRightWin = true;
                                }
                            }
                            else
                            {
                                isLeftWin = true;
                            }

                            if (isLeftWin || isRightWin)
                            {
                                break;
                            }
                        }
                    }
                    else
                    {
                        // 必勝キャラがいない場合、相手側に必勝キャラがいる場合は負け、いない場合は先行行動順が半数以上多い方が勝利とする
                        for (int j = 0; j < rightCheckCnt; j++)
                        {
                            CharacterDto rightCheckSpeedDto = rightFD.Characters.OrderByDescending(characterDto => characterDto.Speed).Skip(j).FirstOrDefault();

                            if (winNames.Contains(rightCheckSpeedDto.Name))
                            {
                                isRightWin = true;
                            }

                            if (isRightWin)
                            {
                                break;
                            }
                        }

                        if (!isLeftWin && !isRightWin)
                        {
                            CharacterDto[] leftCheckSpeedDtos = leftFD.Characters.OrderByDescending(characterDto => characterDto.Speed).ToArray();
                            CharacterDto[] rightCheckSpeedDtos = rightFD.Characters.OrderByDescending(characterDto => characterDto.Speed).ToArray();

                            List<CharacterDto> checkSpeedDtoList = new List<CharacterDto>(leftCheckSpeedDtos.Length + rightCheckSpeedDtos.Length);

                            foreach (CharacterDto dto in leftCheckSpeedDtos)
                            {
                                dto.IsLeft = true;
                            }

                            checkSpeedDtoList.AddRange(leftCheckSpeedDtos);
                            checkSpeedDtoList.AddRange(rightCheckSpeedDtos);

                            checkSpeedDtoList = checkSpeedDtoList.OrderByDescending(x => x.Speed).ToList();

                            int maxCnt = (int)Math.Ceiling(checkSpeedDtoList.Count / 2d);
                            int leftCnt = 0;
                            int rightCnt = 0;

                            foreach (CharacterDto dto in checkSpeedDtoList)
                            {
                                if (dto.IsLeft)
                                {
                                    leftCnt++;
                                }
                                else
                                {
                                    rightCnt++;
                                }

                                if (maxCnt <= leftCnt)
                                {
                                    isLeftWin = true;
                                    break;
                                }
                                else if (maxCnt <= rightCnt)
                                {
                                    isRightWin = true;
                                    break;
                                }
                            }
                        }
                    }

                    if (isLeftWin || isRightWin)
                    {
                        break;
                    }
                }

                if (isLeftWin)
                {
                    battleDic.Add("result", "left");
                }
                else
                {
                    battleDic.Add("result", "right");
                }
            }
            catch (Exception e)
            {
                throw e;
            }

            return battleDic;
        }

        private bool IsSpeedDifferenceJudgment(int leftSpeed, int rightSpeed, int leftProbability, int rightProbability)
        {
            bool isWin = false;

            try
            {
                if (leftSpeed - (DETERMINED_SPEED_DIFFERENCE - ((int)(DETERMINED_SPEED_DIFFERENCE / 10f)) * leftProbability) >
                    rightSpeed - (DETERMINED_SPEED_DIFFERENCE - ((int)(DETERMINED_SPEED_DIFFERENCE / 10f)) * rightProbability))
                {
                    isWin = true;
                }
            }
            catch (Exception e)
            {
                throw e;
            }

            return isWin;
        }

        public static void CalculationSpeed(FormationDto formationDto, int turnCnt = 0, int invasionKO = 0, int defenseKO = 0)
        {
            try
            {
                BaseCharacterDto[] baseCharacterDtos = BaseCharacter.GetBaseCharacterDtos();
                int allBuffSpeed = 1; // 現状プリマヴェーラのみ
                int aLiveCnt = 0;
                float koDebuffSpeed = 0f;

                foreach (CharacterDto characterDto in formationDto.Characters)
                {
                    if (characterDto.IsAlive)
                    {
                        aLiveCnt++;
                    }
                }

                // キャラ基礎スビートとスピードルーンの合計を設定
                foreach (CharacterDto characterDto in formationDto.Characters)
                {
                    BaseCharacterDto baseDto = baseCharacterDtos.Where(x => x.Id == characterDto.Id).First();

                    characterDto.Speed = baseDto.Speed + RuneDto.Speed[characterDto.SpeedRune1]
                                                       + RuneDto.Speed[characterDto.SpeedRune2]
                                                       + RuneDto.Speed[characterDto.SpeedRune3];

                    // プリマヴェーラなら全体バフ(永続)設定
                    if ("プリマヴェーラ".Equals(characterDto.Name))
                    {
                        allBuffSpeed = baseDto.SpeedSkillBuff[turnCnt] * aLiveCnt;
                    }

                    // 個別スキルによる自身のスピードバフ設定(ターンによって付与されるされないがある)
                    if ("トロポン".Equals(characterDto.Name))
                    {
                        if (baseDto.SpeedSkillBuff.Length < turnCnt)
                        {
                            break;
                        }

                        characterDto.Speed = characterDto.Speed + (int)(characterDto.Speed * (baseDto.SpeedSkillBuff[turnCnt] / 100f));
                    }
                    else if ("ステラ".Equals(characterDto.Name) && characterDto.EWeapon > 0)
                    {
                        if (baseDto.SpeedEWBuff.Length < turnCnt)
                        {
                            break;
                        }

                        characterDto.Speed = characterDto.Speed + (int)(characterDto.Speed * (baseDto.SpeedEWBuff[turnCnt] / 100f));
                    }
                    else if ("[天泣の魔女]ルサールカ".Equals(characterDto.Name) && characterDto.EWeapon == 3)
                    {
                        if (baseDto.SpeedEWBuff.Length < turnCnt)
                        {
                            break;
                        }

                        characterDto.Speed = characterDto.Speed + (int)(characterDto.Speed * (baseDto.SpeedEWBuff[turnCnt] / 100f));
                    }
                }

                if (allBuffSpeed > 1)
                {
                    foreach (CharacterDto characterDto in formationDto.Characters)
                    {
                        characterDto.Speed = characterDto.Speed + (int)(characterDto.Speed * (allBuffSpeed / 100f));
                    }
                }

                // KO数に応じてスピード減少計算
                if (invasionKO > 0)
                {
                    if (invasionKO > 0 && invasionKO < 11)
                    {
                        koDebuffSpeed = invasionKO * 1f;
                    }
                    else
                    {
                        koDebuffSpeed = 10f + (invasionKO * 0.2f);
                    }
                }
                else if (defenseKO > 0)
                {
                    koDebuffSpeed = defenseKO * 0.3f;
                }

                if (koDebuffSpeed > 0)
                {
                    foreach (CharacterDto characterDto in formationDto.Characters)
                    {
                        characterDto.Speed = (int)(characterDto.Speed * ((100 - koDebuffSpeed) / 100));
                    }
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}
