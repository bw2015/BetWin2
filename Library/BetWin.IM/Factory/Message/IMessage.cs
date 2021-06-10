﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SP.Studio.Core;

namespace BW.IM.Factory.Message
{
    public abstract class IMessage
    {
        public string Action
        {
            get
            {
                return this.GetType().Name;
            }
        }

        /// <summary>
        /// 构造要发出的json信息
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this.ToJson();
        }
    }
}