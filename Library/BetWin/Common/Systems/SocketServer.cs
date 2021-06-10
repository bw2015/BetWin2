/*    本实体类自动生成，请勿改动。 如数据库结构有变动请重新生成。 */
using System;
using System.Data.Linq.Mapping;

namespace BW.Common.Systems
{
    /// <summary>
    /// websocket服务器地址
    /// </summary>
    [Table(Name = "sys_SocketServer")]
    public partial class SocketServer
    {


        [Column(Name = "Domain")]
        public string Domain { get; set; }

    }
}
