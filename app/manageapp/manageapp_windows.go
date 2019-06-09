package manageapp

import (
	"fmt"

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

// Unregister removes the native app registration
func Unregister() {
	err := registry.DeleteKey(registry.CURRENT_USER, `Software\Mozilla\NativeMessagingHosts\me.onchro`)

	if err != nil {
		fmt.Printf("Error: %s", err)
	} else {
		fmt.Println("Successfully removed the registry entry")
		fmt.Println("You can now delete this folder")
	}
}
