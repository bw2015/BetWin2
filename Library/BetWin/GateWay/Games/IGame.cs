using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

using BW.Agent;
using BW.Common.Games;
using SP.Studio.Core;
using System.ComponentModel;

namespace BW.GateWay.Games
{
    /// <summary>
    /// 第三方游戏接口的抽象接口
    /// </summary>
    public abstract class IGame : SettingBase
    {
        public IGame() : base() { }

        public IGame(string setting) : base(setting) { }

        /// <summary>
        /// 当前的http请求对象
        /// </summary>
        protected HttpContext context
        {
            get
            {
                return HttpContext.Current;
            }
        }

        /// <summary>
        /// 第三方游戏的类型
        /// </summary>
        public virtual GameType Type
        {
            get
            {
                return this.GetType().Name.ToEnum<GameType>();
            }
        }

        protected virtual string Message(string message = null, params object[] args)
        {
            if (!string.IsNullOrEmpty(message)) UserAgent.Instance().Message(message, args);
            return UserAgent.Instance().Message();
        }

        /// <summary>
        /// 获取玩家的第三方游戏用户名
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        protected virtual string GetPlayerName(int userId)
        {
            return UserAgent.Instance().GetPlayerName(userId, this.Type);
        }

        /// <summary>
        /// 提交资料到登录接口
        /// </summary>
        /// <param name="url">登录接口地址</param>
        /// <param name="dic">当前要提交的信息</param>
        /// <param name="method"></param>
        protected virtual void BuildForm(string url, Dictionary<string, string> dic, string method = "POST")
        {
            if (dic == null) dic = new Dictionary<string, string>();
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("<form action=\"{0}\" method=\"{1}\" id=\"{2}\">", url, method, this.Type)
                .Append(string.Join("\n", dic.Select(t => string.Format("<input type=\"hidden\" name=\"{0}\" value=\"{1}\" />", t.Key, t.Value))))
                .Append("</form>")
                .AppendFormat("<script type=\"text/javascript\"> if(document.getElementById(\"{0}\") != null)  document.getElementById(\"{0}\").submit();  </script>", this.Type);

            HttpContext.Current.Response.Write(sb);
            HttpContext.Current.Response.End();
        }

        /// <summary>
        /// 保存于第三方通信过程中的日志信息
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="message"></param>
        protected virtual void SaveLog(int userId, string message, params string[] args)
        {
            SystemAgent.Instance().SaveGameGatewayLog(userId, this.Type, message, args);
        }

        /// <summary>
        /// 保存通讯日志（包括参数值）
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="message"></param>
        /// <param name="dic"></param>
        /// <param name="args"></param>
        protected virtual void SaveLog(int userId, string message, Dictionary<string, string> dic, params string[] args)
        {
            List<string> param = new List<string>();
            foreach (KeyValuePair<string, string> keyValue in dic)
            {
                param.Add(keyValue.Key);
                param.Add(keyValue.Value);
            }
            param.AddRange(args);
            SystemAgent.Instance().SaveGameGatewayLog(userId, this.Type, message, param.ToArray());
        }

        /// <summary>
        /// 创建一个用户
        /// </summary>
        /// <param name="userName">用户名</param>
        /// <param name="args">扩展参数</param>
        /// <returns>如果存在用户返回true</returns>
        public abstract bool CreateUser(int userId, params object[] args);

        /// <summary>
        /// 查询余额
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <returns></returns>
        public abstract decimal GetBalance(int userId);

        /// <summary>
        /// 玩家存款（注：此处存款只调用接口，不判断用户余额是否足够）
        /// </summary>
        /// <param name="userId">玩家编号</param>
        /// <param name="money"></param>
        /// <param name="id">存款交易编号，可用于查询是否到账</param>
        /// <param name="amount">存款成功之后的余额</param>
        /// <returns></returns>
        public abstract bool Deposit(int userId, decimal money, string id, out decimal amount);

        /// <summary>
        /// 玩家提款
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="money"></param>
        /// <param name="orderId">提款的编号</param>
        /// <param name="amount">提款完成之后的余额</param>
        /// <returns></returns>
        public abstract bool Withdraw(int userId, decimal money, string orderId, out decimal amount);

        /// <summary>
        /// 查询玩家的交易记录（通过交易编码检查是否成功）
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public abstract TransferStatus CheckTransfer(int userId, string id);

        /// <summary>
        /// 快速登录（进入游戏）
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="key">游戏编号（标识）</param>
        public abstract void FastLogin(int userId, string key);

        /// <summary>
        /// 转账状态
        /// </summary>
        public enum TransferStatus
        {
            /// <summary>
            /// 无法查询状态
            /// </summary>
            [Description("无法查询")]
            None,
            /// <summary>
            /// 成功
            /// </summary>
            [Description("转账成功")]
            Success,
            /// <summary>
            /// 失败
            /// </summary>
            [Description("转账失败")]
            Faild,
            /// <summary>
            /// 其他错误
            /// </summary>
            [Description("其他错误")]
            Other
        }
    }
}
