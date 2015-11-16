using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Transactions;
using Newtonsoft.Json;
using System.Web.Mvc;
using vilniuseventsAPI.Models;

namespace vilniuseventsAPI.Controllers
{
    public class HomeController : Controller
    {
        // GET: Home
        public JsonResult Index()
        {
            try
            {
                using (var ts = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions(){IsolationLevel = IsolationLevel.ReadUncommitted}))
                using (var conn = EventsMySqlConnection.GetConnection())
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT facebookId, endDate FROM events";

                    var facebookIds = new List<EventModel>();

                    conn.Open();

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {       
                            facebookIds.Add(new EventModel()
                            {
                                FacebookId = reader.GetString(0),
                                EndingDateTime = !reader.IsDBNull(1) ? (DateTime?)reader.GetDateTime(1): null
                            });
                        }
                    }

                    ts.Complete();

                    return new JsonResult()
                    {
                        Data = facebookIds,
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet
                    };
                }
            }
            catch (Exception e)
            {
                return new JsonResult()
                {
                    Data = e.Message,
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
            }
        }

        // GET: Home/Details/5
        public string Add()
        {
            using (var ts = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = IsolationLevel.ReadUncommitted }))
            using (var conn = EventsMySqlConnection.GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                try
                {
                    Stream req = Request.InputStream;

                    req.Seek(0, SeekOrigin.Begin);

                    var json = new StreamReader(req).ReadToEnd();

                    var eventsModel =
                        JsonConvert.DeserializeObject<List<EventReceiveModel>>(json);

                    if (eventsModel != null)
                    {
                        conn.Open();
                        foreach (var eventModel in eventsModel)
                        {
                            if (eventModel.end_time != null)
                            {
                                var dateForMySql = ((DateTime) eventModel.end_time).ToString("yyyy-MM-dd HH:mm");

                                cmd.CommandText =
                                    $"INSERT INTO events (facebookId, endDate) VALUES ('{eventModel.id}', '{dateForMySql}') ON DUPLICATE KEY UPDATE endDate='{dateForMySql}'";
                                cmd.ExecuteNonQuery();
                            }
                            else
                            {
                                cmd.CommandText =
                                    $"INSERT INTO events (facebookId) VALUES ('{eventModel.id}') ON DUPLICATE KEY UPDATE facebookId=facebookId";
                                cmd.ExecuteNonQuery();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    return ex.Message;
                }

                ts.Complete();

                return "success";
            }
        }
    }
}
