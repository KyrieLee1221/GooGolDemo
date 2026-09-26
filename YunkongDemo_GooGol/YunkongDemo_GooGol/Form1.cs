using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static gts.mc;

namespace YunkongDemo_GooGol
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private bool isFree = true;  // 控制模式，点动，jog，插补，每次开启的时候先判断，为true才能启动
        private short AXIS;  // 轴号
        private short Rtn; // 方法调用的状态
        private bool isConn = false; //卡是否连接


        private bool bFlagAlarm = false; //驱动报警标记
        private bool bFlagMError = false; // 运动出错标志
        private bool bFlagPosLimit = false; // 正向限位标志
        private bool bFlagNegLimit = false; // 负向限位标志
        private bool bFlagServoOn = false; // 伺服使能标志
        private bool bFlagSmoothStop = false; // 平滑停止标志
        private bool bFlagAbruptStop = false; // 急停标志
        private bool bFlagMotion = false; // 运动状态标志


        private void InitAxisStatus()
        {
            if (!isConn)
            {
                return;
            }
            AXIS = Convert.ToInt16(comboBox1.Text);
            GT_GetSts(AXIS,out int AxisStatus,1,out uint pClock); // 第三个参数是连续读几个轴，第四个是当前系统时钟
            if ((AxisStatus & 0x2) !=0)
            {
                bFlagAlarm = true;
                panel1.BackColor = Color.Red;
            }
            else
            {
                bFlagAlarm = false;
                panel1.BackColor = Color.Green;
            }
            // 运动出错
            bFlagMError= (AxisStatus & 0x10)!=0?true:false;
            panel3.BackColor = bFlagMError ? Color.Red:Color.Green;
            // 正向限位
            bFlagPosLimit = (AxisStatus & 0x20) != 0 ? true : false;
            panel4.BackColor = bFlagPosLimit ? Color.Red : Color.Green;
            //负向限位
            bFlagNegLimit = (AxisStatus & 0x40) != 0 ? true : false;
            panel2.BackColor = bFlagNegLimit ? Color.Red : Color.Green;
            // 伺服使能
            bFlagServoOn = (AxisStatus & 0x200) != 0 ? true : false;
            panel8.BackColor = bFlagServoOn ? Color.Red : Color.Green;
            // 平滑停止
            bFlagSmoothStop = (AxisStatus & 0x80) != 0 ? true : false;
            panel6.BackColor = bFlagSmoothStop ? Color.Red : Color.Green;
            //急停
            bFlagAbruptStop = (AxisStatus & 0x100) != 0 ? true : false;
            panel5.BackColor = bFlagAbruptStop ? Color.Red : Color.Green;
            // 运动状态标志
            bFlagMotion = (AxisStatus & 0x400) != 0 ? true : false;
            panel7.BackColor = bFlagMotion ? Color.Red : Color.Green;
        }


        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                comboBox1.SelectedIndex = 0;
                textBox1.Enabled = false;
                textBox2.Enabled = false;
                textBox3.Enabled = false;
                textBox4.Enabled = false;
                Rtn = GT_Open(0, 1);
                if (Rtn == 0)
                {
                    isConn = true;
                    LEDTimer.Interval = 500;
                    LEDTimer.Start();
                }
                else
                {
                    isConn = false;
                    MessageBox.Show($"打开控制卡失败，错误码：{Rtn}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("打开运动控制器失败："+ex.Message);
            }
        }

        // 初始化
        private void InitBtn_Click(object sender, EventArgs e)
        {
            if (!isConn)
            {
                MessageBox.Show("控制卡未连接，无法初始化");
                return;
            }
            AXIS = Convert.ToInt16(comboBox1.Text);
            Rtn = GT_LoadConfig("GTS400.cfg");
            if (Rtn!=0)
            {
                MessageBox.Show($"调用的指令状态异常，指定是:GT_LoadConfig；状态码是{Rtn}");
                return;
            }
            AxisTimer.Interval = 100;
            AxisTimer.Start();
            timer2.Interval = 20;
            timer2.Start();
        }

        // 清除状态
        private void ClearStausBtn_Click(object sender, EventArgs e)
        {
            AXIS = Convert.ToInt16(comboBox1.Text);
            Rtn = GT_ClrSts(AXIS,8);
            if (Rtn != 0)
            {
                MessageBox.Show($"调用的指令状态异常，指令是:GT_ClrSts；状态异常，状态码是{Rtn}");
                return;
            }
            
        }


        private bool servoToggle = false; // 新增：false=未使能，true=已使能
        // 伺服使能
        private void ServoEnableBtn_Click(object sender, EventArgs e)
        {
            AXIS = Convert.ToInt16(comboBox1.Text);
            if (!servoToggle)
            {
                Rtn = GT_AxisOn(AXIS); // 关闭伺服使能
                if (Rtn != 0)
                {
                    MessageBox.Show($"调用的指令状态异常，指令是:GT_AxisOff；状态异常，状态码是{Rtn}");
                    return;
                }
                servoToggle = true;
                ServoEnableBtn.Text = "伺服已使能";

            }
            else
            {
                Rtn = GT_AxisOff(AXIS);
                if (Rtn != 0)
                {
                    MessageBox.Show($"调用的指令状态异常，指令是:GT_AxisOn；状态异常，状态码是{Rtn}");
                    return;
                }
                bFlagServoOn =false;
                ServoEnableBtn.Text = "伺服使能";
            }
        }

        // 位置清零
         private void PositionResetBtn_Click(object sender, EventArgs e)
        {
            AXIS = Convert.ToInt16(comboBox1.Text);
            Rtn = GT_ZeroPos(AXIS,8);
            if (Rtn != 0)
            {
                MessageBox.Show($"调用的指令状态异常，指令是:GT_ZeroPos；状态异常，状态码是{Rtn}");
                return;
            }
        }
        
        //轴状态检测
        private void AxisTimer_Tick(object sender, EventArgs e)
        {
            InitAxisStatus();
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            AXIS = Convert.ToInt16(comboBox1.Text);
            // 读取AXIS轴的规划位置
            Rtn = GT_GetPrfPos(AXIS, out double pvalue, 1, out uint pclock);

            // 读取实际位置
            Rtn = GT_GetEncPos(AXIS, out double pvalue1, 1, out uint pclock1);

            textBox1.Text=pvalue.ToString();
            textBox3.Text=pvalue1.ToString();

            // 读取规划速度
            Rtn = GT_GetPrfVel(AXIS, out double pvalue2, 1, out uint pclock2);

            //读取实际速度 
            Rtn = GT_GetEncVel(AXIS, out double pvalue3, 1, out uint pclock3);

            textBox2.Text=pvalue2.ToString();
            textBox4.Text=pvalue3.ToString();
        }

        // 启动点位运动
        private void ActivatePointStart_Click(object sender, EventArgs e)
        {
            if (!isFree)
            {
                MessageBox.Show("设备正在进行别的运动，请先停止后重试");
                return;
            }
            isFree = false;
            Rtn = GT_PrfTrap(AXIS);
            if (Rtn != 0)
            {
                MessageBox.Show($"调用的指令状态异常，指令是:GT_PrfTrap；状态异常，状态码是{Rtn}");
                return;
            }
        }

        // 开始点位运动
        private void StartBtn_Click(object sender, EventArgs e)
        {
            AXIS = Convert.ToInt16(comboBox1.Text);
            TTrapPrm trapPrm = new TTrapPrm()
            {
                acc = 0.25,
                dec = 0.25,
                velStart = 0,
                smoothTime = 25
            };
            Rtn = GT_SetTrapPrm(AXIS,ref trapPrm); // 设置点位运动的参数
            if (Rtn != 0)
            {
                MessageBox.Show($"调用的指令状态异常，指令是:GT_SetTrapPrm；状态异常，状态码是{Rtn}");
                return;
            }
            Rtn = GT_SetPos(AXIS, Convert.ToInt32(textBox5.Text)); // 设置点位运动的位置
            if (Rtn != 0)
            {
                MessageBox.Show($"调用的指令状态异常，指令是:GT_SetPos；状态异常，状态码是{Rtn}");
                return;
            }
            Rtn = GT_SetVel(AXIS,Convert.ToDouble(textBox6.Text));
            if (Rtn != 0)
            {
                MessageBox.Show($"调用的指令状态异常，指令是:GT_SetVel；状态异常，状态码是{Rtn}");
                return;
            }
            Rtn = GT_Update(1<<(AXIS-1));
            if (Rtn != 0)
            {
                MessageBox.Show($"调用的指令状态异常，指令是:GT_Update；状态异常，状态码是{Rtn}");
                return;
            }
        }

        // 停止点位运动
        private void StopBtn_Click(object sender, EventArgs e)
        {
            AXIS = Convert.ToInt16(comboBox1.Text);
            Rtn = GT_Stop(AXIS - 1, 0); //停止点位运动
            if (Rtn != 0)
            {
                MessageBox.Show($"调用的指令状态异常，指令是:GT_Stop；状态异常，状态码是{Rtn}");
                return;
            }
            isFree = true;
        }


        // 启动JOG运动
        private void JogBtn_Click(object sender, EventArgs e)
        {
            AXIS = Convert.ToInt16(comboBox1.Text);
            if (!isFree)
            {
                MessageBox.Show("设备正在进行别的运动，请先停止后重试");
                return;
            }
            isFree = false;
            Rtn = GT_PrfJog(AXIS);
            if (Rtn != 0)
            {
                MessageBox.Show($"调用的指令状态异常，指令是:GT_PrfJog；状态异常，状态码是{Rtn}");
                return;
            }
        }

        #region jog运动正向移动
        private void button2_MouseDown(object sender, MouseEventArgs e)
        {
            AXIS = Convert.ToInt16(comboBox1.Text);
            TJogPrm prm = new TJogPrm()
            {
                acc=0.25,
                dec=0.25
            };
            Rtn = GT_SetJogPrm(AXIS, ref prm);
            if (Rtn != 0)
            {
                MessageBox.Show($"调用的指令状态异常，指令是:GT_SetJogPrm；状态异常，状态码是{Rtn}");
                return;
            }
            Rtn = GT_SetVel(AXIS, Convert.ToDouble(textBox7.Text));
            if (Rtn != 0)
            {
                MessageBox.Show($"调用的指令状态异常，指令是:GT_SetVel；状态异常，状态码是{Rtn}");
                return;
            }
            Rtn = GT_Update(1 << (AXIS - 1));
            if (Rtn != 0)
            {
                MessageBox.Show($"调用的指令状态异常，指令是:GT_Update；状态异常，状态码是{Rtn}");
                return;
            }
        }

        private void button2_MouseUp(object sender, MouseEventArgs e)
        {
            AXIS = Convert.ToInt16(comboBox1.Text);
            Rtn = GT_Stop(1 << (AXIS - 1), 0);
            if (Rtn != 0)
            {
                MessageBox.Show($"调用的指令状态异常，指令是:GT_Stop；状态异常，状态码是{Rtn}");
                return;
            }
            isFree = true;
        }
        #endregion

        #region jog运动负向移动

        private void button1_MouseDown(object sender, MouseEventArgs e)
        {
            AXIS = Convert.ToInt16(comboBox1.Text);
            TJogPrm prm = new TJogPrm()
            {
                acc = 0.25,
                dec = 0.25
            };
            Rtn = GT_SetJogPrm(AXIS, ref prm);
            if (Rtn != 0)
            {
                MessageBox.Show($"调用的指令状态异常，指令是:GT_SetJogPrm；状态异常，状态码是{Rtn}");
                return;
            }
            Rtn = GT_SetVel(AXIS, -Convert.ToDouble(textBox7.Text));
            if (Rtn != 0)
            {
                MessageBox.Show($"调用的指令状态异常，指令是:GT_SetVel；状态异常，状态码是{Rtn}");
                return;
            }
            Rtn = GT_Update(1 << (AXIS - 1));
            if (Rtn != 0)
            {
                MessageBox.Show($"调用的指令状态异常，指令是:GT_Update；状态异常，状态码是{Rtn}");
                return;
            }
        }

        private void button1_MouseUp(object sender, MouseEventArgs e)
        {
            AXIS = Convert.ToInt16(comboBox1.Text);
            AXIS = Convert.ToInt16(comboBox1.Text);
            Rtn = GT_Stop(1 << (AXIS - 1), 0);
            if (Rtn != 0)
            {
                MessageBox.Show($"调用的指令状态异常，指令是:GT_Stop；状态异常，状态码是{Rtn}");
                return;
            }
            isFree = true;
        }
        #endregion

        // 建立坐标系
        private void SetCSbtn_Click(object sender, EventArgs e)
        {
            if (!isFree)
            {
                MessageBox.Show("设备正在进行别的运动，请先停止后重试");
                return;
            }
            isFree = false;
            Rtn = GT_ClrSts(1, 8); // 清除 1~8 轴的报警和限位
            if (Rtn != 0)
            {
                MessageBox.Show($"调用的指令状态异常，指令是:GT_ClrSts；状态异常，状态码是{Rtn}");
                return;
            }
            TCrdPrm prm = new TCrdPrm()
            {
                dimension = 2,
                profile1 = 1, //对应x轴
                profile2 = 2, //对应y轴
                synAccMax = 3, //设置最大加速度
                synVelMax = 500, // 设置最大速度
                evenTime = 50, //最小匀速时间
                setOriginFlag = 1,
                originPos1 = 100,
                originPos2 = 100
            };
            Rtn = GT_SetCrdPrm(1, ref prm);
            if (Rtn != 0)
            {
                MessageBox.Show($"调用的指令状态异常，指令是:GT_SetCrdPrm；状态异常，状态码是{Rtn}");
                return;
            }
            Rtn = GT_AxisOn(1); // 使能轴1（X）
            if (Rtn != 0)
            {
                MessageBox.Show($"调用的指令状态异常，指令是:GT_AxisOn；状态异常，状态码是{Rtn}");
                return;
            }
            Rtn = GT_AxisOn(2); // 使能轴2（Y）
            if (Rtn != 0)
            {
                MessageBox.Show($"调用的指令状态异常，指令是:GT_AxisOn；状态异常，状态码是{Rtn}");
                return;
            }
            isFree = true; // 建立完成后恢复，否则后续点位/JOG会被 isFree 拦截
        }

        // 绘制线
        private void GetLineBtn_Click(object sender, EventArgs e)
        {
            // 即将把数据存入坐标系1的FIF00中，所以要首先清楚此缓存区中的数据
            Rtn = GT_CrdClear(1, 0);
            if (Rtn != 0)
            {
                MessageBox.Show($"调用的指令状态异常，指令是:GT_CrdClear；状态异常，状态码是{Rtn}");
                return;
            }
            // 像缓存区中写入第一段插补数据  从当前位置 → (20000, 0)
            // 参数含义： 插补段的坐标系是1； 终点坐标是（20000，0）； 目标速度是：100 pulse/ms； 加速度 0.1 pulse/ms^2；终点速度为0；像坐标系1的FIF00缓存区传递该直线插补数据
            Rtn = GT_LnXY(1, 20000, 0, 100, 0.1, 0, 0);
            if (Rtn != 0)
            {
                MessageBox.Show($"调用的指令状态异常，指令是:GT_LnXY；状态异常，状态码是{Rtn}");
                return;
            }
            Rtn = GT_CrdSpace(1, out int space, 0); // 查询缓存并启动插补运动
            GT_CrdStart(1,0);
            if (Rtn != 0)
            {
                MessageBox.Show($"调用的指令状态异常，指令是:GT_CrdStart；状态异常，状态码是{Rtn}");
                return;
            }
        }

        // 画弧长
        private void DrawAnArc_Click(object sender, EventArgs e)
        {
            Rtn = GT_CrdClear(1, 0);
            if (Rtn != 0)
            {
                MessageBox.Show($"调用的指令状态异常，指令是:GT_CrdClear；状态异常，状态码是{Rtn}");
                return;
            }
            // 起点：圆最右侧 (10000, 0)
            Rtn = GT_LnXY(1, 10000, 0, 100, 0.1, 0, 0);

            // 圆弧：终点在正上方 (0, 10000)，半径 10000，逆时针
            Rtn = GT_ArcXYR(1, 0, 10000, 10000, 0, 100, 0.1, 0, 0);

            // 查询缓存并启动
            Rtn = GT_CrdSpace(1, out int space, 0);
            Rtn = GT_CrdStart(1, 0);
        }

        // 绘制圆
        private void DrawACircle_Click(object sender, EventArgs e)
        {
            Rtn = GT_CrdClear(1, 0);
            if (Rtn != 0)
            {
                MessageBox.Show($"调用的指令状态异常，指令是:GT_CrdClear；状态异常，状态码是{Rtn}");
                return;
            }

            // 起点：圆最右侧
            Rtn = GT_LnXY(1, 10000, 0, 100, 0.1, 0, 0);

            // 上半圆：从 (10000,0) 顺时针到 (-10000,0)
            Rtn = GT_ArcXYR(1, -10000, 0, 10000, 1, 100, 0.1, 0, 0);

            // 下半圆：从 (-10000,0) 顺时针回到 (10000,0)
            Rtn = GT_ArcXYR(1, 10000, 0, 10000, 1, 100, 0.1, 0, 0);

            // 查询缓存并启动
            Rtn = GT_CrdSpace(1, out int space, 0);
            Rtn = GT_CrdStart(1, 0);
        }

        // 三角形
        private void DrawATriangle_Click(object sender, EventArgs e)
        {
            Rtn = GT_CrdClear(1, 0);
            if (Rtn != 0)
            {
                MessageBox.Show($"调用的指令状态异常，指令是:GT_CrdClear；状态异常，状态码是{Rtn}");
                return;
            }

            // 起点：底部左顶点
            Rtn = GT_LnXY(1, -10000, -10000, 100, 0.1, 0, 0);

            // 底部右顶点
            Rtn = GT_LnXY(1, 10000, -10000, 100, 0.1, 0, 0);

            // 顶部顶点
            Rtn = GT_LnXY(1, 0, 10000, 100, 0.1, 0, 0);

            // 回到起点，闭合三角形
            Rtn = GT_LnXY(1, -10000, -10000, 100, 0.1, 0, 0);

            // 查询缓存并启动
            Rtn = GT_CrdSpace(1, out int space, 0);
            Rtn = GT_CrdStart(1, 0);
        }   

        // 矩形
        private void DrawRectangle_Click(object sender, EventArgs e)
        {
            Rtn = GT_CrdClear(1, 0);

            // 起点：左下角
            Rtn = GT_LnXY(1, 0, 0, 100, 0.1, 0, 0);

            // 右下角
            Rtn = GT_LnXY(1, 20000, 0, 100, 0.1, 0, 0);

            // 右上角
            Rtn = GT_LnXY(1, 20000, 20000, 100, 0.1, 0, 0);

            // 左上角
            Rtn = GT_LnXY(1, 0, 20000, 100, 0.1, 0, 0);

            // 回到起点，闭合正方形
            Rtn = GT_LnXY(1, 0, 0, 100, 0.1, 0, 0);

            // 查询缓存并启动
            Rtn = GT_CrdSpace(1, out int space, 0);
            Rtn = GT_CrdStart(1, 0);
        }

        //五角星
        private void DrawAPentagram_Click(object sender, EventArgs e)
        {
            // 绘制五角星
            Rtn = GT_CrdClear(1, 0);

            // 起点：五角星最上方顶点
            Rtn = GT_LnXY(1, 0, 30000, 100, 0.1, 0, 0);

            // 右上：连到右内凹点
            Rtn = GT_LnXY(1, 7000, 10000, 100, 0.1, 0, 0);

            // 右外顶点
            Rtn = GT_LnXY(1, 28000, 10000, 100, 0.1, 0, 0);

            // 右下内凹点
            Rtn = GT_LnXY(1, 11000, -4000, 100, 0.1, 0, 0);

            // 右下外顶点
            Rtn = GT_LnXY(1, 18000, -25000, 100, 0.1, 0, 0);

            // 底部内凹点
            Rtn = GT_LnXY(1, 0, -10000, 100, 0.1, 0, 0);

            // 左下外顶点
            Rtn = GT_LnXY(1, -18000, -25000, 100, 0.1, 0, 0);

            // 左下内凹点
            Rtn = GT_LnXY(1, -11000, -4000, 100, 0.1, 0, 0);

            // 左外顶点
            Rtn = GT_LnXY(1, -28000, 10000, 100, 0.1, 0, 0);

            // 左上内凹点
            Rtn = GT_LnXY(1, -7000, 10000, 100, 0.1, 0, 0);

            // 回到起点，闭合五角星
            Rtn = GT_LnXY(1, 0, 30000, 100, 0.1, 0, 0);

            // 查询缓存并启动
            Rtn = GT_CrdSpace(1, out int space, 0);
            Rtn = GT_CrdStart(1, 0);
        }

        private bool boardLedOn = false;   
        // 红绿灯
        private void button3_Click(object sender, EventArgs e)
        {
            if (!isConn)
            {
                MessageBox.Show("控制卡没连上");
                return;
            }
            boardLedOn = !boardLedOn;
            Rtn = GT_SetDo(MC_GPO,boardLedOn?0x01:0x00);
            if (Rtn != 0)
            {
                MessageBox.Show($"调用的指令状态异常，指令是:GT_SetDo；状态异常，状态码是{Rtn}");
                return;
            }
        }

        private bool ledToggle = false;
        private void LEDTimer_Tick(object sender, EventArgs e)
        {
            Rtn = GT_GetDi(MC_GPI, out int pValue);
            if ((pValue & 0x01) == 0)
            {
                ledToggle = !ledToggle;
                led1.BackColor = ledToggle ? Color.Green : Color.Gray;
                led2.BackColor = ledToggle ? Color.Green : Color.Gray;
            }
            else
            {
                led1.BackColor= Color.Gray;
                led2.BackColor = Color.Gray;
                ledToggle = false;
            }
        }
    }
}
