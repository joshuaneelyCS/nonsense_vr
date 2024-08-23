using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using System;
using UnityEditor.PackageManager.Requests;

public class GraphQLFetcher : MonoBehaviour
{
    private const string graphqlEndpoint = "https://api.wordscenes.com/graphql";
    private const string authToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJleHAiOjE3NTQ2OTM0ODgzNDEsInVzZXJJZCI6IjExZWY0ZGNhNTY4NDUzYTBhYzY1MTkzYzkxNjk1MDNlIiwiaXNTdXBlckFkbWluIjpmYWxzZSwiaXNDb250ZW50QWRtaW4iOmZhbHNlLCJpc1VuaW52aXRlZCI6ZmFsc2UsImlzSW50ZXJuYWxUZXN0ZXIiOnRydWUsImlhdCI6MTcyMzE1NzQ4OH0.PKB9CsG2stGm5QMdQNIix-97U5SC5yqzbEMY5YK1h8M";
    private string _fetchedData;

    
    public IEnumerator FetchData(string queryString)
    {

        // Define the GraphQL query
        string query = queryString;

        // Create the UnityWebRequest
        UnityWebRequest request = new UnityWebRequest(graphqlEndpoint, "POST");

        // Set the request body
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(query);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();

        // Set the content type
        request.SetRequestHeader("Content-Type", "application/json");

        // Add the authorization token
        request.SetRequestHeader("Authorization", authToken);

        // Send the request and wait for the response
        yield return request.SendWebRequest();

        // Handle errors
        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"Request failed: {request.error}");
            Debug.LogError($"Response code: {request.responseCode}");
            Debug.LogError($"Response: {request.downloadHandler.text}");
        }
        else
        {
            Debug.Log($"Connection Established (Response: {request.downloadHandler.text})");

            _fetchedData = request.downloadHandler.text;

        }
    }

    public string returnFetchedData()
    {
        return _fetchedData;
    }
}