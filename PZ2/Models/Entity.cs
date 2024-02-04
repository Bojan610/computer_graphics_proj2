using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PZ2.Models
{
    public abstract class Entity
    {
        private int xPosition;
        private int yPosition;
        private int zPosition;

        public int XPosition { get => xPosition; set => xPosition = value; }
        public int YPosition { get => yPosition; set => yPosition = value; }
        public int ZPosition { get => zPosition; set => zPosition = value; }
    }
}
