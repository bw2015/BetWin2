using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BW.GateWay.Lottery
{
    /// <summary>
    /// 玩法工具类
    /// </summary>
    public static class PlayerUtils
    {
        /// <summary>
        /// 复式是否中奖
        /// </summary>
        /// <param name="input">投注号码</param>
        /// <param name="number">开奖号码</param>
        /// <returns></returns>
        public static bool IsDuplex(string[][] input, string[] number)
        {
            if (input.Length != number.Length) return false;
            for (int index = 0; index < number.Length; index++)
            {
                if (!input[index].Contains(number[index]))
                {
                    return false;
                }
            }
            return true;
        }


        /// <summary>
        /// 把投注号码转化成为复式组合列队
        /// </summary>
        /// <param name="input"></param>
        /// <param name="flags">位数 选中的位数为true</param>
        /// <returns></returns>
        public static IEnumerable<string> ToDuplexList(this string input, params bool[] flags)
        {
            string[][] inputNumber = input.GetInputNumber();
            if (flags.Count(t => t) == inputNumber.Length)
            {
                string[][] newInputNumber = new string[flags.Length][];
                int index = 0;
                for (int i = 0; i < flags.Length; i++)
                {
                    if (flags[i])
                    {
                        newInputNumber[i] = inputNumber[index];
                        index++;
                    }
                    else
                    {
                        newInputNumber[i] = new string[] { "*" };
                    }
                }
                inputNumber = newInputNumber;
            }
            return inputNumber.Aggregate((t1, t2) => t1.SelectMany(p1 => t2.Select(p2 => p1 + "," + p2)).ToArray());
        }

        /// <summary>
        /// 获取单式号码的组合列队
        /// </summary>
        /// <param name="input"></param>
        /// <param name="flags">位数 选中的位数为true</param>
        /// <returns></returns>
        public static IEnumerable<string> ToSingleList(this string input, params bool[] flags)
        {
            string[][] inputNumber = input.GetInputNumber();

            foreach (string[] t in inputNumber)
            {
                if (t.Length != flags.Count(p => p))
                {
                    yield return string.Join(",", t);
                    continue;
                }

                string[] number = new string[flags.Length];
                var index = 0;
                for (int i = 0; i < flags.Length; i++)
                {
                    if (!flags[i])
                    {
                        number[i] = "*";
                    }
                    else
                    {
                        number[i] = t[index];
                        index++;
                    }
                }
                yield return string.Join(",", number);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static IEnumerable<string> ToGroupList(this string[] input, int length)
        {
            int[] index = new int[length];
            for (index[0] = 0; index[0] < input.Length; index[0]++)
            {
                if (length == 1) { yield return string.Join(",", index.Select(t => input[t])); continue; }
                for (index[1] = index[0] + 1; index[1] < input.Length; index[1]++)
                {
                    if (length == 2) { yield return string.Join(",", index.Select(t => input[t])); continue; }
                    for (index[2] = index[1] + 1; index[2] < input.Length; index[2]++)
                    {
                        if (length == 3) { yield return string.Join(",", index.Select(t => input[t])); continue; }
                        for (index[3] = index[2] + 1; index[3] < input.Length; index[3]++)
                        {
                            if (length == 4) { yield return string.Join(",", index.Select(t => input[t])); continue; }
                            for (index[4] = index[3] + 1; index[4] < input.Length; index[4]++)
                            {
                                if (length == 5) { yield return string.Join(",", index.Select(t => input[t])); continue; }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 投注号码转化成为数组形式
        /// </summary>
        /// <param name="input">|隔开，单注数字逗号隔开</param>
        /// <returns></returns>
        public static string[][] GetInputNumber(this string input)
        {
            return input.Split('|').Select(t =>
            {
                if (string.IsNullOrEmpty(t)) return new string[] { };
                return t.Split(',');
            }).ToArray();
        }

        /// <summary>
        /// 把投注号码转成数组形式
        /// </summary>
        /// <param name="input">|隔开，单注数字逗号隔开</param>
        /// <param name="ball">数字的可选范围</param>
        /// <param name="minLength">最小长度范围，数组的长度表示要号码组合的长度</param>
        /// <param name="maxLength">最大长度范围，为null表示不检查</param>
        /// <param name="isRepeater">是否可重复</param>
        /// <returns>返回null表示输入格式错误</returns>
        public static string[][] GetInputNumber(this string input, string[] ball, int[] minLength, int[] maxLength = null, bool isRepeater = false)
        {
            string[][] inputNumber = input.GetInputNumber();
            if (inputNumber.Length != minLength.Length) return null;
            if (maxLength != null && maxLength.Length != minLength.Length) return null;
            for (int i = 0; i < inputNumber.Length; i++)
            {
                string[] number = inputNumber[i];
                int inputLength = number.Length;
                // 检查是否有重复
                if (!isRepeater && inputLength != 0 && number.Distinct().Count() != inputLength) return null;
                // 检查号码长度是否达到最小长度要求
                if (inputLength < minLength[i]) return null;
                // 检查号码长度是否超过了最大长度
                if (maxLength != null && inputLength > maxLength[i]) return null;
                // 检查是否超出允许的号码范围
                if (inputLength != 0 && number.Except(ball).Count() != 0) return null;
            }

            return inputNumber;
        }

        /// <summary>
        /// 获取单式的投注号码
        /// </summary>
        /// <param name="input">投注号码的文本形式</param>
        /// <param name="ball">号码范围</param>
        /// <param name="length">单式长度</param>
        /// <param name="isRepeater">是否允许重复号码</param>
        /// <param name="ignoreOrder">判断重复的时候是否忽略前后关系</param>
        /// <returns></returns>
        public static string[][] GetSingleInputNumber(this string input, string[] ball, int length, bool isRepeater = true, bool ignoreOrder = false)
        {
            string[][] inputNumber = input.GetInputNumber();
            // 存在重复
            if (inputNumber.Distinct().Count() != inputNumber.Length) return null;
            // 忽略前后关系
            if (ignoreOrder)
            {
                inputNumber = inputNumber.Select(t => t.OrderBy(p => p).ToArray()).ToArray();
            }
            foreach (string[] number in inputNumber)
            {
                if (number.Length != length) return null;
                if (number.Where(t => !ball.Contains(t)).Count() != 0) return null;
                if (!isRepeater && number.Distinct().Count() != number.Length) return null;
            }
            if (inputNumber.Select(t => string.Join(string.Empty, t)).Distinct().Count() != inputNumber.Length) return null;
            return inputNumber;
        }

        /// <summary>
        /// 获取开奖号码的重号范围 Key = 重复次数  Value = 符合该重复次数的号码
        /// </summary>
        /// <param name="input">开奖号码，用逗号隔开的号码</param>
        /// <returns></returns>
        public static Dictionary<int, string[]> GetRepeaterNumber(this string input)
        {
            return input.Split(',').GetRepeaterNumber();
        }

        /// <summary>
        /// 获取号码的重复数量 
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static Dictionary<int, string[]> GetRepeaterNumber(this string[] input)
        {
            var list = input.GroupBy(t => t).Select(t => new { num = t.Key, Count = t.Count() });
            Dictionary<int, string[]> dic = new Dictionary<int, string[]>();
            foreach (int key in list.Select(t => t.Count).Distinct())
            {
                dic.Add(key, list.Where(t => t.Count == key).Select(t => t.num).ToArray());
            }
            return dic;
        }

        /// <summary>
        /// 获取重号的数量
        /// </summary>
        /// <param name="dic"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static int GetNumberLength(this Dictionary<int, string[]> dic, int key)
        {
            if (!dic.ContainsKey(key)) return 0;
            return dic[key].Length;
        }


    }

    /// <summary>
    /// 数学扩展方法
    /// </summary>
    public static class MathExtend
    {
        /// <summary>
        /// 计算阶乘
        /// </summary>
        public static int Factorial(int num, int start = 2)
        {
            var value = 1;
            for (var i = start; i <= num; i++) value *= i;
            return value;
        }

        /// <summary>
        /// 从可选的数量中选出指定数量的组合
        /// </summary>
        /// <param name="length">要求的长度</param>
        /// <param name="count">总可选数量</param>
        /// <returns></returns>
        public static int Combinations(int length, int count)
        {
            if (length > count) return 0;
            return Factorial(count, count - length + 1) / Factorial(length);
        }

        /// <summary>
        /// 获取2个列队的交集数量
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list1"></param>
        /// <param name="list2"></param>
        /// <returns></returns>
        public static int Intersect<T>(IEnumerable<T> list1, IEnumerable<T> list2)
        {
            return list1.Intersect(list2).Count();
        }

    }
}
