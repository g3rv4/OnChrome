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
            "me.onchro.netcore",
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

// from http://devdoc.net/web/developer.mozilla.org/en-US/docs/Mozilla/Add-ons/WebExtensions/match_patterns.html
const matchPattern = (/^(?:(\*|http|https|file|ftp|app):\/\/(\*|(?:\*\.)?[^\/\*]+|)\/(.*))$/i);

const escapeRegExp = s => s.replace(/[.*+?^${}()|[\]\\]/g, "\\$&");

// An exclusion may be written either as a url match pattern (the same syntax as
// the urls box, e.g. *://*.example.com/*) or as a JavaScript regular expression.
// A url match pattern is converted to an equivalent anchored RegExp; anything
// else is compiled as a RegExp. Blank lines are skipped, and a line that is neither is skipped with a
// console warning instead of throwing -- before this, one invalid line made
// registerUrls() throw before addListener() ran, so NO url opened in Chrome.
function toExclusionRegExp(line) {
    const s = line.trim();
    if (!s) {
        return null;
    }
    const m = matchPattern.exec(s);
    if (m) {
        const scheme = m[1] === "*" ? "https?" : escapeRegExp(m[1]);
        let host = m[2];
        if (host === "*") {
            host = "[^/]*";
        } else if (host.startsWith("*.")) {
            host = "(?:[^/]*\\.)?" + escapeRegExp(host.slice(2));
        } else {
            host = escapeRegExp(host);
        }
        const path = m[3].split("*").map(escapeRegExp).join(".*");
        return new RegExp("^" + scheme + "://" + host + "/" + path + "$", "i");
    }
    try {
        return new RegExp(s);
    } catch (e) {
        console.warn("OnChrome: ignoring invalid exclusion", JSON.stringify(s), e);
        return null;
    }
}

function registerUrls(urls, exclusions, profile) {
    currentProfile = profile;
    if (urls) {
        urls = JSON.parse(urls).map(u => u.trim()).filter(u => matchPattern.exec(u));
        currentExclusions = exclusions ? JSON.parse(exclusions).map(toExclusionRegExp).filter(r => r) : [];
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

// have the extension check the status of the native app
async function checkStatus()
{
    let success = false;
    try {
        const response = await browser.runtime.sendNativeMessage("me.onchro.netcore", {
            command: "compatibility",
            extensionVersion: browser.runtime.getManifest().version
        });
        success = response.success && response.compatibilityStatus === "Ok"
    } catch {}

    browser.browserAction.setBadgeText({text: success ? "" : "!"});

    setTimeout(checkStatus, 10000);
}

checkStatus()
