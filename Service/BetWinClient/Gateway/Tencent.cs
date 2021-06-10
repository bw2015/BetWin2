using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.ComponentModel;

using SP.Studio.Web;

using SP.Studio.Net;

namespace BetWinClient.Gateway
{
    /// <summary>
    /// 腾讯分分彩
    /// </summary>
    public partial class Tencent : IGateway
    {

        /// <summary>
        /// 转换
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        private string getNumber(string number)
        {
            string num = (number.Select(t => int.Parse(t.ToString())).Sum() % 10).ToString();
            num += number.Substring(number.Length - 4);
            return num;
        }

        /// <summary>
        /// 从M彩获取
        /// </summary>
        /// <returns></returns>
        private Dictionary<string, string> getResultByMCai()
        {
            return Utils.GetAPI(API.mcai, this.GetType());
        }
    }
}
