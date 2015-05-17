using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crawler
{

    class Program
    {
        static void Main(string[] args)
        {
            Crawl c = new Crawl();
            List<MusicData> data = c.crawlMusicData(Environment.GetFolderPath(Environment.SpecialFolder.MyMusic));
            //TODO Insert database upload
        }

    }
}
