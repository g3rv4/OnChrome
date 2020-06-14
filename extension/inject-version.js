var actualCode = `
window.OnChromeVersion = '${browser.runtime.getManifest().version}';
`;

var script = document.createElement('script');
script.textContent = actualCode;
(document.head || document.documentElement).appendChild(script);
script.remove();
