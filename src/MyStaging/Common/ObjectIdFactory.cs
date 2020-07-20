using System;
using System.Diagnostics;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace MyStaging.Common
{
    /// <summary>
    ///  24 位唯一编号生成工厂对象
    /// </summary>
    public class ObjectIdFactory
    {
        /// <summary>
        ///  格林尼治时间戳
        /// </summary>
        public static DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        private readonly static UTF8Encoding utf8 = new UTF8Encoding(false);
        private readonly object inc_lock = new object();
        private byte[] pidHex;
        private byte[] machineHash;

        /// <summary>
        ///  默认构造函数
        /// </summary>
        public ObjectIdFactory()
        {
            GenerateConstants();
        }

        /// <summary>
        ///  产生一个新的 24 位唯一编号
        /// </summary>
        /// <returns></returns>
        public ObjectId NewId()
        {
            int copyIdx = 0;
            byte[] hex = new byte[12];

            byte[] time = BitConverter.GetBytes(GetTimestamp());
            Array.Reverse(time);
            Array.Copy(time, 0, hex, copyIdx, 4);
            copyIdx += 4;

            Array.Copy(this.machineHash, 0, hex, copyIdx, 3);
            copyIdx += 3;

            Array.Copy(this.pidHex, 2, hex, copyIdx, 2);
            copyIdx += 2;

            byte[] inc = BitConverter.GetBytes(this.GetInc());
            Array.Reverse(inc);
            Array.Copy(inc, 1, hex, copyIdx, 3);

            return new ObjectId(hex);
        }

        /// <summary>
        ///  根据当前机器名称和进程编号生成唯一编号指定区间的值
        /// </summary>
        private void GenerateConstants()
        {
            MD5 md5 = MD5.Create();
            string host = Dns.GetHostName();
            this.machineHash = md5.ComputeHash(utf8.GetBytes(host));

            int processId = Process.GetCurrentProcess().Id;
            this.pidHex = BitConverter.GetBytes(processId);
            Array.Reverse(pidHex);
        }

        private int increment;
        /// <summary>
        ///  获取一个自增的值
        /// </summary>
        /// <returns></returns>
        private int GetInc()
        {
            lock (inc_lock)
                return ++this.increment;
        }

        /// <summary>
        ///  获取一个时间戳，结果为格林尼治时间到目前为止的世界时钟总秒数
        /// </summary>
        /// <returns></returns>
        private int GetTimestamp()
        {
            TimeSpan ts = DateTime.UtcNow - UnixEpoch;
            double d = Math.Floor(ts.TotalSeconds);
            int totalseconds = Convert.ToInt32(d);
            return totalseconds;
        }
    }
}
