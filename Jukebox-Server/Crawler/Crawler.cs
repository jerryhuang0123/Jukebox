using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crawler
{
    public struct MusicData
    {
        public String Id;
        public String Path;
        public String Artist;
        public String Album;
        public String SongName;
    }

    class Crawl
    {
        List<MusicData> mList;

        public Crawl()
        {
            
        }

        public List<MusicData> crawlMusicData(String scanFolder)
        {
            mList = new List<MusicData>();
            List<String> foldersToScan = new List<String>();
            foldersToScan.Add(scanFolder);

            do
            {
                foldersToScan.AddRange(Directory.GetDirectories(foldersToScan[0]));

                var files = Directory.GetFiles(foldersToScan[0]);
                foreach (String file in files)
                {
                    if (file.EndsWith(".mp3"))
                    {
                        TagLib.File tags = TagLib.File.Create(file);
                        
                        MusicData mData = new MusicData();
                        mData.Path = file;
                        try
                        {
                            mData.Artist = tags.Tag.Artists[0];
                        }
                        catch {  }
                        mData.Album = tags.Tag.Album;
                        mData.SongName = tags.Tag.Title;

                        ////////
                        //Add ID code here

                        ///////

                        mList.Add(mData);
                    }
                }
                foldersToScan.RemoveAt(0);
            } while (foldersToScan.Count > 0);
            return mList;
        }
    }
}
