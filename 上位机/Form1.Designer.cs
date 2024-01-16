namespace version_03
{
    partial class Form1
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.serialPort = new System.IO.Ports.SerialPort(this.components);
            this.StopBit = new System.Windows.Forms.ComboBox();
            this.CheckBit = new System.Windows.Forms.ComboBox();
            this.DataBit = new System.Windows.Forms.ComboBox();
            this.comboBoxBaudrate = new System.Windows.Forms.ComboBox();
            this.buttonFresh = new System.Windows.Forms.Button();
            this.openButton = new System.Windows.Forms.Button();
            this.freshCAN_richBox = new System.Windows.Forms.Button();
            this.send = new System.Windows.Forms.Button();
            this.Disenble = new System.Windows.Forms.Button();
            this.CAN_richBox = new System.Windows.Forms.RichTextBox();
            this.Turn = new System.Windows.Forms.Button();
            this.InverTurn = new System.Windows.Forms.Button();
            this.testbutton1 = new System.Windows.Forms.Button();
            this.AnglePlot = new ScottPlot.FormsPlot();
            this.button1 = new System.Windows.Forms.Button();
            this.Duration = new System.Windows.Forms.ComboBox();
            this.Step = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.StartTime = new System.Windows.Forms.Button();
            this.TorquePlot = new ScottPlot.FormsPlot();
            this.button2 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.button4 = new System.Windows.Forms.Button();
            this.button5 = new System.Windows.Forms.Button();
            this.button6 = new System.Windows.Forms.Button();
            this.button7 = new System.Windows.Forms.Button();
            this.VelocityPlot = new ScottPlot.FormsPlot();
            this.TimePlot = new ScottPlot.FormsPlot();
            this.button8 = new System.Windows.Forms.Button();
            this.button9 = new System.Windows.Forms.Button();
            this.button10 = new System.Windows.Forms.Button();
            this.button11 = new System.Windows.Forms.Button();
            this.button12 = new System.Windows.Forms.Button();
            this.button13 = new System.Windows.Forms.Button();
            this.button16 = new System.Windows.Forms.Button();
            this.comboBoxPort = new System.Windows.Forms.ComboBox();
            this.label12 = new System.Windows.Forms.Label();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.comboBox2 = new System.Windows.Forms.ComboBox();
            this.comboBox3 = new System.Windows.Forms.ComboBox();
            this.comboBox4 = new System.Windows.Forms.ComboBox();
            this.comboBox5 = new System.Windows.Forms.ComboBox();
            this.comboBox6 = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.directorySearcher1 = new System.DirectoryServices.DirectorySearcher();
            this.button14 = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.SuspendLayout();
            // 
            // timer1
            // 
            this.timer1.Interval = 10;
            // 
            // StopBit
            // 
            this.StopBit.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.StopBit.FormattingEnabled = true;
            this.StopBit.Items.AddRange(new object[] {
            "One",
            "Two"});
            this.StopBit.Location = new System.Drawing.Point(14, 120);
            this.StopBit.Margin = new System.Windows.Forms.Padding(4);
            this.StopBit.Name = "StopBit";
            this.StopBit.Size = new System.Drawing.Size(79, 23);
            this.StopBit.TabIndex = 81;
            // 
            // CheckBit
            // 
            this.CheckBit.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.CheckBit.FormattingEnabled = true;
            this.CheckBit.Items.AddRange(new object[] {
            "None",
            "Even",
            "Odd"});
            this.CheckBit.Location = new System.Drawing.Point(14, 159);
            this.CheckBit.Margin = new System.Windows.Forms.Padding(4);
            this.CheckBit.Name = "CheckBit";
            this.CheckBit.Size = new System.Drawing.Size(79, 23);
            this.CheckBit.TabIndex = 82;
            // 
            // DataBit
            // 
            this.DataBit.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.DataBit.FormattingEnabled = true;
            this.DataBit.Items.AddRange(new object[] {
            "8",
            "9"});
            this.DataBit.Location = new System.Drawing.Point(14, 207);
            this.DataBit.Margin = new System.Windows.Forms.Padding(4);
            this.DataBit.Name = "DataBit";
            this.DataBit.Size = new System.Drawing.Size(79, 23);
            this.DataBit.TabIndex = 78;
            // 
            // comboBoxBaudrate
            // 
            this.comboBoxBaudrate.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxBaudrate.FormattingEnabled = true;
            this.comboBoxBaudrate.Items.AddRange(new object[] {
            "4800",
            "9600",
            "14400",
            "19200",
            "28800",
            "38400",
            "57600",
            "115200",
            "460800",
            "921600",
            "自定义输入"});
            this.comboBoxBaudrate.Location = new System.Drawing.Point(14, 252);
            this.comboBoxBaudrate.Margin = new System.Windows.Forms.Padding(4);
            this.comboBoxBaudrate.Name = "comboBoxBaudrate";
            this.comboBoxBaudrate.Size = new System.Drawing.Size(79, 23);
            this.comboBoxBaudrate.TabIndex = 76;
            // 
            // buttonFresh
            // 
            this.buttonFresh.Location = new System.Drawing.Point(107, 101);
            this.buttonFresh.Margin = new System.Windows.Forms.Padding(4);
            this.buttonFresh.Name = "buttonFresh";
            this.buttonFresh.Size = new System.Drawing.Size(113, 58);
            this.buttonFresh.TabIndex = 74;
            this.buttonFresh.Text = "刷新设备";
            this.buttonFresh.UseVisualStyleBackColor = true;
            this.buttonFresh.Click += new System.EventHandler(this.buttonFresh_Click);
            // 
            // openButton
            // 
            this.openButton.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.openButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.openButton.Location = new System.Drawing.Point(107, 43);
            this.openButton.Margin = new System.Windows.Forms.Padding(4);
            this.openButton.Name = "openButton";
            this.openButton.Size = new System.Drawing.Size(113, 58);
            this.openButton.TabIndex = 71;
            this.openButton.Text = "打开串口";
            this.openButton.UseVisualStyleBackColor = false;
            this.openButton.Click += new System.EventHandler(this.openButton_Click);
            // 
            // freshCAN_richBox
            // 
            this.freshCAN_richBox.Location = new System.Drawing.Point(99, 335);
            this.freshCAN_richBox.Name = "freshCAN_richBox";
            this.freshCAN_richBox.Size = new System.Drawing.Size(114, 29);
            this.freshCAN_richBox.TabIndex = 84;
            this.freshCAN_richBox.Text = "手动刷新显示";
            this.freshCAN_richBox.UseVisualStyleBackColor = true;
            this.freshCAN_richBox.Click += new System.EventHandler(this.freshCAN_richBox_Click);
            // 
            // send
            // 
            this.send.Location = new System.Drawing.Point(107, 159);
            this.send.Name = "send";
            this.send.Size = new System.Drawing.Size(113, 58);
            this.send.TabIndex = 86;
            this.send.Text = "使能电机";
            this.send.UseVisualStyleBackColor = true;
            this.send.Click += new System.EventHandler(this.send_Click);
            // 
            // Disenble
            // 
            this.Disenble.Location = new System.Drawing.Point(107, 217);
            this.Disenble.Name = "Disenble";
            this.Disenble.Size = new System.Drawing.Size(113, 58);
            this.Disenble.TabIndex = 87;
            this.Disenble.Text = "失能电机";
            this.Disenble.UseVisualStyleBackColor = true;
            this.Disenble.Click += new System.EventHandler(this.Disenble_Click);
            // 
            // CAN_richBox
            // 
            this.CAN_richBox.BackColor = System.Drawing.Color.MistyRose;
            this.CAN_richBox.Location = new System.Drawing.Point(6, 20);
            this.CAN_richBox.Name = "CAN_richBox";
            this.CAN_richBox.Size = new System.Drawing.Size(395, 341);
            this.CAN_richBox.TabIndex = 88;
            this.CAN_richBox.Text = "";
            // 
            // Turn
            // 
            this.Turn.Location = new System.Drawing.Point(219, 331);
            this.Turn.Name = "Turn";
            this.Turn.Size = new System.Drawing.Size(114, 33);
            this.Turn.TabIndex = 89;
            this.Turn.Text = "测试键";
            this.Turn.UseVisualStyleBackColor = true;
            this.Turn.Click += new System.EventHandler(this.Turn_Click);
            // 
            // InverTurn
            // 
            this.InverTurn.Location = new System.Drawing.Point(6, 27);
            this.InverTurn.Name = "InverTurn";
            this.InverTurn.Size = new System.Drawing.Size(146, 45);
            this.InverTurn.TabIndex = 90;
            this.InverTurn.Text = "轨迹跟踪初始化";
            this.InverTurn.UseVisualStyleBackColor = true;
            this.InverTurn.Click += new System.EventHandler(this.InverTurn_Click);
            // 
            // testbutton1
            // 
            this.testbutton1.Location = new System.Drawing.Point(6, 178);
            this.testbutton1.Name = "testbutton1";
            this.testbutton1.Size = new System.Drawing.Size(265, 33);
            this.testbutton1.TabIndex = 91;
            this.testbutton1.Text = "运行算法";
            this.testbutton1.UseVisualStyleBackColor = true;
            this.testbutton1.Click += new System.EventHandler(this.testbutton1_Click);
            // 
            // AnglePlot
            // 
            this.AnglePlot.Location = new System.Drawing.Point(432, 1);
            this.AnglePlot.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
            this.AnglePlot.Name = "AnglePlot";
            this.AnglePlot.Size = new System.Drawing.Size(456, 348);
            this.AnglePlot.TabIndex = 92;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(6, 78);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(265, 33);
            this.button1.TabIndex = 93;
            this.button1.Text = "清除数据";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // Duration
            // 
            this.Duration.DisplayMember = "1000, 500";
            this.Duration.FormattingEnabled = true;
            this.Duration.Items.AddRange(new object[] {
            "1",
            "6",
            "8",
            "10"});
            this.Duration.Location = new System.Drawing.Point(133, 222);
            this.Duration.Name = "Duration";
            this.Duration.Size = new System.Drawing.Size(43, 23);
            this.Duration.TabIndex = 100;
            this.Duration.TabStop = false;
            this.Duration.Text = "6";
            // 
            // Step
            // 
            this.Step.DisplayMember = "1000, 500";
            this.Step.FormattingEnabled = true;
            this.Step.Items.AddRange(new object[] {
            "1000",
            "500",
            "100",
            "50",
            "10"});
            this.Step.Location = new System.Drawing.Point(19, 252);
            this.Step.Name = "Step";
            this.Step.Size = new System.Drawing.Size(74, 23);
            this.Step.TabIndex = 101;
            this.Step.Text = "500";
            this.Step.ValueMember = "1000, 500";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(182, 225);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(22, 15);
            this.label4.TabIndex = 96;
            this.label4.Text = "秒";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(75, 225);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(52, 15);
            this.label3.TabIndex = 98;
            this.label3.Text = "总时长";
            // 
            // StartTime
            // 
            this.StartTime.Location = new System.Drawing.Point(6, 128);
            this.StartTime.Name = "StartTime";
            this.StartTime.Size = new System.Drawing.Size(265, 33);
            this.StartTime.TabIndex = 95;
            this.StartTime.Text = "设置时长";
            this.StartTime.UseVisualStyleBackColor = true;
            this.StartTime.Click += new System.EventHandler(this.StartTime_Click);
            // 
            // TorquePlot
            // 
            this.TorquePlot.Location = new System.Drawing.Point(432, 330);
            this.TorquePlot.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
            this.TorquePlot.Name = "TorquePlot";
            this.TorquePlot.Size = new System.Drawing.Size(456, 348);
            this.TorquePlot.TabIndex = 102;
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(85, 39);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(89, 36);
            this.button2.TabIndex = 103;
            this.button2.Text = "A1+";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(85, 82);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(89, 36);
            this.button3.TabIndex = 104;
            this.button3.Text = "A2+";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // button4
            // 
            this.button4.Location = new System.Drawing.Point(206, 39);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(89, 36);
            this.button4.TabIndex = 105;
            this.button4.Text = "A1-";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.button4_Click);
            // 
            // button5
            // 
            this.button5.Location = new System.Drawing.Point(206, 82);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(89, 36);
            this.button5.TabIndex = 106;
            this.button5.Text = "A2-";
            this.button5.UseVisualStyleBackColor = true;
            this.button5.Click += new System.EventHandler(this.button5_Click);
            // 
            // button6
            // 
            this.button6.Location = new System.Drawing.Point(85, 133);
            this.button6.Name = "button6";
            this.button6.Size = new System.Drawing.Size(89, 36);
            this.button6.TabIndex = 107;
            this.button6.Text = "B1+";
            this.button6.UseVisualStyleBackColor = true;
            this.button6.Click += new System.EventHandler(this.button6_Click);
            // 
            // button7
            // 
            this.button7.Location = new System.Drawing.Point(206, 133);
            this.button7.Name = "button7";
            this.button7.Size = new System.Drawing.Size(89, 36);
            this.button7.TabIndex = 108;
            this.button7.Text = "B1-";
            this.button7.UseVisualStyleBackColor = true;
            this.button7.Click += new System.EventHandler(this.button7_Click);
            // 
            // VelocityPlot
            // 
            this.VelocityPlot.Location = new System.Drawing.Point(2, 1);
            this.VelocityPlot.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
            this.VelocityPlot.Name = "VelocityPlot";
            this.VelocityPlot.Size = new System.Drawing.Size(456, 348);
            this.VelocityPlot.TabIndex = 109;
            this.VelocityPlot.Load += new System.EventHandler(this.formsPlot3_Load);
            // 
            // TimePlot
            // 
            this.TimePlot.Location = new System.Drawing.Point(2, 330);
            this.TimePlot.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
            this.TimePlot.Name = "TimePlot";
            this.TimePlot.Size = new System.Drawing.Size(456, 348);
            this.TimePlot.TabIndex = 110;
            // 
            // button8
            // 
            this.button8.Location = new System.Drawing.Point(85, 180);
            this.button8.Name = "button8";
            this.button8.Size = new System.Drawing.Size(89, 36);
            this.button8.TabIndex = 111;
            this.button8.Text = "B2+";
            this.button8.UseVisualStyleBackColor = true;
            this.button8.Click += new System.EventHandler(this.button8_Click);
            // 
            // button9
            // 
            this.button9.Location = new System.Drawing.Point(206, 180);
            this.button9.Name = "button9";
            this.button9.Size = new System.Drawing.Size(89, 36);
            this.button9.TabIndex = 112;
            this.button9.Text = "B2-";
            this.button9.UseVisualStyleBackColor = true;
            this.button9.Click += new System.EventHandler(this.button9_Click);
            // 
            // button10
            // 
            this.button10.Location = new System.Drawing.Point(85, 230);
            this.button10.Name = "button10";
            this.button10.Size = new System.Drawing.Size(89, 36);
            this.button10.TabIndex = 113;
            this.button10.Text = "C1+";
            this.button10.UseVisualStyleBackColor = true;
            this.button10.Click += new System.EventHandler(this.button10_Click);
            // 
            // button11
            // 
            this.button11.Location = new System.Drawing.Point(206, 230);
            this.button11.Name = "button11";
            this.button11.Size = new System.Drawing.Size(89, 36);
            this.button11.TabIndex = 114;
            this.button11.Text = "C1-";
            this.button11.UseVisualStyleBackColor = true;
            this.button11.Click += new System.EventHandler(this.button11_Click);
            // 
            // button12
            // 
            this.button12.Location = new System.Drawing.Point(85, 278);
            this.button12.Name = "button12";
            this.button12.Size = new System.Drawing.Size(89, 36);
            this.button12.TabIndex = 115;
            this.button12.Text = "C2+";
            this.button12.UseVisualStyleBackColor = true;
            this.button12.Click += new System.EventHandler(this.button12_Click);
            // 
            // button13
            // 
            this.button13.Location = new System.Drawing.Point(206, 278);
            this.button13.Name = "button13";
            this.button13.Size = new System.Drawing.Size(89, 36);
            this.button13.TabIndex = 116;
            this.button13.Text = "C2-";
            this.button13.UseVisualStyleBackColor = true;
            this.button13.Click += new System.EventHandler(this.button13_Click);
            // 
            // button16
            // 
            this.button16.Location = new System.Drawing.Point(47, 662);
            this.button16.Name = "button16";
            this.button16.Size = new System.Drawing.Size(820, 81);
            this.button16.TabIndex = 119;
            this.button16.Text = " 显 示 图 像";
            this.button16.UseVisualStyleBackColor = true;
            this.button16.Click += new System.EventHandler(this.button16_Click);
            // 
            // comboBoxPort
            // 
            this.comboBoxPort.FormattingEnabled = true;
            this.comboBoxPort.Location = new System.Drawing.Point(14, 78);
            this.comboBoxPort.Margin = new System.Windows.Forms.Padding(4);
            this.comboBoxPort.Name = "comboBoxPort";
            this.comboBoxPort.Size = new System.Drawing.Size(79, 23);
            this.comboBoxPort.TabIndex = 73;
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(19, 43);
            this.label12.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(67, 15);
            this.label12.TabIndex = 72;
            this.label12.Text = "串口号：";
            this.label12.Click += new System.EventHandler(this.label12_Click);
            // 
            // comboBox1
            // 
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Items.AddRange(new object[] {
            "1",
            "3",
            "6",
            "10"});
            this.comboBox1.Location = new System.Drawing.Point(301, 39);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(64, 23);
            this.comboBox1.TabIndex = 121;
            this.comboBox1.Text = "1";
            // 
            // comboBox2
            // 
            this.comboBox2.FormattingEnabled = true;
            this.comboBox2.Items.AddRange(new object[] {
            "1",
            "3",
            "6",
            "10"});
            this.comboBox2.Location = new System.Drawing.Point(301, 82);
            this.comboBox2.Name = "comboBox2";
            this.comboBox2.Size = new System.Drawing.Size(64, 23);
            this.comboBox2.TabIndex = 121;
            this.comboBox2.Text = "1";
            // 
            // comboBox3
            // 
            this.comboBox3.FormattingEnabled = true;
            this.comboBox3.Items.AddRange(new object[] {
            "1",
            "3",
            "6",
            "10"});
            this.comboBox3.Location = new System.Drawing.Point(301, 141);
            this.comboBox3.Name = "comboBox3";
            this.comboBox3.Size = new System.Drawing.Size(64, 23);
            this.comboBox3.TabIndex = 121;
            this.comboBox3.Text = "1";
            // 
            // comboBox4
            // 
            this.comboBox4.FormattingEnabled = true;
            this.comboBox4.Items.AddRange(new object[] {
            "1",
            "3",
            "6",
            "10"});
            this.comboBox4.Location = new System.Drawing.Point(301, 193);
            this.comboBox4.Name = "comboBox4";
            this.comboBox4.Size = new System.Drawing.Size(64, 23);
            this.comboBox4.TabIndex = 121;
            this.comboBox4.Text = "1";
            // 
            // comboBox5
            // 
            this.comboBox5.FormattingEnabled = true;
            this.comboBox5.Items.AddRange(new object[] {
            "1",
            "3",
            "6",
            "10"});
            this.comboBox5.Location = new System.Drawing.Point(301, 243);
            this.comboBox5.Name = "comboBox5";
            this.comboBox5.Size = new System.Drawing.Size(64, 23);
            this.comboBox5.TabIndex = 121;
            this.comboBox5.Text = "1";
            // 
            // comboBox6
            // 
            this.comboBox6.FormattingEnabled = true;
            this.comboBox6.Items.AddRange(new object[] {
            "1",
            "3",
            "6",
            "10"});
            this.comboBox6.Location = new System.Drawing.Point(301, 291);
            this.comboBox6.Name = "comboBox6";
            this.comboBox6.Size = new System.Drawing.Size(64, 23);
            this.comboBox6.TabIndex = 121;
            this.comboBox6.Text = "1";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(11, 47);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(23, 15);
            this.label1.TabIndex = 122;
            this.label1.Text = "d1";
            this.label1.Click += new System.EventHandler(this.label1_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(11, 90);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(23, 15);
            this.label2.TabIndex = 122;
            this.label2.Text = "d2";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(11, 141);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(23, 15);
            this.label5.TabIndex = 122;
            this.label5.Text = "d3";
            this.label5.Click += new System.EventHandler(this.label5_Click_1);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(11, 191);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(23, 15);
            this.label6.TabIndex = 122;
            this.label6.Text = "k1";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(11, 234);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(23, 15);
            this.label7.TabIndex = 122;
            this.label7.Text = "k2";
            this.label7.Click += new System.EventHandler(this.label7_Click);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(11, 287);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(23, 15);
            this.label8.TabIndex = 122;
            this.label8.Text = "k3";
            // 
            // directorySearcher1
            // 
            this.directorySearcher1.ClientTimeout = System.TimeSpan.Parse("-00:00:01");
            this.directorySearcher1.ServerPageTimeLimit = System.TimeSpan.Parse("-00:00:01");
            this.directorySearcher1.ServerTimeLimit = System.TimeSpan.Parse("-00:00:01");
            // 
            // button14
            // 
            this.button14.Location = new System.Drawing.Point(176, 27);
            this.button14.Name = "button14";
            this.button14.Size = new System.Drawing.Size(95, 45);
            this.button14.TabIndex = 123;
            this.button14.Text = "示教";
            this.button14.UseVisualStyleBackColor = true;
            this.button14.Click += new System.EventHandler(this.button14_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.BackColor = System.Drawing.SystemColors.Info;
            this.groupBox1.Controls.Add(this.openButton);
            this.groupBox1.Controls.Add(this.Step);
            this.groupBox1.Controls.Add(this.label12);
            this.groupBox1.Controls.Add(this.comboBoxPort);
            this.groupBox1.Controls.Add(this.buttonFresh);
            this.groupBox1.Controls.Add(this.comboBoxBaudrate);
            this.groupBox1.Controls.Add(this.DataBit);
            this.groupBox1.Controls.Add(this.CheckBit);
            this.groupBox1.Controls.Add(this.StopBit);
            this.groupBox1.Controls.Add(this.send);
            this.groupBox1.Controls.Add(this.Disenble);
            this.groupBox1.Location = new System.Drawing.Point(1311, 4);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(248, 369);
            this.groupBox1.TabIndex = 124;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "连接设备";
            this.groupBox1.Enter += new System.EventHandler(this.groupBox1_Enter);
            // 
            // groupBox2
            // 
            this.groupBox2.BackColor = System.Drawing.SystemColors.Info;
            this.groupBox2.Controls.Add(this.button2);
            this.groupBox2.Controls.Add(this.freshCAN_richBox);
            this.groupBox2.Controls.Add(this.Turn);
            this.groupBox2.Controls.Add(this.label8);
            this.groupBox2.Controls.Add(this.button3);
            this.groupBox2.Controls.Add(this.label7);
            this.groupBox2.Controls.Add(this.button4);
            this.groupBox2.Controls.Add(this.label6);
            this.groupBox2.Controls.Add(this.button5);
            this.groupBox2.Controls.Add(this.label5);
            this.groupBox2.Controls.Add(this.button6);
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Controls.Add(this.button7);
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Controls.Add(this.button8);
            this.groupBox2.Controls.Add(this.comboBox6);
            this.groupBox2.Controls.Add(this.button9);
            this.groupBox2.Controls.Add(this.comboBox5);
            this.groupBox2.Controls.Add(this.button10);
            this.groupBox2.Controls.Add(this.comboBox4);
            this.groupBox2.Controls.Add(this.button11);
            this.groupBox2.Controls.Add(this.comboBox3);
            this.groupBox2.Controls.Add(this.button12);
            this.groupBox2.Controls.Add(this.comboBox2);
            this.groupBox2.Controls.Add(this.button13);
            this.groupBox2.Controls.Add(this.comboBox1);
            this.groupBox2.Location = new System.Drawing.Point(1179, 379);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(380, 380);
            this.groupBox2.TabIndex = 125;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "参数调试";
            // 
            // groupBox3
            // 
            this.groupBox3.BackColor = System.Drawing.SystemColors.Info;
            this.groupBox3.Controls.Add(this.InverTurn);
            this.groupBox3.Controls.Add(this.testbutton1);
            this.groupBox3.Controls.Add(this.button1);
            this.groupBox3.Controls.Add(this.button14);
            this.groupBox3.Controls.Add(this.StartTime);
            this.groupBox3.Controls.Add(this.label3);
            this.groupBox3.Controls.Add(this.label4);
            this.groupBox3.Controls.Add(this.Duration);
            this.groupBox3.Location = new System.Drawing.Point(890, 379);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(283, 380);
            this.groupBox3.TabIndex = 126;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "运行设置";
            // 
            // groupBox4
            // 
            this.groupBox4.BackColor = System.Drawing.Color.SeaShell;
            this.groupBox4.Controls.Add(this.CAN_richBox);
            this.groupBox4.Location = new System.Drawing.Point(890, 4);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(414, 369);
            this.groupBox4.TabIndex = 127;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "提示";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
            this.ClientSize = new System.Drawing.Size(1571, 755);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.button16);
            this.Controls.Add(this.TimePlot);
            this.Controls.Add(this.VelocityPlot);
            this.Controls.Add(this.TorquePlot);
            this.Controls.Add(this.AnglePlot);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Timer timer1;
        private System.IO.Ports.SerialPort serialPort;
        private System.Windows.Forms.ComboBox StopBit;
        private System.Windows.Forms.ComboBox CheckBit;
        private System.Windows.Forms.ComboBox DataBit;
        private System.Windows.Forms.ComboBox comboBoxBaudrate;
        private System.Windows.Forms.Button buttonFresh;
        private System.Windows.Forms.Button openButton;
        private System.Windows.Forms.Button freshCAN_richBox;
        private System.Windows.Forms.Button send;
        private System.Windows.Forms.Button Disenble;
        private System.Windows.Forms.RichTextBox CAN_richBox;
        private System.Windows.Forms.Button Turn;
        private System.Windows.Forms.Button InverTurn;
        private System.Windows.Forms.Button testbutton1;
        private ScottPlot.FormsPlot AnglePlot;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.ComboBox Duration;
        private System.Windows.Forms.ComboBox Step;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button StartTime;
        private ScottPlot.FormsPlot TorquePlot;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.Button button5;
        private System.Windows.Forms.Button button6;
        private System.Windows.Forms.Button button7;
        private ScottPlot.FormsPlot VelocityPlot;
        private ScottPlot.FormsPlot TimePlot;
        private System.Windows.Forms.Button button8;
        private System.Windows.Forms.Button button9;
        private System.Windows.Forms.Button button10;
        private System.Windows.Forms.Button button11;
        private System.Windows.Forms.Button button12;
        private System.Windows.Forms.Button button13;
        private System.Windows.Forms.Button button16;
        private System.Windows.Forms.ComboBox comboBoxPort;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.ComboBox comboBox2;
        private System.Windows.Forms.ComboBox comboBox3;
        private System.Windows.Forms.ComboBox comboBox4;
        private System.Windows.Forms.ComboBox comboBox5;
        private System.Windows.Forms.ComboBox comboBox6;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        private System.DirectoryServices.DirectorySearcher directorySearcher1;
        private System.Windows.Forms.Button button14;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.GroupBox groupBox4;
    }
}

