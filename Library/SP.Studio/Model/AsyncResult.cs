using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using SP.Studio.ErrorLog;

namespace SP.Studio.Model
{
    public class AsyncResult : IAsyncResult
    {
        public AsyncResult(HttpContext context, AsyncCallback cb, object extraData)
        {
            this.context = context;
            this.callback = cb;
            this.extraData = extraData;
        }

        private AsyncCallback callback { get; set; }

        private object extraData { get; set; }

        private HttpContext context { get; set; }


        public bool IsCompleted { get; set; }

        public WaitHandle AsyncWaitHandle { get; set; }

        public object AsyncState { get; set; }

        public bool CompletedSynchronously
        {
            get { return false; }
        }

        /// <summary>
        /// 异步执行完毕，返回处理结果
        /// </summary>
        /// <param name="result"></param>
        public void Send(Result result)
        {
            try
            {
                context.Response.Write(result, false);
            }
            //catch (Exception ex)
            //{
            //    throw ex;
            //   // context.Response.Write(false, ErrorAgent.CreateDetail(ex));
            //}
            finally
            {
                callback(this);
                this.IsCompleted = true;
            }
        }
    }
}
