using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Popups;
using JukeBoxTester;

using Windows.UI.Notifications;
using Windows.Data.Xml.Dom;
using System.Threading.Tasks;
using System.Threading;


// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace JukeBox
{
    
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        Cloudtools cloudtools = new Cloudtools();

        List<Song> myList = null;
        List<Playlist> playList = null;
        Song selectedSong = null;
        DispatcherTimer dispatcherTimer;

        public MainPage()
        {
            this.InitializeComponent();
            ProcessPlaylist("","");
            dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += ProcessPlaylist;
            dispatcherTimer.Interval = new TimeSpan(0, 0, 5);
            dispatcherTimer.Start();

            Default_Library();

        }

        private async void ProcessPlaylist(Object sender, object e)
        {
            Debug.WriteLine("Timer");
            PlaylistListBox.Items.Clear();
            playList = await cloudtools.GetPlayList();
            List<Playlist> OrderplayList = playList.OrderBy(o => o.State).ToList();
           
            foreach (Playlist pl in OrderplayList)
            {
                String str = pl.SongName + "\n " + pl.ArtistName + "\n " + pl.AlbumName;
                PlaylistListBox.Items.Add(str);
            }
            if(PlaylistListBox.Items.Count > 0)
            {
                PlaylistListBox.SelectedIndex = 3;
            }
            
            
            
            TileTemplateType tileTemplate = TileTemplateType.TileSquare150x150Text03;
            XmlDocument tileXml = TileUpdateManager.GetTemplateContent(tileTemplate);
            XmlNodeList tileTextAttributes = tileXml.GetElementsByTagName("text");
            tileTextAttributes[0].InnerText = "Current Song: \n" + PlaylistListBox.Items[3];

            Int16 dueTimeInSeconds = 10;
            DateTime dueTime = DateTime.Now.AddSeconds(dueTimeInSeconds);
            ScheduledTileNotification scheduledTile = new ScheduledTileNotification(tileXml, dueTime);
            TileUpdateManager.CreateTileUpdaterForApplication().AddToSchedule(scheduledTile);
            TileNotification tileNot = new TileNotification(tileXml);
            //tileNot.ExpirationTime = DateTimeOffset.UtcNow.AddSeconds(5);
            TileUpdateManager.CreateTileUpdaterForApplication().Update(tileNot);
        }


        private async void SelectedSong(object sender, SelectionChangedEventArgs e)
        {
            if (listBox.SelectedItem == null) return;

            
            selectedSong = myList[listBox.SelectedIndex];
            MessageDialog messageDialog = new MessageDialog(selectedSong.ArtistName +
                    "\n" +
                    selectedSong.SongName + "\n" +
                    (selectedSong.AlbumName));
            messageDialog.Commands.Add(new UICommand("Add To Playlist", new UICommandInvokedHandler(AddPlaylist)));
            messageDialog.DefaultCommandIndex = 0;
            messageDialog.CancelCommandIndex = 1;
            await messageDialog.ShowAsync();

        }

        private async void Default_Library()
        {
            myList = await cloudtools.SearchSongText("");
            foreach (Song s in myList)
            {
                String st = s.SongName + "\n " + s.ArtistName + "\n " + s.AlbumName;
                Debug.WriteLine("Add: " + st);
                listBox.Items.Add(st);
            }
        }

        private async void SearchBox_Submitted(SearchBox sender, SearchBoxQuerySubmittedEventArgs args)
        {
            Debug.WriteLine("Search box submitted");
           listBox.Items.Clear();
            
            myList = await cloudtools.SearchSongText(searchBox.QueryText);
            foreach(Song s in myList)
            {
                String st = s.SongName + "\n " + s.ArtistName + "\n " + s.AlbumName;
                Debug.WriteLine("Add: " +st);
                listBox.Items.Add(st);
            }

            pivot.SelectedItem = SongLibrary;
            
        }



        private async void AddPlaylist(IUICommand command)
        { 
            await cloudtools.RequestSong(selectedSong.Id);
        }

    }


    
}
