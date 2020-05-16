const save = document.getElementById("save");
save.addEventListener("click", () => {
    let urls = document.getElementById("urls").value;
    let profile = document.getElementById("profile").value;
    let valueToSave = [];

    if (urls) {
        urls = urls.split(/\r?\n/);
        if (urls.length) {
            valueToSave = urls;
        }
    }
    browser.storage.sync.set({
        urls: JSON.stringify(valueToSave),
        profile: profile
    }, () => {
        chrome.runtime.sendMessage({ type: "urlsUpdated" });
        window.close();
    });
})

chrome.storage.sync.get(["urls","profile"], res => {
    var urls = res.urls;
    if (urls) {
        urls = JSON.parse(urls);
        if (urls.length) {
            document.getElementById("urls").value = urls.join("\n");
        }
    }
    if (res.profile) {
        document.getElementById("profile").value = res.profile;
    }
})
