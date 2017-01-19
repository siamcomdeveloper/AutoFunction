using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections;
using System.IO;
using System.Diagnostics;
using Microsoft.VisualBasic;
using System.Windows.Threading;
using Renci.SshNet;

namespace SmartGrid
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        DirectoryInfo folderimages;
        FileInfo[] images;
        bool flagCursor = true, flagLine = false, flagMarkLD = false, flagMarkLN = false, flagDelete = false, flagGenerateSCD = false, flagInputIO = false;
        bool flagGateAND = false, flagGateOR = false, flagGateNOT = false, flagOutputIO = false, flagInputGoose = false, flagOutputGoose = false, flagFK = false, flagLED = false;
        bool flagNew = false;
        double top, left;
        double XDevice, YDevice, LDevice, RDevice, TDevice, BDevice, DHDevice, DWDevice;
        double DLDevice, DRDevice, DTDevice, DBDevice;
        int startGridX = 40, startGridY = 70, GridSizeH = 15, GridSizeV = 20;
        int pointL, pointR, pointT, pointB;
        int ColumnLineStartH, ColumnLineStartV, ColumnLineEndH, ColumnLineEndV;
        bool flagPointL, flagPointR, flagPointT, flagPointB;
        bool flagLineStartX, flagLineStartY, flagLineEndX, flagLineEndY;
        bool DeiveOnGrid = false;
        bool flagDeleteLine = true;
        int count = 0, countBuffer = 201;
        int selectedIndex;
        int LineTime,countLine = 0;
        bool flagCreateDevice = false;
        bool FindObjectFirst = false;
        int index = 0, indexOldLine = -1, size, FirstObject, SecondObject;

        int ColumnH, ColumnV;
        bool flagColumnH, flagColumnV;

        List<Image> ObjList = new List<Image>();
        List<List<String>> LineList;// List<int> LineList = new List<int>();
        List<BitmapImage> Objimg = new List<BitmapImage>();
        string ObjectDeviceData;
        Line CurrentLine;//, newLine;
        Point start;
        Point end;

        int[,] gridArray;
        string srt = "",lineName;
        List<List<String>> matrix;
        Color customColor;
        Brush brush;

        Border rect;
        TextBlock Child;
        String TextLN, TextLD, TextLine, TextMuli, TextUnit, TextValue;
        HitTestResult target;
        Image img;
        Border bor;
        Line line;
        String TypeObj;

        StringBuilder str,strGoose;

        String device;
        int Nodevice;
        private const string GestureSaveFileLocation = @"D:\";
        private const string GestureSaveFileNamePrefix = @"command";
        string fileName;
        string newLine = "\r\n";
        System.IO.StreamReader file;
        DispatcherTimer dispatcherTimer1, dispatcherTimer2;
        int loop = 0;
        Boolean cp = true;
        public MainWindow()
        {
            InitializeComponent();
            dispatcherTimer1 = new System.Windows.Threading.DispatcherTimer();
            dispatcherTimer1.Tick += new EventHandler(dispatcherTimer1_Tick);
            dispatcherTimer1.Interval = TimeSpan.FromMilliseconds(100);
            
            dispatcherTimer2 = new System.Windows.Threading.DispatcherTimer();
            dispatcherTimer2.Tick += new EventHandler(dispatcherTimer2_Tick);
            dispatcherTimer2.Interval = TimeSpan.FromMilliseconds(1000);

            matrix = new List<List<String>>();
            LineList = new List<List<String>>();
            richTextBox1.VerticalScrollBarVisibility = ScrollBarVisibility.Visible;
            richTextBox2.VerticalScrollBarVisibility = ScrollBarVisibility.Visible;
            customColor = Color.FromArgb(50, 100, 100, 100);
            brush = new SolidColorBrush(customColor);
            str = new StringBuilder();
            strGoose = new StringBuilder();
            //fileName = GestureSaveFileNamePrefix + DateTime.Now.ToString("yyyy-MM-dd_HH-mm") + ".xml";
            //System.IO.File.WriteAllText(GestureSaveFileLocation + fileName, str.ToString());
        }

        private void dispatcherTimer1_Tick(object sender, EventArgs e)
        {
            if(loop<=100)progressBar1.Value = loop;
            loop++;
            if (loop > 100)
            {
                //timer1.Enabled = false;
                //timer2.Enabled = true;
                dispatcherTimer1.Stop();
                dispatcherTimer2.Start();
            }
        }

        private void dispatcherTimer2_Tick(object sender, EventArgs e)
        {
            //timer1.Enabled = false;
            //timer2.Enabled = false;
            dispatcherTimer1.Stop();
            dispatcherTimer2.Stop();
            progressBar1.Value = 0;
            if (!cp)
                MessageBox.Show("Error : IEC data upload failed ! \nPlease check network connection and try agian.");
            else MessageBox.Show("upload complete.");
        }

        bool activated = false;
        Point point;

        public class MyItem
        {
            public int index { get; set; }
            public BitmapImage Dimg { get; set; }
            public string DimgName { get; set; }
            public string DName { get; set; }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            gridArray = new int[63, 63];
            for (int i = 0; i < 63; i++)
            {
                for (int j = 0; j < 63; j++)
                {
                    gridArray[i, j] = 0;
                }
            }

            DirectoryInfo folderimage = new DirectoryInfo(System.IO.Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName) + @"\image\");
            FileInfo[] image = folderimage.GetFiles("*.bmp");
            foreach (FileInfo img in image)
            {
                image1.Source = new BitmapImage(new Uri(img.FullName));
                //image2.Source = new BitmapImage(new Uri(img.FullName));
            }
        }


        private void ellipse_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (false)//(flagCursor)
            {
                activated = true;

                Point pt = e.GetPosition(WorkSpace);
                HitTestResult result = VisualTreeHelper.HitTest(WorkSpace, pt);

                if (result != null)
                {
                    if ("System.Windows.Controls.Image" == result.VisualHit.ToString())
                    {
                        textBox1.Text = "Image";
                        TypeObj = "Image";
                        img = (Image)e.OriginalSource;
                        point = Mouse.GetPosition(img);
                        Mouse.Capture(img);
                    }
                    else if ("System.Windows.Controls.Border" == result.VisualHit.ToString())
                    {
                        textBox1.Text = "Border";
                        TypeObj = "Border";
                        bor = (Border)e.OriginalSource;
                    }
                    else if ("System.Windows.Controls.Line" == result.VisualHit.ToString())
                    {
                        textBox1.Text = "Line";
                        TypeObj = "Line";
                        line = (Line)e.OriginalSource;
                    }
                }
            }
            else if (flagLine)
            {
                LineTime++;
                if (LineTime % 2 == 1)
                {
                    indexOldLine = -1;
                    start = e.GetPosition(this);
                }
                else
                {
                    end = e.GetPosition(WorkSpace);

                    DrawLine(start, end, WorkSpace);

                    Point pt = new Point(start.X, start.Y);
                    RedrawObject(pt);

                    pt = new Point(end.X, end.Y);
                    RedrawObject(pt);


                    ColumnLineStartH = -1;
                    ColumnLineStartV = -1;
                    ColumnLineEndH = -1;
                    ColumnLineEndV = -1;

                    for (int i = 0; i < 64; i++)
                    {
                        if (start.X >= (startGridX + (i * GridSizeV)) && start.X < (startGridX + ((i + 1) * GridSizeV)))
                        {
                            flagLineStartX = true;
                            ColumnLineStartH = i;
                        }
                        if (start.Y >= (startGridY + (i * GridSizeH)) && start.Y < (startGridY + ((i + 1) * GridSizeH)))
                        {
                            flagLineStartY = true;
                            ColumnLineStartV = i;
                        }
                        if (end.X >= (startGridX + (i * GridSizeV)) && end.X < (startGridX + ((i + 1) * GridSizeV)))
                        {
                            flagLineEndX = true;
                            ColumnLineEndH = i;
                        }
                        if (end.Y >= (startGridY + (i * GridSizeH)) && end.Y < (startGridY + ((i + 1) * GridSizeH)))
                        {
                            flagLineEndY = true;
                            ColumnLineEndV = i;
                        }
                    }

                    if (ColumnLineEndV < 0 || ColumnLineEndH < 0 || ColumnLineStartV < 0 || ColumnLineStartH < 0)
                    {
                        //WorkSpace.Children.Remove(rect);
                    }
                    else
                    {
                        FirstObject = -1;
                        SecondObject = -1;

                        for (int i = 0; i < matrix.Count; i++)
                        {
                            if (ColumnLineStartV >= Convert.ToInt32(matrix[i][2]) &&
                                 ColumnLineStartV < Convert.ToInt32(matrix[i][3]) &&
                                 ColumnLineStartH >= Convert.ToInt32(matrix[i][4]) &&
                                 ColumnLineStartH < Convert.ToInt32(matrix[i][5]))
                            {
                                //matrix[i].Add(TextLine);
                                FirstObject = i;
                            }

                            else if (ColumnLineEndV >= Convert.ToInt32(matrix[i][2]) &&
                                        ColumnLineEndV < Convert.ToInt32(matrix[i][3]) &&
                                        ColumnLineEndH >= Convert.ToInt32(matrix[i][4]) &&
                                        ColumnLineEndH < Convert.ToInt32(matrix[i][5]))
                            {
                                SecondObject = i;
                            }
                        }

                        if (FirstObject < 0 || SecondObject < 0)
                        {
                            MessageBox.Show("Error Link Line");//WorkSpace
                            WorkSpace.Children.RemoveAt(indexOldLine);
                        }
                        else
                        {
                            richTextBox2.AppendText(matrix[FirstObject][0] + "-(T=" + matrix[FirstObject][2]
                                                                 + ",B=" + matrix[FirstObject][3]
                                                                 + ",L=" + matrix[FirstObject][4]
                                                                 + ",R=" + matrix[FirstObject][5] + ")");
                            richTextBox2.AppendText(System.Environment.NewLine);
                            richTextBox2.AppendText(" <--Line Link--> ");// + matrix[FirstObject][matrix[FirstObject].Count - 1] + " Link--> ");
                            richTextBox2.AppendText(System.Environment.NewLine);

                            richTextBox2.AppendText(matrix[SecondObject][0] + "-(T=" + matrix[SecondObject][2]
                                                                 + ",B=" + matrix[SecondObject][3]
                                                                 + ",L=" + matrix[SecondObject][4]
                                                                 + ",R=" + matrix[SecondObject][5] + ")");
                            richTextBox2.AppendText(System.Environment.NewLine);
                            richTextBox2.AppendText("________________________________________");
                            richTextBox2.AppendText(System.Environment.NewLine);

                            ///////////////////////////////////////PORT & PORT//////////////////////////////////////////
                            if (matrix[FirstObject][1] == "PORT" && matrix[SecondObject][1] == "PORT")
                            {
                                matrix[SecondObject][6] = matrix[FirstObject][0];
                            }
                            ///////////////////////////////////////PORT & AND//////////////////////////////////////////
                            if (matrix[FirstObject][1] == "PORT" && matrix[SecondObject][1] == "AND")
                            {
                                //matrix[FirstObject].Add(
                                matrix[SecondObject][6] = (Convert.ToInt32(matrix[SecondObject][6]) + 1).ToString();
                                //MessageBox.Show(matrix[SecondObject][6].ToString());
                                if (matrix[SecondObject][6] == "1") matrix[SecondObject][7] = matrix[FirstObject][0];
                                else if (matrix[SecondObject][6] == "2") matrix[SecondObject][8] = matrix[FirstObject][0];
                                if (matrix[SecondObject][7] != "0" && matrix[SecondObject][8] != "0")
                                {
                                    if (matrix[SecondObject][7].Substring(0, 1) == "P" && matrix[SecondObject][8].Substring(0, 1) == "P")
                                    {
                                        matrix[SecondObject][10] = "1";
                                    }
                                    else if (matrix[SecondObject][7].Substring(0, 1) == "B" || matrix[SecondObject][8].Substring(0, 1) == "B")
                                    {
                                        matrix[SecondObject][10] = (Convert.ToInt32(matrix[SecondObject][10]) + 1).ToString();
                                    }
                                }
                            }
                            else if (matrix[FirstObject][1] == "AND" && matrix[SecondObject][1] == "PORT")
                            {
                                matrix[FirstObject][9] = matrix[SecondObject][0];
                            }
                            else if (matrix[FirstObject][1] == "AND" && matrix[SecondObject][1] == "AND")
                            {
                                matrix[FirstObject][0] = "B" + countBuffer;
                                matrix[FirstObject][9] = "B" + countBuffer++;
                                matrix[SecondObject][6] = (Convert.ToInt32(matrix[SecondObject][6]) + 1).ToString();
                                //MessageBox.Show(matrix[SecondObject][6].ToString());
                                if (matrix[SecondObject][6] == "1")
                                {
                                    matrix[SecondObject][7] = matrix[FirstObject][9];
                                    matrix[SecondObject][10] = (Convert.ToInt32(matrix[FirstObject][10]) + 1).ToString();
                                }
                                else if (matrix[SecondObject][6] == "2")
                                {
                                    matrix[SecondObject][8] = matrix[FirstObject][9];

                                    if (matrix[SecondObject][7].Substring(0, 1) == "B" || matrix[SecondObject][8].Substring(0, 1) == "B")
                                    {
                                        if (Convert.ToInt32(matrix[FirstObject][10]) > Convert.ToInt32(matrix[SecondObject][10]))
                                        {
                                            matrix[SecondObject][10] = (Convert.ToInt32(matrix[FirstObject][10]) + 1).ToString();
                                        }
                                    }
                                }
                            }
                            ///////////////////////////////////////PORT & OR//////////////////////////////////////////
                            if (matrix[FirstObject][1] == "PORT" && matrix[SecondObject][1] == "OR")
                            {
                                //matrix[FirstObject].Add(
                                matrix[SecondObject][6] = (Convert.ToInt32(matrix[SecondObject][6]) + 1).ToString();
                                //MessageBox.Show(matrix[SecondObject][6].ToString());
                                if (matrix[SecondObject][6] == "1") matrix[SecondObject][7] = matrix[FirstObject][0];
                                else if (matrix[SecondObject][6] == "2") matrix[SecondObject][8] = matrix[FirstObject][0];
                                if (matrix[SecondObject][7] != "0" && matrix[SecondObject][8] != "0")
                                {
                                    if (matrix[SecondObject][7].Substring(0, 1) == "P" && matrix[SecondObject][8].Substring(0, 1) == "P")
                                    {
                                        matrix[SecondObject][10] = "1";
                                    }
                                    else if (matrix[SecondObject][7].Substring(0, 1) == "B" || matrix[SecondObject][8].Substring(0, 1) == "B")
                                    {
                                        matrix[SecondObject][10] = (Convert.ToInt32(matrix[SecondObject][10]) + 1).ToString();
                                    }
                                }
                            }
                            else if (matrix[FirstObject][1] == "OR" && matrix[SecondObject][1] == "PORT")
                            {
                                matrix[FirstObject][9] = matrix[SecondObject][0];
                            }
                            else if (matrix[FirstObject][1] == "OR" && matrix[SecondObject][1] == "OR")
                            {
                                matrix[FirstObject][0] = "B" + countBuffer;
                                matrix[FirstObject][9] = "B" + countBuffer++;
                                matrix[SecondObject][6] = (Convert.ToInt32(matrix[SecondObject][6]) + 1).ToString();
                                //MessageBox.Show(matrix[SecondObject][6].ToString());
                                if (matrix[SecondObject][6] == "1")
                                {
                                    matrix[SecondObject][7] = matrix[FirstObject][9];
                                    matrix[SecondObject][10] = (Convert.ToInt32(matrix[FirstObject][10]) + 1).ToString();
                                }
                                else if (matrix[SecondObject][6] == "2")
                                {
                                    matrix[SecondObject][8] = matrix[FirstObject][9];

                                    if (matrix[SecondObject][7].Substring(0, 1) == "B" || matrix[SecondObject][8].Substring(0, 1) == "B")
                                    {
                                        if (Convert.ToInt32(matrix[FirstObject][10]) > Convert.ToInt32(matrix[SecondObject][10]))
                                        {
                                            matrix[SecondObject][10] = (Convert.ToInt32(matrix[FirstObject][10]) + 1).ToString();
                                        }
                                    }
                                }
                            }
                            ///////////////////////////////////////PORT & NOT//////////////////////////////////////////
                            if (matrix[FirstObject][1] == "PORT" && matrix[SecondObject][1] == "NOT")
                            {
                                matrix[SecondObject][6] = (Convert.ToInt32(matrix[SecondObject][6]) + 1).ToString();
                                matrix[SecondObject][7] = matrix[FirstObject][0];
                                matrix[SecondObject][10] = "1";
                            }
                            else if (matrix[FirstObject][1] == "NOT" && matrix[SecondObject][1] == "PORT")
                            {
                                matrix[FirstObject][9] = matrix[SecondObject][0];
                            }
                            else if (matrix[FirstObject][1] == "NOT" && matrix[SecondObject][1] == "NOT")
                            {
                                matrix[FirstObject][0] = "B" + countBuffer;
                                matrix[FirstObject][9] = "B" + countBuffer++;

                                matrix[SecondObject][7] = matrix[FirstObject][9];
                                matrix[SecondObject][10] = (Convert.ToInt32(matrix[FirstObject][10]) + 1).ToString();
                            }
                            ///////////////////////////////////////AND Link OR/////////////////////////////////////////
                            else if (matrix[FirstObject][1] == "AND" && matrix[SecondObject][1] == "OR")
                            {
                                matrix[FirstObject][0] = "B" + countBuffer;
                                matrix[FirstObject][9] = "B" + countBuffer++;
                                matrix[SecondObject][6] = (Convert.ToInt32(matrix[SecondObject][6]) + 1).ToString();
                                //MessageBox.Show(matrix[SecondObject][6].ToString());
                                if (matrix[SecondObject][6] == "1")
                                {
                                    matrix[SecondObject][7] = matrix[FirstObject][9];
                                    matrix[SecondObject][10] = (Convert.ToInt32(matrix[FirstObject][10]) + 1).ToString();
                                }
                                else if (matrix[SecondObject][6] == "2")
                                {
                                    matrix[SecondObject][8] = matrix[FirstObject][9];

                                    if (matrix[SecondObject][7].Substring(0, 1) == "B" || matrix[SecondObject][8].Substring(0, 1) == "B")
                                    {
                                        if (Convert.ToInt32(matrix[FirstObject][10]) > Convert.ToInt32(matrix[SecondObject][10]))
                                        {
                                            matrix[SecondObject][10] = (Convert.ToInt32(matrix[FirstObject][10]) + 1).ToString();
                                        }
                                    }
                                }
                            }
                            else if (matrix[FirstObject][1] == "OR" && matrix[SecondObject][1] == "AND")
                            {
                                matrix[FirstObject][0] = "B" + countBuffer;
                                matrix[FirstObject][9] = "B" + countBuffer++;
                                matrix[SecondObject][6] = (Convert.ToInt32(matrix[SecondObject][6]) + 1).ToString();
                                //MessageBox.Show(matrix[SecondObject][6].ToString());
                                if (matrix[SecondObject][6] == "1")
                                {
                                    matrix[SecondObject][7] = matrix[FirstObject][9];
                                    matrix[SecondObject][10] = (Convert.ToInt32(matrix[FirstObject][10]) + 1).ToString();
                                }
                                else if (matrix[SecondObject][6] == "2")
                                {
                                    matrix[SecondObject][8] = matrix[FirstObject][9];

                                    if (matrix[SecondObject][7].Substring(0, 1) == "B" || matrix[SecondObject][8].Substring(0, 1) == "B")
                                    {
                                        if (Convert.ToInt32(matrix[FirstObject][10]) > Convert.ToInt32(matrix[SecondObject][10]))
                                        {
                                            matrix[SecondObject][10] = (Convert.ToInt32(matrix[FirstObject][10]) + 1).ToString();
                                        }
                                    }
                                }
                            }
                            ///////////////////////////////////////AND Link NOT/////////////////////////////////////////
                            else if (matrix[FirstObject][1] == "AND" && matrix[SecondObject][1] == "NOT")
                            {
                                matrix[FirstObject][0] = "B" + countBuffer;
                                matrix[FirstObject][9] = "B" + countBuffer++;

                                matrix[SecondObject][7] = matrix[FirstObject][9];
                                matrix[SecondObject][10] = (Convert.ToInt32(matrix[FirstObject][10]) + 1).ToString();
                            }
                            else if (matrix[FirstObject][1] == "NOT" && matrix[SecondObject][1] == "AND")
                            {
                                matrix[FirstObject][0] = "B" + countBuffer;
                                matrix[FirstObject][9] = "B" + countBuffer++;
                                matrix[SecondObject][6] = (Convert.ToInt32(matrix[SecondObject][6]) + 1).ToString();
                                //MessageBox.Show(matrix[SecondObject][6].ToString());
                                if (matrix[SecondObject][6] == "1")
                                {
                                    matrix[SecondObject][7] = matrix[FirstObject][9];
                                    matrix[SecondObject][10] = (Convert.ToInt32(matrix[FirstObject][10]) + 1).ToString();
                                }
                                else if (matrix[SecondObject][6] == "2")
                                {
                                    matrix[SecondObject][8] = matrix[FirstObject][9];
                                }
                            }
                            ///////////////////////////////////////OR Link NOT/////////////////////////////////////////
                            else if (matrix[FirstObject][1] == "OR" && matrix[SecondObject][1] == "NOT")
                            {
                                matrix[FirstObject][0] = "B" + countBuffer;
                                matrix[FirstObject][9] = "B" + countBuffer++;

                                matrix[SecondObject][7] = matrix[FirstObject][9];
                                matrix[SecondObject][10] = (Convert.ToInt32(matrix[FirstObject][10]) + 1).ToString();
                            }
                            else if (matrix[FirstObject][1] == "NOT" && matrix[SecondObject][1] == "OR")
                            {
                                matrix[FirstObject][0] = "B" + countBuffer;
                                matrix[FirstObject][9] = "B" + countBuffer++;
                                matrix[SecondObject][6] = (Convert.ToInt32(matrix[SecondObject][6]) + 1).ToString();
                                //MessageBox.Show(matrix[SecondObject][6].ToString());
                                if (matrix[SecondObject][6] == "1")
                                {
                                    matrix[SecondObject][7] = matrix[FirstObject][9];
                                    matrix[SecondObject][10] = (Convert.ToInt32(matrix[FirstObject][10]) + 1).ToString();
                                }
                                else if (matrix[SecondObject][6] == "2")
                                {
                                    matrix[SecondObject][8] = matrix[FirstObject][9];
                                }
                            }

                            matrix[SecondObject][matrix[SecondObject].Count - 1] += lineName;// "line" + (countLine - 1).ToString();
                            matrix[FirstObject][matrix[FirstObject].Count - 1] += lineName;// "line" + (countLine - 1).ToString();

                        }
                    }

                }

            }//Delete Object
            else if (flagDelete)
            {
                Point pt = e.GetPosition(WorkSpace);

                flagDeleteLine = true;
                ColumnH = -1;
                ColumnV = -1;

                for (int i = 0; i < 64; i++)
                {
                    if (pt.X >= (startGridX + (i * GridSizeV)) && pt.X < (startGridX + ((i + 1) * GridSizeV)))
                    {
                        flagColumnH = true;
                        ColumnH = i;
                    }
                    if (pt.Y >= (startGridY + (i * GridSizeH)) && pt.Y < (startGridY + ((i + 1) * GridSizeH)))
                    {
                        flagColumnV = true;
                        ColumnV = i;
                    }
                }

                for (int i = 0; i < matrix.Count; i++)
                {
                    if (ColumnV >= Convert.ToInt32(matrix[i][2]) &&
                         ColumnV < Convert.ToInt32(matrix[i][3]) &&
                         ColumnH >= Convert.ToInt32(matrix[i][4]) &&
                         ColumnH < Convert.ToInt32(matrix[i][5]))
                    {

                        target = VisualTreeHelper.HitTest(WorkSpace, pt);

                        if (target != null)
                        {
                            WorkSpace.Children.Remove(target.VisualHit as UIElement);
                        }

                        for (int ii = Convert.ToInt16(matrix[i][2]); ii < Convert.ToInt16(matrix[i][3]); ii++)
                        {
                            for (int jj = Convert.ToInt16(matrix[i][4]); jj < Convert.ToInt16(matrix[i][5]); jj++)
                            {
                                gridArray[ii, jj] = 0;
                            }
                        }


                        richTextBox1.Document.Blocks.Clear();
                        srt = "";
                        for (int j = 0; j < 63; j++)
                        {
                            for (int k = 0; k < 63; k++)
                            {
                                srt += gridArray[j, k];
                            }
                            srt += System.Environment.NewLine;
                        }
                        richTextBox1.AppendText(srt);

                        matrix.RemoveAt(i);

                        flagDeleteLine = false;
                    }
                }

                if (flagDeleteLine)
                {
                    target = VisualTreeHelper.HitTest(WorkSpace, pt);

                    if (target != null)
                    {
                        //WorkSpace.Children.Remove(target.VisualHit as UIElement);

                        Line src = target.VisualHit as Line;
                        //MessageBox.Show(src.Name.ToString());
                        WorkSpace.Children.Remove(target.VisualHit as UIElement);

                        for (int i = 0; i < matrix.Count; i++)
                        {
                            if (matrix[i][matrix[i].Count - 1].ToString().Contains(src.Name))
                            {
                                if (matrix[i][1].ToString() == "PORT")
                                {
                                    matrix[i][6] = "0";
                                    matrix[i][matrix[i].Count - 1] = matrix[i][matrix[i].Count - 1].ToString().Replace(src.Name, "");
                                }
                                else
                                {
                                    matrix[i][0] = "GATE";
                                    matrix[i][6] = "0";
                                    matrix[i][7] = "0";
                                    matrix[i][8] = "0";
                                    matrix[i][9] = "0";
                                    matrix[i][10] = "0";
                                    matrix[i][matrix[i].Count - 1] = matrix[i][matrix[i].Count - 1].ToString().Replace(src.Name, "");
                                }
                                //MessageBox.Show("Found");
                            }
                        }
                    }
                }
            }
        }

        private void RedrawObject(Point pt)
        {
            ColumnH = -1;
            ColumnV = -1;

            for (int i = 0; i < 64; i++)
            {
                if (pt.X >= (startGridX + (i * GridSizeV)) && pt.X < (startGridX + ((i + 1) * GridSizeV)))
                {
                    flagColumnH = true;
                    ColumnH = i;
                }
                if (pt.Y >= (startGridY + (i * GridSizeH)) && pt.Y < (startGridY + ((i + 1) * GridSizeH)))
                {
                    flagColumnV = true;
                    ColumnV = i;
                }
            }

            if (ColumnV >= 0 && ColumnH >= 0)
            {
                for (int i = 0; i < matrix.Count; i++)
                {
                    if (ColumnV >= Convert.ToInt32(matrix[i][2]) &&
                         ColumnV < Convert.ToInt32(matrix[i][3]) &&
                         ColumnH >= Convert.ToInt32(matrix[i][4]) &&
                         ColumnH < Convert.ToInt32(matrix[i][5]))
                    {
                        pointL = Convert.ToInt32(matrix[i][4]);
                        pointT = Convert.ToInt32(matrix[i][2]);

                        left = startGridX + (pointL * GridSizeV);
                        top = startGridY + (pointT * GridSizeH);

                        Image carImg = new Image();
                        carImg.Name = matrix[i][0];
                        carImg.Tag = matrix[i][1];

                        target = VisualTreeHelper.HitTest(WorkSpace, pt);

                        Image src = target.VisualHit as Image;
                        //MessageBox.Show(src.Source.ToString());

                        WorkSpace.Children.Remove(target.VisualHit as UIElement);
                        ObjList.RemoveAt(i);

                        Image myImage3 = new Image();
                        BitmapImage bi3 = new BitmapImage();
                        bi3.BeginInit();
                        string srctmp = src.Source.ToString();
                        string tmp = "pack://application:,,,/";
                        srctmp = srctmp.Replace(tmp, "");
                        //pack://application:,,,/
                        bi3.UriSource = new Uri(srctmp, UriKind.Relative);
                        bi3.EndInit();
                        myImage3.Stretch = Stretch.Fill;
                        // myImage3.Source = bi3;
                        carImg.Source = bi3;

                        carImg.Width = 60;
                        carImg.Height = 30;

                        ObjList.Insert(i, carImg);
                        WorkSpace.Children.Add(ObjList[i]);

                        Canvas.SetTop(carImg, top);
                        Canvas.SetLeft(carImg, left);
                        break;
                    }
                }
            }
        }
        private void ellipse_MouseMove(object sender, MouseEventArgs e)
        {
            if (false)//flagCursor)
            {
                if (activated)
                {
                    //img = (Image)e.OriginalSource;
                    top = Canvas.GetTop(img) + Mouse.GetPosition(img).Y - point.Y;
                    Canvas.SetTop(img, top);

                    left = Canvas.GetLeft(img) + Mouse.GetPosition(img).X - point.X;
                    Canvas.SetLeft(img, left);

                    //XDevice = top + (ellipse.Height / 2);
                    //YDevice = left + (ellipse.Width / 2);

                    DHDevice = (img.Height / 2);
                    DWDevice = (img.Width / 2);
                    // Find Center Object
                    YDevice = top + DHDevice;
                    XDevice = left + DWDevice;

                    label1.Content = XDevice.ToString();
                    label2.Content = YDevice.ToString();

                    LDevice = XDevice - DWDevice;
                    RDevice = XDevice + DWDevice;

                    top = YDevice - DHDevice;
                    left = XDevice - DWDevice;

                    label3.Content = top.ToString();
                    label4.Content = left.ToString();

                    TDevice = YDevice - DHDevice;
                    BDevice = YDevice + DHDevice;

                    label5.Content = TDevice.ToString();
                    label6.Content = BDevice.ToString();
                }
            }
        }

        private void ellipse_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (false)//(flagCursor)
            {
                var rect = (Image)e.OriginalSource;
                CalculatePosition(rect);
            }
        }

        private ArrayList LoadListBoxDataIOInput()
        {
            ArrayList itemsList = new ArrayList();
            /*for (int i = 1; i < 33; i++)
            {
                itemsList.Add("Input " + i);
            }*/
            string line;

            // Read the file and display it line by line.
            System.IO.StreamReader file =
               new System.IO.StreamReader("c:\\IEDUP/file/InputIO.txt");
            while ((line = file.ReadLine()) != null)
            {
                itemsList.Add(line);
            }

            file.Close();

            return itemsList;
        }

        private ArrayList LoadListBoxDataIOOutput()
        {
            ArrayList itemsList = new ArrayList();
            /*for (int i = 1; i < 33; i++)
            {
                itemsList.Add("Output " + i);
            }*/
            string line;

            // Read the file and display it line by line.
            System.IO.StreamReader file =
               new System.IO.StreamReader("c:\\IEDUP/file/OutputIO.txt");
            while ((line = file.ReadLine()) != null)
            {
                itemsList.Add(line);
            }

            file.Close();
            return itemsList;
        }

        private ArrayList LoadListBoxDataGooseInput()
        {
            ArrayList itemsList = new ArrayList();
            for (int i = 1; i < 17; i++)
            {
                itemsList.Add("Goose In " + i);
            }
            return itemsList;
        }

        private ArrayList LoadListBoxDataGooseOutput()
        {
            ArrayList itemsList = new ArrayList();
            for (int i = 1; i < 17; i++)
            {
                itemsList.Add("Goose Out " + i);
            }
            return itemsList;
        }

        private ArrayList LoadListBoxDataFunctionKey()
        {
            ArrayList itemsList = new ArrayList();
            for (int i = 1; i < 9; i++)
            {
                itemsList.Add("Function Key " + i);
            }
            return itemsList;
        }

        private ArrayList LoadListBoxDataLED()
        {
            ArrayList itemsList = new ArrayList();
            for (int i = 1; i < 9; i++)
            {
                itemsList.Add("LED_" + i);
            }
            return itemsList;
        }



        public void SelectDevice(ArrayList LoadListBoxData)
        {
            SelectDevice popup = new SelectDevice();
            popup.listDevice.ItemsSource = LoadListBoxData;
            popup.ShowDialog();
            try
            {
                string[] parts = popup.device.Split('\t');
                device = parts[0];
                //device = popup.device;
                Nodevice = popup.Nodevice;
            }
            catch(Exception e){
                device = popup.device;
                Nodevice = popup.Nodevice;
            }
            //MessageBox.Show(Nodevice.ToString());
        }

        void ClearFlag()
        {
            flagCursor = false;
            flagLine = false;
            flagDelete = false;
            flagGenerateSCD = false;
            flagInputIO = false;
            flagOutputIO = false;
            flagGateAND = false;
            flagGateOR = false;
            flagGateNOT = false;
            flagInputGoose = false;
            flagOutputGoose = false;
            flagFK = false;
            flagLED = false;
        }

        public void LoadGesturesFromFileDance(string fileLocation)
        {
            ClearFlag();
            flagNew = true;
            WorkSpace.Children.Clear();
            ObjList.Clear();
            matrix.Clear();
            LineList.Clear();

            gridArray = new int[63, 63];
            for (int i = 0; i < 63; i++)
            {
                for (int j = 0; j < 63; j++)
                {
                    gridArray[i, j] = 0;
                }
            }
           
            string line;
            bool loadLine = false;
            //เปิดอ่านไฟล์ และ แสดงข้อมูลแถวแต่ละแถว
            System.IO.StreamReader file = new System.IO.StreamReader(fileLocation);
            while ((line = file.ReadLine()) != null)//แถวนี้มีข้อมูล?
            {
                //เจอ + ให้ทำการเก็บข้อมูลลงตัวแปร ArrayList
                if (!loadLine)
                {
                    if (line.StartsWith("+"))
                    {
                        matrix.Add(new List<String>());
                        //frames.Add(items);
                        //itemCount = 0;
                        //items = new double[60];
                        continue;
                    }

                    if (line.StartsWith("-"))
                    {
                        matrix[matrix.Count - 1].Add(line.Substring(1));
                        //matrix.Add(new List<String>());
                        //frames.Add(items);
                        //itemCount = 0;
                        //items = new double[60];
                        continue;
                    }

                    //ยังไม่เจอ ! ให้แปลงค่า String เป็น Double
                    /* if (!line.StartsWith("!"))
                     {
                         //items[itemCount] = Double.Parse(line);

                     }*/

                    //itemCount++;
                    //เจอ ! ทำการถ่ายค่า ArrayList
                    if (line.StartsWith("/"))
                    {
                        loadLine = true;
                        //temp = frames;
                    }
                    //ยังไม่เจอ ! ให้แปลงค่า String เป็น Double
                    if (line.StartsWith("!"))
                    {
                        //items[itemCount] = Double.Parse(line);
                        break;
                    }
                }
                else
                {
                    if (line.StartsWith("+"))
                    {
                        LineList.Add(new List<String>());
                        continue;
                    }

                    if (line.StartsWith("-"))
                    {
                        LineList[LineList.Count - 1].Add(line.Substring(1));
                    }

                    if (line.StartsWith("!"))
                    {
                        break;
                    }
                }
                
            }

            file.Close();
        }
       
        private void tlbTray_Click(object sender, RoutedEventArgs e)
        {
            ClearFlag();
            Button button = (Button)e.OriginalSource;
            //MessageBox.Show(button.Name.ToString());
            if (button.Name == "New")
            {
                New_WorkSpace();
            }
            else if (button.Name == "Template")
            {
                //MessageBox.Show("Open Template");
                OpenFile();
            }
            else if (button.Name == "Open")
            {
                OpenFile();
            }
            else if (button.Name == "Save")
            {
                SaveFile();
            }

            if (button.Name == "Cursor")
            {
                flagCursor = true;
                
                // MessageBox.Show("Cursor");
            }
            else if (button.Name == "Wire")
            {
                flagLine = true;
                //MessageBox.Show("Wire");
            }
            else if (button.Name == "Delete")
            {
                flagDelete = true;
                // MessageBox.Show("Rubber");
            }
            else if (button.Name == "InputIO")
            {
                flagInputIO = true;
                SelectDevice(LoadListBoxDataIOInput());

                // MessageBox.Show("IO");
            }
            else if (button.Name == "OutputIO")
            {
                flagOutputIO = true;

                SelectDevice(LoadListBoxDataIOOutput());
                // MessageBox.Show("OutputIO");
            }
            else if (button.Name == "InputGoose")
            {
                flagInputGoose = true;

                SelectDevice(LoadListBoxDataGooseInput());
                // MessageBox.Show("OutputIO");
            }
            else if (button.Name == "OutputGoose")
            {
                flagOutputGoose = true;

                SelectDevice(LoadListBoxDataGooseOutput());
                // MessageBox.Show("OutputIO");
            }
            else if (button.Name == "FK")
            {
                flagFK = true;

                SelectDevice(LoadListBoxDataFunctionKey());
                // MessageBox.Show("OutputIO");
            }
            else if (button.Name == "LED")
            {
                flagLED = true;

                SelectDevice(LoadListBoxDataLED());
                // MessageBox.Show("OutputIO");
            }
            else if (button.Name == "GateAND")
            {
                flagGateAND = true;
                // MessageBox.Show("GateAND");
            }
            else if (button.Name == "GateOR")
            {
                flagGateOR = true;
                //  MessageBox.Show("GateOR");
            }
            else if (button.Name == "GateNOT")
            {
                flagGateNOT = true;
                //  MessageBox.Show("GateNOT");
            }

            else if (button.Name == "Generate")
            {
                GenerateAutoFuntion();
            }
        }
        private void New_WorkSpace()
        {
            //MessageBox.Show("New");
            flagNew = true;
            WorkSpace.Children.Clear();
            ObjList.Clear();
            matrix.Clear();
            LineList.Clear();

            gridArray = new int[63, 63];
            for (int i = 0; i < 63; i++)
            {
                for (int j = 0; j < 63; j++)
                {
                    gridArray[i, j] = 0;
                }
            }
        }
        private void GenerateAutoFuntion()
        {
            flagGenerateSCD = true;
            int SetLV = 10;
            int lv, count = 0, countBuf = 0;
            //string[] firstCmd = new string[SetLV];
            List<string> firstCmd = new List<string>();
            string[] buf = new string[100];
            string[] P = new string[SetLV];
            string cmd = "", tmp = "", fullcmd = "", Oper = "";
            for (lv = 0; lv < SetLV; lv++)
                for (int i = 0; i < matrix.Count; i++)
                {
                    //Is Gate
                    if (matrix[i].Count > 10)
                    {   //Gate LV = LV
                        if (matrix[i][10] == lv.ToString() && matrix[i][9] != "0")
                        {
                            cmd += matrix[i][9];
                            cmd += "=";
                            if (matrix[i][1] == "NOT")
                            {
                                cmd += " not " + matrix[i][7];
                                //continue;
                            }
                            else
                            {
                                cmd += matrix[i][7];
                                if (matrix[i][1] == "AND") cmd += " and ";
                                else if (matrix[i][1] == "OR") cmd += " or ";

                                if (matrix[i][8] != "0") cmd += matrix[i][8];
                            }
                            //cmd += "#";
                            //firstCmd[count++] = cmd;
                            //fullcmd += firstCmd[count - 1] + newLine;
                            firstCmd.Add(cmd);
                            count++;
                            fullcmd += firstCmd[count - 1] + newLine;
                            cmd = "";
                        }
                    }
                    else
                    {   //Port 
                        if (lv == 0)
                            if (matrix[i][6] != "0")
                            {
                                cmd += ">" + matrix[i][0];
                                cmd += "=";
                                cmd += matrix[i][6];
                                firstCmd.Add(cmd);
                                count++;
                                fullcmd += firstCmd[count - 1] + newLine;
                                cmd = "";
                            }
                    }

                }

            bool firstIn = true;
            int found;
            int countB;
            int round = firstCmd.Count();
            for (count = 1; count < round + 1; count++)
            {
                countB = 0;
                while ((countB = firstCmd[count - 1].IndexOf('B', countB)) != -1 &&
                       (countB = firstCmd[count - 1].IndexOf('B', countB)) != 0)
                {
                    try
                    {
                        buf[countBuf++] = firstCmd[count - 1].Substring(countB, 4);
                        for (int j = 0; j < round; j++)
                            //if (firstCmd[j].Contains(buf[countBuf - 1]))
                            if (firstCmd[j].Substring(0, 4)==buf[countBuf - 1])
                            {
                                found = firstCmd[j].IndexOf("=");
                                tmp = firstCmd[j].Substring(found + 1);
                                break;
                            }
                        tmp = "(" + tmp + ")";
                        //MessageBox.Show(tmp);
                        firstCmd[count - 1] = firstCmd[count - 1].Replace(buf[countBuf - 1], tmp);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }

                    // Increment the index.
                    countB++;
                }
            }
            for (int i = 0; i < round; i++)
            {
                if (firstCmd[i].IndexOf(">") == -1 && firstCmd[i].IndexOf("B") == -1)
                {
                    firstCmd[i] = ">" + firstCmd[i];
                }
            }
            str.Clear();// = " ";
            strGoose.Clear();
            for (int i = 0; i < round; i++)
            {
                if (firstCmd[i].IndexOf(">") != -1)
                {
                    if (firstCmd[i].Contains("not"))
                    {
                        firstCmd[i] = firstCmd[i].Replace("not", "!");
                    }
                    if (firstCmd[i].Contains("and"))
                    {
                        firstCmd[i] = firstCmd[i].Replace("and", "&");
                    }
                    if (firstCmd[i].Contains("or"))
                    {
                        firstCmd[i] = firstCmd[i].Replace("or", "|");
                    }

                    fullcmd = "\t\t" + firstCmd[i].Substring(1) + ";" + newLine;
                    if (fullcmd.Contains("Goose"))
                    {
                        strGoose.Append(fullcmd);
                    }
                    else{
                        str.Append(fullcmd);
                    }
                }
            }

            System.IO.StreamReader file = new System.IO.StreamReader("c:\\IEDUP/file/beagle_demo_Template.c");
            string line, strtmp = "";
            //string newLine = "\r\n";
            while ((line = file.ReadLine()) != null)
            {
                int cnt = line.Split('[').Length - 1;
                if (cnt > 0)
                {
                    /* IDE Detail ----------------------------------------------------------------- */
                    if (line.Contains("/*[AutoFunctionGGIO]*/"))
                        line = line.Replace("/*[AutoFunctionGGIO]*/", str.ToString());
                    if (line.Contains("/*[AutoFunctionGoose]*/"))
                        line = line.Replace("/*[AutoFunctionGoose]*/", strGoose.ToString());
                    strtmp += line + newLine;
                }
                else
                {
                    strtmp += line + newLine;
                }
            }

            file.Close();

            System.IO.File.WriteAllText("C:\\IEDUP/iecfile/beagle_demo.c", strtmp.ToString());
            loop = 0;
            MessageBoxResult result = MessageBox.Show("Generate suscess!\nDo you upload to Bay?", "Upload", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    progressBar1.Value = 0;
                    using (var c = new ScpClient("192.168.7.2", "root", ""))
                    {
                        c.Connect();
                        //c.Upload(new DirectoryInfo(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "/IECFile"), "/lib/lib/demos/beaglebone/");
                        c.Upload(new DirectoryInfo("C:/IEDUP/IECFile"), "/lib/lib/demos/beaglebone/");
                        c.Disconnect();
                    }

                    using (var client = new SshClient("192.168.7.2", "root", ""))
                    {
                        client.Connect();
                        client.RunCommand("reboot");
                        client.Disconnect();
                    }
                    cp = true;
                    //MessageBox.Show("Generate Suscess");
                }
                catch (Exception n)
                {
                    Console.WriteLine("Error: {0}", n);
                    // timer2.Enabled = true;
                    dispatcherTimer2.Start();
                    cp = false;
                }
            }
        }

        private void SaveFile()
        {
            String cmd = "";
            cmd += "#Device" + newLine;
            for (int i = 0; i < matrix.Count; i++)
            {
                cmd += "+" + newLine;
                for (int j = 0; j < matrix[i].Count; j++)
                {
                    cmd += "-" + matrix[i][j].ToString() + newLine;
                }
            }

            cmd += "/Line" + newLine;
            for (int i = 0; i < WorkSpace.Children.Count; i++)
            {
                if (VisualTreeHelper.GetChild(WorkSpace, i).GetType().ToString() == "System.Windows.Shapes.Line")
                {
                    Line src = VisualTreeHelper.GetChild(WorkSpace, i) as Line;
                    //MessageBox.Show("X1 = " + src.X1.ToString() + " Y1 = " + src.Y1.ToString());
                    //MessageBox.Show("X2 = " + src.X2.ToString() + " Y2 = " + src.Y2.ToString());
                    //cmd += "#" + countSaveLine++ + newLine;
                    cmd += "+" + newLine;
                    cmd += "-" + src.X1.ToString() + newLine;
                    cmd += "-" + src.Y1.ToString() + newLine;
                    cmd += "-" + src.X2.ToString() + newLine;
                    cmd += "-" + src.Y2.ToString() + newLine;
                }
            }

            cmd += "!";
            fileName = "SaveGrid.txt";// +DateTime.Now.ToString("yyyy-MM-dd_HH-mm") + ".txt";
            Microsoft.Win32.SaveFileDialog dl1 = new Microsoft.Win32.SaveFileDialog();
            dl1.FileName = "MYFileSave";
            dl1.DefaultExt = ".dat";
            dl1.Filter = "Auto Function Grid (.dat)|*.dat";
            Nullable<bool> result = dl1.ShowDialog();
            if (result == true)
            {
                string filename = dl1.FileName;
                System.IO.File.WriteAllText(filename, cmd.ToString());
                MessageBox.Show("Save Suscess");
            }
        }
        private void OpenFile()
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.Filter = "Auto Function (.dat)|*.dat";
            Nullable<bool> result = dlg.ShowDialog();
            if (result == true)
            {
                string filename = dlg.FileName;
                System.IO.File.OpenRead(filename);
                LoadGesturesFromFileDance(dlg.FileName);
                for (int i = 0; i < LineList.Count; i++)
                {
                    Point from = new Point(Int32.Parse(LineList[i][0]), Int32.Parse(LineList[i][1]));
                    Point to = new Point(Int32.Parse(LineList[i][2]), Int32.Parse(LineList[i][3]));
                    DrawLine(from, to, WorkSpace);
                }

                for (int i = 0; i < matrix.Count; i++)
                {
                    pointL = Convert.ToInt32(matrix[i][4]);
                    pointT = Convert.ToInt32(matrix[i][2]);

                    left = startGridX + (pointL * GridSizeV);
                    top = startGridY + (pointT * GridSizeH);

                    Image carImg = new Image();
                    carImg.Name = matrix[i][0];
                    carImg.Tag = matrix[i][1];
                    Image myImage3 = new Image();
                    BitmapImage bi3 = new BitmapImage();
                    bi3.BeginInit();
                  
                    string line;
                    int countPin = 0, countLine = 0;
                    bool foundName = false;
                    // Read the file and display it line by line.
                    if (carImg.Tag.ToString() == "PORT")
                    {
                        if (carImg.Name.ToString().Substring(0, 3) == "LED" || carImg.Name.ToString().Substring(0, 3) == "Fun"
                            || carImg.Name.ToString().Substring(0, 3) == "Goo")
                        {
                            bi3.UriSource = new Uri("Images/Gates/" + carImg.Name.ToString() + ".jpg", UriKind.Relative);
                        }
                        else
                        {
                            file = new System.IO.StreamReader("c:\\IEDUP/file/PinInput.txt");
                            while ((line = file.ReadLine()) != null)
                            {
                                if (carImg.Name == line)
                                {
                                    foundName = true;
                                    break;
                                }
                                else countPin++;
                            }
                            file.Close();

                            if (foundName)
                            {
                                file = new System.IO.StreamReader("c:\\IEDUP/file/InputIO.txt");
                                while ((line = file.ReadLine()) != null)
                                {
                                    if (countLine == countPin)
                                    {
                                        //imageName = line.ToString();
                                        string[] parts = line.Split('\t');
                                        imageName = parts[0];
                                        break;
                                    }
                                    else countLine++;
                                }
                                file.Close();
                            }
                            else
                            {
                                countPin = 0;
                                file = new System.IO.StreamReader("c:\\IEDUP/file/PinOutput.txt");
                                while ((line = file.ReadLine()) != null)
                                {
                                    if (carImg.Name == line) break;
                                    else countPin++;
                                }
                                file.Close();

                                file = new System.IO.StreamReader("c:\\IEDUP/file/OutputIO.txt");
                                while ((line = file.ReadLine()) != null)
                                {
                                    if (countLine == countPin)
                                    {
                                        //imageName = line.ToString();
                                        string[] parts = line.Split('\t');
                                        imageName = parts[0];
                                        break;
                                    }
                                    else countLine++;
                                }
                                file.Close();
                            }

                            string tmp = " ";
                            imageName = imageName.Replace(tmp, "");
                            bi3.UriSource = new Uri("Images/Gates/" + imageName.ToString() + ".jpg", UriKind.Relative);
                        }
                    }
                    else bi3.UriSource = new Uri("Images/Gates/" + carImg.Tag.ToString() + ".jpg", UriKind.Relative);


                    bi3.EndInit();
                    myImage3.Stretch = Stretch.Fill;
                    // myImage3.Source = bi3;
                    carImg.Source = bi3;

                    carImg.Width = 60;
                    carImg.Height = 30;

                    ObjList.Insert(i, carImg);
                    WorkSpace.Children.Add(ObjList[i]);

                    Canvas.SetTop(carImg, top);
                    Canvas.SetLeft(carImg, left);
                    //break;
                }
                MessageBox.Show("Open Suscess");
            }
        }

        private void DrawLine(Point From, Point To, Canvas TargetCanvas)
        {
            CurrentLine = new Line();
            CurrentLine.StrokeEndLineCap = PenLineCap.Round;
            CurrentLine.StrokeStartLineCap = PenLineCap.Round;
            CurrentLine.Stroke = Brushes.Black;
            CurrentLine.StrokeThickness = 2.0;
            CurrentLine.X1 = From.X;
            CurrentLine.Y1 = From.Y;
            CurrentLine.X2 = To.X;
            CurrentLine.Y2 = To.Y;
            CurrentLine.Name = "line" + countLine.ToString() + "_";
            countLine++;
            lineName = CurrentLine.Name;
            Canvas.SetLeft(TargetCanvas, From.X);
            Canvas.SetTop(TargetCanvas, From.Y);    
            TargetCanvas.Children.Add(CurrentLine);
        }

        private void image1_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //flagCursor = true;
            activated = true;
            if (flagLine && LineTime % 2 == 1)
            {
                //indexOldLine = -1;
                //start = e.GetPosition(this);
                MessageBox.Show("Error Link\nPlease Link Again");
                LineTime--;
            }
            else if (flagInputIO || flagOutputIO || flagGateAND || flagGateNOT || flagGateOR || flagInputGoose || flagOutputGoose || flagFK || flagLED)
            {

                try
                {

                    index++;

                    size = ObjList.Count;

                    point = Mouse.GetPosition(this);

                    Image carImg = new Image();

                    if (flagInputIO || flagOutputIO || flagInputGoose || flagOutputGoose || flagFK || flagLED)
                    {
                        Image myImage3 = new Image();
                        BitmapImage bi3 = new BitmapImage();
                        bi3.BeginInit();
                        bi3.UriSource = new Uri("images/Gates/" + device.Replace(" ", string.Empty) + ".jpg", UriKind.Relative);
                        bi3.EndInit();
                        myImage3.Stretch = Stretch.Fill;
                        // myImage3.Source = bi3;
                        carImg.Source = bi3;// new BitmapImage(new Uri("images/Gates/PORT.jpg"));
                        carImg.Tag = "PORT";

                        string line;
                        int countPin = 0;

                        // Read the file and display it line by line.
                        if (flagInputIO)
                        {
                            file = new System.IO.StreamReader("c:\\IEDUP/file/PinInput.txt");
                        }
                        else if (flagOutputIO)
                        {
                            file = new System.IO.StreamReader("c:\\IEDUP/file/PinOutput.txt");
                        }

                        if (flagInputIO || flagOutputIO)
                        {
                            while ((line = file.ReadLine()) != null)
                            {
                                if (countPin == Nodevice) break;
                                else countPin++;
                            }

                            file.Close();

                            //device = line;
                            carImg.Name = line.Replace(" ", string.Empty);
                        }
                        else carImg.Name = device.Replace(" ", string.Empty);
                    }

                    else if (flagGateAND)
                    {
                        carImg.Name = "GATE";
                        carImg.Tag = "AND";

                        Image myImage3 = new Image();
                        BitmapImage bi3 = new BitmapImage();
                        bi3.BeginInit();
                        bi3.UriSource = new Uri("images/Gates/AND.jpg", UriKind.Relative);
                        bi3.EndInit();
                        myImage3.Stretch = Stretch.Fill;
                        // myImage3.Source = bi3;
                        carImg.Source = bi3;// new BitmapImage(new Uri("images/Gates/PORT.jpg"));
                    }
                    else if (flagGateOR)
                    {
                        carImg.Name = "GATE";
                        carImg.Tag = "OR";

                        Image myImage3 = new Image();
                        BitmapImage bi3 = new BitmapImage();
                        bi3.BeginInit();
                        bi3.UriSource = new Uri("images/Gates/OR.jpg", UriKind.Relative);
                        bi3.EndInit();
                        myImage3.Stretch = Stretch.Fill;
                        // myImage3.Source = bi3;
                        carImg.Source = bi3;// new BitmapImage(new Uri("images/Gates/PORT.jpg"));
                    }
                    else if (flagGateNOT)
                    {
                        carImg.Name = "GATE";
                        carImg.Tag = "NOT";

                        Image myImage3 = new Image();
                        BitmapImage bi3 = new BitmapImage();
                        bi3.BeginInit();
                        bi3.UriSource = new Uri("images/Gates/NOT.jpg", UriKind.Relative);
                        bi3.EndInit();
                        myImage3.Stretch = Stretch.Fill;
                        // myImage3.Source = bi3;
                        carImg.Source = bi3;// new BitmapImage(new Uri("images/Gates/PORT.jpg"));
                    }

                    //.Name = 
                    carImg.Width = 60;
                    carImg.Height = 30;
                    //carImg.Width = (int)Math.Floor(Objimg[selectedIndex].Width);
                    //carImg.Height = (int)Math.Floor(Objimg[selectedIndex].Height);

                    ObjList.Add(carImg);

                    //Canvas.SetLeft(carImg, point.X);
                    //Canvas.SetTop(carImg, point.Y);

                    WorkSpace.Children.Add(ObjList[size]);

                    textBox1.Text = "Image";
                    TypeObj = "Image";
                    img = carImg;
                    Mouse.Capture(img);
                    top = point.Y;
                    left = point.X;

                    //XDevice = top + (ellipse.Height / 2);
                    //YDevice = left + (ellipse.Width / 2);

                    DHDevice = (img.Height / 2);
                    DWDevice = (img.Width / 2);
                    // Find Center Object
                    YDevice = top + DHDevice;
                    XDevice = left + DWDevice;

                    label1.Content = XDevice.ToString();
                    label2.Content = YDevice.ToString();

                    LDevice = XDevice - DWDevice;
                    RDevice = XDevice + DWDevice;

                    top = YDevice - DHDevice;
                    left = XDevice - DWDevice;

                    label3.Content = top.ToString();
                    label4.Content = left.ToString();

                    TDevice = YDevice - DHDevice;
                    BDevice = YDevice + DHDevice;

                    label5.Content = TDevice.ToString();
                    label6.Content = BDevice.ToString();

                    CalculatePosition(carImg);
                }
                catch (Exception ex)
                {
                }
            }
        }

        private void CalculatePosition(Image e)
        {
            var rect = e;
            activated = false;
            pointL = -1;
            pointR = -1;
            pointT = -1;
            pointB = -1;
            flagPointL = false;
            flagPointR = false;
            flagPointT = false;
            flagPointB = false;

            //Find Location And Difference
            for (int i = 0; i < 64; i++)
            {
                if (LDevice >= (startGridX + (i * GridSizeV)) && LDevice < (startGridX + ((i + 1) * GridSizeV)) && !flagPointL)
                {
                    pointL = i;
                    left = startGridX + (i * GridSizeV);
                    //DLDevice = LDevice - (startGridX + (i * GridSizeV));
                    flagPointL = true;
                }
                if (RDevice >= (startGridX + (i * GridSizeV)) && RDevice < (startGridX + ((i + 1) * GridSizeV)) && !flagPointR)
                {
                    pointR = i;
                    //DRDevice = (startGridX + ((i + 1) * GridSizeV)) - RDevice;
                    flagPointR = true;
                }
                if (TDevice >= (startGridY + (i * GridSizeH)) && TDevice < (startGridY + ((i + 1) * GridSizeH)) && !flagPointT)
                {
                    pointT = i;
                    top = startGridY + (i * GridSizeH);
                    //DTDevice = TDevice - (startGridY + (i * GridSizeH));
                    flagPointT = true;
                }
                if (BDevice >= (startGridY + (i * GridSizeH)) && BDevice < (startGridY + ((i + 1) * GridSizeH)) && !flagPointB)
                {
                    pointB = i;
                    //DBDevice = (startGridY + ((i + 1) * GridSizeH)) - BDevice;
                    flagPointB = true;
                }
            }

            if (pointL < 0 || pointR < 0 || pointT < 0 || pointB < 0)
            {
                WorkSpace.Children.Remove(rect);
                for (int i = 0; i < matrix.Count-1; i++)
                {
                    if (matrix[i][0] == rect.Name.ToString())
                    {
                        for (int ii = Convert.ToInt16(matrix[i][1]); ii < Convert.ToInt16(matrix[i][2]); ii++)
                        {
                            for (int jj = Convert.ToInt16(matrix[i][3]); jj < Convert.ToInt16(matrix[i][4]); jj++)
                            {
                                gridArray[ii, jj] = 0;

                            }
                        }
                        matrix.RemoveAt(i);
                    }
                }
            }
            else
            {
                //เป็นการตรวจสอบว่าใน Grid ตำแหน่ที่วางนั้นมี Device อยู่หรือไม่
                for (int i = pointT; i < pointB; i++)
                {
                    for (int j = pointL; j < pointR; j++)
                    {
                        if (gridArray[i, j] == 1)
                        {
                            DeiveOnGrid = true;
                        }
                        if (DeiveOnGrid)
                        {
                            break;
                        }
                    }
                    if (DeiveOnGrid)
                    {
                        break;
                    }
                }

                if (DeiveOnGrid)
                {
                    WorkSpace.Children.Remove(rect);
                }
                else
                {
                    Canvas.SetTop(rect, top);
                    Canvas.SetLeft(rect, left);
                    Mouse.Capture(null);

                    matrix.Add(new List<String>());
                    if (rect.Tag.ToString() == "PORT")
                    {
                        matrix[matrix.Count - 1].Add(rect.Name);
                        matrix[matrix.Count - 1].Add(rect.Tag.ToString());
                        matrix[matrix.Count - 1].Add(pointT.ToString());
                        matrix[matrix.Count - 1].Add(pointB.ToString());
                        matrix[matrix.Count - 1].Add(pointL.ToString());
                        matrix[matrix.Count - 1].Add(pointR.ToString());
                        matrix[matrix.Count - 1].Add("0");//GateOutput
                        matrix[matrix.Count - 1].Add("");//linename
                    }
                    else if (rect.Tag.ToString() == "AND" || rect.Tag.ToString() == "OR" || rect.Tag.ToString() == "NOT")
                    {
                        matrix[matrix.Count - 1].Add(rect.Name);
                        matrix[matrix.Count - 1].Add(rect.Tag.ToString());
                        matrix[matrix.Count - 1].Add(pointT.ToString());
                        matrix[matrix.Count - 1].Add(pointB.ToString());
                        matrix[matrix.Count - 1].Add(pointL.ToString());
                        matrix[matrix.Count - 1].Add(pointR.ToString());
                        matrix[matrix.Count - 1].Add("0");//countnode
                        matrix[matrix.Count - 1].Add("0");//prenode1
                        matrix[matrix.Count - 1].Add("0");//prenode2
                        matrix[matrix.Count - 1].Add("0");//nextnode
                        matrix[matrix.Count - 1].Add("0");//lvGate
                        matrix[matrix.Count - 1].Add("0");//linename
                    }

                    //เป็นการกำหนดตำแหน่ง Device บน Grid
                    for (int i = pointT; i < pointB; i++)
                    {
                        for (int j = pointL; j < pointR; j++)
                        {
                            gridArray[i, j] = 1;

                        }
                    }
                }
            }
            richTextBox1.Document.Blocks.Clear();
            srt = "";
            for (int i = 0; i < 63; i++)
            {
                for (int j = 0; j < 63; j++)
                {
                    srt += gridArray[i, j];
                }
                srt += System.Environment.NewLine;
            }
            richTextBox1.AppendText(srt);

            DeiveOnGrid = false;

        }

        private void Devicelist_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!flagCursor)
            {
                MessageBox.Show("Please Change Cursor Tool");
            }
            else
            {
                DependencyObject dep = (DependencyObject)e.OriginalSource;

                while ((dep != null) && !(dep is ListViewItem))
                {
                    dep = VisualTreeHelper.GetParent(dep);
                }

                if (dep == null)
                    return;

                MyItem item = (MyItem)Devicelist.ItemContainerGenerator.ItemFromContainer(dep);
                //MessageBox.Show(item.DName.ToString());
                //MessageBox.Show(item.index.ToString());

                if (!flagCreateDevice)
                {
                    index++;
                    size = ObjList.Count;
                    point = Mouse.GetPosition(this);

                    Image carImg = new Image();
                    if (item.DName == "PORT")
                    {
                        carImg.Name = device;
                        carImg.Tag = item.DName;
                    }
                    else if (item.DName == "AND")
                    {
                        carImg.Name = "GATE";
                        carImg.Tag = "AND";
                    }
                    else if (item.DName == "OR")
                    {
                        carImg.Name = "GATE";
                        carImg.Tag = "OR";
                    }
                    else if (item.DName == "NOT")
                    {
                        carImg.Name = "GATE";
                        carImg.Tag = "NOT";
                    }


                    carImg.Source = Objimg[item.index]; 
                    carImg.Width = Objimg[item.index].Width;
                    carImg.Height = Objimg[item.index].Height;
                  
                    ObjList.Add(carImg);

                    Canvas.SetLeft(carImg, point.X);
                    Canvas.SetTop(carImg, point.Y);

                    WorkSpace.Children.Add(ObjList[size]);
                }
            }
        }

        private void New_Click(object sender, RoutedEventArgs e)
        {
            New_WorkSpace();
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Open_Template(object sender, RoutedEventArgs e)
        {
            OpenFile();
        }

        private void Open_File(object sender, RoutedEventArgs e)
        {
            OpenFile();
        }

        private void Save_File(object sender, RoutedEventArgs e)
        {
            SaveFile();
        }

        
        private void Cursor_Click(object sender, RoutedEventArgs e)
        {
            ClearFlag();
            flagCursor = true;
        }

        private void Wire_Click(object sender, RoutedEventArgs e)
        {
            ClearFlag();
            flagLine = true;
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            ClearFlag();
            flagDelete = true;
        }

        private void generate_File(object sender, RoutedEventArgs e)
        {
            GenerateAutoFuntion();
        }

        private void InputIO_Click(object sender, RoutedEventArgs e)
        {
            ClearFlag();
            flagInputIO = true;
            SelectDevice(LoadListBoxDataIOInput());
        }

        private void OutputIO_Click(object sender, RoutedEventArgs e)
        {
            ClearFlag();
            flagOutputIO = true;
            SelectDevice(LoadListBoxDataIOOutput());
        }

        private void InputGoose_Click(object sender, RoutedEventArgs e)
        {
            ClearFlag();
            flagInputGoose = true;
            SelectDevice(LoadListBoxDataGooseInput());
        }

        private void OutputGoose_Click(object sender, RoutedEventArgs e)
        {
            ClearFlag();
            flagOutputGoose = true;
            SelectDevice(LoadListBoxDataGooseOutput());
        }

        private void Function_Key_Click(object sender, RoutedEventArgs e)
        {
            ClearFlag();
            flagFK = true;
            SelectDevice(LoadListBoxDataFunctionKey());
        }

        private void LED_Click(object sender, RoutedEventArgs e)
        {
            ClearFlag();
            flagLED = true;
            SelectDevice(LoadListBoxDataLED());
        }

        private void AND_GATE_Click(object sender, RoutedEventArgs e)
        {
            ClearFlag();
            flagGateAND = true;
        }

        private void OR_GATE_Click(object sender, RoutedEventArgs e)
        {
            ClearFlag();
            flagGateOR = true;
        }

        private void NOT_GATE_Click(object sender, RoutedEventArgs e)
        {
            ClearFlag();
            flagGateNOT = true;
        }
    }
}
