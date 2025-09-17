using DataAnalyticsPlatform.Shared;
using DataAnalyticsPlatform.Shared.DataAccess;
using DataAnalyticsPlatform.SharedUtils;
using DataAnalyticsPlatform.Common.Helpers;
using DataAnalyticsPlatform.Common.Builders;
using System;
using System.Collections.Generic;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace DataAnalyticsPlatform.Common
{
    /// <summary>
    /// Main entrypoint for dynamic model code generation.
    /// Orchestrates various builder classes.
    /// </summary>
    public class TransformationCodeGenerator
    {
        private readonly CodeGenHelper _helpers;
        private readonly ModelCompiler _compiler;
        private readonly TypeConfigBuilder _typeConfigBuilder;
        private readonly JsonBuilder _jsonBuilder;
        private readonly TwitterBuilder _twitterBuilder;
        private readonly PartialBuilder _partialBuilder;

        public TransformationCodeGenerator()
        {
            _helpers = new CodeGenHelper();
            _compiler = new ModelCompiler();
            _typeConfigBuilder = new TypeConfigBuilder(_helpers, _compiler);
            _jsonBuilder = new JsonBuilder(_helpers, _compiler);
            _twitterBuilder = new TwitterBuilder(_compiler);
            _partialBuilder = new PartialBuilder();
        }

        public List<Type> Code(TypeConfig typeConfig, int jobid = 0, string filename = "")
            => _typeConfigBuilder.Code(typeConfig, jobid, filename);

        public List<Type> CodeJSON(TypeConfig typeConfig, int jobid = 0)
            => _jsonBuilder.CodeJSON(typeConfig, jobid);

        public List<Type> CodeTwitter(TypeConfig typeConfig, int jobid = 0)
            => _twitterBuilder.CodeTwitter(typeConfig, jobid);

        public string AddModelPartials(ref System.CodeDom.CodeTypeDeclaration myclass,
                                       TypeConfig typeConfig,
                                       bool hasRowid = false,
                                       int jobId = 0)
            => _partialBuilder.AddModelPartials(ref myclass, typeConfig, hasRowid, jobId);
    }
}
