using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BW.Agent;

namespace BW.GateWay.Games
{
    public class Casino : IGame
    {
        public Casino() : base() { }

        public Casino(string setting) : base(setting) { }

        public override bool CreateUser(int userId, params object[] args)
        {
            return true;
        }

        public override decimal GetBalance(int userId)
        {
            return UserAgent.Instance().GetUserMoney(userId);
        }

        /// <summary>
        /// 换筹码账户
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="money"></param>
        /// <param name="id"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        public override bool Deposit(int userId, decimal money, string id, out decimal amount)
        {
            amount = decimal.Zero;
            return false;
        }

        public override bool Withdraw(int userId, decimal money, string orderId, out decimal amount)
        {
            amount = decimal.Zero;
            return false;
        }

        public override IGame.TransferStatus CheckTransfer(int userId, string id)
        {
            return TransferStatus.None;
        }

        public override void FastLogin(int userId, string key)
        {
            throw new NotImplementedException();
        }
    }
}
