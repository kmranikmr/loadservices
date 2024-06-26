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
    public class PropertyCopier<TParent, TChild> where TParent : class
                                          where TChild : class
    {
        public static void Copy(TParent parent, TChild child)
        {
            var parentProperties = parent.GetType().GetProperties();
            var childProperties = child.GetType().GetProperties();

            foreach (var parentProperty in parentProperties)
            {
                foreach (var childProperty in childProperties)
                {
                    if (parentProperty.Name == childProperty.Name && parentProperty.PropertyType == childProperty.PropertyType)
                    {
                        childProperty.SetValue(child, parentProperty.GetValue(parent));
                        break;
                    }
                }
            }
        }
    }
    public class TwitterReader : BaseReader
    {
        public TwitterContext twitterContext = null;
        public TwitterConfiguration twitConf { get; set; }
        public ulong sinceID { get; set; }
        public List<object> twitCollection { get; set; }
        public ulong maxID { get; set; }
        public int ListElement = 0;
        public IMapper mapper;
        public IMapper mapper_runtime;
        public List<Type> Types { get; set; }
        public TwitterReader(ReaderConfiguration conf, List<Type> types = null) : base(conf)
        {
            Types = types;
            twitConf = (TwitterConfiguration)conf.ConfigurationDetails;
            twitCollection = new List<object>();
            sinceID = 1;
            //  var config = new MapperConfiguration(x => { x.CreateMap <GetConfiguration().ModelType>(); })
            var auth = new SingleUserAuthorizer
            {
                CredentialStore = new SingleUserInMemoryCredentialStore
                {
                    //ConsumerKey = "lGKPJML9GIVeEgzv4RbuOhjsh",//((TwitterConfiguration)conf.ConfigurationDetails).twitterAccess.key3,
                    //ConsumerSecret = "LIKdQnpsRbBID9Jof9HgNME7Kmuynwah8vWSWhjGbI3wDNDYRQ",//((TwitterConfiguration)conf.ConfigurationDetails).twitterAccess.key4,
                    //AccessToken = "28496275-zYn9MCSpxa582fgfIPnSo2qwkfAYisRS0GZGycli4",//((TwitterConfiguration)conf.ConfigurationDetails).twitterAccess.key1,
                    //AccessTokenSecret = "BBLuEw2FmwPyaXeJrjvdsHSNyF9FKUxQ0YAixgvI6Tjs1"//((TwitterConfiguration)conf.ConfigurationDetails).twitterAccess.key2
                    ConsumerKey = "i78CO6iTEuLGh8dJbgH6dma1w",//((TwitterConfiguration)conf.ConfigurationDetails).twitterAccess.key3,
                    ConsumerSecret = "sVwVsi1seEShjIURZWNz6Ht5mEUtcszYMGyjnQp5WaJq6ZmLSp",//((TwitterConfiguration)conf.ConfigurationDetails).twitterAccess.key4,
                    AccessToken = "28496275-ibBJKL6muFzfoWqAI4Q87U7qTc4mhBJMENWxH06H3",//((TwitterConfiguration)conf.ConfigurationDetails).twitterAccess.key1,
                    AccessTokenSecret = "I7pg2SJhPzmxxCr8sDEUEVBeOhfhfGOUlMXLSs7F6Gjmn"//((TwitterConfiguration)conf.ConfigurationDetails).twitterAccess.key2

                }
            };
            twitterContext = new TwitterContext(auth);
            if (Types != null)
            {
                var UserType = Types.Where(x => x.FullName.Contains("User")).FirstOrDefault();
                var CordinateType = Types.Where(x => x.FullName.Contains("Coordinate")).FirstOrDefault();
                var MetaDataType = Types.Where(x => x.FullName.Contains("MetaData")).FirstOrDefault();
                var OrginalType = GetConfiguration().ModelType;
                var configuration = new MapperConfiguration(cfg =>
                {
                    cfg.CreateMap(typeof(LinqToTwitter.Status), OrginalType);
                    if (MetaDataType != null)
                        cfg.CreateMap(typeof(LinqToTwitter.StatusMetaData), MetaDataType);
                    if (CordinateType != null)
                        cfg.CreateMap(typeof(LinqToTwitter.Coordinate), CordinateType);
                    if (UserType != null)
                        cfg.CreateMap(typeof(LinqToTwitter.User), UserType);
                });
                mapper_runtime = configuration.CreateMapper();
            }

            //  var configuration = new MapperConfiguration(cfg => {
            //     cfg.CreateMap<LinqToTwitter.Status, OriginalRecord>();
            //cfg.CreateMap<LinqToTwitter.StatusMetaData, MetaData>();
            //cfg.CreateMap<LinqToTwitter.Coordinate, Coordinates>();
            //cfg.CreateMap<LinqToTwitter.User, Common.Twitter.User>();
            //  });
            // mapper = configuration.CreateMapper();

        }


        public async Task<List<LinqToTwitter.Status>> GetTweets()
        {
            // var combinedSearchResults = new List<Status>();
            dynamic queryObj = null;
            var queryStr = "";
            var geocodeStr = "";
            if (twitConf.twitterQuerry.QueryJson.Contains("{") == true)
            {
                queryObj = JObject.Parse(twitConf.twitterQuerry.QueryJson);
                queryStr = queryObj.query;
                geocodeStr = queryObj.geocode != null ? queryObj.geocode : "";
            }
            else
            {
                queryStr = twitConf.twitterQuerry.QueryJson;
            }
            try
            {
                List<LinqToTwitter.Status> searchResponse =
                    await
                    (from search in twitterContext.Search
                     where search.Type == SearchType.Search &&
                           search.Query == queryStr &&
                           search.Count == twitConf.MaxSearchEntriesToReturn &&
                           search.MaxID == maxID &&
                           search.SearchLanguage == "en" &&
                           search.SinceID == sinceID &&
                           search.GeoCode == geocodeStr &&
                           search.TweetMode == LinqToTwitter.TweetMode.Extended
                     select search.Statuses)
                    .SingleOrDefaultAsync();
                maxID = searchResponse.Min(status => status.StatusID) - 1;

                return searchResponse;
            }
            catch (Exception ex)
            {
                return null;
            }

        }
        public override bool GetRecords(out IRecord record)
        {
            bool result = false;

            try
            {
                var combinedSearchResults = new List<LinqToTwitter.Status>();
                if (twitCollection.Count == 0 || ListElement == twitCollection.Count)
                {
                    twitCollection.Clear();
                    ListElement = 0;
                    var task = Task.Run(async () => await GetTweets());

                    if (task.Result.Any() && ((task.Result.Count < twitConf.MaxTotalResults) || (twitConf.MaxTotalResults == -1)))
                    {

                        foreach (LinqToTwitter.Status status in task.Result)
                        {
                            if (GetConfiguration().ModelType == null)
                            {
                                var configuration = new MapperConfiguration(cfg =>
                                {
                                    cfg.CreateMap<LinqToTwitter.Status, OriginalRecord>();
                                    cfg.CreateMap<LinqToTwitter.StatusMetaData, MetaData>();
                                    cfg.CreateMap<LinqToTwitter.Coordinate, Coordinates>();
                                    cfg.CreateMap<LinqToTwitter.User, Common.Twitter.User>();
                                });
                                mapper = configuration.CreateMapper();
                                var result2 = mapper.Map<OriginalRecord>(status);
                                twitCollection.Add(result2);
                            }
                            else
                            {
                                //var UserType = Types.Where(x => x.FullName.Contains("User")).FirstOrDefault();
                                //var CordinateType = Types.Where(x => x.FullName.Contains("Coordinate")).FirstOrDefault();
                                //var MetaDataType = Types.Where(x => x.FullName.Contains("MetaData")).FirstOrDefault();
                                //var t = GetConfiguration().ModelType;
                                //var configuration = new MapperConfiguration(cfg => {
                                //    cfg.CreateMap(typeof(LinqToTwitter.Status),t );

                                //    cfg.CreateMap(typeof(LinqToTwitter.StatusMetaData), MetaDataType);
                                //    cfg.CreateMap(typeof(LinqToTwitter.Coordinate), CordinateType );
                                //    cfg.CreateMap(typeof(LinqToTwitter.User), UserType);
                                //});

                                // dynamic obj = Activator.CreateInstance(GetConfiguration().ModelType);
                                //  PropertyCopier<LinqToTwitter.Status, t>(status, obj);
                                var result2 = mapper_runtime.Map(status, status.GetType(), GetConfiguration().ModelType);
                                twitCollection.Add(result2);
                            }
                        }
                    }
                }
                if (ListElement < twitCollection.Count)
                {
                    // var obj = Activator.CreateInstance(GetConfiguration().ModelType);
                    //    twitCollection[ListElement].Adapt<(obj);
                    record = new SingleRecord(twitCollection[ListElement]);
                    result = true;
                    ListElement++;

                }
                else
                {
                    record = null;
                    Dispose();
                }

            }
            catch (Exception ex)
            {
                record = null;
                Dispose();
                return false;
            }
            return result;
        }
        public override bool GetRecords(out IRecord record, System.Type tt)
        {
            record = null;
            throw new NotImplementedException();
        }

        public override DataTable Preview(int size)
        {
            throw new NotImplementedException();
        }
        private void Dispose()
        {
        }
    }
}
