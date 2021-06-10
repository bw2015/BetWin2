using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SP.Studio.GateWay.AliYun.MQ
{
    /// <summary>
    /// 消息队列操作
    /// </summary>
    public class MessageQueue
    {
        /*
AccessKey详情
AccessKeyID：LTAIUqYmxAoRVVea
AccessKeySecret：C8s6Is1jPimE2BgEE4oxgZfSdcyt1C
        */


        /// <summary>
        /// your access key id
        /// </summary>
        private readonly string _accessKeyId;

        /// <summary>
        /// your secret access key
        /// </summary>
        private readonly string _secretAccessKey;

        /// <summary>
        /// valid endpoint, 比如http://$AccountId.mns.cn-hangzhou.aliyuncs.com
        /// </summary>
        private readonly string _endpoint;

        public MessageQueue(string accessKeyId, string secretAccessKey, string endpoint)
        {
            this._accessKeyId = accessKeyId;
            this._secretAccessKey = secretAccessKey;
            this._endpoint = endpoint;
        }


        /// <summary>
        /// 创建一个列队
        /// </summary>
        public void CreateQueue()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 发送消息到消息队列中
        /// </summary>
        /// <param name="queueName">队列名称</param>
        /// <param name="message">消息名称</param>
        public bool SendMessage(string queueName, string message)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 接受消息
        /// </summary>
        /// <param name="action">接收到消息之后的处理方法</param>
        /// <param name="deleteMessage">处理完成之后是否删除</param>
        /// <param name="time">长轮询的最长等待时间</param>
        public void ReceiveMessage(Action<object> action, bool deleteMessage = true, int time = 30)
        {

        }

        /// <summary>
        /// 删除一个队列
        /// </summary>
        /// <param name="queueName"></param>
        /// <returns></returns>
        public bool DeleteQueue(string queueName)
        {
            throw new NotImplementedException();
        }
    }
}
