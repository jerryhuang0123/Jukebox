using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices;
using JukeBoxTester;
using System.Diagnostics;

namespace JukeBox
{
    class Cloudtools
    {
        private IMobileServiceTable<Song> songTable = App.MobileService.GetTable<Song>();
        private IMobileServiceTable<TodoItem> todoTable = App.MobileService.GetTable<TodoItem>();
        private IMobileServiceTable<Client> ClientTable = App.MobileService.GetTable<Client>();
        private IMobileServiceTable<Playlist> PlayListTable = App.MobileService.GetTable<Playlist>();

        public static String clientGUID = "";
        public async void GenerateClientID(String name)
        {
            if (Windows.Storage.ApplicationData.Current.LocalSettings.Values["JukeBoxID"] == null)
            {
                Guid guid = Guid.NewGuid();

                Windows.Storage.ApplicationData.Current.LocalSettings.Values["JukeBoxID"] = guid;
            }

            clientGUID = Windows.Storage.ApplicationData.Current.LocalSettings.Values["JukeBoxID"].ToString();

            Debug.WriteLine("GUID : " + clientGUID + "  " + name);
            Client clientID = new Client();
            clientID.name = name;
            clientID.state = "Active";
            clientID.guid = clientGUID;

            await ClientTable.InsertAsync(clientID);
        }

        public async Task RequestSong(String songID)
        {
            TodoItem item = new TodoItem();

            item.clientid = clientGUID;
            item.songid = songID;
            Debug.WriteLine("clientGUID: " + clientGUID);
            await todoTable.InsertAsync(item);
        }

        public async Task<List<Song>> SearchSongText(String str)
        {
            var obj = (from a in songTable
                       where a.SongName.Contains(str) || a.ArtistName.Contains(str) || a.AlbumName.Contains(str)
                       select a);

            var list = await obj.ToListAsync();
            return list;
        }

        public async Task<List<Song>> SearchSongID(String str)
        {
            var obj = (from a in songTable where a.Id == str select a);

            var list = await obj.ToListAsync();
            return list;
        }

        public async Task<List<Playlist>> GetPlayList()
        {
            var obj = (from a in PlayListTable select a).ToListAsync();

            var list = await obj;
            return list;
        }

        /*

         private async void button2_Click(object sender, RoutedEventArgs e)
        {
            var obj = await SearchSongText("POW");
            int c = obj.Count;

            Debug.WriteLine("Found : " + c + " records.");
        }

        private async void button3_Click(object sender, RoutedEventArgs e)
        {
            var obj = await SearchSongID("##11CA1070##");
            int c = obj.Count;

            Debug.WriteLine("Found : " + c + " records.");
        }

        private async void button4_Click(object sender, RoutedEventArgs e)
        {
            DateTime d = new DateTime(0);
            List<TodoItem> obj = await Searchtodo(d);
            int c = obj.Count;

            Debug.WriteLine("Found : " + c + " records.");

            DateTime x = obj[3].__createdAt;

            List<TodoItem> obj2 = await Searchtodo(x);
            int c2 = obj2.Count;

            Debug.WriteLine("Found : " + c2 + " records.");
        }

    */



    }
}
