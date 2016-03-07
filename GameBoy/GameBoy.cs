using System;
using System.Windows.Forms;

namespace GameBoy
{
    public partial class GameBoy : Form
    {
        private OpenFileDialog romDialog;
        private RomViewer romView;

        private MMU mmu;
        private CPU cpu;
        private string filename;

        public GameBoy()
        {
            InitializeComponent();
        }

        private void Window_Load(object sender, EventArgs e)
        {
            // ROM file opening dialog
            romDialog = new OpenFileDialog()
            {
                Title = "Open ROM file",
                Filter = "Game boy ROM files (*.gb)|*.gb",
                Multiselect = false
            };

            // Menu
            this.Menu = new MainMenu();
            MenuItem item;

            item = new MenuItem("File");
            item.MenuItems.Add(new MenuItem("Open ROM...", new EventHandler(OpenRom)));
            item.MenuItems.Add(new MenuItem("Exit", new EventHandler(CloseGameboy)));
            this.Menu.MenuItems.Add(item);

            item = new MenuItem("Tools");
            item.MenuItems.Add(new MenuItem("ROM viewer", new EventHandler(OpenRomViewer)));
            item.Enabled = false;
            this.Menu.MenuItems.Add(item);

            item = new MenuItem("About");
            this.Menu.MenuItems.Add(item);
        }

        private void OpenRom(object sender, EventArgs e)
        {
            if (romDialog.ShowDialog() == DialogResult.OK)
            {
                filename = romDialog.FileName;
                mmu = new MMU(filename);
                cpu = new CPU(mmu);
                Text = mmu.title;

                // TEMP 'RUNNING' OF CPU
                int instructions_run = 0;
                while (true)
                {
                    cpu.Execute();
                    instructions_run++;
                }
            }
            this.Menu.MenuItems[1].Enabled = true;
        }

        private void OpenRomViewer(object sender, EventArgs e)
        {
            if (romView == null || romView.IsDisposed)
            {
                romView = new RomViewer(mmu.getViewableRom());
            }
            romView.Show();
            romView.BringToFront();
        }

        private void CloseGameboy(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
