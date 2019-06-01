package main

import (
	"os"
	"os/user"
)

func getExecutablePath(currentPath string) string {
	return currentPath + "/FirefoxEndpoint"
}

func getManifestPath(currentPath string) (string, error) {
	usr, err := user.Current()
	if err != nil {
		return "", err
	}

	_ = os.Mkdir(usr.HomeDir+"/Library/Application Support/Mozilla/NativeMessagingHosts", os.ModePerm)
	return usr.HomeDir + "/Library/Application Support/Mozilla/NativeMessagingHosts/me.onchro.json", nil
}

func registerManifest(manifestPath string) error {
	return nil
}
