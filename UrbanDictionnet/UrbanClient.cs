﻿using System;
using System.Threading.Tasks;
using RestSharp;
using System.Collections.Generic;

namespace UrbanDictionnet
{
    /// <summary>
    /// The main UrbanClient to instanciate in order to use the functions.
    /// </summary>
    /// <remarks>
    /// It has a funny name because in this library everything is urban.
    /// </remarks>
    public class UrbanClient
    {
        /// <summary>
        /// Get a definiton using a string as query.
        /// </summary>
        /// <param name="query">The query to request, will be escaped later</param>
        /// <returns>When awaited, a <see cref="WordDefine"/>.</returns>
        /// <exception cref="WordNotFoundException">
        /// When the returned <see cref="WordDefine.ResultType"/> is <see cref="ResultType.NoResults"/>
        /// </exception>
        public async Task<WordDefine> GetWordAsync(string query)
        {
            var escapedQuery = Uri.EscapeDataString(query);
            var result = await Rest.ExecuteAsync<WordDefine>(new RestRequest
            {
                Resource = $"define?term={escapedQuery}",
            }).ConfigureAwait(false);
            if (result.ResultType == ResultType.NoResults)
            {
                throw new WordNotFoundException($"The word {query} wasn't found.");
            }
            return result;
        }
        /// <summary>
        /// Get a definiton using a definition id (defid) as query.
        /// </summary>
        /// <param name="defId">The definiton id to request.</param>
        /// <returns>When awaited, an <see cref="UniqueWordDefine"/></returns>
        /// <exception cref="WordNotFoundException">
        /// When the returned <see cref="UniqueWordDefine.ResultType"/> is <see cref="ResultType.NoResults"/>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// When <paramref name="defId"/> is negative.
        /// </exception>
        public async Task<UniqueWordDefine> GetWordAsync(int defId)
        {
            CheckDefinitionId(defId);
            var result = await Rest.ExecuteAsync<UniqueWordDefine>(new RestRequest
            {
                Resource = $"define?defid={defId}",
            }).ConfigureAwait(false);
            if (result.ResultType == ResultType.NoResults)
            {
                throw new WordNotFoundException($"The definition with the id {defId} wasn't found.");
            }
            return result;
        }
        /// <summary>
        /// Check the definiton id if it is lower or equal than 0
        /// </summary>
        /// <param name="defId">The id to check</param>
        /// <exception cref="ArgumentException">When the <paramref name="defId"/> is lower or equal to 0</exception>
        private static void CheckDefinitionId(int defId)
        {
            if (defId <= 0)
            {
                throw new ArgumentException("The definition id is equal or lower than 0.",nameof(defId));
            }
        }
        /// <summary>
        /// Gets 10 random words from the API.
        /// </summary>
        /// <returns>An array of ten <see cref="WordDefine"/></returns>
        public async Task<List<DefinitionData>> GetRandomWordsAsync()
        {
            return (await Rest.ExecuteAsync<SingleList<DefinitionData>>(new RestRequest("random")).ConfigureAwait(false)).List;
        }
        /// <summary>
        /// Get a random word from the API. Takes the first from <see cref="GetRandomWordsAsync"/>
        /// </summary>
        /// <returns>When awaited, a <see cref="WordDefine"/>.</returns>
        public async Task<DefinitionData> GetRandomWordAsync()
        {
            return (await GetRandomWordsAsync())[0];
        }

        public async Task<List<string>> GetAutocompletionFor(string query)
        {
            var escapedQuery = Uri.EscapeDataString(query);
            return (await Rest.ExecuteAsync<List<string>>(new RestRequest($"autocomplete?term={escapedQuery}")));
        }

        /// <summary>
        /// Vote a definiton to be up or down.
        /// </summary>
        /// <param name="defId">The definiton id of the... definiton.</param>
        /// <param name="direction">The direction, either Up or Down</param>
        /// <returns>When awaited, returns a <see cref="VoteResponse"/>.</returns>
        /// <exception cref="VoteException">
        /// When the <see cref="VoteResponse.Status"/> is <see cref="VoteStatus.Error"/>.
        /// </exception>     
        public async Task<VoteResponse> VoteOnDefinition(int defId,VoteDirection direction)
        {
            CheckDefinitionId(defId);
            var lowered = direction.ToString().ToLower();
            var result = await Rest.ExecuteAsync<VoteResponse>(new RestRequest($"vote?defid={defId}&direction={lowered}")).ConfigureAwait(false);
            if (result.Status == VoteStatus.Error)
            {
                throw new VoteException($"An error occured while sending the vote request, the definiton id ({defId}) is probably wrong.");
            }
            return result;
        }
    }
}
