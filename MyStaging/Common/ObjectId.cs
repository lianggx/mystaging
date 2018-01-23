using System;
using System.Text;

namespace MyStaging.Common
{
    public class ObjectId
    {
        private readonly static ObjectIdFactory factory = new ObjectIdFactory();
        private byte[] hexData;
        public ObjectId()
        {
        }
        public ObjectId(byte[] hexData)
        {
            this.hexData = hexData;
            ReverseHex();
        }

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

        public override int GetHashCode()
        {
            return this.ToString().GetHashCode();
        }

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

        public DateTime CreationTime
        {
            get { return ObjectIdFactory.UnixEpoch.AddSeconds(timestamp); }
        }

        public static ObjectId NewId()
        {
            return factory.NewId();
        }

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

        public bool Equals(ObjectId other)
        {
            return CompareTo(other) == 0;
        }

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
        public static bool operator <(ObjectId a, ObjectId b)
        {
            return a.CompareTo(b) < 0;
        }

        public static bool operator <=(ObjectId a, ObjectId b)
        {
            return a.CompareTo(b) <= 0;
        }

        public static bool operator ==(ObjectId a, ObjectId b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(ObjectId a, ObjectId b)
        {
            return !(a == b);
        }

        public static bool operator >=(ObjectId a, ObjectId b)
        {
            return a.CompareTo(b) >= 0;
        }

        public static bool operator >(ObjectId a, ObjectId b)
        {
            return a.CompareTo(b) > 0;
        }

        public static ObjectId Empty { get { return new ObjectId("000000000000000000000000"); } }

        public byte[] Hex { get { return hexData; } }

        private int timestamp;
        public int Timestamp { get { return timestamp; } }
        private int machine;
        public int Machine { get { return machine; } }
        private int processId;
        public int ProcessId { get { return processId; } }
        private int increment;
        public int Increment { get { return increment; } }

    }
}
