using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace NetEye.res.model
{
    public class TechEquipment
    {
        public string Id { get; set; }
        public string IpAddress { get; set; }
        public TechType Type { get; set; }
    }

    public enum TechType
    {
        Computer,
        Printer,
        Camera
    }
}
