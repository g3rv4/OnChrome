function onError(error) {
    // we couldn't send the native message! maybe it's not installed?
    const encodedError = encodeURIComponent(error);
    browser.tabs.create({
        url: browser.runtime.getURL("error.html?error=" + encodedError)
    });
}

var currentProfile, currentExclusions;
const blockUrl = function (requestDetails) {
    return new Promise(function (resolve, reject) {
        if (currentExclusions.find(r => requestDetails.url.match(r))) {
            resolve({ cancel: false });
            return
        }
        browser.runtime.sendNativeMessage(
            "me.onchro",
            { command: "open", url: requestDetails.url, profile: currentProfile })
            .then(r => {
                if (r.success) {
                    browser.tabs.remove(requestDetails.tabId)
                } else {
                    onError(r.errorMessage);
                }
            }, onError);
        resolve({ cancel: true });
    });
}

function registerUrls(urls, exclusions, profile) {
    currentProfile = profile;
    if (urls) {
        urls = JSON.parse(urls);
        currentExclusions = exclusions ? JSON.parse(exclusions).map(s => new RegExp(s)) : [];
        if (urls && urls.length) {
            browser.webRequest.onBeforeRequest.addListener(blockUrl, { urls: urls, types: ["main_frame"] }, ["blocking"])
        }
    }
}

chrome.storage.sync.get(["urls", "profile", "exclusions"], res => registerUrls(res.urls, res.exclusions, res.profile))

browser.runtime.onMessage.addListener((request, sender, sendResponse) => {
    if (request.type === "urlsUpdated") {
        browser.webRequest.onBeforeRequest.removeListener(blockUrl)
        chrome.storage.sync.get(["urls", "profile", "exclusions"], res => registerUrls(res.urls, res.exclusions, res.profile))
    }
});
