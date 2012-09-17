using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Net;
using HtmlAgilityPack;
using System.IO;
using System.Text;

namespace IMDB_getter
{

    class Program
    {
        public static ArrayList ids = new ArrayList();
        public static ArrayList movieName = new ArrayList();
        public static ArrayList linksList = new ArrayList();
        public static Dictionary<string, MovieInfo> movies = new Dictionary<string, MovieInfo>();
        //movies.Add("yourKey", new MovieInfo());
        //then to access a movie by key
        //var movieinfo = movies["yourKey"];

        public static ArrayList fileIDS = new ArrayList();

        public static void Main(string[] args)
        {

            readlinks();

            for (int k = 0; k < linksList.Count; k++)
            {
                //movies.Add
                Decimal movieRatingnum = 0;

                var web = new HtmlWeb();
                var doc = web.Load(linksList[k].ToString());

                //Finding IMDB ID
                var nodes = doc.DocumentNode.SelectNodes("//a[@id]");
                //var nodes = doc.DocumentNode.SelectNodes("a.iframe");

                //Finding IMDB Name
                var node2s = doc.DocumentNode.SelectNodes("//a[contains(@href,'/ip.php?v=')]");

                foreach (var node in nodes)
                {
                    ids.Add(string.Format("{0}", node.Attributes["id"].Value));
                }

/*                foreach (var node2 in node2s)
                {
                    movieName.Add(node2.InnerHtml);
                }
                */
                ids.RemoveAt(0);
                ids.RemoveAt(0);

                Console.WriteLine("Got {0} movie IDS on first time read", ids.Count);

                //Removing all the ids I have already checked
                removeIDs();

                for (int i = 0; i < ids.Count; i++)
                {
                    string movieRating = rating(buildIMDBurl(ids[i].ToString()), ids[i].ToString());
                    storeIDs(ids[i].ToString());
                    //why do i get null here??
                    if ((movieRating != null))
                    {
                        try
                        {
                            movieRatingnum = Decimal.Parse(movieRating);
                        }
                        catch
                        {
                            Console.WriteLine("Somehow could not grab the rating");
                        }

                        if (movieRatingnum > 6)
                        {
                            var movieinfo = movies[ids[i].ToString()];
                            Console.Write(movieinfo.Name);
                            Console.Write(" " + movieRatingnum);
                            Console.Write(" " + buildIMDBurl(ids[i].ToString()));
                            Console.WriteLine("");

                        if(movieinfo.Name.Contains("TV Series"))
                        {
                            StreamWriter sw2 = File.AppendText("TVList.txt");
                            Console.WriteLine("Writing in TV List");
                            sw2.WriteLine(movieinfo.Name.ToString() + "|" + movieRatingnum + "|" + buildIMDBurl(ids[i].ToString()) + "|" + ids[i]);

                            sw2.Close();
                        }
                            StreamWriter sw = File.AppendText("MovieList.txt");

                            sw.WriteLine(movieinfo.Name.ToString() + "|" + movieRatingnum + "|" + buildIMDBurl(ids[i].ToString()) + "|" + ids[i]);

                            sw.Close();
                        }

                    }

                }
            }
            Console.WriteLine("Done!");
            Console.ReadKey();
        }

        public static void readlinks()
        {
            var sr1 = new StreamReader("Links.txt");
            while (!sr1.EndOfStream)
            {
                linksList.Add(sr1.ReadLine().ToString());
            }

            sr1.Close();
        }

        static void removeIDs()
        {
            //// Lets remove the ids that are already in the file
            if (File.Exists("StoredIDs.txt"))
            {
                Console.WriteLine("Reading stored file");

                StreamReader sr = new StreamReader("StoredIDs.txt");

                while (!sr.EndOfStream)
                {
                    string line = sr.ReadLine();
                    fileIDS.Add(line);
                }

                sr.Close();

                Console.WriteLine("Found {0} IDs in the file", fileIDS.Count);

                for (int a = 0; a < fileIDS.Count; a++)
                {
                    if (ids.Contains(fileIDS[a]))
                    {
                        ids.Remove(fileIDS[a]);
                    }
                }

                Console.WriteLine("{0} IDs are remaining", ids.Count);
            }
            ////Done removing IDs
        }

        static void storeIDs(string idstore)
        {

                StreamWriter swid = File.AppendText("StoredIDs.txt");

                swid.WriteLine(idstore);

                swid.Close();
            
        }

        static string buildIMDBurl(string imdbID)
        {
            String imdbURL = "http://www.imdb.com/title/tt" + imdbID;
            return imdbURL;
        }


        static string rating(string url, string id)
        {

            var web1 = new HtmlWeb();
            var doc1 = web1.Load(url);

            var nodes1 = doc1.DocumentNode.SelectNodes("//span[@itemprop='ratingValue']");
            var title = doc1.DocumentNode.SelectSingleNode("//title");

            if (nodes1 != null)
            {
                foreach (var node in nodes1)
                {

                    //Console.WriteLine("Here is the title of the IMDB page {0}", title.InnerHtml);
                    if (!movies.ContainsKey(id))
                    {
                        movies.Add(id, new MovieInfo());
                        var movieinfo = movies[id];
                        movieinfo.Name = title.InnerHtml;
                        return node.InnerHtml;
                    }
                }
            }

            return null;
        }

        
    }
}
