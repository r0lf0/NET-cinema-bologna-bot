using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CinemaRolfoBot.Model.Json
{
    public class FilmsWithShowings
    {
        public Film[] films { get; set; }
        public string cdate { get; set; }
        public string SiteRootPath { get; set; }
        public string Site { get; set; }
        public string Lang { get; set; }
    }

    public class Film
    {
        public int original_s_count { get; set; }
        public int sortable { get; set; }
        public Showing[] showings { get; set; }
        public bool show_showings { get; set; }
        public string film_page_name { get; set; }
        public string title { get; set; }
        public string id { get; set; }
        public string image_hero { get; set; }
        public string image_poster { get; set; }
        public string cert_image { get; set; }
        public object cert_desc { get; set; }
        public string synopsis_short { get; set; }
        public string info_release { get; set; }
        public bool info_runningtime_visible { get; set; }
        public string info_runningtime { get; set; }
        public string info_age { get; set; }
        public string pegi_class { get; set; }
        public string pegi_href { get; set; }
        public string info_director { get; set; }
        public string info_cast { get; set; }
        public string availablecopy { get; set; }
        public string videolink { get; set; }
        public string filmlink { get; set; }
        public string timeslink { get; set; }
        public string video { get; set; }
        public bool hidden { get; set; }
        public bool coming_soon { get; set; }
        public bool comming_soon { get; set; }
        public bool announcement { get; set; }
        public bool virtual_reality { get; set; }
        public bool young_adult_tag { get; set; }
        public Genres genres { get; set; }
        public Tags tags { get; set; }
        public Categories categories { get; set; }
        public Showing_Type showing_type { get; set; }
        public object rank_votes { get; set; }
        public object rank_value { get; set; }
        public Promo_Labels promo_labels { get; set; }
        public DateTime? ReleaseDate { get; set; }
        public string type { get; set; }
        public string wantsee { get; set; }
        public bool showwantsee { get; set; }
        public string newsletterurl { get; set; }
        public bool always_in_QB { get; set; }
        public int priority_value { get; set; }
        public bool available3D { get; set; }
        public bool selected3D { get; set; }
        public object distributor { get; set; }
        public object[] title_synonyms { get; set; }
        public bool happy { get; set; }
        public string mobile_poster_image { get; set; }
    }

    public class Genres
    {
        public Name[] names { get; set; }
        public bool active { get; set; }
    }

    public class Name
    {
        public string name { get; set; }
        public string url { get; set; }
        public bool highlighted { get; set; }
    }

    public class Tags
    {
        public object[] names { get; set; }
        public bool active { get; set; }
    }

    public class Categories
    {
        public object[] names { get; set; }
        public bool active { get; set; }
    }

    public class Showing_Type
    {
        public string name { get; set; }
        public bool active { get; set; }
    }

    public class Promo_Labels
    {
        public object[] names { get; set; }
        public string position { get; set; }
        public bool isborder { get; set; }
    }

    public class Showing
    {
        public string date_prefix { get; set; }
        public string date_day { get; set; }
        public string date_short { get; set; }
        public string date_long { get; set; }
        public string date_time { get; set; }
        public string date_formatted { get; set; }
        public Time[] times { get; set; }
        public DateTime date { get; set; }
        public DateTime cdate { get; set; }
        public bool clone { get; set; }
        public bool is_peak_pricing { get; set; }
    }

    public class Time
    {
        public string session_id { get; set; }
        public string version_id { get; set; }
        public string time { get; set; }
        public string screen_type { get; set; }
        public string screen_number { get; set; }
        public object lang { get; set; }
        public Tag[] tags { get; set; }
        public object event_info { get; set; }
        public bool hidden { get; set; }
        public DateTime date { get; set; }
        public bool kids_club { get; set; }
        public bool first_class { get; set; }
        public object[] promo_types { get; set; }
        public string happy_price { get; set; }
        public bool peak_pricing { get; set; }
        public string happy_tag { get; set; }
        public string link { get; set; }
        public bool is_limit_reached { get; set; }
    }

    public class Tag
    {
        public string name { get; set; }
        public string fullname { get; set; }
    }
}