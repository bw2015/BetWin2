using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BW.IM.Common
{
    /// <summary>
    /// 客服
    /// </summary>
    public class CustomerService : SP.Studio.Core.SettingBase
    {
        public CustomerService(string setting) : base(setting) { }

        /// <summary>
        /// 是否开启
        /// </summary>
        public bool IsOpen { get; set; }

        private string _name = "客服";
        /// <summary>
        /// 客服名字
        /// </summary>
        public string Name
        {
            get
            {
                return this._name;
            }
            set
            {
                this._name = value;
            }
        }

        public string Face { get; set; }

        /// <summary>
        /// 签名
        /// </summary>
        public string Sign { get; set; }

        /// <summary>
        /// 头像
        /// </summary>
        public string FaceShow
        {
            get
            {
                return Utils.GetFace(this.Face);
            }
        }
        
    }
}
