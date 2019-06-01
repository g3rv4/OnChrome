package main

import (
	"golang.org/x/sys/windows/registry"
)

func getExecutablePath(currentPath string) string {
	return currentPath + "\\FirefoxEndpoint.exe"
}

func getManifestPath(currentPath string) (string, error) {
	return currentPath + "\\manifest.json", nil
}

func registerManifest(manifestPath string) error {
	k, _, err := registry.CreateKey(registry.CURRENT_USER, `Software\Mozilla\NativeMessagingHosts\me.onchro`, registry.ALL_ACCESS)

	if err != nil {
		return err
	}
	defer k.Close()

	return k.SetStringValue("", manifestPath)
}
