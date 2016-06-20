using Model;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace LuceneSample.Controllers
{
    public class HomeController : Controller
    {
     
        
        
        // GET: Home
        public ActionResult Index()
        {
            DataAccessLayer.Context context = new DataAccessLayer.Context();
            
            if (context.Persons.Count() <1000)
            {
                List<Person> personList = new List<Person>();
                int counter = 0;
                while (counter < 2000)
                {


                    string barcode = GeneratePersonRendom.GetBarcode();
                    Model.Person personExistObj = context.Persons.FirstOrDefault(p=>p.Barcode==barcode);
                    if (personExistObj == null)
                    {
                        personExistObj = new Person();
                        personExistObj.Address = "Tehran";
                        personExistObj.Barcode = barcode;
                        personExistObj.Family = GeneratePersonRendom.GetFamily();
                        personExistObj.FatherName = "mohammad";
                        personExistObj.Name = GeneratePersonRendom.GetName();
                        personExistObj.NationalCode = "99999999";
                        personExistObj.NOId = 0;
                        personExistObj.Tel = "09125270217";
                        context.Persons.Add(personExistObj);
                        context.SaveChanges();
                        Lucene.CreateIndex.AddIndex(personExistObj);
                        counter++;
                    }

                }
              
            }
            return View();
        }

        public virtual JsonResult ScoredTerms(string Prefix)
        {
            

           var result = new StringBuilder();
            var items =Lucene.CreateIndex.SearchPerson(Prefix).Take(100);
            IList<jsonAutoComplete> jsonItemList = new List<jsonAutoComplete>();
            foreach (var item in items)
            {
                jsonAutoComplete jsonObj = new jsonAutoComplete();
                jsonObj.label = item.Value;
                jsonObj.value = item.Id.ToString();
                jsonItemList.Add(jsonObj);
            }
            return Json(jsonItemList, JsonRequestBehavior.AllowGet);
        }

        internal class jsonAutoComplete
        {
            public string label { get; set; }
            public string value { get; set; }
        }
    }
}