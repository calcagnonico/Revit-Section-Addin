using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Windows.Media.Imaging;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Media;

namespace RevitPlugin
{
    internal class ExternalApplication : IExternalApplication
    {
        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }

        public Result OnStartup(UIControlledApplication application)
        {
            //Crear el ribbon tab
            application.CreateRibbonTab("Nico");

            string path = Assembly.GetExecutingAssembly().Location;
            PushButtonData button = new PushButtonData("Button 1", "Create Sections", path, "RevitPlugin.Secciones");
            RibbonPanel panel = application.CreateRibbonPanel("Nico", "Commands");


            PushButton pushButton = panel.AddItem(button) as PushButton;
            pushButton.LargeImage = PngImageSource("RevitPlugin.Icono.png");

            PushButtonData button1 = new PushButtonData("Button 2", "Order Sections", path, "RevitPlugin.AcomodarSecciones");
            PushButton pushButton1 = panel.AddItem(button1) as PushButton;
            pushButton1.LargeImage = PngImageSource("RevitPlugin.Icono.png");


            return Result.Succeeded;
        }

        private System.Windows.Media.ImageSource PngImageSource(string embeddedPath)
        {
            Stream stream = this.GetType().Assembly.GetManifestResourceStream(embeddedPath);
            var decoder = new System.Windows.Media.Imaging.PngBitmapDecoder(stream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);

            return decoder.Frames[0];
        }


    }
}
