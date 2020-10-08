using PlacesGet;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using System.Globalization;

public class Review_Writer : MonoBehaviour
{
    enum Style { Bold, Underline, Strikethrough, Italic}
    public PlacesRequest infoGetter;
    public TextMeshProUGUI output, mlOutput;
    char[] vowels = { 'a', 'e', 'i', 'o', 'u' };
    string styles = "bisu";


    public void NewPlaceSearch()
    {
        StartCoroutine(infoGetter.GetPlacesApiData(PlacesRequest.Query.placeID));
    }

    public void WriteNewReview()
    {
        Result detailsResult = infoGetter.detailsResult;
        List<NearbyRoot> nearbyResults = infoGetter.nearbyResults;
        if (detailsResult != null)
        {
            string review = Theme_Report();
            string reviewPt2 = In_The_Area();
            output.text = review + reviewPt2;
        }
        else
        {
            Debug.LogError("There is no place to review");
        }
    }

    string Theme_Report()
    {
        return TextStyle(infoGetter.detailsResult.name, Style.Bold) + " " + FormatTheme(infoGetter.detailsResult.types, true) + " themed nightclub. Visitor reports include: \"" + Get_I_Sentence(infoGetter.detailsResult.reviews) + "\"";
    }

    string In_The_Area()
    {
        return " It is one of " + infoGetter.nearbyResults.Count + " " + FormatTheme(infoGetter.detailsResult.types) + " themed nightclubs in the area. Most " + FormatTheme(infoGetter.detailsResult.types) + " themed nightclub enthusiasts toss up between " + TextStyle(infoGetter.detailsResult.name, Style.Bold) + " and " + TextStyle(infoGetter.nearbyResults[0].name, Style.Bold) + ", which is generally accepted as the more hipster option.";
    }

    string TextStyle(string input, Style _style)
    {
        // get style letter from enum
        string style = "";
        switch (_style)
        {
            case Style.Bold:
                style = "b";
                break;
            case Style.Italic:
                style = "i";
                break;
            case Style.Underline:
                style = "u";
                break;
            case Style.Strikethrough:
                style = "s";
                break;
        }

        return "<" + style + ">" + input + "</" + style + ">";
    }

    string FormatTheme(List<string> input, bool addIntro = false)
    {
        string[] themesToFilterOut = new string[] { "point_of_interest", "establishment" };
        int index = 0;
        while((index +1 )< input.Count)
        {
            if(themesToFilterOut.Contains(input[index]))
            {
                index++;
                continue;
            }
            break;
        }
        string theme = input[index];

        if (!addIntro)
            return theme.Replace('_', ' ');

        string article = vowels.Any(o => o == theme[0]) ? "an" : "a";
        return "is " + article + " " + theme.Replace('_', ' ');
    }

    string Get_I_Sentence(List<Review> reviews)
    {
        if (reviews == null || reviews.Count == 0)
        {
            return TextStyle("ghost noises...",Style.Italic);
        }
        foreach (var item in reviews)
        {
            if (item.text.Contains(" I "))
            {
                return GetExtract(" I ", item.text, new char[] { '!', '.', '?' });
            }
        }
        return "couldnt find anything...";
    }

    string GetExtract(string starter, string input, char[] enders)
    {
        int starter_Index = input.IndexOf(starter);
        int ender_Index = input.Length;

        for (int i = starter_Index; i < input.Length; i++)
        {
            if (enders.Any(o => o == input[i]))
            {
                ender_Index = i + 1;
                break;
            }
        }
        return input.Substring(starter_Index, ender_Index - starter_Index);
    }
}
