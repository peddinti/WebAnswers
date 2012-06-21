using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BingAnswers
{
    public static class QueryAlternation
    {
        public static string Speller(string query)
        {
            SearchResponse response = Util.SendBingSearchRequest(query, "", "spell");
            // return the same query if the speller doesnt have any correction
            if (response == null || response.Spell == null || response.Spell.Total == "0")
                return query;
            return response.Spell.Results.ElementAt(0).Value;
        }
    }
}