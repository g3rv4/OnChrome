# OnChrome ![](extension/images/icon48.png)

A Firefox extension so that certain urls are opened using Chrome. I built it because I wanted to switch to Firefox, but certain sites I use all the time require Chrome. I want links and everything to work just as it used to, but if it ends opening one of those sites, it should open them in Chrome.

The extension lets you define [url match patterns](https://developer.mozilla.org/en-US/docs/Mozilla/Add-ons/WebExtensions/Match_patterns) to determine which sites should always be opened with Chrome.

Read [my blog post about it](https://g3rv4.com/2019/06/how-to-migrate-to-firefox) where I explain why I built it.

## How do I install it?

1. Install the extension on [the Firefox store](https://addons.mozilla.org/en-US/firefox/addon/onchrome/)
2. Install the [supporting application](https://github.com/g3rv4/OnChrome/releases)

## Requirements

* Windows x64 or MacOs (other environments can be supported later, ask!)
* Chrome installed

## Why do I need to install software on my machine? I never had to do it for an extension before!

Ugh, yeah... I know... that sucks. The reason is that Firefox can't talk to Chrome directly... you need to use [Native Messaging](https://developer.mozilla.org/en-US/docs/Mozilla/Add-ons/WebExtensions/Native_messaging), and that requires a native app that understands the messages from the extension and in turn opens Chrome.

I chose Go to program this application (with the installer / uninstaller... that take care of all the registrations) because it has 0 dependencies. You just run the executables and that's it. Also, their source code [is available](https://github.com/g3rv4/OnChrome/tree/master/app) for you to build it on your own.
