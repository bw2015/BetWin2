using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Xml;
using System.Xml.Linq;
using System.Web;

using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;

using BW.Agent;
using BW.Common.Games;
using BW.Common.Users;

using SP.Studio.Security;
using SP.Studio.Net;
using SP.Studio.Xml;
using SP.Studio.Web;
using SP.Studio.Model;

namespace BW.GateWay.Games
{
    /// <summary>
    /// OG的沙巴体育
    /// </summary>
    public class SB : IGame
    {
        public SB() : base() { }

        public SB(string setting) : base(setting) { }


        [Description("公钥")]
        public string PublicKey { get; set; }


        [Description("私钥")]
        public string PrivateKey { get; set; }

        [Description("公司代码")]
        public string CompanyCode { get; set; }

        [Description("前缀")]
        public string Prefix { get; set; }

        [Description("授权码")]
        public string AccessKey { get; set; }


        [Description("接口地址")]
        public string Gateway { get; set; }

        /// <summary>
        /// 登录地址
        /// </summary>
        [Description("登录地址")]
        public string LoginUrl { get; set; }

        /// <summary>
        /// 移动端的登录地址
        /// </summary>
        [Description("移动端地址")]
        public string MobileUrl { get; set; }

        public override bool CreateUser(int userId, params object[] args)
        {
            string playerName = this.GetPlayerName(userId);
            if (!string.IsNullOrEmpty(playerName)) return true;

            playerName = string.Concat(this.Prefix, GameAgent.Instance().CreatePlayerName(userId, 10));
            string password = Guid.NewGuid().ToString("N").Substring(0, 8);

            //get-access-key.php?r=ACTIVE_KEY|Pass key|Prefix code|prefix + Member Accountcode|User currency code| Online Key (User online key in your system , is unique key )=@=(as funny character splitter)";
            string source = string.Format("{0}|{1}|{2}|{3}|{4}|{5}=@=", "ACTIVE_KEY", this.AccessKey, this.Prefix, playerName, "RMB", userId);
            string finish = this.encyrpt(source, playerName) + "^" + playerName;
            string url;
            string result = this.getContent("get-access-key.php", finish, out url);
            this.SaveLog(userId, result, "Source", source, "URL", url);
            string msg;
            if (this.getInfo(result, out msg, "access_key"))
            {
                return UserAgent.Instance().AddGameAccount(userId, this.Type, playerName, password);
            }

            return false;
        }

        public override decimal GetBalance(int userId)
        {
            string playerName = this.GetPlayerName(userId);
            if (string.IsNullOrEmpty(playerName))
            {
                base.Message("暂未开户");
                return decimal.MinusOne;
            }

            //GET_CCL|AXf7jJsJSa|x4b0123456789|22222=@=
            string source = string.Format("GET_CCL|{0}|{1}|{2}=@=", this.AccessKey, playerName, userId);
            string finish = this.encyrpt(source, playerName) + "^" + playerName;
            string result = this.getContent("get-wallet-ccl.php", finish);
            this.SaveLog(userId, result);
            string msg;
            if (this.getInfo(result, out msg, "credit_left"))
            {
                return decimal.Parse(msg);
            }
            else
            {
                base.Message(msg);
                return decimal.MinusOne;
            }

        }

        public override bool Deposit(int userId, decimal money, string id, out decimal amount)
        {
            amount = decimal.MinusOne;
            string playerName = this.GetPlayerName(userId);
            if (string.IsNullOrEmpty(playerName))
            {
                base.Message("暂未开户");
                return false;
            }

            //GET_DEPOSIT|AXf7jJsJSa|2347834546|x4b0123456789|RMB|100|22222=@=
            string source = string.Format("GET_DEPOSIT|{0}|{1}|{2}|RMB|{3}|{4}=@=", this.AccessKey, id, playerName, (int)money, userId);
            string finish = this.encyrpt(source, playerName);
            string result = this.getContent("get_wallet_deposit.php", finish);
            this.SaveLog(userId, result);
            string msg;
            if (this.getInfo(result, out msg, "total_amount"))
            {
                amount = decimal.Parse(msg);
                return true;
            }

            base.Message(msg);
            return false;
        }

        public override bool Withdraw(int userId, decimal money, string orderId, out decimal amount)
        {
            amount = 0.00M;
            string playerName = this.GetPlayerName(userId);
            if (string.IsNullOrEmpty(playerName))
            {
                base.Message("暂未开户");
                return false;
            }

            //GET_WITHDRAW|AXf7jJsJSa|1234567|x4b0123456789|RMB|1000|22222=@= (as funny character splitter)
            string source = string.Format("GET_WITHDRAW|{0}|{1}|{2}|RMB|{3}|{4}=@=", this.AccessKey, orderId, playerName, money.ToString("0.00"), userId);
            string finish = this.encyrpt(source, playerName);
            string result = this.getContent("get_wallet_withdraw.php", finish);
            this.SaveLog(userId, result);

            string msg;
            if (this.getInfo(result, out msg, "total_amount"))
            {
                amount = decimal.Parse(msg);
                return true;
            }

            base.Message(msg);
            return false;
        }

        public override TransferStatus CheckTransfer(int userId, string id)
        {
            return TransferStatus.None;
        }

        public override void FastLogin(int userId, string key)
        {
            string playerName = this.GetPlayerName(userId);
            if (string.IsNullOrEmpty(playerName))
            {
                HttpContext.Current.Response.Write(false, "暂未开户");
            }

            string source = string.Format("{0}|{1}|{2}|{3}|{4}|{5}=@=", "ACTIVE_KEY", this.AccessKey, this.Prefix, playerName, "RMB", userId);
            string finish = this.encyrpt(source, playerName) + "^" + playerName;
            string result = this.getContent("get-access-key.php", finish);
            this.SaveLog(userId, result);
            string msg;
            if (this.getInfo(result, out msg, "access_key"))
            {
                Dictionary<string, string> input = new Dictionary<string, string>();
                input.Add("activekey", msg);
                input.Add("acc", playerName);
                input.Add("langs", "2");
                string loginUrl = this.LoginUrl;
                if (WebAgent.IsMobile())
                {
                    loginUrl = this.MobileUrl;
                }
                this.BuildForm(loginUrl, input, "get");
            }
            else
            {
                HttpContext.Current.Response.Write(false, msg);
            }
        }

        #region =============  私有方法  ==============

        /// <summary>
        /// 加密
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        private string encyrpt(string source, string playerName)
        {
            BlowfishCryptographer sce1 = new BlowfishCryptographer(true, this.PrivateKey + playerName);
            string result = sce1.getEncodestr(source);
            BlowfishCryptographer sce2 = new BlowfishCryptographer(true, this.PublicKey);
            string final = sce2.getEncodestr(result + "^" + playerName);

            return final;
        }

        /// <summary>
        /// 不需要用户名的加密
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        private string encyrpt(string source)
        {
            BlowfishCryptographer sce1 = new BlowfishCryptographer(true, this.PrivateKey + this.AccessKey);
            string result = sce1.getEncodestr(source);

            BlowfishCryptographer sce2 = new BlowfishCryptographer(true, this.PublicKey);
            string final = sce2.getEncodestr(result + "^" + this.AccessKey);

            return final;
        }

        /// <summary>
        /// 获取远程API返回的结果
        /// </summary>
        /// <param name="file_access"></param>
        /// <param name="final"></param>
        /// <returns></returns>
        private string getContent(string file_access, string final)
        {
            string url = this.Gateway + file_access + "?r=" + final;
            return NetAgent.DownloadData(url, Encoding.UTF8);
        }

        private string getContent(string file_access, string final, out string url)
        {
            url = this.Gateway + file_access + "?r=" + final;
            return NetAgent.DownloadData(url, Encoding.UTF8);
        }

        /// <summary>
        /// 获取返回的值，判断是否正确
        /// </summary>
        /// <param name="result"></param>
        /// <param name="successPath">正确的路径</param>
        /// <param name="msg"></param>
        /// <returns></returns>
        private bool getInfo(string result, out string msg, string successKey)
        {
            msg = string.Empty;
            XElement root = XElement.Parse(result);
            bool success = false;
            switch (root.Name.LocalName)
            {
                case "error":
                    msg = root.Element("error_code").Value;
                    if (this.errorCode.ContainsKey(msg))
                    {
                        base.Message("[{0}] {1}", msg, this.errorCode[msg]);
                    }
                    else
                    {
                        base.Message(msg);
                    }
                    break;
                case "success":
                    msg = root.Element(successKey).Value;
                    success = true;
                    break;
            }

            return success;
        }

        private Dictionary<string, string> _errorCode;
        private Dictionary<string, string> errorCode
        {
            get
            {
                if (this._errorCode != null) return _errorCode;

                _errorCode = new Dictionary<string, string>();
                string key = @"INVALID-k 密钥错误
RAAK-1a 加密错误
RAAK-1b IP未授权
RAAK-1c 加密错误
RAAK-1d 用户名长度错误
RAAK-1e 加密错误
RAAK-1f 用户名已经存在
RAAK-1g 代理号不存在
RAAK-1h 用户名不符合规则
RAAK-1i bsh验证错误
RAAK-1j 不存在的动作
RAAK-2a-1 系统繁忙
RAAK-4a 用户被阻止
RAAK-5a 用户暂停
RAAK-6a 不支持的币种
RAAK-1z 注册失败";

                foreach (string line in key.Split('\n'))
                {
                    string[] name = line.Split(' ');
                    if (_errorCode.ContainsKey(name[0])) continue;
                    _errorCode.Add(name[0], name[1]);
                }

                return _errorCode;
            }
        }

        #endregion

        /// <summary>
        /// 加密算法
        /// </summary>
        private class BlowfishCryptographer
        {
            private bool forEncryption;
            private IBufferedCipher cipher;
            public string defaultIV;

            public BlowfishCryptographer(bool forEncryption, string key)
            {
                this.forEncryption = forEncryption;
                cipher = new BufferedBlockCipher(new CfbBlockCipher(new BlowfishEngine(), 8));
                key = setKey(key);
                Random rm = new Random();
                int random = rm.Next(10000000, 99999999);
                defaultIV = Convert.ToBase64String(Encoding.ASCII.GetBytes(Convert.ToString(random)));
                cipher.Init(forEncryption, new ParametersWithIV(new KeyParameter(Encoding.ASCII.GetBytes(setKey(key))), Convert.FromBase64String(defaultIV)));
            }

            public void ReInit(byte[] IV, BigInteger pubkey)
            {
                cipher.Init(forEncryption, new ParametersWithIV(new KeyParameter(pubkey.ToByteArrayUnsigned()), IV));
            }

            public byte[] DoFinal()
            {
                return cipher.DoFinal();
            }

            public byte[] DoFinal(byte[] buffer)
            {
                return cipher.DoFinal(buffer);
            }

            public string getEncodestr(string buffer)
            {

                byte[] temp1 = cipher.DoFinal(Encoding.ASCII.GetBytes(buffer));


                byte[] temp2 = Convert.FromBase64String(defaultIV);

                byte[] temp3 = new byte[temp1.Length + temp2.Length];

                temp2.CopyTo(temp3, 0);
                temp1.CopyTo(temp3, temp2.Length);



                string IV_encryptedStr = Convert.ToBase64String(temp3);
                return IV_encryptedStr;
            }

            public byte[] DoFinal(byte[] buffer, int startIndex, int len)
            {
                return cipher.DoFinal(buffer, startIndex, len);
            }

            public byte[] ProcessBytes(byte[] buffer)
            {
                return cipher.ProcessBytes(buffer);
            }

            public byte[] ProcessBytes(byte[] buffer, int startIndex, int len)
            {
                return cipher.ProcessBytes(buffer, startIndex, len);
            }

            public void Reset()
            {
                cipher.Reset();
            }

            public string md5(string key)
            {
                System.Security.Cryptography.MD5CryptoServiceProvider get_md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
                byte[] hash_byte = get_md5.ComputeHash(Encoding.ASCII.GetBytes(key));
                string result = System.BitConverter.ToString(hash_byte);
                return result;
            }

            public string setKey(string key)
            {
                string rekey;
                int keysize = 56;
                if (key.Length < 32 && keysize >= 32)
                    rekey = md5(key);
                else if (key.Length > keysize && keysize == 32)
                    rekey = md5(key);
                else
                {
                    if (keysize > key.Length)
                    {
                        rekey = key;
                        for (int i = key.Length; i < keysize; i++)
                            rekey += " ";

                    }
                    else
                        rekey = key.Substring(0, keysize);
                }

                return rekey;

            }
        }
    }
}
