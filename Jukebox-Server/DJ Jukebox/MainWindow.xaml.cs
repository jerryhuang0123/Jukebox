using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using NAudio;
using NAudio.Wave;
using System.Windows.Threading;
using JukeBoxTester;
using System.Collections.ObjectModel;

namespace DJ_Jukebox
{
    //public struct MusicData
    //{
    //    public String Path;
    //    public String Artist;
    //    public String Album;
    //    public String SongName;
    //}

    public class QueriedSong
    {
        public int Score { get; set; }
        public String Path { get; set; }
        public String Artist { get; set; }
        public String Album { get; set; }
        public String SongName { get; set; }
        public String SongId { get; set; }
        public DateTime dt { get; set; }
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static Cloud.CloudFunctions cFunctions;
        DispatcherTimer dTime;
        DispatcherTimer upTime;
        private static ObservableCollection<QueriedSong> upcoming = new ObservableCollection<QueriedSong>();
        private static ObservableCollection<QueriedSong> recent = new ObservableCollection<QueriedSong>();
        private static QueriedSong currentSong;

        static IWavePlayer waveOutDevice;
        static AudioFileReader audioFileReader;

        private bool isDirty = false;

        const int PlaylistUploadMax = 3;
        const int PlaylistUploadMin = -3;

        private static Boolean Termination;
        public MainWindow()
        {
            InitializeComponent();
            Termination = false;
            waveOutDevice = new WaveOut();
            waveOutDevice.PlaybackStopped += WaveOutDevice_PlaybackStopped;

            Play_List.ItemsSource = upcoming;
            cFunctions = new Cloud.CloudFunctions();
        }

        private static bool Compare(QueriedSong apple, QueriedSong orange)
        {
            if (apple.Score > orange.Score)
                return false;
            if (apple.Score < orange.Score)
                return true;

            return (apple.dt > orange.dt);
        }

        static ObservableCollection<QueriedSong> sortUpcoming(ObservableCollection<QueriedSong> oldList)
        {
            ObservableCollection<QueriedSong> newList = new ObservableCollection<QueriedSong>();
            foreach (QueriedSong song in oldList)
            {
                Boolean b = false;
                foreach (QueriedSong qs in newList)
                {
                    if (Compare(qs, song))
                    {
                        newList.Insert(newList.IndexOf(qs), song);
                        b = true;
                        break;
                    }
                    if (b) { break; }
                }
                if (!b)
                {
                    newList.Add(song);
                }
            }
            return newList;
        }

        private QueriedSong setData(QueriedSong qs, Song md, DateTime dt)
        {
            qs.Album = md.AlbumName;
            qs.Artist = md.ArtistName;
            qs.Path = md.PathString;
            qs.SongName = md.SongName;
            qs.SongId = md.Id;
            qs.dt = dt;

            return qs;
        }

        private async void readVotes()
        {
            List<TodoItem> votes = await cFunctions.DownloadVoteQueue();
            foreach (TodoItem vote in votes)
            {
                //Get Vote info
                Song song = await cFunctions.SearchSongID(vote.songid);
                if (song != null)
                {
                    int contains = -1;
                    for (int i = 0; i < upcoming.Count; i++)
                    {
                        if (upcoming[i].Path == song.PathString)
                        {
                            contains = i;
                        }
                    }
                    if (contains == -1)
                    {
                        QueriedSong qs = new QueriedSong();
                        setData(qs, song, vote.__createdAt);
                        qs.Score = 1;
                        upcoming.Add(qs);
                    }
                    else
                    {
                        QueriedSong qs = upcoming[contains];
                        qs.Score++;
                        upcoming[contains] = qs;
                    }
                }
                else
                {
                    MessageBox.Show(Properties.Resources.notFound);
                }
            }
            upcoming = sortUpcoming(upcoming);
            Play_List.ItemsSource = upcoming;
            if (waveOutDevice.PlaybackState == PlaybackState.Stopped)
            {
                playSongs();
            }
        }

        private static void StopMusic()
        {
            if (waveOutDevice != null)
            {
                waveOutDevice.Stop();
            }
        }

        private static void CloseWaveOut()
        {
            if (waveOutDevice != null)
            {
                waveOutDevice.Dispose();
                waveOutDevice = null;
            }
        }

        private Playlist setPlaylistData(QueriedSong song, int state)
        {
            Playlist p = new Playlist();
            p.AlbumName = song.Album;
            p.ArtistName = song.Artist;
            p.songid = song.SongId;
            p.SongName = song.SongName;
            p.State = state;
            return p;
        }

        private ObservableCollection<Playlist> getfromQueryList(ObservableCollection<QueriedSong> queriedSongList, ref int state, int maxcount)
        {
            ObservableCollection<Playlist> playlist = new ObservableCollection<Playlist>();
            int count = (queriedSongList.Count < maxcount) ? queriedSongList.Count : maxcount;
            for (int i = 0; i < count; i++)
            {
                playlist.Add(setPlaylistData(queriedSongList[i], state++));

            }
            return playlist;
        }

        private List<Playlist> GenPlaylist()
        {
            List<Playlist> playlist = new List<Playlist>();
            int st = -PlaylistUploadMax;

            for(int i = 0; i < PlaylistUploadMax - recent.Count; i++)
            {
                Playlist song = new Playlist();
                song.AlbumName = "";
                song.ArtistName = "";
                song.songid = "";
                song.SongName = "";
                song.State = st++;
                playlist.Add(song);
            }
            if (recent.Count > 0)
            {
                playlist.AddRange(getfromQueryList(recent, ref st, PlaylistUploadMax));
            }
            playlist.Add(setPlaylistData(currentSong, st++));
            if (upcoming.Count > 0)
            {
                playlist.AddRange(getfromQueryList(upcoming, ref st, PlaylistUploadMax));
            }

            return playlist;
        }

        private void startPlay()
        {
            if (upcoming.Count > 0)
            {
                isDirty = true;
                audioFileReader = new AudioFileReader(upcoming[0].Path);
                waveOutDevice.Init(audioFileReader);
                waveOutDevice.Play();
                currentSong = upcoming[0];
                Song_Status.Content = Properties.Resources.nowPlaying + currentSong.SongName + Properties.Resources.nowPlaying_transition + currentSong.Artist;
                if (upcoming.Count > 0)
                {
                    upcoming.RemoveAt(0);
                }
                Play_List.ItemsSource = upcoming;

                isDirty = true;
            }
        }

        private void playSongs()
        {
            startPlay();
        }

        private void WaveOutDevice_PlaybackStopped(object sender, StoppedEventArgs e)
        {
            recent.Add(currentSong);
            if(recent.Count > 3)
            {
                recent.RemoveAt(0);
            }

            Recent_List.ItemsSource = recent;

            if(upcoming.Count == 0)
            {
                Song_Status.Content = (Properties.Resources.noSongs);
            }

            if (Termination == false)
            {
                startPlay();
            }

            Play_List.ItemsSource = upcoming;
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            dTime = new DispatcherTimer();
            dTime.Tick +=new EventHandler(voteTime_Tick);
            dTime.Interval = new TimeSpan(0, 0, 10);
            dTime.Start();

            upTime = new DispatcherTimer();
            upTime.Tick += new EventHandler(upTime_Tick);
            upTime.Interval = new TimeSpan(0, 0, 1);
            upTime.Start();

            readVotes();
            
        }

        private void upTime_Tick(object sender, EventArgs e)
        {
            if (isDirty)
            {
                cFunctions.PostPlayList(GenPlaylist());
                isDirty = false;
            }
        }

        private void voteTime_Tick(object sender, EventArgs e)
        {
            readVotes();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Termination = true;
            StopMusic();
            CloseWaveOut();
        }

        private void Chg_Mode_Button_Click(object sender, RoutedEventArgs e)
        {
            if((String)Chg_Mode_Button.Content == "■")
            {
                Termination = true;
                Chg_Mode_Button.Content = "►";
                StopMusic();
                Song_Status.Content = Properties.Resources.djMode;
            }
            else
            {
                Chg_Mode_Button.Content = "■";
                if (currentSong != null)
                {
                    Song_Status.Content = Properties.Resources.nowPlaying + currentSong.SongName + Properties.Resources.nowPlaying_transition + currentSong.Artist;
                }
                else
                {
                    Song_Status.Content = Properties.Resources.noSongs;
                }
                Termination = false;
            }
        }

        //List-Item specific commands
        private void Remove_Suggestion_Button_Click(object sender, RoutedEventArgs e)
        {
            isDirty = true;
            upcoming.RemoveAt(Play_List.SelectedIndex);
            Play_List.ItemsSource = upcoming;
        }

        private void Play_Next_Button_Click(object sender, RoutedEventArgs e)
        {
            isDirty = true;
            upcoming[Play_List.SelectedIndex].Score = upcoming[0].Score + 1;
            upcoming = sortUpcoming(upcoming);
            Play_List.ItemsSource = upcoming;
            isDirty = true;

        }

        private void Send_Up_Button_Click(object sender, RoutedEventArgs e)
        {
            isDirty = true;
            upcoming[Play_List.SelectedIndex].Score++;
            upcoming = sortUpcoming(upcoming);
            Play_List.ItemsSource = upcoming;

            Send_Down_Button.IsEnabled = true;
        }

        private void Send_Down_Button_Click(object sender, RoutedEventArgs e)
        {
            isDirty = true;
            upcoming[Play_List.SelectedIndex].Score--;
            upcoming = sortUpcoming(upcoming);
            Play_List.ItemsSource = upcoming;
            if (upcoming[Play_List.SelectedIndex].Score <= 1)
            {
                Send_Down_Button.IsEnabled = false;
            }
        }

        //other commands
        private void Play_List_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Remove_Suggestion_Button.IsEnabled = false;
            Play_Next_Button.IsEnabled = false;
            Send_Down_Button.IsEnabled = false;
            Send_Up_Button.IsEnabled = false;

            if (Play_List.SelectedIndex > -1)
            {
                Remove_Suggestion_Button.IsEnabled = true;
                Play_Next_Button.IsEnabled = true;
                if (upcoming[Play_List.SelectedIndex].Score > 1)
                {
                    Send_Down_Button.IsEnabled = true;
                }
                Send_Up_Button.IsEnabled = true;
            }
        }

        private void Sux_Button_Click(object sender, RoutedEventArgs e)
        {
            isDirty = true;
            recent.Add(currentSong);
            if(recent.Count > 3)
            {
                recent.RemoveAt(0);
            }

            Recent_List.ItemsSource = recent;

            if(upcoming.Count == 0)
            {
                Song_Status.Content = (Properties.Resources.noSongs);
            }

            Play_List.ItemsSource = upcoming;

            if (Termination == false)
            {
                startPlay();
            }
        }
    }
}
