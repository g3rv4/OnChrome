function onError(error) {
    // we couldn't send the native message! maybe it's not installed?
    const encodedError = encodeURIComponent(error);
    browser.tabs.create({
        url: browser.runtime.getURL("error.html?error=" + encodedError)
    });
}

var currentProfile;
const blockUrl = function (exclusions) {
    return function(requestDetails){
        return new Promise(function(resolve, reject) {
            var resolved = false;
            for(exclusion of exclusions){
                if(requestDetails.url.match(new RegExp(exclusion))) {
                    resolve({cancel:false});
                    resolved = true;
                }
            }
            if (!resolved){
                browser.runtime.sendNativeMessage(
                    "me.onchro",
                    { url: requestDetails.url, profile: currentProfile }).then(() => browser.tabs.remove(requestDetails.tabId), onError);
                resolve({cancel: true});
            }
        });
    }
}

function registerUrls(urls, exclusions, profile) {
    currentProfile = profile;
    if (urls) {
        urls = JSON.parse(urls);
        exclusions = JSON.parse(exclusions);
        if (urls && urls.length) {
            browser.webRequest.onBeforeRequest.addListener(blockUrl(exclusions), { urls: urls, types: ["main_frame"] }, ["blocking"])
        }
    }
}

chrome.storage.sync.get(["urls", "exclusions"], res => registerUrls(res.urls, res.exclusions))

browser.runtime.onMessage.addListener((request, sender, sendResponse) => {
    if (request.type === "urlsUpdated") {
        browser.webRequest.onBeforeRequest.removeListener(blockUrl)
        chrome.storage.sync.get(["urls", "profile", exclusions], res => registerUrls(res.urls, res.exclusions, res.profile))
    }
});
