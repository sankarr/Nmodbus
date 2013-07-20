using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Modbus;
using Modbus.Device;
using Modbus.Data;
using Modbus.IO;
using System.IO.Ports;
using System.Threading;


namespace ReadModbusCommands
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            comboBox1.DataSource = SerialPort.GetPortNames();
            //this.FormClosing += new FormClosingEventHandler(Form1_FormClosing);
            //this.FormClosed += new FormClosedEventHandler(Form1_FormClosed);
        }

        void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            slave.stop = true;
        }

        void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if(slave!=null)
                slave.stop = true;
            //t1.Join();
            //this.Close();
        }
        ModbusSlave slave;

        void doStuff()
        {

            DataStore data52 = DataStoreFactory.CreateDefaultDataStore();
            Dictionary<byte, DataStore> map = new Dictionary<byte, DataStore>();

            byte unitID52 = 52;

            map.Add(unitID52, data52);
            string portname = "";

            if (InvokeRequired)
            {
                this.Invoke((MethodInvoker)delegate() { portname = comboBox1.SelectedItem.ToString(); });
            }


            using (SerialPort com = new SerialPort(portname, 9600, Parity.None, 8, StopBits.One))
            {
                com.Open();
                slave = ModbusSerialSlave.CreateRtu(map, com);
                //slave.stop = false;
                slave.Listen();

            }

        }

        Thread t1;
        private void button1_Click(object sender, EventArgs e)
        {
            t1 = new Thread(new ThreadStart(doStuff));
            t1.Start();


        }
    }
}
