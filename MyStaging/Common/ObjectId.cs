using System;
using System.Text;

namespace MyStaging.Common
{
    /// <summary>
    ///  生成 24 位唯一编号管理对象
    /// </summary>
    public class ObjectId
    {
        private readonly static ObjectIdFactory factory = new ObjectIdFactory();
        private byte[] hexData;

        /// <summary>
        ///  默认构造函数
        /// </summary>
        public ObjectId()
        {
        }

        /// <summary>
        ///  根据传入的 hex（24位唯一编号）数据构造管理对象 
        /// </summary>
        /// <param name="hexData"></param>
        public ObjectId(byte[] hexData)
        {
            this.hexData = hexData;
            ReverseHex();
        }

        /// <summary>
        ///  将对象序列化为 24 位唯一编号
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (hexData == null)
                hexData = new byte[12];

            StringBuilder hexText = new StringBuilder();
            for (int i = 0; i < this.hexData.Length; i++)
            {
                hexText.Append(this.hexData[i].ToString("x2"));
            }
            return hexText.ToString();
        }

        /// <summary>
        ///  获取 24 位唯一编号的哈希值
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return this.ToString().GetHashCode();
        }

        /// <summary>
        ///  使用一组 24 位唯一编号初始化管理对象
        /// </summary>
        /// <param name="value"></param>
        public ObjectId(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentNullException("value");
            }
            if (value.Length != 24)
            {
                throw new ArgumentOutOfRangeException("value should be 24 characters");
            }

            this.hexData = new byte[12];
            for (int i = 0; i < value.Length; i += 2)
            {
                try
                {
                    this.hexData[i / 2] = Convert.ToByte(value.Substring(i, 2), 16);
                }
                catch
                {
                    this.hexData[i / 2] = 0;
                }
            }

            ReverseHex();
        }

        /// <summary>
        ///  反序列化 24 位唯一编号为管理对象
        /// </summary>
        private void ReverseHex()
        {
            int copyIdx = 0;
            byte[] time = new byte[4];
            Array.Copy(this.hexData, copyIdx, time, 0, 4);
            Array.Reverse(time);
            this.timestamp = BitConverter.ToInt32(time, 0);
            copyIdx += 4;

            byte[] mid = new byte[4];
            Array.Copy(this.hexData, copyIdx, mid, 0, 3);
            this.machine = BitConverter.ToInt32(mid, 0);
            copyIdx += 3;

            byte[] pids = new byte[4];
            Array.Copy(this.hexData, copyIdx, pids, 0, 2);
            Array.Reverse(pids);
            this.processId = BitConverter.ToInt32(pids, 0);
            copyIdx += 2;

            byte[] inc = new byte[4];
            Array.Copy(this.hexData, copyIdx, inc, 0, 3);
            Array.Reverse(inc);
            this.increment = BitConverter.ToInt32(inc, 0);
        }

        /// <summary>
        ///  获取一个时间戳
        /// </summary>
        public DateTime CreationTime
        {
            get { return ObjectIdFactory.UnixEpoch.AddSeconds(timestamp); }
        }

        /// <summary>
        ///  生成新的 24 位唯一编号
        /// </summary>
        /// <returns></returns>
        public static ObjectId NewId()
        {
            return factory.NewId();
        }

        /// <summary>
        ///  将两个管理对象进行比较，并输出比较结果，比较结果为：当前对象大于目标：1，小于目标：-1，两个对象相等：0
        /// </summary>
        /// <param name="other">要比较的目标对象</param>
        /// <returns></returns>
        public int CompareTo(ObjectId other)
        {
            if (ReferenceEquals(other, null))
                return 1;
            for (int i = 0; i < this.hexData.Length; i++)
            {
                if (this.hexData[i] < other.hexData[i])
                    return -1;
                else if (this.hexData[i] > other.hexData[i])
                    return 1;
            }
            return 0;
        }

        /// <summary>
        /// 将两个管理对象进行比较，如果一致，则返回 true，反之返回 false
        /// </summary>
        /// <param name="other">要比较的目标对象</param>
        /// <returns></returns>
        public bool Equals(ObjectId other)
        {
            return CompareTo(other) == 0;
        }

        /// <summary>
        /// 将两个管理对象进行比较，如果一致，则返回 true，反之返回 false
        /// </summary>
        /// <param name="other">要比较的目标对象</param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj is ObjectId)
            {
                return Equals((ObjectId)obj);
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        ///  运算符重载 <
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool operator <(ObjectId a, ObjectId b)
        {
            return a.CompareTo(b) < 0;
        }

        /// <summary>
        ///  运算符重载 <=
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool operator <=(ObjectId a, ObjectId b)
        {
            return a.CompareTo(b) <= 0;
        }

        /// <summary>
        ///  运算符重载 ==
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool operator ==(ObjectId a, ObjectId b)
        {
            return a.Equals(b);
        }

        /// <summary>
        ///  运算符重载 !=
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool operator !=(ObjectId a, ObjectId b)
        {
            return !(a == b);
        }

        /// <summary>
        ///  运算符重载 >=
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool operator >=(ObjectId a, ObjectId b)
        {
            return a.CompareTo(b) >= 0;
        }

        /// <summary>
        ///  运算符重载 >
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool operator >(ObjectId a, ObjectId b)
        {
            return a.CompareTo(b) > 0;
        }

        /// <summary>
        ///  获取一个长度为 24 位的值为 000000000000000000000000 的编号
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static ObjectId Empty { get { return new ObjectId("000000000000000000000000"); } }

        /// <summary>
        ///  获取 24 唯一编号进行 hex 后的值
        /// </summary>
        public byte[] Hex { get { return hexData; } }

        private int timestamp;
        /// <summary>
        ///  获取当前生成的时间戳
        /// </summary>
        public int Timestamp { get { return timestamp; } }

        /// <summary>
        ///  获取当前机器的名称
        /// </summary>
        private int machine;
        public int Machine { get { return machine; } }

        private int processId;
        /// <summary>
        ///  获取当前进程编号
        /// </summary>
        public int ProcessId { get { return processId; } }

        private int increment;
        /// <summary>
        ///  或者当前管理对象自增值
        /// </summary>
        public int Increment { get { return increment; } }

    }
}
