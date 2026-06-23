import http from "k6/http";
import { check } from "k6";
import encoding from "k6/encoding";

import * as config from "../config.js";
import { addErrorCount, stopIterationOnFail } from "../errorhandler.js";

const tokenGeneratorUserName = __ENV.tokenGeneratorUserName;
const tokenGeneratorUserPwd = __ENV.tokenGeneratorUserPwd;

/*
Generate enterprise token for test environment
*/
export function generateEnterpriseToken(queryParams) {
  const credentials = `${tokenGeneratorUserName}:${tokenGeneratorUserPwd}`;
  const encodedCredentials = encoding.b64encode(credentials);

  var endpoint =
    config.tokenGenerator.getEnterpriseToken +
    buildQueryParametersForEndpoint(queryParams);

  var params = {
    headers: {
      Authorization: `Basic ${encodedCredentials}`,
    },
  };

  var response = http.get(endpoint, params);

  if (response.status != 200) {
    stopIterationOnFail("Enterprise token generation failed", false, response);
  }

  var token = response.body;
  return token;
}


/*
Generate personal token for test environment
*/
export function generatePersonalToken(queryParams) {
  const credentials = `${tokenGeneratorUserName}:${tokenGeneratorUserPwd}`;
  const encodedCredentials = encoding.b64encode(credentials);

  var endpoint =
    config.tokenGenerator.getPersonalToken +
    buildQueryParametersForEndpoint(queryParams);

  var params = {
    headers: {
      Authorization: `Basic ${encodedCredentials}`,
    },
  };

  var response = http.get(endpoint, params);

  if (response.status != 200) {
    stopIterationOnFail("Personal token generation failed", false, response);
  }

  var token = response.body;
  return token;
}



/*
Logs in an end user via Mockporten (test IDP); returns the runtime token.
pid must be a synthetic Tenor fødselsnummer (month 81-92). Never log res.url.
*/
export function authenticateWithMockporten() {
  http.cookieJar().clear(config.platformAuthentication.refresh);
  var endpoint = config.platformAuthentication.authentication
    + "?goto=" + config.platformAuthentication.refresh
    + "&iss=mockporten";
  var res = http.get(endpoint);
  var success = check(res, { "Mockporten login form loaded": (r) => r.status === 200 });
  addErrorCount(success);
  stopIterationOnFail("Mockporten login form not loaded", success, res);

  res = res.submitForm({ fields: { Pid: __ENV.pid, Password: __ENV.testidppwd } });
  success = check(res, { "Mockporten authentication success": (r) => r.status === 200 });
  addErrorCount(success);
  stopIterationOnFail("Mockporten authentication failed", success, res);
  return res.body;
}


/*
Build query parameters
*/
function buildQueryParametersForEndpoint(filterParameters) {
  var query = "?";
  Object.keys(filterParameters).forEach(function (key) {
    if (Array.isArray(filterParameters[key])) {
      filterParameters[key].forEach((value) => {
        query += key + "=" + value + "&";
      });
    } else {
      query += key + "=" + filterParameters[key] + "&";
    }
  });
  query = query.slice(0, -1);

  return query;
}
