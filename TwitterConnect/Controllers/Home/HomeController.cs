using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using TweetSharp;

namespace TwitterConnect.Controllers.Home
{
    public class HomeController : Controller
    {
        //
        // GET: /Home/

        private const string ConsumerKey = "NUDy8gAHrtQCQH5yhnWNiw";
        private const string ConsumerSecret = "vJ6xFGtuCpa5DyXAJIbWkjRYI3H98c3waEvnNG1Zc";

        // Step 1 - Retrieve an OAuth Request Token

        TwitterService _service = new TwitterService(ConsumerKey, ConsumerSecret);

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Logar()
        {
            // This is the registered callback URL
            OAuthRequestToken requestToken = _service.GetRequestToken("http://localhost:57376/Home/AuthorizeCallback");

            // Step 2 - Redirect to the OAuth Authorization URL
            Uri uri = _service.GetAuthorizationUri(requestToken);
            return new RedirectResult(uri.ToString(), false /*permanent*/);
        }

        public ActionResult AuthorizeCallback(string oauth_token, string oauth_verifier)
        {
            var requestToken = new OAuthRequestToken { Token = oauth_token };

            // Step 3 - Exchange the Request Token for an Access Token
            OAuthAccessToken AccessToken = _service.GetAccessToken(requestToken, oauth_verifier);


            //COOKIES com os TOKENS
            HttpCookie Token = new HttpCookie("Token");
            Token.Value = AccessToken.Token;
            ControllerContext.HttpContext.Response.Cookies.Add(Token);

            HttpCookie TokenSecret = new HttpCookie("TokenSecret");
            TokenSecret.Value = AccessToken.TokenSecret;
            ControllerContext.HttpContext.Response.Cookies.Add(TokenSecret);

            // Step 4 - User authenticates using the Access Token
            _service.AuthenticateWith(AccessToken.Token, AccessToken.TokenSecret);
            TwitterUser user = _service.VerifyCredentials(new VerifyCredentialsOptions());
            _service.VerifyCredentials(new VerifyCredentialsOptions());
            return RedirectToAction("THome");
        }

        public ActionResult THome()
        {
            HttpCookie Token = ControllerContext.HttpContext.Request.Cookies["Token"];
            HttpCookie TokenSecret = ControllerContext.HttpContext.Request.Cookies["TokenSecret"];

            _service.AuthenticateWith(Token.Value, TokenSecret.Value);

            TwitterUser user = _service.VerifyCredentials(new VerifyCredentialsOptions());

            ViewBag.Nome = user.Name;
            ViewBag.Img = user.ProfileImageUrl.Substring(0, user.ProfileImageUrl.Length - 12);
            ViewBag.Descricao = user.Description;
            ViewBag.ScreenName = user.ScreenName;
            ViewBag.Background = user.ProfileBackgroundImageUrl;
            ViewBag.Seguidores = user.FollowersCount;
            ViewBag.Amigos = user.FriendsCount;
            ViewBag.Tweets = user.StatusesCount;


            //INFORMACOES PARA TESTE
            //ViewBag.Nome = "- #W e s ll e y.";
            //ViewBag.Img = "http://pbs.twimg.com/profile_images/2991190384/679abfd3d2ea6a034650894e3a3c8a9f";
            //ViewBag.Descricao = "“Tô na área, se derrubar é pênalti!” - Yusuke Urameshi";
            //ViewBag.ScreenName = "Weslley_Neri";
            //ViewBag.Background = "";
            //ViewBag.Seguidores = "74";
            //ViewBag.Amigos = "151";
            //ViewBag.Tweets = "1781";

            return View();
        }

        [HttpPost]
        public JsonResult ListTweets()
        {
            HttpCookie Token = ControllerContext.HttpContext.Request.Cookies["Token"];
            HttpCookie TokenSecret = ControllerContext.HttpContext.Request.Cookies["TokenSecret"];

            _service.AuthenticateWith(Token.Value, TokenSecret.Value);

            var tweets = _service.ListTweetsOnHomeTimeline(new ListTweetsOnHomeTimelineOptions());

            return Json(new {tweets}, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult Post(string msg)
        {
                //PEGANDO OS COOKIES
                HttpCookie Token = ControllerContext.HttpContext.Request.Cookies["Token"];
                HttpCookie TokenSecret = ControllerContext.HttpContext.Request.Cookies["TokenSecret"];

                _service.AuthenticateWith(Token.Value, TokenSecret.Value);
                _service.VerifyCredentials(new VerifyCredentialsOptions());

                try
                {
                    _service.SendTweet(new SendTweetOptions { Status = msg }, (tweet, response) =>
                    {
                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            //TweetOK = true;
                        }
                        else
                        {
                            throw new Exception(response.StatusCode.ToString());
                        }
                    });
                }
                catch (Exception)
                {
                    return Json(new { ERRO = "Tweet não enviado =(" }, JsonRequestBehavior.AllowGet);
                }
                    return Json(new {OK = "Tweet Enviado com Sucesso"}, JsonRequestBehavior.AllowGet);
        }

    }
}
