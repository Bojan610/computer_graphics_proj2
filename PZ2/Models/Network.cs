using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PZ2.Models
{
    public class Network
    {
        private List<SubstationEntity> substations;
        private List<SwitchEntity> switches;
        private List<NodeEntity> nodes;
        private List<LineEntity> lines;

        public Network()
        {
            this.substations = new List<SubstationEntity>();
            this.switches = new List<SwitchEntity>();
            this.nodes = new List<NodeEntity>();
            this.lines = new List<LineEntity>();
        }

        public List<SubstationEntity> Substations { get => substations; set => substations = value; }
        public List<SwitchEntity> Switches { get => switches; set => switches = value; }
        public List<NodeEntity> Nodes { get => nodes; set => nodes = value; }
        public List<LineEntity> Lines { get => lines; set => lines = value; }
    }
}
