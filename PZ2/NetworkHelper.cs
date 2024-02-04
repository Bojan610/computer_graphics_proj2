using PZ2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Xml;

namespace PZ2
{
    public class NetworkHelper
    {    
        private static List<Entity> allEntities = new List<Entity>();
       
        public static Network ParseXML(string path)
        {
            Network retNetwork = new Network();

            XmlDocument document = new XmlDocument();
            document.Load(path);
            XmlNodeList substations = document.SelectNodes("/NetworkModel/Substations/SubstationEntity");
            XmlNodeList switches = document.SelectNodes("/NetworkModel/Switches/SwitchEntity");
            XmlNodeList nodes = document.SelectNodes("/NetworkModel/Nodes/NodeEntity");
            XmlNodeList lines = document.SelectNodes("/NetworkModel/Lines/LineEntity");

            retNetwork.Substations = GetSubstations(substations);
            retNetwork.Switches = GetSwitches(switches);
            retNetwork.Nodes = GetNodes(nodes);
            retNetwork.Lines = GetLines(lines);

            return retNetwork;
        }

        private static List<SubstationEntity> GetSubstations(XmlNodeList xmlSubstations)
        {
            List<SubstationEntity> substationEntities = new List<SubstationEntity>();

            for (int i = 0; i < xmlSubstations.Count; i++)
            {
                SubstationEntity substationEntity = new SubstationEntity();

                substationEntity.Id = long.Parse(xmlSubstations[i].SelectSingleNode("Id").InnerText);
                substationEntity.Name = xmlSubstations[i].SelectSingleNode("Name").InnerText;
                substationEntity.X = double.Parse(xmlSubstations[i].SelectSingleNode("X").InnerText);
                substationEntity.Y = double.Parse(xmlSubstations[i].SelectSingleNode("Y").InnerText);

                ToLatLon(substationEntity.X, substationEntity.Y, 34, out double x, out double y);
                substationEntity.X = x;
                substationEntity.Y = y;

                substationEntities.Add(substationEntity);
            }

            return substationEntities;
        }

        private static List<SwitchEntity> GetSwitches(XmlNodeList xmlSwitches)
        {
            List<SwitchEntity> sw = new List<SwitchEntity>();

            for (int i = 0; i < xmlSwitches.Count; i++)
            {
                SwitchEntity switchEntity = new SwitchEntity();

                switchEntity.Id = long.Parse(xmlSwitches[i].SelectSingleNode("Id").InnerText);
                switchEntity.Name = xmlSwitches[i].SelectSingleNode("Name").InnerText;
                switchEntity.X = double.Parse(xmlSwitches[i].SelectSingleNode("X").InnerText);
                switchEntity.Y = double.Parse(xmlSwitches[i].SelectSingleNode("Y").InnerText);
                switchEntity.Status = xmlSwitches[i].SelectSingleNode("Status").InnerText;

                ToLatLon(switchEntity.X, switchEntity.Y, 34, out double x, out double y);
                switchEntity.X = x;
                switchEntity.Y = y;

                sw.Add(switchEntity);
            }

            return sw;
        }

        private static List<NodeEntity> GetNodes(XmlNodeList xmlNodes)
        {
            List<NodeEntity> nodes = new List<NodeEntity>();

            for (int i = 0; i < xmlNodes.Count; i++)
            {
                NodeEntity node = new NodeEntity();

                node.Id = long.Parse(xmlNodes[i].SelectSingleNode("Id").InnerText);
                node.Name = xmlNodes[i].SelectSingleNode("Name").InnerText;
                node.X = double.Parse(xmlNodes[i].SelectSingleNode("X").InnerText);
                node.Y = double.Parse(xmlNodes[i].SelectSingleNode("Y").InnerText);

                ToLatLon(node.X, node.Y, 34, out double x, out double y);
                node.X = x;
                node.Y = y;

                nodes.Add(node);
            }

            return nodes;
        }

        private static List<LineEntity> GetLines(XmlNodeList xmlLines)
        {
            List<LineEntity> lines = new List<LineEntity>();

            for (int i = 0; i < xmlLines.Count; i++)
            {
                LineEntity line = new LineEntity();

                line.ConductorMaterial = xmlLines[i].SelectSingleNode("ConductorMaterial").InnerText;
                line.FirstEnd = long.Parse(xmlLines[i].SelectSingleNode("FirstEnd").InnerText);
                line.Id = long.Parse(xmlLines[i].SelectSingleNode("Id").InnerText);
                line.IsUnderground = bool.Parse(xmlLines[i].SelectSingleNode("IsUnderground").InnerText);
                line.LineType = xmlLines[i].SelectSingleNode("LineType").InnerText;
                line.Name = xmlLines[i].SelectSingleNode("Name").InnerText;
                line.R = float.Parse(xmlLines[i].SelectSingleNode("R").InnerText);
                line.SecondEnd = long.Parse(xmlLines[i].SelectSingleNode("SecondEnd").InnerText);
                line.ThermalConstantHeat = long.Parse(xmlLines[i].SelectSingleNode("ThermalConstantHeat").InnerText);

                XmlNode vertices = xmlLines[i].SelectSingleNode("Vertices");
                XmlNodeList pointsXml = vertices.SelectNodes("Point");
                List<Point> points = new List<Point>();

                for (int j = 0; j < pointsXml.Count; j++)
                {
                    Point p = new Point();
                    p.X = double.Parse(pointsXml[j].SelectSingleNode("X").InnerText);
                    p.Y = double.Parse(pointsXml[j].SelectSingleNode("Y").InnerText);

                    ToLatLon(p.X, p.Y, 34, out double x, out double y);
                    p.X = x;
                    p.Y = y;

                    points.Add(p);
                }

                line.Vertices = points;
                lines.Add(line);
            }

            return lines;
        }

        public static void ScalePositions(Network network)
        {
            double minX = 45.2325;
            double minY = 19.793909;
            double maxX = 45.277031;
            double maxY = 19.894459;

            double stepX = (maxX - minX) / 300;
            double stepY = (maxY - minY) / 300;

            ScaleSubstations(network, minX, minY, maxX, maxY, stepX, stepY);
            ScaleSwitches(network, minX, minY, maxX, maxY, stepX, stepY);
            ScaleNodes(network, minX, minY, maxX, maxY, stepX, stepY);
            ScaleVertices(network, minX, minY, maxX, maxY, stepX, stepY);
        }

        private static void ScaleSubstations(Network network, double minX, double minY, double maxX, double maxY, double stepX, double stepY)
        {
            foreach (SubstationEntity se in network.Substations)
            {
                if (se.X <= (maxX - stepX) && se.X >= minX && se.Y <= (maxY - stepY) && se.Y >= minY)
                {
                    se.XPosition = Convert.ToInt32((se.X - minX) / stepX);
                    se.YPosition = Convert.ToInt32((se.Y - minY) / stepY);

                    allEntities.Add(se);
                }
            }
        }

        private static void ScaleSwitches(Network network, double minX, double minY, double maxX, double maxY, double stepX, double stepY)
        {
            foreach (SwitchEntity sw in network.Switches)
            {
                if (sw.X <= (maxX - stepX) && sw.X >= minX && sw.Y <= (maxY - stepY) && sw.Y >= minY)
                {
                    sw.XPosition = Convert.ToInt32((sw.X - minX) / stepX);
                    sw.YPosition = Convert.ToInt32((sw.Y - minY) / stepY);

                    allEntities.Add(sw);
                }
            }
        }

        private static void ScaleNodes(Network network, double minX, double minY, double maxX, double maxY, double stepX, double stepY)
        {
            foreach (NodeEntity node in network.Nodes)
            {
                if (node.X <= (maxX - stepX) && node.X >= minX && node.Y <= (maxY - stepY) && node.Y >= minY)
                {
                    node.XPosition = Convert.ToInt32((node.X - minX) / stepX);
                    node.YPosition = Convert.ToInt32((node.Y - minY) / stepY);

                    allEntities.Add(node);
                }
            }
        }

        private static void ScaleVertices(Network network, double minX, double minY, double maxX, double maxY, double stepX, double stepY)
        {
            foreach (LineEntity line in network.Lines)
            {
                bool foundFirstEnd = false;
                bool foundSecondEnd = false;
                foreach (SubstationEntity entity in network.Substations)
                {
                    if (line.FirstEnd == entity.Id)
                    {
                        foundFirstEnd = true;
                        line.FirstEndXPosition = entity.XPosition;
                        line.FirstEndYPosition = entity.YPosition;
                    }

                    if (line.SecondEnd == entity.Id)
                    {
                        foundSecondEnd = true;
                        line.SecondEndXPosition = entity.XPosition;
                        line.SecondEndYPosition = entity.YPosition;
                    }
                }

                if (!foundFirstEnd || !foundSecondEnd)
                {
                    foreach (NodeEntity entity in network.Nodes)
                    {
                        if (line.FirstEnd == entity.Id)
                        {
                            foundFirstEnd = true;
                            line.FirstEndXPosition = entity.XPosition;
                            line.FirstEndYPosition = entity.YPosition;
                        }
                        if (line.SecondEnd == entity.Id)
                        {
                            foundSecondEnd = true;
                            line.SecondEndXPosition = entity.XPosition;
                            line.SecondEndYPosition = entity.YPosition;
                        }
                    }
                }

                if (!foundFirstEnd || !foundSecondEnd)
                {
                    foreach (SwitchEntity entity in network.Switches)
                    {
                        if (line.FirstEnd == entity.Id)
                        {
                            foundFirstEnd = true;
                            line.FirstEndXPosition = entity.XPosition;
                            line.FirstEndYPosition = entity.YPosition;
                        }
                        if (line.SecondEnd == entity.Id)
                        {
                            foundSecondEnd = true;
                            line.SecondEndXPosition = entity.XPosition;
                            line.SecondEndYPosition = entity.YPosition;
                        }
                    }
                }
           
                if (foundSecondEnd && foundSecondEnd)
                {
                    bool allPoints = true;
                    foreach (Point p in line.Vertices)
                    {
                        if (p.X <= (maxX - stepX) && p.X >= minX && p.Y <= (maxY - stepY) && p.Y >= minY)
                        {
                            p.XPosition = Convert.ToInt32((p.X - minX) / stepX);
                            p.YPosition = Convert.ToInt32((p.Y - minY) / stepY);
                        }
                        else
                        {
                            allPoints = false;
                            break;
                        }
                    }

                    if (allPoints)
                        line.Draw = true;
                }
            }
        }

        public static void DrawEntities(Network network, Model3DGroup model3DGroup)
        {
            List<Entity> groupEntity = new List<Entity>();
            int counterZ = 0;

            bool[] array = new bool[allEntities.Count];
            for (int i = 0; i < allEntities.Count; i++)
                array[i] = false;

            for (int i = 0; i < allEntities.Count - 1; i++)
            {
                counterZ = 0;
                if (array[i] == true)
                    continue;
                else
                {
                    allEntities[i].ZPosition = counterZ;
                    groupEntity = new List<Entity>();
                    array[i] = true;                  
                    groupEntity.Add(allEntities[i]);

                    for (int j = i + 1; j < allEntities.Count; j++)
                    {
                        if (allEntities[i].XPosition == allEntities[j].XPosition && allEntities[i].YPosition == allEntities[j].YPosition)
                        {
                            counterZ++;
                            allEntities[j].ZPosition = counterZ;
                            groupEntity.Add(allEntities[j]);
                            array[j] = true;
                        }
                    }

                    DrawGroupEntity(groupEntity, model3DGroup);
                }
            }        
        }

        private static void DrawGroupEntity(List<Entity> entities, Model3DGroup model3DGroup)
        {
            double dimension = 1;
            foreach (Entity entity in entities)
            {
                GeometryModel3D myGeometryModel = new GeometryModel3D();               
                MeshGeometry3D cube = new MeshGeometry3D();
                
                Point3DCollection myPositionCollection = new Point3DCollection();
                myPositionCollection.Add(new Point3D(entity.XPosition, entity.YPosition, entity.ZPosition));
                myPositionCollection.Add(new Point3D(entity.XPosition + dimension, entity.YPosition, entity.ZPosition));
                myPositionCollection.Add(new Point3D(entity.XPosition, entity.YPosition + dimension, entity.ZPosition));
                myPositionCollection.Add(new Point3D(entity.XPosition + dimension, entity.YPosition + dimension, entity.ZPosition));
                myPositionCollection.Add(new Point3D(entity.XPosition, entity.YPosition, entity.ZPosition + dimension));
                myPositionCollection.Add(new Point3D(entity.XPosition + dimension, entity.YPosition, entity.ZPosition + dimension));
                myPositionCollection.Add(new Point3D(entity.XPosition, entity.YPosition + dimension, entity.ZPosition + dimension));
                myPositionCollection.Add(new Point3D(entity.XPosition + dimension, entity.YPosition + dimension, entity.ZPosition + dimension));

                cube.Positions = myPositionCollection;

                Int32[] indices ={
                                   2,3,1, 2,1,0, 7,1,3, 7,5,1, 6,5,7, 6,4,5, 6,2,4,
                                    2,0,4, 2,7,3, 2,6,7, 0,1,5, 0,5,4
                };

                Int32Collection triangles = new Int32Collection();
                foreach (Int32 index in indices)
                {
                    triangles.Add(index);
                }
                cube.TriangleIndices = triangles;
               
                myGeometryModel.Geometry = cube;
                if (entity is SubstationEntity)
                    myGeometryModel.Material = new DiffuseMaterial(new SolidColorBrush(Colors.Coral));
                else if (entity is SwitchEntity)
                    myGeometryModel.Material = new DiffuseMaterial(new SolidColorBrush(Colors.Black));
                else if (entity is NodeEntity)
                    myGeometryModel.Material = new DiffuseMaterial(new SolidColorBrush(Colors.DarkBlue));
                
                model3DGroup.Children.Add(myGeometryModel);
            }
        }

        public static void DrawLines(Network network, Model3DGroup model3DGroup)
        {
            double dimension = 0.2;
            foreach (LineEntity line in network.Lines)
            {
                if (line.Draw == true)
                {
                    GeometryModel3D myGeometryModel = new GeometryModel3D();
                    MeshGeometry3D cube = new MeshGeometry3D();

                    int j = 0;
                    Point3DCollection myPositionCollection = new Point3DCollection();
                    AddFirstEnd(line, myPositionCollection);
                    for (int i = 1; i < line.Vertices.Count; i++)
                    {                     
                        myPositionCollection.Add(new Point3D(line.Vertices[j].XPosition, line.Vertices[j].YPosition, 1));
                        myPositionCollection.Add(new Point3D(line.Vertices[i].XPosition, line.Vertices[i].YPosition, 1));
                        myPositionCollection.Add(new Point3D(line.Vertices[j].XPosition, line.Vertices[j].YPosition + dimension, 1));
                        myPositionCollection.Add(new Point3D(line.Vertices[i].XPosition, line.Vertices[i].YPosition + dimension, 1));
                        myPositionCollection.Add(new Point3D(line.Vertices[j].XPosition, line.Vertices[j].YPosition, 1 + dimension));
                        myPositionCollection.Add(new Point3D(line.Vertices[i].XPosition, line.Vertices[i].YPosition, 1 + dimension));
                        myPositionCollection.Add(new Point3D(line.Vertices[j].XPosition, line.Vertices[j].YPosition + dimension, 1 + dimension));
                        myPositionCollection.Add(new Point3D(line.Vertices[i].XPosition, line.Vertices[i].YPosition + dimension, 1 + dimension));

                        j = i;
                    }
                    AddSecondEnd(line, myPositionCollection);
                    cube.Positions = myPositionCollection;

                    List<Int32[]> listIndices = new List<int[]>();
                    for (int i = 0; i < cube.Positions.Count / 8; i++)
                    {
                        Int32[] indices ={
                                   (i*8)+2,(i*8)+3,(i*8)+1, (i*8)+2,(i*8)+1,(i*8)+0, (i*8)+7,(i*8)+1,(i*8)+3, (i*8)+7,(i*8)+5,(i*8)+1, (i*8)+6,(i*8)+5,(i*8)+7, (i*8)+6,(i*8)+4,(i*8)+5, (i*8)+6,(i*8)+2,(i*8)+4,
                                    (i*8)+2,(i*8)+0,(i*8)+4, (i*8)+2,(i*8)+7,(i*8)+3, (i*8)+2,(i*8)+6,(i*8)+7, (i*8)+0,(i*8)+1,(i*8)+5, (i*8)+0,(i*8)+5,(i*8)+4
                        };
                        listIndices.Add(indices);
                    }

                    Int32Collection triangles = new Int32Collection();
                    foreach (Int32[] item in listIndices)
                    {
                        foreach (Int32 index in item)
                        {
                            triangles.Add(index);
                        }
                    }
                    cube.TriangleIndices = triangles;

                    myGeometryModel.Geometry = cube;
                    if (line.ConductorMaterial == "Steel")
                        myGeometryModel.Material = new DiffuseMaterial(new SolidColorBrush(Colors.Gray));
                    else if (line.ConductorMaterial == "Acsr")
                        myGeometryModel.Material = new DiffuseMaterial(new SolidColorBrush(Colors.Blue));
                    else if (line.ConductorMaterial == "Copper")
                        myGeometryModel.Material = new DiffuseMaterial(new SolidColorBrush(Colors.OrangeRed));
                    else if (line.ConductorMaterial == "Other")
                        myGeometryModel.Material = new DiffuseMaterial(new SolidColorBrush(Colors.Violet));
                    
                    model3DGroup.Children.Add(myGeometryModel);                                       
                }
            }
        }

        private static void AddFirstEnd(LineEntity line, Point3DCollection myPositionCollection)
        {
            double dimension = 0.2;

            myPositionCollection.Add(new Point3D(line.FirstEndXPosition, line.FirstEndYPosition, 1));
            myPositionCollection.Add(new Point3D(line.Vertices[0].XPosition, line.Vertices[0].YPosition, 1));
            myPositionCollection.Add(new Point3D(line.FirstEndXPosition, line.FirstEndYPosition + dimension, 1));
            myPositionCollection.Add(new Point3D(line.Vertices[0].XPosition, line.Vertices[0].YPosition + dimension, 1));
            myPositionCollection.Add(new Point3D(line.FirstEndXPosition, line.FirstEndYPosition, 1 + dimension));
            myPositionCollection.Add(new Point3D(line.Vertices[0].XPosition, line.Vertices[0].YPosition, 1 + dimension));
            myPositionCollection.Add(new Point3D(line.FirstEndXPosition, line.FirstEndYPosition + dimension, 1 + dimension));
            myPositionCollection.Add(new Point3D(line.Vertices[0].XPosition, line.Vertices[0].YPosition + dimension, 1 + dimension));
        }
                   
        private static void AddSecondEnd(LineEntity line, Point3DCollection myPositionCollection)
        {
            double dimension = 0.2;
           
            myPositionCollection.Add(new Point3D(line.SecondEndXPosition, line.SecondEndYPosition, 1));
            myPositionCollection.Add(new Point3D(line.Vertices[line.Vertices.Count - 1].XPosition, line.Vertices[line.Vertices.Count - 1].YPosition, 1));
            myPositionCollection.Add(new Point3D(line.SecondEndXPosition, line.SecondEndYPosition + dimension, 1));
            myPositionCollection.Add(new Point3D(line.Vertices[line.Vertices.Count - 1].XPosition, line.Vertices[line.Vertices.Count - 1].YPosition + dimension, 1));
            myPositionCollection.Add(new Point3D(line.SecondEndXPosition, line.SecondEndYPosition, 1 + dimension));
            myPositionCollection.Add(new Point3D(line.Vertices[line.Vertices.Count - 1].XPosition, line.Vertices[line.Vertices.Count - 1].YPosition, 1 + dimension));
            myPositionCollection.Add(new Point3D(line.SecondEndXPosition, line.SecondEndYPosition + dimension, 1 + dimension));
            myPositionCollection.Add(new Point3D(line.Vertices[line.Vertices.Count - 1].XPosition, line.Vertices[line.Vertices.Count - 1].YPosition + dimension, 1 + dimension));         
        }

        //From UTM to Latitude and longitude in decimal
        public static void ToLatLon(double utmX, double utmY, int zoneUTM, out double latitude, out double longitude)
        {
            bool isNorthHemisphere = true;

            var diflat = -0.00066286966871111111111111111111111111;
            var diflon = -0.0003868060578;

            var zone = zoneUTM;
            var c_sa = 6378137.000000;
            var c_sb = 6356752.314245;
            var e2 = Math.Pow((Math.Pow(c_sa, 2) - Math.Pow(c_sb, 2)), 0.5) / c_sb;
            var e2cuadrada = Math.Pow(e2, 2);
            var c = Math.Pow(c_sa, 2) / c_sb;
            var x = utmX - 500000;
            var y = isNorthHemisphere ? utmY : utmY - 10000000;

            var s = ((zone * 6.0) - 183.0);
            var lat = y / (c_sa * 0.9996);
            var v = (c / Math.Pow(1 + (e2cuadrada * Math.Pow(Math.Cos(lat), 2)), 0.5)) * 0.9996;
            var a = x / v;
            var a1 = Math.Sin(2 * lat);
            var a2 = a1 * Math.Pow((Math.Cos(lat)), 2);
            var j2 = lat + (a1 / 2.0);
            var j4 = ((3 * j2) + a2) / 4.0;
            var j6 = ((5 * j4) + Math.Pow(a2 * (Math.Cos(lat)), 2)) / 3.0;
            var alfa = (3.0 / 4.0) * e2cuadrada;
            var beta = (5.0 / 3.0) * Math.Pow(alfa, 2);
            var gama = (35.0 / 27.0) * Math.Pow(alfa, 3);
            var bm = 0.9996 * c * (lat - alfa * j2 + beta * j4 - gama * j6);
            var b = (y - bm) / v;
            var epsi = ((e2cuadrada * Math.Pow(a, 2)) / 2.0) * Math.Pow((Math.Cos(lat)), 2);
            var eps = a * (1 - (epsi / 3.0));
            var nab = (b * (1 - epsi)) + lat;
            var senoheps = (Math.Exp(eps) - Math.Exp(-eps)) / 2.0;
            var delt = Math.Atan(senoheps / (Math.Cos(nab)));
            var tao = Math.Atan(Math.Cos(delt) * Math.Tan(nab));

            longitude = ((delt * (180.0 / Math.PI)) + s) + diflon;
            latitude = ((lat + (1 + e2cuadrada * Math.Pow(Math.Cos(lat), 2) - (3.0 / 2.0) * e2cuadrada * Math.Sin(lat) * Math.Cos(lat) * (tao - lat)) * (tao - lat)) * (180.0 / Math.PI)) + diflat;
        }
    }
}
