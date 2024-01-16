using System;
using System.Drawing;
using System.IO.Ports;
using System.Text;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential, Pack = 1)] //强制单字节对齐 否则 Marshal.SizeOf计算结构体长度 会按照四字节对齐
struct CAN_Function                //can发送功能相关结构体  15bytes
{
    //public Byte      sendFlag;       //发送标志位
    public UInt32    sendTimes;        //发送次数
    public UInt32    sendInterval;     //发送间隔 单位100us
    public Byte      canIdType;        //can id    ID 类型
    public UInt32    CANID;            //can ID
    public Byte      canFrameType;     //can frame 帧 类型  
    public Byte      canDataLen;       //数据长度
    public Byte      idAcc;            //ID累加操作标志位
    public Byte      dataAcc;          //DATA累加操作标志位
};



namespace version_03
{


    class CanProcess
    {
        #region Manager Enums
        byte[] canFrameHeader = { 0x55, 0xAA, 0x00, 0x01};
        /// <summary>
        /// enumeration to hold our transmission types
        /// </summary>
        public enum TransmissionType { Text, Hex }

        /// <summary>
        /// enumeration to hold our message types
        /// </summary>
        public enum MessageType { Incoming, Outgoing, Normal, Warning, Error };

        CAN_Function CAN_Function_t = new CAN_Function();
        #endregion

        #region Manager Variables
        //property variables
        private bool _addEOL = true;
        private string _baudRate = string.Empty;
        private string _parity = string.Empty;
        private string _stopBits = string.Empty;
        private string _dataBits = string.Empty;
        private string _portName = string.Empty;
        private TransmissionType _transType;

        //global manager variables
        private Color[] MessageColor = { Color.Blue, Color.Green, Color.Black, Color.Orange, Color.Red };
        private SerialPort comPort = new SerialPort();
        #endregion


        /// <summary>
        /// property to hold our display window
        /// value
        /// </summary>


        public TransmissionType CurrentTransmissionType
        {
            get { return _transType; }
            set { _transType = value; }
        }

        //将Byte转换为结构体类型
    #region StructToBytes
    public static byte[] StructToBytes(object structObj,int size)
    {
        byte[] bytes = new byte[size];
        IntPtr structPtr = Marshal.AllocHGlobal(size);
        //将结构体拷到分配好的内存空间
        Marshal.StructureToPtr(structObj, structPtr, false);
        //从内存空间拷贝到byte 数组
        Marshal.Copy(structPtr, bytes, 0, size);
        //释放内存空间
        Marshal.FreeHGlobal(structPtr);
        return bytes;
  
    }

    #endregion

        #region ByteToStruct
        //将Byte转换为结构体类型
    public static object ByteToStruct(byte[] bytes, Type type)
    {
        int size = Marshal.SizeOf(type);
        if (size > bytes.Length)
        {
            return null;
        }
        //分配结构体内存空间
        IntPtr structPtr = Marshal.AllocHGlobal(size);
        //将byte数组拷贝到分配好的内存空间
        Marshal.Copy(bytes, 0, structPtr, size);
        //将内存空间转换为目标结构体
        object obj = Marshal.PtrToStructure(structPtr, type);
        //释放内存空间
        Marshal.FreeHGlobal(structPtr);
        return obj;
    }
        #endregion


    #region HexToByte
    /// <summary>
        /// method to convert hex string into a byte array
        /// </summary>
        /// <param name="msg">string to convert</param>
        /// <returns>a byte array</returns>
        private byte[] HexToByte(string msg)
        {
            //remove any spaces from the string
            msg = msg.Replace(" ", "");
            if ((msg.Length % 2) == 1)//补零
            {

                msg = '0'+msg;
            }
            //create a byte array the length of the
            //divided by 2 (Hex is 2 characters in length)
            byte[] comBuffer = new byte[msg.Length / 2];
            //loop through the length of the provided string
            for (int i = 0; i < msg.Length; i += 2)
                //convert each set of 2 characters to a byte
                //and add to the array
                comBuffer[i / 2] = (byte)Convert.ToByte(msg.Substring(i, 2), 16);
            //return the array
            return comBuffer;
        }
        #endregion

        #region ByteToHex
        /// <summary>
        /// method to convert a byte array into a hex string
        /// </summary>
        /// <param name="comByte">byte array to convert</param>
        /// <returns>a hex string</returns>
        private string ByteToHex(byte[] comByte)
        {
            //create a new StringBuilder object
            StringBuilder builder = new StringBuilder(comByte.Length * 3);
            //loop through each byte in the array
            foreach (byte data in comByte)
                //convert the byte to a string and add to the stringbuilder
                builder.Append(Convert.ToString(data, 16).PadLeft(2, '0').PadRight(3, ' '));
            //return the converted value
            return builder.ToString().ToUpper();
        }
        #endregion

        #region SetParityValues
        public void SetParityValues(object obj)
        {
            foreach (string str in Enum.GetNames(typeof(Parity)))
            {
                ((ComboBox)obj).Items.Add(str);
            }
        }
        #endregion





        /*#region CanWriteData
        public void CanWriteData(string msg)
        {
            if (!(comPort.IsOpen == true))
            {
                DisplayData(MessageType.Error, "Open Port before sending data!\n");
            }
            switch (CurrentTransmissionType)
            {
                case TransmissionType.Text:
                    //send the message to the port
                    comPort.Write(msg);
                    SendEndOfLine();
                    //display the message
                    DisplayData(MessageType.Outgoing, msg + "\n");
                    break;
                case TransmissionType.Hex:
                    try
                    {
                        //convert the message to byte array
                        byte[] newMsg = HexToByte(msg);
                        //send the message to the port
                        comPort.Write(newMsg, 0, newMsg.Length);
                        SendEndOfLine();
                        //convert back to hex and display
                        DisplayData(MessageType.Outgoing, ByteToHex(newMsg) + "\n");
                    }
                    catch (FormatException ex)
                    {
                        //display error message
                        DisplayData(MessageType.Error, ex.Message + "\n");
                    }
                    finally
                    {
                        _displayWindow.SelectAll();
                    }
                    break;
                default:
                    //send the message to the port
                    comPort.Write(msg);
                    SendEndOfLine();
                    //display the message
                    DisplayData(MessageType.Outgoing, msg + "\n");
                    break;
            }
        }

        /// <summary>
        /// Method to send END_OF_LINE (0x0D) if connection is open
        /// </summary>
        public void SendEndOfLine()
        {
            byte[] end_of_line = { 0x0D };
            if ((comPort.IsOpen == true) && (true == _addEOL))
                comPort.Write(end_of_line, 0, 1);
        }
        #endregion*/

        #region CRC_8
        private byte CRC_8(byte[] source, int len)
        {
            byte CRC = 0;
            int i;
            int count = 0;
            while ((len--) != 0)
            {
                CRC ^= source[count];
                count++;
                for (i = 0; i < 8; i++)
                {
                    if ((CRC & 0x01) == 1)
                    {
                        CRC = (byte)((CRC >> 1) ^ 0x8C);
                    }
                    else
                    {
                        CRC >>= 1;
                    }
                }
            }
            return CRC;
        }
        #endregion

        public object BytesToStruct(byte[] bytes, Type strcutType)
        {
            int size = Marshal.SizeOf(strcutType);
            IntPtr buffer = Marshal.AllocHGlobal(size);
            try
            {
                Marshal.Copy(bytes, 0, buffer, size);
                return Marshal.PtrToStructure(buffer, strcutType);
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }

        }

        public string ParsingCommand(byte bytes)
        {
            string _cancmd;
            switch (bytes)
            {
                case 0x00:
                    _cancmd = "心跳数据";
                    break;
                case 0x01:
                    _cancmd = "接收失败";
                    break;
                case 0x11:
                    _cancmd = "接收    ";
                    break;
                case 0x02:
                    _cancmd = "发送失败";
                    break;
                case 0x12:
                    _cancmd = "发送    ";
                    break;
                case 0x03:
                    _cancmd = "波特率设置失败";
                    break;
                case 0x13:
                    _cancmd = "波特率设置成功";
                    break;
                default:
                    _cancmd = "未知命令";
                    break;
            }
            return _cancmd;
        }


        #region SendToCan
        //tocan.SendToCan(vcom, richTexSend.Text.ToUpper(), frameId.Text, sendTimes.Text, sendInterval.Text, sendTimes.Text, frameType.Text, frameFormat.Text);
        public void SendToCan(SerialPort com, string data, string frameId_t, string sendTimes_t, string sendInterval_t, Byte frameIdType_t, Byte frameFormat_t, Byte idacc, byte dataacc )
        {
            if (!(com.IsOpen == true))
            {
                //DisplayData(MessageType.Error, "Open Port before sending data!\n");
            }

            try
            {
                if (frameId_t == "")//ID 大于16bit或者为空
                {
                    MessageBox.Show("帧ID设置错误，请更正！","警告");
                    return;

                }
                //convert the message to byte array
                byte[] newMsg = HexToByte(data);
                //byte[] newId = HexToByte(frameId_t);
                byte CRC = 0;
                CAN_Function_t.canDataLen    = (byte)newMsg.Length;  //CAN数据长度
                CAN_Function_t.sendTimes     = uint.Parse( sendTimes_t);
                float temp = 0;
                temp =  float.Parse(sendInterval_t);
                if (temp < 0.1)
                {
                    MessageBox.Show("发送时间间隔最小是0.1ms！并且实际发送速度受波特率影响", "提示");
                    //return;
                }

                UInt32 _temp = Convert.ToUInt32(temp * 10);

                CAN_Function_t.sendInterval  = _temp;
                CAN_Function_t.canFrameType  = frameIdType_t;
                CAN_Function_t.canIdType     = frameFormat_t;

                if (CAN_Function_t.canIdType == 0)   // 标准帧 扩展帧
                {

                    if(frameId_t.Length>3)
                        frameId_t = frameId_t.Substring(frameId_t.Length - 3);  //仅取后三位
                    CAN_Function_t.CANID = Convert.ToUInt32(frameId_t, 16); //仅0x7FF有效 低11位ID
                    CAN_Function_t.CANID &= 0x7FF;
                }

                else
                {
                    if (frameId_t.Length > 8)
                        frameId_t = frameId_t.Substring(frameId_t.Length - 8);  //仅取后三位
                    CAN_Function_t.CANID = Convert.ToUInt32(frameId_t, 16);
                    CAN_Function_t.CANID &= 0x1FFFFFFF;
                } 
                CAN_Function_t.idAcc   = idacc;
                CAN_Function_t.dataAcc = dataacc;

                canFrameHeader[2] = (byte) (canFrameHeader.Length + Marshal.SizeOf(new CAN_Function()) + 8 + 1); //本帧的总长度

                byte[] frame = new byte[canFrameHeader.Length + Marshal.SizeOf(new CAN_Function()) + 8 + 1];  //


                canFrameHeader.CopyTo(frame, 0);              //拼接帧头

                //newMsg.CopyTo(frame, canFrameHeader.Length);

                byte[] newCAN_Functio = StructToBytes(CAN_Function_t, Marshal.SizeOf(new CAN_Function()));

                newCAN_Functio.CopyTo(frame, canFrameHeader.Length);                                              //拼接帧头//拼接can结构信息  Marshal.SizeOf(new CAN_Function())
                byte i = (byte)Marshal.SizeOf(new CAN_Function());
                newMsg.CopyTo(frame, canFrameHeader.Length + Marshal.SizeOf(new CAN_Function()));



                CRC = CRC_8(frame, frame.Length);

                frame[canFrameHeader.Length + Marshal.SizeOf(new CAN_Function()) + 8] = CRC;

                //send the message to the port
                
                com.Write(frame, 0, frame.Length);

                //等待CAN数据发送状态

              /*  UInt32 count = 0;
                byte  bufferlen = 15;  //send fail! or send success!
                while ((com.BytesToRead != bufferlen) && (count < 0xFFFFF))
                {
                    count++;
                    if (count >= 0xFFFFF)
                    {
                        MessageBox.Show("发送失败！", "提示");
                        return;
                    }
                }
                byte[] Rxbuff = new byte[bufferlen];
                com.Read(Rxbuff, 0, bufferlen);  //读发送状态 
                string str = System.Text.Encoding.ASCII.GetString(Rxbuff);
                string sendState = "发送";
                 if (str.Equals("Send Fail   !\r\n"))
                 {
                     sendState = "发送成功";
                 }
                 else if (str.Equals("Send Fail   !\r\n"))
                 {
                     sendState = "发送失败";
                 }
                
                //convert back to hex and display
                 DisplayData(MessageType.Outgoing, sendState + ByteToHex(newMsg) + "\n");*/
            }
            catch (FormatException ex)
            {
                //display error message
                //DisplayData(MessageType.Error, ex.Message + "\n");
            }
            finally
            {
                //_displayWindow.SelectAll();
            }

        }
        #endregion


    [DllImport("kernel32.dll")]
        public static extern bool SetProcessWorkingSetSize(IntPtr proc, int min, int max);

        public void FlushMemory()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                SetProcessWorkingSetSize(System.Diagnostics.Process.GetCurrentProcess().Handle, -1, -1);
            }
        }


    }
}

