using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using crawller.Models;
using OpenNLP;
using OpenNLP.Tools.Tokenize;
using HtmlAgilityPack;
using System.Text;
using StopWord;
using System.IO;

namespace crawller.Models
{
    public class indexing
    {

        public  indexing()
        {
            Entities db = new Entities();
            var pages = db.pages.ToList();
            foreach (var item in pages)
            {
                List<string> tokens = tokenization(item.content.ToLower());
                List<string> tokens_After_punc_removed = remove_punctuations(tokens);
                List<indexer> indexers = new List<indexer>();
                int counter = 0;
                foreach (var item1 in tokens_After_punc_removed)
                {
                    indexer indexer = indexers.Where(x => x.term.Equals(item1)).FirstOrDefault();
                    if (indexer != null)
                    {
                        indexer.frequancy += 1;
                        indexer.position = counter;
                    }
                    else
                    {
                        indexer = new indexer();
                        indexer.doc_id = item.id;
                        indexer.term = item1;
                        indexer.frequancy = 1;
                        indexer.position = counter;
                        indexers.Add(indexer);
                    }
                    terms_before_Stemming term = db.terms_before_Stemming.Where(x => x.term.Equals(item1)).FirstOrDefault();
                    if (term == null)
                    {
                        terms_before_Stemming t = new terms_before_Stemming();
                        t.doc_id = item.id;
                        t.term = item1;
                        db.terms_before_Stemming.Add(t);
                        db.SaveChanges();
                    }
                    counter++;
                }
                List<string> tokens_After_remove_stopwords = remove_stopwords(tokens_After_punc_removed);
                List<indexer> tokens_After_stemming = stemmer(indexers);
                foreach (var item1 in tokens_After_stemming)
                {
                    db.indexers.Add(item1);
                    db.SaveChanges();
                }
                //int count = 0;
                //for (int i = 0; i < tokens_After_remove_stopwords.Count; i++)
                //{
                //    if (!tokens_After_remove_stopwords[i].Equals(tokens_After_stemming[i]))
                //        count++;
                //}
            }
        }

        private List<string> tokenization(string content)
        {

            HtmlDocument doc = new HtmlDocument();
            //  string html = /* whatever */;
            doc.LoadHtml(content);
            StringBuilder sb = new StringBuilder();
            IEnumerable<HtmlNode> nodes = doc.DocumentNode.Descendants().Where(n =>
               n.NodeType == HtmlNodeType.Text &&
               n.ParentNode.Name != "script" &&
               n.ParentNode.Name != "style");
            foreach (HtmlNode node in nodes)
                sb.AppendLine(node.InnerText);
            //var text = string.Join("", doc.DocumentNode.SelectNodes("//text()[normalize-space()]").
            //                            Select(t => t.InnerText));
            ////foreach (HtmlNode p in doc.DocumentNode.SelectNodes("//p"))
            //{
            //    string text = p.InnerText;
            //    // do whatever with text
            //}
            var modelPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "lag/EnglishTok.nbin");
            // var sentence = "- Sorry Mrs. Hudson, I'll skip the tea.";
            var tokenizer = new EnglishMaximumEntropyTokenizer(modelPath);
            var tokens = tokenizer.Tokenize(sb.ToString());
            return tokens.ToList();
        }

        private List<string> remove_punctuations(List<string> tokens)
        {
            string punctuations = "-?:!.,;[]'$``\\\n,&";
            List<string> newtokens = new List<string>();
            foreach (var item in tokens)
            {
                bool valid = true;
                foreach (var item1 in punctuations)
                {

                    if (item.Contains(item1))
                    {
                        string i = item.Replace(item1.ToString(), "");
                        if (i == "")
                        {
                            valid = false;

                        }
                    }
                }
                if (valid)
                {
                    newtokens.Add(item);

                }

            }
            return newtokens;

        }
        private List<string> remove_stopwords(List<string> tokens)
        {
            var stopWords = StopWords.GetStopWords("en");
            List<string> newtokens = new List<string>();
            foreach (var item in tokens)
            {
                bool valid = true;
                foreach (var item1 in stopWords)
                {

                    if (item.Contains(item1))
                    {
                        string i = item.Replace(item1, "");
                        if (i == "")
                        {
                            valid = false;

                        }
                    }
                }
                if (valid)
                {
                    newtokens.Add(item);

                }

            }
            return newtokens;

        }

        private List<indexer> stemmer(List<indexer> tokens)
        {

            Iveonik.Stemmers.EnglishStemmer englishStemmer = new Iveonik.Stemmers.EnglishStemmer();
            List<string> newtokens = new List<string>();
            foreach (var item in tokens)
                item.term = englishStemmer.Stem(item.term);
            return tokens;

        }

    }
}