﻿namespace DragDropPhoneApp.ApiConsumer
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Runtime.Serialization.Json;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;

    using Build.DataLayer.Model;

    using DragDropPhoneApp.ViewModel;

    using Microsoft.Phone.Controls;

    using Newtonsoft.Json;

    #endregion

    internal static class ApiService<T>
        where T : class
    {
        #region Static Fields

        private static int skip;

        private static int take = 1;

        private static Uri uriRealtApi = new Uri("http://localhost:61251/api/buildapi");

        private static Uri uriUserApi = new Uri("http://localhost:61251/api/userapi");

        #endregion

        #region Public Methods and Operators

        public static void GetRealties()
        {
            Deployment.Current.Dispatcher.BeginInvoke(() => { App.DataContext.IsLoading = true; });

            WebClient client = new WebClient();

            client.Headers["Accept"] = "application/json";
            client.DownloadStringCompleted += RealtyDownloadedCallback;
            if (take == 0)
            {
                // return;
            }

            client.DownloadStringAsync(
                new Uri(uriRealtApi.OriginalString + string.Format("?skip={0}&take={1}", skip, take)));
            skip += take;
        }

        public static void Login(string login, string pass)
        {
            WebClient client = new WebClient();

            HttpWebRequest myReq =
                (HttpWebRequest)
                WebRequest.Create(uriRealtApi.OriginalString + string.Format("?login={0}&pass={1}", login, pass));

            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(uriRealtApi);
            StartWebRequest(uriUserApi.OriginalString + string.Format("?login={0}&pass={1}", login, pass), null);
        }

        public static void RealtyDownloadedCallback(object s1, DownloadStringCompletedEventArgs e1)
        {
            Task.Factory.StartNew(
                () =>
                    {
                        var realtys = JsonConvert.DeserializeObject<Realty[]>(e1.Result);
                        if (realtys != null)
                        {
                            var newList = new List<Realty>();
                            newList.AddRange(App.DataContext.Realtys);
                            var downloadedRealtyList = realtys.ToList();
                            newList.AddRange(downloadedRealtyList);
                            if (downloadedRealtyList.Count < take)
                            {
                                Deployment.Current.Dispatcher.BeginInvoke(() => { App.DataContext.IsLoading = false; });
                                Thread.Sleep(100000);
                            }

                            App.DataContext.Realtys = newList;
                        }

                        GetRealties();
                    });
        }

        public static void SendPost(T gizmo, bool isRealtApi = true)
        {
            var serializedString = JsonConvert.SerializeObject(gizmo);

            DataContractJsonSerializer jsonData = new DataContractJsonSerializer(typeof(T));
            MemoryStream memStream = new MemoryStream();
            jsonData.WriteObject(memStream, serializedString);

            byte[] jsonDataToPost = memStream.ToArray();
            memStream.Close();

            var data1 = Encoding.UTF8.GetString(jsonDataToPost, 0, jsonDataToPost.Length);

            WebClient webClient = new WebClient();

            webClient.Headers["content-type"] = "application/json";
            if (isRealtApi)
            {
                webClient.UploadStringAsync(uriRealtApi, "POST", data1);
            }
            else
            {
                webClient.UploadStringAsync(uriUserApi, "POST", data1);
            }
        }

        #endregion

        #region Methods

        private static void FinishWebRequest(IAsyncResult result)
        {
            try
            {
                HttpWebResponse response =
                    (result.AsyncState as HttpWebRequest).EndGetResponse(result) as HttpWebResponse;
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    Deployment.Current.Dispatcher.BeginInvoke(
                        () =>
                            {
                                if (((PhoneApplicationFrame)Application.Current.RootVisual).DataContext is MainViewModel)
                                {
                                    (((PhoneApplicationFrame)Application.Current.RootVisual).DataContext as
                                     MainViewModel).IsLoading = false;
                                }

                                ((PhoneApplicationFrame)Application.Current.RootVisual).Navigate(
                                    new Uri("/RealtyList.xaml", UriKind.Relative));
                            });
                }
                else
                {
                    Deployment.Current.Dispatcher.BeginInvoke(
                        () => { MessageBox.Show("No user with such credentials"); });
                }
            }
            catch (WebException e)
            {
                Deployment.Current.Dispatcher.BeginInvoke(
                    () =>
                        {
                          
                            MessageBox.Show("No user with such credentials");
                        });
            }
        }

        private static void StartWebRequest(string url, AsyncCallback asyncCallback, string method = "GET")
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = method;
            AsyncCallback callback;
            if (asyncCallback == null)
            {
                callback = FinishWebRequest;
            }
            else
            {
                callback = asyncCallback;
            }

            request.BeginGetResponse(callback, request);
        }

        #endregion
    }
}