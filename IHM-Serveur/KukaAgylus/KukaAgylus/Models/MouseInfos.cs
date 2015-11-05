using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace KukaAgylus.Models
{
    public class MouseInfos
    {
        public bool IsConnected { get; set; } = false;
        public bool IsCalibrated { get; set; } = false;
        public double TranslationX { get; set; } = 0.0;
        public double TranslationY { get; set; } = 0.0;
        public double TranslationZ { get; set; } = 0.0;
        public double RotationX { get; set; } = 0.0;
        public double RotationY { get; set; } = 0.0;
        public double RotationZ { get; set; } = 0.0;
        public double Angle { get; set; } = 0.0;

        public string GetHtmlString()
        {
            // #0: display-name, #1: id-label, #2: display-value, #3: css-classes (separate with spaces)
            string htmlFormat = "<div class='row'><div class='col-md-4 col-md-offset-1'>{0}:</div><div class='col-md-2'><label id='{1}' class='val-info {3}'>{2}</label></div></div>";

            StringBuilder builder = new StringBuilder();
            builder.AppendFormat(htmlFormat, "Status", "mouse-status", IsConnected ? "OK" : "KO", IsConnected ? "status-ok" : "status-ko");
            builder.AppendFormat(htmlFormat, "Calibration", "mouse-calibration", IsCalibrated ? "OK" : "KO", IsCalibrated ? "status-ok" : "status-ko");
            builder.AppendFormat(htmlFormat, "Translation X", "mouse-transx", TranslationX.ToString(), string.Empty);
            builder.AppendFormat(htmlFormat, "Translation Y", "mouse-trany", TranslationY.ToString(), string.Empty);
            builder.AppendFormat(htmlFormat, "Translation Z", "mouse-transz", TranslationZ.ToString(), string.Empty);
            builder.AppendFormat(htmlFormat, "Rotation X", "mouse-rotx", RotationX, string.Empty);
            builder.AppendFormat(htmlFormat, "Rotation Y", "mouse-roty", RotationY, string.Empty);
            builder.AppendFormat(htmlFormat, "Rotation Z", "mouse-rotz", RotationZ, string.Empty);
            builder.AppendFormat(htmlFormat, "Angle", "mouse-angle", Angle, string.Empty);

            return builder.ToString();
        }

    }
}