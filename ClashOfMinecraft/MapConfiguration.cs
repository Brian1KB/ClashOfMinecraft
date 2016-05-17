using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ClashOfMinecraft
{
    [DataContract]
    internal class MapConfiguration
    {
        [DataMember]
        public string PositionOne { get; set; }
        [DataMember]
        public string PositionTwo { get; set; }
    }
}
