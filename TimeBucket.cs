using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinFormsActiveTango
{
    public class TimeBucket
    {
        public long ID { get; set; }
        public string BucketName { get; set; }
        public string Time { get; set; }

        public override string ToString()
        {
            return BucketName + "(" + Time + ")";
        }
    }
}
