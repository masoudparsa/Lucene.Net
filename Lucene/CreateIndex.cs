using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using Lucene.Net.Search.Vectorhighlight;
using Lucene.Net.Store;
using Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lucene
{
    public static class CreateIndex
    {

        static readonly Lucene.Net.Util.Version _version = Lucene.Net.Util.Version.LUCENE_30;
        private static IndexSearcher _searcher;
        private static string  _path = AppDomain.CurrentDomain.BaseDirectory + "\\LuceneIndex";

        public static Document MapPostToDocument(Person person)
        {
            var personDocument = new Document();
            personDocument.Add(new Field("Id", person.Id.ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED));
            var nameField = new Field("Name", person.Name, Field.Store.YES, Field.Index.ANALYZED, Field.TermVector.WITH_POSITIONS_OFFSETS);
            nameField.Boost = 3;
            personDocument.Add(nameField);
            var familyField = new Field("Family", person.Family, Field.Store.YES, Field.Index.ANALYZED, Field.TermVector.WITH_POSITIONS_OFFSETS);
            familyField.Boost = 3;
            personDocument.Add(familyField);
            personDocument.Add(new Field("Barcode", person.Barcode, Field.Store.YES, Field.Index.ANALYZED, Field.TermVector.WITH_POSITIONS_OFFSETS));
            return personDocument;
        }

        public static void CreateFullTextIndex(IEnumerable<Person> dataList)
        {
            var directory = FSDirectory.Open(new DirectoryInfo(_path));
            var analyzer = new StandardAnalyzer(_version);

            if (!IndexReader.IndexExists(directory))
            {
                using (var writer = new IndexWriter(directory, analyzer, create: true, mfl: IndexWriter.MaxFieldLength.UNLIMITED))
                {
                    foreach (var post in dataList)
                    {
                        writer.AddDocument(MapPostToDocument(post));
                    }

                    writer.Optimize();
                    writer.Commit();
                    writer.Dispose();
                    directory.Dispose();
                }
            }
        }

        public static void UpdateIndex(Person person)
        {
            var directory = FSDirectory.Open(new DirectoryInfo(_path));
            var analyzer = new StandardAnalyzer(_version);
            using (var indexWriter = new IndexWriter(directory, analyzer, create: false, mfl: IndexWriter.MaxFieldLength.UNLIMITED))
            {
                var newDoc = MapPostToDocument(person);

                indexWriter.UpdateDocument(new Term("Id", person.Id.ToString()), newDoc);
                indexWriter.Commit();
                indexWriter.Dispose();
                directory.Dispose();
            }
        }

        public static void DeleteIndex(Person person)
        {
            var directory = FSDirectory.Open(new DirectoryInfo(_path));
            var analyzer = new StandardAnalyzer(_version);
            using (var indexWriter = new IndexWriter(directory, analyzer, create: false, mfl: IndexWriter.MaxFieldLength.UNLIMITED))
            {
                indexWriter.DeleteDocuments(new Term("Id", person.Id.ToString()));
                indexWriter.Commit();
                indexWriter.Dispose();
                directory.Dispose();
            }
        }

        public static void AddIndex(Person person)
        {
            var directory = FSDirectory.Open(new DirectoryInfo(_path));
            var analyzer = new StandardAnalyzer(_version);
            using (var indexWriter = new IndexWriter(directory, analyzer, create: false, mfl: IndexWriter.MaxFieldLength.UNLIMITED))
            {
                var searchQuery = new TermQuery(new Term("Id", person.Id.ToString()));
                indexWriter.DeleteDocuments(searchQuery);

                var newDoc = MapPostToDocument(person);
                indexWriter.AddDocument(newDoc);
                indexWriter.Commit();
                indexWriter.Dispose();
                directory.Dispose();
            }
        }
        public static IList<SearchResult> SearchPerson(string searchTerm)
        {
            int maxItems = 100;
            if (_searcher == null)
                _searcher = new IndexSearcher(FSDirectory.Open(new DirectoryInfo(_path)), true);
            var resultsList = new List<SearchResult>();
            if (string.IsNullOrWhiteSpace(searchTerm))
                return resultsList;
            var analyzer = new StandardAnalyzer(_version);
            QueryParser nameParser = new QueryParser(_version, "Name", analyzer);
            Query nameQuery = nameParser.Parse(searchByPartialWords(searchTerm));
            QueryParser familyParser = new QueryParser(_version, "Family", analyzer);
            Query familyQuery = familyParser.Parse(searchByPartialWords(searchTerm));
            QueryParser barcodeParser = new QueryParser(_version, "Barcode", analyzer);
            Query barcodeQuery = barcodeParser.Parse(searchByPartialWords(searchTerm));
            BooleanQuery finalQuery = new BooleanQuery();
            finalQuery.Add(nameQuery, Occur.SHOULD);
            finalQuery.Add(familyQuery, Occur.SHOULD);
            finalQuery.Add(barcodeQuery, Occur.SHOULD);
            var results = _searcher.Search(finalQuery, 10).ScoreDocs;
            //if (results.Length == 0)
            //{
            //    searchTerm = searchByPartialWords(searchTerm);
            //    query = parseQuery(searchTerm, parser);
            //    results = _searcher.Search(query, 10);
            //}
            foreach (var doc in results)
            {
                resultsList.Add(new SearchResult
                {
                    Value = _searcher.Doc(doc.Doc).Get("Barcode") + "-" + _searcher.Doc(doc.Doc).Get("Name") + "-" + _searcher.Doc(doc.Doc).Get("Family"),
                    Id = int.Parse(_searcher.Doc(doc.Doc).Get("Id"))
                });
            }
            return resultsList;
        }
        private static string searchByPartialWords(string bodyTerm)
        {
            bodyTerm = bodyTerm.Replace("*", "").Replace("?", "");
            var terms = bodyTerm.Trim().Replace("-", " ").Split(' ')
                                     .Where(x => !string.IsNullOrEmpty(x))
                                     .Select(x => x.Trim() + "*");
            bodyTerm = string.Join(" ", terms);
            return bodyTerm;
        }
        private static Query parseQuery(string searchQuery, QueryParser parser)
        {
            Query query;
            try
            {
                query = parser.Parse(searchQuery.Trim());
            }
            catch (ParseException)
            {
                query = parser.Parse(QueryParser.Escape(searchQuery.Trim()));
            }
            return query;
        }
    }
}
