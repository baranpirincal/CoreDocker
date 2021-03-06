using System;
using System.Linq;
using System.Threading.Tasks;
using CoreDocker.Sdk.RestApi.Clients;
using CoreDocker.Shared.Models;
using CoreDocker.Shared.Models.Auth;
using CoreDocker.Utilities.Helpers;
using GraphQL.Client;
using GraphQL.Common.Request;
using GraphQL.Common.Response;
using RestSharp;

namespace CoreDocker.Sdk.RestApi
{
    public class CoreDockerClient : ICoreDockerClient
    {
        internal RestClient _restClient;
        private readonly GraphQLClient _graphQlClient;

        public CoreDockerClient(string urlBase)
        {
            UrlBase = urlBase;
            _restClient = new RestClient(UrlBase);
            Authenticate = new AuthenticateApiClient(this);
            Projects = new ProjectApiClient(this);
            Users = new UserApiClient(this);
            Ping = new PingApiClient(this);
            _graphQlClient = new GraphQLClient(UrlBase.UriCombine("/graphql"));
        }

        public RestClient Client => _restClient;
        
        public string UrlBase { get; }

        #region Implementation of ICoreDockerApi

        public void SetToken(TokenResponseModel data)
        {
            var bearerToken = $"Bearer {data.AccessToken}";
            _restClient.DefaultParameters.Add(new Parameter() { Type = ParameterType.HttpHeader, Name = "Authorization", Value = bearerToken });
            _graphQlClient.DefaultRequestHeaders.Add("Authorization",new []{ bearerToken });
        }

        public AuthenticateApiClient Authenticate { get; set; }
        public PingApiClient Ping { get; set; }
       

        public ProjectApiClient Projects { get; set; }
        public UserApiClient Users { get; set; }

        #endregion
        
        public async Task<GraphQLResponse> GraphQlPost(GraphQLRequest heroRequest)
        {
            var graphQlResponse = await _graphQlClient.PostAsync(heroRequest);
            if (graphQlResponse.Errors != null && graphQlResponse.Errors.Any())
            {
                throw new GraphQlResponseException(graphQlResponse) ;
            }
            return graphQlResponse;
        }
    }

    public class GraphQlResponseException : Exception
    {
        public GraphQLResponse GraphQlResponse { get; }

        public GraphQlResponseException(GraphQLResponse graphQlResponse) : base(graphQlResponse.Errors.Select(x=>x.Message).StringJoin())
        {
            GraphQlResponse = graphQlResponse;
        }
    }
}