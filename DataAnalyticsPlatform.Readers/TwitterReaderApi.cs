using AutoMapper;
using DataAnalyticsPlatform.Shared.Interfaces;
using DataAnalyticsPlatform.Shared.Models;
using LinqToTwitter;

using System;
using System.Collections.Generic;
using System.Data;

namespace DataAnalyticsPlatform.Readers
{
    public class TwitterReaderApi : BaseReader
    {
        public TwitterContext twitterContext = null;
        public TwitterConfiguration twitConf { get; set; }
        public ulong sinceID { get; set; }
        public List<object> twitCollection { get; set; }
        public ulong maxID { get; set; }
        public int ListElement = 0;
        public IMapper mapper;
        public TwitterReaderApi(ReaderConfiguration conf) : base(conf)
        {
            twitConf = (TwitterConfiguration)conf.ConfigurationDetails;
            twitCollection = new List<object>();
            sinceID = 1;

            List body = new List();

            var auth = new SingleUserAuthorizer
            {
                CredentialStore = new SingleUserInMemoryCredentialStore
                {
                    ConsumerKey = ((TwitterConfiguration)conf.ConfigurationDetails).twitterAccess.key3,
                    ConsumerSecret = ((TwitterConfiguration)conf.ConfigurationDetails).twitterAccess.key4,
                    AccessToken = ((TwitterConfiguration)conf.ConfigurationDetails).twitterAccess.key1,
                    AccessTokenSecret = ((TwitterConfiguration)conf.ConfigurationDetails).twitterAccess.key2
                }
            };
            twitterContext = new TwitterContext(auth);



        }

        public override bool GetRecords(out IRecord record)
        {
            bool result = false;
            record = null;
            return result;
        }

        public override bool GetRecords(out IRecord record, Type t)
        {
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
