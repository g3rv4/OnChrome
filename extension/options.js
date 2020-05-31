const save = document.getElementById("save");
save.addEventListener("click", () => {
    let urls = document.getElementById("urls").value;
    let exclusions = document.getElementById("exclusions").value;
    let profile = document.getElementById("profile").value;
    let urlMatchPatternsToSave = [];

    if (urls) {
        urls = urls.split(/\r?\n/);
        if (urls.length) {
            urlMatchPatternsToSave = urls;
        }
    }

    if (exclusions) {
        exclusions = exclusions.split(/\r?\n/);
        if (exclusions.length) {
            exclusionsToSave = exclusions;
        }
    } else {
        exclusionsToSave = [];
    }
    browser.storage.sync.set({
        urls: JSON.stringify(urlMatchPatternsToSave),
        profile: profile,
        exclusions: JSON.stringify(exclusionsToSave)
    }, () => {
        chrome.runtime.sendMessage({ type: "urlsUpdated" });
        window.close();
    });
})

chrome.storage.sync.get(["urls","profile","exclusions"], res => {
    var urls = res.urls;
    if (urls) {
        urls = JSON.parse(urls);
        if (urls.length) {
            document.getElementById("urls").value = urls.join("\n");
        }
    }
    var exclusions = res.exclusions;
    if (exclusions) {
        exclusions = JSON.parse(exclusions);
        if (exclusions.length) {
            document.getElementById("exclusions").value = exclusions.join("\n");
        }
    }
    if (res.profile) {
        document.getElementById("profile").value = res.profile;
    }
})
