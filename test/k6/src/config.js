// Baseurls for platform
export var baseUrls = {
  at22: "at22.altinn.cloud",
  at23: "at23.altinn.cloud",
  at24: "at24.altinn.cloud",
  yt01: "yt01.altinn.cloud",
  tt02: "tt02.altinn.no",
  prod: "altinn.no"
};

// Auth cookie names in the different environments. NB: Must be updated until changes
// are rolled out to all environments
export var authCookieNames = {
  at22: '.AspxAuthCloud',
  at23: '.AspxAuthCloud',
  at24: '.AspxAuthCloud',
  tt02: '.AspxAuthTT02',
  yt01: '.AspxAuthYt',
  prod: '.AspxAuthProd',
};

//Get values from environment
const environment = __ENV.env.toLowerCase();
export let baseUrl = baseUrls[environment];
export let authCookieName = authCookieNames[environment];

//AltinnTestTools
export var tokenGenerator = {
  getEnterpriseToken:
    "https://altinn-testtools-token-generator.azurewebsites.net/api/GetEnterpriseToken",
  getPersonalToken:
    "https://altinn-testtools-token-generator.azurewebsites.net/api/GetPersonalToken"
};

// Platform Storage
export var platformStorage = {
  instances: "https://platform." + baseUrl + "/storage/api/v1/instances/",
};

// Platform Authentication
export var platformAuthentication={
  authentication: 'https://platform.' + baseUrl + '/authentication/api/v1/authentication',
  refresh: 'https://platform.' + baseUrl + '/authentication/api/v1/refresh',
}

// Altinn App
export var app = {
  ttd: "https://ttd.apps."+baseUrl+"/ttd/",
}

export var sbl = {
  authenticationWithPassword: 'https://' + baseUrl + '/api/authentication/authenticatewithpassword',
}