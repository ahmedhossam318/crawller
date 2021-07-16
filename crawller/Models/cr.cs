
using NTextCat;
using mshtml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;

namespace crawller.Models
{
    public class cr
    {
        Dictionary<string, string> langurl = new Dictionary<string, string>();
        public Queue<string> non_visited_links = new Queue<string>();
        public Queue<string> visited_links = new Queue<string>();
        public Queue<string> not_working_links = new Queue<string>();
        Entities db = new Entities();
        public cr(string seeds)
        {
            //ahmed hakam

            //non_visited_links.Enqueue(seeds);
            page entity = db.pages.Where(x => x.url.Equals(seeds)).FirstOrDefault();
            string Seedcontent="";
            if (entity != null)
            {
                Seedcontent = entity.content;
            }
            else
            {
                Seedcontent = Fetch_Page(seeds);
                page entity1 = db.pages.Where(x => x.url.Equals(seeds)).FirstOrDefault(); 
                if(entity1==null)
                {
                    page p = new page();
                    p.url = seeds;
                    p.content = Seedcontent;
                    db.pages.Add(p);
                    db.SaveChanges();
                }
              
              

            }
            string SeedRobotscontent = Fetch_robots(seeds);
            List<string> All_Links = Parse_Page_(Seedcontent, seeds);
            List<string> invalid_Links = Pars_Robots(SeedRobotscontent);
            foreach (var item in invalid_Links)
            {
                if (All_Links.Contains(item))
                    All_Links.Remove(item);
            }
            foreach (var item in All_Links)
            {
                non_visited_links.Enqueue(item);
            }
            BFS();

        }

        public void BFS()
        {
            while (non_visited_links.Count() > 0 || visited_links.Count == 3000)
            {
                string URL = non_visited_links.Dequeue();
                page p1 = db.pages.Where(x => x.url.Equals(URL)).FirstOrDefault();
                if (p1 != null)
                {
                    Parse_Page(p1.content, URL);
                    continue;
                }
                string pagecontent = Fetch_Page(URL);
                if (pagecontent == "")
                {
                    string anothertry = Fetch_Page(URL);
                    if (anothertry == "")
                        continue;
                    else
                    {

                        page entity1 = db.pages.Where(x => x.url.Equals(URL)).FirstOrDefault();
                        if (entity1 == null)
                        {
                            page p = new page();
                            p.url = URL;
                            p.content = anothertry;
                            db.pages.Add(p);
                            db.SaveChanges();
                        }
                        Parse_Page(anothertry, URL);
                      

                    }
                }
                else
                {

                   
                    page entity1 = db.pages.Where(x => x.url.Equals(URL)).FirstOrDefault();
                    if (entity1 == null)
                    {
                        page p = new page();
                        p.url = URL;
                        p.content = pagecontent;
                        db.pages.Add(p);
                        db.SaveChanges();
                    }
                    Parse_Page(pagecontent, URL);


                }
            }
        }

        public string Fetch_robots(string URL)
        {
            WebRequest myWebRequest = WebRequest.Create(URL + "/robots.txt");
            WebResponse myWebResponse = myWebRequest.GetResponse();
            Stream streamResponse = myWebResponse.GetResponseStream();
            StreamReader sReader = new StreamReader(streamResponse);
            string rString = sReader.ReadToEnd();
            streamResponse.Close();
            sReader.Close();
            myWebResponse.Close();

            return rString;
        }

        public string Fetch_Page(string URL)
        {
            //Omar hassan

            if (validate_URL(URL) == 1)
            {

                try
                {
                    Uri myUri = new Uri(URL, UriKind.RelativeOrAbsolute);
                    WebRequest myWebRequest = WebRequest.Create(URL);

                    WebResponse myWebResponse = myWebRequest.GetResponse();
                    Dictionary<string, string> dic = new Dictionary<string, string>();

                    Stream streamResponse = myWebResponse.GetResponseStream();
                    StreamReader sReader = new StreamReader(streamResponse);
                    string rString = sReader.ReadToEnd();
                    streamResponse.Close();
                    sReader.Close();
                    myWebResponse.Close();
                    visited_links.Enqueue(URL);
                   string lang= language_detection(rString);
                    if(lang!= "eng")
                    {
                        not_working_links.Enqueue(URL);
                        return "";
                    }


                    return rString;
                }
                catch (Exception ex)
                {
                    not_working_links.Enqueue(URL);
                    return "";
                }


            }
            not_working_links.Enqueue(URL);
            return "";
        }

        public int validate_URL(string URL)
        {
            //ahmed hakam
            if (URL == null)
                return 0;
            Uri uriResult;
            bool result = Uri.TryCreate(URL, UriKind.RelativeOrAbsolute, out uriResult);


            if (result == true)
            {

        
                return 1;
            }
            else
                return 0;

            //return status code of request 
        }

        public void Parse_Page(string content, string URL)
        {
            //ali  khalil


            List<string> new_links = new List<string>();
            HTMLDocument doc = new HTMLDocument();
            IHTMLDocument2 myDoc = (IHTMLDocument2)(doc);
            myDoc.write(content);
            IHTMLElementCollection elements = myDoc.links;



            string link;
            foreach (IHTMLElement el in elements)
            {
                link = (string)el.getAttribute("href", 0);
                if (validate_URL(link) == 1)
                {
                    if (!visited_links.Contains(link) && !non_visited_links.Contains(link))
                    {
                        if (link.StartsWith("about://"))
                            link = link.Replace("about:/", "https:/");
                        else
                            link = link.Replace("about:", URL);
                        non_visited_links.Enqueue(link);

                    }

                }
                else
                    not_working_links.Enqueue(link);
            }

            // return new_links;



        }
        public List<string> Parse_Page_(string content, string URL)
        {
            //ali  khalil

            List<string> new_links = new List<string>();
            HTMLDocument doc = new HTMLDocument();
            IHTMLDocument2 myDoc = (IHTMLDocument2)(doc);
            myDoc.write(content);
            IHTMLElementCollection elements = myDoc.links;
            string link;
            foreach (IHTMLElement el in elements)
            {
                link = (string)el.getAttribute("href", 0);
                if (validate_URL(link) == 1)
                {
                    if (!visited_links.Contains(link) && !non_visited_links.Contains(link))
                    {
                        if (link.StartsWith("about://"))
                            link = link.Replace("about:/", "https:/");
                        else
                            link = link.Replace("about:", URL);
                        // non_visited_links.Enqueue(link);

                        if (!new_links.Contains(link))
                        {
                            new_links.Add(link);

                        }
                    }

                }
                else
                    not_working_links.Enqueue(link);
            }

            return new_links;



        }

        public List<string> Pars_Robots(string content)
        {
            List<string> links = new List<string>();

            //ali  khalil
            string[] lines = content.Split('\n');
            foreach (var item in lines)
            {
                string[] line = item.Split();
                if (line[0] == "Disallow:")
                    links.Add(line[1]);

            }
            return links;

        }
        private string language_detection( string content)
        {
            var factory = new RankedLanguageIdentifierFactory();
            var identifier = factory.Load(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "lag/Core14.profile.xml"));
            var languages = identifier.Identify(content);
            var mostCertainLanguage = languages.FirstOrDefault();
            if (mostCertainLanguage != null)
                return mostCertainLanguage.Item1.Iso639_3;
            else
                return "";


        }
    }
}