using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using static StoryApi.ApiHandler.DataType;
using static StoryApi.ApiHandler.DataType.CommentData;

namespace StoryApi
{
    public static partial class ApiHandler
    {
        private static CookieContainer _cookieContainer { get; set; } = null;
        public static int MaxRetryCount { get; set; } = 10;

        public static void Init(CookieContainer cookieContainer)
        {
            _cookieContainer = cookieContainer;
        }
        public static async Task<ProfileData.ProfileObject> GetProfileFeed(string id, string from, bool noActivity = false)
        {
            string requestURI = "https://story.kakao.com/a/profiles/" + id + (!noActivity ? "?with=activities" : "");
            if (from != null)
                requestURI += "&since=" + from;
            HttpWebRequest webRequest = GenerateDefaultProfile(requestURI);
            string response = await GetResponseFromRequest(webRequest);
            ProfileData.ProfileObject obj = JsonConvert.DeserializeObject<ProfileData.ProfileObject>(response);
            return obj;
        }
        public static async Task<ProfileRelationshipData.ProfileRelationship> GetProfileRelationship(string id)
        {
            string requestURI = "https://story.kakao.com/a/profiles/" + id + "?profile_only=true";
            HttpWebRequest webRequest = GenerateDefaultProfile(requestURI);
            string response = await GetResponseFromRequest(webRequest);
            ProfileRelationshipData.ProfileRelationship obj = JsonConvert.DeserializeObject<ProfileRelationshipData.ProfileRelationship>(response);
            return obj;
        }
        public static async Task<TimeLineData.TimeLine> GetFeed(string from)
        {
            string requestURI = "https://story.kakao.com/a/feeds";
            if (from != null)
                requestURI += "?since=" + from;
            HttpWebRequest webRequest = GenerateDefaultProfile(requestURI);
            string response = await GetResponseFromRequest(webRequest);
            return JsonConvert.DeserializeObject<TimeLineData.TimeLine>(response);
        }
        public static async Task<FriendData.Friends> GetFriends()
        {
            string requestURI = "https://story.kakao.com/a/friends/";

            HttpWebRequest webRequest = GenerateDefaultProfile(requestURI);
            return JsonConvert.DeserializeObject<FriendData.Friends>(await GetResponseFromRequest(webRequest));
        }
        public static async Task<BookmarkData.Bookmarks> GetBookmarks(string id, string from)
        {
            string requestURI = "https://story.kakao.com/a/profiles/" + id + "/sections/bookmark";
            if (from != null)
                requestURI += $"?since={from}";
            HttpWebRequest webRequest = GenerateDefaultProfile(requestURI);
            string response = await GetResponseFromRequest(webRequest);
            return JsonConvert.DeserializeObject<BookmarkData.Bookmarks>(response);
        }
        public static async Task<string> GetScrapData(string url)
        {
            string requestURI = "https://story.kakao.com/a/scraper?url=" + Uri.EscapeDataString(url);
            HttpWebRequest webRequest = GenerateDefaultProfile(requestURI);
            return await GetResponseFromRequest(webRequest);
        }
        public static async Task<bool> SetActivityProfile(string id, string permission, bool enable_share, bool comment_all_writable, bool is_must_read)
        {
            string requestURI = "https://story.kakao.com/a/activities/" + id;

            HttpWebRequest webRequest = GenerateDefaultProfile(requestURI, "PUT");
            string postData = $"permission={permission}&enable_share={GetBoolString(enable_share)}&comment_all_writable={GetBoolString(comment_all_writable)}&is_must_read={GetBoolString(is_must_read)}";
            byte[] byteArray = Encoding.UTF8.GetBytes(postData);

            Stream writeStream = await webRequest.GetRequestStreamAsync();
            writeStream.Write(byteArray, 0, byteArray.Length);
            writeStream.Close();
            return await GetResponseFromRequest(webRequest) != null;
        }
        public static async Task<bool> MutePost(string id, bool mute)
        {
            string requestURI = "https://story.kakao.com/a/activities/" + id + "/mute_push";

            HttpWebRequest webRequest = GenerateDefaultProfile(requestURI, mute ? "POST" : "DELETE");
            string postData = $"push_mute={mute}";
            byte[] byteArray = Encoding.UTF8.GetBytes(postData);

            Stream writeStream = await webRequest.GetRequestStreamAsync();
            writeStream.Write(byteArray, 0, byteArray.Length);
            writeStream.Close();

            return await GetResponseFromRequest(webRequest) != null;
        }
        public static async Task<List<ShareData.Share>> GetShares(PostData data, bool isUP, string from = null)
        {

            string requestURI = "https://story.kakao.com/a/activities/" + data.id + "/shares/";
            if (isUP)
                requestURI = "https://story.kakao.com/a/activities/" + data.id + "/sympathies/";

            if (from != null)
                requestURI += $"?since={from}";

            HttpWebRequest webRequest = GenerateDefaultProfile(requestURI);
            string response = await GetResponseFromRequest(webRequest);
            return JsonConvert.DeserializeObject<List<ShareData.Share>>(response);
        }
        public static async Task<List<Comment>> GetComments(string id, string since)
        {
            string requestURI = "https://story.kakao.com/a/activities/" + id + "/comments?lpp=30&order=desc";
            if (since != null)
                requestURI += "&since=" + since;
            HttpWebRequest webRequest = GenerateDefaultProfile(requestURI);
            string response = await GetResponseFromRequest(webRequest);
            return JsonConvert.DeserializeObject<List<Comment>>(response);
        }
        public static async Task<UserProfile.ProfileData> GetProfileData()
        {
            string requestURI = "https://story.kakao.com/a/settings/profile";

            HttpWebRequest webRequest = GenerateDefaultProfile(requestURI);
            string response = await GetResponseFromRequest(webRequest);
            return JsonConvert.DeserializeObject<UserProfile.ProfileData>(response);
        }
        public static async Task<List<DataType.Actor>> GetSpecificFriend(string id)
        {
            string requestURI = "https://story.kakao.com/a/activities/" + id + "/specific_friends";
            HttpWebRequest webRequest = GenerateDefaultProfile(requestURI);
            string response = await GetResponseFromRequest(webRequest);
            return JsonConvert.DeserializeObject<List<DataType.Actor>>(response);
        }
        public static async Task<bool> LikeComment(string postId, string commentID, bool isDelete)
        {
            string requestURI = "https://story.kakao.com/a/activities/" + postId + "/comments/" + commentID + "/likes";
            string method;
            if (isDelete == true)
                method = "DELETE";
            else
                method = "POST";
            HttpWebRequest webRequest = GenerateDefaultProfile(requestURI, method);
            return await GetResponseFromRequest(webRequest) != null;
        }
        public static async Task<bool> RequestFriend(string id, bool isDelete)
        {
            string requestURI;
            string key;

            if (isDelete == true)
            {
                requestURI = "https://story.kakao.com/a/invitations/cancel";
                key = "user_id";
            }
            else
            {
                requestURI = "https://story.kakao.com/a/invitations";
                key = "friend_id";
            }

            HttpWebRequest webRequest = GenerateDefaultProfile(requestURI, "POST");

            string postData = $"{key}={id}&has_profile=true";
            byte[] byteArray = Encoding.UTF8.GetBytes(postData);
            Stream writeStream = await webRequest.GetRequestStreamAsync();
            writeStream.Write(byteArray, 0, byteArray.Length);
            writeStream.Close();

            return await GetResponseFromRequest(webRequest) != null;
        }
        public static async Task<bool> AcceptFriendRequest(string id, bool isDelete)
        {
            string requestURI;
            if (isDelete)
                requestURI = "https://story.kakao.com/a/invitations/ignore";
            else
                requestURI = "https://story.kakao.com/a/invitations/accept";

            HttpWebRequest webRequest = GenerateDefaultProfile(requestURI, "POST");

            string postData = $"inviter_id={id}&has_profile=true";
            byte[] byteArray = Encoding.UTF8.GetBytes(postData);
            Stream writeStream = await webRequest.GetRequestStreamAsync();
            writeStream.Write(byteArray, 0, byteArray.Length);
            writeStream.Close();

            return await GetResponseFromRequest(webRequest) != null;
        }
        public static async Task<bool> RequestFavorite(string id, bool isUnpin)
        {
            string requestURI = "https://story.kakao.com/a/friends/" + id + "/favorite";
            string method;
            if (isUnpin != true)
                method = "POST";
            else
                method = "DELETE";

            HttpWebRequest webRequest = GenerateDefaultProfile(requestURI, method);
            webRequest.Method = method;
            return await GetResponseFromRequest(webRequest) != null;
        }
        public static async Task<bool> PinPost(string id, bool isUnpin)
        {
            string requestURI = "https://story.kakao.com/a/activities/" + id + "/bookmark";
            string method;
            if (isUnpin != true)
                method = "POST";
            else
                method = "DELETE";

            HttpWebRequest webRequest = GenerateDefaultProfile(requestURI, method);
            return await GetResponseFromRequest(webRequest) != null;
        }
        public static async Task<bool> SharePost(string postId, string text, List<QuoteData> quoteDatas, string permission, bool commentable, List<string> with_ids, List<string> trust_ids)
        {
            string requestURI = "https://story.kakao.com/a/activities/" + postId + "/share";
            HttpWebRequest webRequest = GenerateDefaultProfile(requestURI, "POST");
            string textContent = Uri.EscapeDataString(JsonConvert.SerializeObject(quoteDatas).Replace("\"id\":null,", ""));

            string postData = "content=" + textContent
                + "&permission=" + permission + "&comment_all_writable=" + (commentable ? "true" : "false")
                + "&is_must_read=false&enable_share=true";

            if (with_ids.Count > 0)
                postData += "&with_tags=" + Uri.EscapeDataString(JsonConvert.SerializeObject(with_ids));
            if (trust_ids.Count > 0)
                postData += "&allowed_profile_ids=" + Uri.EscapeDataString(JsonConvert.SerializeObject(trust_ids));

            byte[] byteArray = Encoding.UTF8.GetBytes(postData);

            Stream writeStream = await webRequest.GetRequestStreamAsync();
            writeStream.Write(byteArray, 0, byteArray.Length);
            writeStream.Close();

            return await GetResponseFromRequest(webRequest) != null;
        }
        public static async Task<bool> UpPost(string postId, bool isDelete)
        {
            string requestURI = "https://story.kakao.com/a/activities/" + postId + "/sympathy";
            string method;
            if (isDelete)
                method = "DELETE";
            else
                method = "POST";

            HttpWebRequest webRequest = GenerateDefaultProfile(requestURI, method);
            return await GetResponseFromRequest(webRequest) != null;
        }
        public static async Task<bool> LikePost(string postId, string emotion)
        {
            string requestURI = "https://story.kakao.com/a/activities/" + postId + "/like";
            string method;
            if (emotion == null)
                method = "DELETE";
            else
                method = "POST";

            HttpWebRequest webRequest = GenerateDefaultProfile(requestURI, method);

            string postData;
            if (emotion == null)
                postData = "";
            else
                postData = "emotion=" + emotion;

            byte[] byteArray = Encoding.UTF8.GetBytes(postData);

            Stream writeStream = await webRequest.GetRequestStreamAsync();
            writeStream.Write(byteArray, 0, byteArray.Length);
            writeStream.Close();

            return await GetResponseFromRequest(webRequest) != null;
        }
        public static async Task<PostData> GetPost(string activityID)
        {
            string requestURI = "https://story.kakao.com/a/activities/" + activityID;
            HttpWebRequest webRequest = GenerateDefaultProfile(requestURI, "GET");
            string respResult = await GetResponseFromRequest(webRequest);

            PostData obj = null;
            if (respResult != null)
                obj = JsonConvert.DeserializeObject<PostData>(respResult);

            return obj;
        }
        public static async Task<bool> DeleteFriend(string id)
        {
            string requestURI = "https://story.kakao.com/a/friends/" + id;
            HttpWebRequest webRequest = GenerateDefaultProfile(requestURI, "DELETE");
            return await GetResponseFromRequest(webRequest) != null;
        }
        public static async Task<bool> DeleteLike(string postId, string id)
        {
            string requestURI = "https://story.kakao.com/a/activities/" + postId + "/like";

            HttpWebRequest webRequest = GenerateDefaultProfile(requestURI, "DELETE");

            string postData = $"id={id}";

            byte[] byteArray = Encoding.UTF8.GetBytes(postData);

            Stream writeStream = await webRequest.GetRequestStreamAsync();
            writeStream.Write(byteArray, 0, byteArray.Length);
            writeStream.Close();

            return await GetResponseFromRequest(webRequest) != null;
        }
        public static async Task<bool> DeleteBirthday()
        {
            string requestURI = "https://story.kakao.com/a/agreement/birth";
            HttpWebRequest webRequest = GenerateDefaultProfile(requestURI, "DELETE");
            return await GetResponseFromRequest(webRequest) != null;
        }
        public static async Task<bool> SetProfileName(string name)
        {
            string requestURI = "https://story.kakao.com/a/settings/profile/name";
            string postData = $"name={Uri.EscapeDataString(name)}";
            HttpWebRequest webRequest = GenerateDefaultProfile(requestURI, "POST");

            byte[] byteArray = Encoding.UTF8.GetBytes(postData);
            Stream writeStream = await webRequest.GetRequestStreamAsync();
            writeStream.Write(byteArray, 0, byteArray.Length);
            writeStream.Close();

            return await GetResponseFromRequest(webRequest) != null;
        }
        public static async Task<bool> SetBirthday(DateTime date, bool isLunar, bool isLeapType)
        {
            string requestURI = "https://story.kakao.com/a/settings/profile/birthday";
            string postData = $"birth={Uri.EscapeDataString(date.ToString("yyyyMMdd"))}&birth_type={Uri.EscapeDataString(isLeapType == true ? "-" : "+")}&birth_leap_type={isLeapType.ToString().ToLower()}";
            HttpWebRequest webRequest = GenerateDefaultProfile(requestURI, "PUT");

            byte[] byteArray = Encoding.UTF8.GetBytes(postData);
            Stream writeStream = await webRequest.GetRequestStreamAsync();
            writeStream.Write(byteArray, 0, byteArray.Length);
            writeStream.Close();

            return await GetResponseFromRequest(webRequest) != null;
        }
        public static async Task<bool> SetGender(string gender, string permission)
        {
            string requestURI = "https://story.kakao.com/a/settings/profile/gender";
            string postData = $"gender={gender}&permission={permission}";
            HttpWebRequest webRequest = GenerateDefaultProfile(requestURI, "PUT");

            byte[] byteArray = Encoding.UTF8.GetBytes(postData);
            Stream writeStream = await webRequest.GetRequestStreamAsync();
            writeStream.Write(byteArray, 0, byteArray.Length);
            writeStream.Close();

            return await GetResponseFromRequest(webRequest) != null;
        }
        public static async Task<bool> DeleteGender()
        {
            string requestURI = "https://story.kakao.com/a/settings/profile/gender";
            string postData = $"gender=&permission=";
            HttpWebRequest webRequest = GenerateDefaultProfile(requestURI, "PUT");

            byte[] byteArray = Encoding.UTF8.GetBytes(postData);
            Stream writeStream = await webRequest.GetRequestStreamAsync();
            writeStream.Write(byteArray, 0, byteArray.Length);
            writeStream.Close();

            return await GetResponseFromRequest(webRequest) != null;
        }
        public static async Task<bool> SetStatusMessage(string message)
        {
            string requestURI = "https://story.kakao.com/a/settings/profile/status_message";
            string postData = $"status_message={Uri.EscapeDataString(message)}";
            HttpWebRequest webRequest = GenerateDefaultProfile(requestURI, "PUT");

            byte[] byteArray = Encoding.UTF8.GetBytes(postData);
            Stream writeStream = await webRequest.GetRequestStreamAsync();
            writeStream.Write(byteArray, 0, byteArray.Length);
            writeStream.Close();

            return await GetResponseFromRequest(webRequest) != null;
        }
        public static async Task<bool> SendMail(string content, string id, bool bomb, string imgURI = null)
        {
            string requestURI = "https://story.kakao.com/a/messages?_=" + ((long)DateTime.Now.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds - 32400).ToString() + "11149";
            string objectStr = $"&object=%7B%22background%22%3A%7B%22type%22%3A%22color%22%2C%22value%22%3A{new Random().Next(10983816, 10983816)}%7D%7D";

            if (imgURI != null)
                objectStr = "";

            string postData = $"content={Uri.EscapeDataString("[{\"type\":\"text\",\"text\":\"" + content + "\"}]")}&bomb={bomb.ToString().ToLower()}" + objectStr + $"&receiver_id%5B%5D={id}&reference_id=";

            HttpWebRequest webRequest = GenerateDefaultProfile(requestURI, "POST");
            webRequest.Headers["Origin"] = "https://story.kakao.com";
            webRequest.Headers["Cache-Control"] = "no-cache";
            webRequest.Referer = "https://story.kakao.com/";

            byte[] byteArray = Encoding.UTF8.GetBytes(postData);
            Stream writeStream = await webRequest.GetRequestStreamAsync();
            writeStream.Write(byteArray, 0, byteArray.Length);
            writeStream.Close();

            return await GetResponseFromRequest(webRequest) != null;
        }
        public static async Task<List<MailData.Mail>> GetMails(string since = null)
        {
            string requestURI = "https://story.kakao.com/a/messages/";
            if (since != null)
                requestURI += $"?since={since}";

            HttpWebRequest webRequest = GenerateDefaultProfile(requestURI, "GET");

            return JsonConvert.DeserializeObject<List<MailData.Mail>>(await GetResponseFromRequest(webRequest)); ;
        }
        public static async Task<MailData.MailDetail> GetMailDetail(string id)
        {
            string requestURI = "https://story.kakao.com/a/messages/" + id;

            HttpWebRequest webRequest = GenerateDefaultProfile(requestURI, "GET");

            return JsonConvert.DeserializeObject<MailData.MailDetail>(await GetResponseFromRequest(webRequest)); ;
        }
        public static async Task<bool> DeleteMail(string id)
        {
            string requestURI = "https://story.kakao.com/a/messages/" + id;

            HttpWebRequest webRequest = GenerateDefaultProfile(requestURI, "DELETE");

            return await GetResponseFromRequest(webRequest) != null;
        }
        public static async Task<List<Notification>> GetNotifications()
        {
            string requestURI = "https://story.kakao.com/a/notifications";
            HttpWebRequest webRequest = GenerateDefaultProfile(requestURI);
            string response = await GetResponseFromRequest(webRequest);
            return JsonConvert.DeserializeObject<List<Notification>>(response);
        }
        public static async Task<bool> DeletePost(string id)
        {
            string requestURI = "https://story.kakao.com/a/activities/" + id;
            HttpWebRequest webRequest = GenerateDefaultProfile(requestURI, "DELETE");
            return await GetResponseFromRequest(webRequest) != null;
        }
        public static async Task<bool> ReplyToPost(string postId, string text, List<QuoteData> quoteDatas, UploadedImageProp img = null)
        {
            string requestURI = "https://story.kakao.com/a/activities/" + postId + "/comments";
            string textContent = Uri.EscapeDataString(JsonConvert.SerializeObject(quoteDatas).Replace("\"id\":null,", ""));

            string postData;
            string imageData2 = "";

            if (img != null)
            {
                imageData2 = "(Image) ";
                string imageData = "{\"media_path\":\"" + img.access_key + "/" + img.info.original.filename + "?width=" + img.info.original.width + "&height=" + img.info.original.height + "&avg=" + img.info.original.avg + "\",\"type\":\"image\",\"text\":\"(Image) \"},";
                textContent = textContent.Insert(3, Uri.EscapeDataString(imageData));
            }

            postData = "text=" + Uri.EscapeDataString(imageData2 + text) + "&decorators=" + textContent;

            postData = postData.Replace("%20", "+");

            byte[] byteArray = Encoding.UTF8.GetBytes(postData);


            HttpWebRequest webRequest = GenerateDefaultProfile(requestURI, "POST");

            Stream writeStream = await webRequest.GetRequestStreamAsync();
            writeStream.Write(byteArray, 0, byteArray.Length);
            writeStream.Close();

            return await GetResponseFromRequest(webRequest) != null;
        }
        public static async Task<bool> DeleteComment(string commentID, PostData data)
        {
            string requestURI = "https://story.kakao.com/a/activities/" + data.id + "/comments/" + commentID;
            HttpWebRequest webRequest = GenerateDefaultProfile(requestURI, "DELETE");
            return await GetResponseFromRequest(webRequest) != null;
        }
        public static async Task EditComment(Comment comment, PostData data, List<QuoteData> quoteDatas, string text)
        {
            string requestURI = "https://story.kakao.com/a/activities/" + data.id + "/comments/" + comment.id + "/content";

            string textContent = Uri.EscapeDataString(JsonConvert.SerializeObject(quoteDatas).Replace("\"id\":null,", ""));
            string imageData2 = "";
            foreach (QuoteData qdata in comment.decorators)
            {
                if (qdata.media_path != null)
                {
                    imageData2 = "(Image) ";
                    string imageData = JsonConvert.SerializeObject(qdata, Formatting.None, new JsonSerializerSettings()
                    {
                        NullValueHandling = NullValueHandling.Ignore
                    });
                    textContent = textContent.Insert(3, Uri.EscapeDataString(imageData));
                }
            }
            string postData = "text=" + Uri.EscapeDataString(imageData2 + text);
            postData += "&decorators=" + textContent;

            postData = postData.Replace("%20", "+");

            byte[] byteArray = Encoding.UTF8.GetBytes(postData);

            HttpWebRequest webRequest = GenerateDefaultProfile(requestURI, "PUT");

            Stream writeStream = await webRequest.GetRequestStreamAsync();
            writeStream.Write(byteArray, 0, byteArray.Length);
            writeStream.Close();

            await GetResponseFromRequest(webRequest);
        }
        public static async Task<List<ShareData.Share>> GetShares(bool isUP, PostData data, string from)
        {

            string requestURI = "https://story.kakao.com/a/activities/" + data.id + "/shares/";
            if (isUP)
                requestURI = "https://story.kakao.com/a/activities/" + data.id + "/sympathies/";

            if (from != null)
                requestURI += $"?since={from}";

            HttpWebRequest webRequest = GenerateDefaultProfile(requestURI);
            string response = await GetResponseFromRequest(webRequest);
            return JsonConvert.DeserializeObject<List<ShareData.Share>>(response);
        }
        public static async Task<List<ShareData.Share>> GetLikes(PostData data, string from)
        {
            string requestURI = "https://story.kakao.com/a/activities/" + data.id + "/likes/";
            if (from != null)
                requestURI += "?since=" + from;
            var webRequest = GenerateDefaultProfile(requestURI);
            string response = await GetResponseFromRequest(webRequest);
            return JsonConvert.DeserializeObject<List<ShareData.Share>>(response);
        }
        private static async Task<string> GetResponseFromRequest(WebRequest webRequest, int count = 0)
        {
            try
            {
                var readStream = await webRequest.GetResponseAsync();
                var respReader = readStream.GetResponseStream();

                string respResult = await new StreamReader(respReader).ReadToEndAsync();

                respReader.Close();
                readStream.Close();
                return respResult;
            }
            catch (WebException e)
            {
                if ((int)(e.Response as HttpWebResponse).StatusCode == 401)
                {
                    throw new InvalidOperationException("Invalid Cookie");
                }
                else
                {
                    if (count < MaxRetryCount)
                        return await GetResponseFromRequest(webRequest, ++count);
                }
                return null;
            }
        }

        /// <summary>
        /// Writes multi part HTTP POST request. Author : Farhan Ghumra
        /// </summary>
        private static void WriteMultipartForm(Stream s, string boundary, Dictionary<string, string> data, string fileName, string fileContentType, Stream fileStream)
        {
            /// The first boundary
            byte[] boundarybytes = Encoding.UTF8.GetBytes("--" + boundary + "\r\n");
            /// the last boundary.
            byte[] trailer = Encoding.UTF8.GetBytes("\r\n--" + boundary + "--\r\n");
            /// the form data, properly formatted
            /// Content-Disposition: form-data; name="file_1"; filename="waifu.png"
            //Content - Type: image / png

            string formdataTemplate = "Content-Disposition; name=\"{0}\"\r\n\r\n{1}";
            /// the form-data file upload, properly formatted
            string fileheaderTemplate = "Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\";\r\nContent-Type: {2}\r\n\r\n";

            /// Added to track if we need a CRLF or not.
            bool bNeedsCRLF = false;

            if (data != null)
            {
                foreach (string key in data.Keys)
                {
                    /// if we need to drop a CRLF, do that.
                    if (bNeedsCRLF)
                        WriteToStream(s, "\r\n");

                    /// Write the boundary.
                    WriteToStream(s, boundarybytes);

                    /// Write the key.
                    WriteToStream(s, string.Format(formdataTemplate, key, data[key]));
                    bNeedsCRLF = true;
                }
            }

            /// If we don't have keys, we don't need a crlf.
            if (bNeedsCRLF)
                WriteToStream(s, "\r\n");

            WriteToStream(s, boundarybytes);
            WriteToStream(s, string.Format(fileheaderTemplate, "file_1", fileName, fileContentType));
            // Write the file data to the stream.
            byte[] buffer = new byte[4096];
            while ((fileStream.Read(buffer, 0, buffer.Length)) != 0)
            {
                WriteToStream(s, buffer);
            }
            fileStream.Dispose();
            WriteToStream(s, trailer);
        }

        /// <summary>
        /// Writes string to stream. Author : Farhan Ghumra
        /// </summary>
        private static void WriteToStream(Stream s, string txt)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(txt);
            s.Write(bytes, 0, bytes.Length);
        }
        private static void WriteToStream(Stream s, byte[] bytes)
        {
            s.Write(bytes, 0, bytes.Length);
        }
        public static async Task<UploadedImageProp> UploadImage(string filepath)
        {
            string filename = Path.GetFileName(filepath);
            StreamReader fileStream = new StreamReader(filepath);

            string requestURI = "https://up-api-kage-4story.kakao.com/web/webstory-img/";

            HttpWebRequest request = WebRequest.CreateHttp(requestURI);
            request.Method = "POST";
            string boundary = "----" + DateTime.Now.Ticks.ToString("x");
            request.ContentType = "multipart/form-data; boundary=" + boundary;
            request.CookieContainer = _cookieContainer;

            request.Headers["X-Kakao-DeviceInfo"] = "web:d;-;-";
            request.Headers["X-Kakao-ApiLevel"] = "45";
            request.Headers["X-Requested-With"] = "XMLHttpRequest";
            request.Headers["X-Kakao-VC"] = "1b242cf8fa50f1f96765";
            request.Headers["Cache-Control"] = "max-age=0";
            request.Headers["Accept-Encoding"] = "gzip, deflate, br";
            request.Headers["Accept-Language"] = "ko-KR,ko;q=0.8,en-US;q=0.6,en;q=0.4";

            request.Headers["DNT"] = "1";

            request.Headers["authority"] = "story.kakao.com";
            request.Referer = "https://story.kakao.com";
            request.KeepAlive = true;
            request.UseDefaultCredentials = true;
            request.Host = "up-api-kage-4story.kakao.com";
            request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/68.0.3440.106 Safari/537.36";
            request.Accept = "*/*";
            request.AutomaticDecompression = DecompressionMethods.GZip;

            Stream writeStream = await request.GetRequestStreamAsync();

            WriteMultipartForm(writeStream, boundary, null, filename, MimeTypes.GetMimeType(filename), fileStream.BaseStream);

            var readStream = await request.GetResponseAsync();
            var respReader = readStream.GetResponseStream();

            string respResult = await (new StreamReader(respReader, Encoding.UTF8)).ReadToEndAsync();
            respReader.Close();

            UploadedImageProp result = JsonConvert.DeserializeObject<UploadedImageProp>(respResult);
            return result;
        }
        public static async Task<string> UploadVideo(AssetData asset)
        {
            StreamReader fileStream = new StreamReader(asset.Path);

            string requestURI = "https://up-api-kage-4story-video.kakao.com/web/webstory-video/";

            string boundary = "----" + DateTime.Now.Ticks.ToString("x");

            HttpWebRequest request = WebRequest.CreateHttp(requestURI);
            request.Method = "POST";
            request.ContentType = "multipart/form-data; boundary=" + boundary;
            request.CookieContainer = _cookieContainer;

            request.Headers["X-Kakao-DeviceInfo"] = "web:d;-;-";
            request.Headers["X-Kakao-ApiLevel"] = "46";
            request.Headers["X-Requested-With"] = "XMLHttpRequest";
            request.Headers["X-Kakao-VC"] = "185412afe1da9580e67f";
            request.Headers["Cache-Control"] = "max-age=0";
            request.Headers["Accept-Encoding"] = "gzip, deflate, br";
            request.Headers["Accept-Language"] = "ko-KR,ko;q=0.8,en-US;q=0.6,en;q=0.4";

            request.Headers["DNT"] = "1";

            request.Headers["authority"] = "story.kakao.com";
            request.Referer = "https://story.kakao.com";
            request.KeepAlive = true;
            request.UseDefaultCredentials = true;
            request.Host = "up-api-kage-4story-video.kakao.com";
            request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/68.0.3440.106 Safari/537.36";
            request.Accept = "*/*";
            request.AutomaticDecompression = DecompressionMethods.GZip;

            Stream writeStream = await request.GetRequestStreamAsync();

            WriteMultipartForm(writeStream, boundary, null, System.IO.Path.GetFileName(asset.Path), MimeTypes.GetMimeType(asset.Path), fileStream.BaseStream);
            fileStream.Close();

            var readStream = await request.GetResponseAsync();
            var respReader = readStream.GetResponseStream();

            string respResult = await (new StreamReader(respReader, Encoding.UTF8)).ReadToEndAsync();
            respReader.Close();

            var videoData = JsonConvert.DeserializeObject<VideoData.Video>(respResult);
            return videoData.access_key;
        }
        public static int retryCount = 0;
        public static async Task<bool> WritePost(List<QuoteData> quoteDatas, MediaData mediaData, string permission, bool isCommentable, bool isSharable, List<string> with_ids, List<string> trust_ids, string scrapDataString = null, bool isEdit = false, List<string> editOldMediaPaths = null, string editPostId = null)
        {
            try
            {
                if (editOldMediaPaths is null)
                    editOldMediaPaths = new List<string>();

                string commentable = isCommentable ? "true" : "false";
                string sharable = isSharable ? "true" : "false";
                string textContent = Uri.EscapeDataString(JsonConvert.SerializeObject(quoteDatas, Formatting.None, new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore
                }));
                StringBuilder postDataBuilder = new StringBuilder();
                postDataBuilder.Append("permission=" + permission + "&comment_all_writable=" + commentable + "&is_must_read=false&enable_share=" + sharable);
                postDataBuilder.Append("&content=" + textContent);

                if ((with_ids?.Count ?? 0) > 0)
                    postDataBuilder.Append("&with_tags=" + Uri.EscapeDataString(JsonConvert.SerializeObject(with_ids)));
                if ((trust_ids?.Count ?? 0) > 0)
                    postDataBuilder.Append("&allowed_profile_ids=" + Uri.EscapeDataString(JsonConvert.SerializeObject(trust_ids)));

                string mediaText = JsonConvert.SerializeObject(mediaData);
                if (mediaText != null)
                {
                    postDataBuilder.Append("&" + Uri.EscapeDataString("media") + "=" + Uri.EscapeDataString(mediaText));
                }
                foreach (string mediaPath in editOldMediaPaths)
                {
                    postDataBuilder.Append("&" + Uri.EscapeDataString("old_media_path[]") + "=" + Uri.EscapeDataString(mediaPath));
                }

                if (scrapDataString != null)
                {
                    postDataBuilder.Append("&scrap_content=" + Uri.EscapeDataString(scrapDataString));
                }

                string postData = postDataBuilder.ToString();

                byte[] byteArray = Encoding.UTF8.GetBytes(postData);

                string requestURI = "https://story.kakao.com/a/activities";
                if (isEdit)
                    requestURI = "https://story.kakao.com/a/activities/" + editPostId + "/content";

                HttpWebRequest request = WebRequest.CreateHttp(requestURI);
                request.Method = "POST";
                if (isEdit)
                    request.Method = "PUT";
                request.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";

                request.CookieContainer = _cookieContainer;
                request.Headers["X-Kakao-DeviceInfo"] = "web:d;-;-";
                request.Headers["X-Kakao-ApiLevel"] = "46";
                request.Headers["X-Requested-With"] = "XMLHttpRequest";
                request.Headers["X-Kakao-VC"] = "185412afe1da9580e67f";
                request.Headers["Cache-Control"] = "max-age=0";

                request.Headers["Accept-Encoding"] = "gzip, deflate, br";
                request.Headers["Accept-Language"] = "ko";

                request.Headers["DNT"] = "1";

                request.Headers["authority"] = "story.kakao.com";
                request.Referer = "https://story.kakao.com";
                request.KeepAlive = true;
                request.UseDefaultCredentials = true;
                request.Host = "story.kakao.com";
                request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/68.0.3440.106 Safari/537.36";
                request.Accept = "application/json";

                //Console.WriteLine("L4");
                Stream writeStream = await request.GetRequestStreamAsync();
                writeStream.Write(byteArray, 0, byteArray.Length);

                //Console.WriteLine("L3");
                var readStream = await request.GetResponseAsync();
                //Console.WriteLine("L2");
                var respReader = readStream.GetResponseStream();
                await (new StreamReader(respReader, Encoding.UTF8)).ReadToEndAsync();
                respReader.Close();
                retryCount = 0;
            }
            catch (WebException ex)
            {
                File.AppendAllText("ErrorNative.log", $"{DateTime.Now}: {ex.Message}\n{ex.StackTrace}{Environment.NewLine}");
                var readStream = ex.Response;
                var respReader = readStream.GetResponseStream();
                var response = readStream as HttpWebResponse;
                var content = await (new StreamReader(respReader, Encoding.UTF8)).ReadToEndAsync();
                File.AppendAllText("Error.log", $"{DateTime.Now}: {content}{Environment.NewLine}");
                if (response?.StatusCode == HttpStatusCode.BadRequest)
                {
                    retryCount++;
                    await Task.Delay(500);
                    if (retryCount <= 5)
                        return await WritePost(quoteDatas, mediaData, permission, isCommentable, isSharable, with_ids, trust_ids, scrapDataString, isEdit, editOldMediaPaths, editPostId);
                    else
                    {
                        return false;
                    }
                }
                else
                    return false;
            }
            return true;
        }
        public static async Task<string> UploadImage(AssetData asset)
        {
            StreamReader fileStream = new StreamReader(asset.Path);

            string requestURI = "https://up-api-kage-4story.kakao.com/web/webstory-img/";

            string boundary = "----" + DateTime.Now.Ticks.ToString("x");

            HttpWebRequest request = WebRequest.CreateHttp(requestURI);
            request.Method = "POST";
            request.ContentType = "multipart/form-data; boundary=" + boundary;
            request.CookieContainer = _cookieContainer;

            request.Headers["Accept-Encoding"] = "gzip, deflate, br";
            request.Headers["Accept-Language"] = "ko-KR";
            request.Headers["Origin"] = "https://story.kakao.com";

            request.Headers["DNT"] = "1";

            request.Referer = "https://story.kakao.com/";
            request.KeepAlive = true;
            request.UseDefaultCredentials = true;
            request.Host = "up-api-kage-4story.kakao.com";
            request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/68.0.3440.106 Safari/537.36";
            request.Accept = "*/*";
            request.AutomaticDecompression = DecompressionMethods.GZip;

            Stream writeStream = await request.GetRequestStreamAsync();

            WriteMultipartForm(writeStream, boundary, null, System.IO.Path.GetFileName(asset.Path), MimeTypes.GetMimeType(asset.Path), fileStream.BaseStream);
            fileStream.Close();

            var readStream = await request.GetResponseAsync();
            var respReader = readStream.GetResponseStream();

            string respResult = await (new StreamReader(respReader, Encoding.UTF8)).ReadToEndAsync();
            respReader.Close();

            UploadedImageProp result = JsonConvert.DeserializeObject<UploadedImageProp>(respResult);

            return result.access_key + "/" + result.info.original.filename + "?width=" + result.info.original.width + "&height=" + result.info.original.height + "&avg=" + result.info.original.avg;
        }
        private static async Task<bool> WaitForMetaVideoFinish(string access_key)
        {
            string requestURI = "https://story.kakao.com/a/kage/video/dn/" + access_key + "/meta.json";
            HttpWebRequest request = WebRequest.CreateHttp(requestURI);

            request.Method = "GET";

            request.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
            request.CookieContainer = _cookieContainer;

            request.Headers["X-Kakao-DeviceInfo"] = "web:d;-;-";
            request.Headers["X-Kakao-ApiLevel"] = "46";
            request.Headers["X-Requested-With"] = "XMLHttpRequest";
            request.Headers["X-Kakao-VC"] = "185412afe1da9580e67f";
            request.Headers["Cache-Control"] = "max-age=0";

            request.Headers["Accept-Encoding"] = "gzip, deflate, br";
            request.Headers["Accept-Language"] = "ko";

            request.Headers["DNT"] = "1";

            request.Headers["authority"] = "story.kakao.com";
            request.Referer = "https://story.kakao.com/";
            request.KeepAlive = true;
            request.UseDefaultCredentials = true;
            request.Host = "story.kakao.com";
            request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/68.0.3440.106 Safari/537.36";
            request.Accept = "application/json";

            var readStream = await request.GetResponseAsync();
            var respReader = readStream.GetResponseStream();
            await new StreamReader(respReader, Encoding.UTF8).ReadToEndAsync();
            respReader.Close();
            return true;
        }
        public static async Task<bool> WaitForVideoUploadFinish(string access_key)
        {
            string requestURI = "https://story.kakao.com/a/kage/video/wcheck/" + access_key + "/?_t=0";
            HttpWebRequest request = WebRequest.CreateHttp(requestURI);

            request.Method = "GET";

            request.CookieContainer = _cookieContainer;

            request.Headers["X-Kakao-DeviceInfo"] = "web:d;-;-";
            request.Headers["X-Kakao-ApiLevel"] = "46";
            request.Headers["X-Requested-With"] = "XMLHttpRequest";
            request.Headers["X-Kakao-VC"] = "185412afe1da9580e67f";

            request.Headers["Accept-Encoding"] = "gzip, deflate, br";
            request.Headers["Accept-Language"] = "ko";

            request.Headers["DNT"] = "1";

            request.AutomaticDecompression = DecompressionMethods.GZip;
            request.Headers["authority"] = "story.kakao.com";
            request.Referer = "https://story.kakao.com/";
            request.KeepAlive = true;
            request.UseDefaultCredentials = true;
            request.Host = "story.kakao.com";
            request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/68.0.3440.106 Safari/537.36";
            request.Accept = "application/json";

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            var respReader = response.GetResponseStream();
            string respResult = new StreamReader(respReader).ReadToEnd();
            respReader.Close();
            response.Close();
            VideoData.Percent pecrentData = JsonConvert.DeserializeObject<VideoData.Percent>(respResult);
            if (pecrentData.code == 200 && pecrentData.percent == 100)
                return await WaitForMetaVideoFinish(access_key);
            else
                await Task.Delay(500);
            return await WaitForVideoUploadFinish(access_key);
        }

        private static string GetBoolString(bool src)
        {
            return src ? "true" : "false";
        }
        private static HttpWebRequest GenerateDefaultProfile(string requestURI, string method = "GET")
        {
            HttpWebRequest webRequest = WebRequest.CreateHttp(requestURI);
            webRequest.Method = method.ToUpper();
            webRequest.ContentType = "application/x-www-form-urlencoded; charset=utf-8";

            webRequest.CookieContainer = _cookieContainer;

            webRequest.Headers["X-Kakao-DeviceInfo"] = "web:d;-;-";
            webRequest.Headers["X-Kakao-ApiLevel"] = "46";
            webRequest.Headers["X-Requested-With"] = "XMLHttpRequest";
            webRequest.Headers["X-Kakao-VC"] = Guid.NewGuid().ToString().ToLower().Substring(0, 20);
            webRequest.Headers["Cache-Control"] = "max-age=0";

            webRequest.Headers["Accept-Encoding"] = "gzip, deflate, br";
            webRequest.Headers["Accept-Language"] = "ko";

            webRequest.Headers["DNT"] = "1";

            webRequest.Referer = "https://story.kakao.com/";
            webRequest.KeepAlive = true;
            webRequest.UseDefaultCredentials = true;
            webRequest.Host = "story.kakao.com";
            webRequest.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/68.0.3440.106 Safari/537.36";
            webRequest.Accept = "application/json";

            webRequest.AutomaticDecompression = DecompressionMethods.GZip;
            webRequest.Date = DateTime.Now;

            return webRequest;
        }
    }
}
