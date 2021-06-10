using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using BW.GateWay.Planning;

namespace BW.Common.Sites
{
    partial class Planning
    {
        public Planning()
        {

        }

        public Planning(IPlan plan)
        {
            this.Type = plan.Type;
        }

        private IPlan _planSetting;
        public IPlan PlanSetting
        {
            get
            {
                if (_planSetting == null) _planSetting = PlanFactory.CreatePlanSetting(this.Type, this.Setting);
                return _planSetting;
            }
        }
    }
}
