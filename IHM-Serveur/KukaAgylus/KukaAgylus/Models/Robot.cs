using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace KukaAgylus.Models
{
    public class RobotInfos
    {
        public bool IsConnected { get; set; } = false;
        public string Mode { get; set; } = "Offline";
        public bool IsGripperOpened { get; set; } = false;
        public double Velocity { get; set; } = 0.0;
        public double PosX { get; set; } = 0.0;
        public double PosY { get; set; } = 0.0;
        public double PosZ { get; set; } = 0.0;

        public string GetHtmlString()
        {
            // #0: display-name, #1: id-label, #2: display-value, #3: css-classes (separate with spaces)
            string htmlFormat = "<div class='row'><div class='col-md-4 col-md-offset-1'>{0}:</div><div class='col-md-2'><label id='{1}' class='val-info {3}'>{2}</label></div></div>";

            StringBuilder builder = new StringBuilder();
            builder.AppendFormat(htmlFormat, "Status", "rob-status", IsConnected ? "OK" : "KO", IsConnected ? "status-ok" : "status-ko");
            builder.AppendFormat(htmlFormat, "Mode", "rob-mode", Mode, string.Empty);
            builder.AppendFormat(htmlFormat, "Gripper", "rob-gripper", IsGripperOpened ? "OPEN" : "CLOSE", IsGripperOpened ? "status-ok" : "status-ko");
            builder.AppendFormat(htmlFormat, "Velocity", "rob-velocity", Velocity.ToString(), string.Empty);
            builder.AppendFormat(htmlFormat, "PosX", "rob-posx", PosX, string.Empty);
            builder.AppendFormat(htmlFormat, "PosY", "rob-posy", PosY, string.Empty);
            builder.AppendFormat(htmlFormat, "PosZ", "rob-posz", PosZ, string.Empty);

            return builder.ToString();
        }
    }
}