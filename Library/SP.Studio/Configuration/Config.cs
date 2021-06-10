using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace SP.Studio.Configuration
{
    public class Config
    {
        /// <summary>
        /// 当前程序所处环境的Key 用于config文件中的AppSetting标识
        /// </summary>
        public const string HOST_KEY = "Host";

        /// <summary>
        /// 数据库链接对象的单次访问生命周期内使用的KEY
        /// </summary>
        public const string DATA_KEY = "DbDefaultKey";

        /// <summary>
        /// 用于测试的HttpContext Key
        /// </summary>
        public const string TEST_KEY = "TESTING";

        /// <summary>
        /// 用于测试的HttpContext Key
        /// </summary>
        public const string TEST_PAGE = "TESTINGPAGE";

        /// <summary>
        /// Mono平台的标识
        /// </summary>
        internal const string MONO = "Mono";
    }

    /// <summary>
    /// 语言
    /// </summary>
    public enum LanguagePack
    {
        [Description("简体中文")]
        Simplified,
        [Description("繁體中文")]
        Traditional,
        [Description("English")]
        English,
        [Description("日本語")]
        Japanese,
        [Description("한국어")]
        Korean,
        [Description("私有语言包")]
        Private
    }

    public static class PayError
    {
        public static string ExistOrder = "订单号已存在数据库";
        public static string SendType = "结果发送类型SendType为0";
        public static string VerifySignC = "验证签名失败";
        public static string NotifySignisNull = "订单签名信息NotifySign为空";
    }
}
