using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BW.GateWay.SMS
{ /// <summary>
    /// 短信工厂
    /// </summary>
    public class SMSFactory
    {
        public static ISMS CreateSMS(SMSConfig config)
        {
            ISMS sms = null;
            sms = (ISMS)Activator.CreateInstance(Type.GetType("BW.GateWay.SMS." + config.Provider),
                config.UserName,
                config.Password);
            return sms;
        }

        /// <summary>
        /// 短信息配置类
        /// </summary>
        public class SMSConfig
        {
            public SMSProvider Provider { get; set; }

            public string UserName { get; set; }

            public string Password { get; set; }

        }
    }
}
