namespace WinSpaceDiff
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.Resize += new EventHandler(Form1_Resize);
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            richTextBox1.Width = this.Width / 2;
            label2.Left = this.Width / 2;
            textBox1.Width = this.Width / 2 - 55;
            textBox2.Width = this.Width / 2 - 65;
            textBox2.Left = this.Width / 2 + 40;
            button1.Left = this.Width / 2 - 165;
            button2.Left = this.Width - 165;
            button3.Left = this.Width / 2 - (button3.Width / 2);

        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Create an instance of the open file dialog box.
            var openFileDialog1 = new OpenFileDialog();

            // Set filter options and filter index.
            openFileDialog1.Filter = "Text Files (.txt)|*.txt|All Files (*.*)|*.*";
            openFileDialog1.FilterIndex = 1;

            // Set the initial directory to the Documents folder.
            openFileDialog1.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            // Call the ShowDialog method to show the dialog box.
            var userClickedOK = openFileDialog1.ShowDialog();

            // Process input if the user clicked OK.
            if (userClickedOK == DialogResult.OK)
            {
                // Get the file name from the open file dialog box.
                var fileName = openFileDialog1.FileName;

                // Assign the file name to the TextBox control.
                textBox1.Text = fileName;

                // You can also use the OpenFile method to get a Stream object.
                // var fileStream = openFileDialog1.OpenFile();

                // Open the file and fill richTextBox1 with the contents.
                var fileStream = openFileDialog1.OpenFile();
                using (var reader = new StreamReader(fileStream))
                {
                    richTextBox1.Text = reader.ReadToEnd();
                }

                textBox1.Enabled = true;
                if (textBox2.Enabled == true)
                    button3.Enabled = true;
            }


        }

        private void button2_Click(object sender, EventArgs e)
        {
            // Create an instance of the open file dialog box.
            var openFileDialog1 = new OpenFileDialog();

            // Set filter options and filter index.
            openFileDialog1.Filter = "Text Files (.txt)|*.txt|All Files (*.*)|*.*";
            openFileDialog1.FilterIndex = 1;

            // Set the initial directory to the Documents folder.
            openFileDialog1.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            // Call the ShowDialog method to show the dialog box.
            var userClickedOK = openFileDialog1.ShowDialog();

            // Process input if the user clicked OK.
            if (userClickedOK == DialogResult.OK)
            {
                // Get the file name from the open file dialog box.
                var fileName = openFileDialog1.FileName;

                // Assign the file name to the TextBox control.
                textBox2.Text = fileName;

                // You can also use the OpenFile method to get a Stream object.
                // var fileStream = openFileDialog1.OpenFile();

                // Open the file and fill richTextBox2 with the contents.
                var fileStream = openFileDialog1.OpenFile();
                using (var reader = new StreamReader(fileStream))
                {
                    richTextBox2.Text = reader.ReadToEnd();
                }

                textBox2.Enabled = true;
                if (textBox1.Enabled == true)
                    button3.Enabled = true;
            }

        }

        private void button3_Click(object sender, EventArgs e)
        {
            spaceDiff.spaceDiff spaceDiff = new spaceDiff.spaceDiff();

            // Get the contents of the two text boxes.
            string file1 = textBox1.Text;
            string file2 = textBox2.Text;

            spaceDiff.loadFilesforComparsion(file1, file2);
            spaceDiff.beginAnalysis();

            richTextBox1.Clear();
            richTextBox2.Clear();

            bool fileDone = false;
            int filePosition = 0;
            bool isMatch = false;

            while (!fileDone)
            {
                string text = spaceDiff.getNextBlock(ref filePosition, ref isMatch, ref fileDone, true);

                if (isMatch)
                {
                    richTextBox1.SelectionColor = Color.White;
                }
                else
                {
                    richTextBox1.SelectionColor = Color.Red;
                }

                richTextBox1.AppendText(text);
            }

            isMatch = false;
            fileDone = false;
            filePosition = 0;

            while (!fileDone)
            {
                string text = spaceDiff.getNextBlock(ref filePosition, ref isMatch, ref fileDone, false);

                if (isMatch)
                {
                    richTextBox2.SelectionColor = Color.White;
                }
                else
                {
                    richTextBox2.SelectionColor = Color.Green;
                }

                richTextBox2.AppendText(text);
            }

        }
    }
}