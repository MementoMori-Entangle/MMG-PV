using System;
using System.Security.Cryptography;
using System.Text;

namespace MMG.Common
{
    public class Auth
    {
        private readonly string BASE_SECRET_KEY = "MementoMori-Entangle";
        private readonly int PASS_LENGTH = 8;

        public Auth() { }

        public bool IsLogin(string password)
        {
            bool isLogin = false;

            string checkPass = GetLoginPassowrd();

            if (checkPass.Equals(password))
            {
                isLogin = true;
            }

            return isLogin;
        }

        internal string GetLoginPassowrd()
        {
            DateTime nowDT = GetNowDateTime();

            int weeks = (nowDT.Day - 1) / 7 + 1;

            DateTime dateTime = new DateTime(nowDT.Year, nowDT.Month, weeks);

            long time = dateTime.ToFileTimeUtc();

            string hash = GetHash(BASE_SECRET_KEY + time + BASE_SECRET_KEY);

            StringBuilder sb = new StringBuilder();
            bool isComp = true;
            int cnt = 1;

            while (isComp)
            {
                foreach (char chara in hash)
                {
                    if (cnt % 2 != 0)
                    {
                        sb.Append(chara);
                    }

                    cnt++;

                    if (PASS_LENGTH * 2 < cnt)
                    {
                        isComp = false;
                        break;
                    }
                }
            }

            return sb.ToString();
        }

        private DateTime GetNowDateTime()
        {
            DateTime dateTime = DateTime.Now;

            try
            {
                if (!AssemblyState.IsDebug)
                {
                    dateTime = GetTimeByNTP();
                }
            }
            catch (Exception e)
            {
                throw e;
            }

            return dateTime;
        }

        private string GetHash(string targetStr)
        {
            string hash = string.Empty;

            try
            {
                var targetBytes = Encoding.UTF8.GetBytes(targetStr);
                var csp = new MD5CryptoServiceProvider();
                var hashBytes = csp.ComputeHash(targetBytes);
                var hashStr = new StringBuilder();

                foreach (var hashByte in hashBytes)
                {
                    hashStr.Append(hashByte.ToString("x2"));
                }

                hash = hashStr.ToString();
            }
            catch (Exception)
            {

            }

            return hash;
        }

        private DateTime GetTimeByNTP(string ntp = "time.windows.com")
        {
            try
            {
                // NTPサーバへの接続用UDP生成
                System.Net.Sockets.UdpClient objSck;
                System.Net.IPEndPoint ipAny = new System.Net.IPEndPoint(System.Net.IPAddress.Any, 0);
                objSck = new System.Net.Sockets.UdpClient(ipAny);

                // NTPサーバへのリクエスト送信
                byte[] sdat = new byte[48];
                sdat[0] = 0xB;
                objSck.Send(sdat, sdat.GetLength(0), ntp, 123);

                // NTPサーバから日時データ受信
                byte[] rdat = objSck.Receive(ref ipAny);

                // 1900年1月1日からの経過時間(日時分秒)
                long lngAllS; // 1900年1月1日からの経過秒数
                long lngD;    // 日
                long lngH;    // 時
                long lngM;    // 分
                long lngS;    // 秒

                // 1900年1月1日からの経過秒数計算
                lngAllS = (long)(
                          rdat[40] * Math.Pow(2, (8 * 3)) +
                          rdat[41] * Math.Pow(2, (8 * 2)) +
                          rdat[42] * Math.Pow(2, (8 * 1)) +
                          rdat[43]);

                // 1900年1月1日からの経過(日時分秒)計算
                lngD = lngAllS / (24 * 60 * 60); // 日
                lngS = lngAllS % (24 * 60 * 60); // 残りの秒数
                lngH = lngS / (60 * 60);         // 時
                lngS = lngS % (60 * 60);         // 残りの秒数
                lngM = lngS / 60;                // 分
                lngS = lngS % 60;                // 秒

                // 現在の日時(DateTime)計算
                DateTime dateTime = new DateTime(1900, 1, 1);
                dateTime = dateTime.AddDays(lngD);
                dateTime = dateTime.AddHours(lngH);
                dateTime = dateTime.AddMinutes(lngM);
                dateTime = dateTime.AddSeconds(lngS);

                // グリニッジ標準時から日本時間への変更
                dateTime = dateTime.AddHours(9);

                return dateTime;
            }
            catch (Exception e)
            {
                throw e;
            }
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
