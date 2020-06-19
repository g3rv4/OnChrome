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

// This shuts down the web server when the user navigates away from the OnChrome local site
// I will convert this to typescript someday
var watching;
function shutdownApp() {
    fetch('http://localhost:12346/shutdown');
    clearInterval(watching);
    watching = undefined;
}

var onChromeTab;
function watchTheUrl() {
    if (watching) {
        return;
    }
    watching = setInterval(() => {
        if (!onChromeTab) {
            return;
        }
        browser.tabs.get(onChromeTab).then(tab => {
            if (!tab.url.startsWith("http://localhost:12346")) {
                shutdownApp();
            }
        }, () => {
            shutdownApp();
        })
    }, 1000);
}

const injectVersion = (tabId, changeInfo, tab) => {
    if (changeInfo.status === "loading" && changeInfo.url && changeInfo.url.startsWith("http://localhost:12346")) {
        browser.tabs.executeScript(tab.id, {
            file: "/inject-version.js",
            runAt: "document_start"
          });
        onChromeTab = tab.id;
        watchTheUrl();
    } else {
        console.log(changeInfo);
    }
}

browser.tabs.onUpdated.addListener(injectVersion, {
    urls: ["*://localhost/*"],
    properties: ["status"]
})

// when the extension is installed, refresh the local OnChrome webserver tabs, so that we see if there are
// any messages there
chrome.runtime.onInstalled.addListener(() => {
    chrome.tabs.query({ url: "*://localhost/*" }, tabs => {
        for (var i = 0; i < tabs.length; i++){
            const tab = tabs[i];
            if (tab.url.startsWith("http://localhost:12346")) {
                chrome.tabs.reload(tab.id);
            }
        }
    });
});

// have the extension check the status of the native app
async function checkStatus()
{
    let success = false;
    try {
        const response = await browser.runtime.sendNativeMessage("me.onchro", {
            command: "compatibility",
            extensionVersion: browser.runtime.getManifest().version,
            url: '?' // passing this as url so that old versions (the golang ones) error out
        });
        success = response.success && response.compatibilityStatus === "Ok"
    } catch {}

    browser.browserAction.setBadgeText({text: success ? "" : "!"});

    if (!success) {
        setTimeout(checkStatus, 10000);
    }
}

checkStatus()
