using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PZ2.Models
{
    public class NodeEntity : Entity
    {
        private long id;
        private string name;
        private double x;
        private double y;
       
        public NodeEntity() { }

        public long Id { get => id; set => id = value; }
        public string Name { get => name; set => name = value; }
        public double X { get => x; set => x = value; }
        public double Y { get => y; set => y = value; }
        public int XPosition { get => base.XPosition; set => base.XPosition = value; }
        public int YPosition { get => base.YPosition; set => base.YPosition = value; }
        public int ZPosition { get => base.ZPosition; set => base.ZPosition = value; }
    }
}
