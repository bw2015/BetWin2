using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace UpdateBetWin
{
    class Program
    {
        static void Main(string[] args)
        {
            int siteId = int.Parse(args[0]);

            int cursorLeft;
            int count = 0;

            OldAgent oldAgent = new OldAgent();
            NewAgent newAgent = new NewAgent();

            DataSet userlist = oldAgent.GetUserList();
            Dictionary<string, int> oldUserID = new Dictionary<string, int>();
            //#1 导入所有用户
            Console.Write("用户导入：");
            cursorLeft = Console.CursorLeft;
            foreach (DataRow dr in userlist.Tables[0].Rows)
            {
                newAgent.ImportUser(siteId, dr);
                if (!oldUserID.ContainsKey((string)dr["UserName"]))
                    oldUserID.Add((string)dr["UserName"], (int)dr["UserID"]);
                count++;
                Console.CursorLeft = cursorLeft;
                Console.Write(count);
            }

            //#2 创造新旧用户的ID对比 旧ID，新ID
            Dictionary<string, int> newUserID = newAgent.GetUserID(siteId);
            Dictionary<int, int> userid = newUserID.Join(oldUserID, t => t.Key, t => t.Key, (t2, t1) => new
            {
                NewID = t2.Value,
                OldID = t1.Value
            }).ToDictionary(t => t.OldID, t => t.NewID);

            //#3 导入帐变流水
            Console.WriteLine();
            count = 0;
            Console.Write("资金流水导入：");
            cursorLeft = Console.CursorLeft;

            DataSet moneyLog = oldAgent.GetMoneyLog();
            foreach (DataRow dr in moneyLog.Tables[0].Rows)
            {
                if (!userid.ContainsKey((int)dr["UserID"])) continue;
                newAgent.ImportUser(siteId, userid[(int)dr["UserID"]], dr);
                count++;
                Console.CursorLeft = cursorLeft;
                Console.Write(count);
            }

            //#4 代理关系
            newAgent.UpdateAgent(siteId, userid);

            Console.WriteLine();
        }
    }
}
