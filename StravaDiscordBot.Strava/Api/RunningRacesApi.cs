/* 
 * Strava API v3
 *
 * The [Swagger Playground](https://developers.strava.com/playground) is the easiest way to familiarize yourself with the Strava API by submitting HTTP requests and observing the responses before you write any client code. It will show what a response will look like with different endpoints depending on the authorization scope you receive from your athletes. To use the Playground, go to https://www.strava.com/settings/api and change your “Authorization Callback Domain” to developers.strava.com. Please note, we only support Swagger 2.0. There is a known issue where you can only select one scope at a time. For more information, please check the section “client code” at https://developers.strava.com/docs.
 *
 * OpenAPI spec version: 3.0.0
 * 
 * Generated by: https://github.com/swagger-api/swagger-codegen.git
 */

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using RestSharp;
using IO.Swagger.Client;
using IO.Swagger.Model;

namespace IO.Swagger.Api
{
    /// <summary>
    /// Represents a collection of functions to interact with the API endpoints
    /// </summary>
    public interface IRunningRacesApi : IApiAccessor
    {
        #region Synchronous Operations
        /// <summary>
        /// Get Running Race
        /// </summary>
        /// <remarks>
        /// Returns a running race for a given identifier.
        /// </remarks>
        /// <exception cref="IO.Swagger.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="id">The identifier of the running race.</param>
        /// <returns>RunningRace</returns>
        RunningRace GetRunningRaceById (long? id);

        /// <summary>
        /// Get Running Race
        /// </summary>
        /// <remarks>
        /// Returns a running race for a given identifier.
        /// </remarks>
        /// <exception cref="IO.Swagger.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="id">The identifier of the running race.</param>
        /// <returns>ApiResponse of RunningRace</returns>
        ApiResponse<RunningRace> GetRunningRaceByIdWithHttpInfo (long? id);
        /// <summary>
        /// List Running Races
        /// </summary>
        /// <remarks>
        /// Returns a list running races based on a set of search criteria.
        /// </remarks>
        /// <exception cref="IO.Swagger.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="year">Filters the list by a given year. (optional)</param>
        /// <returns>List&lt;RunningRace&gt;</returns>
        List<RunningRace> GetRunningRaces (int? year = null);

        /// <summary>
        /// List Running Races
        /// </summary>
        /// <remarks>
        /// Returns a list running races based on a set of search criteria.
        /// </remarks>
        /// <exception cref="IO.Swagger.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="year">Filters the list by a given year. (optional)</param>
        /// <returns>ApiResponse of List&lt;RunningRace&gt;</returns>
        ApiResponse<List<RunningRace>> GetRunningRacesWithHttpInfo (int? year = null);
        #endregion Synchronous Operations
        #region Asynchronous Operations
        /// <summary>
        /// Get Running Race
        /// </summary>
        /// <remarks>
        /// Returns a running race for a given identifier.
        /// </remarks>
        /// <exception cref="IO.Swagger.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="id">The identifier of the running race.</param>
        /// <returns>Task of RunningRace</returns>
        System.Threading.Tasks.Task<RunningRace> GetRunningRaceByIdAsync (long? id);

        /// <summary>
        /// Get Running Race
        /// </summary>
        /// <remarks>
        /// Returns a running race for a given identifier.
        /// </remarks>
        /// <exception cref="IO.Swagger.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="id">The identifier of the running race.</param>
        /// <returns>Task of ApiResponse (RunningRace)</returns>
        System.Threading.Tasks.Task<ApiResponse<RunningRace>> GetRunningRaceByIdAsyncWithHttpInfo (long? id);
        /// <summary>
        /// List Running Races
        /// </summary>
        /// <remarks>
        /// Returns a list running races based on a set of search criteria.
        /// </remarks>
        /// <exception cref="IO.Swagger.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="year">Filters the list by a given year. (optional)</param>
        /// <returns>Task of List&lt;RunningRace&gt;</returns>
        System.Threading.Tasks.Task<List<RunningRace>> GetRunningRacesAsync (int? year = null);

        /// <summary>
        /// List Running Races
        /// </summary>
        /// <remarks>
        /// Returns a list running races based on a set of search criteria.
        /// </remarks>
        /// <exception cref="IO.Swagger.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="year">Filters the list by a given year. (optional)</param>
        /// <returns>Task of ApiResponse (List&lt;RunningRace&gt;)</returns>
        System.Threading.Tasks.Task<ApiResponse<List<RunningRace>>> GetRunningRacesAsyncWithHttpInfo (int? year = null);
        #endregion Asynchronous Operations
    }

    /// <summary>
    /// Represents a collection of functions to interact with the API endpoints
    /// </summary>
    public partial class RunningRacesApi : IRunningRacesApi
    {
        private IO.Swagger.Client.ExceptionFactory _exceptionFactory = (name, response) => null;

        /// <summary>
        /// Initializes a new instance of the <see cref="RunningRacesApi"/> class.
        /// </summary>
        /// <returns></returns>
        public RunningRacesApi(String basePath)
        {
            this.Configuration = new IO.Swagger.Client.Configuration { BasePath = basePath };

            ExceptionFactory = IO.Swagger.Client.Configuration.DefaultExceptionFactory;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RunningRacesApi"/> class
        /// using Configuration object
        /// </summary>
        /// <param name="configuration">An instance of Configuration</param>
        /// <returns></returns>
        public RunningRacesApi(IO.Swagger.Client.Configuration configuration = null)
        {
            if (configuration == null) // use the default one in Configuration
                this.Configuration = IO.Swagger.Client.Configuration.Default;
            else
                this.Configuration = configuration;

            ExceptionFactory = IO.Swagger.Client.Configuration.DefaultExceptionFactory;
        }

        /// <summary>
        /// Gets the base path of the API client.
        /// </summary>
        /// <value>The base path</value>
        public String GetBasePath()
        {
            return this.Configuration.ApiClient.RestClient.BaseUrl.ToString();
        }

        /// <summary>
        /// Sets the base path of the API client.
        /// </summary>
        /// <value>The base path</value>
        [Obsolete("SetBasePath is deprecated, please do 'Configuration.ApiClient = new ApiClient(\"http://new-path\")' instead.")]
        public void SetBasePath(String basePath)
        {
            // do nothing
        }

        /// <summary>
        /// Gets or sets the configuration object
        /// </summary>
        /// <value>An instance of the Configuration</value>
        public IO.Swagger.Client.Configuration Configuration {get; set;}

        /// <summary>
        /// Provides a factory method hook for the creation of exceptions.
        /// </summary>
        public IO.Swagger.Client.ExceptionFactory ExceptionFactory
        {
            get
            {
                if (_exceptionFactory != null && _exceptionFactory.GetInvocationList().Length > 1)
                {
                    throw new InvalidOperationException("Multicast delegate for ExceptionFactory is unsupported.");
                }
                return _exceptionFactory;
            }
            set { _exceptionFactory = value; }
        }

        /// <summary>
        /// Gets the default header.
        /// </summary>
        /// <returns>Dictionary of HTTP header</returns>
        [Obsolete("DefaultHeader is deprecated, please use Configuration.DefaultHeader instead.")]
        public IDictionary<String, String> DefaultHeader()
        {
            return new ReadOnlyDictionary<string, string>(this.Configuration.DefaultHeader);
        }

        /// <summary>
        /// Add default header.
        /// </summary>
        /// <param name="key">Header field name.</param>
        /// <param name="value">Header field value.</param>
        /// <returns></returns>
        [Obsolete("AddDefaultHeader is deprecated, please use Configuration.AddDefaultHeader instead.")]
        public void AddDefaultHeader(string key, string value)
        {
            this.Configuration.AddDefaultHeader(key, value);
        }

        /// <summary>
        /// Get Running Race Returns a running race for a given identifier.
        /// </summary>
        /// <exception cref="IO.Swagger.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="id">The identifier of the running race.</param>
        /// <returns>RunningRace</returns>
        public RunningRace GetRunningRaceById (long? id)
        {
             ApiResponse<RunningRace> localVarResponse = GetRunningRaceByIdWithHttpInfo(id);
             return localVarResponse.Data;
        }

        /// <summary>
        /// Get Running Race Returns a running race for a given identifier.
        /// </summary>
        /// <exception cref="IO.Swagger.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="id">The identifier of the running race.</param>
        /// <returns>ApiResponse of RunningRace</returns>
        public ApiResponse< RunningRace > GetRunningRaceByIdWithHttpInfo (long? id)
        {
            // verify the required parameter 'id' is set
            if (id == null)
                throw new ApiException(400, "Missing required parameter 'id' when calling RunningRacesApi->GetRunningRaceById");

            var localVarPath = "/running_races/{id}";
            var localVarPathParams = new Dictionary<String, String>();
            var localVarQueryParams = new List<KeyValuePair<String, String>>();
            var localVarHeaderParams = new Dictionary<String, String>(this.Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<String, String>();
            var localVarFileParams = new Dictionary<String, FileParameter>();
            Object localVarPostBody = null;

            // to determine the Content-Type header
            String[] localVarHttpContentTypes = new String[] {
            };
            String localVarHttpContentType = this.Configuration.ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            String[] localVarHttpHeaderAccepts = new String[] {
                "application/json"
            };
            String localVarHttpHeaderAccept = this.Configuration.ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            if (id != null) localVarPathParams.Add("id", this.Configuration.ApiClient.ParameterToString(id)); // path parameter

            // authentication (strava_oauth) required
            // oauth required
            if (!String.IsNullOrEmpty(this.Configuration.AccessToken))
            {
                localVarHeaderParams["Authorization"] = "Bearer " + this.Configuration.AccessToken;
            }

            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse) this.Configuration.ApiClient.CallApi(localVarPath,
                Method.GET, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int) localVarResponse.StatusCode;

            if (ExceptionFactory != null)
            {
                Exception exception = ExceptionFactory("GetRunningRaceById", localVarResponse);
                if (exception != null) throw exception;
            }

            return new ApiResponse<RunningRace>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                (RunningRace) this.Configuration.ApiClient.Deserialize(localVarResponse, typeof(RunningRace)));
        }

        /// <summary>
        /// Get Running Race Returns a running race for a given identifier.
        /// </summary>
        /// <exception cref="IO.Swagger.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="id">The identifier of the running race.</param>
        /// <returns>Task of RunningRace</returns>
        public async System.Threading.Tasks.Task<RunningRace> GetRunningRaceByIdAsync (long? id)
        {
             ApiResponse<RunningRace> localVarResponse = await GetRunningRaceByIdAsyncWithHttpInfo(id);
             return localVarResponse.Data;

        }

        /// <summary>
        /// Get Running Race Returns a running race for a given identifier.
        /// </summary>
        /// <exception cref="IO.Swagger.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="id">The identifier of the running race.</param>
        /// <returns>Task of ApiResponse (RunningRace)</returns>
        public async System.Threading.Tasks.Task<ApiResponse<RunningRace>> GetRunningRaceByIdAsyncWithHttpInfo (long? id)
        {
            // verify the required parameter 'id' is set
            if (id == null)
                throw new ApiException(400, "Missing required parameter 'id' when calling RunningRacesApi->GetRunningRaceById");

            var localVarPath = "/running_races/{id}";
            var localVarPathParams = new Dictionary<String, String>();
            var localVarQueryParams = new List<KeyValuePair<String, String>>();
            var localVarHeaderParams = new Dictionary<String, String>(this.Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<String, String>();
            var localVarFileParams = new Dictionary<String, FileParameter>();
            Object localVarPostBody = null;

            // to determine the Content-Type header
            String[] localVarHttpContentTypes = new String[] {
            };
            String localVarHttpContentType = this.Configuration.ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            String[] localVarHttpHeaderAccepts = new String[] {
                "application/json"
            };
            String localVarHttpHeaderAccept = this.Configuration.ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            if (id != null) localVarPathParams.Add("id", this.Configuration.ApiClient.ParameterToString(id)); // path parameter

            // authentication (strava_oauth) required
            // oauth required
            if (!String.IsNullOrEmpty(this.Configuration.AccessToken))
            {
                localVarHeaderParams["Authorization"] = "Bearer " + this.Configuration.AccessToken;
            }

            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse) await this.Configuration.ApiClient.CallApiAsync(localVarPath,
                Method.GET, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int) localVarResponse.StatusCode;

            if (ExceptionFactory != null)
            {
                Exception exception = ExceptionFactory("GetRunningRaceById", localVarResponse);
                if (exception != null) throw exception;
            }

            return new ApiResponse<RunningRace>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                (RunningRace) this.Configuration.ApiClient.Deserialize(localVarResponse, typeof(RunningRace)));
        }

        /// <summary>
        /// List Running Races Returns a list running races based on a set of search criteria.
        /// </summary>
        /// <exception cref="IO.Swagger.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="year">Filters the list by a given year. (optional)</param>
        /// <returns>List&lt;RunningRace&gt;</returns>
        public List<RunningRace> GetRunningRaces (int? year = null)
        {
             ApiResponse<List<RunningRace>> localVarResponse = GetRunningRacesWithHttpInfo(year);
             return localVarResponse.Data;
        }

        /// <summary>
        /// List Running Races Returns a list running races based on a set of search criteria.
        /// </summary>
        /// <exception cref="IO.Swagger.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="year">Filters the list by a given year. (optional)</param>
        /// <returns>ApiResponse of List&lt;RunningRace&gt;</returns>
        public ApiResponse< List<RunningRace> > GetRunningRacesWithHttpInfo (int? year = null)
        {

            var localVarPath = "/running_races";
            var localVarPathParams = new Dictionary<String, String>();
            var localVarQueryParams = new List<KeyValuePair<String, String>>();
            var localVarHeaderParams = new Dictionary<String, String>(this.Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<String, String>();
            var localVarFileParams = new Dictionary<String, FileParameter>();
            Object localVarPostBody = null;

            // to determine the Content-Type header
            String[] localVarHttpContentTypes = new String[] {
            };
            String localVarHttpContentType = this.Configuration.ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            String[] localVarHttpHeaderAccepts = new String[] {
                "application/json"
            };
            String localVarHttpHeaderAccept = this.Configuration.ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            if (year != null) localVarQueryParams.AddRange(this.Configuration.ApiClient.ParameterToKeyValuePairs("", "year", year)); // query parameter

            // authentication (strava_oauth) required
            // oauth required
            if (!String.IsNullOrEmpty(this.Configuration.AccessToken))
            {
                localVarHeaderParams["Authorization"] = "Bearer " + this.Configuration.AccessToken;
            }

            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse) this.Configuration.ApiClient.CallApi(localVarPath,
                Method.GET, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int) localVarResponse.StatusCode;

            if (ExceptionFactory != null)
            {
                Exception exception = ExceptionFactory("GetRunningRaces", localVarResponse);
                if (exception != null) throw exception;
            }

            return new ApiResponse<List<RunningRace>>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                (List<RunningRace>) this.Configuration.ApiClient.Deserialize(localVarResponse, typeof(List<RunningRace>)));
        }

        /// <summary>
        /// List Running Races Returns a list running races based on a set of search criteria.
        /// </summary>
        /// <exception cref="IO.Swagger.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="year">Filters the list by a given year. (optional)</param>
        /// <returns>Task of List&lt;RunningRace&gt;</returns>
        public async System.Threading.Tasks.Task<List<RunningRace>> GetRunningRacesAsync (int? year = null)
        {
             ApiResponse<List<RunningRace>> localVarResponse = await GetRunningRacesAsyncWithHttpInfo(year);
             return localVarResponse.Data;

        }

        /// <summary>
        /// List Running Races Returns a list running races based on a set of search criteria.
        /// </summary>
        /// <exception cref="IO.Swagger.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="year">Filters the list by a given year. (optional)</param>
        /// <returns>Task of ApiResponse (List&lt;RunningRace&gt;)</returns>
        public async System.Threading.Tasks.Task<ApiResponse<List<RunningRace>>> GetRunningRacesAsyncWithHttpInfo (int? year = null)
        {

            var localVarPath = "/running_races";
            var localVarPathParams = new Dictionary<String, String>();
            var localVarQueryParams = new List<KeyValuePair<String, String>>();
            var localVarHeaderParams = new Dictionary<String, String>(this.Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<String, String>();
            var localVarFileParams = new Dictionary<String, FileParameter>();
            Object localVarPostBody = null;

            // to determine the Content-Type header
            String[] localVarHttpContentTypes = new String[] {
            };
            String localVarHttpContentType = this.Configuration.ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            String[] localVarHttpHeaderAccepts = new String[] {
                "application/json"
            };
            String localVarHttpHeaderAccept = this.Configuration.ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            if (year != null) localVarQueryParams.AddRange(this.Configuration.ApiClient.ParameterToKeyValuePairs("", "year", year)); // query parameter

            // authentication (strava_oauth) required
            // oauth required
            if (!String.IsNullOrEmpty(this.Configuration.AccessToken))
            {
                localVarHeaderParams["Authorization"] = "Bearer " + this.Configuration.AccessToken;
            }

            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse) await this.Configuration.ApiClient.CallApiAsync(localVarPath,
                Method.GET, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int) localVarResponse.StatusCode;

            if (ExceptionFactory != null)
            {
                Exception exception = ExceptionFactory("GetRunningRaces", localVarResponse);
                if (exception != null) throw exception;
            }

            return new ApiResponse<List<RunningRace>>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                (List<RunningRace>) this.Configuration.ApiClient.Deserialize(localVarResponse, typeof(List<RunningRace>)));
        }

    }
}
