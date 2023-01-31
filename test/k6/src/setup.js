import http from "k6/http";
import { check } from "k6";
import * as tokenGenerator from "./api/token-generator.js";
import * as config from "../config.js";
import { stopIterationOnFail } from "../errorhandler.js";

const environment = __ENV.env.toLowerCase();

/*
 * generate an altinn token for TTD based on the environment using AltinnTestTools
 * @returns altinn token with the provided scopes for an org/appowner
 */
export function getAltinnTokenForTTD(scopes) {
  var queryParams = {
    env: environment,
    scopes: scopes,
    org: "ttd",
    orgNo: "991825827",
  };

  return tokenGenerator.generateEnterpriseToken(queryParams);
}

export function getPersonalTokenForTest(userId, partyid, pid) {
  var queryParams = {
    env: environment,
    scopes: "altinn:enduser",
    userId: userId,
    partyId: partyid,
    pid: pid,
  };

  return tokenGenerator.generatePersonalToken(queryParams);
}

export function getPersonalTokenForProd(username, password) {
  let aspxauth =  getAspxAuth(username, password);
  return getAltinnStudioRuntimeToken(aspxauth);
}

function getAspxAuth(username, password) {
  var endpoint = config.sbl.authenticationWithPassword;

  var requestBody = {
    UserName: username,
    UserPassword: password,
  };

  var params = {
    headers: {
      Accept: "application/hal+json",
    },
  };

  var res = http.post(endpoint, requestBody, params);
  var success = check(res, {
    "Authentication towards Altinn 2 Success": (r) => r.status === 200,
  });

  stopIterationOnFail("Authentication towards Altinn 2 Failed", success, res);

  const cookieName = ".ASPXAUTH";
  var cookieValue = res.cookies[cookieName][0].value;
  return cookieValue;
}

export function getAltinnStudioRuntimeToken(aspxauthCookie) {
  clearCookies();
  var endpoint = config.platformAuthentication.authentication + '?goto=' + config.platformAuthentication.refresh;

  var params = {
    cookies: { '.ASPXAUTH': aspxauthCookie },
  };

  var res = http.get(endpoint, params);
  var success = check(res, {
    'T3.0 Authentication Success': (r) => r.status === 200,
  });
  stopIterationOnFail('T3.0 Authentication Failed', success, res);
  return res.body;
}

//Function to clear the cookies under baseurl by setting the expires field to a past date
export function clearCookies() {
  var jar = http.cookieJar();
  jar.set('https://' + config.baseUrl, 'AltinnStudioRuntime', 'test', { expires: 'Mon, 02 Jan 2010 15:04:05 MST' });
  jar.set('https://' + config.baseUrl, '.ASPXAUTH', 'test', { expires: 'Mon, 02 Jan 2010 15:04:05 MST' });
}