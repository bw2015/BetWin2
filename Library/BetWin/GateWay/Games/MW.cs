using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BW.GateWay.Games
{
    public class MW : SunBet
    {
        public MW() : base() { }

        public MW(string setting) : base(setting) { }

        protected override string MoneyFormat(decimal money)
        {
            return money.ToString("0");
        }
    }
}
