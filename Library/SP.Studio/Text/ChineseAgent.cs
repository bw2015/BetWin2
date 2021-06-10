using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;
using System.Web;

//using Microsoft.International.Converters.PinYinConverter;

namespace SP.Studio.Text
{
    /// <summary>
    /// 中文处理
    /// 需要 Microsoft Visual Studio International Pack 1.0 SR1 支持
    /// 下载地址：http://download.microsoft.com/download/5/7/3/57345088-ACF8-4E9B-A9A7-EBA35452DEF2/vsintlpack1.zip
    /// 把 ChnCharInfo.dll 放入项目的Bin目录下
    /// 注：2.0是1.0的扩展，需要安装1.0之后再安装2.0
    /// </summary>
    public static class ChineseAgent
    {
        /// <summary>
        /// 静态的字典，修正类库的错误
        /// </summary>
        private static Dictionary<char, string> dic = new Dictionary<char, string>();

        /// <summary>
        /// dll文件的名字
        /// </summary>
        const string dllName = "ChnCharInfo.dll";

        /// <summary>
        /// 资源
        /// </summary>
        private static Assembly assembly;

        static ChineseAgent()
        {
            string file = string.Empty;

            if (HttpContext.Current == null)
            {
                file = System.Windows.Forms.Application.StartupPath + @"\" + dllName;
            }
            else
            {
                file = HttpContext.Current.Server.MapPath("~/Bin/" + dllName);
            }

            if (File.Exists(file))
            {
                assembly = Assembly.LoadFile(file);
                dic.Add('广', "Guang");
            }
            else
            {
                assembly = null;
            }
        }

        #region ============  扩展方法  ============

        /// <summary>
        /// 判断是否是中文
        /// </summary>
        public static bool IsChinese(this char ch)
        {
            return ch >= 0x4e00 && ch <= 0x9fbb;
        }

        #endregion

        /// <summary>
        /// 获取一个汉字的拼音
        /// </summary>
        public static string PinYin(char ch)
        {
            if (dic.ContainsKey(ch)) return dic[ch];
            if (!ch.IsChinese()) return null;
            dynamic chinese = Activator.CreateInstance(GetType("ChineseChar"), ch);
            return String.Join(string.Empty, chinese.Pinyins);
        }

        public static List<string> PinYin(string str)
        {
            List<string> list = new List<string>();
            foreach (char ch in str)
            {
                var py = PinYin(ch);
                if (string.IsNullOrEmpty(py)) continue;
                list.Add(py);
            }
            return list;
        }

        /// <summary>
        /// 获取首字母简拼
        /// </summary>
        public static string FirstPinYin(string str)
        {
            return new string(PinYin(str).ConvertAll(t => t.FirstOrDefault()).ToArray());
        }

        private static Type GetType(string typeName)
        {
            return assembly.GetType("Microsoft.International.Converters.PinYinConverter." + typeName);
        }

        /// <summary>
        /// 把数字转成中文大写的模式
        /// </summary>
        public static string Upper(double number)
        {
            Money money = new Money(number);
            return money.Convert();
        }


    }

    class Money
    {
        /// <summary>
        /// 要转换的数字
        /// </summary>
        private double j;
        /// <summary>
        /// 
        /// </summary>
        private string[] NumChineseCharacter = new string[] { "零", "壹", "贰", "叁", "肆", "伍", "陆", "柒", "捌", "玖" };

        public Money(double m)
        {
            this.j = Math.Round(m, 2);
        }
        /// <summary>
        /// 判断输入的数字是否大于double类型
        /// </summary>
        private bool IsNumber
        {
            get
            {
                if (j > double.MaxValue || j <= 0)
                    return false;
                else
                    return true;
            }
        }
        /// <summary>
        /// 数字转换成大写汉字主函数
        /// </summary>
        /// <returns>返回转换后的大写汉字</returns>
        public string Convert()
        {
            string bb = "";
            if (IsNumber)
            {
                string str = j.ToString();
                string[] Num = str.Split('.');
                if (Num.Length == 1)
                {
                    bb = NumberString(Num[0]) + "元整";
                    bb = bb.Replace("零零", "零");
                }
                else
                {
                    bb = NumberString(Num[0]) + "元";
                    bb += FloatString(Num[1]);
                    bb = bb.Replace("零零", "零");
                }
            }
            else
            {
                throw new FormatException("你输入的数字格式不正确或不是数字!");
            }
            return bb;
        }
        /// <summary>
        /// 小数位转换只支持两位的小数
        /// </summary>
        /// <param name="Num">转换的小数</param>
        /// <returns>小数转换成汉字</returns>
        private string FloatString(string Num)
        {
            string cc = "";
            if (Num.Length > 2)
            {
                throw new FormatException("小数位数过多.");
            }
            else
            {
                string bb = ConvertString(Num);
                int len = bb.IndexOf("零");
                if (len != 0)
                {
                    bb = bb.Replace("零", "");
                    if (bb.Length == 1)
                    {
                        cc = bb.Substring(0, 1) + "角整";
                    }
                    else
                    {
                        cc = bb.Substring(0, 1) + "角";
                        cc += bb.Substring(1, 1) + "分";
                    }
                }
                else
                    cc = bb + "分";
            }
            return cc;
        }
        /// <summary>
        /// 判断数字位数以进行拆分转换
        /// </summary>
        /// <param name="Num">要进行拆分的数字</param>
        /// <returns>转换成的汉字</returns>
        private string NumberString(string Num)
        {
            string bb = "";
            if (Num.Length <= 4)
            {
                bb = Convert4(Num);
            }
            else if (Num.Length > 4 && Num.Length <= 8)
            {
                bb = Convert4(Num.Substring(0, Num.Length - 4)) + "万";
                bb += Convert4(Num.Substring(Num.Length - 4, 4));
            }
            else if (Num.Length > 8 && Num.Length <= 12)
            {
                bb = Convert4(Num.Substring(0, Num.Length - 8)) + "亿";
                if (Convert4(Num.Substring(Num.Length - 8, 4)) == "")
                    if (Convert4(Num.Substring(Num.Length - 4, 4)) != "")
                        bb += "零";
                    else
                        bb += "";
                else
                    bb += Convert4(Num.Substring(Num.Length - 8, 4)) + "万";
                bb += Convert4(Num.Substring(Num.Length - 4, 4));
            }
            return bb;
        }
        /// <summary>
        /// 四位数字的转换
        /// </summary>
        /// <param name="Num">准备转换的四位数字</param>
        /// <returns>转换以后的汉字</returns>
        private string Convert4(string Num)
        {
            string bb = "";
            if (Num.Length == 1)
            {
                bb = ConvertString(Num);
            }
            else if (Num.Length == 2)
            {
                bb = ConvertString(Num);
                bb = Convert2(bb);
            }
            else if (Num.Length == 3)
            {
                bb = ConvertString(Num);
                bb = Convert3(bb);
            }
            else
            {
                bb = ConvertString(Num);
                string cc = "";
                string len = bb.Substring(0, 4);
                if (len != "零零零零")
                {
                    len = bb.Substring(0, 3);
                    if (len != "零零零")
                    {
                        bb = bb.Replace("零零零", "");
                        if (bb.Length == 1)
                        {
                            bb = bb.Substring(0, 1) + "仟";
                        }
                        else
                        {
                            if (bb.Substring(0, 1) != "零" && bb.Substring(0, 2) != "零")
                                cc = bb.Substring(0, 1) + "仟";
                            else
                                cc = bb.Substring(0, 1);
                            bb = cc + Convert3(bb.Substring(1, 3));
                        }
                    }
                    else
                    {
                        bb = bb.Replace("零零零", "零");
                    }
                }
                else
                {
                    bb = bb.Replace("零零零零", "");
                }
            }
            return bb;
        }
        /// <summary>
        /// 将数字转换成汉字
        /// </summary>
        /// <param name="Num">需要转换的数字</param>
        /// <returns>转换后的汉字</returns>
        private string ConvertString(string Num)
        {
            string bb = "";
            for (int i = 0; i < Num.Length; i++)
            {
                bb += NumChineseCharacter[int.Parse(Num.Substring(i, 1))];
            }
            return bb;
        }
        /// <summary>
        /// 两位数字的转换
        /// </summary>
        /// <param name="Num">两位数字</param>
        /// <returns>转换后的汉字</returns>
        private string Convert2(string Num)
        {
            string bb = ""; string cc = "";
            string len = Num.Substring(0, 1);
            if (len != "零")
            {
                bb = Num.Replace("零", "");
                if (bb.Length == 1)
                {
                    cc = bb.Substring(0, 1) + "拾";
                }
                else
                {
                    cc = bb.Substring(0, 1) + "拾";
                    cc += bb.Substring(1, 1);
                }
            }
            else
                cc = Num;
            return cc;
        }
        /// <summary>
        /// 三位数字的转换
        /// </summary>
        /// <param name="Num">三位数字</param>
        /// <returns>转换后的汉字</returns>
        private string Convert3(string Num)
        {
            string bb = ""; string cc = "";
            string len = Num.Substring(0, 2);
            if (len != "零零")
            {
                bb = Num.Replace("零零", "");
                if (bb.Length == 1)
                {
                    bb = bb.Substring(0, 1) + "佰";
                }
                else
                {
                    if (bb.Substring(0, 1) != "零")
                        cc = bb.Substring(0, 1) + "佰";
                    else
                        cc = bb.Substring(0, 1);
                    bb = cc + Convert2(bb.Substring(1, 2));
                }
            }
            else
            {
                bb = Num.Replace("零零", "零");
            }
            return bb;
        }
    }
}
