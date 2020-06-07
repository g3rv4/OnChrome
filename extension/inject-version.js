var actualCode = `
window.OnChromeVersion = '${browser.runtime.getManifest().version}';
`;

// this is going to be injected on any localhost page... let's at least make sure
// it's on the weird OnChrome port
if (document.location.port === "12346") {
    var script = document.createElement('script');
    script.textContent = actualCode;
    (document.head || document.documentElement).appendChild(script);
    script.remove();
}
