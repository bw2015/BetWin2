using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

using BW.Common.Users;
using BW.Common.Sites;
using BW.GateWay.Planning;

using SP.Studio.Core;
using SP.Studio.Data;
using SP.Studio.Array;

namespace BW.Agent
{
    /// <summary>
    /// 契约管理
    /// </summary>
    partial class UserAgent
    {
        /// <summary>
        /// 获取用户与上级签订的契约
        /// 如果是总代则返回系统配置
        /// </summary>
        /// <param name="type">契约类型</param>
        /// <param name="userId">签约的乙方（自己）</param>
        /// <param name="pagentId">签约的甲方（上级）</param>
        /// <returns></returns>
        public Contract GetContractInfo(Contract.ContractType type, int userId, int parentId)
        {
            Planning plan = SiteAgent.Instance().GetPlanInfo((PlanType)type);
            int depth = (int)plan.PlanSetting.Value.Get("Depth", 0);
            if (depth != 0 && UserInfo.UserLevel < depth) return null;
            // 如果自己是总代
            if (parentId == 0 || UserInfo.UserLevel == depth)
            {
                return new Contract(type)
                {
                    SiteID = SiteInfo.ID
                };
            }
            else
            {
                return BDC.Contract.Where(t => t.SiteID == SiteInfo.ID && t.Type == type && t.User1 == parentId && t.User2 == userId && t.Status == Contract.ContractStatus.Normal).FirstOrDefault();
            }
        }

        /// <summary>
        /// 获取契约转账信息
        /// </summary>
        /// <param name="logId"></param>
        /// <returns></returns>
        public ContractLog GetContractLogInfo(int logId)
        {
            return BDC.ContractLog.Where(t => t.SiteID == SiteInfo.ID && t.ID == logId).FirstOrDefault();
        }

        /// <summary>
        /// 契约详情
        /// </summary>
        /// <param name="contractId"></param>
        /// <returns></returns>
        public Contract GetContractInfo(int contractId)
        {
            return BDC.Contract.Where(t => t.SiteID == SiteInfo.ID && t.ID == contractId).FirstOrDefault();
        }

        /// <summary>
        /// 新建一个契约
        /// </summary>
        /// <param name="userId">当前用户ID</param>
        /// <param name="childId">下级ID</param>
        /// <param name="type">契约类型</param>
        /// <param name="payPassword">资金密码</param>
        /// <param name="data">保存的数据</param>
        /// <returns></returns>
        public bool AddContract(int userId, int childId, Contract.ContractType type, string payPassword, Dictionary<string, decimal> data)
        {
            User child = this.GetUserInfo(childId);
            if (child == null || child.AgentID != userId)
            {
                base.Message("该用户不是您的直属下级");
                return false;
            }
            if (data.Count == 0 || data.Where(t => t.Value < decimal.Zero).Count() != 0)
            {
                base.Message("参数值填写错误");
                return false;
            }

            if (!this.CheckPayPassword(userId, payPassword)) return false;

            User user = this.GetUserInfo(userId);
            Contract contract = this.GetContractInfo(type, userId, user.AgentID);
            if (contract == null || contract.Setting.Count == 0)
            {
                base.Message("您尚未签订契约");
            }

            Planning plan = SiteAgent.Instance().GetPlanInfo((BW.GateWay.Planning.PlanType)type);
            if (plan == null)
            {
                base.Message("未开设此契约类型");
                return false;
            }

            bool isLimit = plan.Type == PlanType.WagesAgent && plan.PlanSetting.Value.Get("Limit", decimal.Zero) == decimal.One;


            foreach (Contract.ContractSetting item in contract.Setting)
            {
                if (item.ReadOnly) continue;

                if (!data.ContainsKey(item.Key))
                {
                    base.Message("{0}未填写", item.Name);
                    return false;
                }

                if (isLimit)
                {
                    if (data[item.Key] * 2000M + child.Rebate > item.MaxValue * 2000M + user.Rebate)
                    {
                        base.Message("{0}超过了允许的最大值{1}", item.Name, ((item.MaxValue * 2000M + user.Rebate - child.Rebate) / 100M).ToString("p"));
                        return false;
                    }
                }
                else if (data[item.Key] > item.MaxValue)
                {
                    base.Message("{0}超过了允许的最大值{1}", item.Name, item.MaxValue);
                    return false;
                }
            }

            return new Contract()
            {
                SiteID = SiteInfo.ID,
                Type = type,
                Data = data,
                User1 = userId,
                User2 = childId,
                CreateAt = DateTime.Now,
                Status = Contract.ContractStatus.None
            }.Add();
        }

        /// <summary>
        /// 更新契约
        /// </summary>
        /// <param name="contractId"></param>
        /// <param name="userId"></param>
        /// <param name="payPassword"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public bool UpdateContractInfo(int contractId, int userId, string payPassword, string action)
        {
            Contract contract = this.GetContractInfo(contractId);
            if (contract == null || !new int[] { contract.User1, contract.User2 }.Contains(userId))
            {
                base.Message("契约编号错误");
                return false;
            }
            if (!this.CheckPayPassword(userId, payPassword)) return false;
            if (!contract.GetAction(userId).Contains(action))
            {
                base.Message("不允许的操作");
                return false;
            }

            Contract.ContractStatus status = Contract.ContractStatus.None;
            switch (contract.Status)
            {
                case Contract.ContractStatus.None:
                    switch (action)
                    {
                        case "Agree":
                            status = Contract.ContractStatus.Normal;
                            break;
                        case "Inject":
                            status = Contract.ContractStatus.Cancel;
                            break;
                    }
                    break;
                case Contract.ContractStatus.Normal:
                    switch (action)
                    {
                        case "Delete":
                            status = Contract.ContractStatus.AcceptCancel;
                            break;
                    }
                    break;
                case Contract.ContractStatus.AcceptCancel:
                    switch (action)
                    {
                        case "Agree":
                            status = Contract.ContractStatus.Cancel;
                            break;
                        case "Inject":
                            status = Contract.ContractStatus.Normal;
                            break;
                    }
                    break;
            }

            if (status == contract.Status)
            {
                base.Message("状态未发生改变");
                return false;
            }

            bool success;
            if (status == Contract.ContractStatus.Cancel)
            {

                success = contract.Delete() == 1;
                if (success)
                {
                    this.DeleteContract(contract.Type, contract.User2);
                }
            }
            else
            {
                contract.Status = status;
                success = contract.Update(null, t => t.Status) == 1;
            }

            this.SaveLog(userId, "契约{0}({1})进行了{2}操作", contract.ID, contract.Status.GetDescription(), action);
            return success;
        }

        /// <summary>
        /// 删除所有的下级契约
        /// </summary>
        /// <param name="userId">契约乙方</param>
        private void DeleteContract(Contract.ContractType type, int userId)
        {
            foreach (int id in BDC.Contract.Where(t => t.SiteID == SiteInfo.ID && t.Type == type).
                Join(BDC.UserDepth.Where(t => t.SiteID == SiteInfo.ID && t.UserID == userId), t => t.User2, t => t.ChildID, (a, b) => a.ID))
            {
                new Contract() { ID = id }.Delete();
            }
        }

        /// <summary>
        /// 管理员操作删除契约
        /// </summary>
        /// <param name="contractId"></param>
        public bool DeleteContract(int contractId)
        {
            if (AdminInfo == null)
            {
                base.Message("没有权限");
                return false;
            }

            Contract contract = this.GetContractInfo(contractId);
            if (contract == null)
            {
                base.Message("契约编号错误");
                return false;
            }
            if (contract.Delete() == 0)
            {
                return false;
            }
            this.DeleteContract(contract.Type, contract.User2);

            AdminInfo.Log(Common.Admins.AdminLog.LogType.User, "删除契约，类型：{0} 甲方：{1} 乙方：{2}", contract.Type.GetDescription(), contract.User1, contract.User2);

            return true;
        }

        /// <summary>
        /// 添加一个契约转账日志
        /// </summary>
        /// <param name="contract"></param>
        /// <returns>日志ID，作为契约转账的源ID插入</returns>
        public bool AddContractLog(Contract contract, decimal money, int sourceId, string description, decimal amount)
        {
            using (DbExecutor db = NewExecutor())
            {
                return this.AddContractLog(db, contract, money, sourceId, description, amount);
            }
        }

        /// <summary>
        /// 添加契约转账日志
        /// </summary>
        /// <param name="db"></param>
        /// <param name="contract"></param>
        /// <param name="money"></param>
        /// <param name="sourceId"></param>
        /// <param name="description"></param>
        /// <param name="amount">需要计算的业绩</param>
        /// <returns></returns>
        public bool AddContractLog(DbExecutor db, Contract contract, decimal money, int sourceId, string description, decimal amount)
        {

            ContractLog log = new ContractLog()
            {
                SiteID = contract.SiteID,
                UserID = contract.User1,
                User2 = contract.User2,
                ContractID = contract.ID,
                CreateAt = DateTime.Now,
                Money = money,
                Type = contract.Type,
                SourceID = sourceId,
                Status = ContractLog.TransferStatus.None,
                Description = description,
                Amount = amount
            };

            if (log.Exists(db, t => t.SiteID, t => t.Type, t => t.UserID, t => t.User2, t => t.SourceID))
            {
                return false;
            }
            if (log.Add(db))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 执行契约转账金额（适用于非web程序）
        /// </summary>
        /// <param name="logId"></param>
        /// <returns></returns>
        public bool ExecContractLog(int logId)
        {
            using (DbExecutor db = NewExecutor(IsolationLevel.ReadCommitted))
            {
                ContractLog log = new ContractLog() { ID = logId }.Info(db);
                if (log == null || log.Status == ContractLog.TransferStatus.Success)
                {
                    db.Rollback();
                    return false;
                }

                MoneyLog.MoneyType logOut = MoneyLog.MoneyType.ContractOut;
                MoneyLog.MoneyType logIn = MoneyLog.MoneyType.ContractIn;

                switch (log.Type)
                {
                    case Contract.ContractType.Bouns:
                        logOut = MoneyLog.MoneyType.BonusTransferOut;
                        logIn = MoneyLog.MoneyType.BonusTransferIn;
                        break;
                }

                if (!UserAgent.Instance().AddMoneyLog(db, log.UserID, log.Money * -1, logOut, log.ID,
                    string.Format("【{0}】下级{1},{2}", log.Type.GetDescription(), UserAgent.Instance().GetUserName(log.User2), log.Description)))
                {
                    db.Rollback();
                    return false;
                }

                if (!UserAgent.Instance().AddMoneyLog(db, log.User2, log.Money, logIn, log.ID,
                       string.Format("【{0}】上级转入,{1}", log.Type.GetDescription(), log.Description)))
                {
                    db.Rollback();
                    return false;
                }

                log.Status = ContractLog.TransferStatus.Success;
                log.Update(db, t => t.Status);

                db.Commit();
                return true;
            }
        }

        /// <summary>
        /// 批量执行契约转账
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="type"></param>
        /// <param name="sourceId"></param>
        /// <returns>返回发放成功的数量</returns>
        public int ExecContractLog(int siteId, Contract.ContractType type, int sourceId)
        {
            List<int> list = BDC.ContractLog.Where(t => t.SiteID == siteId && t.Type == type && t.SourceID == sourceId && t.Status == ContractLog.TransferStatus.None).Select(t => t.ID).ToList();
            int success = 0;
            StringBuilder sb = new StringBuilder();

            foreach (int logId in list)
            {
                this.MessageClean();
                if (this.ExecContractLog(logId))
                {
                    success++;
                    sb.AppendFormat("契约{0}执行成功", logId)
                        .AppendLine();
                }
                else
                {
                    sb.AppendFormat("契约{0}执行失败,{1}", logId, this.Message())
                        .AppendLine();
                }
            }

            SystemAgent.Instance().AddSystemLog(siteId, sb.ToString());

            return success;
        }

        /// <summary>
        /// 全局检查契约的转账锁定状态（适用于非web程序）
        /// </summary>
        /// <returns></returns>
        public void CheckContackLockStatus()
        {
            int[] logs = BDC.ContractLog.Where(t => t.Status == ContractLog.TransferStatus.None).Select(t => t.ID).ToArray();
            foreach (int logId in logs)
            {
                this.ExecContractLog(logId);
            }

            // 存在没有处理契约的用户列表
            logs = BDC.ContractLog.Where(t => t.Status == ContractLog.TransferStatus.None).Select(t => t.UserID).Distinct().ToArray();

            int[] users = BDC.User.Where(t => (t.Lock & User.LockStatus.Contract) == User.LockStatus.Contract).Select(t => t.ID).ToArray();

            foreach (int userId in users.Where(t => !logs.Contains(t)))
            {
                this.UpdateUserLockStatus(userId, User.LockStatus.Contract, false);
            }

            foreach (int userId in logs.Where(t => !users.Contains(t)))
            {
                this.UpdateUserLockStatus(userId, User.LockStatus.Contract, true);
            }
        }

        /// <summary>
        /// 检查用户是否有未处理的契约
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public void CheckContackLockStatus(int userId)
        {
            int[] logs = BDC.ContractLog.Where(t => t.UserID == userId && t.Status == ContractLog.TransferStatus.None).Select(t => t.ID).ToArray();
            int success = 0;
            foreach (int logId in logs)
            {
                if (this.ExecContractLog(logId)) success++;
            }
            this.UpdateUserLockStatus(userId, User.LockStatus.Contract, logs.Length != success);
        }
    }
}
