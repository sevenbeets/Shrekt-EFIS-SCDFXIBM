using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;

namespace BitsBolts
{
    static class Program
    {
        static Form1 form;
        static MqttClient client;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            form = new Form1();
            
            // MQTT+
            // create client instance 
            client = new MqttClient("172.17.190.38");

            // register to message received 
            client.MqttMsgPublishReceived += client_MqttMsgPublishReceived;

            string clientId = "ui";

            client.Connect(clientId);
            //Listen to backend server for processed images.
            client.Subscribe(new string[] { "device/status" }, new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });
            //Listen to IoT for rodent activity.
            client.Subscribe(new string[] { "device/fire" }, new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });
            //Listen to IoT for rodent activity.
            client.Subscribe(new string[] { "device/temp" }, new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });
            Application.Run(form);
            client.Disconnect();
        }

        private static void client_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
        {
            // handle message received
            if (e.Topic == "device/status")
            {
                // camera number
                String status_live = System.Text.Encoding.Default.GetString(e.Message);
                form.StatusUpdate(Int32.Parse(status_live));
            }
            else if (e.Topic == "device/fire")
            {
                // format - sensorNum ON/OFF
                String status_fire = System.Text.Encoding.Default.GetString(e.Message);
                form.FireUpdate(Int32.Parse(status_fire));
            }
            else 
            {
                float status_temp = float.Parse(System.Text.Encoding.Default.GetString(e.Message));
                form.TempUpdate(status_temp);
            }         
        }
    }
}
