import http from "k6/http";
import * as config from "../config.js";

/**
 * Api call to Storage:Instances to create an app instance and returns response
 * @param {*} altinnStudioRuntimeCookie token value to be sent in header for authentication
 * @param {*} partyId party id of the user to whom instance is to be created
 * @param {*} appOwner app owner name
 * @param {*} appName app name
 * @param {JSON} instanceJson instance json metadata sent in request body
 * @returns {JSON} Json object including response headers, body, timings
 */

const subskey = __ENV.subskey.toLowerCase();

export function postInstance(token, org, app, instanceJson) {
  var appId = org + "/" + app;
  var endpoint = config.platformStorage["instances"] + "?appId=" + appId;

  var params = {
    headers: {
      Authorization: `Bearer ${token}`,
      "Content-Type": "application/json",
      "Ocp-Apim-Subscription-Key": subskey,
    },
  };

  return http.post(endpoint, instanceJson, params);
}

export function hardDeleteInstance(token, instanceId) {
  var endpoint =
    config.platformStorage["instances"] + instanceId + "?hard=true";

    var params = {
      headers: {
        Authorization: `Bearer ${token}`,
        "Ocp-Apim-Subscription-Key": subskey,
      },
    };


  return http.del(endpoint, undefined, params);
}

export function getInstance(token, instanceId) {
  var endpoint = config.platformStorage.instances + instanceId;

  var params = {
    headers: {
      Authorization: `Bearer ${token}`,
    },
  };

  return http.get(endpoint, params);
}


export function postData(token, instanceId, dataType, content){
  var endpoint = config.platformStorage.instances + instanceId + "/data?dataType=" + dataType;

  var params = {
    headers: {
      Authorization: `Bearer ${token}`,
      "Content-Type": "application/octet-stream",
      "Content-Disposition": "attachment; filename=kattebilde.png",
      "Ocp-Apim-Subscription-Key": subskey,
    },
  };

  return http.post(endpoint, content, params);

}