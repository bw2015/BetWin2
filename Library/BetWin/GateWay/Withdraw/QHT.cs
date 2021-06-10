using BW.Common.Sites;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SP.Studio.Array;
using SP.Studio.Core;
using SP.Studio.Json;
using SP.Studio.Net;
using SP.Studio.Security;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BW.GateWay.Withdraw
{
    public class QHT : IWithdraw
    {
        public QHT()
        {
        }

        public QHT(string setting) : base(setting)
        {
        }

        protected override Dictionary<BankType, string> InterfaceCode => new Dictionary<BankType, string>()
        {
                {BankType.ABC, BankType.ABC.GetDescription()},
                {BankType.ICBC, BankType.ICBC.GetDescription()},
                {BankType.CCB, BankType.CCB.GetDescription()},
                {BankType.COMM, BankType.COMM.GetDescription()},
                {BankType.BOC, BankType.BOC.GetDescription()},
                {BankType.CMB, BankType.CMB.GetDescription()},
                {BankType.CMBC, BankType.CMBC.GetDescription()},
                {BankType.CEB, BankType.CEB.GetDescription()},
                {BankType.CIB, BankType.CIB.GetDescription()},
                {BankType.PSBC, BankType.PSBC.GetDescription()},
                {BankType.SPABANK, BankType.SPABANK.GetDescription()},
                {BankType.CITIC, BankType.CITIC.GetDescription()},
                {BankType.GDB, BankType.GDB.GetDescription()},
                {BankType.HXBANK, BankType.HXBANK.GetDescription()},
                {BankType.SPDB, BankType.SPDB.GetDescription()},
                {BankType.HKBEA, BankType.HKBEA.GetDescription()},
                {BankType.BJBANK, BankType.BJBANK.GetDescription()},
                {BankType.SHBANK, BankType.SHBANK.GetDescription()},
                {BankType.NBBANK, BankType.NBBANK.GetDescription()},
                {BankType.BHB, BankType.BHB.GetDescription()},
                {BankType.HSBANK, BankType.HSBANK.GetDescription()}
        };

        [Description("网关")]
        public string Gateway { get; set; } = "http://47.110.94.193/paid_index.html";

        [Description("查询网关")]
        public string QueryGate { get; set; } = "http://47.110.94.193/Paid_DingShi.html";

        [Description("商户ID")]
        public string userid { get; set; }

        [Description("通道代码")]
        public string channel { get; set; }

        [Description("密钥")]
        public string Key { get; set; }

        public override WithdrawStatus Query(string orderId, out string msg)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>()
           {
               {"mch_id",this.userid },
               {"ordierid",orderId }
           };
            string signStr = dic.OrderBy(t => t.Key).ToQueryString() + "&key=" + this.Key;
            dic.Add("sign", MD5.toMD5(signStr));

            string data = dic.ToJson();

            string result = NetAgent.UploadData(this.QueryGate, data, Encoding.UTF8);

            WithdrawStatus status = WithdrawStatus.Error;
            try
            {
                if (result.IndexOf("{") != 0 && result.Contains("{")) result = result.Substring(result.IndexOf("{"));
                //﻿{"status":0,"rollback":"订单号不存在"}
                JObject info = (JObject)JsonConvert.DeserializeObject(result);
                msg = info["rollback"].Value<string>();
                switch (info["status"].Value<int>())
                {
                    case 1:
                        status = WithdrawStatus.Paymenting;
                        break;
                    case 2:
                        status = WithdrawStatus.Success;
                        break;
                    case 4:
                        status = WithdrawStatus.Return;
                        break;
                }
            }
            catch (Exception ex)
            {
                msg = ex.Message + "<br />" + result;
            }
            return status;
        }

        public override bool Remit(out string msg)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>()
            {
                {"userid",this.userid },
                {"paid_ordierid",this.OrderID },
                {"bank_city","深圳市" },
                {"channel",this.channel },
                {"money",this.Money.ToString("0.00") },
                {"bankname",this.GetBankCode(this.BankCode) },
                {"cardName",this.Account },
                {"cardNo",this.CardNo },
                {"telphone","13800138000" },
                {"sfznumber","533527198909210238" }
            };

            string signStr = dic.OrderBy(t => t.Key).ToQueryString() + "&key=" + this.Key;
            dic.Add("sign", MD5.toMD5(signStr));
            dic.Add("pubOrPri", "0");
            string result = NetAgent.UploadData(this.Gateway, dic.ToJson(), Encoding.UTF8);
            msg = result;
            return result == "交易处理中";
        }

        public override void Remit(Action<bool, string> callback)
        {
            throw new NotImplementedException();
        }
    }
}
