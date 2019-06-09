package manageapp

import (
	"fmt"
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
	// not needed in MacOS as long as it's stored in the appropriate folder
	return nil
}

// Unregister removes the native app registration
func Unregister() {
	usr, err := user.Current()
	if err != nil {
		fmt.Printf("Error: %s", err)
		return
	}

	manifestPath := usr.HomeDir + "/Library/Application Support/Mozilla/NativeMessagingHosts/me.onchro.json"

	err = os.Remove(manifestPath)
	if err != nil {
		fmt.Printf("Error: %s", err)
		return
	} else {
		fmt.Printf("Successfully deleted %s\n", manifestPath)
		fmt.Println("You can now delete this folder")
	}
}
