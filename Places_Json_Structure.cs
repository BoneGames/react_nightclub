using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace PlacesGet
{
    [Serializable]
    public class Close
    {
        public int day;
        public string time;
    }
    [Serializable]
    public class Open
    {
        public int day;
        public string time;
    }
    [Serializable]
    public class Period
    {
        public Close close;
        public Open open;
    }
    [Serializable]
    public class OpeningHours
    {
        public bool open_now;
        public List<Period> periods;
        public List<string> weekday_text;
    }
    [Serializable]
    public class Review
    {
        public string author_name;
        public string author_url;
        public string language;
        public string profile_photo_url;
        public int rating;
        public string relative_time_description;
        public string text;
        public int time;
    }
    [Serializable]
    public class Result
    {
        public string business_status;
        public string formatted_address;
        public string name;
        public OpeningHours opening_hours;
        public string place_id;
        public int price_level;
        public float rating;
        public List<Review> reviews;
        public List<string> types;
        public int user_ratings_total;
        public List<Photo> photos;
    }
    [Serializable]
    public class Location
    {
        public double lat;
        public double lng;
    }
    [Serializable]
    public class Northeast
    {
        public double lat;
        public double lng;
    }
    [Serializable]
    public class Southwest
    {
        public double lat;
        public double lng;
    }
    [Serializable]
    public class Viewport
    {
        public Northeast northeast;
        public Southwest southwest;
    }
    [Serializable]
    public class Geometry
    {
        public Location location;
        public Viewport viewport;
    }
    [Serializable]
    public class FirstSearch
    {
        public Geometry geometry;
        public string place_id;
    }
    [Serializable]
    public class Photo
    {
        public int height;
        public List<string> html_attributions;
        public string photo_reference;
        public int width;
    }
    [Serializable]
    public class Root1
    {
        public List<object> html_attributions;
        public Result result;
        public string status;

    }
    [Serializable]
    public class PlusCode
    {
        public string compound_code;
        public string global_code;
    }
    [Serializable]
    public class NearbyRoot
    {
        public string business_status;
        public Geometry geometry;
        public string icon;
        public string name;
        public OpeningHours opening_hours;
        public List<Photo> photos;
        public string place_id;
        public PlusCode plus_code;
        public double rating;
        public string reference;
        public string scope;
        public List<string> types;
        public int user_ratings_total;
        public string vicinity;
    }
}
