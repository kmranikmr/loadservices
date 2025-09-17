using DataAnalyticsPlatform.Common.Helpers;
using DataAnalyticsPlatform.Shared.DataAccess;
using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DataAnalyticsPlatform.Shared;
using Microsoft.CSharp;
using DataAnalyticsPlatform.SharedUtils;

namespace DataAnalyticsPlatform.Common.Builders
{
    public class TwitterBuilder
    {
        private readonly ModelCompiler _modelCompiler;

        public TwitterBuilder(ModelCompiler modelCompiler)
        {
            _modelCompiler = modelCompiler;
        }

        public List<Type> BuildTwitterModel(TypeConfig typeConfig, int jobId = 0)
        {
            try
            {
                string code = File.ReadAllText("TwitterOriginal.cs");
                var types = _modelCompiler.GenerateModelCode(code, "TwitterModel");
                return types != null ? new List<Type> { types } : null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error building Twitter model: {ex.Message}");
                return BuildCustomTwitterModel(typeConfig, jobId);
            }
        }

        private List<Type> BuildCustomTwitterModel(TypeConfig typeConfig, int jobId = 0)
        {
            StringBuilder codeBuilder = new StringBuilder();
            
            // Add namespace and imports
            codeBuilder.AppendLine("using System;");
            codeBuilder.AppendLine("using System.Collections.Generic;");
            codeBuilder.AppendLine("using System.Linq;");
            codeBuilder.AppendLine("namespace DataAnalyticsPlatform.Common {");
            
            // Generate Twitter specific model classes
            codeBuilder.AppendLine("public partial class Tweet {");
            codeBuilder.AppendLine("  public string created_at { get; set; }");
            codeBuilder.AppendLine("  public long id { get; set; }");
            codeBuilder.AppendLine("  public string id_str { get; set; }");
            codeBuilder.AppendLine("  public string text { get; set; }");
            codeBuilder.AppendLine("  public bool truncated { get; set; }");
            codeBuilder.AppendLine("  public Entities entities { get; set; }");
            codeBuilder.AppendLine("  public string source { get; set; }");
            codeBuilder.AppendLine("  public User user { get; set; }");
            codeBuilder.AppendLine("  public bool is_quote_status { get; set; }");
            codeBuilder.AppendLine("  public int retweet_count { get; set; }");
            codeBuilder.AppendLine("  public int favorite_count { get; set; }");
            codeBuilder.AppendLine("  public bool favorited { get; set; }");
            codeBuilder.AppendLine("  public bool retweeted { get; set; }");
            codeBuilder.AppendLine("  public string lang { get; set; }");
            codeBuilder.AppendLine("}");
            
            codeBuilder.AppendLine("public partial class User {");
            codeBuilder.AppendLine("  public long id { get; set; }");
            codeBuilder.AppendLine("  public string id_str { get; set; }");
            codeBuilder.AppendLine("  public string name { get; set; }");
            codeBuilder.AppendLine("  public string screen_name { get; set; }");
            codeBuilder.AppendLine("  public string location { get; set; }");
            codeBuilder.AppendLine("  public string description { get; set; }");
            codeBuilder.AppendLine("  public string url { get; set; }");
            codeBuilder.AppendLine("  public Entities entities { get; set; }");
            codeBuilder.AppendLine("  public bool protected_user { get; set; }");
            codeBuilder.AppendLine("  public int followers_count { get; set; }");
            codeBuilder.AppendLine("  public int friends_count { get; set; }");
            codeBuilder.AppendLine("  public int listed_count { get; set; }");
            codeBuilder.AppendLine("  public string created_at { get; set; }");
            codeBuilder.AppendLine("  public int favourites_count { get; set; }");
            codeBuilder.AppendLine("  public int? utc_offset { get; set; }");
            codeBuilder.AppendLine("  public string time_zone { get; set; }");
            codeBuilder.AppendLine("  public bool geo_enabled { get; set; }");
            codeBuilder.AppendLine("  public bool verified { get; set; }");
            codeBuilder.AppendLine("  public int statuses_count { get; set; }");
            codeBuilder.AppendLine("  public string lang { get; set; }");
            codeBuilder.AppendLine("  public bool contributors_enabled { get; set; }");
            codeBuilder.AppendLine("  public bool is_translator { get; set; }");
            codeBuilder.AppendLine("  public string profile_background_color { get; set; }");
            codeBuilder.AppendLine("  public string profile_background_image_url { get; set; }");
            codeBuilder.AppendLine("  public string profile_background_image_url_https { get; set; }");
            codeBuilder.AppendLine("  public bool profile_background_tile { get; set; }");
            codeBuilder.AppendLine("  public string profile_image_url { get; set; }");
            codeBuilder.AppendLine("  public string profile_image_url_https { get; set; }");
            codeBuilder.AppendLine("  public string profile_banner_url { get; set; }");
            codeBuilder.AppendLine("  public string profile_link_color { get; set; }");
            codeBuilder.AppendLine("  public string profile_sidebar_border_color { get; set; }");
            codeBuilder.AppendLine("  public string profile_sidebar_fill_color { get; set; }");
            codeBuilder.AppendLine("  public string profile_text_color { get; set; }");
            codeBuilder.AppendLine("  public bool profile_use_background_image { get; set; }");
            codeBuilder.AppendLine("  public bool has_extended_profile { get; set; }");
            codeBuilder.AppendLine("  public bool default_profile { get; set; }");
            codeBuilder.AppendLine("  public bool default_profile_image { get; set; }");
            codeBuilder.AppendLine("  public bool? following { get; set; }");
            codeBuilder.AppendLine("  public bool? follow_request_sent { get; set; }");
            codeBuilder.AppendLine("  public bool? notifications { get; set; }");
            codeBuilder.AppendLine("  public string translator_type { get; set; }");
            codeBuilder.AppendLine("}");
            
            codeBuilder.AppendLine("public partial class Entities {");
            codeBuilder.AppendLine("  public Hashtag[] hashtags { get; set; }");
            codeBuilder.AppendLine("  public object[] symbols { get; set; }");
            codeBuilder.AppendLine("  public UserMention[] user_mentions { get; set; }");
            codeBuilder.AppendLine("  public Url[] urls { get; set; }");
            codeBuilder.AppendLine("}");
            
            codeBuilder.AppendLine("public partial class Hashtag {");
            codeBuilder.AppendLine("  public string text { get; set; }");
            codeBuilder.AppendLine("  public int[] indices { get; set; }");
            codeBuilder.AppendLine("}");
            
            codeBuilder.AppendLine("public partial class UserMention {");
            codeBuilder.AppendLine("  public string screen_name { get; set; }");
            codeBuilder.AppendLine("  public string name { get; set; }");
            codeBuilder.AppendLine("  public long id { get; set; }");
            codeBuilder.AppendLine("  public string id_str { get; set; }");
            codeBuilder.AppendLine("  public int[] indices { get; set; }");
            codeBuilder.AppendLine("}");
            
            codeBuilder.AppendLine("public partial class Url {");
            codeBuilder.AppendLine("  public string url { get; set; }");
            codeBuilder.AppendLine("  public string expanded_url { get; set; }");
            codeBuilder.AppendLine("  public string display_url { get; set; }");
            codeBuilder.AppendLine("  public int[] indices { get; set; }");
            codeBuilder.AppendLine("}");
            
            // Generate model mapping classes
            codeBuilder.AppendLine("public partial class TwitterModel : BaseModel {");
            codeBuilder.AppendLine("  public int rowid { get; set; }");
            codeBuilder.AppendLine("  public int fileid { get; set; }");
            codeBuilder.AppendLine("  public long sessionid { get; set; }");
            codeBuilder.AppendLine("  public string created_at { get; set; }");
            codeBuilder.AppendLine("  public long tweet_id { get; set; }");
            codeBuilder.AppendLine("  public string text { get; set; }");
            codeBuilder.AppendLine("  public string source { get; set; }");
            codeBuilder.AppendLine("  public string lang { get; set; }");
            codeBuilder.AppendLine("  public int retweet_count { get; set; }");
            codeBuilder.AppendLine("  public int favorite_count { get; set; }");
            codeBuilder.AppendLine("  public long user_id { get; set; }");
            codeBuilder.AppendLine("  public string user_name { get; set; }");
            codeBuilder.AppendLine("  public string user_screen_name { get; set; }");
            codeBuilder.AppendLine("  public string user_location { get; set; }");
            codeBuilder.AppendLine("  public int user_followers_count { get; set; }");
            codeBuilder.AppendLine("  public int user_friends_count { get; set; }");
            codeBuilder.AppendLine("  public bool user_verified { get; set; }");
            codeBuilder.AppendLine("  public string hashtags { get; set; }");
            codeBuilder.AppendLine("  public string user_mentions { get; set; }");
            codeBuilder.AppendLine("  public string urls { get; set; }");
            codeBuilder.AppendLine("}");
            
            // Add original record class for Twitter
            codeBuilder.AppendLine("public partial class OriginalRecord {");
            codeBuilder.AppendLine("  static int rowId = 1;");
            codeBuilder.AppendLine("  public OriginalRecord() { Init(); }");
            codeBuilder.AppendLine("  partial void Init();");
            codeBuilder.AppendLine("  public Tweet[] tweets { get; set; }");
            codeBuilder.AppendLine("  public int rowid { get; set; }");
            codeBuilder.AppendLine("  public int fileid { get; set; }");
            codeBuilder.AppendLine("  public long sessionid { get; set; }");
            codeBuilder.AppendLine("  public TwitterModel TwitterModel { get; set; }");
            codeBuilder.AppendLine("  public List<BaseModel> models { get; set; }");
            
            // Add MapIt method
            codeBuilder.AppendLine("  public void MapIt() {");
            codeBuilder.AppendLine("    models = new List<BaseModel>();");
            codeBuilder.AppendLine("    if (tweets == null || tweets.Length == 0) return;");
            codeBuilder.AppendLine("    foreach (var tweet in tweets) {");
            codeBuilder.AppendLine("      TwitterModel = new TwitterModel();");
            codeBuilder.AppendLine("      TwitterModel.ModelName = \"TwitterModel\";");
            codeBuilder.AppendLine("      TwitterModel.created_at = tweet.created_at;");
            codeBuilder.AppendLine("      TwitterModel.tweet_id = tweet.id;");
            codeBuilder.AppendLine("      TwitterModel.text = tweet.text;");
            codeBuilder.AppendLine("      TwitterModel.source = tweet.source;");
            codeBuilder.AppendLine("      TwitterModel.lang = tweet.lang;");
            codeBuilder.AppendLine("      TwitterModel.retweet_count = tweet.retweet_count;");
            codeBuilder.AppendLine("      TwitterModel.favorite_count = tweet.favorite_count;");
            codeBuilder.AppendLine("      if (tweet.user != null) {");
            codeBuilder.AppendLine("        TwitterModel.user_id = tweet.user.id;");
            codeBuilder.AppendLine("        TwitterModel.user_name = tweet.user.name;");
            codeBuilder.AppendLine("        TwitterModel.user_screen_name = tweet.user.screen_name;");
            codeBuilder.AppendLine("        TwitterModel.user_location = tweet.user.location;");
            codeBuilder.AppendLine("        TwitterModel.user_followers_count = tweet.user.followers_count;");
            codeBuilder.AppendLine("        TwitterModel.user_friends_count = tweet.user.friends_count;");
            codeBuilder.AppendLine("        TwitterModel.user_verified = tweet.user.verified;");
            codeBuilder.AppendLine("      }");
            codeBuilder.AppendLine("      if (tweet.entities != null) {");
            codeBuilder.AppendLine("        if (tweet.entities.hashtags != null && tweet.entities.hashtags.Length > 0) {");
            codeBuilder.AppendLine("          TwitterModel.hashtags = string.Join(\",\", tweet.entities.hashtags.Select(h => h.text));");
            codeBuilder.AppendLine("        }");
            codeBuilder.AppendLine("        if (tweet.entities.user_mentions != null && tweet.entities.user_mentions.Length > 0) {");
            codeBuilder.AppendLine("          TwitterModel.user_mentions = string.Join(\",\", tweet.entities.user_mentions.Select(u => u.screen_name));");
            codeBuilder.AppendLine("        }");
            codeBuilder.AppendLine("        if (tweet.entities.urls != null && tweet.entities.urls.Length > 0) {");
            codeBuilder.AppendLine("          TwitterModel.urls = string.Join(\",\", tweet.entities.urls.Select(u => u.expanded_url));");
            codeBuilder.AppendLine("        }");
            codeBuilder.AppendLine("      }");
            codeBuilder.AppendLine("      TwitterModel.rowid = rowId++;");
            codeBuilder.AppendLine($"      TwitterModel.sessionid = {jobId};");
            codeBuilder.AppendLine("      models.Add(TwitterModel);");
            codeBuilder.AppendLine("    }");
            codeBuilder.AppendLine("  }");
            
            // Add GetModels method
            codeBuilder.AppendLine("  public List<BaseModel> GetModels(int file_id) {");
            codeBuilder.AppendLine("    fileid = file_id;");
            codeBuilder.AppendLine("    return models;");
            codeBuilder.AppendLine("  }");
            codeBuilder.AppendLine("}");
            
            codeBuilder.AppendLine("}"); // Close namespace
            
            // Compile the code
            RoslynCompiler ros = new RoslynCompiler();
            Type[] types = ros.Generate(
                codeBuilder.ToString(), 
                new[] { "DataAnalyticsPlatform.Common.dll" }, 
                "DataAnalyticsPlatform.ModelGen"
            );
            
            return types?.ToList();
        }
        
        public string GenerateTwitterMapping(TypeConfig typeConfig)
        {
            var ns = new CodeNamespace("DataAnalyticsPlatform.Common");
            ns.Imports.AddRange(new[]
            {
                new CodeNamespaceImport("System"),
                new CodeNamespaceImport("System.IO"),
                new CodeNamespaceImport("System.Collections.Generic"),
                new CodeNamespaceImport("System.Linq"),
                new CodeNamespaceImport("DataAnalyticsPlatform.Common")
            });
            
            var mapItMethod = new CodeMemberMethod
            {
                Name = "MapIt",
                Attributes = MemberAttributes.Public | MemberAttributes.Final
            };
            
            mapItMethod.Statements.Add(new CodeSnippetStatement("models = new List<BaseModel>();"));
            mapItMethod.Statements.Add(new CodeSnippetStatement("if (tweets == null || tweets.Length == 0) return;"));
            mapItMethod.Statements.Add(new CodeSnippetStatement("foreach (var tweet in tweets) {"));
            
            // Create Twitter model
            mapItMethod.Statements.Add(new CodeSnippetStatement("  TwitterModel = new TwitterModel();"));
            mapItMethod.Statements.Add(new CodeSnippetStatement("  TwitterModel.ModelName = \"TwitterModel\";"));
            
            // Map tweet properties
            mapItMethod.Statements.Add(new CodeSnippetStatement("  TwitterModel.created_at = tweet.created_at;"));
            mapItMethod.Statements.Add(new CodeSnippetStatement("  TwitterModel.tweet_id = tweet.id;"));
            mapItMethod.Statements.Add(new CodeSnippetStatement("  TwitterModel.text = tweet.text;"));
            mapItMethod.Statements.Add(new CodeSnippetStatement("  TwitterModel.source = tweet.source;"));
            mapItMethod.Statements.Add(new CodeSnippetStatement("  TwitterModel.lang = tweet.lang;"));
            mapItMethod.Statements.Add(new CodeSnippetStatement("  TwitterModel.retweet_count = tweet.retweet_count;"));
            mapItMethod.Statements.Add(new CodeSnippetStatement("  TwitterModel.favorite_count = tweet.favorite_count;"));
            
            // Map user properties
            mapItMethod.Statements.Add(new CodeSnippetStatement("  if (tweet.user != null) {"));
            mapItMethod.Statements.Add(new CodeSnippetStatement("    TwitterModel.user_id = tweet.user.id;"));
            mapItMethod.Statements.Add(new CodeSnippetStatement("    TwitterModel.user_name = tweet.user.name;"));
            mapItMethod.Statements.Add(new CodeSnippetStatement("    TwitterModel.user_screen_name = tweet.user.screen_name;"));
            mapItMethod.Statements.Add(new CodeSnippetStatement("    TwitterModel.user_location = tweet.user.location;"));
            mapItMethod.Statements.Add(new CodeSnippetStatement("    TwitterModel.user_followers_count = tweet.user.followers_count;"));
            mapItMethod.Statements.Add(new CodeSnippetStatement("    TwitterModel.user_friends_count = tweet.user.friends_count;"));
            mapItMethod.Statements.Add(new CodeSnippetStatement("    TwitterModel.user_verified = tweet.user.verified;"));
            mapItMethod.Statements.Add(new CodeSnippetStatement("  }"));
            
            // Map entities
            mapItMethod.Statements.Add(new CodeSnippetStatement("  if (tweet.entities != null) {"));
            
            // Map hashtags
            mapItMethod.Statements.Add(new CodeSnippetStatement("    if (tweet.entities.hashtags != null && tweet.entities.hashtags.Length > 0) {"));
            mapItMethod.Statements.Add(new CodeSnippetStatement("      TwitterModel.hashtags = string.Join(\",\", tweet.entities.hashtags.Select(h => h.text));"));
            mapItMethod.Statements.Add(new CodeSnippetStatement("    }"));
            
            // Map user mentions
            mapItMethod.Statements.Add(new CodeSnippetStatement("    if (tweet.entities.user_mentions != null && tweet.entities.user_mentions.Length > 0) {"));
            mapItMethod.Statements.Add(new CodeSnippetStatement("      TwitterModel.user_mentions = string.Join(\",\", tweet.entities.user_mentions.Select(u => u.screen_name));"));
            mapItMethod.Statements.Add(new CodeSnippetStatement("    }"));
            
            // Map URLs
            mapItMethod.Statements.Add(new CodeSnippetStatement("    if (tweet.entities.urls != null && tweet.entities.urls.Length > 0) {"));
            mapItMethod.Statements.Add(new CodeSnippetStatement("      TwitterModel.urls = string.Join(\",\", tweet.entities.urls.Select(u => u.expanded_url));"));
            mapItMethod.Statements.Add(new CodeSnippetStatement("    }"));
            mapItMethod.Statements.Add(new CodeSnippetStatement("  }"));
            
            // Set standard fields
            mapItMethod.Statements.Add(new CodeSnippetStatement("  TwitterModel.rowid = rowId++;"));
            mapItMethod.Statements.Add(new CodeSnippetStatement("  models.Add(TwitterModel);"));
            mapItMethod.Statements.Add(new CodeSnippetStatement("}"));
            
            // Generate code
            var provider = new CSharpCodeProvider();
            var sw = new StringWriter();
            provider.GenerateCodeFromMember(mapItMethod, sw, new CodeGeneratorOptions());
            
            return sw.ToString();
        }
        
        public List<Type> CodeTwitter(TypeConfig typeConfig, int jobId = 0)
        {
            try
            {
                string code = File.ReadAllText("TwitterOriginal.cs");
                RoslynCompiler ros = new RoslynCompiler();
                Type[] types = ros.Generate(code, new string[] { "DataAnalyticsPlatform.Common.dll" }, "DataAnalyticsPlatform.ModelGen");
                return types?.ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error generating Twitter model: {ex.Message}");
                return BuildCustomTwitterModel(typeConfig, jobId);
            }
        }
    }
}
