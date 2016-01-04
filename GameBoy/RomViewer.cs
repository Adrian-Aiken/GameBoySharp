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

namespace GameBoy
{
    public partial class RomViewer : Form
    {
        private DataTable table;

        public RomViewer(string rom)
        {
            InitializeComponent();

            romTextBox.Text = rom;
        }
    }
}
