using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BW.GateWay.Lottery.SmartGame
{
    public abstract class IGamePlayer : IPlayer
    {
        /// <summary>
        /// 产生一个随机开奖结果
        /// </summary>
        /// <returns></returns>
        public virtual string CreateRandomNumber()
        {
            return this.InputBall.OrderBy(t => Guid.NewGuid()).FirstOrDefault();
        }
    }
}
