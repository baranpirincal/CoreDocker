﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using CoreDocker.Api.AppStartup;
using CoreDocker.Api.Security;
using CoreDocker.Utilities.Helpers;
using GraphQL;
using GraphQL.Authorization;
using GraphQL.Server.Transports.AspNetCore;
using GraphQL.Server.Ui.Playground;
using GraphQL.Types;
using GraphQL.Validation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CoreDocker.Api.GraphQl
{
    public static class GraphQlSetup
    {
        public static void AddGraphQl(this IServiceCollection services)
        {
            services.AddSingleton<IDependencyResolver>(s => new FuncDependencyResolver(s.GetRequiredService));
            services.AddGraphQLHttp();
        }

        public static void AddGraphQl(this IApplicationBuilder app)
        {
            var openIdSettings = IocApi.Instance.Resolve<OpenIdSettings>();
            var uriCombine = new Uri(openIdSettings.HostUrl.UriCombine("/graphql"));

            var settings = new GraphQLHttpOptions
            {
                Path = uriCombine.PathAndQuery,
                ExposeExceptions = true,
                BuildUserContext = ctx =>
                {
                    var userContext = new GraphQLUserContext
                    {
                        User = ctx.User
                    };
                  

                    
                    return Task.FromResult(userContext);
                }
            };
            var rules = app.ApplicationServices.GetServices<IValidationRule>();
            rules.ForEach(x => settings.ValidationRules.Add(x));
            
            app.UseGraphQLHttp<ISchema>(settings);
          
            app.UseGraphQLPlayground(new GraphQLPlaygroundOptions(){GraphQLEndPoint = uriCombine.PathAndQuery});
        }

        
       

        #region Nested type: GraphQLUserContext

        public class GraphQLUserContext : IProvideClaimsPrincipal
        {
            public ClaimsPrincipal User { get; set; }
        }

        #endregion
    }
}