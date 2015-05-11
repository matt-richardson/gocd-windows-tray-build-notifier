using System;
using System.Drawing;
using System.Windows.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ToastNotifications;
using WebSocket4Net;

namespace go_cd_windows_tray_build_notifier
{
    public partial class SysTrayApp : Form
    {
        private NotifyIcon trayIcon;
        private ContextMenu trayMenu;

        public SysTrayApp()
        {
            trayMenu = new ContextMenu();
            trayMenu.MenuItems.Add("Exit", OnExit);

            //test code
            trayMenu.MenuItems.Add("Trigger toast", OnTriggerToast);

            trayIcon = new NotifyIcon
            {
                Text = "GoCD Build Notifier",
                Icon = new Icon(SystemIcons.Application, 40, 40),
                ContextMenu = trayMenu,
                Visible = true
            };

            trayIcon.ContextMenu = trayMenu;
            trayIcon.Visible = true;

            SetupWebSocketListener();
        }

        private void SetupWebSocketListener()
        {
            var websocket = new WebSocket("ws://localhost:8887/");
            //websocket.Opened += new EventHandler(websocket_Opened);
            //websocket.Error += new EventHandler<ErrorEventArgs>(websocket_Error);
            //websocket.Closed += new EventHandler(websocket_Closed);
            websocket.MessageReceived += (sender, e) => { ShowToast(e.Message); };
            websocket.Open();
        }

        private static void ShowToast(string message)
        {
            var result = JValue.Parse(message);
            var body = string.Format("{0} {1}\n{2} (run {3})\n{4}",
                result["pipeline-name"].Value<string>(),
                result["pipeline-counter"].Value<string>(),
                result["stage-name"].Value<string>(),
                result["stage-counter"].Value<string>(),
                result["stage-state"].Value<string>()
                );
            var toastNotification = new Notification("GoCD notification", body, 5, FormAnimator.AnimationMethod.Fade, FormAnimator.AnimationDirection.Left);
            toastNotification.Show();
        }

        protected override void OnLoad(EventArgs e)
        {
            Visible = false;
            ShowInTaskbar = false;

            base.OnLoad(e);
        }

        private void OnExit(object sender, EventArgs e)
        {
            Application.Exit();
        }

        //test code
        private void OnTriggerToast(object sender, EventArgs eventArgs)
        {
            var timer = new System.Timers.Timer { Interval = 1000 };
            timer.Elapsed += (o, e) =>
            {
                //var toastNotification = new Notification("title", "body", 5, FormAnimator.AnimationMethod.Fade, FormAnimator.AnimationDirection.Left);
                //toastNotification.Show();
                timer.Stop();


                var json = @"{
    ""pipeline-name"": ""Application_With_Long_Name"",
    ""pipeline-counter"": ""7.0.355"",
    ""stage-name"": ""Build.UnitTest.IntegrationTest"",
    ""stage-counter"": ""1"",
    ""stage-state"": ""Passed"",
    ""stage-result"": ""Passed"",
    ""create-time"": ""2011-07-14T19:43:37.100Z""
}";
                ShowToast(json);
            };
            timer.Start();
        }

        protected override void Dispose(bool isDisposing)
        {
            if (isDisposing)
            {
                trayIcon.Dispose();
            }

            base.Dispose(isDisposing);
        }
    }
}
