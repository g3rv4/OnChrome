function onError(error) {
    // we couldn't send the native message! maybe it's not installed?
    const encodedError = encodeURIComponent(error);
    browser.tabs.create({
        url: browser.runtime.getURL("error.html?error=" + encodedError)
    });
}

var currentProfile;
const blockUrl = function (requestDetails) {
    return new Promise(function(resolve, reject) {
        browser.runtime.sendNativeMessage(
            "me.onchro",
            { url: requestDetails.url, profile: currentProfile }).then(() => browser.tabs.remove(requestDetails.tabId), onError);
        resolve({cancel: true});
      });
}

function registerUrls(urls, profile) {
    currentProfile = profile;
    if (urls) {
        urls = JSON.parse(urls);
        if (urls && urls.length) {
            browser.webRequest.onBeforeRequest.addListener(blockUrl, { urls: urls, types: ["main_frame"] }, ["blocking"])
        }
    }
}

chrome.storage.sync.get(["urls"], res => registerUrls(res.urls))

browser.runtime.onMessage.addListener((request, sender, sendResponse) => {
    if (request.type === "urlsUpdated") {
        browser.webRequest.onBeforeRequest.removeListener(blockUrl)
        chrome.storage.sync.get(["urls", "profile"], res => registerUrls(res.urls, res.profile))
    }
});
