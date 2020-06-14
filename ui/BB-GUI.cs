using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using GMap.NET;
using GMap.NET.WindowsForms;
using GMap.NET.WindowsForms.Markers;

namespace BitsBolts
{
    public partial class Form1 : Form
    {
        System.Timers.Timer timer1;

        public Form1()
        {
            InitializeComponent();
        }

        bool flag_offline = false;

        GMapMarker marker = new GMarkerGoogle(new PointLatLng(1.334327, 103.893821), GMarkerGoogleType.red_dot);
        public void StatusUpdate(Int32 Status)
        {
            if (Status == 1)
            {
                label5.Invoke(new Action(() => label5.Text = "Online"));
                label5.Invoke(new Action(() => label5.ForeColor = Color.Green));
                label4.Invoke(new Action(() => label4.Visible = true));
                label6.Invoke(new Action(() => label6.Visible = true));
                Online_Polygon();
                timer1.Stop();
                timer1.Start();
                flag_offline = false;
            }
        }

        private void offline(object sender, EventArgs e)
        {
            label5.Invoke(new Action(() => label5.Text = "Offline"));
            label5.Invoke(new Action(() => label5.ForeColor = Color.Red));
            label4.Invoke(new Action(() => label4.Visible = false));
            label6.Invoke(new Action(() => label6.Visible = false));
            label7.Invoke(new Action(() => label7.Visible = false));
            label6.Invoke(new Action(() => label6.Text = "Default"));
            flag_offline = true;
            Offline_Polygon();
        }

        // Update sensors (only sensor 16 for demo purposes)
        public void FireUpdate(Int32 Fire)
        {
            if (Fire == 1 && flag_offline == false)
            {
                Fire_Polygon();
                label7.Invoke(new Action(() => label7.Visible = true));
            }
            else if (Fire == 0 && flag_offline == false)
            {
                label7.Invoke(new Action(() => label7.Visible = false));
                Online_Polygon();
            }
        }

        public void TempUpdate(float temp)
        {
            label6.Invoke(new Action(() => label6.Text = temp.ToString() + " °C"));
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            timer1 = new System.Timers.Timer(2000);
            timer1.Elapsed += offline;
            timer1.Enabled = true;

            gmap.MapProvider = GMap.NET.MapProviders.GoogleMapProvider.Instance;
            GMap.NET.GMaps.Instance.Mode = GMap.NET.AccessMode.ServerOnly;
            gmap.SetPositionByKeywords("Singapore,Singapore");
            gmap.Position = new GMap.NET.PointLatLng(1.334327, 103.893821); //1.3478° N, 103.6817° E
            gmap.Zoom = 11;
            gmap.ShowCenter = false;
            GMapOverlay markers = new GMapOverlay("markers");
            markers.Markers.Add(marker);
            gmap.Overlays.Add(markers);
            
            polygons.Polygons.Add(polygon);
            gmap.Overlays.Add(polygons);
            Default_Polygon();
        }

        private static List<PointLatLng> thisss() {

            List<PointLatLng> points = new List<PointLatLng>();
            points.Add(new PointLatLng(1.334280, 103.893322));
            points.Add(new PointLatLng(1.334098, 103.893365));
            points.Add(new PointLatLng(1.333951, 103.893065));
            points.Add(new PointLatLng(1.333803, 103.893143));
            points.Add(new PointLatLng(1.334181, 103.893983));
            points.Add(new PointLatLng(1.334398, 103.893876));
            points.Add(new PointLatLng(1.334264, 103.893525));
            points.Add(new PointLatLng(1.334352, 103.893482));

            return points;
        }

        GMapOverlay polygons = new GMapOverlay("polygons");
        GMapPolygon polygon = new GMapPolygon(thisss(), "SCDF HQ");

        private void Offline_Polygon()
        {
            polygon.Fill = new SolidBrush(Color.FromArgb(50, Color.Red));
            polygon.Stroke = new Pen(Color.Red, 1);
        }


        private void Default_Polygon()
        {
            polygon.Fill = new SolidBrush(Color.FromArgb(50, Color.Transparent));
            polygon.Stroke = new Pen(Color.Transparent, 1);
        }


        private void Online_Polygon()
        {
            polygon.Fill = new SolidBrush(Color.FromArgb(50, Color.Green));
            polygon.Stroke = new Pen(Color.Green, 1);
        }

        private void Fire_Polygon()
        {
            polygon.Fill = new SolidBrush(Color.FromArgb(50, Color.Orange));
            polygon.Stroke = new Pen(Color.Orange, 1);
        }

        private void gmap_OnMarkerEnter(GMapMarker item)
        {
            update_ToolTip();
        }

        private void update_ToolTip()
        {
            if (label6.Text.ToString() != "Default")
            {
                marker.ToolTipText = "\n" + "Current temperature being measured: " + label6.Text.ToString() + "\n";
            }
            else
            {
                marker.ToolTipText = "\n" + "Current temperature unmeasured as IoT device is Offline " + "\n";
            }

        }

        private void gmap_OnMarkerClick(GMapMarker item, MouseEventArgs e)
        {
            groupBox1.Visible = true;
        }

        private void gmap_Click(object sender, EventArgs e)
        {
            groupBox1.Visible = false;
        }
    }
}