using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace version_03
{
    class FIFO
    {
        public byte[] Buffer { get; set; } // 存放内存的数组
        public int DataCount { get; set; } // 写入数据大小
        public int DataStart { get; set; } // 数据起始索引
        public int DataEnd { get; set; }  // 数据结束索引
        public FIFO(int bufferSize)
        {
            DataCount = 0;
            DataStart = 0;
            DataEnd = 0;
            Buffer = new byte[bufferSize];
        }

        public byte this[int index]
        {
            get
            {
                if (index >= DataCount) throw new Exception("环形缓冲区异常，索引溢出");
                if (DataStart + index < Buffer.Length)
                {
                    return Buffer[DataStart + index];
                }
                else
                {
                    return Buffer[(DataStart + index) - Buffer.Length];
                }
            }
        }

        public int GetDataCount() // 获得当前写入的字节数
        {
            return DataCount;
        }

        public int GetReserveCount() // 获得剩余的字节数
        {
            return Buffer.Length - DataCount;
        }

        public void Clear()
        {
            DataCount = 0;
        }

        public void Clear(int count) // 清空指定大小的数据
        {
            if (count >= DataCount) // 如果需要清理的数据大于现有数据大小，则全部清理
            {
                DataCount = 0;
                DataStart = 0;
                DataEnd = 0;
            }
            else
            {
                if (DataStart + count >= Buffer.Length)
                {
                    DataStart = (DataStart + count) - Buffer.Length;
                }
                else
                {
                    DataStart += count;
                }
                DataCount -= count;
            }
        }

        public void WriteBuffer(byte[] buffer, int offset, int count)
        {
            Int32 reserveCount = Buffer.Length - DataCount;
            if (reserveCount >= count)             // 可用空间够使用
            {
                if (DataEnd + count < Buffer.Length)      // 数据没到结尾
                {
                    Array.Copy(buffer, offset, Buffer, DataEnd, count);
                    DataEnd += count;
                    DataCount += count;
                }
                else      // 数据结束索引超出结尾 循环到开始
                {
                    System.Diagnostics.Debug.WriteLine("缓存重新开始....");
                    Int32 overflowIndexLength = (DataEnd + count) - Buffer.Length;   // 超出索引长度
                    Int32 endPushIndexLength = count - overflowIndexLength;       // 填充在末尾的数据长度
                    Array.Copy(buffer, offset, Buffer, DataEnd, endPushIndexLength);
                    DataEnd = 0;
                    offset += endPushIndexLength;
                    DataCount += endPushIndexLength;
                    if (overflowIndexLength != 0)
                    {
                        Array.Copy(buffer, offset, Buffer, DataEnd, overflowIndexLength);
                    }
                    DataEnd += overflowIndexLength;                   // 结束索引
                    DataCount += overflowIndexLength;                  // 缓存大小
                }
            }
            else
            {
                // 缓存溢出，不处理
            }
        }

        public void ReadBuffer(byte[] targetBytes, Int32 offset, Int32 count)
        {
            if (count > DataCount) throw new Exception("环形缓冲区异常，读取长度大于数据长度");
            Int32 tempDataStart = DataStart;
            if (DataStart + count < Buffer.Length)
            {
                Array.Copy(Buffer, DataStart, targetBytes, offset, count);
            }
            else
            {
                Int32 overflowIndexLength = (DataStart + count) - Buffer.Length;  // 超出索引长度
                Int32 endPushIndexLength = count - overflowIndexLength;       // 填充在末尾的数据长度
                Array.Copy(Buffer, DataStart, targetBytes, offset, endPushIndexLength);

                offset += endPushIndexLength;

                if (overflowIndexLength != 0)
                {
                    Array.Copy(Buffer, 0, targetBytes, offset, overflowIndexLength);
                }
            }
        }


        public void WriteBuffer(byte[] buffer)
        {
            WriteBuffer(buffer, 0, buffer.Length);
        }
    }
}
