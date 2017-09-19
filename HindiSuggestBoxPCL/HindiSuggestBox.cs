using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml.Controls;
using Windows.Web.Http;

namespace sourcEleven.UWP.HindiSuggestBoxPCL
{
    public static class HindiSuggestBox
    {
        private static List<string> _imeResults;
        private static bool _isImeEnabled;

        static HindiSuggestBox()
        {
            _imeResults = new List<string>();
            _isImeEnabled = false;
        }

        public static void EnableHindiIME(this AutoSuggestBox box)
        {
            if (_isImeEnabled == false)
            {
                box.TextChanged += HindiSuggestBox_TextChanged;
                _isImeEnabled = true;
            }
        }

        public static void DisableHindiIME(this AutoSuggestBox box)
        {
            if (_isImeEnabled == true)
            {
                box.TextChanged -= HindiSuggestBox_TextChanged;
                _isImeEnabled = false;
            }
        }

        private static async void HindiSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                if (!String.IsNullOrWhiteSpace(sender.Text))
                {
                    using (HttpClient http = new HttpClient())
                    {
                        string uriString = "http://www.google.com/transliterate/indic?tlqt=1&langpair=en|hi&text=" + sender.Text + "&tl_app=3";
                        Uri requestUri = new Uri(uriString);

                        HttpResponseMessage response = new HttpResponseMessage();
                        string responseText = "";

                        try
                        {
                            response = await http.GetAsync(requestUri);
                            response.EnsureSuccessStatusCode();
                            responseText = await response.Content.ReadAsStringAsync();
                            responseText = responseText.TrimStart('[').TrimEnd(']');

                            HindiJsonResult res = Newtonsoft.Json.JsonConvert.DeserializeObject<HindiJsonResult>(responseText);

                            List<string> transliterations = res.hws.ToList();
                            sender.ItemsSource = transliterations;
                        }
                        catch (Exception ex)
                        {
                            sender.ItemsSource = null;
                            System.Diagnostics.Debug.WriteLine("ERROR GETTING TRANSLITERATIONS " + uriString + ":" + ex.ToString());
                        }
                    }
                }
                else
                {
                    sender.ItemsSource = null;
                }
            }
        }


        class HindiJsonResult
        {
            public string ew;
            public string[] hws;

            HindiJsonResult()
            {

            }

            HindiJsonResult(string input, string[] suggestions)
            {
                this.ew = input;
                this.hws = suggestions;
            }
        }
    }
}
