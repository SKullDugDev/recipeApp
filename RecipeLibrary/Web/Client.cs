//using System;
//using System.Net.Http;
//using System.Threading.Tasks;

//namespace RecipeLibrary.Web
//{
//    class Client
//    {
      
//        public static async Task<HttpResponseMessage> GetSiteResponse(string recipeLink, IHttpClientFactory httpClientFactory)
//        {

//            var httpClient = httpClientFactory.CreateClient();

//            // if the link doesn't include the http:// scheme already
//            if (recipeLink.Contains("http:") == false)
//            {

//                // make a proper uri out of the link
//                var recipeURI = AddUriScheme(recipeLink);

//                try
//                {
//                    // make a header request and return the response
//                    var response = await httpClient.GetAsync(recipeURI.ToString(), HttpCompletionOption.ResponseHeadersRead);
//                    return response;
//                }
//                catch (Exception e)
//                {
//                    Console.WriteLine("Header Request refused...attempting a normal Get request for {0}...error as follows: {1}", recipeURI, e);
//                    var response = await httpClient.GetAsync(recipeURI);
//                    return response;
//                }

//            }

//            // if the link includes the https:// scheme already
//            try
//            {
//                // send a header request
//                var response = await httpClient.GetAsync(recipeLink, HttpCompletionOption.ResponseContentRead);
//                return response;
//            }
//            catch (Exception e)
//            {
//                Console.WriteLine("Header Request refused...attempting a normal Get request for {0}...error as follows: {1}", recipeLink, e);
//                var response = await httpClient.GetAsync(recipeLink);
//                return response;
//            }

//        }

//    }
//}
