//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Web;
//using System.Threading.Tasks;
//using Microsoft.AspNet.SignalR;
//using Owin;
//using Microsoft.Owin;

//[assembly: OwinStartup(typeof(HeO.Controllers.Startup))]
//namespace HeO.Controllers
//{
//    public class OrderHub : Hub
//    {
//        static List<UserData> Userdata = new List<UserData>(0);
//        public void userConnected()
//        {
//            var query = from u in Userdata
//                        where u.Memberid == Context.ConnectionId
//                        select u;
//            if (query.Count() == 0)
//            {
//                Userdata.Add(new UserData { Memberid = Context.ConnectionId });
//            }
//            Clients.All.getList(Userdata);
//        }

//        public override Task OnConnected()
//        {
//            Clients.All.removeList(Context.ConnectionId);

//            var item = Userdata.FirstOrDefault(a => a.Memberid == Context.ConnectionId);
//            if (item != null)
//            {
//                Userdata.Remove(item);
//                Clients.All.onUserDisconnected(item.Memberid);
//            }
//            return base.OnConnected();
//        }
//    }
//    public class UserData
//    {
//        public string Memberid { get; set; }
//    }

//    public class Startup
//    {
//        public void Configuration(IAppBuilder app)
//        {
//            // Any connection or hub wire up and configuration should go here
//            app.MapSignalR();
//        }
//    }
//}