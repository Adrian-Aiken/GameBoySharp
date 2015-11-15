using System;
using System.Windows.Forms;

namespace GameBoy
{
    public partial class GameBoy : Form
    {
        public GameBoy()
        {
            InitializeComponent();
        }

        private void Window_Load(object sender, EventArgs e)
        {
            this.Menu = new MainMenu();
            MenuItem item;

            item = new MenuItem("File");
            item.MenuItems.Add(new MenuItem("Open ROM...", new EventHandler(OpenRom)));
            item.MenuItems.Add(new MenuItem("Exit", new EventHandler(CloseGameboy)));
            this.Menu.MenuItems.Add(item);

            item = new MenuItem("About");
            this.Menu.MenuItems.Add(item);
        }

        private void OpenRom(object sender, EventArgs e)
        {

        }

        private void CloseGameboy(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
