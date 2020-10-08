using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using TMPro;
using NaughtyAttributes;
using Newtonsoft.Json;
using System.IO;

namespace PlacesGet
{
    public class PlacesRequest : MonoBehaviour
    {
        #region Vars
        public enum Query { placeID, Details, Photo, Nearby }

        const string searchRequest = @"https://maps.googleapis.com/maps/api/place/findplacefromtext/json?";
        const string detailsRequest = @"https://maps.googleapis.com/maps/api/place/details/json?";
        const string photoRequest = @"https://maps.googleapis.com/maps/api/place/photo?";
        const string nearbySearch = @"https://maps.googleapis.com/maps/api/place/nearbysearch/json?";
        const string inputType = "&inputtype=textquery";
        public string ApiKey; // API Key needs to be added here
        string place_id, location;

        // UI
        InputField searchInput;
        Image[] photoDisplays;
        public Transform photoParent;
        string searchQuery => searchInput.text;
        Review_Writer writer;
        public Sprite noPhoto;

        // data
        public Result detailsResult;
        public List<NearbyRoot> nearbyResults;

        // params
        public Vector2Int maxPhotoSize;
        public float nearbySearchRadius;
        public bool showSearchFields;
        [BoxGroup("Search Fields"), ShowIf(nameof(showSearchFields))]
        public bool business_status, formatted_address, vicinity, Name, photos, types, price_level, rating, user_ratings_total, review, opening_hours;

        // debug
        public bool debugQuery;

        #endregion

        #region Format_Query
        string GetFullQuery(Query query)
        {
            switch (query)
            {
                case Query.Details:
                    return detailsRequest + FormatPlaceId() + FormatFieldsToReturn(query) + FormatApiKey(); ;

                case Query.placeID:
                    return searchRequest + FormatSearchQuery() + inputType + FormatFieldsToReturn(query) + FormatApiKey(); ;

                case Query.Photo:
                    string fullQuery = photoRequest + FormatPhotoSize() + FormatPhotoRef() + FormatApiKey();
                    detailsResult.photos.RemoveAt(0);
                    return fullQuery;

                case Query.Nearby:
                    return nearbySearch + FormatLocation() + "&type=" + GetPlaceType() + "&rankby=prominence" + FormatApiKey();

            }
            return "";
        }

        string FormatSearchQuery()
        {
            return "input=" + searchQuery.Replace(" ", "%20");
        }

        string FormatFieldsToReturn(Query queryType)
        {
            string fieldsToFormat = "";
            if (queryType == Query.placeID)
            {
                fieldsToFormat += "place_id,geometry,";
            }
            else if (queryType == Query.Details)
            {
                if (types)
                    fieldsToFormat += nameof(types) + ',';
                if (photos)
                    fieldsToFormat += nameof(photos) + ',';
                if (Name)
                    fieldsToFormat += "name" + ',';
                if (formatted_address)
                    fieldsToFormat += nameof(formatted_address) + ',';
                if (business_status)
                    fieldsToFormat += nameof(business_status) + ',';
                if (user_ratings_total)
                    fieldsToFormat += nameof(user_ratings_total) + ',';
                if (rating)
                    fieldsToFormat += nameof(rating) + ',';
                if (price_level)
                    fieldsToFormat += nameof(price_level) + ',';
                if (review)
                    fieldsToFormat += nameof(review) + ',';
                if (opening_hours)
                    fieldsToFormat += nameof(opening_hours) + ',';
                if (vicinity)
                    fieldsToFormat += nameof(vicinity) + ',';

                fieldsToFormat += "geometry,";
            }


            //Debug.LogError("Field Formatted:");
            //Debug.LogError(fieldsToFormat);

            // remove final comma
            return "&fields=" + fieldsToFormat.Remove(fieldsToFormat.Length - 1, 1);
        }

        string GetPlaceType()
        {
            string type = detailsResult.types[0];
            if (type == "point_of_interest" && detailsResult.types.Count > 1)
            {
                type = detailsResult.types[1];
            }
            return type.ToString();
        }

        string FormatApiKey()
        {
            return "&key=" + ApiKey;
        }

        string FormatPlaceId()
        {
            return "place_id=" + place_id;
        }

        string FormatLocation()
        {
            return "location=" + location + "&radius=" + nearbySearchRadius;
            //return "location=" + location;
        }


        string FormatPhotoSize()
        {
            string toReturn = string.Empty;
            Vector2 sizeRef = maxPhotoSize != Vector2.zero ? maxPhotoSize : photoDisplays[0].rectTransform.sizeDelta;

            sizeRef = new Vector2((int)sizeRef.x, (int)sizeRef.y);

            if (sizeRef.x != 0)
                toReturn += "maxwidth=" + sizeRef.x;


            if (sizeRef.y != 0)
            {
                if (toReturn != string.Empty)
                    toReturn += "&";

                toReturn += "maxheight=" + sizeRef.y;
            }

            if (toReturn != string.Empty)
                return toReturn + "&";

            return toReturn;
        }

        string FormatPhotoRef()
        {
            return "photoreference=" + detailsResult.photos[0].photo_reference;
        }



        #endregion

        #region Init
        private void Awake()
        {
            Screen.SetResolution(800, 600, false);
            searchInput = FindObjectOfType<InputField>();
            writer = FindObjectOfType<Review_Writer>();
            photoDisplays = photoParent.GetComponentsInChildren<Image>();
        }

        #endregion

        #region Get_Data
        public IEnumerator GetPlacesApiData(Query query)
        {
            string fullQuery = GetFullQuery(query);
            if (debugQuery)
            {
                Debug.Log(query + " query:");
                Debug.Log(fullQuery);
            }
            using (UnityWebRequest webRequest = UnityWebRequest.Get(fullQuery))
            {
                // Request and wait for the desired page.
                yield return webRequest.SendWebRequest();

                if (webRequest.isNetworkError)
                {
                    Debug.LogError("Network Failed To Connect: " + webRequest.error);
                }
                else
                {
                    string json = webRequest.downloadHandler.text;
                    if (query == Query.placeID)
                    {
                        bool success = GetFirstSearchDataFields(json);
                        // restart routine
                        if (success)
                            StartCoroutine(GetPlacesApiData(Query.Details));
                        else
                            Debug.Log("No Results");
                        yield return null;
                    }
                    else if (query == Query.Details)
                    {
                        GetResultData(json, query);

                        ClearPreviousPhotos();
                        TryGetPhoto();
                    }
                    else if (query == Query.Photo)
                    {
                        DisplayPhoto(webRequest.downloadHandler.data);
                        TryGetPhoto();
                    }
                    else if (query == Query.Nearby)
                    {
                        GetResultData(json, query);
                    }
                }
            }
        }

        void TryGetPhoto()
        {
            // check if there is a photo to load in
            if (PhotoRefAvailable())
            {
                // check there is a disply to show photo
                if (PhotoSlotAvailable() != null)
                {
                    StartCoroutine(GetPlacesApiData(Query.Photo));
                }
            }
            else
            {
                while (PhotoSlotAvailable() != null)
                {
                    PhotoSlotAvailable().sprite = noPhoto;
                }
            }
        }

        void GetResultData(string json, Query query)
        {
            //output.text = json;
            if (query == Query.Details)
            {
                json = GetJsonPortion(json, "result");

                detailsResult = JsonConvert.DeserializeObject<Result>(json);

                StartCoroutine(GetPlacesApiData(Query.Nearby));
                return;
            }
            if (query == Query.Nearby)
            {
                json = GetJsonPortion(json, "results", true);
                nearbyResults = JsonConvert.DeserializeObject<List<NearbyRoot>>(json);
            }

            writer.WriteNewReview();
        }

        #endregion

        #region Do_Stuff_With_Data

        void DisplayPhoto(byte[] photoData)
        {
            // create texture container
            Texture2D tex = new Texture2D(2, 2);
            // get texture from byte[]
            tex.LoadImage(photoData);
            // create sprite to display texture
            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(.5f, .5f));
            // get display for texture
            Image photoDisplay = PhotoSlotAvailable();
            // put texture in display
            photoDisplay.sprite = sprite;
        }
        void ClearPreviousPhotos()
        {
            foreach (var item in photoDisplays)
            {
                item.sprite = null;
            }
        }

        #endregion

        #region Helpers
        bool PhotoRefAvailable()
        {
            if (detailsResult.photos == null)
                return false;

            if (detailsResult.photos.Count < 1)
                return false;

            if (detailsResult.photos[0].photo_reference.Length < 1)
                return false;

            return true;
        }

        Image PhotoSlotAvailable()
        {
            foreach (var item in photoDisplays)
            {
                if (item.sprite == null)
                {
                    return item;
                }
            }
            return null;
        }

        bool GetFirstSearchDataFields(string json)
        {
            json = GetJsonPortion(json, "candidates");
            try
            {
                FirstSearch data = JsonConvert.DeserializeObject<FirstSearch>(json);
                place_id = data.place_id;
                location = data.geometry.location.lat.ToString() + ',' + data.geometry.location.lng.ToString();
            }
            catch
            {
                return false;
            }

            return true;
        }

        string GetJsonPortion(string json, string objectName, bool isList = false)
        {
            //Debug.Log("pre " + objectName + " extraction json:");
            //Debug.Log(json);
            char delim1 = isList ? '[' : '{';
            char delim2 = isList ? ']' : '}';


            int startIndex = json.IndexOf(objectName);
            for (int i = startIndex; i < json.Length; i++)
            {
                if (json[i] == delim1)
                {
                    startIndex = i;
                    break;
                }
            }
            int braceCount = 0;
            int endIndex = 0;
            for (int i = startIndex; i < json.Length; i++)
            {
                if (json[i] == delim1)
                {
                    braceCount++;
                }
                else if (json[i] == delim2)
                {
                    braceCount--;
                }
                if (braceCount == 0)
                {
                    endIndex = i + 1;
                    break;
                }
            }
            return json.Substring(startIndex, endIndex - startIndex);
        }
        #endregion
    }
}
