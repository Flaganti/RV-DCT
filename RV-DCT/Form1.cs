using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace RV_DCT
{

    public partial class Form1 : Form
    {
        int[,] tabelaStiskanja = new int[8, 8] { { 15, 14, 13, 12, 11, 10, 9, 8 }, { 14, 13, 12, 11, 10, 9, 8, 7 }, { 13, 12, 11, 10, 9, 8, 7, 6 }, { 12, 11, 10, 9, 8, 7, 6, 5 }, { 11, 10, 9, 8, 7, 6, 5, 4 }, { 10, 9, 8, 7, 6, 5, 4, 3 }, { 9, 8, 7, 6, 5, 4, 3, 2 }, { 8, 7, 6, 5, 4, 3, 2, 1 } };
        Bitmap img;
        public Form1()
        {
            InitializeComponent();
        }

        private void LoadBtn_Click(object sender, EventArgs e)
        {

            Bitmap img = null;
            int width=0, height=0;
            OpenFileDialog open = new OpenFileDialog();
            if(open.ShowDialog() == DialogResult.OK)
            {
                
                img = (Bitmap)Bitmap.FromFile(open.FileName);
                width = img.Width;
                height = img.Height;
            }
            if (img.Width % 8 != 0)
            {
                width = img.Width + (8 - (img.Width % 8));
            }
            if (img.Height % 8 != 0)
            {
                height = img.Height + (8 - (img.Height % 8));
            }
            img = new Bitmap(img, new Size(width, height));
            pictureBox1.Image = img;
            this.img = img;
        }

        private void CompressBtn_Click(object sender, EventArgs e)
        {
            List<byte[,]> eightByEightArrayR = new List<byte[,]>();//vsi 8x8 rdeči barvni kanal
            List<byte[,]> eightByEightArrayG = new List<byte[,]>();//vsi 8x8 zeleni barvni kanal
            List<byte[,]> eightByEightArrayB = new List<byte[,]>();//vsi 8x8 modri barvni kanal

            int startI = 0;//pove kje bomo začeli ko se for zanka začne (kje v sliki smo)
            int startJ = 0;//pove kje bomo začeli ko se for zanka začne (kje v sliki smo)
            int tmpI = 0, tmpJ = 0;//s tem si pomagamo postavljanja trenutne lokacije
            while (startJ < img.Height)//premikamo se od leve proti desni ter vedno nižje. kak stopimp prenisko zaključimo
            {
                byte[,] eightByEightR = new byte[8, 8];
                byte[,] eightByEightG = new byte[8, 8];
                byte[,] eightByEightB = new byte[8, 8];
                for (int i = startI; i < startI + 8; i++)
                {
                    tmpJ = startJ;
                    for (int j = startJ; j < startJ + 8; j++)
                    {
                        eightByEightR[i - startI, j - startJ] = (byte)(img.GetPixel(i, j).R-128);
                        eightByEightG[i - startI, j - startJ] = (byte)(img.GetPixel(i, j).G-128);
                        eightByEightB[i - startI, j - startJ] = (byte)(img.GetPixel(i, j).B-128);
                        tmpJ++;
                    }
                    tmpI++;
                }
                startI = tmpI;
                eightByEightArrayR.Add(eightByEightR);
                eightByEightArrayG.Add(eightByEightG);
                eightByEightArrayB.Add(eightByEightB);
                if (!(tmpI < img.Width)) //pomaknemo se en 8x8 kvadrat nižje kak pridemo dokonca širine
                {
                    startJ = tmpJ;
                    startI = 0;
                    tmpI = 0;
                }
            }
            int[] newSmth = cikCak(FDCT(eightByEightArrayR[0]));
            int ss = 9;
        }
        public int[,] FDCT(byte[,] eightByEight)
        {
            //eightByEight = new byte[8, 8] { { 5, 5, 5, 5, 45, 45, 45, 45 }, { 5, 5, 5, 5, 45, 45, 45, 45 }, { 5, 5, 5, 5, 45, 45, 45, 45 }, { 5, 5, 5, 5, 45, 45, 45, 45 }, { 5, 5, 5, 5, 45, 45, 45, 45 }, { 5, 5, 5, 5, 45, 45, 45, 45 }, { 5, 5, 5, 5, 45, 45, 45, 45 }, { 5, 5, 5, 5, 45, 45, 45, 45 } };
            int[,] eightByEight_new = new int[8, 8];
            double c1, c2;
            double vstota = 0;
            for(int u = 0; u < 8; u++)
            {
                for(int v = 0; v < 8; v++)
                {
                    if (u == 0)
                        c1 = 1.0 / (double)(Math.Sqrt(2));
                    else
                        c1 = 1.0;
                    if(v==0)
                        c2 = 1.0 / (double)(Math.Sqrt(2));
                    else
                        c2 = 1.0;
                    vstota = 0.0;
                    for(int x = 0; x < 8; x++)
                    {
                        for(int y = 0; y < 8; y++)
                        {
                            double CosX = Math.Cos(((2.0 * x + 1.0) * u * Math.PI) / 16.0);
                            double CosY = Math.Cos(((2.0 * y + 1.0) * v * Math.PI) / 16.0);
                            vstota += eightByEight[x, y] * CosX * CosY ;
                        }
                    }
                    double one_diVBy_four = 1.0 / 4.0;
                    eightByEight_new[u, v] = (int)(one_diVBy_four * c1 * c2 * vstota);
                }
            }
            return eightByEight_new;
        }
        public int[] cikCak(int[,] array) {
            int[] cikCak = new int[64];
            int N = 7;
            int OVERFLOW = 2033;
            bool konec = false;
            int x = 0;
            int y = 0;
            int indeks = 0;
            do
            {
                cikCak[indeks] = array[y,x];
                array[y,x] = OVERFLOW;
                if ((x > 0) && (y < N) && (array[y + 1,x - 1] < OVERFLOW)) // lahko gre levo dol
                { x--; y++; }
                else
                if ((x < N) && (y > 0) && (array[y - 1,x + 1] < OVERFLOW)) // lahko gre desno gor
                { x++; y--; }
                else if ((x > 0) && (x < N)) // lahko gre desno in ni v 1. stolpcu
                    x++;
                else if ((y > 0) && (y < N)) // lahko gre dol in ni v 1. vrstici
                    y++;
                else if (x < N) // lahko gre desno (in je v 1. stolpcu)
                    x++;
                else konec = true;
                indeks++;
            } while (konec == false);
            return cikCak;
        }

        private void DecompressBtn_Click(object sender, EventArgs e)
        {

        }

    }
}
