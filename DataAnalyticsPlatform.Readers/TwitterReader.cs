/*
 * TwitterReader.cs
 * 
 * This file implements a Twitter data reader using the LinqToTwitter library and AutoMapper
 * for object mapping. It fetches tweets based on provided configuration and transforms them
 * into the application's data model.
 * 
 * Author: Data Analytics Platform Team
 * Last Modified: September 17, 2025
 */

using AutoMapper;
using DataAnalyticsPlatform.Common.Twitter;
using DataAnalyticsPlatform.Shared.Interfaces;
using DataAnalyticsPlatform.Shared.Models;
using LinqToTwitter;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace DataAnalyticsPlatform.Readers
{
    /// <summary>
    /// Utility class to copy properties from a parent object to a child object where names and types match.
    /// </summary>
    /// <typeparam name="TParent">The parent object type</typeparam>
    /// <typeparam name="TChild">The child object type</typeparam>
    public class PropertyCopier<TParent, TChild> 
        where TParent : class
        where TChild : class
    {
        /// <summary>
        /// Copies matching properties from parent object to child object.
        /// </summary>
        /// <param name="parent">The source object</param>
        /// <param name="child">The destination object</param>
        public static void Copy(TParent parent, TChild child)
        {
            var parentProperties = parent.GetType().GetProperties();
            var childProperties = child.GetType().GetProperties();

            foreach (var parentProperty in parentProperties)
            {
                foreach (var childProperty in childProperties)
                {
                    if (parentProperty.Name == childProperty.Name && 
                        parentProperty.PropertyType == childProperty.PropertyType)
                    {
                        childProperty.SetValue(child, parentProperty.GetValue(parent));
                        break;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Configuration class for Twitter API credentials.
    /// </summary>
    public class TwitterApiConfig
    {
        /// <summary>
        /// The Twitter API consumer key
        /// </summary>
        public string ConsumerKey { get; set; }
        
        /// <summary>
        /// The Twitter API consumer secret
        /// </summary>
        public string ConsumerSecret { get; set; }
        
        /// <summary>
        /// The Twitter API access token
        /// </summary>
        public string AccessToken { get; set; }
        
        /// <summary>
        /// The Twitter API access token secret
        /// </summary>
        public string AccessTokenSecret { get; set; }
    }

    /// <summary>
    /// Reads Twitter API configuration from environment variables.
    /// </summary>
    public class AppConfigReader
    {
        /// <summary>
        /// Reads Twitter API configuration from environment variables.
        /// </summary>
        /// <returns>A TwitterApiConfig object with credentials</returns>
        public static TwitterApiConfig ReadTwitterApiConfig()
        {
            var config = new TwitterApiConfig
            {
                ConsumerKey = Environment.GetEnvironmentVariable("TwitterConsumerKey"),
                ConsumerSecret = Environment.GetEnvironmentVariable("TwitterConsumerSecret"),
                AccessToken = Environment.GetEnvironmentVariable("TwitterAccessToken"),
                AccessTokenSecret = Environment.GetEnvironmentVariable("TwitterAccessTokenSecret")
            };

            return config;
        }
    }
    /// <summary>
    /// Reader for Twitter data that fetches tweets based on configuration and maps them to specified types.
    /// </summary>
    public class TwitterReader : BaseReader
    {
        private TwitterContext _twitterContext = null;
        private TwitterConfiguration _twitterConfig;
        private ulong _sinceId;
        private ulong _maxId;
        private List<object> _tweetCollection;
        private int _currentIndex = 0;
        private IMapper _mapper;
        private IMapper _runtimeMapper;
        private List<Type> _types;

        /// <summary>
        /// Initializes a new instance of the TwitterReader class.
        /// </summary>
        /// <param name="conf">Reader configuration</param>
        /// <param name="types">Optional list of types for mapping</param>
        public TwitterReader(ReaderConfiguration conf, List<Type> types = null) : base(conf)
        {
            try
            {
                LogInfo("Initializing TwitterReader");
                
                _types = types;
                _twitterConfig = (TwitterConfiguration)conf.ConfigurationDetails;
                _tweetCollection = new List<object>();
                _sinceId = 1;

                // Initialize Twitter API authentication
                var twitterApiConfig = AppConfigReader.ReadTwitterApiConfig();
                if (string.IsNullOrEmpty(twitterApiConfig.ConsumerKey) || 
                    string.IsNullOrEmpty(twitterApiConfig.ConsumerSecret))
                {
                    LogError("Twitter API credentials are not properly configured in environment variables");
                }
                
                var auth = new SingleUserAuthorizer
                {
                    CredentialStore = new SingleUserInMemoryCredentialStore
                    {
                        ConsumerKey = twitterApiConfig.ConsumerKey,
                        ConsumerSecret = twitterApiConfig.ConsumerSecret,
                        AccessToken = twitterApiConfig.AccessToken,
                        AccessTokenSecret = twitterApiConfig.AccessTokenSecret
                    }
                };
                _twitterContext = new TwitterContext(auth);
                
                // Configure AutoMapper if types are provided
                ConfigureMappers();
            }
            catch (Exception ex)
            {
                LogError($"Error initializing TwitterReader: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Configures AutoMapper instances for type mapping.
        /// </summary>
        private void ConfigureMappers()
        {
            if (_types != null)
            {
                LogInfo("Configuring custom type mapping");
                
                var userType = _types.FirstOrDefault(x => x.FullName.Contains("User"));
                var coordinateType = _types.FirstOrDefault(x => x.FullName.Contains("Coordinate"));
                var metaDataType = _types.FirstOrDefault(x => x.FullName.Contains("MetaData"));
                var originalType = GetConfiguration().ModelType;
                
                var configuration = new MapperConfiguration(cfg =>
                {
                    cfg.CreateMap(typeof(LinqToTwitter.Status), originalType);
                    
                    if (metaDataType != null)
                        cfg.CreateMap(typeof(LinqToTwitter.StatusMetaData), metaDataType);
                    
                    if (coordinateType != null)
                        cfg.CreateMap(typeof(LinqToTwitter.Coordinate), coordinateType);
                    
                    if (userType != null)
                        cfg.CreateMap(typeof(LinqToTwitter.User), userType);
                });
                
                _runtimeMapper = configuration.CreateMapper();
            }
        }

        /// <summary>
        /// Asynchronously fetches tweets from Twitter API based on configured search criteria.
        /// </summary>
        /// <returns>A list of Twitter status objects</returns>
        public async Task<List<LinqToTwitter.Status>> GetTweets()
        {
            try
            {
                LogInfo("Fetching tweets from Twitter API");
                
                // Parse query parameters
                string queryStr = "";
                string geocodeStr = "";
                
                if (_twitterConfig.twitterQuerry.QueryJson.Contains("{"))
                {
                    dynamic queryObj = JObject.Parse(_twitterConfig.twitterQuerry.QueryJson);
                    queryStr = queryObj.query;
                    geocodeStr = queryObj.geocode != null ? queryObj.geocode : "";
                }
                else
                {
                    queryStr = _twitterConfig.twitterQuerry.QueryJson;
                }
                
                // Execute Twitter search query
                List<LinqToTwitter.Status> searchResponse =
                    await
                    (from search in _twitterContext.Search
                     where search.Type == SearchType.Search &&
                           search.Query == queryStr &&
                           search.Count == _twitterConfig.MaxSearchEntriesToReturn &&
                           search.MaxID == _maxId &&
                           search.SearchLanguage == "en" &&
                           search.SinceID == _sinceId &&
                           search.GeoCode == geocodeStr &&
                           search.TweetMode == LinqToTwitter.TweetMode.Extended
                     select search.Statuses)
                    .SingleOrDefaultAsync();
                
                if (searchResponse != null && searchResponse.Any())
                {
                    _maxId = searchResponse.Min(status => status.StatusID) - 1;
                    LogInfo($"Retrieved {searchResponse.Count} tweets");
                }
                else
                {
                    LogInfo("No tweets found matching the search criteria");
                }
                
                return searchResponse;
            }
            catch (Exception ex)
            {
                LogError($"Error fetching tweets: {ex.Message}");
                return null;
            }
        }
        /// <summary>
        /// Gets the next record from the Twitter data.
        /// </summary>
        /// <param name="record">Output parameter to receive the next record</param>
        /// <returns>True if a record was retrieved, false otherwise</returns>
        public override bool GetRecords(out IRecord record)
        {
            try
            {
                // If collection is empty or we've read all items, fetch new tweets
                if (_tweetCollection.Count == 0 || _currentIndex == _tweetCollection.Count)
                {
                    LogInfo("Fetching new batch of tweets");
                    _tweetCollection.Clear();
                    _currentIndex = 0;
                    
                    var task = Task.Run(async () => await GetTweets());
                    var tweets = task.Result;
                    
                    if (tweets != null && tweets.Any() && 
                        (tweets.Count < _twitterConfig.MaxTotalResults || _twitterConfig.MaxTotalResults == -1))
                    {
                        // Map each tweet to the appropriate model
                        foreach (LinqToTwitter.Status status in tweets)
                        {
                            // Default mapping if no model type specified
                            if (GetConfiguration().ModelType == null)
                            {
                                if (_mapper == null)
                                {
                                    LogInfo("Creating default mapper configuration");
                                    var configuration = new MapperConfiguration(cfg =>
                                    {
                                        cfg.CreateMap<LinqToTwitter.Status, OriginalRecord>();
                                        cfg.CreateMap<LinqToTwitter.StatusMetaData, MetaData>();
                                        cfg.CreateMap<LinqToTwitter.Coordinate, Coordinates>();
                                        cfg.CreateMap<LinqToTwitter.User, Common.Twitter.User>();
                                    });
                                    _mapper = configuration.CreateMapper();
                                }
                                
                                var mappedResult = _mapper.Map<OriginalRecord>(status);
                                _tweetCollection.Add(mappedResult);
                            }
                            else
                            {
                                // Use runtime mapper if model type is specified
                                var mappedResult = _runtimeMapper.Map(
                                    status, 
                                    status.GetType(), 
                                    GetConfiguration().ModelType
                                );
                                _tweetCollection.Add(mappedResult);
                            }
                        }
                        
                        LogInfo($"Mapped {_tweetCollection.Count} tweets to model objects");
                    }
                }
                
                // Return the next record if available
                if (_currentIndex < _tweetCollection.Count)
                {
                    record = new SingleRecord(_tweetCollection[_currentIndex]);
                    _currentIndex++;
                    return true;
                }
                else
                {
                    LogInfo("No more records available");
                    record = null;
                    Dispose();
                    return false;
                }
            }
            catch (Exception ex)
            {
                LogError($"Error retrieving records: {ex.Message}");
                record = null;
                Dispose();
                return false;
            }
        }
        /// <summary>
        /// Gets the next record from the Twitter data with a specified type.
        /// </summary>
        /// <param name="record">Output parameter to receive the next record</param>
        /// <param name="type">The type to use for the record</param>
        /// <returns>True if a record was retrieved, false otherwise</returns>
        public override bool GetRecords(out IRecord record, System.Type type)
        {
            LogError("GetRecords with type parameter is not implemented for TwitterReader");
            record = null;
            throw new NotImplementedException("This method is not implemented for TwitterReader");
        }

        /// <summary>
        /// Provides a preview of Twitter data as a DataTable.
        /// </summary>
        /// <param name="size">The number of records to preview</param>
        /// <returns>A DataTable containing the preview data</returns>
        public override DataTable Preview(int size)
        {
            LogError("Preview functionality is not implemented for TwitterReader");
            throw new NotImplementedException("Preview functionality is not implemented for TwitterReader");
        }

        /// <summary>
        /// Releases resources used by the TwitterReader.
        /// </summary>
        private void Dispose()
        {
            LogInfo("Disposing TwitterReader resources");
            
            // Reset collection state
            _tweetCollection?.Clear();
            _currentIndex = 0;
            
            // Note: TwitterContext doesn't have a Dispose method,
            // but we should set it to null to allow garbage collection
            _twitterContext = null;
        }
    }
}
