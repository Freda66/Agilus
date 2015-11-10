using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;

namespace KukaAgylus.Webservices
{
    /// <summary>
    /// Summary description for Mouse
    /// </summary>
    [WebService(Namespace = "http://localhost/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    // [System.Web.Script.Services.ScriptService]
    public class Mouse : System.Web.Services.WebService
    {
        [WebMethod]
        public void SendMousePosition(double tx, double ty, double tz, double rx, double ry, double rz, double angle)
        {
            var mouseInfos = MvcApplication.MouseInfos;

            mouseInfos.TranslationX = tx;
            mouseInfos.TranslationY = ty;
            mouseInfos.TranslationZ = tz;

            mouseInfos.RotationX = rx;
            mouseInfos.RotationY = ry;
            mouseInfos.RotationZ = rz;

            mouseInfos.Angle = angle;

            //MvcApplication.Logs.AddLog("daemon", string.Format("Receive mouse vector : X={0}, Y={1}, Z={2}, Rx={3}, Ry={4}, Rz={5}, Angle={6}", tx, ty, tz, rx, ry, rz, angle));
        }

    }
}
