using System;
using System.Text;
using System.IO;
using System.Web;

namespace SP.Studio.Text
{
    /// <summary>
    /// 簡體中文轉換成為繁體中文（只支持Windows）
    /// </summary>
    public class ResponseBig5 : Stream
    {
        private Stream m_sink;
        private long m_position;

        public ResponseBig5(Stream sink)
        {
            m_sink = sink;
        }

        // The following members of Stream must be overriden.
        public override bool CanRead
        {
            get { return true; }
        }

        public override bool CanSeek
        {
            get { return false; }
        }

        public override bool CanWrite
        {
            get { return false; }
        }

        public override long Length
        {
            get { return 0; }
        }

        public override long Position
        {
            get { return m_position; }
            set { m_position = value; }
        }

        public override long Seek(long offset, System.IO.SeekOrigin direction)
        {
            return 0;
        }

        public override void SetLength(long length)
        {
            m_sink.SetLength(length);
        }

        public override void Close()
        {
            m_sink.Close();
        }

        public override void Flush()
        {
            m_sink.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return m_sink.Read(buffer, offset, count);
        }

  // Override the Write method to filter Response to a file.
        public override void Write(byte[] buffer, int offset, int count)
        {
            if (HttpContext.Current.Response.ContentType != "text/html")
            {
                m_sink.Write(buffer, offset, count);
                return;
            }
            string s = Microsoft.VisualBasic.Strings.StrConv(
                       Encoding.UTF8.GetString(buffer),
                       Microsoft.VisualBasic.VbStrConv.TraditionalChinese,
                       0);
            m_sink.Write(Encoding.UTF8.GetBytes(s), 0, Encoding.UTF8.GetByteCount(s));

        }
 
    
    }
}
