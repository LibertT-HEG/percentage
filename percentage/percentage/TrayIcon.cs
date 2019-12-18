using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace percentage {
    class TrayIcon {
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern bool DestroyIcon(IntPtr handle);

        private const string iconFont = "Consolas";
        private const int iconFontSize = 12;

        private string batteryPercentage;
        private NotifyIcon notifyIcon;

        public TrayIcon() {
            ContextMenu contextMenu = new ContextMenu();
            MenuItem menuItem = new MenuItem();

            notifyIcon = new NotifyIcon();

            // initialize contextMenu
            contextMenu.MenuItems.AddRange(new MenuItem[] { menuItem });

            // initialize menuItem
            menuItem.Index = 0;
            menuItem.Text = "E&xit";
            menuItem.Click += new System.EventHandler(menuItem_Click);

            notifyIcon.ContextMenu = contextMenu;

            batteryPercentage = "?";

            notifyIcon.Visible = true;

            Timer timer = new Timer();
            timer.Tick += new EventHandler(timer_Tick);
            timer.Interval = 1000; // in miliseconds
            timer.Start();
        }

        private void timer_Tick(object sender, EventArgs e) {
            PowerStatus powerStatus = SystemInformation.PowerStatus;
            float battery = (powerStatus.BatteryLifePercent * 100);

            Color bgColor = Color.White;
            Color textColor = Color.Black;

            if (battery >= 50) {
                bgColor = Color.LightGreen;
            } else if (battery >= 30) {
                bgColor = Color.Yellow;
            } else {
                bgColor = Color.DarkRed;
                textColor = Color.White;
            }

            batteryPercentage = String.Format("{0:00}", battery);

            using (Bitmap bitmap = new Bitmap(DrawText(batteryPercentage, new Font(iconFont, iconFontSize), textColor, bgColor))) {
                System.IntPtr intPtr = bitmap.GetHicon();
                try {
                    using (Icon icon = Icon.FromHandle(intPtr)) {
                        notifyIcon.Icon = icon;
                        notifyIcon.Text = batteryPercentage + "%";
                    }
                } finally {
                    DestroyIcon(intPtr);
                }
            }
        }

        private void menuItem_Click(object sender, EventArgs e) {
            notifyIcon.Visible = false;
            notifyIcon.Dispose();
            Application.Exit();
        }

        private Image DrawText(String text, Font font, Color textColor, Color backColor) {
            var textSize = GetImageSize(text, font);
            var textSize2Char = GetImageSize("22", font);
            Image image = new Bitmap((int)textSize.Width, (int)textSize2Char.Height);
            using (Graphics graphics = Graphics.FromImage(image)) {
                // paint the background
                graphics.Clear(backColor);

                // create a brush for the text
                using (Brush textBrush = new SolidBrush(textColor)) {
                    graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
                    graphics.DrawString(text, font, textBrush, 0, 0);
                    graphics.Save();
                }
            }

            return image;
        }

        private static SizeF GetImageSize(string text, Font font) {
            using (Image image = new Bitmap(1, 1))
            using (Graphics graphics = Graphics.FromImage(image))
                return graphics.MeasureString(text, font);
        }
    }
}
