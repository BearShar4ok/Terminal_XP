using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net;
using System.Text;

namespace Terminal_XP.Classes
{
    public static class RequestSender
    {
        public static void SendGet(string requestString)
        {
            try
            {
                string url = "http://" + requestString;

                using (HttpClient httpClient = new HttpClient())
                {
                    httpClient.GetAsync(url);
                }
            }
            catch (Exception)
            {

            }
           
        }
    }
}
