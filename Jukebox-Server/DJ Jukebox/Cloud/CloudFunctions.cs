using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JukeBoxTester;
using Microsoft.WindowsAzure.MobileServices;

namespace DJ_Jukebox.Cloud
{
    class CloudFunctions
    {
        private IMobileServiceTable<Song> songTable = App.MobileService.GetTable<Song>();
        private IMobileServiceTable<TodoItem> todoTable = App.MobileService.GetTable<TodoItem>();
        private IMobileServiceTable<Clientid> ClientTable = App.MobileService.GetTable<Clientid>();
        private IMobileServiceTable<Playlist> PlayListTable = App.MobileService.GetTable<Playlist>();

        private static DateTime voteTime = new DateTime(0);

        /// <summary>
        /// Get the latest votes from DateTime
        /// </summary>
        /// <param name="d">time to get votes from to present</param>
        /// <returns></returns>
        public async Task<List<TodoItem>> DownloadVoteQueue()
        {
            var obj = (from a in todoTable
                       where a.__createdAt > voteTime
                       select a);

            var list = await obj.ToListAsync();
            if(list.Count > 0)
            {
                voteTime = list[list.Count - 1].__createdAt;
            }
            return list;
        }

        /// <summary>
        /// Gets the whole playlist, used for clearing the playlist
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private async Task<List<Playlist>> GetPlayList(String str)
        {
            var obj = (from a in PlayListTable select a).ToListAsync();

            var list = await obj;
            return list;
        }

        /// <summary>
        /// Used for pushing the playlist to the cloud, for viewing on the client
        /// </summary>
        /// <param name="newlist">Playlist to upload</param>
        public async void PostPlayList(List<Playlist> newlist)
        {
            List<Playlist> playlistT = await GetPlayList("");

            foreach (Playlist item in playlistT)
            {
                await PlayListTable.DeleteAsync(item);
            }

            foreach (Playlist item in newlist)
            {
                await PlayListTable.InsertAsync(item);
            }
        }

        /// <summary>
        /// Gets song data by ID from database
        /// </summary>
        /// <param name="str">ID to use</param>
        /// <returns></returns>
        public async Task<Song> SearchSongID(String str)
        {
            var obj = (from a in songTable where a.Id == str select a);

            var list = await obj.ToListAsync();
            if(list.Count > 0)
            {
                return list[0];
            } else
            {
                return null;
            }
        }
        /*

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
