using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Timers;
using System.Configuration;

namespace BetWinClient
{
    public partial class Service1 : ServiceBase
    {
        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            this.Start();
        }


        void time_Elapsed(object sender, ElapsedEventArgs e)
        {
            BetWinClient.Gateway.LotteryFactory.Run();
        }

        protected override void OnStop()
        {

        }
    }
}
