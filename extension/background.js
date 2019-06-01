function onError(error) {
    // we couldn't send the native message! maybe it's not installed?
    const encodedError = encodeURIComponent(error);
    browser.tabs.create({
        url: browser.runtime.getURL("error.html?error=" + encodedError)
    });
}

let availableToRun = true;
const listener = function (tabId, changeInfo, tab) {
    if (!availableToRun) {
        return;
    }
    availableToRun = false;

    const url = tab.url;
    browser.runtime.sendNativeMessage(
        "me.onchro",
        { url: url }).then(() => browser.tabs.remove(tabId), onError);

    setTimeout(function () {
        availableToRun = true;
    }, 500);
};

function registerUrls(urls) {
    if (urls) {
        urls = JSON.parse(urls);
        if (urls && urls.length) {
            browser.tabs.onUpdated.addListener(listener, { urls });
        }
    }
}

chrome.storage.sync.get(["urls"], res => registerUrls(res.urls))

browser.runtime.onMessage.addListener((request, sender, sendResponse) => {
    if (request.type === "urlsUpdated") {
        browser.tabs.onUpdated.removeListener(listener);
        chrome.storage.sync.get(["urls"], res => registerUrls(res.urls))
    }
});
