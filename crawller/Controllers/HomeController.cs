using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using crawller.Models;
using OpenNLP;
using OpenNLP.Tools.Tokenize;
using HtmlAgilityPack;
using System.Text;
using StopWord;

namespace crawller.Controllers
{
    public class HomeController : Controller
    {
       
        public ActionResult Index()
        {
           // cr c = new cr("https://edition.cnn.com");
            //indexing i = new indexing();
            return RedirectToAction("search");
        }
        public ActionResult search()
        {
            
            return View();
        }

        [HttpPost]
        public ActionResult search(string searchtext)
        {
            searchtext = searchtext.ToLower();

            Iveonik.Stemmers.EnglishStemmer englishStemmer = new Iveonik.Stemmers.EnglishStemmer();
          
            Entities db = new Entities();
            var modelPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "lag/EnglishTok.nbin");

            var tokenizer = new EnglishMaximumEntropyTokenizer(modelPath);
            List<string> tokens = tokenizer.Tokenize(searchtext).ToList();
            List<indexer> result = new List<indexer>();
            Dictionary<page, int> rank = new Dictionary<page, int>();
            foreach (var item in tokens)
            {
                string newtoken = englishStemmer.Stem(item);
                List<indexer> sub = db.indexers.Where(x => x.term.Equals(newtoken)).ToList();
                foreach (var item1 in sub)
                {
                    if (rank.ContainsKey(item1.page))
                    {
                        rank[item1.page] += 1;

                    }
                    else
                    {
                        rank.Add(item1.page, 1);
                    }

                }
                result.AddRange(sub);

            }

            List<object> data = new List<object>();
            foreach (var item in rank.OrderByDescending(key=>key.Value))
            {
                data.Add(new
                {
                pageid = item.Key.id,
                pageurl=item.Key.url,
                percentage = item.Value/tokens.Count*100,
                });

            }
            return Json(new {data=data});
        }

        //List<string> kgrame(string word)
        //{






        //}
    }
}