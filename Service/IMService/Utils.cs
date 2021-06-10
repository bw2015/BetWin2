using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.ComponentModel;

using IMService.Common;
using IMService.Agent;
using IMService.Framework;

using Fleck;
using SP.Studio.Array;
using SP.Studio.Web;

namespace IMService
{
    public class Utils
    {
        /// <summary>
        /// 从接收者的ID中获取用户类型
        /// </summary>
        /// <param name="id">客户端标识</param>
        /// <param name="userId">客户端ID，游客为0</param>
        /// <returns></returns>
        public static UserType GetUserType(string id, out int userId)
        {
            Regex regex = new Regex(@"^(?<Type>ADMIN|USER|GUEST)\-(?<Value>\w+)");
            if (!regex.IsMatch(id)) { userId = 0; return UserType.None; }

            string type = regex.Match(id).Groups["Type"].Value;
            string value = regex.Match(id).Groups["Value"].Value;
            userId = 0;
            UserType userType = UserType.None;
            switch (type)
            {
                case "ADMIN":
                    userType = UserType.Admin;
                    userId = int.Parse(value);
                    break;
                case "USER":
                    userType = UserType.User;
                    userId = int.Parse(value);
                    break;
                case "GUEST":
                    userType = UserType.Guest;
                    userId = WebAgent.GetNumber(Guid.Parse(value));
                    break;
            }
            return userType;
        }

        /// <summary>
        /// 图片路径
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string GetImage(string path)
        {
            return BW.Framework.SysSetting.GetSetting().GetImage(path);
        }


        private static Dictionary<string, string> _ip = new Dictionary<string, string>();
    }
}
