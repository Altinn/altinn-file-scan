/*
    Test script to platform events api with user token
    Command: docker compose run k6 run /src/tests/end2end.js -e tokenGeneratorUserName=autotest -e tokenGeneratorUserPwd=*** -e env=*** -e subskey=*** -e partyId=*** -e personNumber=*** -e org=ttd -e app=filescan-end-to-end
*/
import { check, sleep } from "k6";
import * as setupToken from "../setup.js";
import { generateJUnitXML, reportPath } from "../report.js";
import * as storageApi from "../api/storage.js";
import { addErrorCount, stopIterationOnFail } from "../errorhandler.js";

const instanceJson = JSON.parse(open("../data/instance.json"));
const kattebilde = open("../data/kattebilde.png");

export const options = {
  thresholds: {
    errors: ['count<1'],
  },
};

export function setup() {
  const partyId = __ENV.partyId;
  const personNumber = __ENV.personNumber;
  const userId = __ENV.userId;
  const username = __ENV.username;
  const password = __ENV.password;
  const environment = __ENV.env;
  const org = __ENV.org.toLowerCase();
  const app = __ENV.app.toLowerCase();

  let userToken;

  if (environment === "prod"){
    userToken = setupToken.getPersonalTokenForProd(username, password);
  }else{
     userToken = setupToken.getPersonalTokenForTest(
      userId,
      partyId,
      personNumber
    );
  }

  var instanceTemplate = instanceJson;
  instanceTemplate.instanceOwner = {
    partyId: partyId,
    personNumber: personNumber,
  };
  instanceTemplate.org = org;
  instanceTemplate.appId = org + "/" + app;

  var data = {
    token: userToken,
    instance: instanceTemplate,
    org: org,
    app: app,
    kattebilde: kattebilde,
  };

  return data;
}

export default function (data) {
  var res, success;

  res = storageApi.postInstance(
    data.token,
    data.org,
    data.app,
    JSON.stringify(data.instance)
  );

  success = check(res, {
    "POST instance status is 201.": (r) =>
      r.status === 201,
  });

  addErrorCount(success);
  stopIterationOnFail("POST instance", success, res);

  const instanceId = res.json('id');

  res = storageApi.postData(data.token, instanceId, "vedlegg", data.kattebilde);
  success = check(res, {
    "POST attachment kattebilde status is 201.": (r) =>
      r.status === 201,
  });

  addErrorCount(success);
  stopIterationOnFail("POST attachment kattebilde", success, res);

  const dataElementId = res.json('id');

  res = storageApi.getInstance(data.token, instanceId);
  success = check(res, {
    "Get instance status is 200.": (r) =>
      r.status === 200,
  });
  addErrorCount(success);
  stopIterationOnFail("Get instance", success, res);

  let retrievedInstance = JSON.parse(res.body);
  let dataElements = retrievedInstance.data.filter(function (d) {
    return d.id == dataElementId;
  });

  let dataElement = dataElements[0];

  success = check(dataElement, {
    "GET check data element. Confirm that scan result is pending or clean.":
      dataElement.fileScanResult == "Pending" || dataElement.fileScanResult == "Clean",
  });

  addErrorCount(success);
  sleep(15);

  res = storageApi.getInstance(data.token, instanceId);
  retrievedInstance = JSON.parse(res.body);
  dataElements = retrievedInstance.data.filter(function (d) {
    return d.id == dataElementId;
  });

  dataElement = dataElements[0];

  success = check(res, {
    "GET check data element. Confirm that scan result is clean.":
      dataElement.fileScanResult === "Clean",
  });
  addErrorCount(success);

  // clean up instance
  res = storageApi.hardDeleteInstance(data.token, instanceId);

  success = check(res, {
    "DELETE hard delete instance after test status is 200.": (r) =>
      r.status === 200,
  });
  addErrorCount(success);
}

/*
export function handleSummary(data) {
  let result = {};
  result[reportPath("filescan.xml")] = generateJUnitXML(data, "platform-filescan");

  return result;
}
*/
