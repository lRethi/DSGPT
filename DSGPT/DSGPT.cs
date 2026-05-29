using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Linq;

namespace DSGPT
{
    public partial class DSGPT : Form
    {
        private KNN knn;
        private FeatureExtractor extractor = new FeatureExtractor();
        private double[] currentFeatures = new double[11];

        public DSGPT()
        {
            InitializeComponent();

            var data = new List<DataPoint>
            {
                // [ comprimento, largura, altura, peso, consumo, conectores,
                // pcie, sata, nvme, dimm, socket ]
                // ================= GPU =================
                new DataPoint(new double[] {300/400.0,120/200.0,50/150.0,1200/2000.0,250/500.0,2/4.0, 1,0,0,0,0}, "GPU"),
                new DataPoint(new double[] {280/400.0,110/200.0,45/150.0,1000/2000.0,220/500.0,2/4.0, 1,0,0,0,0}, "GPU"),
                new DataPoint(new double[] {320/400.0,130/200.0,55/150.0,1400/2000.0,300/500.0,3/4.0, 1,0,0,0,0}, "GPU"),

                // ================= SSD =================
                // NVMe
                new DataPoint(new double[] {100/400.0,70/200.0,7/150.0,50/2000.0,5/500.0,1/4.0, 0,0,1,0,0}, "SSD"),
                new DataPoint(new double[] {80/400.0,22/200.0,3/150.0,20/2000.0,4/500.0,1/4.0, 0,0,1,0,0}, "SSD"),

                // SATA
                new DataPoint(new double[] {100/400.0,70/200.0,7/150.0,60/2000.0,6/500.0,1/4.0, 0,1,0,0,0}, "SSD"),

                // ================= HDD =================
                new DataPoint(new double[] {147/400.0,101/200.0,26/150.0,450/2000.0,10/500.0,2/4.0, 0,1,0,0,0}, "HDD"),
                new DataPoint(new double[] {147/400.0,101/200.0,20/150.0,400/2000.0,9/500.0,2/4.0, 0,1,0,0,0}, "HDD"),
                new DataPoint(new double[] {100/400.0,70/200.0,15/150.0,300/2000.0,8/500.0,2/4.0, 0,1,0,0,0}, "HDD"),

                // ================= RAM =================
                new DataPoint(new double[] {133/400.0,30/200.0,5/150.0,40/2000.0,5/500.0,1/4.0, 0,0,0,1,0}, "RAM"),
                new DataPoint(new double[] {140/400.0,35/200.0,6/150.0,50/2000.0,6/500.0,1/4.0, 0,0,0,1,0}, "RAM"),
                new DataPoint(new double[] {120/400.0,28/200.0,5/150.0,35/2000.0,4/500.0,1/4.0, 0,0,0,1,0}, "RAM"),

                // ================= CPU =================
                new DataPoint(new double[] {45/400.0,45/200.0,5/150.0,70/2000.0,95/500.0,1/4.0, 0,0,0,0,1}, "CPU"),
                new DataPoint(new double[] {40/400.0,40/200.0,5/150.0,60/2000.0,65/500.0,1/4.0, 0,0,0,0,1}, "CPU"),
                new DataPoint(new double[] {50/400.0,50/200.0,6/150.0,80/2000.0,125/500.0,1/4.0, 0,0,0,0,1}, "CPU"),

                // ================= Placa-mãe =================
                new DataPoint(new double[] {305/400.0,244/200.0,30/150.0,800/2000.0,70/500.0,4/4.0, 1,1,1,0,1}, "Placa Mãe"),
                new DataPoint(new double[] {244/400.0,244/200.0,25/150.0,700/2000.0,60/500.0,4/4.0, 1,1,1,0,1}, "Placa Mãe"),
                new DataPoint(new double[] {170/400.0,170/200.0,20/150.0,500/2000.0,50/500.0,3/4.0, 1,1,0,0,1}, "Placa Mãe"),
            };

            knn = new KNN(data, 3);
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void txtInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                ProcessUserInput(txtInput.Text);
                txtInput.Clear();
                e.SuppressKeyPress = true;
            }
        }

        private void ProcessUserInput(string text)
        {
            double[] extracted = extractor.Extract(text);

            // acumula os dados
            for (int i = 0; i < currentFeatures.Length; i++)
            {
                if (extracted[i] != 0)
                    currentFeatures[i] = extracted[i];
                Console.Write(i);
            }

            // verifica se já tem dados suficientes
            if (IsReadyToPredict())
            {
                string result = knn.Predict(currentFeatures);
                lblResultado.Text = "Resultado: " + result;
                AnimateMouth();

                // reseta pra nova conversa
                currentFeatures = new double[11];
            }
            else
            {
                lblResultado.Text = "Acumulando dados! Dê mais informações!";
            }
        }
        private bool IsReadyToPredict()
        {
            int filled = currentFeatures.Count(f => f != 0);

            bool temTipo =
                currentFeatures[6] == 1 || // PCIe
                currentFeatures[7] == 1 || // SATA
                currentFeatures[8] == 1 || // NVMe
                currentFeatures[9] == 1 || // DIMM
                currentFeatures[10] == 1;  // Socket

            return temTipo && filled >= 3;
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private int animFrame = 0;
        private int animCount = 0;

        private void AnimateMouth()
        {
            animFrame = 0;
            animCount = 0;
            timer1.Start();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            string basePath = Application.StartupPath + "C:\\Users\\Alunos\\Pictures\\Saved Pictures";

            if (animFrame % 2 == 0)
                pictureBox1.ImageLocation = basePath + "boca_fechada.png";
            else
                pictureBox1.ImageLocation = basePath + "boca_aberta.png";

            animFrame = 1 - animFrame;
            animCount++;

            if (animCount > 6)
                timer1.Stop();
        }
    }
}