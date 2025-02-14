using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACE.Database.Models.Auth
{
    public partial class AccessLimits
    {
        public uint AccountId { get; set; }
        public byte[] IP { get; set; }
        public byte AccessLimit { get; set; }
    }
}
