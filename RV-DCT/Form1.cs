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
                        eightByEightR[i - startI, j - startJ] = img.GetPixel(i, j).R;
                        eightByEightG[i - startI, j - startJ] = img.GetPixel(i, j).G;
                        eightByEightB[i - startI, j - startJ] = img.GetPixel(i, j).B;
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

        }

        private void DecompressBtn_Click(object sender, EventArgs e)
        {

        }
    }
}
