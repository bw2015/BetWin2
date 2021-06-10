using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using BW.Agent;
using BW.Framework;

namespace BW.Common.Admins
{
    /// <summary>
    /// 管理员状态
    /// </summary>
    partial class Admin
    {
        public enum AdminStatus : byte
        {
            [Description("正常")]
            Normal = 0,
            [Description("停止")]
            Stop = 1
        }

        public void Log(AdminLog.LogType type, string content, params object[] args)
        {
            BW.Agent.AdminAgent.Instance().AddLog(this.ID, type, content, args);
        }

        /// <summary>
        /// 扩展类型的日志
        /// </summary>
        /// <param name="type"></param>
        /// <param name="content"></param>
        /// <param name="args"></param>
        public void Log(object type, string content, params object[] args)
        {
            this.Log((AdminLog.LogType)type, content, args);
        }

        /// <summary>
        /// 对外显示的名字
        /// </summary>
        public string Name
        {
            get
            {
                return string.IsNullOrEmpty(this.NickName) ? this.AdminName : this.NickName;
            }
        }

        /// <summary>
        /// 对外显示的头像地址
        /// </summary>
        public string FaceShow
        {
            get
            {
                if (string.IsNullOrEmpty(this.Face))
                {
                    return SysSetting.GetSetting().imgServer + "/images/staff.png";
                }
                return SysSetting.GetSetting().imgServer + this.Face;
            }
        }

        private AdminGroup _groupInfo;
        public AdminGroup GroupInfo
        {
            get
            {
                if (_groupInfo == null)
                {
                    _groupInfo = this.GroupID == 0 ? new AdminGroup() { Name = "超级管理员" } : AdminAgent.Instance().GetAdminGroupInfo(this.GroupID);
                }
                return _groupInfo;
            }
        }

        /// <summary>
        /// 判断用户是否拥有该权限
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool HasPermission(string id)
        {
            return this.GroupID == 0 || this.GroupInfo.HasPermission(id);
        }

        /// <summary>
        /// 判断是否拥有权限（多个权限只需要满足一个）
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool HasPermission(string[] id)
        {
            if (id.Length == 0) return true;
            if (this.GroupID == 0) return true;
            foreach (string _id in id)
            {
                if (this.HasPermission(_id)) return true;
            }
            return false;
        }
    }
}
