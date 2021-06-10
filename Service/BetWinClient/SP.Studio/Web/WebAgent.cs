using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SP.Studio.Web
{
    internal class WebAgent
    {
        internal static int GetRandom(int start, int end)
        {
            return new Random().Next(end);
        }

        internal static string GetError(Exception ex)
        {
            StringBuilder sb = new StringBuilder();
            if (ex != null)
            {
                sb.AppendLine("Type\t:\t" + ex.GetType());
                sb.AppendLine("Source\t:\t" + ex.Source);
                sb.AppendLine("StackTrace\t:\t");
                sb.AppendLine(ex.StackTrace);
                if (ex.TargetSite != null)
                {
                    sb.AppendLine("Method\t:\t" + ex.TargetSite.Name);
                    sb.AppendLine("Class\t:\t" + ex.TargetSite.DeclaringType.FullName);
                }
            }

            foreach (DictionaryEntry obj in ex.Data)
            {
                sb.AppendLine(obj.Key + "\t:\t" + obj.Value);
            }
            return sb.ToString();
        }
    }
}
