using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Xml;

namespace RecipeScraper.Scrapers
{
    class TroisFoisParJourScraper : BaseScraper
    {
        //Not handling content attribute in BaseScraper
        public TroisFoisParJourScraper(string url) : base(url) 
        {
            
        }

        public override string GetYield()
        {
            return GetSingleItemPropElement("recipeYield").GetAttribute("content");
        }

        public override TimeSpan GetPrepTime()
        {
            return XmlConvert.ToTimeSpan(GetSingleItemPropElement("prepTime").GetAttribute("content"));
        }

        public override TimeSpan GetCookTime()
        {
            return XmlConvert.ToTimeSpan(GetSingleItemPropElement("cookTime").GetAttribute("content"));
        }
    }
}
