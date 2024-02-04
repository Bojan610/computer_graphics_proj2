using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PZ2.Models
{
    public class LineEntity : Entity
    {
        private long id;
        private string name;
        private bool isUnderground;
        private float r;
        private string conductorMaterial;
        private string lineType;
        private long thermalConstantHeat;
        private long firstEnd;
        private int firstEndXPosition;
        private int firstEndYPosition;
        private long secondEnd;
        private int secondEndXPosition;
        private int secondEndYPosition;
        private List<Point> vertices;
        private bool draw;

        public LineEntity() { draw = false; }

        public long Id { get => id; set => id = value; }
        public string Name { get => name; set => name = value; }
        public bool IsUnderground { get => isUnderground; set => isUnderground = value; }
        public float R { get => r; set => r = value; }
        public string ConductorMaterial { get => conductorMaterial; set => conductorMaterial = value; }
        public string LineType { get => lineType; set => lineType = value; }
        public long ThermalConstantHeat { get => thermalConstantHeat; set => thermalConstantHeat = value; }
        public long FirstEnd { get => firstEnd; set => firstEnd = value; }
        public long SecondEnd { get => secondEnd; set => secondEnd = value; }
        public List<Point> Vertices { get => vertices; set => vertices = value; }
        public bool Draw { get => draw; set => draw = value; }
        public int FirstEndXPosition { get => firstEndXPosition; set => firstEndXPosition = value; }
        public int FirstEndYPosition { get => firstEndYPosition; set => firstEndYPosition = value; }
        public int SecondEndXPosition { get => secondEndXPosition; set => secondEndXPosition = value; }
        public int SecondEndYPosition { get => secondEndYPosition; set => secondEndYPosition = value; }
    }
}
