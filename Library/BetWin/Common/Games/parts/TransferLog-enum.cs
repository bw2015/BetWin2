using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

using BW.Agent;
using BW.GateWay.Games;

namespace BW.Common.Games
{
    /// <summary>
    /// 转账日志
    /// </summary>
    partial class TransferLog
    {
        public enum TransferStatus : byte
        {
            /// <summary>
            /// 等待检查
            /// </summary>
            [Description("等待检查")]
            None = 0,
            /// <summary>
            /// 转账成功
            /// </summary>
            [Description("转账成功")]
            Success = 1,
            /// <summary>
            /// 失败
            /// </summary>
            [Description("转账失败")]
            Faild = 2,
            /// <summary>
            /// 错误数据（可能是没有资金锁定等等）
            /// </summary>
            [Description("错误")]
            Error = 3
        }

        /// <summary>
        /// 转账动作
        /// </summary>
        public enum ActionType : byte
        {
            /// <summary>
            /// 转入
            /// </summary>
            [Description("转入")]
            IN = 1,
            /// <summary>
            /// 转出
            /// </summary>
            [Description("转出")]
            OUT = 2
        }

        /// <summary>
        /// 调用查账接口获取远程网关的转账状态
        /// </summary>
        public IGame.TransferStatus Check()
        {            
            GameSetting setting = GameAgent.Instance().GetGameSettingInfo(this.Type,this.SiteID);
            if (setting == null || !setting.IsOpen) return IGame.TransferStatus.None;

            return setting.Setting.CheckTransfer(this.UserID, this.ID.ToString());
        }
    }
}
