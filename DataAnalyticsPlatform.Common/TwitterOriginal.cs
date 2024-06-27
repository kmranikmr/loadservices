
using DataAnalyticsPlatform.Shared.DataAccess;
using System;
using System.Collections.Generic;

namespace DataAnalyticsPlatform.Common.Twitter
{
    //public partial class Type
    //{
    //}

    //public partial class OEmbedAlign
    //{
    //}

    //public partial class CreatedAt
    //{
    //}

    //public partial class Type
    //{
    //}

    //public partial class OEmbedAlign
    //{
    //}

    //public partial class CreatedAt
    //{
    //}

    //public partial class TweetMode
    //{
    //}

    //public partial class FilterLevel
    //{
    //}

    public partial class ExtendedTweet
    {
        // public Type Type { get; set; }
        public long? ID { get; set; }
        public long? UserID { get; set; }
        public long? SinceID { get; set; }
        public long? MaxID { get; set; }
        public int? Count { get; set; }
        public long? Cursor { get; set; }
        public bool? IncludeRetweets { get; set; }
        public bool? ExcludeReplies { get; set; }
        public bool? IncludeEntities { get; set; }
        public bool? IncludeUserEntities { get; set; }
        public bool? IncludeMyRetweet { get; set; }
        public bool? IncludeAltText { get; set; }
        public int? OEmbedMaxWidth { get; set; }
        public bool? OEmbedHideMedia { get; set; }
        public bool? OEmbedHideThread { get; set; }
        public bool? OEmbedOmitScript { get; set; }
        //    public OEmbedAlign OEmbedAlign { get; set; }
        //   public CreatedAt CreatedAt { get; set; }
        public long? StatusID { get; set; }
        public bool? Truncated { get; set; }
        //   public TweetMode TweetMode { get; set; }
        public long? InReplyToStatusID { get; set; }
        public long? InReplyToUserID { get; set; }
        public bool? Favorited { get; set; }
        public bool? TrimUser { get; set; }
        public bool? IncludeContributorDetails { get; set; }
        public int? RetweetCount { get; set; }
        public bool? Retweeted { get; set; }
        public bool? PossiblySensitive { get; set; }
        public long? CurrentUserRetweet { get; set; }
        public bool? IsQuotedStatus { get; set; }
        public long? QuotedStatusID { get; set; }
        public bool? WithheldCopyright { get; set; }
        public bool? Map { get; set; }
        public FilterLevel FilterLevel { get; set; }
    }

    //public partial class DisplayTextRange
    //{
    //}

    //public partial class TweetMode
    //{
    //}

    //public partial class Type
    //{
    //}

    public partial class ImageSize
    {
    }

    public partial class CursorMovement
    {
        public long? Next { get; set; }
        public long? Previous { get; set; }
    }

    //public partial class CreatedAt
    //{
    //}

    //public partial class Type
    //{
    //}

    //public partial class OEmbedAlign
    //{
    //}

    //public partial class CreatedAt
    //{
    //}

    //public partial class TweetMode
    //{
    //}

    //public partial class FilterLevel
    //{
    //}

    public partial class Status
    {
        // public Type Type { get; set; }
        public long? ID { get; set; }
        public long? UserID { get; set; }
        public long? SinceID { get; set; }
        public long? MaxID { get; set; }
        public int? Count { get; set; }
        public long? Cursor { get; set; }
        public bool? IncludeRetweets { get; set; }
        public bool? ExcludeReplies { get; set; }
        public bool? IncludeEntities { get; set; }
        public bool? IncludeUserEntities { get; set; }
        public bool? IncludeMyRetweet { get; set; }
        public bool? IncludeAltText { get; set; }
        public int? OEmbedMaxWidth { get; set; }
        public bool? OEmbedHideMedia { get; set; }
        public bool? OEmbedHideThread { get; set; }
        public bool? OEmbedOmitScript { get; set; }
        // public OEmbedAlign OEmbedAlign { get; set; }
        // public CreatedAt CreatedAt { get; set; }
        public long? StatusID { get; set; }
        public bool? Truncated { get; set; }
        //   public TweetMode TweetMode { get; set; }
        public long? InReplyToStatusID { get; set; }
        public long? InReplyToUserID { get; set; }
        public bool? Favorited { get; set; }
        public bool? TrimUser { get; set; }
        public bool? IncludeContributorDetails { get; set; }
        public int? RetweetCount { get; set; }
        public bool? Retweeted { get; set; }
        public bool? PossiblySensitive { get; set; }
        public long? CurrentUserRetweet { get; set; }
        public bool? IsQuotedStatus { get; set; }
        public long? QuotedStatusID { get; set; }
        public bool? WithheldCopyright { get; set; }
        public bool? Map { get; set; }
        public FilterLevel FilterLevel { get; set; }
    }

    //public partial class Categories
    //{
    //}

    //public partial class BannerSizes
    //{
    //}

    public partial class User
    {
        //   public Type Type { get; set; }
        public long? UserID { get; set; }
        public int? Page { get; set; }
        public int? Count { get; set; }
        public long? Cursor { get; set; }
        public bool? IncludeEntities { get; set; }
        public bool? SkipStatus { get; set; }
        public string UserIDResponse { get; set; }
        public string ScreenNameResponse { get; set; }
        // public ImageSize ImageSize { get; set; }
        //  public CursorMovement CursorMovement { get; set; }
        public string Name { get; set; }
        public string Location { get; set; }
        public string Description { get; set; }
        public string ProfileImageUrl { get; set; }
        public string ProfileImageUrlHttps { get; set; }
        public bool? DefaultProfileImage { get; set; }
        public bool? DefaultProfile { get; set; }
        public bool? Protected { get; set; }
        public int? FollowersCount { get; set; }
        public string ProfileBackgroundColor { get; set; }
        public string ProfileTextColor { get; set; }
        public string ProfileLinkColor { get; set; }
        public string ProfileSidebarFillColor { get; set; }
        public string ProfileSidebarBorderColor { get; set; }
        public int? FriendsCount { get; set; }
        //  public CreatedAt CreatedAt { get; set; }
        public int? FavoritesCount { get; set; }
        public int? UtcOffset { get; set; }
        public string ProfileBackgroundImageUrl { get; set; }
        public string ProfileBackgroundImageUrlHttps { get; set; }
        public bool? ProfileBackgroundTile { get; set; }
        public bool? ProfileUseBackgroundImage { get; set; }
        public int? StatusesCount { get; set; }
        public bool? Notifications { get; set; }
        public bool? GeoEnabled { get; set; }
        public bool? Verified { get; set; }
        public bool? ContributorsEnabled { get; set; }
        public bool? IsTranslator { get; set; }
        public bool? Following { get; set; }
        //    public Status Status { get; set; }
        // public Categories Categories { get; set; }
        public bool? ShowAllInlineMedia { get; set; }
        public int? ListedCount { get; set; }
        public bool? FollowRequestSent { get; set; }
        //  public BannerSizes BannerSizes { get; set; }
    }

    public partial class Users
    {
    }

    //public partial class Contributors
    //{
    //}

    public partial class Coordinates
    {
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public bool? IsLocationAvailable { get; set; }
    }

    //public partial class Place
    //{
    //}

    public partial class Attributes
    {
    }

    public partial class Elements
    {
    }

    public partial class Annotation
    {
        public Attributes Attributes { get; set; }
        public Elements Elements { get; set; }
    }

    public partial class UserMentionEntities
    {
    }

    public partial class UrlEntities
    {
    }

    public partial class HashTagEntities
    {
    }

    public partial class MediaEntities
    {
    }

    public partial class SymbolEntities
    {
    }

    public partial class Entities
    {
        public UserMentionEntities UserMentionEntities { get; set; }
        public UrlEntities UrlEntities { get; set; }
        public HashTagEntities HashTagEntities { get; set; }
        public MediaEntities MediaEntities { get; set; }
        public SymbolEntities SymbolEntities { get; set; }
    }

    //public partial class UserMentionEntities
    //{
    //}

    public partial class UrlEntities
    {
    }

    public partial class HashTagEntities
    {
    }

    public partial class MediaEntities
    {
    }

    public partial class SymbolEntities
    {
    }

    public partial class ExtendedEntities
    {
        public UserMentionEntities UserMentionEntities { get; set; }
        public UrlEntities UrlEntities { get; set; }
        public HashTagEntities HashTagEntities { get; set; }
        public MediaEntities MediaEntities { get; set; }
        public SymbolEntities SymbolEntities { get; set; }
    }

    //public partial class Type
    //{
    //}

    //public partial class OEmbedAlign
    //{
    //}

    //public partial class CreatedAt
    //{
    //}

    //public partial class TweetMode
    //{
    //}

    //public partial class FilterLevel
    //{
    //}

    public partial class RetweetedStatus
    {
        // public Type Type { get; set; }
        public long? ID { get; set; }
        public long? UserID { get; set; }
        public long? SinceID { get; set; }
        public long? MaxID { get; set; }
        public int? Count { get; set; }
        public long? Cursor { get; set; }
        public bool? IncludeRetweets { get; set; }
        public bool? ExcludeReplies { get; set; }
        public bool? IncludeEntities { get; set; }
        public bool? IncludeUserEntities { get; set; }
        public bool? IncludeMyRetweet { get; set; }
        public bool? IncludeAltText { get; set; }
        public int? OEmbedMaxWidth { get; set; }
        public bool? OEmbedHideMedia { get; set; }
        public bool? OEmbedHideThread { get; set; }
        public bool? OEmbedOmitScript { get; set; }
        // public OEmbedAlign OEmbedAlign { get; set; }
        // public CreatedAt CreatedAt { get; set; }
        public long? StatusID { get; set; }
        public bool? Truncated { get; set; }
        //  public TweetMode TweetMode { get; set; }
        public long? InReplyToStatusID { get; set; }
        public long? InReplyToUserID { get; set; }
        public bool? Favorited { get; set; }
        public bool? TrimUser { get; set; }
        public bool? IncludeContributorDetails { get; set; }
        public int? RetweetCount { get; set; }
        public bool? Retweeted { get; set; }
        public bool? PossiblySensitive { get; set; }
        public long? CurrentUserRetweet { get; set; }
        public bool? IsQuotedStatus { get; set; }
        public long? QuotedStatusID { get; set; }
        public bool? WithheldCopyright { get; set; }
        public bool? Map { get; set; }
        // public FilterLevel FilterLevel { get; set; }
    }

    //public partial class Type
    //{
    //}

    //public partial class OEmbedAlign
    //{
    //}

    //public partial class CreatedAt
    //{
    //}

    //public partial class TweetMode
    //{
    //}

    //public partial class FilterLevel
    //{
    //}

    public partial class QuotedStatus
    {
        // public Type Type { get; set; }
        public long? ID { get; set; }
        public long? UserID { get; set; }
        public long? SinceID { get; set; }
        public long? MaxID { get; set; }
        public int? Count { get; set; }
        public long? Cursor { get; set; }
        public bool? IncludeRetweets { get; set; }
        public bool? ExcludeReplies { get; set; }
        public bool? IncludeEntities { get; set; }
        public bool? IncludeUserEntities { get; set; }
        public bool? IncludeMyRetweet { get; set; }
        public bool? IncludeAltText { get; set; }
        public int? OEmbedMaxWidth { get; set; }
        public bool? OEmbedHideMedia { get; set; }
        public bool? OEmbedHideThread { get; set; }
        public bool? OEmbedOmitScript { get; set; }
        // public OEmbedAlign OEmbedAlign { get; set; }
        //  public CreatedAt CreatedAt { get; set; }
        public long? StatusID { get; set; }
        public bool? Truncated { get; set; }
        //  public TweetMode TweetMode { get; set; }
        public long? InReplyToStatusID { get; set; }
        public long? InReplyToUserID { get; set; }
        public bool? Favorited { get; set; }
        public bool? TrimUser { get; set; }
        public bool? IncludeContributorDetails { get; set; }
        public int? RetweetCount { get; set; }
        public bool? Retweeted { get; set; }
        public bool? PossiblySensitive { get; set; }
        public long? CurrentUserRetweet { get; set; }
        public bool? IsQuotedStatus { get; set; }
        public long? QuotedStatusID { get; set; }
        public bool? WithheldCopyright { get; set; }
        public bool? Map { get; set; }
        public FilterLevel FilterLevel { get; set; }
    }

    public partial class Scopes
    {
    }

    public partial class WithheldInCountries
    {
    }

    public partial class MetaData
    {
        public string ResultType { get; set; }
        public string IsoLanguageCode { get; set; }
    }

    public partial class FilterLevel
    {
    }

    public partial class OriginalRecord
    {
        static int rows = 1;
        public OriginalRecord() { Init(); rowid = rows++; }
        partial void Init();
        public int rowid { get; set; }
        public int fileid { get; set; }
        // public Type Type { get; set; }
        public long? ID { get; set; }
        public long? UserID { get; set; }
        public long? SinceID { get; set; }
        public long? MaxID { get; set; }
        public int? Count { get; set; }
        public long? Cursor { get; set; }
        public bool? IncludeRetweets { get; set; }
        public bool? ExcludeReplies { get; set; }
        public bool? IncludeEntities { get; set; }
        public bool? IncludeUserEntities { get; set; }
        public bool? IncludeMyRetweet { get; set; }
        public bool? IncludeAltText { get; set; }
        public int? OEmbedMaxWidth { get; set; }
        public bool? OEmbedHideMedia { get; set; }
        public bool? OEmbedHideThread { get; set; }
        public bool? OEmbedOmitScript { get; set; }
        //    public OEmbedAlign OEmbedAlign { get; set; }
        public DateTime CreatedAt { get; set; }
        public long? StatusID { get; set; }
        public string FullText { get; set; }
        // public Status ExtendedTweet { get; set; }
        public string Source { get; set; }
        public bool? Truncated { get; set; }
        //    public DisplayTextRange DisplayTextRange { get; set; }
        //     public TweetMode TweetMode { get; set; }
        public long? InReplyToStatusID { get; set; }
        public long? InReplyToUserID { get; set; }
        public int? FavoriteCount { get; set; }
        public bool? Favorited { get; set; }
        public string InReplyToScreenName { get; set; }
        //   public User User { get; set; }-------
        //  public Users Users { get; set; }
        // public Contributors Contributors { get; set; }
        ///  public Coordinates Coordinates { get; set; }---
        // public Place Place { get; set; }
        //   public Annotation Annotation { get; set; }
        // public Entities Entities { get; set; }
        //public ExtendedEntities ExtendedEntities { get; set; }
        public bool? TrimUser { get; set; }
        public bool? IncludeContributorDetails { get; set; }
        public int? RetweetCount { get; set; }
        public bool? Retweeted { get; set; }
        public bool? PossiblySensitive { get; set; }
        //   public RetweetedStatus RetweetedStatus { get; set; }
        public long? CurrentUserRetweet { get; set; }
        public bool? IsQuotedStatus { get; set; }
        public long? QuotedStatusID { get; set; }
        //  public QuotedStatus QuotedStatus { get; set; }
        //  public Scopes Scopes { get; set; }
        public bool? WithheldCopyright { get; set; }
        //   public WithheldInCountries WithheldInCountries { get; set; }
        // public MetaData MetaData { get; set; }---
        public string Lang { get; set; }
        public bool? Map { get; set; }
        //    public FilterLevel FilterLevel { get; set; }
    }
}
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace DataAnalyticsPlatform.Common.Twitter
{


    public partial class OriginalRecord
    {

        public List<BaseModel> models;

        partial void Init()
        {
            models = new List<BaseModel>();
        }

        public void MapIt()
        {
        }

        public System.Collections.Generic.List<BaseModel> GetModels(int file_id)
        {
            fileid = file_id;
            return models;
        }
    }
}

