
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Management;
using System.IO.Ports;
using Microsoft.VisualBasic;
using System.IO;
using System.Threading;
using System.Text.RegularExpressions;
using Microsoft.Win32;
using System.Drawing.Drawing2D;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using ScottPlot.Plottable;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using ScottPlot.Drawing.Colormaps;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Security.Cryptography;
using System.Security.Policy;
using ScottPlot.Palettes;
using MathNet.Numerics.Distributions;

struct CAN_Fream                 //can发送功能相关结构体  16bytes
{
    public Byte freamHeader;          //发送标志位
    public Byte CMD;                  //CAN 命令  //00 心跳  0x01  接收失败  0x11 接收成功   0x02 发送失败  0x12 发送成功  0x03 波特率设置失败  0x13 波特率设置成功
    public Byte canDataLen;           //:6 数据长度
    //uint8_t canIde:1;                  //ide:0,标准帧;1,扩展帧
    //uint8_t canRtr:1;                  //rtr:0,数据帧;1,远程帧
    public UInt32 CANID;                //can ID  
    [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 8, ArraySubType = UnmanagedType.Struct)]  //指定canData 数组长度=8
    public byte[] canData;               //Can 数据 
    public Byte freamEnd;             //结尾
};



namespace version_03
{
    public partial class Form1 : Form
    {


        // Robot robot = new Robot();
        // PIDControal pidcontrol = new PIDControal();
        SerialPort vcom = new SerialPort();
        CanProcess tocan = new CanProcess();
        FIFO canRxFIFO = new FIFO(16 * 1024 * 1024);  //开辟16M can  FIFO
        FIFO uartRxFIFO = new FIFO(16 * 1024 * 1024);  //开辟16M 串口 FIFO
        vSeralPortProsess comm = new vSeralPortProsess();
        readonly vSeralPortProsess vcomp = new vSeralPortProsess();
        delegate void CANDataUpdateDelegate();

        private bool _closing;     //是否正在关闭串口，执行Application.DoEvents，并阻止再次
        private Int32 com_bps = 0;
        private double com_bit = 0;
        public bool comFlag = false;
        private long uCount_Rx = 0;
        private long nCount_Rx = 0;
        private long nCount_Tx = 0;
        private bool _formclosing = true; //窗口被关闭，阻止其余线程运行  
        static byte[] MotorDate = new byte[16];//将电机发来的帧数据储存      
        
        bool isButtonClick = false;
        public int listViewIndex = 0;
        public int listviewCountTemp = 0;
        public UInt32 listRemoveCount = 0;
        public UInt16 acToolsHeartTickCount = 0;
        public bool baudrateSetState = false;

        public double[] angeldate = new double[100_000];
        public double[] angeldate_d = new double[100_000];
        public double[] torquedata = new double[100_000];
        public double[] torquedata_d = new double[100_000];
        public double[] caltimedata = new double[100_000];
        public double[] caltimedata_d = new double[100_000];
        public double[] veloccitydata = new double[100_000];
        public double[] veloccitydata_d = new double[100_000];
        int nextDataIndex = 1;
        SignalPlot Plotangle;
        SignalPlot Plotangle1;
        SignalPlot Plotvelocity;
        SignalPlot Plotvelocity1;
        SignalPlot PlotTime;
        SignalPlot PlotTime1;
        SignalPlot PlotTorque;
        SignalPlot PlotTorque1;

        System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();

        public Form1()
        {
            InitializeComponent();

            //串口数据接收进程
            Thread comReceiveData = new Thread(scomReceiveData);
            comReceiveData.IsBackground = true;  //窗体关闭时，关闭该进程。
            comReceiveData.Start();

            //CAN数据处理进程
            Thread trdCanDataAnalysis = new Thread(CanDataAnalysis);
            trdCanDataAnalysis.IsBackground = true;  //窗体关闭时，关闭该进程。
            trdCanDataAnalysis.Start();

            //定时更新UI进程
             Thread regularUpdate = new Thread(RegularUpdate);
             regularUpdate.IsBackground = true;  //窗体关闭时，关闭该进程。
             regularUpdate.Start();

        }


        /************************窗体操作***************************/
        public bool CloseDevice(SerialPort com)
        {
            _closing = true;
            if (com.IsOpen)
            {
                com.DiscardOutBuffer();
                com.DiscardInBuffer();
                com.Close();
                if (!com.IsOpen)
                    return true;
            }
            MessageBox.Show("设备关闭失败！", "提示");
            return false;
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            //软件刚打开就将所有的参数设置位默认参数
            //这些是can的帧的格式参数
            SetDefaultsParameter();
            //下面是串口的参数默认值
            string[] ports = SerialPort.GetPortNames();//获取可用串口
            //↓↓↓↓↓↓↓↓↓波特率下拉控件↓↓↓↓↓↓↓↓↓
            comboBoxBaudrate.SelectedIndex = 9;
            DataBit.SelectedIndex = 0;
            StopBit.SelectedIndex = 0;
            CheckBit.SelectedIndex = 0;
            SetrichTextBox("晴连接串口并使能电机\n");
            SetrichTextBox("--------------------\n");
            SetrichTextBox("如果想跟踪期望轨迹请先初始化，并等待初始化完成\n");
            SetrichTextBox("......\n");

            

        }
        /************************Ｅ　Ｎ　Ｄ*************************/



        //在串口数据接收进程
        void scomReceiveData()
        {
            while (true)
            {
                //Thread.Sleep(1);


                if(true)//if(do_now)
                {
                if (!comFlag)//当comFlag为真时不再循环
                {
                    continue;//重新开始循环
                }
                if (_closing) continue; //如果正在关闭，忽略操作，直接返回，尽快的完成串口监听线程的一次循环
                if (!_formclosing) continue; //窗体关闭 禁止执行线程
                if (!vcom.IsOpen) continue;  //串口被意外关闭
                try
                {
                    int _temp = vcom.BytesToRead;//     Gets the number of bytes of data in the receive buffer.
                                                 //这里的意思是接收的字节数量会大于16个字节，也就是不只一帧CAN数据
                    for (; (_temp >= 16) && (vcom.IsOpen);)//一直进行for循环直到串口读取的数据byte少于16个，或者，串口为关闭的状态
                    {//这里是从电机读取can数据
                        _temp = vcom.BytesToRead;   //返回读取的字节数
                        byte[] buf = new byte[16];
                        if ((vcom.IsOpen))
                        {
                            _temp -= 16;
                            vcom.Read(buf, 0, 16);   //将数据读取到buf中
                        }
                        else continue;
                        //到这里buf里面已经有数据了，16个byte
                        if ((buf[0] == 0xAA) && (buf[15] == 0x55))
                        {//在帧头和帧尾都匹配上的情况下
                            lock (canRxFIFO)
                            {
                                canRxFIFO.WriteBuffer(buf, 0, 16);//将buf里的16个字节写到fifo里面
                            }
                            continue;
                        }
                        else
                        {
                            for (byte i = 0; i < 16; i++) //寻找帧头
                            {
                            upack: if (buf[i] == 0xAA)
                                {
                                    if (i == 0) //防止截取后 第一帧就是
                                    {
                                        if (buf[15] == 0x55) //此帧是
                                        {
                                            lock (canRxFIFO)
                                            {
                                                canRxFIFO.WriteBuffer(buf, 0, 16);
                                            }
                                            i = 16; //退出
                                        }
                                    }
                                    else if ((i == 0) && (_temp == 0)) //此帧不是
                                    {
                                        lock (uartRxFIFO)
                                        {
                                            uartRxFIFO.WriteBuffer(buf, 0, 16);
                                        }
                                        i = 16; //退出
                                    }
                                    else if (_temp >= i)  //后面还有数据
                                    {//_temp是串口接收的的字节数
                                        byte[] _buf = new byte[1024];
                                        if ((vcom.IsOpen))
                                        {
                                            vcom.Read(_buf, 0, i);
                                            _temp -= i;
                                            if (_buf[i - 1] == 0x55) //此帧是
                                            {
                                                byte[] _temp_buf = new byte[16];
                                                Array.Copy(buf, i, _temp_buf, 0, 16 - i);
                                                Array.Copy(_buf, 0, _temp_buf, 16 - i, i);

                                                lock (canRxFIFO)
                                                {
                                                    canRxFIFO.WriteBuffer(_temp_buf, 0, 16);
                                                }
                                                lock (uartRxFIFO)
                                                {
                                                    uartRxFIFO.WriteBuffer(buf, 0, i);
                                                }
                                                i = 16;  //下一次循环
                                            }
                                            else
                                            {
                                                lock (uartRxFIFO)
                                                {
                                                    uartRxFIFO.WriteBuffer(buf, 0, i);
                                                }
                                                //复位数组 重新开始寻找
                                                byte[] _temp_buf = new byte[16];
                                                Array.Copy(buf, i, _temp_buf, 0, 16 - i);
                                                Array.Copy(_buf, 0, _temp_buf, 16 - i, i);
                                                Array.Copy(_temp_buf, 0, buf, 0, 16);

                                                i = 0;
                                                goto upack;
                                                //continue;
                                            }
                                        }
                                    }
                                    else  //不是CAN数据
                                    {
                                        lock (uartRxFIFO)
                                        {
                                            uartRxFIFO.WriteBuffer(buf, 0, 16);
                                        }
                                        i = 16;  //下一次循环
                                        //continue;
                                    }
                                }
                                if (i == 15) //循环完  未发现帧头
                                {
                                    lock (uartRxFIFO)
                                    {
                                        uartRxFIFO.WriteBuffer(buf, 0, 16);
                                    }
                                }
                            }

                        }
                    }
                    if ((_temp < 16) && (_temp > 0))  //非CAN 数据帧
                    {
                        byte[] __buf = new byte[_temp + 1024];
                        if ((vcom.IsOpen))
                        {
                            vcom.Read(__buf, 0, _temp);
                            lock (uartRxFIFO)
                            {
                                uartRxFIFO.WriteBuffer(__buf, 0, _temp);
                            }
                        }

                    }
                }
                catch
                {
                    continue;
                }
                    do_now = false;
                }
            }
        }



        /************************串口操作***************************/
        private void SetDefaultsParameter()  //设置默认参数
        {

            string[] ports = SerialPort.GetPortNames();//获取可用串口
            if (ports.Length > 0)//ports.Length > 0说明有串口可用
            {
                comboBoxPort.Items.AddRange(ports);
                comboBoxPort.SelectedIndex = 0;//默认选第1个串口
            }
            else//未检测到串口
            {
                MessageBox.Show("无可用串口");
            }

        }
        private void openButton_Click(object sender, EventArgs e)
        {

            if (!vcom.IsOpen) //未打开
            {
                _closing = false;
                vcom.Encoding = Encoding.Default;
                string strDataBit = DataBit.SelectedItem.ToString();
                string strCheckBit = CheckBit.SelectedItem.ToString();
                string strStopBit = StopBit.SelectedItem.ToString();
                Int32 iDataBit = Convert.ToInt32(strDataBit);
                vcom = new SerialPort();
                vcom.PortName = comboBoxPort.SelectedItem.ToString();
                vcom.BaudRate = Convert.ToInt32(comboBoxBaudrate.SelectedItem.ToString());//波特率;
                vcom.DataBits = iDataBit;//数据位
                switch (strStopBit)            //停止位
                {
                    case "One":
                        vcom.StopBits = StopBits.One;
                        break;
                    case "OnePointFive":
                        vcom.StopBits = StopBits.OnePointFive;
                        break;
                    case "Two":
                        vcom.StopBits = StopBits.Two;
                        break;
                    default:
                        MessageBox.Show("Error：停止位参数不正确!", "Error");
                        break;
                }
                //textBox_receive.Text += " switch (strCheckBit) ";
                switch (strCheckBit)             //校验位
                {
                    case "None":
                        vcom.Parity = Parity.None;
                        break;
                    case "Odd":
                        vcom.Parity = Parity.Odd;
                        break;
                    case "Even":
                        vcom.Parity = Parity.Odd;
                        break;
                    default:
                        MessageBox.Show("Error：校验位参数不正确!", "Error");
                        break;
                }

                vcom.ReadTimeout = 500;
                vcom.WriteTimeout = 500;
                if (!vcom.IsOpen)
                {
                    try
                    {
                        vcom.Open();
                        if (serialPort.IsOpen)
                        {
                            vcom.NewLine = "/r/n";
                            vcom.RtsEnable = true;
                        }
                    }
                    catch (System.Exception ex)
                    {
                        vcom = new SerialPort();
                        MessageBox.Show("无法打开设备:" + ex.Message, "Error"); ;
                    }

                }
                //统计使用
                com_bps = Convert.ToInt32(comboBoxBaudrate.SelectedItem.ToString());
                byte _stopbit = 0;
                byte _parity = 0;
                UInt32 _baudrate = 0;
                _baudrate = Convert.ToUInt32(comboBoxBaudrate.SelectedItem.ToString());
                switch (serialPort.StopBits.ToString())
                {
                    case "One":
                        _stopbit = 1;
                        break;
                    case "Two":
                        _stopbit = 2;
                        break;
                }
                if (serialPort.Parity.ToString() == "None")
                    _parity = 0;
                else
                    _parity = 1;


                com_bit = iDataBit + _stopbit + _parity;


                if (vcom.IsOpen == true)
                {
                    openButton.BackColor = Color.Green; //修改背景色
                    openButton.ForeColor = Color.White;
                    openButton.Text = "关闭";
                    //openButton.Text.
                    buttonFresh.Enabled = false;
                    comFlag = true;
                }
            }
            else
            {
                _closing = true;
                if (CloseDevice(vcom) == true)
                {
                    openButton.BackColor = Color.Gray; //修改背景色
                    openButton.ForeColor = Color.Black;
                    openButton.Text = "打开";
                    //清空canFIFO
                    canRxFIFO.Clear(canRxFIFO.DataCount);
                    buttonFresh.Enabled = true;
                }
            }
        }
        private void buttonFresh_Click(object sender, EventArgs e)
        {
            comboBoxPort.Items.Clear();
            string[] ports = SerialPort.GetPortNames();//获取可用串口
            string[] unique = ports.Distinct().ToArray(); //删除重复项
            if (unique.Length > 0)//ports.Length > 0说明有串口可用
            {
                comboBoxPort.Items.AddRange(unique);
                comboBoxPort.SelectedIndex = 0;//默认选第1个串口
            }
            else//未检测到串口
            {
                MessageBox.Show("无可用串口");
            }
        }
        private void freshCAN_richBox_Click(object sender, EventArgs e)
        {//点击一下串口刷新按钮，用来显示电机发来的数据
            int readlength = uartRxFIFO.DataCount;
            byte[] buf = new byte[readlength];
            uartRxFIFO.ReadBuffer(buf, 0, readlength);
            uartRxFIFO.Clear(readlength);
            uCount_Rx += buf.Length;
            UpdateText(buf, buf.Length);  //更新显示
        }
        private void UpdateText(byte[] text, int length)
        {
            try
            {
                // nCount_Rx += length;

                //this.Setlabel_RXThread = new Thread(new ThreadStart(this.Setlabel_RXThreadProcSafe));
                //this.Setlabel_RXThread.Start();

                string str = System.Text.Encoding.Default.GetString(text);
                str = str.Replace("\0", ""); //防止textBox_receive.AppendText遇到 \0 舍弃后面的字符串
                str = str.Replace("\r", "");
                str = str.Replace("\n", Environment.NewLine); //替换换行符
                if (!str.Contains("\n")) //增加换行符
                {
                    str += Environment.NewLine;
                }
                CAN_richBox.Text = str;

            }
            catch (Exception ex)
            {
                //if (_debug)
                MessageBox.Show(ex.Message);
            }

        }
        /************************Ｅ　Ｎ　Ｄ*************************/




        /************************CAN操作***************************/
        public void CanDataAnalysis()//在这里面截取电机发来的帧数据
        {

            while (true)
            {
                //Thread.Sleep(1);
                if(true)//if (do_now)
                {


                    if (!_formclosing) continue; //窗体关闭 禁止执行线程
                    if (!vcom.IsOpen) continue;  //串口被意外关闭
                    for (; (canRxFIFO.DataEnd >= 16) && (vcom.IsOpen);)
                    {
                        try
                        {

                            byte[] buf = new byte[16];

                            lock (canRxFIFO)
                            {
                                canRxFIFO.ReadBuffer(buf, 0, 16);//把fifo里面的数据读取到buf中                            
                                canRxFIFO.Clear(16);
                            }
                            if (buf[3] == 0) MotorDate = buf;
                            switch (MotorDate[7])
                            {
                                case 1://关节1
                                    JointState_B[3] = Byt2Float(MotorDate[10], MotorDate[11], "Vel");
                                    JointState_B[0] = Byt2Float(MotorDate[8], MotorDate[9], "Pos");
                                    break;
                                case 2://关节2
                                    JointState_B[4] = Byt2Float(MotorDate[10], MotorDate[11], "Vel");
                                    JointState_B[1] = Byt2Float(MotorDate[8], MotorDate[9], "Pos");
                                    break;
                                case 3://关节3
                                    JointState_B[5] = -Byt2Float(MotorDate[10], MotorDate[11], "Vel");
                                    JointState_B[2] = -Byt2Float(MotorDate[8], MotorDate[9], "Pos");
                                    //if (JointState_B[2] >0.5) InputMotorTor((float)-0, 3);
                                    break;

                                default:
                                    CAN_richBox.Text += "出错了";
                                    break;
                            }

                        }
                        catch //()
                        {
                            continue;
                        }



                    }
                    do_now = false;
                }
            }
        }

        private bool do_now = false;//记录程序采样的次数
        private float Time ;//记录算法运行总时长
        long  last_time = 0, SaveTime = 0;
        private double Pos;
        private void RegularUpdate()
        {
            while (true)
            { 
                //Thread.Sleep(1);
                if (_formclosing)
                {
                    // do_now = false;

                    if (watch.ElapsedMilliseconds >= 1)
                    {

                        do_now = true;

                        //设置一个委托可以在这个子线程里执行CAN_richBox所在线程里的CAN_richBox显示
                        if (is_shijiao == 0)
                        {// 轨迹跟踪模式
                            CANDataUpdateDelegate listDelegate = new CANDataUpdateDelegate(CANDataUpdate);
                            CAN_richBox.Invoke(listDelegate);
                        }
                        else
                        {//拖动模式
                            CANDataUpdateDelegate listDelegate = new CANDataUpdateDelegate(CANDataUpdate_thech); 
                            CAN_richBox.Invoke(listDelegate);
                        }

                        watch.Restart();

                    }
                }

            }
        }

        
        //期望关节状态数组大小是6000也就是运行时间不能超过6秒
        double[] ControlT = new double[3];
        double[] toleranceT = new double[2];
        double[] referenceT_1 = new double[6001];
        double[] referenceT_2 = new double[6001];
        double[] referenceT_3 = new double[6001];
        double[] JointState_B = new double[6];      //实际关节状态
        double[] q1_D   = new double[6001];      //期望关节状态
        double[] q2_D   = new double[6001];      //期望关节状态
        double[] q3_D   = new double[6001];      //期望关节状态
        double[] dq1_D  = new double[6001];      //期望关节状态
        double[] dq2_D  = new double[6001];      //期望关节状态
        double[] dq3_D  = new double[6001];      //期望关节状态
        double[] ddq1_D = new double[6001];      //期望关节状态
        double[] ddq2_D = new double[6001];      //期望关节状态
        double[] ddq3_D = new double[6001];      //期望关节状态

        //用来给算法参数赋值的全局变量
        //由界面按钮控制加减
        double A1 = 0;//d_1
        double A2 = 0;//d_2
        double B1 = 0;//d_3
        double B2 = 0;//K_1
        double C1 = 0;//K_2
        double C2 = 0;//K_3

        /**控制运算主程序**/
        void CANDataUpdate()
        {
            double[] JointState_D = new double[9];
            double[] JointState = new double[9];
            double[] estimateT = new double[3];
            double[] ControlT = new double[3];        
            double[] s = new double[3];
            double[] e = new double[3];
            double[] de = new double[3];
            double[] K = new double[3];
            double[] V= new double[3];
            double[] Yd = new double[3];
            double[] d_tild = new double[3];
            double detlta,H3_qdd3, H2_qdd2,H1_qdd1 ;
            
            if (!vcom.IsOpen)
            {
                buttonFresh.Enabled = true;
                openButton.Text = @"打开";
                openButton.BackColor = SystemColors.Control; //恢复颜色
                openButton.UseVisualStyleBackColor = true;
                comFlag = false;
                //清空canFIFO
                if (canRxFIFO.DataCount > 0)
                {
                    canRxFIFO.Clear(canRxFIFO.DataCount);
                }
            }
            if (isButtonClick)//只有点击开始运算时的时候才显示isButtonClick
            {

                //软件角度限制                       
                if (Math.Abs(JointState_B[1]) > 2.5f || Math.Abs(JointState_B[2]) > 2.5f)
                {
                    //CAN_richBox.Text += JointState_B[0] + ": 好家伙！！角度太大了！！\n";
                    InputMotorTor(0.0f, 1);
                    InputMotorTor(0.0f, 2);
                }

                //步数自增
                last_time++;

                K[0] = 54 + B2;

                K[1] = 27 + C1;

                K[2] = 90 + C2;


                /*期 望 关 节 状 态*/
                JointState_D[0] = q1_D[last_time];
                JointState_D[1] = q2_D[last_time];
                JointState_D[2] = q3_D[last_time];

                JointState_D[3] = dq1_D[last_time];
                JointState_D[4] = dq2_D[last_time];
                JointState_D[5] = dq3_D[last_time];

                JointState_D[6] = ddq1_D[last_time];
                JointState_D[7] = ddq2_D[last_time];
                JointState_D[8] = ddq3_D[last_time];

                /* 误 差 */
                e[0] = JointState_D[0] - JointState_B[0];
                de[0] = JointState_D[3] - JointState_B[3];

                e[1] = JointState_D[1] - JointState_B[1];
                de[1] = JointState_D[4] - JointState_B[4];

                e[2] = JointState_D[2] - JointState_B[2];
                de[2] = JointState_D[5] - JointState_B[5];

                /*切 换 函 数*/
                s[0] = de[0] + K[0] * e[0];
                s[1] = de[1] + K[1] * e[1];
                s[2] = de[2] + K[2] * e[2];

                /*计算力矩算法实现*/
                //正动力学计算的估计加速度，但是用常数代替了，节省了惯性矩阵求逆的计算
                /**/
                //estimateT = Closed_Arm_Modle_decoup(JointState_D);
                //d1_tild = estimateT[0] / H1_qdd1;
                //d2_tild = estimateT[1] / H2_qdd2;
                //d3_tild = estimateT[2] / H3_qdd3;

                //JointState[0] = JointState_B[0];//q1_b
                //JointState[1] = JointState_B[1];//q2_b
                //JointState[2] = JointState_B[2];//q3_b
                //JointState[3] = JointState_B[3];//qd1_b
                //JointState[4] = JointState_B[4];//qd2_b
                //JointState[5] = JointState_B[5];//qd3_b
                //JointState[6] = JointState_D[6];//v1
                //JointState[7] = JointState_D[7];//v2
                //JointState[8] = JointState_D[8];//v3

                //d_tild = get_d_tiled(JointState);

                //Yd[0] = (d_tild[0] + A1) * Math.Tanh(s[0] / 1);
                //Yd[1] = (d_tild[1] + A2) * Math.Tanh(s[1] / 3.0);
                //Yd[2] = (d_tild[2] + B1) * Math.Tanh(s[2] / 3.0);


                Yd[0] = (54 + A1) * Math.Tanh(s[0] / 3.0);
                Yd[1] = (50 + A2) * Math.Tanh(s[1]);
                Yd[2] = (110 + B1) * Math.Sign(s[2]);
                V[0] = JointState_D[6] + K[0] * de[0] + Yd[0];
                V[1] = JointState_D[7] + K[1] * de[1] + Yd[1];
                V[2] = JointState_D[8] + K[2] * de[2] + Yd[2];
                /********************************************/

                JointState[0] = JointState_B[0];//q1_b
                JointState[1] = JointState_B[1];//q2_b
                JointState[2] = JointState_B[2];//q3_b
                JointState[3] = JointState_B[3];//qd1_b
                JointState[4] = JointState_B[4];//qd2_b
                JointState[5] = JointState_B[5];//qd3_b
                JointState[6] = V[0];//v1
                JointState[7] = V[1];//v2
                JointState[8] = V[2];//v3

                estimateT = Closed_Arm_Modle_decoup(JointState);

                ControlT[0] = estimateT[0];
                //ControlT[0] += 0.2 * Math.Sign(JointState_D[3]);
                detlta = 0.15;
                if (ControlT[0] > (referenceT_1[last_time] + detlta)) ControlT[0] = referenceT_1[last_time] + detlta;
                if (ControlT[0] < (referenceT_1[last_time] - detlta)) ControlT[0] = referenceT_1[last_time] - detlta;


                ControlT[1] = estimateT[1];
                //ControlT[1] += 0.2 * Math.Sign(JointState_D[4]);
                detlta = 0.2;
                if (ControlT[1] > (referenceT_2[last_time] + detlta)) ControlT[1] = referenceT_2[last_time] + detlta;
                if (ControlT[1] < (referenceT_2[last_time] - detlta)) ControlT[1] = referenceT_2[last_time] - detlta;


                ControlT[2] = estimateT[2];
                //ControlT[2] += 0.1 * Math.Sign(JointState_D[5]);
                detlta = 0.11;
                if (ControlT[2] > (referenceT_3[last_time] + detlta)) ControlT[2] = referenceT_3[last_time] + detlta;
                if (ControlT[2] < (referenceT_3[last_time] - detlta)) ControlT[2] = referenceT_3[last_time] - detlta;



                //输入控制力矩
                
                
                InputMotorTor((float)ControlT[0], 1);
                InputMotorTor((float)ControlT[1], 2);
                InputMotorTor((float)-ControlT[2], 3);//3号关节装反了要加一个负号


                //更新图
                FreshPlot(angle_d: (float)q1_D[last_time],
                            angle: (float)JointState_B[0],

                           torque: (float)q2_D[last_time],
                         torque_d: (float)JointState_B[1],

                         velocity: (float)q3_D[last_time],
                       velocity_d: (float)JointState_B[2],

                        caltime_d: (float)referenceT_2[last_time],
                          caltime: (float)ControlT[1]);

            }

        }//轨迹跟踪控制运算主程序

        void CANDataUpdate_thech()
        {
            var R = Matrix<double>.Build;
            //double[] JointState_r = new double[6];
            double[] JointState_D = new double[9];
            double[] JointState = new double[9];
            double[] estimateT = new double[3];
            //double[] refer_tao = new double[3];
           // double[] ControlT = new double[3];          //控制输入
            double[] s = new double[3];
            double[] e = new double[3];
            double[] de = new double[3];
            double[] X = new double[3];
            double[] k = new double[3];
            double[] X_desired = new double[3];
            double[] dX_desired = new double[3];
            double[] F_K1 = new double[3];
            double[] F_K2 = new double[3];
            double[] V = new double[3];
            double[] Yd = new double[3];
            var z00 = CreatMatrix(0, 0, 1);
            var p0 = CreatMatrix(0, 0, 0);
            double q1,q2,q3,a1,a2,a3,M,C,K,Fx,Fy,Fz;

            //double q2, dq2, ddq2, m2, a2, I2;
            //double q3, dq3, ddq3, m3, a3, I3;
            a2 = 0.12;a3 = 0.12;

            if (!vcom.IsOpen)
            {
                buttonFresh.Enabled = true;
                openButton.Text = @"打开";
                openButton.BackColor = SystemColors.Control; //恢复颜色
                openButton.UseVisualStyleBackColor = true;
                comFlag = false;
                //清空canFIFO
                if (canRxFIFO.DataCount > 0)
                {
                    canRxFIFO.Clear(canRxFIFO.DataCount);
                }
            }
            if (isButtonClick)//只有点击开始运算时的时候才显示isButtonClick
            {

                //步数自增
                last_time++;

                q1_D[last_time] = JointState_B[0];
                q2_D[last_time] = JointState_B[1];
                q3_D[last_time] = JointState_B[2];
                dq1_D[last_time] = JointState_B[3];
                dq2_D[last_time] = JointState_B[4];
                dq3_D[last_time] = JointState_B[5];
                ddq1_D[last_time] = 0;
                ddq2_D[last_time] = 0;
                ddq3_D[last_time] = 0;


                //阻抗方程
                //系数越大效果越好
                M = 0;//1;
                C = 0;//3;
                K = 0;//70;


                q1 = JointState_B[0];
                q2 = JointState_B[1];
                q3 = JointState_B[2];
                //变换矩阵
                //似乎默认都是列向量
                double[,] R01Array = { { Math.Cos(q1), 0, Math.Sin(q1) }, { Math.Sin(q1), 0, -Math.Cos(q1) }, { 0, 1, 0 } };
                double[,] R12Array = { { Math.Cos(q2), -Math.Sin(q2), 0 }, { Math.Sin(q2), Math.Cos(q2), 0 }, { 0, 0, 1 } };
                double[,] R23Array = { { Math.Cos(q3), -Math.Sin(q3), 0 }, { Math.Sin(q3), Math.Cos(q3), 0 }, { 0, 0, 1 } };
                double[,] P01Array = { { 0 }, { 0 }, { 0 } };
                double[,] P12Array = { { a2 * Math.Cos(q2) }, { a2 * Math.Sin(q2) }, { 0 } };
                double[,] P23Array = { { a3 * Math.Cos(q3) }, { a3 * Math.Sin(q3) }, { 0 } };
                var R01 = R.DenseOfArray(R01Array);//Console.WriteLine($"R01 = {R01}--第{i}次");
                var R12 = R.DenseOfArray(R12Array);//Console.WriteLine($"R12 = {R12}--第{i}次");
                var R23 = R.DenseOfArray(R23Array);
                var R02 = R01 * R12;
                var R03 = R01 * R12 * R23;
                var P01 = R.DenseOfArray(P01Array);//Console.WriteLine($"P01 = {P01}--第{i}次");
                var P12 = R.DenseOfArray(P12Array);//Console.WriteLine($"P12 = {P12}--第{i}次");
                var P23 = R.DenseOfArray(P23Array);
                /*计算雅可比矩阵*/
                var z01 = R01 * z00;
                var z02 = R01 * R12 * z00;
                var p1 = P01;
                var p2 = R01 * P12 + P01;
                var p3 = R01 * R12 * P23 + p2; //Console.WriteLine($"p3 = {p3}--第{i}次");
                var j_b1 = MyCross(z00, (p3 - p0)); //Console.WriteLine($"P12 = {j_b1}--第{i}次");
                var j_b2 = MyCross(z01, (p3 - p1));
                var j_b3 = MyCross(z02, (p3 - p2));
                var jacobian = j_b1.Append(j_b2);
                jacobian = jacobian.Append(j_b3);//Console.WriteLine($"P12 = {jacobian}--第{i}次");
                double[,] QdArry = { { JointState_B[3] }, { JointState_B[4] }, { JointState_B[5] } };
                var dQ = R.DenseOfArray(QdArry);//关节速度列向量

                var dX = jacobian * dQ;//末端速度向量
                X[0] = p3[0, 0];//x
                X[1] = p3[1, 0];//y
                X[2] = p3[2, 0];//z
                //阻抗的初始点
                X_desired[0] = 0.18;
                X_desired[1] = 0;
                X_desired[2] = 0.01;

                dX_desired[0] = 0;
                dX_desired[1] = 0;
                dX_desired[2] = 0;

                e[0] = X_desired[0] - X[0];
                e[1] = X_desired[1] - X[1];
                e[2] = X_desired[2] - X[2];

                de[0] = dX_desired[0] - dX[0, 0];
                de[1] = dX_desired[1] - dX[1, 0];
                de[2] = dX_desired[2] - dX[2, 0];

                Fx = C * de[0] + K * e[0]; 
                Fy = C * de[1] + K * e[1];
                Fz = C * de[2] + K * e[2];
                double[,] FArry = { { Fx }, { Fy }, { Fz } };
                var F = R.DenseOfArray(FArry);//末端力向量
                var Tao = jacobian.Transpose() * F;//逆向静力学



                JointState[0] = JointState_B[0];//q1_b
                JointState[1] = JointState_B[1];//q2_b
                JointState[2] = JointState_B[2];//q3_b
                JointState[3] = JointState_B[3];//qd1_b
                JointState[4] = JointState_B[4];//qd2_b
                JointState[5] = JointState_B[5];//qd3_b
                JointState[6] = 0;//v1
                JointState[7] = 0;//v2
                JointState[8] = 0;//v3
   
                estimateT = Closed_Arm_Modle_decoup(JointState);

                estimateT[0] += Tao[0, 0];
                estimateT[1] += Tao[1, 0];
                estimateT[2] += Tao[2, 0];

                //输入控制力矩
                InputMotorTor((float)-estimateT[2], 3);//3号关节装反了要加一个负号
                InputMotorTor((float)estimateT[1], 2);
                InputMotorTor((float)estimateT[0], 1);


                //更新图
                FreshPlot(angle_d: (float)X_desired[0],
                          angle: (float)X[0],

                          torque: (float)Tao[1, 0],
                          torque_d: (float)0,

                          velocity: (float)Fx,
                          velocity_d: (float)0,

                          caltime_d: (float)referenceT_2[last_time],
                          caltime: (float)ControlT[1]);

            }

        }//拖拽示教用的函数（含阻抗）

        void CANDataUpdate_impedance()
        {
            var R = Matrix<double>.Build;
            double[] JointState_r = new double[6];
            double[] JointState_D = new double[9];
            double[] JointState = new double[9];
            double[] estimateT = new double[3];
            double[] refer_tao = new double[3];
            double[] ControlT = new double[3];          //控制输入
            double[] s = new double[3];
            double[] e = new double[3];
            double[] de = new double[3];
            double[] X = new double[3];
            double[] k = new double[3];
            double[] X_desired = new double[3];
            double[] dX_desired = new double[3];
            double[] F_K1 = new double[3];
            double[] F_K2 = new double[3];
            //double[] Kp = new double[3];
            //double[] Kd = new double[3];
            double[] V = new double[3];
            double[] Yd = new double[3];
            //double[] S_state = new double[3];
            var z00 = CreatMatrix(0, 0, 1);
            var p0 = CreatMatrix(0, 0, 0);
            double q1, q2, q3, a1, a2, a3, M, C, K, Fx, Fy, Fz;

            double x, dx, ddx, y, dy, ddy, z, dz, ddz;  //期望轨迹变量
            double q21, q22, qd1, qdd1, qd2, qdd2, qd3, qdd3;   //关节状态


            double FuY, r, a, b, c, elbow, m1, m2, m3, l1, l2, l3, detlta;
            //double q2, dq2, ddq2, m2, a2, I2;
            //double q3, dq3, ddq3, m3, a3, I3;
            a2 = 0.12; a3 = 0.12;

            if (!vcom.IsOpen)
            {
                buttonFresh.Enabled = true;
                openButton.Text = @"打开";
                openButton.BackColor = SystemColors.Control; //恢复颜色
                openButton.UseVisualStyleBackColor = true;
                comFlag = false;
                //清空canFIFO
                if (canRxFIFO.DataCount > 0)
                {
                    canRxFIFO.Clear(canRxFIFO.DataCount);
                }
            }
            if (isButtonClick)//只有点击开始运算时的时候才显示isButtonClick
            {

                //步数自增
                last_time++;

                q1 = JointState_B[0];
                q2 = JointState_B[1];
                q3 = JointState_B[2];

                //变换矩阵
                //似乎默认都是列向量
                double[,] R01Array = { { Math.Cos(q1), 0, Math.Sin(q1) }, { Math.Sin(q1), 0, -Math.Cos(q1) }, { 0, 1, 0 } };
                double[,] R12Array = { { Math.Cos(q2), -Math.Sin(q2), 0 }, { Math.Sin(q2), Math.Cos(q2), 0 }, { 0, 0, 1 } };
                double[,] R23Array = { { Math.Cos(q3), -Math.Sin(q3), 0 }, { Math.Sin(q3), Math.Cos(q3), 0 }, { 0, 0, 1 } };
                double[,] P01Array = { { 0 }, { 0 }, { 0 } };
                double[,] P12Array = { { a2 * Math.Cos(q2) }, { a2 * Math.Sin(q2) }, { 0 } };
                double[,] P23Array = { { a3 * Math.Cos(q3) }, { a3 * Math.Sin(q3) }, { 0 } };
                var R01 = R.DenseOfArray(R01Array);//Console.WriteLine($"R01 = {R01}--第{i}次");
                var R12 = R.DenseOfArray(R12Array);//Console.WriteLine($"R12 = {R12}--第{i}次");
                var R23 = R.DenseOfArray(R23Array);
                var R02 = R01 * R12;
                var R03 = R01 * R12 * R23;
                var P01 = R.DenseOfArray(P01Array);//Console.WriteLine($"P01 = {P01}--第{i}次");
                var P12 = R.DenseOfArray(P12Array);//Console.WriteLine($"P12 = {P12}--第{i}次");
                var P23 = R.DenseOfArray(P23Array);
                /*计算雅可比矩阵*/
                var z01 = R01 * z00;
                var z02 = R01 * R12 * z00;
                var p1 = P01;
                var p2 = R01 * P12 + P01;
                var p3 = R01 * R12 * P23 + p2; //Console.WriteLine($"p3 = {p3}--第{i}次");
                var j_b1 = MyCross(z00, (p3 - p0)); //Console.WriteLine($"P12 = {j_b1}--第{i}次");
                var j_b2 = MyCross(z01, (p3 - p1));
                var j_b3 = MyCross(z02, (p3 - p2));
                var jacobian = j_b1.Append(j_b2);
                jacobian = jacobian.Append(j_b3);//Console.WriteLine($"P12 = {jacobian}--第{i}次");
                double[,] QdArry = { { JointState_B[3] }, { JointState_B[4] }, { JointState_B[5] } };
                var dQ = R.DenseOfArray(QdArry);//关节速度列向量


                /*
                 * 末端位置是：p3
                 * 末端速度：jacobian.Qd==>Xd = jacobian*Qd
                 * 末端加速度：无法计算
                 */
                var dX = jacobian * dQ;//末端速度向量
                X[0] = p3[0, 0];//x
                X[1] = p3[1, 0];//y
                X[2] = p3[2, 0];//z

                X_desired[0] = 0.18;
                X_desired[1] = 0;
                X_desired[2] = 0.01;

                dX_desired[0] = 0;
                dX_desired[1] = 0;
                dX_desired[2] = 0;

                e[0] = X_desired[0] - X[0];
                e[1] = X_desired[1] - X[1];
                e[2] = X_desired[2] - X[2];

                de[0] = dX_desired[0] - dX[0, 0];
                de[1] = dX_desired[1] - dX[1, 0];
                de[2] = dX_desired[2] - dX[2, 0];




                //阻抗方程
                //系数越大效果越好
                //太小的话会不正常工作
                M = 1;
                C = 3;
                K = 70;

                Fx = C * de[0] + K * e[0]; //滑模不控制
                //Fx = 0;
                // Fy = C * de[1] + K * e[1];
               //Fz = C * de[2] + K * e[2];
                Fy = 0;//滑模控制这个轴向位置
                Fz = 0;//滑模控制这个轴向位置


                //实时更期望末端位置
                x = 0.18;
                y = 0;
                z = 0.01;

                if (z == 0)
                {
                    r = x * x + y * y + z * z;
                    q2 = Math.Acos(Math.Sqrt(r) / (a2 + a3));
                    q3 = -q2 - Math.Acos(Math.Sqrt(r) / (a2 + a3));
                    q1 = Math.Atan2(y, x);
                }
                else
                {
                    /*逆运动学*/
                    elbow = -1; FuY = 1;
                    if (y < 0) { FuY = -1; } else { FuY = 1; }
                    r = x * x + y * y + z * z;
                    q3 = elbow * Math.Acos((r - (a2 * a2 + a3 * a3)) / (2 * a2 * a3)); //Console.WriteLine($"q3 = {q3}--第{i}次");


                    q1 = Math.Atan2(y, x); //Console.WriteLine($"q1 = {q1}--第{i}次");

                    b = -a3 * Math.Cos(q1) * Math.Sin(q3); a = a2 * Math.Cos(q1) + a3 * Math.Cos(q1) * Math.Cos(q3); c = x;
                    if (a == 0) { q21 = Constants.Pi / 2; } else { q21 = Math.Atan2(b, a); }
                    if (c == 0) { q22 = FuY * Constants.Pi / 2; } else { q22 = Math.Atan2(Math.Sqrt(a * a + b * b - c * c), c); }
                    q2 = q21 + q22; //Console.WriteLine($"q2 = {q2}--第{i}次");

                }

                //更新期望关节位置，因为阻抗位置会更新
                JointState_D[0] = q1;
                JointState_D[1] = q2;
                JointState_D[2] = q3;

                refer_tao = Closed_Arm_Modle_thetch(JointState);

                double[,] FArry = { { Fx }, { Fy }, { Fz } };
                var F = R.DenseOfArray(FArry);//末端力向量
                var Tao = jacobian.Transpose() * F;//逆向静力学


                /* 误 差 */
                e[0] = JointState_D[0] - JointState_B[0];
                de[0] = 0 - JointState_B[3];

                e[1] = JointState_D[1] - JointState_B[1];
                de[1] = 0 - JointState_B[4];

                e[2] = JointState_D[2] - JointState_B[2];
                de[2] = 0 - JointState_B[5];


                k[0] = 40 + B2;

                k[1] = 29 + C1;

                k[2] = 80 + C2;


                /*切 换 函 数*/
                s[0] = de[0] + k[0] * e[0];
                s[1] = de[1] + k[1] * e[1];
                s[2] = de[2] + k[2] * e[2];

                Yd[0] = (54 + A1) * Math.Tanh(s[0] / 3.0);
                Yd[1] = (50 + A2) * Math.Tanh(s[1]);
                Yd[2] = (100 + B1) * Math.Sign(s[2]);
                V[0] = 0 + k[0] * de[0] + Yd[0];
                V[1] = 0 + k[1] * de[1] + Yd[1];
                V[2] = 0 + k[2] * de[2] + Yd[2];
                /********************************************/

                JointState[0] = JointState_B[0];//q1_b
                JointState[1] = JointState_B[1];//q2_b
                JointState[2] = JointState_B[2];//q3_b
                JointState[3] = JointState_B[3];//qd1_b
                JointState[4] = JointState_B[4];//qd2_b
                JointState[5] = JointState_B[5];//qd3_b
                JointState[6] = V[0];//v1
                JointState[7] = V[1];//v2
                JointState[8] = V[2];//v3


                //estimateT = Closed_Arm_Modle_thetch(JointState);
                estimateT = Closed_Arm_Modle_decoup(JointState);

                ControlT[0] = estimateT[0];
                detlta = 0.15;
                if (ControlT[0] > (refer_tao[0] + detlta)) ControlT[0] = refer_tao[0] + detlta;
                if (ControlT[0] < (refer_tao[0] - detlta)) ControlT[0] = refer_tao[0] - detlta;


                ControlT[1] = estimateT[1];
                detlta = 0.25;
                if (ControlT[1] > (refer_tao[1] + detlta)) ControlT[1] = refer_tao[1] + detlta;
                if (ControlT[1] < (refer_tao[1] - detlta)) ControlT[1] = refer_tao[1] - detlta;


                ControlT[2] = estimateT[2];
                detlta = 0.15;
                if (ControlT[2] > (refer_tao[2] + detlta)) ControlT[2] = refer_tao[2] + detlta;
                if (ControlT[2] < (refer_tao[2] - detlta)) ControlT[2] = refer_tao[2] - detlta;


                ControlT[0] += Tao[0, 0];
                ControlT[1] += Tao[1, 0];
                ControlT[2] += Tao[2, 0];

                //输入控制力矩
                InputMotorTor((float)-ControlT[2], 3);//3号关节装反了要加一个负号
                InputMotorTor((float)ControlT[1], 2);
                InputMotorTor((float)ControlT[0], 1);


                //更新图
                FreshPlot(angle_d: (float)X_desired[0],
                          angle: (float)X[0],

                          torque: (float)Tao[1, 0],
                          torque_d: (float)0,

                          velocity: (float)Fx,
                          velocity_d: (float)0,

                          caltime_d: (float)referenceT_2[last_time],
                          caltime: (float)ControlT[1]);

            }

        }//定点阻抗控制，有外力的情况下会很稳定


        private void send_Click(object sender, EventArgs e)
        {

            Pos = 0.0f;
            EnableMotor();
            if (vcom.IsOpen)
            {
                byte[] buf = new byte[] { 0x55, 0x05, 0x00, 0xAA, 0x55 };  // 设置CAN ID  0x05 CMD  参数 baudrateIndex
            //buf[2] = (byte)canBaudrateComBox.Items.IndexOf(canBaudrateComBox.Text);
            vcom.Write(buf, 0, 5);
            Thread.Sleep(200);  //睡眠 等待下位机反馈

            }
            else
            {
                MessageBox.Show("设备未连接！", "Error");
            }
        }
        private void Disenble_Click(object sender, EventArgs e)
        {
            DisEnableMotor();
            isButtonClick = false;
        }
        private void Turn_Click(object sender, EventArgs e)
        {

            InputMotorPosAndVel(1.0f, 0.3f);
            CAN_richBox.Text += Environment.NewLine;
            CAN_richBox.Text += "PosInTurn_Click:" + Pos;
            CAN_richBox.Text += "| ";
        }
        //初始化
        private void InverTurn_Click(object sender, EventArgs e)
        {
                /**
                  * 参 考 模 型
                  * 预先计算，在线调取结果
                  * **/
            if(is_shijiao == 0)
            {
                var R = Matrix<double>.Build;
                double CurrentTime = 0.0;
                double x, dx, ddx, y, dy, ddy, z, dz, ddz;  //期望轨迹变量
                double q1, q21, q22, qd1, qdd1, q2, qd2, qdd2, q3, qd3, qdd3;   //关节状态
                double FuY, r, a, b, c, a1, a2, a3, elbow, m1, m2, m3, l1, l2, l3;
                double rate, Amplitude, x_0, y_0, z_0;


                //机器人模型  
                //质量：kg；长度：m
                //matrix类型
                //杆长、质量、p1sta、Ic , rc , 都要注意
                a1 = 0; a2 = 0.12; a3 = 0.12;

                m1 = 0; m2 = 0.45; m3 = 0.004;


                var z00 = CreatMatrix(0, 0, 1);
                var p0 = CreatMatrix(0, 0, 0);
                var p1sta = CreatMatrix(a1, 0, 0);
                var p2sta = CreatMatrix(a2, 0, 0);
                var p3sta = CreatMatrix(a3, 0, 0);
                var omega00 = CreatMatrix(0, 0, 0);
                var v00 = CreatMatrix(0, 0, 0);
                var epsilon00 = CreatMatrix(0, 0, 0);
                var a00 = CreatMatrix(0, 0, 0);
                var rc1 = CreatMatrix(0, 0, 0);
                var rc2 = CreatMatrix(0, 0, 0);
                var rc3 = CreatMatrix(0, 0, 0);
                var g = CreatMatrix(0, 0, -9.81);
                double[,] Ic1Array = { { 0.0015, 0, 0 }, { 0, 0.0015, 0 }, { 0, 0, 0.0015 } };//常数向量
                var Ic1 = R.DenseOfArray(Ic1Array);
                double[,] Ic2Array = { { 0, 0, 0 }, { 0, 0.00437, 0 }, { 0, 0, 0.00437 } };//常数向量
                var Ic2 = R.DenseOfArray(Ic2Array);
                double[,] Ic3Array = { { 0, 0, 0 }, { 0, 0.00108, 0 }, { 0, 0, 0.00107 } };//常数向量
                var Ic3 = R.DenseOfArray(Ic3Array);
                var f44 = CreatMatrix(0, 0, 0);
                var n44 = CreatMatrix(0, 0, 0);

                /*定时 6 秒，间隔 1 ms，6000步*/
                for (int i = 0; i <= 6000; i++)
                {

                    CurrentTime = i / 1000.0;

                    rate = 3;
                    Amplitude = 0.015;
                    x_0 = 0.19;
                    y_0 = 0;
                    z_0 = 0.00;

                    /*设置工作空间的期望轨迹*/
                    //竖线
                    //x = x_0 + Amplitude * Math.Cos(rate * CurrentTime);
                    //dx = -Amplitude * rate * Math.Sin(rate * CurrentTime);
                    //ddx = -Amplitude * rate * rate * Math.Cos(rate * CurrentTime);
                    //y = y_0; dy = 0; ddy = 0;
                    //z = z_0; dz = 0; ddz = 0;


                    //横线
                    //Amplitude = 0.04;
                    //y = y_0 + Amplitude * Math.Cos(rate * CurrentTime);
                    //dy = -Amplitude * rate * Math.Sin(rate * CurrentTime);
                    //ddy = -Amplitude * rate * rate * Math.Cos(rate * CurrentTime);
                    //x = x_0; dx = 0; ddx = 0;
                    //z = z_0; dz = 0; ddz = 0;

                    //螺旋线
                    Amplitude = 0.025;
                    x = x_0 + Amplitude * Math.Cos(rate * CurrentTime);
                    dx = -Amplitude * rate * Math.Sin(rate * CurrentTime);
                    ddx = -Amplitude * rate * rate * Math.Cos(rate * CurrentTime);
                    y = y_0 + Amplitude * Math.Sin(rate * CurrentTime);
                    dy = Amplitude * rate * Math.Cos(rate * CurrentTime);
                    ddy = -Amplitude * rate * rate * Math.Sin(rate * CurrentTime);
                    z = z_0 + 0.003 * CurrentTime; dz = 0.01; ddz = 0;



                    if (z == 0)
                    {
                        r = x * x + y * y + z * z;
                        q2 = Math.Acos(Math.Sqrt(r) / (a2 + a3));
                        q3 = -q2 - Math.Acos(Math.Sqrt(r) / (a2 + a3));
                        q1 = Math.Atan2(y, x);
                    }
                    else
                    {
                        /*逆运动学*/
                        elbow = -1; FuY = 1;
                        if (y < 0) { FuY = -1; } else { FuY = 1; }
                        r = x * x + y * y + z * z;
                        q3 = elbow * Math.Acos((r - (a2 * a2 + a3 * a3)) / (2 * a2 * a3)); //Console.WriteLine($"q3 = {q3}--第{i}次");


                        q1 = Math.Atan2(y, x); //Console.WriteLine($"q1 = {q1}--第{i}次");

                        b = -a3 * Math.Cos(q1) * Math.Sin(q3); a = a2 * Math.Cos(q1) + a3 * Math.Cos(q1) * Math.Cos(q3); c = x;
                        if (a == 0) { q21 = Constants.Pi / 2; } else { q21 = Math.Atan2(b, a); }
                        if (c == 0) { q22 = FuY * Constants.Pi / 2; } else { q22 = Math.Atan2(Math.Sqrt(a * a + b * b - c * c), c); }
                        q2 = q21 + q22; //Console.WriteLine($"q2 = {q2}--第{i}次");

                    }





                    //变换矩阵
                    //似乎默认都是列向量
                    double[,] R01Array = { { Math.Cos(q1), 0, Math.Sin(q1) }, { Math.Sin(q1), 0, -Math.Cos(q1) }, { 0, 1, 0 } };
                    double[,] R12Array = { { Math.Cos(q2), -Math.Sin(q2), 0 }, { Math.Sin(q2), Math.Cos(q2), 0 }, { 0, 0, 1 } };
                    double[,] R23Array = { { Math.Cos(q3), -Math.Sin(q3), 0 }, { Math.Sin(q3), Math.Cos(q3), 0 }, { 0, 0, 1 } };
                    double[,] P01Array = { { 0 }, { 0 }, { 0 } };
                    double[,] P12Array = { { a2 * Math.Cos(q2) }, { a2 * Math.Sin(q2) }, { 0 } };
                    double[,] P23Array = { { a3 * Math.Cos(q3) }, { a3 * Math.Sin(q3) }, { 0 } };
                    var R01 = R.DenseOfArray(R01Array);//Console.WriteLine($"R01 = {R01}--第{i}次");
                    var R12 = R.DenseOfArray(R12Array);//Console.WriteLine($"R12 = {R12}--第{i}次");
                    var R23 = R.DenseOfArray(R23Array);
                    var R02 = R01 * R12;
                    var R03 = R01 * R12 * R23;
                    var P01 = R.DenseOfArray(P01Array);//Console.WriteLine($"P01 = {P01}--第{i}次");
                    var P12 = R.DenseOfArray(P12Array);//Console.WriteLine($"P12 = {P12}--第{i}次");
                    var P23 = R.DenseOfArray(P23Array);


                    /*计算雅可比矩阵*/
                    var z01 = R01 * z00;
                    var z02 = R01 * R12 * z00;
                    var p1 = P01;
                    var p2 = R01 * P12 + P01;
                    var p3 = R01 * R12 * P23 + p2; //Console.WriteLine($"p3 = {p3}--第{i}次");
                    var j_b1 = MyCross(z00, (p3 - p0)); //Console.WriteLine($"P12 = {j_b1}--第{i}次");
                    var j_b2 = MyCross(z01, (p3 - p1));
                    var j_b3 = MyCross(z02, (p3 - p2));
                    var jacobian = j_b1.Append(j_b2);
                    jacobian = jacobian.Append(j_b3);//Console.WriteLine($"P12 = {jacobian}--第{i}次");


                    /*逆速度计算*/
                    //统一转换为矩阵进行运算
                    //Numerator:分子。Denominator：分母
                    var end_vol = CreatMatrix(dx, dy, dz);//Console.WriteLine($"end_vol = {end_vol}--第{i}次");
                    var j11 = jacobian.Column(0); var j12 = jacobian.Column(1); var j13 = jacobian.Column(2);
                    var j21 = jacobian.Column(0) * jacobian.Column(0);
                    var j22 = jacobian.Column(0) * jacobian.Column(1);
                    var j23 = jacobian.Column(0) * jacobian.Column(2);//
                    var j31 = jacobian.Column(1) * jacobian.Column(0);
                    var j32 = jacobian.Column(1) * jacobian.Column(1);
                    var j33 = jacobian.Column(1) * jacobian.Column(2);
                    var n1 = j11;
                    var n2 = j11 * j22 - j12 * j21;
                    var n3 = -j13 * j22 * j31 + j12 * j23 * j31 + j13 * j21 * j32 - j11 * j23 * j32 - j12 * j21 * j33 + j11 * j22 * j33;
                    var MatrixQd3Numerator = (end_vol.Transpose() * n3.ToColumnMatrix());  //Console.WriteLine($"MatrixQd3Numerator = {MatrixQd3Numerator}--第{i}次");
                    var MatrixQd3Denominator = (jacobian.Column(2).ToRowMatrix() * n3.ToColumnMatrix());   //Console.WriteLine($"MatrixQd2Denominator = {MatrixQd2Denominator}--第{i}次");
                    qd3 = MatrixQd3Numerator[0, 0] / MatrixQd3Denominator[0, 0]; //Console.WriteLine($"qd3 = {qd3}--第{i}次");                                            

                    var MatrixQd2Numerator = n2.ToRowMatrix() * (end_vol - (qd3 * jacobian.Column(2).ToColumnMatrix())); //Console.WriteLine($"MatrixQd1Numerator = {MatrixQd1Numerator}--第{i}次");
                    var MatrixQd2Denominator = (jacobian.Column(1).ToRowMatrix() * n2.ToColumnMatrix());//Console.WriteLine($"matrixqd1Denominator = {matrixqd1Denominator}--第{i}次");
                    qd2 = MatrixQd2Numerator[0, 0] / MatrixQd2Denominator[0, 0]; //Console.WriteLine($"qd2 = {qd2}--第{i}次");

                    var MatrixQd1Numerator = n1.ToRowMatrix() * (end_vol - (qd2 * jacobian.Column(1).ToColumnMatrix() + qd3 * jacobian.Column(2).ToColumnMatrix())); //Console.WriteLine($"MatrixQd1Numerator = {MatrixQd1Numerator}--第{i}次");
                    var MatrixQd1Denominator = (jacobian.Column(0).ToRowMatrix() * n1.ToColumnMatrix()); //Console.WriteLine($"matrixqd1Denominator = {matrixqd1Denominator}--第{i}次");
                    qd1 = MatrixQd1Numerator[0, 0] / MatrixQd1Denominator[0, 0]; //Console.WriteLine($"qd1 = {qd1}--第{i}次");

                    /*正向速度计算*/
                    var omega11 = R01.Transpose() * (omega00 + z00 * qd1); //Console.WriteLine($"omega11 = {omega11}--第{i}次");
                    var omega22 = R12.Transpose() * (omega11 + z00 * qd2); //Console.WriteLine($"omega22 = {omega22}--第{i}次");
                    var omega33 = R23.Transpose() * (omega22 + z00 * qd3); //Console.WriteLine($"omega22 = {omega22}--第{i}次");
                    var v11 = R01.Transpose() * v00 + MyCross(omega11, p1sta); //Console.WriteLine($"v11 = {v11}--第{i}次");
                    var v22 = R12.Transpose() * v11 + MyCross(omega22, p2sta); //Console.WriteLine($"v22 = {v22}--第{i}次\n");
                    var v33 = R23.Transpose() * v22 + MyCross(omega33, p3sta); //Console.WriteLine($"v33 = {v33}--第{i}次\n");
                    var v03 = R01 * R12 * R23 * v33; //Console.WriteLine($"v03 = {v03}--第{i}次\n");


                    /*雅可比导数的计算*/
                    var omega01 = R01 * omega11;
                    var omega02 = R01 * R12 * omega22;
                    var v01 = R01 * v11; var v02 = R01 * R12 * v22;
                    var db11 = MyCross(omega00, z00); var db12 = p3 - p0;
                    var db1 = MyCross(db11, db12) + MyCross(z00, (v03 - v00)); //Console.WriteLine($"db1 = {db1}--第{i}次");
                    var db21 = MyCross(omega01, z01); var db22 = p3 - p1;
                    var db2 = MyCross(db21, db22) + MyCross(z01, (v03 - v01)); //Console.WriteLine($"db2 = {db2}--第{i}次");
                    var db31 = MyCross(omega02, z02); var db32 = p3 - p2;
                    var db3 = MyCross(db31, db32) + MyCross(z02, (v03 - v02)); //Console.WriteLine($"db3 = {db3}--第{i}次");
                    var dj = db1.Append(db2); //Console.WriteLine($"dj = {dj}--第{i}次");
                    dj = dj.Append(db3); //Console.WriteLine($"dj = {dj}--第{i}次");


                    /*逆加速度计算*/
                    var xdds = CreatMatrix(ddx, ddy, ddz) - dj * CreatMatrix(qd1, qd2, qd3); //Console.WriteLine($"xdds = {xdds}--第{i}次");
                    var MatrixQdd3Numerator = (xdds.Transpose() * n3.ToColumnMatrix());                 //Console.WriteLine($"MatrixQdd2Numerator = {MatrixQdd2Numerator}--第{i}次");
                    var MatrixQdd3Denominator = (jacobian.Column(2).ToRowMatrix() * n3.ToColumnMatrix());   //Console.WriteLine($"MatrixQdd2Denominator = {MatrixQdd2Denominator}--第{i}次");
                    qdd3 = MatrixQdd3Numerator[0, 0] / MatrixQdd3Denominator[0, 0]; //Console.WriteLine($"qdd3 = {qdd3}--第{i}次");                

                    var MatrixQdd2Numerator = n2.ToRowMatrix() * (xdds - (qdd3 * jacobian.Column(2).ToColumnMatrix())); //Console.WriteLine($"MatrixQd1Numerator = {MatrixQd1Numerator}--第{i}次");
                    var MatrixQdd2Denominator = (jacobian.Column(1).ToRowMatrix() * n2.ToColumnMatrix());//Console.WriteLine($"matrixqd1Denominator = {matrixqd1Denominator}--第{i}次");
                    qdd2 = MatrixQdd2Numerator[0, 0] / MatrixQdd2Denominator[0, 0]; //Console.WriteLine($"qdd2 = {qdd2}--第{i}次");

                    var MatrixQdd1Numerator = n1.ToRowMatrix() * (xdds - (qdd2 * jacobian.Column(1).ToColumnMatrix() + qdd3 * jacobian.Column(2).ToColumnMatrix())); //Console.WriteLine($"MatrixQd1Numerator = {MatrixQd1Numerator}--第{i}次");
                    var MatrixQdd1Denominator = (jacobian.Column(0).ToRowMatrix() * n1.ToColumnMatrix()); //Console.WriteLine($"matrixqd1Denominator = {matrixqd1Denominator}--第{i}次");
                    qdd1 = MatrixQdd1Numerator[0, 0] / MatrixQdd1Denominator[0, 0]; //Console.WriteLine($"qdd1 = {qdd1}--第{i}次");

                    //保存期望关节状态
                    q1_D[i] = q1; q2_D[i] = q2; q3_D[i] = q3;

                    dq1_D[i] = qd1; dq2_D[i] = qd2; dq3_D[i] = qd3;

                    ddq1_D[i] = qdd1; ddq2_D[i] = qdd2; ddq3_D[i] = qdd3;

                    /*正加速计算*/
                    var epsilon11 = R01.Transpose() * (epsilon00 + MyCross(omega00, z00 * qd1) + z00 * qdd1); //Console.WriteLine($"epsilon11 = {epsilon11}--第{i}次");
                    var epsilon22 = R12.Transpose() * (epsilon11 + MyCross(omega11, z00 * qd2) + z00 * qdd2); //Console.WriteLine($"epsilon22 = {epsilon22}--第{i}次");
                    var epsilon33 = R23.Transpose() * (epsilon22 + MyCross(omega22, z00 * qd3) + z00 * qdd3); //Console.WriteLine($"epsilon33 = {epsilon33}--第{i}次");
                    var a11 = R01.Transpose() * a00 + MyCross(epsilon11, p1sta) + MyCross(omega11, MyCross(omega11, p1sta)); //Console.WriteLine($"a11 = {a11}--第{i}次");
                    var a22 = R12.Transpose() * a11 + MyCross(epsilon22, p2sta) + MyCross(omega22, MyCross(omega22, p2sta)); //Console.WriteLine($"a22 = {a22}--第{i}次");
                    var a33 = R23.Transpose() * a22 + MyCross(epsilon33, p3sta) + MyCross(omega33, MyCross(omega33, p3sta)); //Console.WriteLine($"a22 = {a22}--第{i}次");
                    var ac11 = a11 + MyCross(epsilon11, rc1) + MyCross(omega11, MyCross(omega11, rc1)); //Console.WriteLine($"ac11 = {ac11}--第{i}次");
                    var ac22 = a22 + MyCross(epsilon22, rc2) + MyCross(omega22, MyCross(omega22, rc2)); //Console.WriteLine($"ac22 = {ac22}--第{i}次");
                    var ac33 = a33 + MyCross(epsilon33, rc3) + MyCross(omega33, MyCross(omega33, rc3)); //Console.WriteLine($"ac33 = {ac33}--第{i}次");


                    /*计算力*/
                    var g1 = R01.Transpose() * g;
                    var g2 = R02.Transpose() * g;
                    var g3 = R03.Transpose() * g; //Console.WriteLine($"f33 = {g3}--第{i}次");
                    var f33 = m3 * (ac33 - g3) + f44; //Console.WriteLine($"f11 = {f33}--第{i}次");
                    var f22 = m2 * (ac22 - g2) + R23 * f33;
                    var f11 = m1 * (ac11 - g1) + R12 * f22; //Console.WriteLine($"f11 = {f11}--第{i}次");
                    var n33 = Ic3 * epsilon33 + MyCross(omega33, (Ic3 * omega33)) + n44 + MyCross(p3sta, f33) + MyCross(rc3, (m3 * (ac33 - g3))); //Console.WriteLine($"n33 = {n33}--第{i}次");
                    var n22 = Ic2 * epsilon22 + MyCross(omega22, (Ic2 * omega22)) + R23 * n33 + MyCross(p2sta, f22) + MyCross(rc2, (m2 * (ac22 - g2)));  //Console.WriteLine($"n22 = {n22}--第{i}次");
                    var n11 = Ic1 * epsilon11 + MyCross(omega11, (Ic1 * omega11)) + R12 * n22 + MyCross(p1sta, f11) + MyCross(rc1, (m1 * (ac11 - g1))); //Console.WriteLine($"n11 = {n11}--第{i}次");
                    var tao33 = n33.Transpose() * (R23.Transpose() * z00); //Console.WriteLine($"tao33 = {(float)tao33[0, 0]}--第{i}次");
                    var tao22 = n22.Transpose() * (R12.Transpose() * z00); //Console.WriteLine($"tao22 = {(float)tao22[0, 0]}--第{i}次");
                    var tao11 = n11.Transpose() * (R01.Transpose() * z00); //Console.WriteLine($"tao11 = {(float)tao11[0, 0]}--第{i}次");

                    referenceT_1[i] = tao11[0, 0];
                    referenceT_2[i] = tao22[0, 0];
                    referenceT_3[i] = tao33[0, 0];

                }


                CAN_richBox.Clear();
                SetrichTextBox("完成轨迹跟踪初始化（默认跟踪螺旋线）\n");
                SetrichTextBox("可以开始准备跟踪期望曲线了！\n");
            }

            else
            {
                //示教的参数要比轨迹跟踪小很多
                 A1 = -30;//d_1
                 A2 = -18;//d_2
                 B1 = 0;//d_3
                 B2 = -7;//K_1
                 C1 = -4;//K_2
                 C2 = 0;//K_3

                InverTurn_Click_shijiao();
                CAN_richBox.Clear();
                SetrichTextBox("完成拖动轨迹跟踪初始化\n");
                SetrichTextBox("点击拖动完成\n");
                SetrichTextBox("可以开始准备跟踪拖动曲线了！\n");
            }





        }
        //图标题
        private void testbutton1_Click(object sender, EventArgs e)
        {
            CAN_richBox.Clear();
            isButtonClick = true;

            Plotangle = AnglePlot.Plot.AddSignal(angeldate);
            Plotangle1 = AnglePlot.Plot.AddSignal(angeldate_d);
            //AnglePlot.Plot.YAxis.SetBoundary(-1, 1);
            AnglePlot.Plot.Title("关节1轨迹跟踪");
            AnglePlot.Plot.XLabel("TIME");            
            AnglePlot.Plot.AxisAuto();

            PlotTorque = TorquePlot.Plot.AddSignal(torquedata);
            PlotTorque1 = TorquePlot.Plot.AddSignal(torquedata_d);
            TorquePlot.Plot.Title("关节2轨迹跟踪");
            TorquePlot.Plot.XLabel("TIME");
            TorquePlot.Plot.AxisAuto();

            Plotvelocity = VelocityPlot.Plot.AddSignal(veloccitydata);
            Plotvelocity1 = VelocityPlot.Plot.AddSignal(veloccitydata_d);
            VelocityPlot.Plot.Title("关节3轨迹跟踪");
            VelocityPlot.Plot.XLabel("TIME");
            VelocityPlot.Plot.AxisAuto();
            
            PlotTime = TimePlot.Plot.AddSignal(caltimedata);
            PlotTime1 = TimePlot.Plot.AddSignal(caltimedata_d);
            TimePlot.Plot.Title("关节2控制力矩");
            TimePlot.Plot.XLabel("TIME");
            TimePlot.Plot.AxisAuto();

            watch.Start();

            SetrichTextBox(" 正 在 运 行 算 法  \n");
            SetrichTextBox(" ......  \n");

        }

        private void StartTime_Click(object sender, EventArgs e)
        {
  
            if (Duration.Text == "1") Time = 1;
            if (Duration.Text == "3") Time = 3;
            if (Duration.Text == "6") Time = 6;
            if (Duration.Text == "10") Time = 10;




            SetrichTextBox("运行时长为："); SetrichTextBox(Time.ToString());
            SetrichTextBox("\n");

            SetrichTextBox("A1: "); SetrichTextBox(A1.ToString());
            SetrichTextBox("\n");

            SetrichTextBox("A2: "); SetrichTextBox(A2.ToString());
            SetrichTextBox("\n");

            SetrichTextBox("B1: "); SetrichTextBox(B1.ToString());
            SetrichTextBox("\n");

            SetrichTextBox("B2: "); SetrichTextBox(B2.ToString());
            SetrichTextBox("\n");
            SetrichTextBox("C1: "); SetrichTextBox(C1.ToString());
            SetrichTextBox("\n");

            SetrichTextBox("C2: "); SetrichTextBox(C2.ToString());
            SetrichTextBox("\n");



        }

        /************************Ｅ　Ｎ　Ｄ*************************/







        /************************自定义函数*************************/
        /**
         * 封 闭 式 三 自 由 度 动 力 学 模 型
         * 
         * 输入：关节状态 => [q1,q2,q3 , qd1,qd2,qd3 , qdd1,qdd2,qdd3]
         * 输出：关节力矩 => [tao1 , tao2 , tao3]
         * **/
        double[] Closed_Arm_Modle(double[] jointstate)
        {
            double[] Joint_Torque = new double[9];
            double tao3, tao2, tao1, n11x, n11y, n11z, n22y, n22x, n33x, n33y, n33z;
            double q1, qd1, qdd1, q2, qd2, qdd2, q3, qd3, qdd3;   //关节状态
            double a1, a2, a3, m1, m2, m3, Izz1, Izz2, Izz3;
            q1 = jointstate[0]; qd1 = jointstate[3]; qdd1 = jointstate[6];
            q2 = jointstate[1]; qd2 = jointstate[4]; qdd2 = jointstate[7];
            q3 = jointstate[2]; qd3 = jointstate[5]; qdd3 = jointstate[8];

            a1 = 0; a2 = 0.13; a3 = 0.13;
            m1 = 0; m2 = 0.6; m3 = 0.1;
            Izz1 = 0; Izz2 = 0.2; Izz3 = 0.0133;

            tao3 = a3 * m3 * (Math.Cos(q2 + q3) * 9.81 + (0.5 * a2 * Math.Sin(q3) + 0.5 * a3 * Math.Sin(2 * (q2 + q3)) + 0.5 * a2 * Math.Sin(2 * q2 + q3)) * qd1 * qd1 + a2 * Math.Sin(q3) * qd2 * qd2)
                   + qdd3 * (a3 * a3 * m3 + Izz3)
                   + qdd2 * (a3 * m3 * (a2 * Math.Cos(q3) + a3) + Izz3);

            tao2 = tao3 + a2 * Math.Cos(q2) * (9.81 * m2 + 9.81 * m3) + a2 * a3 * m3 * Math.Cos(q3) * qdd3
                   + a2 * (a2 * Math.Sin(2 * q2) * 0.5 * (m2 + m3) + a3 * m3 * (-0.5 * Math.Sin(q3) + 0.5 * Math.Sin(2 * q2 + q3))) * qd1 * qd1
                   + a2 * a3 * m3 * Math.Sin(q3) * (-qd2 * qd2 - 2 * qd2 * qd3 - qd3 * qd3)
                   + qdd2 * (a2 * (a2 * m2 + a2 * m3 + a3 * m3 * Math.Cos(q3)) + Izz2);

            n22x = -a3 * m3 * Math.Sin(q3) * qdd1 * (a2 * Math.Cos(q2) + a3 * Math.Cos(q2 + q3)) +
                qd1 * (qd3 * (2 * a3 * a3 * m3 * Math.Sin(q3) * Math.Sin(q3 + q2) + Math.Cos(q2) * Izz3)
                   + qd2 * (2 * a3 * m3 * Math.Sin(q3) * (a2 * Math.Sin(q2) + a3 * Math.Sin(q2 + q3)) + Math.Cos(q2) * (Izz2 + Izz3))
                );

            n22y = 0.5 * qdd1 * (
                   Math.Cos(q2) * (a3 * a3 * m3 + 2 * a2 * a2 * (m2 + m3) + 4 * a2 * a3 * m3 * Math.Cos(q3))
                   + a3 * m3 * (a3 * Math.Cos(q2 + 2 * q3) - 2 * a2 * Math.Sin(q2) * Math.Sin(q3))
                   )
                   - qd1 * (
                       qd3 * (2 * a3 * m3 * Math.Sin(q2 + q3) * (a2 + a3 * Math.Cos(q3)) + Math.Sin(q2) * Izz3)
                       + qd2 * (2 * a3 * m3 * Math.Cos(q2) * Math.Sin(q3) * (a2 + a3 * Math.Cos(q3))
                             + Math.Sin(q2) * (a3 * a3 * m3 + 2 * a2 * a2 * (m2 + m3) + a3 * m3 * (4 * a2 * Math.Cos(q3) + a3 * Math.Cos(2 * q3)) + Izz2 + Izz3)
                       )
                       );
            tao1 = Math.Cos(q2) * n22y + Math.Sin(q2) * n22x;

            Joint_Torque[0] = tao1;
            Joint_Torque[1] = tao2;
            Joint_Torque[2] = tao3;

            return Joint_Torque;
        }
        double[] Closed_Arm_Modle_decoup(double[] jointstate)
        {/* 这个形式可以应用计算力矩法，并且惯量矩阵的对角线上的值都可以参与运算*/
            double[] Joint_Torque = new double[9];
            double tao3, tao2, tao1, n11x, n11y, n11z, n22y, n22x, n33x, n33y, n33z;
            double q1, qd1, qdd1, q2, qd2, qdd2, q3, qd3, qdd3;   //关节状态
            double a1, a2, a3, m1, m2, m3;
            double Ixx1, Iyy1, Izz1, Ixx2, Izz2, Iyy2, Izz3, Ixx3, Iyy3;
            double M1_Q1, C1_Q1Q3, C1_Q1Q2, G2, C2_Q1Q1, C2_Q2Q2, C2_Q2Q3, C2_Q3Q3, M2_Q3, M2_Q2, G3, C3_Q1Q1, C3_Q2Q2, M3_Q3, M3_Q2;
            q1 = jointstate[0]; qd1 = jointstate[3]; qdd1 = jointstate[6];
            q2 = jointstate[1]; qd2 = jointstate[4]; qdd2 = jointstate[7];
            q3 = jointstate[2]; qd3 = jointstate[5]; qdd3 = jointstate[8];

            a1 = 0; a2 = 0.12; a3 = 0.12;
            m1 = 0; m2 = 0.35;  m3 = 0.01;
            Izz1 = 0.0015;    Ixx2 = 0;       Ixx3 = 0;
            Iyy1 = 0.0015; Iyy2 = 0.00437; Iyy3 = 0.00108;
            Ixx1 = 0.0015;    Izz2 = 0.00437; Izz3 = 0.00107;

            G3 = a3 * m3 * Math.Cos(q2 + q3) * 9.81;
            C3_Q1Q1 = a3 * m3 * ((0.5 * a2 * Math.Sin(q3) + 0.5 * a3 * Math.Sin(2 * (q2 + q3)) + 0.5 * a2 * Math.Sin(2 * q2 + q3))) + 0.5 * Iyy3 * Math.Sin(2 * (q2 + q3));
            C3_Q2Q2 = a3 * m3 * a2 * Math.Sin(q3);
            M3_Q3 = (a3 * a3 * m3 + Izz3);
            M3_Q2 = (a3 * m3 * (a2 * Math.Cos(q3) + a3) + Izz3);

            tao3 = M3_Q2 * qdd2 + M3_Q3 * qdd3 + C3_Q1Q1 * qd1 * qd1 + C3_Q2Q2 * qd2 * qd2 + G3;


            G2 = tao3 + a2 * Math.Cos(q2) * (9.81 * m2 + 9.81 * m3);
            C2_Q1Q1 = a2 * (a2 * Math.Sin(2 * q2) * 0.5 * (m2 + m3) + a3 * m3 * (-0.5 * Math.Sin(q3) + 0.5 * Math.Sin(2 * q2 + q3))) + 0.5 * Math.Sin(2 * q2) * Iyy2;
            C2_Q2Q2 = -a2 * a3 * m3 * Math.Sin(q3);
            C2_Q2Q3 = -2 * a2 * a3 * m3 * Math.Sin(q3);
            C2_Q3Q3 = -a2 * a3 * m3 * Math.Sin(q3);
            M2_Q3 = a2 * a3 * m3 * Math.Cos(q3);
            M2_Q2 = (a2 * (a2 * m2 + a2 * m3 + a3 * m3 * Math.Cos(q3)) + Izz2);
            tao2 = M2_Q2 * qdd2 + M2_Q3 * qdd3 + C2_Q1Q1 * qd1 * qd1 + C2_Q2Q2 * qd2 * qd2 + C2_Q2Q3 * qd2 * qd3 + C2_Q3Q3 * qd3 * qd3 + G2;



            M1_Q1 = 0.500 * (a3 * a3 * m3 + a2 * a2 * (m2 + m3) + a2 * a2 * (m2 + m3) * Math.Cos(2 * q2) + a3 * m3 * (4 * a2 * Math.Cos(q2) * Math.Cos(q2 + q3) + a3 * Math.Cos(2 * (q2 + q3)))
                  + 2 * Math.Sin(q2) * Math.Sin(q2) * Ixx2 + 2 * Math.Sin(q2 + q3) * Math.Sin(q2 + q3) * Ixx3 + 2 * Iyy1 + 2 * Math.Cos(q2) * Math.Cos(q2) * Iyy2 + 2 * Math.Cos(q2 + q3) * Math.Cos(q2 + q3) * Iyy3);
            C1_Q1Q3 = -(2 * Math.Sin(q2 + q3) * (a3 * m3 * (a2 * Math.Cos(q2) + a3 * Math.Cos(q2 + q3)) + Math.Cos(q2 + q3) * (-Ixx3 + Iyy3)));
            C1_Q1Q2 = -(a2 * a2 * (m2 + m3) * Math.Sin(2 * q2) + a3 * m3 * (a3 * Math.Sin(2 * (q2 + q3)) + 2 * a2 * Math.Sin(2 * q2 + q3)) + Math.Sin(2 * q2) * (-Ixx2 + Iyy2) + Math.Sin(2 * (q2 + q3)) * (-Ixx3 + Iyy3));
            tao1 = M1_Q1 * qdd1 + C1_Q1Q3 * qd1 * qd3 + C1_Q1Q2 * qd1 * qd2;

            Joint_Torque[0] = tao1;
            Joint_Torque[1] = tao2;
            Joint_Torque[2] = tao3;

            return Joint_Torque;
        }
        //拖拽示教，重力补偿
        double[] Closed_Arm_Modle_thetch(double[] jointstate)
        {/* 这个形式可以应用计算力矩法，并且惯量矩阵的对角线上的值都可以参与运算*/
            double[] Joint_Torque = new double[9];
            double tao3, tao2, tao1, n11x, n11y, n11z, n22y, n22x, n33x, n33y, n33z;
            double q1, qd1, qdd1, q2, qd2, qdd2, q3, qd3, qdd3;   //关节状态
            double a1, a2, a3, m1, m2, m3;
            double Ixx1, Iyy1, Izz1, Ixx2, Izz2, Iyy2, Izz3, Ixx3, Iyy3;
            double M1_Q1, C1_Q1Q3, C1_Q1Q2, G2, C2_Q1Q1, C2_Q2Q2, C2_Q2Q3, C2_Q3Q3, M2_Q3, M2_Q2, G3, C3_Q1Q1, C3_Q2Q2, M3_Q3, M3_Q2;
            q1 = jointstate[0]; qd1 = jointstate[3]; qdd1 = jointstate[6];
            q2 = jointstate[1]; qd2 = jointstate[4]; qdd2 = jointstate[7];
            q3 = jointstate[2]; qd3 = jointstate[5]; qdd3 = jointstate[8];

            a1 = 0; a2 = 0.12; a3 = 0.12;
            m1 = 0; m2 = 0.32; m3 = 0.01;
            Izz1 = 0; Izz2 = 0.00437; Izz3 = 0.00107;
            Iyy1 = 0.001; Iyy2 = 0.00437; Iyy3 = 0.00108;
            Ixx1 = 0; Ixx2 = 0; Ixx3 = 0;

            G3 = a3 * m3 * Math.Cos(q2 + q3) * 9.81;

            tao3 = G3;


            G2 = tao3 + a2 * Math.Cos(q2) * (9.81 * m2 + 9.81 * m3);

            tao2 = G2;

            tao1 = 0.0;
            Joint_Torque[0] = tao1;
            Joint_Torque[1] = tao2;
            Joint_Torque[2] = tao3;

            return Joint_Torque;
        }
        //计算d~
        double[] get_d_tiled(double[] jointstate)
        {/* 误差模型，求力矩*/
            double[] Joint_Torque = new double[9];
            double tao3, tao2, tao1, n11x, n11y, n11z, n22y, n22x, n33x, n33y, n33z;
            double q1, qd1, qdd1, q2, qd2, qdd2, q3, qd3, qdd3;   //关节状态
            double a1, a2, a3, m1, m2, m3;
            double Ixx1, Iyy1, Izz1, Ixx2, Izz2, Iyy2, Izz3, Ixx3, Iyy3;
            double a1_hat, a2_hat, a3_hat, m1_hat, m2_hat, m3_hat;
            double Ixx1_hat, Iyy1_hat, Izz1_hat, Ixx2_hat, Izz2_hat, Iyy2_hat, Izz3_hat, Ixx3_hat, Iyy3_hat;
            double M1_Q1, C1_Q1Q3, C1_Q1Q2, G2, C2_Q1Q1, C2_Q2Q2, C2_Q2Q3, C2_Q3Q3, M2_Q3, M2_Q2, G3, C3_Q1Q1, C3_Q2Q2, M3_Q3, M3_Q2;
            q1 = jointstate[0]; qd1 = jointstate[3]; qdd1 = jointstate[6];
            q2 = jointstate[1]; qd2 = jointstate[4]; qdd2 = jointstate[7];
            q3 = jointstate[2]; qd3 = jointstate[5]; qdd3 = jointstate[8];

            /* 误差惯性参数*/
            a1 = 0; a2 = 0.01; a3 = 0.01;
            m1 = 0; m2 = 0.01; m3 = 0.001;
            Izz1 = 0.0001; Ixx2 = 0; Ixx3 = 0;
            Iyy1 = 0.0001; Iyy2 = 0.0001; Iyy3 = 0.00018;
            Ixx1 = 0.0001; Izz2 = 0.0001; Izz3 = 0.00017;

            G3 = a3 * m3 * Math.Cos(q2 + q3) * 9.81;
            C3_Q1Q1 = a3 * m3 * ((0.5 * a2 * Math.Sin(q3) + 0.5 * a3 * Math.Sin(2 * (q2 + q3)) + 0.5 * a2 * Math.Sin(2 * q2 + q3))) + 0.5 * Iyy3 * Math.Sin(2 * (q2 + q3));
            C3_Q2Q2 = a3 * m3 * a2 * Math.Sin(q3);
            M3_Q3 = (a3 * a3 * m3 + Izz3);
            M3_Q2 = (a3 * m3 * (a2 * Math.Cos(q3) + a3) + Izz3);
            tao3 = M3_Q2 * qdd2 + M3_Q3 * qdd3 + C3_Q1Q1 * qd1 * qd1 + C3_Q2Q2 * qd2 * qd2 + G3;


            G2 = tao3 + a2 * Math.Cos(q2) * (9.81 * m2 + 9.81 * m3);
            C2_Q1Q1 = a2 * (a2 * Math.Sin(2 * q2) * 0.5 * (m2 + m3) + a3 * m3 * (-0.5 * Math.Sin(q3) + 0.5 * Math.Sin(2 * q2 + q3))) + 0.5 * Math.Sin(2 * q2) * Iyy2;
            C2_Q2Q2 = -a2 * a3 * m3 * Math.Sin(q3);
            C2_Q2Q3 = -2 * a2 * a3 * m3 * Math.Sin(q3);
            C2_Q3Q3 = -a2 * a3 * m3 * Math.Sin(q3);
            M2_Q3 = a2 * a3 * m3 * Math.Cos(q3);
            M2_Q2 = (a2 * (a2 * m2 + a2 * m3 + a3 * m3 * Math.Cos(q3)) + Izz2);
            tao2 = M2_Q2 * qdd2 + M2_Q3 * qdd3 + C2_Q1Q1 * qd1 * qd1 + C2_Q2Q2 * qd2 * qd2 + C2_Q2Q3 * qd2 * qd3 + C2_Q3Q3 * qd3 * qd3 + G2;


            M1_Q1 = 0.500 * (a3 * a3 * m3 + a2 * a2 * (m2 + m3) + a2 * a2 * (m2 + m3) * Math.Cos(2 * q2) + a3 * m3 * (4 * a2 * Math.Cos(q2) * Math.Cos(q2 + q3) + a3 * Math.Cos(2 * (q2 + q3)))
                  + 2 * Math.Sin(q2) * Math.Sin(q2) * Ixx2 + 2 * Math.Sin(q2 + q3) * Math.Sin(q2 + q3) * Ixx3 + 2 * Iyy1 + 2 * Math.Cos(q2) * Math.Cos(q2) * Iyy2 + 2 * Math.Cos(q2 + q3) * Math.Cos(q2 + q3) * Iyy3);
            C1_Q1Q3 = -(2 * Math.Sin(q2 + q3) * (a3 * m3 * (a2 * Math.Cos(q2) + a3 * Math.Cos(q2 + q3)) + Math.Cos(q2 + q3) * (-Ixx3 + Iyy3)));
            C1_Q1Q2 = -(a2 * a2 * (m2 + m3) * Math.Sin(2 * q2) + a3 * m3 * (a3 * Math.Sin(2 * (q2 + q3)) + 2 * a2 * Math.Sin(2 * q2 + q3)) + Math.Sin(2 * q2) * (-Ixx2 + Iyy2) + Math.Sin(2 * (q2 + q3)) * (-Ixx3 + Iyy3));
            tao1 = M1_Q1 * qdd1 + C1_Q1Q3 * qd1 * qd3 + C1_Q1Q2 * qd1 * qd2;


            /* 估计模型，求惯性矩阵*/
            a1_hat = 0; a2_hat = 0.12; a3_hat = 0.12;
            m1_hat = 0; m2_hat = 0.35; m3_hat = 0.01;
            Izz1_hat = 0.0015; Ixx2_hat = 0; Ixx3_hat = 0;
            Iyy1_hat = 0.0015; Iyy2_hat = 0.00437; Iyy3_hat = 0.00108;
            Ixx1_hat = 0.0015; Izz2_hat = 0.00437; Izz3_hat = 0.00107;


            M3_Q3 = (a3_hat * a3_hat * m3_hat + Izz3_hat);

            M2_Q2 = (a2_hat * (a2_hat * m2_hat + a2_hat * m3_hat + a3_hat * m3_hat * Math.Cos(q3)) + Izz2_hat);

            M1_Q1 = 0.500 * (a3_hat * a3_hat * m3_hat + a2_hat * a2_hat * (m2_hat + m3_hat) + a2_hat * a2_hat * (m2_hat + m3_hat) * Math.Cos(2 * q2) + a3_hat * m3_hat * (4 * a2_hat * Math.Cos(q2) * Math.Cos(q2 + q3) + a3_hat * Math.Cos(2 * (q2 + q3)))
             + 2 * Math.Sin(q2) * Math.Sin(q2) * Ixx2_hat + 2 * Math.Sin(q2 + q3) * Math.Sin(q2 + q3) * Ixx3_hat + 2 * Iyy1_hat + 2 * Math.Cos(q2) * Math.Cos(q2) * Iyy2_hat + 2 * Math.Cos(q2 + q3) * Math.Cos(q2 + q3) * Iyy3_hat);

            Joint_Torque[0] = tao1/ M1_Q1;
            Joint_Torque[1] = tao2/ M2_Q2;
            Joint_Torque[2] = tao3/ M3_Q3;

            return Joint_Torque;
        }


        private void InverTurn_Click_shijiao()
        {
            /**
              * 参 考 模 型
              * 预先计算，在线调取结果
              * **/


            var R = Matrix<double>.Build;
            double CurrentTime = 0.0;
            double x, dx, ddx, y, dy, ddy, z, dz, ddz;  //期望轨迹变量
            double q1, q21, q22, qd1, qdd1, q2, qd2, qdd2, q3, qd3, qdd3;   //关节状态
            double FuY, r, a, b, c, a1, a2, a3, elbow, m1, m2, m3, l1, l2, l3;
            double rate, Amplitude, x_0, y_0, z_0;


            //机器人模型  
            //质量：kg；长度：m
            //matrix类型
            //杆长、质量、p1sta、Ic , rc , 都要注意
            a1 = 0; a2 = 0.12; a3 = 0.12;
            m1 = 0; m2 = 0.45; m3 = 0.008;
            var z00 = CreatMatrix(0, 0, 1);
            var p0 = CreatMatrix(0, 0, 0);
            var p1sta = CreatMatrix(a1, 0, 0);
            var p2sta = CreatMatrix(a2, 0, 0);
            var p3sta = CreatMatrix(a3, 0, 0);
            var omega00 = CreatMatrix(0, 0, 0);
            var v00 = CreatMatrix(0, 0, 0);
            var epsilon00 = CreatMatrix(0, 0, 0);
            var a00 = CreatMatrix(0, 0, 0);
            var rc1 = CreatMatrix(0, 0, 0);
            var rc2 = CreatMatrix(0, 0, 0);
            var rc3 = CreatMatrix(0, 0, 0);
            var g = CreatMatrix(0, 0, -9.81);
            double[,] Ic1Array = { { 0.0015, 0, 0 }, { 0, 0.0015, 0 }, { 0, 0, 0.0015 } };//常数向量
            var Ic1 = R.DenseOfArray(Ic1Array);
            double[,] Ic2Array = { { 0, 0, 0 }, { 0, 0.00437, 0 }, { 0, 0, 0.00437 } };//常数向量
            var Ic2 = R.DenseOfArray(Ic2Array);
            double[,] Ic3Array = { { 0, 0, 0 }, { 0, 0.00108, 0 }, { 0, 0, 0.00107 } };//常数向量
            var Ic3 = R.DenseOfArray(Ic3Array);
            var f44 = CreatMatrix(0, 0, 0);
            var n44 = CreatMatrix(0, 0, 0);

            /*定时 6 秒，间隔 1 ms，6000步*/
            for (int i = 0; i <= 6000; i++)
            {

                CurrentTime = i / 1000.0;

                rate = 3;
                Amplitude = 0.015;
                x_0 = 0.19;
                y_0 = 0;
                z_0 = 0.001;//不能为0！

                /*设置工作空间的期望轨迹*/
                //竖线
                //x = x_0 + Amplitude * Math.Cos(rate * CurrentTime);
                //dx = -Amplitude * rate * Math.Sin(rate * CurrentTime);
                //ddx = -Amplitude * rate * rate * Math.Cos(rate * CurrentTime);
                //y = y_0; dy = 0; ddy = 0;
                //z = z_0; dz = 0; ddz = 0;


                //横线
                //y = y_0 + Amplitude * Math.Cos(rate * CurrentTime);
                //dy = -Amplitude * rate * Math.Sin(rate * CurrentTime);
                //ddy = -Amplitude * rate * rate * Math.Cos(rate * CurrentTime);
                //x = x_0; dx = 0; ddx = 0;
                //z = z_0; dz = 0; ddz = 0;

                //螺旋线
                x = x_0 + Amplitude * Math.Cos(rate * CurrentTime);
                dx = -Amplitude * rate * Math.Sin(rate * CurrentTime);
                ddx = -Amplitude * rate * rate * Math.Cos(rate * CurrentTime);
                y = y_0 + Amplitude * Math.Sin(rate * CurrentTime);
                dy = Amplitude * rate * Math.Cos(rate * CurrentTime);
                ddy = -Amplitude * rate * rate * Math.Sin(rate * CurrentTime);
                z = z_0 + 0.003 * CurrentTime; dz = 0.01; ddz = 0;


                q1 = q1_D[i];
                q2 = q2_D[i];
                q3 = q3_D[i];


                //变换矩阵
                //似乎默认都是列向量
                double[,] R01Array = { { Math.Cos(q1), 0, Math.Sin(q1) }, { Math.Sin(q1), 0, -Math.Cos(q1) }, { 0, 1, 0 } };
                double[,] R12Array = { { Math.Cos(q2), -Math.Sin(q2), 0 }, { Math.Sin(q2), Math.Cos(q2), 0 }, { 0, 0, 1 } };
                double[,] R23Array = { { Math.Cos(q3), -Math.Sin(q3), 0 }, { Math.Sin(q3), Math.Cos(q3), 0 }, { 0, 0, 1 } };
                double[,] P01Array = { { 0 }, { 0 }, { 0 } };
                double[,] P12Array = { { a2 * Math.Cos(q2) }, { a3 * Math.Sin(q2) }, { 0 } };
                double[,] P23Array = { { a3 * Math.Cos(q3) }, { a3 * Math.Sin(q3) }, { 0 } };
                var R01 = R.DenseOfArray(R01Array);//Console.WriteLine($"R01 = {R01}--第{i}次");
                var R12 = R.DenseOfArray(R12Array);//Console.WriteLine($"R12 = {R12}--第{i}次");
                var R23 = R.DenseOfArray(R23Array);
                var R02 = R01 * R12;
                var R03 = R01 * R12 * R23;
                var P01 = R.DenseOfArray(P01Array);//Console.WriteLine($"P01 = {P01}--第{i}次");
                var P12 = R.DenseOfArray(P12Array);//Console.WriteLine($"P12 = {P12}--第{i}次");
                var P23 = R.DenseOfArray(P23Array);


                /*计算雅可比矩阵*/
                //var z01 = R01 * z00;
                //var z02 = R01 * R12 * z00;
                //var p1 = P01;
                //var p2 = R01 * P12 + P01;
                //var p3 = R01 * R12 * P23 + p2; //Console.WriteLine($"p3 = {p3}--第{i}次");
                //var j_b1 = MyCross(z00, (p3 - p0)); //Console.WriteLine($"P12 = {j_b1}--第{i}次");
                //var j_b2 = MyCross(z01, (p3 - p1));
                //var j_b3 = MyCross(z02, (p3 - p2));
                //var jacobian = j_b1.Append(j_b2);
                //jacobian = jacobian.Append(j_b3);//Console.WriteLine($"P12 = {jacobian}--第{i}次");


                /*逆速度计算*/
                //统一转换为矩阵进行运算
                //Numerator:分子。Denominator：分母
                //var end_vol = CreatMatrix(dx, dy, dz);//Console.WriteLine($"end_vol = {end_vol}--第{i}次");
                //var j11 = jacobian.Column(0); var j12 = jacobian.Column(1); var j13 = jacobian.Column(2);
                //var j21 = jacobian.Column(0) * jacobian.Column(0);
                //var j22 = jacobian.Column(0) * jacobian.Column(1);
                //var j23 = jacobian.Column(0) * jacobian.Column(2);//
                //var j31 = jacobian.Column(1) * jacobian.Column(0);
                //var j32 = jacobian.Column(1) * jacobian.Column(1);
                //var j33 = jacobian.Column(1) * jacobian.Column(2);
                //var n1 = j11;
                //var n2 = j11 * j22 - j12 * j21;
                //var n3 = -j13 * j22 * j31 + j12 * j23 * j31 + j13 * j21 * j32 - j11 * j23 * j32 - j12 * j21 * j33 + j11 * j22 * j33;
                //var MatrixQd3Numerator = (end_vol.Transpose() * n3.ToColumnMatrix());  //Console.WriteLine($"MatrixQd3Numerator = {MatrixQd3Numerator}--第{i}次");
                //var MatrixQd3Denominator = (jacobian.Column(2).ToRowMatrix() * n3.ToColumnMatrix());   //Console.WriteLine($"MatrixQd2Denominator = {MatrixQd2Denominator}--第{i}次");
                //qd3 = MatrixQd3Numerator[0, 0] / MatrixQd3Denominator[0, 0]; //Console.WriteLine($"qd3 = {qd3}--第{i}次");                                            

                //var MatrixQd2Numerator = n2.ToRowMatrix() * (end_vol - (qd3 * jacobian.Column(2).ToColumnMatrix())); //Console.WriteLine($"MatrixQd1Numerator = {MatrixQd1Numerator}--第{i}次");
                //var MatrixQd2Denominator = (jacobian.Column(1).ToRowMatrix() * n2.ToColumnMatrix());//Console.WriteLine($"matrixqd1Denominator = {matrixqd1Denominator}--第{i}次");
                //qd2 = MatrixQd2Numerator[0, 0] / MatrixQd2Denominator[0, 0]; //Console.WriteLine($"qd2 = {qd2}--第{i}次");

                //var MatrixQd1Numerator = n1.ToRowMatrix() * (end_vol - (qd2 * jacobian.Column(1).ToColumnMatrix() + qd3 * jacobian.Column(2).ToColumnMatrix())); //Console.WriteLine($"MatrixQd1Numerator = {MatrixQd1Numerator}--第{i}次");
                //var MatrixQd1Denominator = (jacobian.Column(0).ToRowMatrix() * n1.ToColumnMatrix()); //Console.WriteLine($"matrixqd1Denominator = {matrixqd1Denominator}--第{i}次");
                //qd1 = MatrixQd1Numerator[0, 0] / MatrixQd1Denominator[0, 0]; //Console.WriteLine($"qd1 = {qd1}--第{i}次");

                qd1 = dq1_D[i];
                qd2 = dq2_D[i];
                qd3 = dq3_D[i];
                qdd1 = 0; qdd2 = 0; qdd3 = 0;


                /*正向速度计算*/
                var omega11 = R01.Transpose() * (omega00 + z00 * qd1); //Console.WriteLine($"omega11 = {omega11}--第{i}次");
                var omega22 = R12.Transpose() * (omega11 + z00 * qd2); //Console.WriteLine($"omega22 = {omega22}--第{i}次");
                var omega33 = R23.Transpose() * (omega22 + z00 * qd3); //Console.WriteLine($"omega22 = {omega22}--第{i}次");
                var v11 = R01.Transpose() * v00 + MyCross(omega11, p1sta); //Console.WriteLine($"v11 = {v11}--第{i}次");
                var v22 = R12.Transpose() * v11 + MyCross(omega22, p2sta); //Console.WriteLine($"v22 = {v22}--第{i}次\n");
                var v33 = R23.Transpose() * v22 + MyCross(omega33, p3sta); //Console.WriteLine($"v33 = {v33}--第{i}次\n");
                var v03 = R01 * R12 * R23 * v33; //Console.WriteLine($"v03 = {v03}--第{i}次\n");


                /*雅可比导数的计算*/
                //var omega01 = R01 * omega11;
                //var omega02 = R01 * R12 * omega22;
                //var v01 = R01 * v11; var v02 = R01 * R12 * v22;
                //var db11 = MyCross(omega00, z00); var db12 = p3 - p0;
                //var db1 = MyCross(db11, db12) + MyCross(z00, (v03 - v00)); //Console.WriteLine($"db1 = {db1}--第{i}次");
                //var db21 = MyCross(omega01, z01); var db22 = p3 - p1;
                //var db2 = MyCross(db21, db22) + MyCross(z01, (v03 - v01)); //Console.WriteLine($"db2 = {db2}--第{i}次");
                //var db31 = MyCross(omega02, z02); var db32 = p3 - p2;
                //var db3 = MyCross(db31, db32) + MyCross(z02, (v03 - v02)); //Console.WriteLine($"db3 = {db3}--第{i}次");
                //var dj = db1.Append(db2); //Console.WriteLine($"dj = {dj}--第{i}次");
                //dj = dj.Append(db3); //Console.WriteLine($"dj = {dj}--第{i}次");


                ///*逆加速度计算*/
                //var xdds = CreatMatrix(ddx, ddy, ddz) - dj * CreatMatrix(qd1, qd2, qd3); //Console.WriteLine($"xdds = {xdds}--第{i}次");
                //var MatrixQdd3Numerator = (xdds.Transpose() * n3.ToColumnMatrix());                 //Console.WriteLine($"MatrixQdd2Numerator = {MatrixQdd2Numerator}--第{i}次");
                //var MatrixQdd3Denominator = (jacobian.Column(2).ToRowMatrix() * n3.ToColumnMatrix());   //Console.WriteLine($"MatrixQdd2Denominator = {MatrixQdd2Denominator}--第{i}次");
                //qdd3 = MatrixQdd3Numerator[0, 0] / MatrixQdd3Denominator[0, 0]; //Console.WriteLine($"qdd3 = {qdd3}--第{i}次");                

                //var MatrixQdd2Numerator = n2.ToRowMatrix() * (xdds - (qdd3 * jacobian.Column(2).ToColumnMatrix())); //Console.WriteLine($"MatrixQd1Numerator = {MatrixQd1Numerator}--第{i}次");
                //var MatrixQdd2Denominator = (jacobian.Column(1).ToRowMatrix() * n2.ToColumnMatrix());//Console.WriteLine($"matrixqd1Denominator = {matrixqd1Denominator}--第{i}次");
                //qdd2 = MatrixQdd2Numerator[0, 0] / MatrixQdd2Denominator[0, 0]; //Console.WriteLine($"qdd2 = {qdd2}--第{i}次");

                //var MatrixQdd1Numerator = n1.ToRowMatrix() * (xdds - (qdd2 * jacobian.Column(1).ToColumnMatrix() + qdd3 * jacobian.Column(2).ToColumnMatrix())); //Console.WriteLine($"MatrixQd1Numerator = {MatrixQd1Numerator}--第{i}次");
                //var MatrixQdd1Denominator = (jacobian.Column(0).ToRowMatrix() * n1.ToColumnMatrix()); //Console.WriteLine($"matrixqd1Denominator = {matrixqd1Denominator}--第{i}次");
                //qdd1 = MatrixQdd1Numerator[0, 0] / MatrixQdd1Denominator[0, 0]; //Console.WriteLine($"qdd1 = {qdd1}--第{i}次");

                //保存期望关节状态
                //q1_D[i] = q1; q2_D[i] = q2; q3_D[i] = q3;

                //dq1_D[i] = qd1; dq2_D[i] = qd2; dq3_D[i] = qd3;

                //ddq1_D[i] = qdd1; ddq2_D[i] = qdd2; ddq3_D[i] = qdd3;







                /*正加速计算*/
                var epsilon11 = R01.Transpose() * (epsilon00 + MyCross(omega00, z00 * qd1) + z00 * qdd1); //Console.WriteLine($"epsilon11 = {epsilon11}--第{i}次");
                var epsilon22 = R12.Transpose() * (epsilon11 + MyCross(omega11, z00 * qd2) + z00 * qdd2); //Console.WriteLine($"epsilon22 = {epsilon22}--第{i}次");
                var epsilon33 = R23.Transpose() * (epsilon22 + MyCross(omega22, z00 * qd3) + z00 * qdd3); //Console.WriteLine($"epsilon33 = {epsilon33}--第{i}次");
                var a11 = R01.Transpose() * a00 + MyCross(epsilon11, p1sta) + MyCross(omega11, MyCross(omega11, p1sta)); //Console.WriteLine($"a11 = {a11}--第{i}次");
                var a22 = R12.Transpose() * a11 + MyCross(epsilon22, p2sta) + MyCross(omega22, MyCross(omega22, p2sta)); //Console.WriteLine($"a22 = {a22}--第{i}次");
                var a33 = R23.Transpose() * a22 + MyCross(epsilon33, p3sta) + MyCross(omega33, MyCross(omega33, p3sta)); //Console.WriteLine($"a22 = {a22}--第{i}次");
                var ac11 = a11 + MyCross(epsilon11, rc1) + MyCross(omega11, MyCross(omega11, rc1)); //Console.WriteLine($"ac11 = {ac11}--第{i}次");
                var ac22 = a22 + MyCross(epsilon22, rc2) + MyCross(omega22, MyCross(omega22, rc2)); //Console.WriteLine($"ac22 = {ac22}--第{i}次");
                var ac33 = a33 + MyCross(epsilon33, rc3) + MyCross(omega33, MyCross(omega33, rc3)); //Console.WriteLine($"ac33 = {ac33}--第{i}次");


                /*计算力*/
                var g1 = R01.Transpose() * g;
                var g2 = R02.Transpose() * g;
                var g3 = R03.Transpose() * g; //Console.WriteLine($"f33 = {g3}--第{i}次");
                var f33 = m3 * (ac33 - g3) + f44; //Console.WriteLine($"f11 = {f33}--第{i}次");
                var f22 = m2 * (ac22 - g2) + R23 * f33;
                var f11 = m1 * (ac11 - g1) + R12 * f22; //Console.WriteLine($"f11 = {f11}--第{i}次");
                var n33 = Ic3 * epsilon33 + MyCross(omega33, (Ic3 * omega33)) + n44 + MyCross(p3sta, f33) + MyCross(rc3, (m3 * (ac33 - g3))); //Console.WriteLine($"n33 = {n33}--第{i}次");
                var n22 = Ic2 * epsilon22 + MyCross(omega22, (Ic2 * omega22)) + R23 * n33 + MyCross(p2sta, f22) + MyCross(rc2, (m2 * (ac22 - g2)));  //Console.WriteLine($"n22 = {n22}--第{i}次");
                var n11 = Ic1 * epsilon11 + MyCross(omega11, (Ic1 * omega11)) + R12 * n22 + MyCross(p1sta, f11) + MyCross(rc1, (m1 * (ac11 - g1))); //Console.WriteLine($"n11 = {n11}--第{i}次");
                var tao33 = n33.Transpose() * (R23.Transpose() * z00); //Console.WriteLine($"tao33 = {(float)tao33[0, 0]}--第{i}次");
                var tao22 = n22.Transpose() * (R12.Transpose() * z00); //Console.WriteLine($"tao22 = {(float)tao22[0, 0]}--第{i}次");
                var tao11 = n11.Transpose() * (R01.Transpose() * z00); //Console.WriteLine($"tao11 = {(float)tao11[0, 0]}--第{i}次");

                referenceT_1[i] = tao11[0, 0];
                referenceT_2[i] = tao22[0, 0];
                referenceT_3[i] = tao33[0, 0];

            }








        }


        private void SetrichTextBox(string value)
        {
            //CAN_richBox.Focus(); //让文本框获取焦点 
            CAN_richBox.Select(CAN_richBox.TextLength, 0);//设置光标的位置到文本尾
            CAN_richBox.ScrollToCaret();//滚动到控件光标处 
            CAN_richBox.AppendText(value);//添加内容            
        }

        int float_to_uint(float x, float x_min, float x_max, int bits)//将期望浮点数转化为发送的帧数据
        {
            float span = x_max - x_min;
            float offset = x_min;
            return (int)((x - offset) * ((float)((1 << bits) - 1)) / span);
        }

        Byte[] Float2Byte(float x, float x_min, float x_max, int bits)
        {
            //低四位为1：0x0f
            //4-8位为1：0xf0
            //8-12位：0xf00
            //12-16位：0xf000
            int x_int = float_to_uint(x, x_min, x_max, bits);
            byte[] bytes = new byte[2];
            if (bits > 12)
            {
                bytes[1] = (byte)(x_int & 0xff);
                bytes[0] = (byte)((x_int & 0xff00) >> 8);
            }
            else
            {
                bytes[1] = (byte)(x_int & 0xff);
                bytes[0] = (byte)((x_int & 0x0f00) >> 8);
            }
            return bytes;
        }

        void InputMotorTor(float torque, byte ID = 1)
        {
            //可以限制速度，限制角度，限制最大力矩
            if (torque > 2.0f || torque < -1.5f)
            {
                //DisEnableMotor();
                //isButtonClick = false;
                //CAN_richBox.Text += torque + ": 好家伙！！力矩太大了！！";

                 torque = 0f;
            }


            float torque_min = -10.0f;
            float torque_max = 10.0f;
            int torque_bits = 12;

            byte CanID = ID;//数组索引：13
            byte[] pos_byte = new byte[2];
            byte[] SendDate = new byte[30];
            byte[] Turn = new byte[] { 0x7f, 0xff, 0x7f, 0xf0, 0x00, 0x00, 0x07, 0xff, 0x88 };
            byte[] SendDateHead = new byte[] { 0x55, 0xaa, 0x1e, 0x01, 0x01, 0x00, 0x00, 0x00, 0x0a, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x08, 0x00, 0x00 };
            SendDateHead.CopyTo(SendDate, 0);
            pos_byte = Float2Byte(torque, torque_min, torque_max, torque_bits);
            Turn[6] = pos_byte[0]; Turn[7] = pos_byte[1];
            Turn.CopyTo(SendDate, 21);
            SendDate[13] = CanID;
            if (vcom.IsOpen)
            {
                vcom.Write(SendDate, 0, SendDate.Length);
            }
            else
            {
                MessageBox.Show("请先打开设备！", "提示"); ;
            }

        }

        void InputMotorPosAndVel(float Pos, float Vel)
        {



            byte[] bytes_Pos = BitConverter.GetBytes(Pos);
            byte[] bytes_Vel = BitConverter.GetBytes(Vel);

            byte CanID = 0x01;//数组索引：15
            byte[] pos_byte = new byte[2];
            byte[] SendDate = new byte[30];
            byte[] Turn = new byte[] { 0x7f, 0xff, 0x7f, 0xf0, 0x00, 0x00, 0x07, 0xff, 0x88 };
            byte[] SendDateHead = new byte[] { 0x55, 0xaa, 0x1e, 0x01, 0x01, 0x00, 0x00, 0x00, 0x0a, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x08, 0x00, 0x00 };
            SendDateHead.CopyTo(SendDate, 0);
            bytes_Pos.CopyTo(Turn, 0);
            bytes_Vel.CopyTo(Turn, 4);

            Turn.CopyTo(SendDate, 21);
            SendDate[14] = CanID;//加0x100就是向右（高位）移动半个个字节即4位
            if (vcom.IsOpen)
            {
                vcom.Write(SendDate, 0, SendDate.Length);
            }
            else
            {
                MessageBox.Show("请先打开设备！", "提示"); ;
            }

        }


        float uint_to_float(int x_int, float x_min, float x_max, int bits)//将整数转化成浮点数
        {
            float span = x_max - x_min;
            float offset = x_min;
            return ((float)x_int) * span / ((float)((1 << bits) - 1)) + offset;
        }

        float Byt2Float(byte hige, byte low, string state)//将电机发送过来的帧数据转化成浮点数，用以参与控制运算
        {

            int IntValue16;
            float Pos, Vel, T;

            if (state == "Pos")
            {
                IntValue16 = hige << 8 | low;
                Pos = uint_to_float(IntValue16, -12.5f, 12.5f, 16);
                return Pos;
            }
            if (state == "Vel")
            {
                IntValue16 = hige << 4 | low >> 4;
                Vel = uint_to_float(IntValue16, -30.0f, 30.0f, 12);
                return Vel;
            }
            if (state == "T")
            {
                IntValue16 = (hige & 0x0f) << 8 | low;
                T = uint_to_float(IntValue16, -10.0f, 10.0f, 12);
                return T;
            }
            else throw new Exception("你到底想转化什么数据");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            AnglePlot.Plot.Clear();
            TorquePlot.Plot.Clear();
            TimePlot.Plot.Clear();
            VelocityPlot.Plot.Clear();
            nextDataIndex = 0;
            CAN_richBox.Text = string.Empty;
            angeldate[nextDataIndex] = 0;
            torquedata[nextDataIndex] = 0;
            caltimedata[nextDataIndex] = 0;
            veloccitydata[nextDataIndex] = 0;
            Time = 0;

            last_time = 0;
            //oldtime = 0;
        }

        void EnableMotor()
        {
            byte[] Enable = new byte[] { 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xfc, 0x88 };
            byte[] CanID = new byte[] { 0x01, 0x02, 0x03 };//数组索引：13
            byte[] SendDateHead = new byte[] { 0x55, 0xaa, 0x1e, 0x01, 0x01, 0x00, 0x00, 0x00, 0x0a, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x08, 0x00, 0x00 };
            byte[] SendDate_1 = new byte[30];
            byte[] SendDate_2 = new byte[30];
            byte[] SendDate_3 = new byte[30];
            SendDateHead.CopyTo(SendDate_1, 0);
            Enable.CopyTo(SendDate_1, 21);
            SendDate_1[13] = CanID[0];
            SendDate_1.CopyTo(SendDate_2, 0);
            SendDate_1.CopyTo(SendDate_3, 0);
            SendDate_2[13] = CanID[1];
            SendDate_3[13] = CanID[2];

            if (vcom.IsOpen)
            {
                vcom.Write(SendDate_1, 0, SendDate_1.Length);
                vcom.Write(SendDate_2, 0, SendDate_2.Length);
                vcom.Write(SendDate_3, 0, SendDate_3.Length);
            }
            else
            {
                MessageBox.Show("请先打开设备！", "提示"); ;
            }





        }

        void DisEnableMotor()
        {
            byte[] DisEnable = new byte[] { 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xfd, 0x88 };
            byte[] CanID = new byte[] { 0x01, 0x02, 0x03 };//数组索引：13
            byte[] SendDateHead = new byte[] { 0x55, 0xaa, 0x1e, 0x01, 0x01, 0x00, 0x00, 0x00, 0x0a, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x08, 0x00, 0x00 };
            byte[] SendDate_1 = new byte[30];
            byte[] SendDate_2 = new byte[30];
            byte[] SendDate_3 = new byte[30];
            SendDateHead.CopyTo(SendDate_1, 0);
            DisEnable.CopyTo(SendDate_1, 21);
            SendDate_1[13] = CanID[0];
            SendDate_1.CopyTo(SendDate_2, 0);
            SendDate_1.CopyTo(SendDate_3, 0);
            SendDate_2[13] = CanID[1];
            SendDate_3[13] = CanID[2];


            if (vcom.IsOpen)
            {
                vcom.Write(SendDate_1, 0, SendDate_1.Length);
                vcom.Write(SendDate_2, 0, SendDate_2.Length);
                vcom.Write(SendDate_3, 0, SendDate_3.Length);
            }
            else
            {
                MessageBox.Show("请先打开设备！", "提示"); ;
            }

        }




        public Matrix<double> MyCross(Matrix<double> Arr1, Matrix<double> Arr2)
            {
                //Arr1 x Arr2            
                var M = Matrix<double>.Build;
                double[,] SArray1 = { { 0.0, -Arr1[2, 0], Arr1[1, 0] }, { Arr1[2, 0], 0, -Arr1[0, 0] }, { -Arr1[1, 0], Arr1[0, 0], 0 } };
                var SArr1Matrix = M.DenseOfArray(SArray1);
                return SArr1Matrix * Arr2;
            }
        public Matrix<double> CreatMatrix(double x, double y, double z)
            {   //生成列向量     
                var M = Matrix<double>.Build;
                double[,] p0Array = { { x }, { y }, { z } };
                return M.DenseOfArray(p0Array);
            }
        public Matrix<double> CreatMatrix(double x, double y)
            {   //生成列向量     
                var M = Matrix<double>.Build;
                double[,] p0Array = { { x }, { y } };
                return M.DenseOfArray(p0Array);
            }


        private void button3_Click(object sender, EventArgs e)
        {
            int step = 1;
            if (comboBox2.Text == "1") step = 1;
            if (comboBox2.Text == "3") step = 3;
            if (comboBox2.Text == "6") step = 6;
            if (comboBox2.Text == "10") step = 10;
            A2 += step;
            show_factor();
        }
        private void button2_Click(object sender, EventArgs e)
        {
            int step = 1;
            if (comboBox1.Text == "1") step = 1;
            if (comboBox1.Text == "3") step = 3;
            if (comboBox1.Text == "6") step = 6;
            if (comboBox1.Text == "10") step = 10;
            A1 += step;
            show_factor();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            int step = 1;
            if (comboBox1.Text == "1") step = 1;
            if (comboBox1.Text == "3") step = 3;
            if (comboBox1.Text == "6") step = 6;
            if (comboBox1.Text == "10") step = 10;
            A1 -= step;
            show_factor();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            int step = 1;
            if (comboBox2.Text == "1") step = 1;
            if (comboBox2.Text == "3") step = 3;
            if (comboBox2.Text == "6") step = 6;
            if (comboBox2.Text == "10") step = 10;
            A2 -= step;
            show_factor();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            int step = 1;
            if (comboBox3.Text == "1") step = 1;
            if (comboBox3.Text == "3") step = 3;
            if (comboBox3.Text == "6") step = 6;
            if (comboBox3.Text == "10") step = 10;
            B1 += step;
            show_factor();
        }

        private void button7_Click(object sender, EventArgs e)
        {
            int step = 1;
            if (comboBox3.Text == "1") step = 1;
            if (comboBox3.Text == "3") step = 3;
            if (comboBox3.Text == "6") step = 6;
            if (comboBox3.Text == "10") step = 10;
            B1 -= step;
            show_factor();
        }


        private void formsPlot3_Load(object sender, EventArgs e)
        {

        }

        private void button8_Click(object sender, EventArgs e)
        {
            int step = 1;
            if (comboBox4.Text == "1") step = 1;
            if (comboBox4.Text == "3") step = 3;
            if (comboBox4.Text == "6") step = 6;
            if (comboBox4.Text == "10") step = 10;
            B2 += step;
            show_factor();
        }

        private void button9_Click(object sender, EventArgs e)
        {
            int step = 1;
            if (comboBox4.Text == "1") step = 1;
            if (comboBox4.Text == "3") step = 3;
            if (comboBox4.Text == "6") step = 6;
            if (comboBox4.Text == "10") step = 10;
            B2 -= step;
            show_factor();
        }

        private void button10_Click(object sender, EventArgs e)
        {
            int step = 1;
            if (comboBox5.Text == "1") step = 1;
            if (comboBox5.Text == "3") step = 3;
            if (comboBox5.Text == "6") step = 6;
            if (comboBox5.Text == "10") step = 10;
            C1 += step;
            show_factor();
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void label5_Click_1(object sender, EventArgs e)
        {

        }

        private void label7_Click(object sender, EventArgs e)
        {

        }

        private void button11_Click(object sender, EventArgs e)
        {
            int step = 1;
            if (comboBox5.Text == "1") step = 1;
            if (comboBox5.Text == "3") step = 3;
            if (comboBox5.Text == "6") step = 6;
            if (comboBox5.Text == "10") step = 10;
            C1 -= step;
            show_factor();
        }

        int is_shijiao = 0;
        private void button14_Click(object sender, EventArgs e)
        {
            
            if (is_shijiao == 0)
            {
                CAN_richBox.Clear();
                SetrichTextBox("*示*教*模*式*\n");
                SetrichTextBox("\n");
                SetrichTextBox("依次点击：");
                SetrichTextBox("清除数据-设置时长-运行算法\n");
                SetrichTextBox("之后就可以开始拖动机械臂\n");
                SetrichTextBox("拖动完成后请先示教初始化\n");
                SetrichTextBox("示教初始化之后才能点击“拖动完成”按键\n");
                is_shijiao = 1;
                button14.Text = "拖动完成";
                InverTurn.Text = "示教初始化";
            }
            else
            {
                CAN_richBox.Clear();
                SetrichTextBox("*跟*踪*模*式*\n");
                SetrichTextBox("\n");
                SetrichTextBox("请确保已完成示教初始化\n");
                is_shijiao = 0;
                button14.Text = "示教";
                InverTurn.Text = "轨迹跟踪初始化";
            }
        }

        private void label9_Click(object sender, EventArgs e)
        {

        }

        private void label12_Click(object sender, EventArgs e)
        {

        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void button12_Click(object sender, EventArgs e)
        {
            int step = 1;
            if (comboBox6.Text == "1") step = 1;
            if (comboBox6.Text == "3") step = 3;
            if (comboBox6.Text == "6") step = 6;
            if (comboBox6.Text == "10") step = 10;
            C2 += step;
            show_factor();
        }

        private void button13_Click(object sender, EventArgs e)
        {
            int step = 1;
            if (comboBox6.Text == "1") step = 1;
            if (comboBox6.Text == "3") step = 3;
            if (comboBox6.Text == "6") step = 6;
            if (comboBox6.Text == "10") step = 10;
            C2 -= step;
            show_factor();
        }

        private void show_factor()
        {
            label1.Text = "d1=" + A1.ToString();
            label2.Text = "d2=" + A2.ToString();
            label5.Text = "d3=" + B1.ToString();
            label6.Text = "k1=" + B2.ToString();
            label7.Text = "k2=" + C1.ToString();
            label8.Text = "k3=" + C2.ToString();
        }


        private void label5_Click(object sender, EventArgs e)
        {

        }


        private void button16_Click(object sender, EventArgs e)
        {
            Plotangle.MaxRenderIndex = 6010;//设置最大轴
            Plotangle1.MaxRenderIndex = 6010;//设置最大轴

            PlotTorque.MaxRenderIndex = 6010;//设置最大轴
            PlotTorque1.MaxRenderIndex = 6010;//设置最大轴

            PlotTime.MaxRenderIndex = 6010;//设置最大轴
            PlotTime1.MaxRenderIndex = 6010;//设置最大轴

            Plotvelocity.MaxRenderIndex = 6010;//设置最大轴
            Plotvelocity1.MaxRenderIndex = 6010;//设置最大轴

            AnglePlot.Plot.AxisAuto();
            AnglePlot.Refresh();

            TorquePlot.Plot.AxisAuto();
            TorquePlot.Refresh();

            VelocityPlot.Plot.AxisAuto();
            VelocityPlot.Refresh();

            TimePlot.Plot.AxisAuto();
            TimePlot.Refresh();


        }


        private void FreshPlot(float angle_d, float angle, float torque, float torque_d, float velocity = 0, float velocity_d = 0, float caltime = 0, float caltime_d = 0)
        {
            if (nextDataIndex >= angeldate.Length)
            {//数据多到数组装不下时
                throw new OverflowException("data array isn't long enough to accomodate new data");
                // in this situation the solution would be:
                //   1. clear the plot
                //   2. create a new larger array
                //   3. copy the old data into the start of the larger array
                //   4. plot the new (larger) array
                //   5. continue to update the new array
            }


            angeldate[nextDataIndex] = angle;//把当前值放到数组
            angeldate_d[nextDataIndex] = angle_d;
            torquedata[nextDataIndex] = torque;
            torquedata_d[nextDataIndex] = torque_d;
            caltimedata[nextDataIndex] = caltime;
            caltimedata_d[nextDataIndex] = caltime_d;
            veloccitydata[nextDataIndex] = velocity;
            veloccitydata_d[nextDataIndex] = velocity_d;




            nextDataIndex += 1;//次数加一

            if (nextDataIndex >= Time * 1000)//到达时间结束运行
            {
                CAN_richBox.Clear();
                isButtonClick = false;//算法停止不再显示
                SetrichTextBox("采样总次数："); SetrichTextBox(nextDataIndex.ToString());
                SetrichTextBox("\n");
                SetrichTextBox("计时时长："); SetrichTextBox(Time.ToString());
                SetrichTextBox("\n");
                InputMotorTor(0.0f,1);
                InputMotorTor(0.0f,2);
                InputMotorTor(0.0f,3);
                watch.Reset();
                watch.Stop();                
                last_time = 0;
                SaveTime = 0;
                nextDataIndex = 0;

                if(is_shijiao == 1)
                {
                    SetrichTextBox("拖动结束，请点击“示教初始化”");

                }

            }

        }


    }


}
