using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Data.Linq.Mapping;
using System.Runtime.Serialization;
using System.Reflection;
using System.Web;
using System.Data;

using SP.Studio.Core;
using SP.Studio.Data;

namespace SP.Studio.WorkFlow
{
    /// <summary>
    /// 全部的工作流对象
    /// </summary>
    [Serializable, DataContract]
    public class Workflow
    {
        public Workflow()
        {
            this.GroupList = new List<WorkGroup>();
            this.PageList = new List<WorkPage>();
            this.EventList = new List<WorkEvent>();
            this.ResultList = new List<WorkResult>();
            this.LineList = new List<WorkLine>();
        }

        [DataMember]
        public List<WorkGroup> GroupList { get; set; }

        [DataMember]
        public List<WorkPage> PageList { get; set; }

        [DataMember]
        public List<WorkEvent> EventList { get; set; }

        [DataMember]
        public List<WorkResult> ResultList { get; set; }

        [DataMember]
        public List<WorkLine> LineList { get; set; }
    }

    [Serializable, DataContract]
    public abstract class WorkCommonBase
    {
        private dynamic WorkList
        {
            get
            {
                dynamic list = null;
                switch (this.GetType().Name)
                {
                    case "WorkGroup":
                        list = WorkFlowSetting.WorkFlow.GroupList;
                        break;
                    case "WorkPage":
                        list = WorkFlowSetting.WorkFlow.PageList;
                        break;
                    case "WorkEvent":
                        list = WorkFlowSetting.WorkFlow.EventList;
                        break;
                    case "WorkResult":
                        list = WorkFlowSetting.WorkFlow.ResultList;
                        break;
                    case "WorkLine":
                        list = WorkFlowSetting.WorkFlow.LineList;
                        break;
                }
                return list;
            }
        }

        /// <summary>
        /// 当前被继承对象的ID
        /// </summary>
        private int WorkID
        {
            get
            {
                PropertyInfo property = this.GetType().GetProperty("ID");
                int id = (int)property.GetValue(this, null);
                return id;
            }
        }

        /// <summary>
        /// 将自身插入缓存 并且更新配置文件
        /// </summary>
        public void Add()
        {
            if (WorkID == 0)
            {
                int max = 1;
                if (WorkList.Count > 0)
                {
                    List<int> idList = new List<int>();
                    foreach (var obj in WorkList)
                    {
                        idList.Add((int)this.GetType().GetProperty("ID").GetValue(obj, null));
                    }
                    max = idList.Max() + 1;
                }
                this.GetType().GetProperty("ID").SetValue(this, max, null);
            }
            MethodInfo method = WorkList.GetType().GetMethod("Add");
            method.Invoke(WorkList, new object[] { this });
            WorkAgent.CreateConfigurationFile();
        }

        /// <summary>
        /// 更新自身
        /// </summary>
        public void Update<T>(params  Expression<Func<T, object>>[] fields)
        {
            object obj = null;
            foreach (var _obj in WorkList)
            {
                if (_obj.ID == WorkID) { obj = _obj; break; }
            }
            if (obj == null) return;

            foreach (var field in fields)
            {
                PropertyInfo property = null;
                switch (field.Body.NodeType)
                {
                    case ExpressionType.Convert:
                        property = (PropertyInfo)((MemberExpression)((UnaryExpression)field.Body).Operand).Member;
                        break;
                    case ExpressionType.MemberAccess:
                        property = (PropertyInfo)((MemberExpression)field.Body).Member;
                        break;
                }
                property.SetValue(obj, property.GetValue(this, null), null);
            }
            WorkAgent.CreateConfigurationFile();
        }

        /// <summary>
        /// 删除
        /// </summary>
        public void Delete()
        {
            object obj = null;
            int i = 0;
            foreach (var _obj in WorkList)
            {
                if (_obj.ID == WorkID) { obj = _obj; break; }
                i++;
            }
            if (obj == null) return;
            WorkList.RemoveAt(i);
            WorkAgent.CreateConfigurationFile();
        }
    }

    /// <summary>
    /// 工作组
    /// </summary>
    [Serializable, DataContract]
    public class WorkGroup : WorkCommonBase
    {
        public WorkGroup() { }

        [DataMember]
        public int ID { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Description { get; set; }

        /// <summary>
        /// 该组下所有的方法所在的资源名称
        /// </summary>
        [DataMember]
        public string Assembly { get; set; }

        [DataMember]
        public short Sort { get; set; }

        [DataMember]
        public WorkGroupSetting Setting { get; set; }

        [Serializable, DataContract]
        public class WorkGroupSetting 
        {
            public WorkGroupSetting() : base() { }

            /// <summary>
            /// 自定义的高度
            /// </summary>
            [DataMember]
            public int Height { get; set; }
        }


    }

    /// <summary>
    /// 页面
    /// </summary>
    [Serializable, DataContract]
    public class WorkPage : WorkCommonBase
    {
        public WorkPage() { }

        [DataMember]
        public readonly string Genre = "page";

        [DataMember]
        public int ID { get; set; }

        [DataMember]
        public int GroupID { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Description { get; set; }

        [DataMember]
        public string Url { get; set; }

        [DataMember]
        public Position Position { get; set; }

    }

    /// <summary>
    /// 事件
    /// </summary>
    [Serializable, DataContract]
    public class WorkEvent : WorkCommonBase
    {
        public WorkEvent() { }


        [DataMember]
        public readonly string Genre = "event";

        [DataMember]
        public int ID { get; set; }

        [DataMember]
        public int GroupID { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Description { get; set; }

        [DataMember]
        public string Type { get; set; }

        [DataMember]
        public string Method { get; set; }

        [DataMember]
        public object[] Params { get; set; }

        [DataMember]
        public Position Position { get; set; }
    }

    /// <summary>
    /// 对返回结果的处理
    /// </summary>
    [Serializable, DataContract]
    public class WorkResult : WorkCommonBase
    {
        public WorkResult() { }

        [DataMember]
        public readonly string Genre = "result";

        [DataMember]
        public int ID { get; set; }

        [DataMember]
        public int GroupID { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Description { get; set; }

        [DataMember]
        public string Type { get; set; }

        [DataMember]
        public string Method { get; set; }

        [DataMember]
        public string[] Params { get; set; }

        [DataMember]
        public Position Position { get; set; }

        [DataMember]
        public int Next { get; set; }
    }

    /// <summary>
    /// 连接线
    /// </summary>
    [Serializable, DataContract]
    public class WorkLine : WorkCommonBase
    {
        public WorkLine() { }

        [DataMember]
        public readonly string Genre = "line";

        [DataMember]
        public int ID { get; set; }

        [DataMember]
        public int GroupID { get; set; }

        [DataMember]
        public int PageID { get; set; }

        [DataMember]
        public int EventID { get; set; }

        [DataMember]
        public int ResultID { get; set; }

        [DataMember]
        public Position Position { get; set; }
    }

    /// <summary>
    /// 位置配置信息
    /// </summary>
    [DataContract, Serializable]
    public class Position : JsonBase
    {
        public Position() : base() { }

        public Position(string str) : base(str) { }

        [DataMember]
        public int x { get; set; }

        [DataMember]
        public int y { get; set; }

        [DataMember]
        public int x1 { get; set; }

        [DataMember]
        public int y1 { get; set; }

        [DataMember]
        public int x2 { get; set; }

        [DataMember]
        public int y2 { get; set; }
    }

    /// <summary>
    /// 返回值
    /// </summary>
    public enum ResultType
    {
        YES,
        NO,
        Success,
        Faild,
        Alert,
        None
    }



}
