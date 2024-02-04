using Microsoft.Win32;
using PZ2.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PZ2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private System.Windows.Point start = new System.Windows.Point();
        private System.Windows.Point diffOffset = new System.Windows.Point();
        private int zoomMax = 12;
        private int zoomCurent = 1;
        private string transform = "";

        private ArrayList models = new ArrayList();
        private GeometryModel3D hitgeo;
        private int[] previousIndex = new int[3];
        private DiffuseMaterial[] previousMatetial = new DiffuseMaterial[3];
        private bool translated;

        Network network = new Network();

        public MainWindow()
        {
            InitializeComponent();
            switchStatus.Background = Brushes.LightGray;
        }

        private void Viewport3D_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            viewport.CaptureMouse();
            start = e.GetPosition(this);
            diffOffset.X = translacija.OffsetX;
            diffOffset.Y = translacija.OffsetY;
            transform = "translate";
            translated = false;
        }

        private void Viewport3D_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!translated)
            {
                ((GeometryModel3D)models[previousIndex[0]]).Material = previousMatetial[0];
                ((GeometryModel3D)models[previousIndex[1]]).Material = previousMatetial[1];
                ((GeometryModel3D)models[previousIndex[2]]).Material = previousMatetial[2];
                
                toolTipLabel.Content = "";

                System.Windows.Point mouseposition = e.GetPosition(viewport);
                Point3D testpoint3D = new Point3D(mouseposition.X, mouseposition.Y, 0);
                Vector3D testdirection = new Vector3D(mouseposition.X, mouseposition.Y, 10);

                PointHitTestParameters pointparams =
                         new PointHitTestParameters(mouseposition);
                RayHitTestParameters rayparams =
                         new RayHitTestParameters(testpoint3D, testdirection);

                //test for a result in the Viewport3D     
                hitgeo = null;
                VisualTreeHelper.HitTest(viewport, null, HTResult, pointparams);
            }

            viewport.ReleaseMouseCapture();
            transform = "";
        }

        private void Viewport3D_MouseMove(object sender, MouseEventArgs e)
        {
            if (viewport.IsMouseCaptured && transform == "translate")
            {
                System.Windows.Point end = e.GetPosition(this);
                double offsetX = end.X - start.X;
                double offsetY = end.Y - start.Y;
                double w = this.Width;
                double h = this.Height;
                double translateX = (offsetX * 1000) / w;
                double translateY = -(offsetY * 1000) / h;
                translacija.OffsetX = diffOffset.X + (translateX / (10 * skaliranje.ScaleX));
                translacija.OffsetY = diffOffset.Y + (translateY / (10 * skaliranje.ScaleX));
                translated = true;
            }
            else if (viewport.IsMouseCaptured && transform == "rotate")
            {
                System.Windows.Point end = e.GetPosition(this);
                double offsetX = end.X - start.X;
                rotacija.Angle = diffOffset.X + (offsetX * 100) / this.Width;
            }
        }

        private void Viewport3D_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            System.Windows.Point p = e.MouseDevice.GetPosition(this);
            double scaleX = 1;
            double scaleY = 1;
            double scaleZ = 1;
            if (e.Delta > 0 && zoomCurent < zoomMax)
            {
                scaleX = skaliranje.ScaleX + 0.1;
                scaleY = skaliranje.ScaleY + 0.1;
                scaleZ = skaliranje.ScaleZ + 0.1;
                zoomCurent++;
                skaliranje.ScaleX = scaleX;
                skaliranje.ScaleY = scaleY;
                skaliranje.ScaleZ = scaleZ;
            }
            else if (e.Delta <= 0 && zoomCurent > -zoomMax)
            {
                scaleX = skaliranje.ScaleX - 0.1;
                scaleY = skaliranje.ScaleY - 0.1;
                scaleZ = skaliranje.ScaleZ - 0.1;
                zoomCurent--;
                skaliranje.ScaleX = scaleX;
                skaliranje.ScaleY = scaleY;
                skaliranje.ScaleZ = scaleZ;
            }
        }

        private void Viewport_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Middle && e.ButtonState == MouseButtonState.Pressed)
            {
                viewport.CaptureMouse();
                start = e.GetPosition(this);
                diffOffset.X = rotacija.Angle;
                transform = "rotate";
            }
        }

        private void Viewport_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Middle && e.ButtonState == MouseButtonState.Released)
            {
                viewport.ReleaseMouseCapture();
                transform = "";
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog
                {
                    DefaultExt = "xml",
                    Filter = "XML Files|*.xml"
                };

                if (openFileDialog.ShowDialog().GetValueOrDefault())
                {
                    network = NetworkHelper.ParseXML(openFileDialog.FileName);

                    NetworkHelper.ScalePositions(network);
                    NetworkHelper.DrawEntities(network, modelGroup);
                    NetworkHelper.DrawLines(network, modelGroup);

                    for (int i = 2; i < modelGroup.Children.Count; i++)
                        models.Add(modelGroup.Children[i]);
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Error", "Invalid file", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void SwitchStatus_Checked(object sender, RoutedEventArgs e)
        {              
            for (int j = 0; j < models.Count; j++)
            {
                GeometryModel3D geometryModel3D = (GeometryModel3D)models[j];
                MeshGeometry3D mesh1 = new MeshGeometry3D();
                mesh1 = (MeshGeometry3D)geometryModel3D.Geometry;

                foreach (SwitchEntity switchEntity in network.Switches)
                {
                    if (mesh1.Positions[0].X == switchEntity.XPosition && mesh1.Positions[0].Y == switchEntity.YPosition
                                        && mesh1.Positions[0].Z == switchEntity.ZPosition && mesh1.Positions.Count < 9)
                    {
                        if (switchEntity.Status == "Open")
                            geometryModel3D.Material = new DiffuseMaterial(new SolidColorBrush(Colors.Green));

                        else if (switchEntity.Status == "Closed")
                            geometryModel3D.Material = new DiffuseMaterial(new SolidColorBrush(Colors.Red));
                       
                    }
                }                   
            }                    
        }

        private void SwitchStatus_Unchecked(object sender, RoutedEventArgs e)
        {
            for (int j = 0; j < models.Count; j++)
            {
                GeometryModel3D geometryModel3D = (GeometryModel3D)models[j];
                MeshGeometry3D mesh1 = new MeshGeometry3D();
                mesh1 = (MeshGeometry3D)geometryModel3D.Geometry;

                foreach (SwitchEntity switchEntity in network.Switches)
                {
                    if (mesh1.Positions[0].X == switchEntity.XPosition && mesh1.Positions[0].Y == switchEntity.YPosition
                                        && mesh1.Positions[0].Z == switchEntity.ZPosition && mesh1.Positions.Count < 9)
                    {                        
                        geometryModel3D.Material = new DiffuseMaterial(new SolidColorBrush(Colors.Black));
                    }
                }
            }
        }

        private void LineResistance_Checked(object sender, RoutedEventArgs e)
        {
            for (int j = 0; j < models.Count; j++)
            {
                GeometryModel3D geometryModel3D = (GeometryModel3D)models[j];
                MeshGeometry3D mesh1 = new MeshGeometry3D();
                mesh1 = (MeshGeometry3D)geometryModel3D.Geometry;

                if (mesh1.Positions.Count > 8)
                {
                    foreach (LineEntity item in network.Lines)
                    {
                        if (mesh1.Positions[0].X == item.FirstEndXPosition && mesh1.Positions[0].Y == item.FirstEndYPosition
                                            && mesh1.Positions[mesh1.Positions.Count - 8].X == item.SecondEndXPosition && mesh1.Positions[mesh1.Positions.Count - 8].Y == item.SecondEndYPosition)
                        {
                            if (item.R < 1)
                                geometryModel3D.Material = new DiffuseMaterial(new SolidColorBrush(Colors.Red));

                            else if (item.R >= 1 && item.R <= 2)
                                geometryModel3D.Material = new DiffuseMaterial(new SolidColorBrush(Colors.Orange));
                            else if (item.R > 2)
                                geometryModel3D.Material = new DiffuseMaterial(new SolidColorBrush(Colors.Yellow));
                        }
                    }
                }
                else
                    continue;
            }
        }

        private void LineResistance_Unchecked(object sender, RoutedEventArgs e)
        {
            for (int j = 0; j < models.Count; j++)
            {
                GeometryModel3D geometryModel3D = (GeometryModel3D)models[j];
                MeshGeometry3D mesh1 = new MeshGeometry3D();
                mesh1 = (MeshGeometry3D)geometryModel3D.Geometry;

                if (mesh1.Positions.Count > 8)
                {
                    foreach (LineEntity item in network.Lines)
                    {
                        if (mesh1.Positions[0].X == item.FirstEndXPosition && mesh1.Positions[0].Y == item.FirstEndYPosition
                                            && mesh1.Positions[mesh1.Positions.Count - 8].X == item.SecondEndXPosition && mesh1.Positions[mesh1.Positions.Count - 8].Y == item.SecondEndYPosition)
                        {
                            if (item.ConductorMaterial == "Steel")
                                geometryModel3D.Material = new DiffuseMaterial(new SolidColorBrush(Colors.Gray));

                            else if (item.ConductorMaterial == "Acsr")
                                geometryModel3D.Material = new DiffuseMaterial(new SolidColorBrush(Colors.Blue));
                            else if (item.ConductorMaterial == "Copper")
                                geometryModel3D.Material = new DiffuseMaterial(new SolidColorBrush(Colors.OrangeRed));
                            else if (item.ConductorMaterial == "Other")
                                geometryModel3D.Material = new DiffuseMaterial(new SolidColorBrush(Colors.Violet));
                        }
                    }
                }
                else
                    continue;
            }
        }

        private HitTestResultBehavior HTResult(System.Windows.Media.HitTestResult rawresult)
        {
            RayHitTestResult rayResult = rawresult as RayHitTestResult;

            if (rayResult != null)
            {
                DiffuseMaterial darkSide = new DiffuseMaterial(new SolidColorBrush(System.Windows.Media.Colors.LightYellow));
                bool gasit = false;
                for (int i = 0; i < models.Count; i++)
                {
                    if ((GeometryModel3D)models[i] == rayResult.ModelHit)
                    {
                        hitgeo = (GeometryModel3D)rayResult.ModelHit;
                        gasit = true;
                        previousMatetial[0] = (DiffuseMaterial)hitgeo.Material;
                        hitgeo.Material = darkSide;
                        previousIndex[0] = i;

                        MeshGeometry3D mesh = new MeshGeometry3D();
                        mesh = (MeshGeometry3D)hitgeo.Geometry;

                        Entity entity = ModelToEntity(mesh);
                        if (entity is SubstationEntity)
                            toolTipLabel.Content = $"TYPE: SUBSTATION\n ID: {((SubstationEntity)entity).Id}\n NAME: {((SubstationEntity)entity).Name}";

                        else if (entity is NodeEntity)
                            toolTipLabel.Content = $"TYPE: NODE\n ID: {((NodeEntity)entity).Id}\n NAME: {((NodeEntity)entity).Name}";

                        else if (entity is SwitchEntity)
                            toolTipLabel.Content = $"TYPE: SWITCH\n ID: {((SwitchEntity)entity).Id}\n NAME: {((SwitchEntity)entity).Name}";

                        else if ((entity is LineEntity))
                        {
                            string toolTip = "";
                            toolTip = $"TYPE: LINE\n ID: {((LineEntity)entity).Id}\n NAME: {((LineEntity)entity).Name}\n";

                            Entity[] item = FindEndPoints((LineEntity)entity);
                            toolTip += $"FIRST END:\n";
                            if (item[0] is SubstationEntity)
                                toolTip += $"TYPE: SUBSTATION\n ID: {((SubstationEntity)item[0]).Id}\n NAME: {((SubstationEntity)item[0]).Name}";
                            else if (item[0] is NodeEntity)
                                toolTip += $"TYPE: NODE\n ID: {((NodeEntity)item[0]).Id}\n NAME: {((NodeEntity)item[0]).Name}";
                            else if (item[0] is SwitchEntity)
                                toolTip += $"TYPE: SWITCH\n ID: {((SwitchEntity)item[0]).Id}\n NAME: {((SwitchEntity)item[0]).Name}";

                            toolTip += $"\nSECOND END:\n";
                            if (item[1] is SubstationEntity)
                                toolTip += $"TYPE: SUBSTATION\n ID: {((SubstationEntity)item[1]).Id}\n NAME: {((SubstationEntity)item[1]).Name}";
                            else if (item[1] is NodeEntity)
                                toolTip += $"TYPE: NODE\n ID: {((NodeEntity)item[1]).Id}\n NAME: {((NodeEntity)item[1]).Name}";
                            else if (item[1] is SwitchEntity)
                                toolTip += $"TYPE: SWITCH\n ID: {((SwitchEntity)item[1]).Id}\n NAME: {((SwitchEntity)item[1]).Name}";

                            toolTipLabel.Content = toolTip;

                            bool foundFirst = false;
                            bool foundSecond = false;
                            for (int j = 0; j < models.Count; j++)
                            {
                                GeometryModel3D geometryModel3D = (GeometryModel3D)models[j];
                                MeshGeometry3D mesh1 = new MeshGeometry3D();
                                mesh1 = (MeshGeometry3D)geometryModel3D.Geometry;
                                                             
                                if (!foundFirst)
                                {                                   
                                    if (mesh1.Positions[0].X == item[0].XPosition && mesh1.Positions[0].Y == item[0].YPosition
                                        && mesh1.Positions[0].Z == item[0].ZPosition)
                                    {
                                        previousMatetial[1] = (DiffuseMaterial)geometryModel3D.Material;
                                        previousIndex[1] = j;
                                        geometryModel3D.Material = darkSide;
                                        foundFirst = true;
                                    }
                                }
                                if (!foundSecond)
                                {
                                    if (mesh1.Positions[0].X == item[1].XPosition && mesh1.Positions[0].Y == item[1].YPosition
                                        && mesh1.Positions[0].Z == item[1].ZPosition)
                                    {
                                        previousMatetial[2] = (DiffuseMaterial)geometryModel3D.Material;
                                        previousIndex[2] = j;
                                        geometryModel3D.Material = darkSide;
                                        geometryModel3D.Material = darkSide;
                                        foundSecond = true;
                                    }
                                }
                            }
                        }
                    }          
                }
                if (!gasit)
                {
                    hitgeo = null;
                }
            }

            return HitTestResultBehavior.Stop;
        }

        private Entity ModelToEntity(MeshGeometry3D geometry)
        {
            Entity entity = null;         
            if (previousMatetial[0].Brush.ToString() == Colors.Coral.ToString())
            {
                foreach (SubstationEntity item in network.Substations)
                {
                    if (geometry.Positions[0].X == item.XPosition && geometry.Positions[0].Y == item.YPosition
                            && geometry.Positions[0].Z == item.ZPosition)
                    {
                        entity = item;
                        break;
                    }
                }
            }
            else if (previousMatetial[0].Brush.ToString() == Colors.DarkBlue.ToString())
            {
                foreach (NodeEntity item in network.Nodes)
                {
                    if (geometry.Positions[0].X == item.XPosition && geometry.Positions[0].Y == item.YPosition
                            && geometry.Positions[0].Z == item.ZPosition)
                    {
                        entity = item;
                        break;
                    }
                }
            }
            else if (previousMatetial[0].Brush.ToString() == Colors.Black.ToString() || previousMatetial[0].Brush.ToString() == Colors.Red.ToString()
                || previousMatetial[0].Brush.ToString() == Colors.Green.ToString())
            {
                foreach (SwitchEntity item in network.Switches)
                {
                    if (geometry.Positions[0].X == item.XPosition && geometry.Positions[0].Y == item.YPosition
                            && geometry.Positions[0].Z == item.ZPosition)
                    {
                        entity = item;
                        break;
                    }
                }
            }
            else
            {
                foreach (LineEntity item in network.Lines)
                {
                    if (geometry.Positions[0].X == item.FirstEndXPosition && geometry.Positions[0].Y == item.FirstEndYPosition
                            && geometry.Positions[geometry.Positions.Count - 8].X == item.SecondEndXPosition && geometry.Positions[geometry.Positions.Count - 8].Y == item.SecondEndYPosition)
                    {
                        entity = item;
                        break;
                    }
                }
               
            }
            
            return entity;
        }

        private Entity[] FindEndPoints(LineEntity line)
        {
            Entity[] retEntity = new Entity[2];
            bool foundFirstEnd = false;
            bool foundSecondEnd = false;
            foreach (SubstationEntity entity in network.Substations)
            {
                if (line.FirstEnd == entity.Id)
                {
                    foundFirstEnd = true;
                    retEntity[0] = entity;
                }

                if (line.SecondEnd == entity.Id)
                {
                    foundSecondEnd = true;
                    retEntity[1] = entity;
                }
            }

            if (!foundFirstEnd || !foundSecondEnd)
            {
                foreach (NodeEntity entity in network.Nodes)
                {
                    if (line.FirstEnd == entity.Id)
                    {
                        foundFirstEnd = true;
                        retEntity[0] = entity;
                    }
                    if (line.SecondEnd == entity.Id)
                    {
                        foundSecondEnd = true;
                        retEntity[1] = entity;
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
                        retEntity[0] = entity;
                    }
                    if (line.SecondEnd == entity.Id)
                    {
                        foundSecondEnd = true;
                        retEntity[1] = entity;
                    }
                }
            }
            return retEntity;
        }

    }
}
