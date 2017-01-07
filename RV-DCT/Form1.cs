using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
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
        int width = 0, height = 0;
        int faktorKompresije;
        List<int[,]> eightByEightArrayR = new List<int[,]>();//vsi 8x8 rdeči barvni kanal
        List<int[,]> eightByEightArrayG = new List<int[,]>();//vsi 8x8 zeleni barvni kanal
        List<int[,]> eightByEightArrayB = new List<int[,]>();//vsi 8x8 modri barvni kanal
        double[,] cos = new double[8, 8];
        public Form1()
        {
            InitializeComponent();
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    cos[i, j] = Math.Cos(((2.0 * i + 1.0) * j * Math.PI) / 16.0);
                }
            }
        }

        private void LoadBtn_Click(object sender, EventArgs e)
        {

            Bitmap img = null;

            OpenFileDialog open = new OpenFileDialog();
            if (open.ShowDialog() == DialogResult.OK)
            {
                eightByEightArrayB.Clear();
                eightByEightArrayG.Clear();
                eightByEightArrayR.Clear();

                img = (Bitmap)Bitmap.FromFile(open.FileName);
                width = img.Width;
                height = img.Height;
                if (img.Width % 8 != 0)
                {
                    width = img.Width + (8 - (img.Width % 8));
                }
                if (img.Height % 8 != 0)
                {
                    height = img.Height + (8 - (img.Height % 8));
                }
                img = new Bitmap(img, new Size(width, height));
                this.img = img;

                //To sem premakno sem da prihranim čas pri kompresiji
                int startI = 0;//pove kje bomo začeli ko se for zanka začne (kje v sliki smo)
                int startJ = 0;//pove kje bomo začeli ko se for zanka začne (kje v sliki smo)
                int tmpI = 0, tmpJ = 0;//s tem si pomagamo postavljanja trenutne lokacije
                while (startJ < img.Height)//premikamo se od leve proti desni ter vedno nižje. kak stopimp prenisko zaključimo
                {
                    int[,] eightByEightR = new int[8, 8];
                    int[,] eightByEightB = new int[8, 8];
                    int[,] eightByEightG = new int[8, 8];
                    for (int i = startI; i < startI + 8; i++)
                    {
                        tmpJ = startJ;
                        for (int j = startJ; j < startJ + 8; j++)
                        {
                            eightByEightR[i - startI, j - startJ] = (img.GetPixel(i, j).R - 128);
                            eightByEightG[i - startI, j - startJ] = (img.GetPixel(i, j).G - 128);
                            eightByEightB[i - startI, j - startJ] = (img.GetPixel(i, j).B - 128);
                            tmpJ++;
                        }
                        tmpI++;
                    }
                    startI = tmpI;
                    eightByEightArrayR.Add(FDCT(eightByEightR)); //FDCT se najdalje izvaja zato sem ga premakno sem da se naredi preden se slika naloži :v
                    eightByEightArrayG.Add(FDCT(eightByEightG));
                    eightByEightArrayB.Add(FDCT(eightByEightB));
                    if (!(tmpI < img.Width)) //pomaknemo se en 8x8 kvadrat nižje kak pridemo dokonca širine
                    {
                        startJ = tmpJ;
                        startI = 0;
                        tmpI = 0;
                    }
                }
                pictureBox1.Image = img;
            }





        }

        private void CompressBtn_Click(object sender, EventArgs e)
        {
            StringBuilder builder = new StringBuilder();
            var watch = System.Diagnostics.Stopwatch.StartNew();
            for (int i = 0; i < eightByEightArrayR.Count; i++) {
                builder.Append(Code(cikCak(eightByEightArrayR[i])));
                builder.Append(Code(cikCak(eightByEightArrayG[i])));
                builder.Append(Code(cikCak(eightByEightArrayB[i])));
            }
            watch.Stop();
            MessageBox.Show((watch.ElapsedMilliseconds).ToString());
            zapisVDatoteko(builder);

        }

        private void zapisVDatoteko(StringBuilder builder)
        {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\compressed.DCT";
            //width = 8;height = 8;
            String W = Convert.ToString(width, 2).PadLeft(16, '0');
            String H = Convert.ToString(height, 2).PadLeft(16, '0');
            byte ostanek = Convert.ToByte(8 - (builder.Length % 8));
            String ost = ("").PadLeft(ostanek, '0');
            builder.Append(ost);
            List<byte> zapis = new List<byte>();
            zapis.AddRange(GetBytes(W).ToList());
            zapis.AddRange(GetBytes(H).ToList());
            zapis.Add(ostanek);
            zapis.AddRange(GetBytes(builder.ToString()).ToList());
            using (BinaryWriter writer = new BinaryWriter(File.Open(path, FileMode.Create)))
            {
                foreach (byte baj in zapis)
                {
                    writer.Write(baj);
                }
            }

        }
        public static byte[] GetBytes(string bitString)
        {
            return Enumerable.Range(0, bitString.Length / 8).
                Select(pos => Convert.ToByte(
                    bitString.Substring(pos * 8, 8),
                    2)
                ).ToArray();
        }


        public int[,] FDCT(int[,] eightByEight)
        {
            //eightByEight = new int[8, 8] { { 5, 5, 5, 5, 45, 45, 45, 45 }, { 5, 5, 5, 5, 45, 45, 45, 45 }, { 5, 5, 5, 5, 45, 45, 45, 45 }, { 5, 5, 5, 5, 45, 45, 45, 45 }, { 5, 5, 5, 5, 45, 45, 45, 45 }, { 5, 5, 5, 5, 45, 45, 45, 45 }, { 5, 5, 5, 5, 45, 45, 45, 45 }, { 5, 5, 5, 5, 45, 45, 45, 45 } };
            int[,] eightByEight_new = new int[8, 8];
            double c1, c2;
            double vstota = 0;
            for (int u = 0; u < 8; u++)
            {
                for (int v = 0; v < 8; v++)
                {

                    if (u == 0)
                        c1 = 1.0 / (double)(Math.Sqrt(2));
                    else
                        c1 = 1.0;
                    if (v == 0)
                        c2 = 1.0 / (double)(Math.Sqrt(2));
                    else
                        c2 = 1.0;
                    vstota = 0.0;
                    if (!(tabelaStiskanja[u, v] <= faktorKompresije))
                    {
                        for (int x = 0; x < 8; x++)
                        {
                            for (int y = 0; y < 8; y++)
                            {
                                vstota += eightByEight[x, y] * cos[x, u] * cos[y, v];
                            }
                        }
                    }
                    double one_diVBy_four = 1.0 / 4.0;
                    eightByEight_new[u, v] = (int)(one_diVBy_four * c1 * c2 * vstota);
                }
            }
            return eightByEight_new;
        }
        public int[] cikCak(int[,] array_) {
            int[,] array = new int[8, 8];
            int[] cikCak = new int[64];
            int N = 7;
            int OVERFLOW = 2033;
            bool konec = false;
            int x = 0;
            int y = 0;
            int indeks = 0;
            do
            {
                cikCak[indeks] = array_[y, x];
                array[y, x] = OVERFLOW;
                if ((x > 0) && (y < N) && (array[y + 1, x - 1] < OVERFLOW)) // lahko gre levo dol
                { x--; y++; }
                else
                if ((x < N) && (y > 0) && (array[y - 1, x + 1] < OVERFLOW)) // lahko gre desno gor
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
        private String Code(int[] codingArray)
        {
            StringBuilder builder = new StringBuilder();
            if (codingArray[0] < 0) //Prvi se vedno kodira tak! predstavljen je z 11 biti, prvi bit pove ali je + ali - ostalih 10 pa je binarno predstavljeno število
            {
                String str = Convert.ToString((codingArray[0] * (-1)), 2);
                builder.Append(str.PadLeft(10, '0').PadLeft(11, '1'));
            }
            else
            {
                String str = Convert.ToString((codingArray[0]), 2);
                builder.Append(str.PadLeft(11, '0'));
            }
            int nicle = 0;
            for (int i = 1; i < 64; i++)
            {

                if (codingArray[i] == 0) //zaporedje 0 se začne
                {
                    nicle++;//stejemo koliko je nicel
                }
                else if (codingArray[i] != 0 && codingArray[i - 1] == 0)  // a način
                {
                    //zaporedje 0 se konca
                    int stevilo = codingArray[i];
                    builder.Append('0');
                    builder.Append(Convert.ToString(nicle, 2).PadLeft(6, '0'));//v dolžini 6 zapišemo koliko je blo ničel
                    if (stevilo > 0)
                    {
                        String str = Convert.ToString(stevilo, 2);
                        int dolzina = str.Length + 1; // za predznak
                        builder.Append(Convert.ToString(dolzina, 2).PadLeft(4, '0'));
                        builder.Append(str.PadLeft(dolzina, '0')); //dodamo še predznak
                    }
                    else
                    {
                        String str = Convert.ToString(stevilo * (-1), 2);
                        int dolzina = str.Length + 1; // za predznak
                        builder.Append(Convert.ToString(dolzina, 2).PadLeft(4, '0')); //zapišemo v kakšno dolžini je zapisano število
                        builder.Append(str.PadLeft(dolzina, '1')); //zapišemo število in dodamo še predznak
                    }
                    nicle = 0;
                }
                else if (codingArray[i] != 0 && codingArray[i - 1] != 0)//način c
                {
                    builder.Append('1');//nam pove da gre za c
                    if (codingArray[i] > 0)
                    {
                        String str = Convert.ToString(codingArray[i], 2);
                        int dolzina = str.Length + 1;
                        builder.Append(Convert.ToString(dolzina, 2).PadLeft(4, '0'));
                        builder.Append(str.PadLeft(dolzina, '0'));
                    }
                    else
                    {
                        String str = Convert.ToString(codingArray[i] * (-1), 2);
                        int dolzina = str.Length + 1;
                        builder.Append(Convert.ToString(dolzina, 2).PadLeft(4, '0'));
                        builder.Append(str.PadLeft(dolzina, '1'));
                    }
                }
            }
            if (nicle > 0)// izvedemo b -> do konca array so se ble same ničle!
            {
                builder.Append(0);
                builder.Append(Convert.ToString(nicle, 2).PadLeft(6, '0'));
            }

            return builder.ToString();
        }

        private void DecompressBtn_Click(object sender, EventArgs e)
        {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\compressed.DCT";
            byte[] bajt;
            bajt = File.ReadAllBytes(path);
            byte[] sirina = bajt.Take(2).ToArray();
            StringBuilder sirina_str = new StringBuilder();
            sirina_str.Append(Convert.ToString(sirina[0], 2).PadLeft(8, '0'));
            sirina_str.Append(Convert.ToString(sirina[1], 2).PadLeft(8, '0'));
            int sirina_int = Convert.ToInt32(sirina_str.ToString(), 2);

            byte[] visina = bajt.Skip(2).Take(2).ToArray();
            StringBuilder visina_str = new StringBuilder();
            visina_str.Append(Convert.ToString(visina[0], 2).PadLeft(8, '0'));
            visina_str.Append(Convert.ToString(visina[1], 2).PadLeft(8, '0'));
            int visina_int = Convert.ToInt32(visina_str.ToString(), 2);
            byte[] ostanek = bajt.Skip(4).Take(1).ToArray();
            eightByEightArrayB.Clear();
            eightByEightArrayG.Clear();
            eightByEightArrayR.Clear();
            StringBuilder builder = new StringBuilder();
            foreach (byte b in bajt.Skip(5))
            {
                builder.Append(Convert.ToString(b, 2).PadLeft(8, '0'));
            }
            decompress(builder.ToString().Substring(0, builder.Length - (int)ostanek[0]));
            Bitmap image = makeImg(visina_int,sirina_int);
            pictureBox1.Image = image;
        }
        private Bitmap makeImg(int visina,int sirina)
        {
            Bitmap bitmap = new Bitmap(sirina, visina);
            int startI = 0;//pove kje bomo začeli ko se for zanka začne (kje v sliki smo)
            int startJ = 0;//pove kje bomo začeli ko se for zanka začne (kje v sliki smo)
            int tmpI = 0, tmpJ = 0;//s tem si pomagamo postavljanja trenutne lokacije
            //while (startJ < img.Height)//premikamo se od leve proti desni ter vedno nižje. kak stopimp prenisko zaključimo
            for(int a =0;a<eightByEightArrayR.Count;a++){

                int[,] eightByEightR = eightByEightArrayR[a];
                int[,] eightByEightG = eightByEightArrayG[a];
                int[,] eightByEightB = eightByEightArrayB[a];
                for (int i = startI; i < startI + 8; i++)
                {
                    tmpJ = startJ;
                    for (int j = startJ; j < startJ + 8; j++)
                    {
                        
                        int red = eightByEightR[i - startI, j - startJ]+128;
                        int green = eightByEightG[i - startI, j - startJ]+128;
                        int blue = eightByEightB[i - startI, j - startJ]+128;
                        if (red < 0)red = 0;
                        if (red > 255) red = 255;
                        if (green < 0) green = 0;
                        if (green > 255) green = 255;
                        if (blue < 0) blue = 0;
                        if (blue > 255) blue = 255;
                        Color color = Color.FromArgb(red, green, blue);
                        bitmap.SetPixel(i, j, color);
                        tmpJ++;
                    }
                    tmpI++;
                }
                startI = tmpI;
                //eightByEightArrayR.Add(FDCT(eightByEightR));
                //eightByEightArrayG.Add(FDCT(eightByEightG));
                //eightByEightArrayB.Add(FDCT(eightByEightB));
                if (!(tmpI < bitmap.Width)) //pomaknemo se en 8x8 kvadrat nižje kak pridemo dokonca širine
                {
                    startJ = tmpJ;
                    startI = 0;
                    tmpI = 0;
                }
            }
            return bitmap;

        }
        private void decompress(string kodirano)
        {
            int stevec = 0;//stevec za elemente
            int rgb = 0; // rgb % 3 = 0(red) 1(green) 2(blue)
            int[] matrika = new int[64];
            for (int i = 0; i < kodirano.Length;)
            {
                if (stevec == 0)
                {
                    string DCT = kodirano.Substring(i, 11);
                    if (DCT[0] == '0')
                    {
                        matrika[stevec] = Convert.ToInt32(DCT.Substring(1, DCT.Length - 1), 2);
                    }
                    else
                    {
                        matrika[stevec] = (Convert.ToInt32(DCT.Substring(1, DCT.Length - 1), 2)) * (-1);
                    }
                    i += 11;
                    stevec++;
                }
                if (stevec != 0)
                {
                    if (kodirano[i] == '0')
                    {
                        i++;
                        int tek_dol = Convert.ToInt32(kodirano.Substring(i, 6), 2);
                        for (int j = 0; j < tek_dol; j++) {
                            matrika[stevec + j] = 0;
                        }
                        i += 6;
                        stevec += tek_dol;
                        if (stevec < 64)
                        {
                            int dolzina = Convert.ToInt32(kodirano.Substring(i, 4), 2);
                            i += 4;
                            string AC = kodirano.Substring(i, dolzina);
                            if (AC[0] == '0')
                            {
                                matrika[stevec] = Convert.ToInt32(AC.Substring(1, AC.Length - 1), 2);
                            }
                            else
                            {
                                matrika[stevec] = (Convert.ToInt32(AC.Substring(1, AC.Length - 1), 2)) * (-1);
                            }
                            stevec++;
                            i += dolzina;
                        }
                    }
                    else if (kodirano[i] == '1')
                    {
                        i++;
                        int dolzina = Convert.ToInt32(kodirano.Substring(i, 4), 2);
                        i += 4;
                        string AC = kodirano.Substring(i, dolzina);
                        if (AC[0] == '0')
                        {
                            matrika[stevec] = Convert.ToInt32(AC.Substring(1, AC.Length - 1), 2);
                        }
                        else
                        {
                            matrika[stevec] = (Convert.ToInt32(AC.Substring(1, AC.Length - 1), 2)) * (-1);
                        }
                        stevec++;
                        i += dolzina;
                    }
                }
                if (stevec == 64)
                {
                    if (rgb % 3 == 0) {
                        eightByEightArrayR.Add(IDCT(deCikCak(matrika)));
                    }
                    else if (rgb % 3 == 1) {
                        eightByEightArrayG.Add(IDCT(deCikCak(matrika)));
                    }
                    else if (rgb % 3 == 2) {
                        eightByEightArrayB.Add(IDCT(deCikCak(matrika)));
                    }
                    rgb++;
                    stevec = 0;
                }
            }
        }
        private int[,] IDCT(int[,] matrika)
        {
            int[,] IDCT = new int[8, 8];
            for(int x = 0; x < 8; x++)
            {
                for(int y = 0; y < 8; y++)
                {                    
                    double vstota = 0.0;
                    for (int u = 0; u < 8; u++)
                    {
                        for (int v = 0; v < 8; v++)
                        {
                            double c1, c2;
                            if (u == 0)
                                c1 = 1.0 / (double)(Math.Sqrt(2));
                            else
                                c1 = 1.0;
                            if (v == 0)
                                c2 = 1.0 / (double)(Math.Sqrt(2));
                            else
                                c2 = 1.0;
                            vstota +=cos[x, u] * cos[y, v] *c1*c2* matrika[u, v];
                        }
                    }
                double one_diVBy_four = 1.0 / 4.0;
                IDCT[x, y] = (int)(one_diVBy_four * vstota);
                }
            }
            return IDCT;
        }
        private void trackBar1_ValueChanged(object sender, EventArgs e)
        {
            label1.Text = trackBar1.Value.ToString();
            faktorKompresije = trackBar1.Value;
        }
        private int[,] deCikCak(int[] matrika)
        {
            int[,] array = new int[8, 8];
            int[,] deCikCak = new int[8,8];
            int N = 7;
            int OVERFLOW = 2033;
            bool konec = false;
            int x = 0;
            int y = 0;
            int indeks = 0;
            do
            {
                deCikCak[y, x] = matrika[indeks];
                array[y, x] = OVERFLOW;
                if ((x > 0) && (y < N) && (array[y + 1, x - 1] < OVERFLOW)) // lahko gre levo dol
                { x--; y++; }
                else
                if ((x < N) && (y > 0) && (array[y - 1, x + 1] < OVERFLOW)) // lahko gre desno gor
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
            return deCikCak;
        }
    }
}
